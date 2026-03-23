using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Extensions.Configuration;

namespace Marechai.App.Services.Caching;

public sealed class SoftwareScreenshotCache
{
    readonly IConfiguration _configuration;
    StorageFolder           _screenshotsFolder;
    StorageFolder           _thumbnailsFolder;

    public SoftwareScreenshotCache(IConfiguration configuration)
    {
        _configuration = configuration;
        _              = EnsureFolderExistAsync();
    }

    async Task EnsureFolderExistAsync()
    {
        StorageFolder localFolder = ApplicationData.Current.LocalCacheFolder;

        _thumbnailsFolder =
            await localFolder.CreateFolderAsync("software_screenshot_thumbnails", CreationCollisionOption.OpenIfExists);

        _screenshotsFolder =
            await localFolder.CreateFolderAsync("software_screenshots", CreationCollisionOption.OpenIfExists);
    }

    public async Task<Stream> GetThumbnailAsync(Guid screenshotId)
    {
        var filename = $"{screenshotId}.webp";

        if(await _thumbnailsFolder.TryGetItemAsync(filename) is StorageFile file)
            return await file.OpenStreamForReadAsync();

        await CacheThumbnailAsync(screenshotId);

        file = await _thumbnailsFolder.GetFileAsync(filename);

        return await file.OpenStreamForReadAsync();
    }

    public async Task<Stream> GetScreenshotAsync(Guid screenshotId)
    {
        var filename = $"{screenshotId}.webp";

        if(await _screenshotsFolder.TryGetItemAsync(filename) is StorageFile file)
            return await file.OpenStreamForReadAsync();

        await CacheScreenshotAsync(screenshotId);

        file = await _screenshotsFolder.GetFileAsync(filename);

        return await file.OpenStreamForReadAsync();
    }

    public async Task InvalidateCacheAsync(Guid screenshotId)
    {
        var filename = $"{screenshotId}.webp";

        if(await _thumbnailsFolder.TryGetItemAsync(filename) is StorageFile thumbFile)
            await thumbFile.DeleteAsync();

        if(await _screenshotsFolder.TryGetItemAsync(filename) is StorageFile screenshotFile)
            await screenshotFile.DeleteAsync();
    }

    async Task CacheThumbnailAsync(Guid screenshotId)
    {
        var    filename = $"{screenshotId}.webp";
        string baseUrl  = _configuration.GetSection("ApiClient:Url").Value;
        string url      = baseUrl + $"/assets/photos/software-screenshots/thumbs/webp/4k/{filename}";

        using var                 httpClient = new HttpClient();
        using HttpResponseMessage response   = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync();

        StorageFile file =
            await _thumbnailsFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

        using Stream fileStream = await file.OpenStreamForWriteAsync();
        await stream.CopyToAsync(fileStream);
    }

    async Task CacheScreenshotAsync(Guid screenshotId)
    {
        var    filename = $"{screenshotId}.webp";
        string baseUrl  = _configuration.GetSection("ApiClient:Url").Value;
        string url      = baseUrl + $"/assets/photos/software-screenshots/webp/4k/{filename}";

        using var                 httpClient = new HttpClient();
        using HttpResponseMessage response   = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync();

        StorageFile file =
            await _screenshotsFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

        using Stream fileStream = await file.OpenStreamForWriteAsync();
        await stream.CopyToAsync(fileStream);
    }
}
