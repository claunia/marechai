using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
    const    string     BaseUrl   = "https://www.mobygames.com";
    const    string     UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:138.0) Gecko/20100101 Firefox/138.0";
    readonly HttpClient _client;
    readonly int        _delayMs;

    public MobyGamesHttpClient(int delayMs = 2000)
    {
        _delayMs = delayMs;

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect        = true,
            MaxAutomaticRedirections = 5,
            // Enable transparent gzip / deflate / brotli decoding so the server serves us the same
            // compressed payload a real Firefox would get. Without this MobyGames sometimes returns
            // a different (shorter) page than the one a browser sees.
            AutomaticDecompression = DecompressionMethods.GZip
                                   | DecompressionMethods.Deflate
                                   | DecompressionMethods.Brotli,
            UseCookies = false
        };

        _client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(120)
        };

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

        if(!url.StartsWith("http"))
            url = BaseUrl + url;

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

        string largeUrl = thumbnailUrl.Replace("/promo/s/", "/promo/l/");

        if(!largeUrl.StartsWith("http"))
            largeUrl = BaseUrl + largeUrl;

        return largeUrl;
    }

    /// <summary>
    ///     Extract the full-size screenshot URL from a MobyGames screenshot detail page HTML.
    ///     The new MobyGames site uses CDN hash URLs that differ between thumbnail and full-size,
    ///     so we must parse the detail page to find the actual image URL.
    /// </summary>
    public static string ExtractFullSizeScreenshotUrl(string detailPageHtml)
    {
        if(string.IsNullOrWhiteSpace(detailPageHtml)) return null;

        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(detailPageHtml);

        // Primary: look for the full-size <img> inside #gallery-image
        var galleryImg = doc.DocumentNode.SelectSingleNode("//div[@id='gallery-image']//img[contains(@class, 'img-fluid')]");

        if(galleryImg is not null)
        {
            string src = galleryImg.GetAttributeValue("src", null);

            if(!string.IsNullOrWhiteSpace(src))
                return src;
        }

        // Fallback 1: any <img> inside #gallery-image figure
        galleryImg = doc.DocumentNode.SelectSingleNode("//div[@id='gallery-image']//figure//img[@src]");

        if(galleryImg is not null)
        {
            string src = galleryImg.GetAttributeValue("src", null);

            if(!string.IsNullOrWhiteSpace(src) && src.Contains("cdn.mobygames.com"))
                return src;
        }

        // Fallback 2: og:image meta tag (always present in <head>, unaffected by mature content gates)
        var ogImage = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");

        if(ogImage is not null)
        {
            string content = ogImage.GetAttributeValue("content", null);

            if(!string.IsNullOrWhiteSpace(content) && content.Contains("cdn.mobygames.com"))
                return content;
        }

        // Fallback 3: MobyPlus original download link
        var downloadLink = doc.DocumentNode.SelectSingleNode("//a[@download and contains(@href, 'cdn.mobygames.com')]");

        if(downloadLink is not null)
        {
            string href = downloadLink.GetAttributeValue("href", null);

            if(!string.IsNullOrWhiteSpace(href))
                return href;
        }

        return null;
    }

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

    public void Dispose() => _client?.Dispose();
}
