using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Marechai.MobyGames.Services;

/// <summary>
///     HTTP client for downloading cover images and fetching pages from MobyGames.
///     No login required — old /images/covers/l/ URLs redirect to CDN which is publicly accessible.
/// </summary>
public sealed class MobyGamesHttpClient : IDisposable
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
            MaxAutomaticRedirections = 5
        };

        _client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(120)
        };

        _client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        _client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
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

    public void Dispose() => _client?.Dispose();
}
