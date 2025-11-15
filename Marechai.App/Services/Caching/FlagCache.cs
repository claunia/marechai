using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Extensions.Configuration;

namespace Marechai.App.Services.Caching;

public sealed class FlagCache
{
    readonly IConfiguration _configuration;
    StorageFolder           _flagsFolder;

    public FlagCache(IConfiguration configuration)
    {
        _configuration = configuration;
        _              = EnsureFolderExistAsync();
    }

    async Task EnsureFolderExistAsync()
    {
        StorageFolder localFolder = ApplicationData.Current.LocalCacheFolder;
        _flagsFolder = await localFolder.CreateFolderAsync("flags", CreationCollisionOption.OpenIfExists);
    }

    public async Task<Stream> GetFlagAsync(short countryCode)
    {
        var filename = $"{countryCode}.svg";

        Stream retStream;

        if(await _flagsFolder.TryGetItemAsync(filename) is StorageFile file)
        {
            retStream = await file.OpenStreamForReadAsync();

            return retStream;
        }

        await CacheFlagAsync(countryCode);

        file = await _flagsFolder.GetFileAsync(filename);

        retStream = await file.OpenStreamForReadAsync();

        return retStream;
    }

    async Task CacheFlagAsync(short countryCode)
    {
        var                       filename   = $"{countryCode}.svg";
        string                    baseUrl    = _configuration.GetSection("ApiClient:Url").Value;
        string                    flagUrl    = baseUrl + $"/assets/flags/{filename}";
        using var                 httpClient = new HttpClient();
        using HttpResponseMessage response   = await httpClient.GetAsync(flagUrl);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync();
        StorageFile  file   = await _flagsFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

        using Stream fileStream = await file.OpenStreamForWriteAsync();
        await stream.CopyToAsync(fileStream);
    }
}