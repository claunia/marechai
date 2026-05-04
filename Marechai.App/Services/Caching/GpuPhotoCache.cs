using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Extensions.Configuration;

namespace Marechai.App.Services.Caching;

public sealed class GpuPhotoCache
{
    readonly IConfiguration _configuration;
    StorageFolder           _photosFolder;
    StorageFolder           _thumbnailsFolder;

    public GpuPhotoCache(IConfiguration configuration)
    {
        _configuration = configuration;
        _              = EnsureFolderExistAsync();
    }

    async Task EnsureFolderExistAsync()
    {
        StorageFolder localFolder = ApplicationData.Current.LocalCacheFolder;

        _thumbnailsFolder =
            await localFolder.CreateFolderAsync("gpu_thumbnails", CreationCollisionOption.OpenIfExists);

        _photosFolder = await localFolder.CreateFolderAsync("gpu_photos", CreationCollisionOption.OpenIfExists);
    }

    public async Task<Stream> GetThumbnailAsync(Guid photoId)
    {
        var filename = $"{photoId}.webp";

        Stream retStream;

        if(await _thumbnailsFolder.TryGetItemAsync(filename) is StorageFile file)
        {
            retStream = await file.OpenStreamForReadAsync();

            return retStream;
        }

        await CacheThumbnailAsync(photoId);

        file = await _thumbnailsFolder.GetFileAsync(filename);

        retStream = await file.OpenStreamForReadAsync();

        return retStream;
    }

    public async Task<Stream> GetPhotoAsync(Guid photoId)
    {
        var filename = $"{photoId}.webp";

        Stream retStream;

        if(await _photosFolder.TryGetItemAsync(filename) is StorageFile file)
        {
            retStream = await file.OpenStreamForReadAsync();

            return retStream;
        }

        await CachePhotoAsync(photoId);

        file = await _photosFolder.GetFileAsync(filename);

        retStream = await file.OpenStreamForReadAsync();

        return retStream;
    }

    async Task CacheThumbnailAsync(Guid photoId)
    {
        var                       filename   = $"{photoId}.webp";
        string                    baseUrl    = _configuration.GetSection("ApiClient:Url").Value;
        string                    flagUrl    = baseUrl + $"/assets/photos/gpus/thumbs/webp/4k/{filename}";
        using var                 httpClient = new HttpClient();
        using HttpResponseMessage response   = await httpClient.GetAsync(flagUrl);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync();
        StorageFile  file = await _thumbnailsFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

        using Stream fileStream = await file.OpenStreamForWriteAsync();
        await stream.CopyToAsync(fileStream);
    }

    async Task CachePhotoAsync(Guid photoId)
    {
        var                       filename   = $"{photoId}.webp";
        string                    baseUrl    = _configuration.GetSection("ApiClient:Url").Value;
        string                    flagUrl    = baseUrl + $"/assets/photos/gpus/webp/4k/{filename}";
        using var                 httpClient = new HttpClient();
        using HttpResponseMessage response   = await httpClient.GetAsync(flagUrl);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync();
        StorageFile  file = await _photosFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

        using Stream fileStream = await file.OpenStreamForWriteAsync();
        await stream.CopyToAsync(fileStream);
    }
}
