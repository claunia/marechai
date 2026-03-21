using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Extensions.Configuration;

namespace Marechai.App.Services.Caching;

public sealed class BookCoverCache
{
    readonly IConfiguration _configuration;
    StorageFolder           _coversFolder;
    StorageFolder           _thumbnailsFolder;

    public BookCoverCache(IConfiguration configuration)
    {
        _configuration = configuration;
        _              = EnsureFolderExistAsync();
    }

    async Task EnsureFolderExistAsync()
    {
        StorageFolder localFolder = ApplicationData.Current.LocalCacheFolder;

        _thumbnailsFolder =
            await localFolder.CreateFolderAsync("book_cover_thumbnails", CreationCollisionOption.OpenIfExists);

        _coversFolder = await localFolder.CreateFolderAsync("book_covers", CreationCollisionOption.OpenIfExists);
    }

    public async Task<Stream> GetThumbnailAsync(Guid coverId)
    {
        var filename = $"{coverId}.webp";

        if(await _thumbnailsFolder.TryGetItemAsync(filename) is StorageFile file)
            return await file.OpenStreamForReadAsync();

        await CacheThumbnailAsync(coverId);

        file = await _thumbnailsFolder.GetFileAsync(filename);

        return await file.OpenStreamForReadAsync();
    }

    public async Task<Stream> GetCoverAsync(Guid coverId)
    {
        var filename = $"{coverId}.webp";

        if(await _coversFolder.TryGetItemAsync(filename) is StorageFile file)
            return await file.OpenStreamForReadAsync();

        await CacheCoverAsync(coverId);

        file = await _coversFolder.GetFileAsync(filename);

        return await file.OpenStreamForReadAsync();
    }

    public async Task InvalidateCacheAsync(Guid coverId)
    {
        var filename = $"{coverId}.webp";

        if(await _thumbnailsFolder.TryGetItemAsync(filename) is StorageFile thumbFile)
            await thumbFile.DeleteAsync();

        if(await _coversFolder.TryGetItemAsync(filename) is StorageFile coverFile)
            await coverFile.DeleteAsync();
    }

    async Task CacheThumbnailAsync(Guid coverId)
    {
        var                       filename   = $"{coverId}.webp";
        string                    baseUrl    = _configuration.GetSection("ApiClient:Url").Value;
        string                    url        = baseUrl + $"/assets/photos/book-covers/thumbs/webp/4k/{filename}";
        using var                 httpClient = new HttpClient();
        using HttpResponseMessage response   = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync();
        StorageFile  file = await _thumbnailsFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

        using Stream fileStream = await file.OpenStreamForWriteAsync();
        await stream.CopyToAsync(fileStream);
    }

    async Task CacheCoverAsync(Guid coverId)
    {
        var                       filename   = $"{coverId}.webp";
        string                    baseUrl    = _configuration.GetSection("ApiClient:Url").Value;
        string                    url        = baseUrl + $"/assets/photos/book-covers/webp/4k/{filename}";
        using var                 httpClient = new HttpClient();
        using HttpResponseMessage response   = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync();
        StorageFile  file = await _coversFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

        using Stream fileStream = await file.OpenStreamForWriteAsync();
        await stream.CopyToAsync(fileStream);
    }
}
