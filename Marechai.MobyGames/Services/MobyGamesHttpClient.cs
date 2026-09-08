using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Marechai.MobyGames.Services;

/// <summary>
///     HTTP client for downloading cover images and fetching pages from MobyGames.
///     No login required — old /images/covers/l/ URLs redirect to CDN which is publicly accessible.
///     <para>
///         Discovery of new games uses the sitemap XMLs hosted on DigitalOcean Spaces
///         (<c>sfo3.digitaloceanspaces.com/moby-images/...</c>), which bypass the Cloudflare edge
///         entirely and have no pagination cap. See <c>FetchBytesAsync</c>.
///     </para>
/// </summary>
public sealed partial class MobyGamesHttpClient : IDisposable
{
    const    string          BaseUrl   = "https://www.mobygames.com";
    // Must match the headless browser's UA: Cloudflare ties cf_clearance to it (see
    // MobyGamesBrowser.DefaultUserAgent). A different UA here yields 403 on every page.
    const    string          UserAgent = MobyGamesBrowser.DefaultUserAgent;
    readonly HttpClient       _client;
    readonly SocketsHttpHandler _handler;
    string                      _lastRemote;
    readonly int              _delayMs;

    /// <param name="proxy">
    ///     Optional proxy URL (<c>socks5://host:port</c> or <c>http://host:port</c>) so this client
    ///     exits from the same IP as the browser that minted <c>cf_clearance</c>. Used by
    ///     <c>cf-login</c> to verify the freshly minted cookies through the server's IP.
    /// </param>
    public MobyGamesHttpClient(int delayMs = 2000, string proxy = null, string proxyUser = null,
                               string proxyPassword = null)
    {
        _delayMs = delayMs;

        _handler = new SocketsHttpHandler
        {
            AllowAutoRedirect        = true,
            MaxAutomaticRedirections = 5,
            // Enable transparent gzip / deflate / brotli decoding so the server serves us the same
            // compressed payload a real Firefox would get. Without this MobyGames sometimes returns
            // a different (shorter) page than the one a browser sees.
            AutomaticDecompression = DecompressionMethods.GZip
                                   | DecompressionMethods.Deflate
                                   | DecompressionMethods.Brotli,
            // Cookies are now ENABLED so authenticated MobyPlus sessions cascade from the
            // PuppeteerSharp-driven login (see MobyGamesBrowser) into this fast HTTP path via
            // ImportCookies(). When no session is loaded, the container is simply empty and the
            // client behaves anonymously exactly as before.
            UseCookies     = true,
            CookieContainer = new CookieContainer()
        };

        if(!string.IsNullOrWhiteSpace(proxy))
        {
            // SocketsHttpHandler accepts http://, https:// and socks4/4a/5:// proxy URLs on .NET 6+.
            var webProxy = new WebProxy(proxy.Trim());

            if(!string.IsNullOrEmpty(proxyUser))
                webProxy.Credentials = new NetworkCredential(proxyUser, proxyPassword ?? "");

            _handler.Proxy    = webProxy;
            _handler.UseProxy = true;
        }
        else if(Environment.GetEnvironmentVariable("MOBYGAMES_ALLOW_IPV6") is not "1")
        {
            // Cloudflare binds cf_clearance to the client IP. Every clearance we mint reaches
            // MobyGames over IPv4 (the headless browser's SOCKS exit, or a desktop with v4), but a
            // dual-stack server lets .NET prefer the AAAA record of www.mobygames.com and arrive
            // from its IPv6 address instead — a different client to Cloudflare, hence HTTP 403 on
            // every page while the very same cookie works elsewhere. Pin direct connections to
            // IPv4 (set MOBYGAMES_ALLOW_IPV6=1 to opt out) and remember the endpoint for diagnostics.
            _handler.ConnectCallback = async (ctx, ct) =>
            {
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(ctx.DnsEndPoint.Host,
                                                                        System.Net.Sockets.AddressFamily.InterNetwork,
                                                                        ct);

                if(addresses.Length == 0)
                    throw new System.Net.Sockets.SocketException((int)System.Net.Sockets.SocketError.HostNotFound);

                var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,
                                                           System.Net.Sockets.SocketType.Stream,
                                                           System.Net.Sockets.ProtocolType.Tcp) { NoDelay = true };

                try
                {
                    await socket.ConnectAsync(addresses, ctx.DnsEndPoint.Port, ct);
                    _lastRemote = socket.RemoteEndPoint?.ToString();

                    return new System.Net.Sockets.NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();

                    throw;
                }
            };
        }

        _client = new HttpClient(_handler)
        {
            Timeout = TimeSpan.FromSeconds(120)
        };

        // Always opt in to mature/adult content so cover detail pages for adult games
        // render the actual image instead of a gate page with <img src="none">.
        _handler.CookieContainer.Add(new Uri("https://mobygames.com/"),
                                     new Cookie("adult_quicksearch", "true", "/", "mobygames.com")
                                     {
                                         Secure = false, HttpOnly = false,
                                         Expires = DateTime.UtcNow.AddYears(1)
                                     });

        _handler.CookieContainer.Add(new Uri("https://www.mobygames.com/"),
                                     new Cookie("adult_quicksearch", "true", "/", "www.mobygames.com")
                                     {
                                         Secure = false, HttpOnly = false,
                                         Expires = DateTime.UtcNow.AddYears(1)
                                     });

        // Browser-like headers so per-game pages don't trip Cloudflare's bot heuristics.
        _client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        _client.DefaultRequestHeaders.Add(
            "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
        _client.DefaultRequestHeaders.Add("Accept-Language",           "en-US,en;q=0.5");
        _client.DefaultRequestHeaders.Add("Accept-Encoding",           "gzip, deflate, br");
        _client.DefaultRequestHeaders.Add("DNT",                       "1");
        _client.DefaultRequestHeaders.Add("Sec-Fetch-Dest",            "document");
        _client.DefaultRequestHeaders.Add("Sec-Fetch-Mode",            "navigate");
        _client.DefaultRequestHeaders.Add("Sec-Fetch-Site",            "none");
        _client.DefaultRequestHeaders.Add("Sec-Fetch-User",            "?1");
        _client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
    }

    /// <summary>
    ///     Constructs a <see cref="MobyGamesHttpClient"/> and immediately attaches the
    ///     MobyGames browser session cookies via <see cref="MobyGamesBrowser.TryAttachCookiesAsync"/>.
    ///     This is the preferred entry point for every command in <c>Program.cs</c> — using it
    ///     guarantees that every HTTP request carries the Cloudflare clearance and (when
    ///     credentials are configured) the MobyPlus auth cookies, so callers can't accidentally
    ///     spawn an uncookied client that hits the "Just a moment..." challenge page.
    /// </summary>
    public static async Task<MobyGamesHttpClient> CreateAsync(IConfiguration    cfg,
                                                              int               delayMs = 2000,
                                                              CancellationToken ct      = default)
    {
        var client = new MobyGamesHttpClient(delayMs);
        await MobyGamesBrowser.TryAttachCookiesAsync(cfg, client, delayMs, ct);

        return client;
    }

    /// <summary>
    ///     Fetch an HTML page from MobyGames with rate limiting.
    ///     Returns the HTML string or null on failure.
    /// </summary>
    public async Task<string> FetchPageAsync(string url)
    {
        if(_delayMs > 0)
            await Task.Delay(_delayMs);

        if(!url.StartsWith("http"))
            url = BaseUrl + url;

        try
        {
            using var response = await _client.GetAsync(url);

            if(!response.IsSuccessStatusCode)
            {
                // Cloudflare marks its challenge/block responses; surface that plus the remote
                // endpoint so a clearance bound to another IP is recognisable at a glance.
                string mitigated = response.Headers.TryGetValues("cf-mitigated", out var mv) ? string.Join(",", mv) : null;

                Console.WriteLine($"\e[33m  Warning: HTTP {(int)response.StatusCode} fetching {url}" +
                                  (mitigated is not null ? $" (cf-mitigated: {mitigated})" : "") +
                                  (_lastRemote is not null ? $" [remote {_lastRemote}]" : "") + "\e[0m");

                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[33m  Warning: Error fetching {url}: {ex.Message}\e[0m");

            return null;
        }
    }

    /// <summary>
    ///     Fetch a URL and return the raw response bytes. Skips the configured per-mobygames.com
    ///     rate-limit when the URL is on a different host (e.g. the DigitalOcean Spaces CDN that
    ///     serves the sitemaps), since those endpoints have no Cloudflare layer and no rate limit.
    ///     Returns null on failure.
    /// </summary>
    public async Task<byte[]> FetchBytesAsync(string url, int? overrideDelayMs = null)
    {
        if(!url.StartsWith("http"))
            url = BaseUrl + url;

        int delay = overrideDelayMs ??
                    (new Uri(url).Host.EndsWith("mobygames.com", StringComparison.OrdinalIgnoreCase)
                         ? _delayMs
                         : 0);

        if(delay > 0)
            await Task.Delay(delay);

        try
        {
            using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if(!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"\e[33m  Warning: HTTP {(int)response.StatusCode} fetching {url}\e[0m");

                return null;
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[33m  Warning: Error fetching {url}: {ex.Message}\e[0m");

            return null;
        }
    }

    /// <summary>
    ///     Convert a thumbnail URL (/images/covers/s/...) to the large version (/images/covers/l/...)
    ///     and construct the full download URL. MobyGames redirects these to the CDN with the original image.
    /// </summary>
    public static string GetLargeImageUrl(string thumbnailUrl)
    {
        if(string.IsNullOrWhiteSpace(thumbnailUrl)) return null;

        // Reject placeholder values like "none" that aren't real URLs or paths
        if(!thumbnailUrl.StartsWith("http") && !thumbnailUrl.StartsWith("/"))
            return null;

        // Replace /s/ with /l/ in the path to get the large/original version
        string largeUrl = thumbnailUrl.Replace("/covers/s/", "/covers/l/");

        // Ensure it's a full URL
        if(!largeUrl.StartsWith("http"))
            largeUrl = BaseUrl + largeUrl;

        return largeUrl;
    }

    public async Task<string> DownloadImageAsync(string url, string destPath)
    {
        // Apply rate limiting
        if(_delayMs > 0)
            await Task.Delay(_delayMs);

        if(!url.StartsWith("http") && !url.StartsWith("/"))
        {
            Console.WriteLine($"\e[33m  Warning: Invalid image URL '{url}' — skipping download\e[0m");

            return null;
        }

        if(!url.StartsWith("http"))
            url = BaseUrl + url;

        // Final safety check: reject URLs where BaseUrl concatenation produced a broken hostname
        if(!Uri.TryCreate(url, UriKind.Absolute, out var parsedUri) ||
           (parsedUri.Scheme != "http" && parsedUri.Scheme != "https"))
        {
            Console.WriteLine($"\e[33m  Warning: Malformed URL '{url}' — skipping download\e[0m");

            return null;
        }

        try
        {
            using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if(!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"\e[33m  Warning: HTTP {(int)response.StatusCode} downloading {url}\e[0m");

                return null;
            }

            // Determine file extension from content type or URL
            string extension = GetExtensionFromResponse(response, url);
            string fullPath  = $"{destPath}.{extension}";

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var file   = File.Create(fullPath);
            await stream.CopyToAsync(file);

            return extension;
        }
        catch(HttpRequestException ex)
        {
            Console.WriteLine($"\e[33m  Warning: Download failed for {url}: {ex.Message}\e[0m");

            return null;
        }
    }

    static string GetExtensionFromResponse(HttpResponseMessage response, string url)
    {
        // Try content type first
        string contentType = response.Content.Headers.ContentType?.MediaType;

        switch(contentType)
        {
            case "image/jpeg":
                return "jpg";
            case "image/png":
                return "png";
            case "image/webp":
                return "webp";
            case "image/gif":
                return "gif";
            case "image/bmp":
                return "bmp";
            case "image/tiff":
                return "tiff";
        }

        // Fall back to URL extension
        string urlPath = new Uri(url).AbsolutePath;
        string ext     = Path.GetExtension(urlPath).TrimStart('.');

        return string.IsNullOrWhiteSpace(ext) ? "jpg" : ext;
    }

    /// <summary>
    ///     Convert a promo art thumbnail URL (/images/promo/s/...) to the large version (/images/promo/l/...).
    /// </summary>
    public static string GetLargePromoImageUrl(string thumbnailUrl)
    {
        if(string.IsNullOrWhiteSpace(thumbnailUrl)) return null;

        // Reject placeholder values like "none" that aren't real URLs or paths
        if(!thumbnailUrl.StartsWith("http") && !thumbnailUrl.StartsWith("/"))
            return null;

        string largeUrl = thumbnailUrl.Replace("/promo/s/", "/promo/l/");

        if(!largeUrl.StartsWith("http"))
            largeUrl = BaseUrl + largeUrl;

        return largeUrl;
    }

    /// <summary>
    ///     Extract the full-size image URL from a MobyGames image detail page HTML (screenshots,
    ///     promo art, or covers). MobyPlus accounts get a hidden
    ///     <c>&lt;a download href="https://cdn.mobygames.com/..."&gt;</c> block pointing to the
    ///     ORIGINAL-resolution image; we prefer that when present. For anonymous sessions, we fall
    ///     back to the gallery <img> tag and finally to og:image meta.
    ///     <para>
    ///         <c>IsMobyPlusOriginal</c> is <c>true</c> when the URL was sourced from the
    ///         <c>&lt;a download&gt;</c> link (MobyPlus original-resolution path), and <c>false</c>
    ///         for any of the lower-resolution fallbacks. Callers use this to tag download log
    ///         lines so we can tell which path actually served a given image.
    ///     </para>
    /// </summary>
    public static (string Url, bool IsMobyPlusOriginal) ExtractFullSizeImageUrl(string detailPageHtml)
    {
        if(string.IsNullOrWhiteSpace(detailPageHtml)) return (null, false);

        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(detailPageHtml);

        // PREFERRED: MobyPlus original-resolution download link. Numeric IDs in this URL DIFFER
        // from the gallery preview URL, so a simple URL-swap won't work — we MUST parse this from
        // the detail page when MobyPlus is active.
        var downloadLink = doc.DocumentNode.SelectSingleNode(
            "//a[@download and contains(@href, 'cdn.mobygames.com')]");

        if(downloadLink is not null)
        {
            string href = downloadLink.GetAttributeValue("href", null);

            if(!string.IsNullOrWhiteSpace(href))
                return (href, true);
        }

        // Fallback 1: the visible full-size <img> inside #gallery-image (anonymous-session size).
        var galleryImg = doc.DocumentNode.SelectSingleNode(
            "//div[@id='gallery-image']//img[contains(@class, 'img-fluid')]");

        if(galleryImg is not null)
        {
            string src = galleryImg.GetAttributeValue("src", null);

            if(!string.IsNullOrWhiteSpace(src) && (src.StartsWith("http") || src.StartsWith("/")))
                return (src, false);
        }

        // Fallback 2: any <img> inside #gallery-image figure on the CDN.
        galleryImg =
            doc.DocumentNode.SelectSingleNode("//div[@id='gallery-image']//figure//img[@src]");

        if(galleryImg is not null)
        {
            string src = galleryImg.GetAttributeValue("src", null);

            if(!string.IsNullOrWhiteSpace(src) && (src.StartsWith("http") || src.StartsWith("/")) &&
               src.Contains("cdn.mobygames.com"))
                return (src, false);
        }

        // Fallback 3: og:image meta tag (always present in <head>, unaffected by mature-content gates).
        var ogImage = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");

        if(ogImage is not null)
        {
            string content = ogImage.GetAttributeValue("content", null);

            if(!string.IsNullOrWhiteSpace(content) && content.Contains("cdn.mobygames.com"))
                return (content, false);
        }

        return (null, false);
    }

    /// <summary>
    ///     Legacy name kept temporarily so callers that still reference the old method name compile.
    ///     Prefer <see cref="ExtractFullSizeImageUrl"/> in new code.
    /// </summary>
    public static (string Url, bool IsMobyPlusOriginal) ExtractFullSizeScreenshotUrl(string detailPageHtml) =>
        ExtractFullSizeImageUrl(detailPageHtml);

    /// <summary>
    ///     Extract the MobyGames numeric game ID from raw HTML already stored in the source database.
    ///     Tries multiple extraction points in order of reliability, supporting both
    ///     the old (pre-2024) and new MobyGames site HTML formats.
    /// </summary>
    public static int? ExtractNumericGameIdFromHtml(string html)
    {
        if(string.IsNullOrWhiteSpace(html)) return null;

        // 1. New site: gtag content_id: "1068"
        var contentIdMatch = ContentIdRegex().Match(html);

        if(contentIdMatch.Success && int.TryParse(contentIdMatch.Groups[1].Value, out int id1))
            return id1;

        // 2. New site: :game-id="1068" (Vue component attribute)
        var gameIdAttrMatch = GameIdAttrRegex().Match(html);

        if(gameIdAttrMatch.Success && int.TryParse(gameIdAttrMatch.Groups[1].Value, out int id2))
            return id2;

        // 3. Both sites: /contribute/game/1068/ or /contribute/game-shots/1068/
        var contributeMatch = ContributeGameIdRegex().Match(html);

        if(contributeMatch.Success && int.TryParse(contributeMatch.Groups[1].Value, out int id3))
            return id3;

        // 4. Old site: /game/user-rating/post/1068/ or similar action URLs with numeric IDs
        var ratingActionMatch = RatingActionRegex().Match(html);

        if(ratingActionMatch.Success && int.TryParse(ratingActionMatch.Groups[1].Value, out int id4))
            return id4;

        // 5. Both sites: "Moby ID: 1068" or "Moby ID 1068" (with or without colon)
        var mobyIdMatch = MobyIdTextRegex().Match(html);

        if(mobyIdMatch.Success && int.TryParse(mobyIdMatch.Groups[1].Value, out int id5))
            return id5;

        // 6. Old site: game_id or gameId in embedded JavaScript — e.g. game_id: 1068, "game_id": "1068"
        var jsGameIdMatch = JsGameIdRegex().Match(html);

        if(jsGameIdMatch.Success && int.TryParse(jsGameIdMatch.Groups[1].Value, out int id6))
            return id6;

        return null;
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"content_id:\s*""(\d+)""", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex ContentIdRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@":game-id=""(\d+)""", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex GameIdAttrRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"/contribute/game[^""']*/(\d+)/", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex ContributeGameIdRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"/game/(?:user-rating/post|rate)/(\d+)", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex RatingActionRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"Moby\s+ID[:\s]+(\d+)", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex MobyIdTextRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"[""']?game_?[Ii]d[""']?\s*[:=]\s*[""']?(\d+)", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex JsGameIdRegex();

    /// <summary>
    ///     Resolve a MobyGames game slug to numeric ID by following the redirect.
    ///     Returns null if the slug cannot be resolved.
    /// </summary>
    public async Task<int?> ResolveNumericGameIdAsync(string slug)
    {
        // Try original slug first
        int? result = await TryResolveSlugAsync(slug);

        // If it failed and slug has a leading '-', retry without it
        if(result is null && slug.StartsWith('-'))
        {
            string trimmed = slug.TrimStart('-');

            if(!string.IsNullOrEmpty(trimmed))
                result = await TryResolveSlugAsync(trimmed);
        }

        return result;
    }

    /// <summary>
    ///     Fallback resolver for legacy rows where the slug-based redirect fails (typically
    ///     because the slug is truncated to 64 chars by the old <c>mobygames_raw</c> schema,
    ///     or because MobyGames editors have since renamed the title and the old slug now
    ///     404s). Searches the public <c>/search/?q=…&amp;type=game</c> page and returns the
    ///     numeric ID of the first result whose normalised title contains, equals, or is
    ///     contained by the normalised <paramref name="name"/>. Conservative on purpose —
    ///     prefer null over a wrong link in unattended runs.
    /// </summary>
    public async Task<int?> ResolveNumericGameIdByNameAsync(string name)
    {
        if(string.IsNullOrWhiteSpace(name)) return null;

        if(_delayMs > 0) await Task.Delay(_delayMs);

        string url = $"{BaseUrl}/search/?q={Uri.EscapeDataString(name)}&type=game";

        string html;

        try
        {
            using var response = await _client.GetAsync(url);

            if(!response.IsSuccessStatusCode) return null;

            html = await response.Content.ReadAsStringAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[33m  Warning: search error for '{name}': {ex.Message}\e[0m");

            return null;
        }

        string needle = NormalizeTitleForCompare(name);

        if(needle.Length == 0) return null;

        var needleTokens = TokeniseAndFilterStopWords(needle);

        if(needleTokens.Count == 0) return null;

        // MobyGames search results live in <b><a href="/game/N/slug/">Title</a></b>. Parse them
        // in order; the first hit whose token set is a superset of the needle's wins. Token-set
        // comparison (vs substring) is required because MobyGames editors often reorder a
        // title's words (e.g. our DB has "Rock Band: 'Godzilla' - Blue Öyster Cult" while the
        // live title is "Rock Band: Blue Öyster Cult - 'Godzilla'" — same words, different
        // order, never substrings of each other). Connector words ("by", "ft", "feat", "the",
        // "a", "an", "of", "vs", "and") are stripped on both sides because MobyGames editors
        // often replace them with dashes / drop them (e.g. our DB has "'American Girl' by
        // Bonnie McKee" while the live title is "Bonnie McKee - American Girl").
        var matches = System.Text.RegularExpressions.Regex.Matches(
            html,
            @"<b>\s*<a[^>]+href=""(?:https?://www\.mobygames\.com)?/game/(\d+)/[^""]*""[^>]*>([^<]+)</a>\s*</b>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach(System.Text.RegularExpressions.Match m in matches)
        {
            if(!int.TryParse(m.Groups[1].Value, out int id)) continue;

            string titleText = System.Net.WebUtility.HtmlDecode(m.Groups[2].Value);
            string hay       = NormalizeTitleForCompare(titleText);

            if(hay.Length == 0) continue;

            var hayTokens = TokeniseAndFilterStopWords(hay);

            if(hayTokens.IsSupersetOf(needleTokens)) return id;
        }

        return null;
    }

    static readonly HashSet<string> TitleStopWords = new(StringComparer.Ordinal)
    {
        "a", "an", "the", "of", "by", "and", "vs", "ft", "feat", "featuring", "with", "from"
    };

    static HashSet<string> TokeniseAndFilterStopWords(string normalised)
    {
        var tokens = new HashSet<string>(StringComparer.Ordinal);

        foreach(string t in normalised.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            if(!TitleStopWords.Contains(t))
                tokens.Add(t);

        return tokens;
    }


    /// <summary>
    ///     Lowercase, drop non-alphanumeric ASCII, collapse runs of whitespace. Used by the
    ///     name-based fallback resolver so titles like
    ///     <c>"Just Dance 2014: 'One Way Or Another (Teenage Kicks)' by One Direction"</c> and
    ///     <c>"Just Dance 2014: One Direction - One Way or Another"</c> can be compared.
    /// </summary>
    static string NormalizeTitleForCompare(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        bool lastSpace = false;

        foreach(char c in s.ToLowerInvariant())
        {
            if(char.IsLetterOrDigit(c))
            {
                sb.Append(c);
                lastSpace = false;
            }
            else if(!lastSpace && sb.Length > 0)
            {
                sb.Append(' ');
                lastSpace = true;
            }
        }

        return sb.ToString().Trim();
    }

    async Task<int?> TryResolveSlugAsync(string slug)
    {
        if(_delayMs > 0)
            await Task.Delay(_delayMs);

        string url = $"{BaseUrl}/game/{slug}";

        try
        {
            using var response = await _client.GetAsync(url);
            string    finalUrl = response.RequestMessage?.RequestUri?.ToString();

            if(string.IsNullOrWhiteSpace(finalUrl)) return null;

            var match = System.Text.RegularExpressions.Regex.Match(finalUrl, @"/game/(\d+)/");

            return match.Success ? int.Parse(match.Groups[1].Value) : null;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[33m  Warning: Error resolving slug '{slug}': {ex.Message}\e[0m");

            return null;
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
        _handler?.Dispose();
    }

    /// <summary>
    ///     Copy a set of PuppeteerSharp cookies into this client's <see cref="CookieContainer"/>.
    ///     Call this after the embedded browser (see <see cref="MobyGamesBrowser"/>) has logged in.
    ///     Existing cookies for the same name/domain/path are overwritten.
    /// </summary>
    /// <param name="cookies">
    ///     The cookies exported via <c>MobyGamesBrowser.ExportCookiesAsync()</c>.
    /// </param>
    public void ImportCookies(IEnumerable<PuppeteerSharp.CookieParam> cookies)
    {
        if(cookies is null) return;

        foreach(PuppeteerSharp.CookieParam c in cookies)
        {
            if(string.IsNullOrWhiteSpace(c.Name) || string.IsNullOrWhiteSpace(c.Domain)) continue;

            string domain = c.Domain.StartsWith('.') ? c.Domain : c.Domain;
            string path   = string.IsNullOrEmpty(c.Path) ? "/" : c.Path;
            bool   secure = c.Secure ?? false;

            var cookie = new Cookie(c.Name, c.Value ?? string.Empty, path, domain.TrimStart('.'))
            {
                Secure   = secure,
                HttpOnly = c.HttpOnly ?? false
            };

            if(c.Expires is > 0)
            {
                try
                {
                    cookie.Expires = DateTimeOffset.FromUnixTimeSeconds((long)c.Expires.Value).UtcDateTime;
                }
                catch
                {
                    /* leave session-scoped */
                }
            }

            // Add against both apex and www so SameSite=lax cookies bound to apex still match
            // www.mobygames.com requests issued by HttpClient.
            try { _handler.CookieContainer.Add(new Uri($"https://{cookie.Domain}/"),     cookie); } catch { }
            try { _handler.CookieContainer.Add(new Uri($"https://www.{cookie.Domain}/"), cookie); } catch { }
        }
    }

    /// <summary>
    ///     Set a MobyGames user-preference cookie on the underlying <see cref="CookieContainer"/>.
    ///     Used to bump the search-results page size: the SPA's "Results Per Page" dropdown
    ///     writes <c>perPage=N</c> on the apex domain, and the server honors it on subsequent
    ///     requests. The server caps the effective value at 100 — anything larger is clamped.
    /// </summary>
    public void SetPreferenceCookie(string name, string value)
    {
        if(string.IsNullOrWhiteSpace(name)) return;

        var cookie = new Cookie(name, value ?? string.Empty, "/", "mobygames.com")
        {
            Secure   = false,
            HttpOnly = false,
            Expires  = DateTime.UtcNow.AddYears(1)
        };

        try { _handler.CookieContainer.Add(new Uri("https://mobygames.com/"),     cookie); } catch { }
        try { _handler.CookieContainer.Add(new Uri("https://www.mobygames.com/"), cookie); } catch { }
    }

    /// <summary>
    ///     Fetch one page of year-filtered game search results as the SPA's JSON envelope.
    ///     <para>
    ///         The endpoint is the path-segment-filtered <c>/game/.../page:{N}/?format=json</c>
    ///         URL the new MobyGames Vue SPA uses internally. Response shape is
    ///         <c>{ apiVersion, data: { games[], page, perPage, total, maxPages, ... } }</c>.
    ///         Set the <c>perPage</c> preference cookie via <see cref="SetPreferenceCookie"/>
    ///         before calling to lift the default 18-row page cap up to the server-enforced
    ///         maximum of 100.
    ///     </para>
    ///     <para>
    ///         Returns the raw JSON body, or <c>null</c> on HTTP failure. Caller hands it to
    ///         <see cref="Parsers.SearchResultsPageParser.ParseEnvelope"/>.
    ///     </para>
    /// </summary>
    public async Task<string> FetchSearchPageJsonAsync(int year, int page)
    {
        string url = $"{BaseUrl}/game/from:{year}/include_dlc:true/include_nsfw:true/" +
                     $"release_status:all/sort:title/until:{year}/page:{page}/?format=json";

        if(_delayMs > 0)
            await Task.Delay(_delayMs);

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.TryAddWithoutValidation("Accept", "application/json,*/*;q=0.1");

            using HttpResponseMessage response = await _client.SendAsync(req);

            if(!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"\e[33m  Warning: HTTP {(int)response.StatusCode} fetching {url}\e[0m");

                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"\e[33m  Warning: Error fetching {url}: {ex.Message}\e[0m");

            return null;
        }
    }
}
