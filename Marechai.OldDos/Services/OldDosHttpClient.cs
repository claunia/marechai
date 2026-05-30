using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Marechai.OldDos.Services;

/// <summary>
///     Rate-limited HTTP client for old-dos.ru. Modeled on MobyGamesHttpClient: gzip/deflate/brotli
///     decompression, cookie container, browser-like User-Agent. old-dos.ru is plain HTML with no
///     JS-driven rendering and no Cloudflare challenge, so a single shared HttpClient is enough.
/// </summary>
public sealed class OldDosHttpClient : IDisposable
{
    static OldDosHttpClient()
    {
        // old-dos.ru serves Content-Type: text/html; charset=windows-1251. .NET Core does not
        // ship that code page by default; without this registration HttpContent.ReadAsStringAsync
        // throws "The character set provided in ContentType is invalid". Idempotent.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    readonly string             _baseUrl;
    readonly int                _delayMs;
    readonly HttpClient         _client;
    readonly HttpClientHandler  _handler;

    public OldDosHttpClient(string baseUrl, int delayMs, string userAgent)
    {
        _baseUrl  = baseUrl.TrimEnd('/');
        _delayMs  = delayMs;

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
        _client.DefaultRequestHeaders.Add("User-Agent",      userAgent);
        _client.DefaultRequestHeaders.Add("Accept",          "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.9,en;q=0.8");
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
    }

    public string BaseUrl => _baseUrl;

    /// <summary>Resolve a possibly-relative URL against the configured base.</summary>
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
        if(_delayMs > 0) await Task.Delay(_delayMs);
        url = Resolve(url);

        // Manual redirect loop with same-host scheme-downgrade tolerance. old-dos.ru's TLS
        // endpoint 301s every request to plain http://; .NET's HttpClient refuses to follow
        // an https→http redirect, so we do it ourselves (only for the same host, capped at 5
        // hops). Anything off-host gets the default safe behavior.
        Uri current = new(url);
        for(int hop = 0; hop < 5; hop++)
        {
            try
            {
                using HttpResponseMessage response = await _client.GetAsync(current,
                                                          HttpCompletionOption.ResponseHeadersRead);
                if(response.StatusCode is HttpStatusCode.MovedPermanently
                                          or HttpStatusCode.Found
                                          or HttpStatusCode.SeeOther
                                          or HttpStatusCode.TemporaryRedirect
                                          or HttpStatusCode.PermanentRedirect)
                {
                    Uri loc = response.Headers.Location;
                    if(loc is null) { Console.WriteLine($"\e[33m  {(int)response.StatusCode} no Location header fetching {current}\e[0m"); return null; }
                    if(!loc.IsAbsoluteUri) loc = new Uri(current, loc);
                    if(!string.Equals(loc.Host, current.Host, StringComparison.OrdinalIgnoreCase))
                    { Console.WriteLine($"\e[33m  Cross-host redirect refused: {current} -> {loc}\e[0m"); return null; }
                    current = loc;
                    continue;
                }
                if(!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"\e[33m  HTTP {(int)response.StatusCode} fetching {current}\e[0m");
                    return null;
                }
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                Encoding enc = ResolveEncoding(response.Content.Headers?.ContentType?.CharSet, bytes);
                return enc.GetString(bytes);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\e[33m  Error fetching {current}: {ex.Message}\e[0m");
                return null;
            }
        }
        Console.WriteLine($"\e[33m  Too many redirects starting at {url}\e[0m");
        return null;
    }

    public void Dispose()
    {
        _client?.Dispose();
        _handler?.Dispose();
    }

    /// <summary>
    ///     Prefer the charset declared on the Content-Type header; fall back to a `&lt;meta charset&gt;`
    ///     sniff in the first 2 KB; finally default to UTF-8. Strips surrounding quotes the server
    ///     sometimes emits. Unknown / unsupported charsets fall through to UTF-8 rather than throw.
    /// </summary>
    static Encoding ResolveEncoding(string headerCharset, byte[] body)
    {
        string cs = headerCharset?.Trim().Trim('"').Trim('\'');
        if(!string.IsNullOrEmpty(cs))
        {
            try { return Encoding.GetEncoding(cs); }
            catch { /* fall through */ }
        }

        // Sniff <meta charset=...> or <meta http-equiv="Content-Type" content="...; charset=...">
        // from the first 2 KB interpreted as ASCII (all meta-tag bytes are ASCII-safe).
        int sniffLen = Math.Min(body.Length, 2048);
        string head = Encoding.ASCII.GetString(body, 0, sniffLen);
        System.Text.RegularExpressions.Match m =
            System.Text.RegularExpressions.Regex.Match(head, @"charset\s*=\s*[""']?([\w\-]+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if(m.Success)
        {
            try { return Encoding.GetEncoding(m.Groups[1].Value); }
            catch { /* fall through */ }
        }
        return Encoding.UTF8;
    }
}
