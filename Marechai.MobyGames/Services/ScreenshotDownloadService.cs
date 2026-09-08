using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Models;
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
        Console.WriteLine(dryRun
                              ? "\n  \e[33;1m[DRY RUN]\e[0m Parsing screenshots without downloading...\n"
                              : downloadOnly
                                  ? "\n  Starting screenshot download \e[33;1m(--download-only: conversion skipped, writing to photos-new/)\e[0m...\n"
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

        var c              = new MediaCounters();
        int gamesProcessed = 0;
        int total          = Math.Min(batchSize, importedGames.Count);

        foreach(var game in importedGames.Take(batchSize))
        {
            gamesProcessed++;

            // Fetch the screenshot page directly from the fixed chunk slot
            string screenshotHtml = await _sourceDb.GetChunkBodyAsync(game.MobyGameId, NewGameRawFetcher.ChunkScreenshots);

            var rows = screenshotHtml is null
                           ? new List<MobyGamesRawRow>()
                           : [new MobyGamesRawRow { Id = game.MobyGameId, Chunk = NewGameRawFetcher.ChunkScreenshots, Body = screenshotHtml }];

            bool ok = await ProcessGameAsync(game, rows, processedUrls, dryRun, downloadOnly, c,
                                             $"[{gamesProcessed}/{total}]");

            if(!ok) break;
        }

        Console.WriteLine("\n  ────────────────────────────────────");

        if(dryRun)
        {
            Console.WriteLine("  \e[33;1m[DRY RUN]\e[0m No changes made");
            Console.WriteLine($"    Games scanned:          {gamesProcessed}");
            Console.WriteLine($"    No screenshot page:     {c.NoPage}");
            Console.WriteLine($"    Total screenshots found: {c.Total}");
        }
        else
        {
            Console.WriteLine($"    Games processed:        {gamesProcessed}");
            Console.WriteLine($"    No screenshot page:     {c.NoPage}");
            Console.WriteLine($"    Total screenshots found: {c.Total}");
            Console.WriteLine($"    Downloaded:             {c.Added}");
            Console.WriteLine($"    Skipped (existing):     {c.Skipped}");
            Console.WriteLine($"    Failed:                 {c.Failed}");
        }

        Console.WriteLine("  ────────────────────────────────────\n");
    }

    /// <summary>
    ///     Downloads every screenshot of ONE imported game not yet recorded as <c>Downloaded</c> in
    ///     <see cref="MobyGamesScreenshotDownloadState" />. The screenshots page is taken from
    ///     <paramref name="rows" /> (chunk <see cref="NewGameRawFetcher.ChunkScreenshots" />), which
    ///     may come from the source DB or from a live re-download held in memory. Dedupes by
    ///     MobyGames detail-page URL. Returns <c>false</c> when the run must abort (disk space).
    /// </summary>
    public async Task<bool> ProcessGameAsync(MobyGamesImportState game, List<MobyGamesRawRow> rows, HashSet<string> processedUrls,
        bool dryRun, bool downloadOnly, MediaCounters c, string progress = "")
    {
        string photosRoot = downloadOnly ? "photos-new" : "photos";

        if(!dryRun)
            await _platformMatcher.LoadAsync();


        string screenshotHtml = rows.FirstOrDefault(r => r.Chunk == NewGameRawFetcher.ChunkScreenshots)?.Body;

        if(screenshotHtml is null)
        {
            c.NoPage++;

            if(dryRun || c.NoPage <= 10)
                Console.WriteLine($"  {progress} {game.MobyGameId}: No scraped screenshot page (run scrape-screenshot-pages first)");

            if(c.NoPage == 10 && !dryRun)
                Console.WriteLine("  ... suppressing further 'no screenshot page' messages ...");

            return true;
        }

        var screenshotGroups = ScreenshotsPageParser.Parse(screenshotHtml);

        if(screenshotGroups.Count == 0)
        {
            // Game has a Screenshots tab on MobyGames but no actual screenshots uploaded — skip silently
            return true;
        }

        int imageCount = screenshotGroups.Sum(g => g.Images.Count);

        Console.WriteLine($"\n  {progress} \e[36;1m{game.MobyGameId}\e[0m — {screenshotGroups.Count} platform(s), {imageCount} screenshot(s)");

        foreach(var group in screenshotGroups)
        {

            c.Total += group.Images.Count;

            Console.WriteLine($"    Platform: {group.PlatformName} ({group.Images.Count} screenshots)");

            // Match platform
            SoftwarePlatform platform = null;

            if(!dryRun)
                platform = await _platformMatcher.MatchOrCreateAsync(group.PlatformName);

            foreach(var image in group.Images)
            {

                string dedupeKey = image.DetailPageUrl ?? $"screenshot-{image.ScreenshotId}";

                if(dryRun)
                {
                    if(processedUrls.Contains(dedupeKey))
                    {
                        c.Skipped++;

                        continue;
                    }

                    Console.WriteLine($"      screenshot-{image.ScreenshotId}: {image.Caption ?? "(no caption)"}");
                    c.WouldAdd++;

                    continue;
                }

                if(processedUrls.Contains(dedupeKey))
                {
                    c.Skipped++;

                    continue;
                }

                var existingState = await _stateService.GetStateByScreenshotUrlAsync(dedupeKey);

                if(existingState is not null &&
                   existingState.Status == MobyGamesCoverDownloadStatus.Downloaded)
                {
                    c.Skipped++;

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
                    c.Failed++;

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
                    c.Failed++;

                    continue;
                }

                if(!HasSufficientDiskSpace(_assetRootPath))
                {
                    Console.WriteLine("\n\n  \e[31;1mABORTING: Less than 100 MB free disk space.\e[0m\n");
                    return false;
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
                    c.Failed++;

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

                c.Added++;
            }
        }

        return true;
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
