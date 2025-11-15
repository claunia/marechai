using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Extensions.Configuration;

namespace Marechai.App.Services.Caching;

public sealed class CompanyLogoCache
{
    readonly IConfiguration _configuration;
    StorageFolder           _flagsFolder;

    public CompanyLogoCache(IConfiguration configuration)
    {
        _configuration = configuration;
        _              = EnsureFolderExistAsync();
    }

    async Task EnsureFolderExistAsync()
    {
        StorageFolder localFolder = ApplicationData.Current.LocalCacheFolder;
        _flagsFolder = await localFolder.CreateFolderAsync("logos", CreationCollisionOption.OpenIfExists);
    }

    public async Task<Stream> GetFlagAsync(Guid companyLogoId)
    {
        var filename = $"{companyLogoId}.svg";

        Stream retStream;

        if(await _flagsFolder.TryGetItemAsync(filename) is StorageFile file)
        {
            retStream = await file.OpenStreamForReadAsync();

            return retStream;
        }

        await CacheFlagAsync(companyLogoId);

        file = await _flagsFolder.GetFileAsync(filename);

        retStream = await file.OpenStreamForReadAsync();

        return retStream;
    }

    async Task CacheFlagAsync(Guid companyLogoId)
    {
        var                       filename   = $"{companyLogoId}.svg";
        string                    baseUrl    = _configuration.GetSection("ApiClient:Url").Value;
        string                    flagUrl    = baseUrl + $"/assets/logos/{filename}";
        using var                 httpClient = new HttpClient();
        using HttpResponseMessage response   = await httpClient.GetAsync(flagUrl);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync();
        StorageFile  file   = await _flagsFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

        using Stream fileStream = await file.OpenStreamForWriteAsync();
        await stream.CopyToAsync(fileStream);
    }
}