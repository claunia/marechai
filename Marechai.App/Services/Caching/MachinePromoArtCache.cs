using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Extensions.Configuration;

namespace Marechai.App.Services.Caching;

public sealed class MachinePromoArtCache
{
    readonly IConfiguration _configuration;
    StorageFolder           _promoArtFolder;
    StorageFolder           _thumbnailsFolder;

    public MachinePromoArtCache(IConfiguration configuration)
    {
        _configuration = configuration;
        _              = EnsureFolderExistAsync();
    }

    async Task EnsureFolderExistAsync()
    {
        StorageFolder localFolder = ApplicationData.Current.LocalCacheFolder;

        _thumbnailsFolder =
            await localFolder.CreateFolderAsync("machine_promo_art_thumbnails", CreationCollisionOption.OpenIfExists);

        _promoArtFolder =
            await localFolder.CreateFolderAsync("machine_promo_art", CreationCollisionOption.OpenIfExists);
    }

    public async Task<Stream> GetThumbnailAsync(Guid promoArtId)
    {
        var filename = $"{promoArtId}.webp";

        if(await _thumbnailsFolder.TryGetItemAsync(filename) is StorageFile file)
            return await file.OpenStreamForReadAsync();

        await CacheThumbnailAsync(promoArtId);

        file = await _thumbnailsFolder.GetFileAsync(filename);

        return await file.OpenStreamForReadAsync();
    }

    public async Task<Stream> GetPromoArtAsync(Guid promoArtId)
    {
        var filename = $"{promoArtId}.webp";

        if(await _promoArtFolder.TryGetItemAsync(filename) is StorageFile file)
            return await file.OpenStreamForReadAsync();

        await CachePromoArtAsync(promoArtId);

        file = await _promoArtFolder.GetFileAsync(filename);

        return await file.OpenStreamForReadAsync();
    }

    async Task CacheThumbnailAsync(Guid promoArtId)
    {
        var    filename = $"{promoArtId}.webp";
        string baseUrl  = _configuration.GetSection("ApiClient:Url").Value;
        string url      = baseUrl + $"/assets/photos/machine-promo-art/thumbs/webp/4k/{filename}";

        using var                 httpClient = new HttpClient();
        using HttpResponseMessage response   = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync();

        StorageFile file =
            await _thumbnailsFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

        using Stream fileStream = await file.OpenStreamForWriteAsync();
        await stream.CopyToAsync(fileStream);
    }

    async Task CachePromoArtAsync(Guid promoArtId)
    {
        var    filename = $"{promoArtId}.webp";
        string baseUrl  = _configuration.GetSection("ApiClient:Url").Value;
        string url      = baseUrl + $"/assets/photos/machine-promo-art/webp/4k/{filename}";

        using var                 httpClient = new HttpClient();
        using HttpResponseMessage response   = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync();

        StorageFile file =
            await _promoArtFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

        using Stream fileStream = await file.OpenStreamForWriteAsync();
        await stream.CopyToAsync(fileStream);
    }
}
