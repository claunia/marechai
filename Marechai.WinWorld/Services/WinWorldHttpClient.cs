using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Marechai.WinWorld.Services;

/// <summary>
///     Rate-limited HTTP client for winworldpc.com. Modeled on <c>OldDosHttpClient</c>: gzip/deflate/brotli
///     decompression, cookie container, browser-like User-Agent, and a polite per-request delay.
///     WinWorldPC serves UTF-8 HTML with no JS-driven rendering and no challenge wall, so a single
///     shared <see cref="HttpClient" /> is enough.
/// </summary>
public sealed class WinWorldHttpClient : IDisposable
{
    static WinWorldHttpClient() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    readonly string             _baseUrl;
    readonly int                _delayMs;
    readonly HttpClient         _client;
    readonly HttpClientHandler  _handler;

    public WinWorldHttpClient(string baseUrl, int delayMs, string userAgent)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _delayMs = delayMs;

        _handler = new HttpClientHandler
        {
            AllowAutoRedirect        = true,
            MaxAutomaticRedirections = 5,
            AutomaticDecompression   = DecompressionMethods.GZip
                                     | DecompressionMethods.Deflate
                                     | DecompressionMethods.Brotli,
            UseCookies      = true,
            CookieContainer = new CookieContainer()
        };

        _client = new HttpClient(_handler) { Timeout = TimeSpan.FromSeconds(60) };
        _client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        _client.DefaultRequestHeaders.Add("Accept",
                                          "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
    }

    public string BaseUrl => _baseUrl;

    public string Resolve(string url)
    {
        if(string.IsNullOrEmpty(url)) return url;
        if(url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return url;
        if(url.StartsWith("//")) return "https:" + url;
        if(url.StartsWith("/")) return _baseUrl + url;
        return _baseUrl + "/" + url;
    }

    public async Task<string> FetchPageAsync(string url)
    {
        (string _, string body) = await FetchPageWithFinalUrlAsync(url);
        return body;
    }

    /// <summary>Get the final URL after redirects (used to resolve <c>/product/{slug}</c> → <c>/product/{slug}/{firstReleaseSlug}</c>).</summary>
    public async Task<(string finalUrl, string body)> FetchPageWithFinalUrlAsync(string url)
    {
        url = Resolve(url);

        // Retry on HTTP 429 with exponential backoff, honouring Retry-After when present.
        // WinWorldPC enforces a soft per-IP rate limit that occasionally trips during long
        // section walks; backing off and retrying keeps the crawl going without losing the row.
        const int maxAttempts = 5;
        for(int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if(_delayMs > 0) await Task.Delay(_delayMs);
            try
            {
                using HttpResponseMessage response = await _client.GetAsync(url);
                if((int)response.StatusCode == 429 && attempt < maxAttempts)
                {
                    int waitMs = ResolveRetryAfterMs(response) ?? Math.Min(60_000, 5_000 * (1 << (attempt - 1)));
                    Console.WriteLine($"\e[33m  HTTP 429 fetching {url}; backing off {waitMs}ms (attempt {attempt}/{maxAttempts - 1})\e[0m");
                    await Task.Delay(waitMs);
                    continue;
                }
                if(!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"\e[33m  HTTP {(int)response.StatusCode} fetching {url}\e[0m");
                    return (null, null);
                }
                byte[]   bytes = await response.Content.ReadAsByteArrayAsync();
                Encoding enc   = ResolveEncoding(response.Content.Headers?.ContentType?.CharSet, bytes);
                string   final = response.RequestMessage?.RequestUri?.ToString() ?? url;
                return (final, enc.GetString(bytes));
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\e[33m  Error fetching {url}: {ex.Message}\e[0m");
                return (null, null);
            }
        }
        return (null, null);
    }

    /// <summary>Translate a <c>Retry-After</c> header (delta-seconds or HTTP-date) into a wait in ms.</summary>
    static int? ResolveRetryAfterMs(HttpResponseMessage response)
    {
        System.Net.Http.Headers.RetryConditionHeaderValue ra = response.Headers?.RetryAfter;
        if(ra is null) return null;
        if(ra.Delta is { } delta) return (int)Math.Min(int.MaxValue, delta.TotalMilliseconds);
        if(ra.Date is { } date)
        {
            double ms = (date - DateTimeOffset.UtcNow).TotalMilliseconds;
            if(ms > 0) return (int)Math.Min(int.MaxValue, ms);
        }
        return null;
    }

    public void Dispose()
    {
        _client?.Dispose();
        _handler?.Dispose();
    }

    static Encoding ResolveEncoding(string headerCharset, byte[] body)
    {
        string cs = headerCharset?.Trim().Trim('"').Trim('\'');
        if(!string.IsNullOrEmpty(cs))
        {
            try { return Encoding.GetEncoding(cs); }
            catch { /* fall through */ }
        }

        int    sniffLen = Math.Min(body.Length, 2048);
        string head     = Encoding.ASCII.GetString(body, 0, sniffLen);
        Match m = Regex.Match(head, @"charset\s*=\s*[""']?([\w\-]+)", RegexOptions.IgnoreCase);
        if(m.Success)
        {
            try { return Encoding.GetEncoding(m.Groups[1].Value); }
            catch { /* fall through */ }
        }
        return Encoding.UTF8;
    }
}
