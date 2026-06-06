using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Parsers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class ScreenshotDownloadService
{
    const string ScreenshotItemName = "software-screenshots";

    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly ScreenshotStateService            _stateService;
    readonly PlatformMatcher                   _platformMatcher;
    readonly MobyGamesHttpClient               _httpClient;
    readonly string                            _assetRootPath;

    public ScreenshotDownloadService(
        IDbContextFactory<MarechaiContext> contextFactory,
        SourceDatabaseService             sourceDb,
        ScreenshotStateService            stateService,
        PlatformMatcher                   platformMatcher,
        MobyGamesHttpClient               httpClient,
        string                            assetRootPath)
    {
        _contextFactory  = contextFactory;
        _sourceDb        = sourceDb;
        _stateService    = stateService;
        _platformMatcher = platformMatcher;
        _httpClient      = httpClient;
        _assetRootPath   = assetRootPath;
    }

    public async Task RunAsync(int batchSize, bool dryRun, bool downloadOnly = false)
    {
        // When --download-only is set, originals are written to `photos-new/` instead of
        // `photos/` so the operator can rsync them to a separate conversion host without
        // colliding with the existing converted-asset tree.
        string photosRoot = downloadOnly ? "photos-new" : "photos";

        Console.WriteLine(dryRun
                              ? "\n  \e[33;1m[DRY RUN]\e[0m Parsing screenshots without downloading...\n"
                              : downloadOnly
                                  ? $"\n  Starting screenshot download \e[33;1m(--download-only: conversion skipped, writing to {photosRoot}/)\e[0m...\n"
                                  : "\n  Starting screenshot download...\n");

        await using var context = await _contextFactory.CreateDbContextAsync();

        var importedGames = await context.MobyGamesImportStates
                                         .Where(s => s.Status     == MobyGamesImportStatus.Imported &&
                                                     s.SoftwareId != null)
                                         .OrderBy(s => s.MobyGameId)
                                         .ToListAsync();

        Console.WriteLine($"  Found {importedGames.Count} imported games");

        var processedUrls = dryRun ? new HashSet<string>() : await _stateService.GetProcessedScreenshotUrlsAsync();

        Console.WriteLine($"  Already downloaded: {processedUrls.Count} screenshots\n");

        if(!dryRun)
            await _platformMatcher.LoadAsync();

        int totalImages     = 0;
        int downloadedCount = 0;
        int skippedCount    = 0;
        int failedCount     = 0;
        int noScreenshotPage = 0;
        int gamesProcessed  = 0;
        bool aborted        = false;

        foreach(var game in importedGames.Take(batchSize))
        {
            if(aborted) break;

            gamesProcessed++;

            // Fetch the screenshot page directly from the fixed chunk slot
            string screenshotHtml = await _sourceDb.GetChunkBodyAsync(game.MobyGameId, NewGameRawFetcher.ChunkScreenshots);

            if(screenshotHtml is null)
            {
                noScreenshotPage++;

                if(dryRun || noScreenshotPage <= 10)
                    Console.WriteLine($"  [{gamesProcessed}/{Math.Min(batchSize, importedGames.Count)}] {game.MobyGameId}: No scraped screenshot page (run scrape-screenshot-pages first)");

                if(noScreenshotPage == 10 && !dryRun)
                    Console.WriteLine("  ... suppressing further 'no screenshot page' messages ...");

                continue;
            }

            var screenshotGroups = ScreenshotsPageParser.Parse(screenshotHtml);

            if(screenshotGroups.Count == 0)
            {
                // Game has a Screenshots tab on MobyGames but no actual screenshots uploaded — skip silently
                continue;
            }

            int imageCount = screenshotGroups.Sum(g => g.Images.Count);

            Console.WriteLine($"\n  [{gamesProcessed}] \e[36;1m{game.MobyGameId}\e[0m — {screenshotGroups.Count} platform(s), {imageCount} screenshot(s)");

            foreach(var group in screenshotGroups)
            {
                if(aborted) break;

                totalImages += group.Images.Count;

                Console.WriteLine($"    Platform: {group.PlatformName} ({group.Images.Count} screenshots)");

                // Match platform
                SoftwarePlatform platform = null;

                if(!dryRun)
                    platform = await _platformMatcher.MatchOrCreateAsync(group.PlatformName);

                foreach(var image in group.Images)
                {
                    if(aborted) break;

                    if(dryRun)
                    {
                        Console.WriteLine($"      screenshot-{image.ScreenshotId}: {image.Caption ?? "(no caption)"}");

                        continue;
                    }

                    string dedupeKey = image.DetailPageUrl ?? $"screenshot-{image.ScreenshotId}";

                    if(processedUrls.Contains(dedupeKey))
                    {
                        skippedCount++;

                        continue;
                    }

                    var existingState = await _stateService.GetStateByScreenshotUrlAsync(dedupeKey);

                    if(existingState is not null &&
                       existingState.Status == MobyGamesCoverDownloadStatus.Downloaded)
                    {
                        skippedCount++;

                        continue;
                    }

                    if(existingState is null)
                    {
                        existingState = new MobyGamesScreenshotDownloadState
                        {
                            MobyGameId        = game.MobyGameId,
                            SoftwareId        = game.SoftwareId!.Value,
                            ScreenshotPageUrl = dedupeKey,
                            Caption           = image.Caption,
                            Platform          = group.PlatformName,
                            Status            = MobyGamesCoverDownloadStatus.Pending
                        };

                        await _stateService.CreateStateAsync(existingState);
                    }

                    // Visit detail page to get the full-size image URL
                    Console.Write($"      Fetching detail for screenshot-{image.ScreenshotId}...");

                    string detailHtml = await _httpClient.FetchPageAsync(image.DetailPageUrl);

                    if(detailHtml is null)
                    {
                        Console.WriteLine(" \e[31mFAILED\e[0m (detail page fetch failed)");

                        existingState.Status       = MobyGamesCoverDownloadStatus.Failed;
                        existingState.ErrorMessage = "Detail page fetch failed";
                        existingState.ProcessedOn  = DateTime.UtcNow;
                        await _stateService.UpdateStateAsync(existingState);
                        failedCount++;

                        continue;
                    }

                    string originalUrl;
                    bool   isHighRes;
                    (originalUrl, isHighRes) = MobyGamesHttpClient.ExtractFullSizeImageUrl(detailHtml);

                    if(originalUrl is null)
                    {
                        Console.WriteLine(" \e[31mFAILED\e[0m (no image URL found in detail page)");

                        existingState.Status       = MobyGamesCoverDownloadStatus.Failed;
                        existingState.ErrorMessage = "No full-size image URL found in detail page";
                        existingState.ProcessedOn  = DateTime.UtcNow;
                        await _stateService.UpdateStateAsync(existingState);
                        failedCount++;

                        continue;
                    }

                    if(!HasSufficientDiskSpace(_assetRootPath))
                    {
                        Console.WriteLine("\n\n  \e[31;1mABORTING: Less than 100 MB free disk space.\e[0m\n");
                        aborted = true;

                        break;
                    }

                    Console.Write(isHighRes ? " downloading (high res)..." : " downloading...");

                    var    screenshotId = Guid.NewGuid();
                    string originalsDir = Path.Combine(_assetRootPath, photosRoot, ScreenshotItemName, "originals");
                    Directory.CreateDirectory(originalsDir);
                    string destBasePath = Path.Combine(originalsDir, screenshotId.ToString());

                    string extension = await _httpClient.DownloadImageAsync(originalUrl, destBasePath);

                    if(extension is null)
                    {
                        Console.WriteLine(" \e[31mFAILED\e[0m (download failed)");

                        existingState.Status       = MobyGamesCoverDownloadStatus.Failed;
                        existingState.ErrorMessage = "Image download failed";
                        existingState.ProcessedOn  = DateTime.UtcNow;
                        await _stateService.UpdateStateAsync(existingState);
                        failedCount++;

                        continue;
                    }

                    string originalFilePath = $"{destBasePath}.{extension}";

                    // Create SoftwareScreenshot record
                    await using var dbContext = await _contextFactory.CreateDbContextAsync();

                    var screenshot = new SoftwareScreenshot
                    {
                        Id                 = screenshotId,
                        SoftwareId         = game.SoftwareId!.Value,
                        SoftwarePlatformId = platform?.Id,
                        Caption            = image.Caption,
                        OriginalExtension  = extension
                    };

                    dbContext.SoftwareScreenshots.Add(screenshot);
                    await dbContext.SaveChangesAsync();

                    if(downloadOnly)
                    {
                        Console.WriteLine(" \e[33mskipped conversion\e[0m");
                    }
                    else
                    {
                        Console.Write(" converting...");

                        try
                        {
                            ImageConverter.ConvertAll(_assetRootPath, screenshotId, originalFilePath, extension,
                                                     ScreenshotItemName);

                            Console.WriteLine(" \e[32mOK\e[0m");
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($" \e[33mconversion warning: {ex}\e[0m");
                        }
                    }

                    existingState.Status               = MobyGamesCoverDownloadStatus.Downloaded;
                    existingState.SoftwareScreenshotId  = screenshotId;
                    existingState.OriginalUrl           = originalUrl;
                    existingState.ProcessedOn           = DateTime.UtcNow;
                    await _stateService.UpdateStateAsync(existingState);

                    downloadedCount++;
                }
            }
        }

        Console.WriteLine("\n  ────────────────────────────────────");

        if(dryRun)
        {
            Console.WriteLine("  \e[33;1m[DRY RUN]\e[0m No changes made");
            Console.WriteLine($"    Games scanned:          {gamesProcessed}");
            Console.WriteLine($"    No screenshot page:     {noScreenshotPage}");
            Console.WriteLine($"    Total screenshots found: {totalImages}");
        }
        else
        {
            Console.WriteLine($"    Games processed:        {gamesProcessed}");
            Console.WriteLine($"    No screenshot page:     {noScreenshotPage}");
            Console.WriteLine($"    Total screenshots found: {totalImages}");
            Console.WriteLine($"    Downloaded:             {downloadedCount}");
            Console.WriteLine($"    Skipped (existing):     {skippedCount}");
            Console.WriteLine($"    Failed:                 {failedCount}");
        }

        Console.WriteLine("  ────────────────────────────────────\n");
    }

    const long MinFreeSpaceBytes = 100 * 1024 * 1024;

    static bool HasSufficientDiskSpace(string path)
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(path))!);

            return driveInfo.AvailableFreeSpace > MinFreeSpaceBytes;
        }
        catch
        {
            return true;
        }
    }
}
