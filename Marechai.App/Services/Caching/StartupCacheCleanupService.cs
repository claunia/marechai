using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Windows.Storage;

namespace Marechai.App.Services.Caching;

internal sealed class StartupCacheCleanupService
{
    const int RetentionDays = 15;

    static readonly string[] CacheFolderNames =
    [
        "flags",
        "logos",
        "machine_thumbnails",
        "machine_photos",
        "machine_promo_art_thumbnails",
        "machine_promo_art",
        "gpu_thumbnails",
        "gpu_photos",
        "processor_thumbnails",
        "processor_photos",
        "sound_synth_thumbnails",
        "sound_synth_photos",
        "book_cover_thumbnails",
        "book_covers",
        "magazine_issue_cover_thumbnails",
        "magazine_issue_covers",
        "software_screenshot_thumbnails",
        "software_screenshots",
        "software_cover_thumbnails",
        "software_covers",
        "software_promo_art_thumbnails",
        "software_promo_art"
    ];

    readonly ILogger<StartupCacheCleanupService> _logger;

    public StartupCacheCleanupService(ILogger<StartupCacheCleanupService> logger) => _logger = logger;

    public async Task CleanupExpiredEntriesAsync()
    {
        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-RetentionDays);

        foreach(string folderName in CacheFolderNames)
        {
            try
            {
                StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;
                IStorageItem? folderItem = await localCacheFolder.TryGetItemAsync(folderName);

                if(folderItem is not StorageFolder folder)
                    continue;

                await CleanupFolderAsync(folder, cutoff);
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean cache folder {FolderName}", folderName);
            }
        }
    }

    static bool IsExpired(DateTimeOffset modifiedAt, DateTimeOffset cutoff) => modifiedAt < cutoff;

    async Task CleanupFolderAsync(StorageFolder folder, DateTimeOffset cutoff)
    {
        IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();

        foreach(StorageFile file in files)
        {
            try
            {
                var properties = await file.GetBasicPropertiesAsync();

                if(!IsExpired(properties.DateModified, cutoff))
                    continue;

                await file.DeleteAsync();
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove expired cache file {FileName} from {FolderName}",
                                   file.Name, folder.Name);
            }
        }
    }
}
