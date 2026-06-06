using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Parsers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Imports video links from scraped MobyGames media pages into the database.
///     Phase 2: parses stored media HTML, extracts YouTube video IDs, creates SoftwareVideo records.
///     No image/file download needed — only extracts URLs and metadata.
/// </summary>
public class VideoImportService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly VideoStateService                 _stateService;

    public VideoImportService(
        IDbContextFactory<MarechaiContext> contextFactory,
        SourceDatabaseService             sourceDb,
        VideoStateService                 stateService)
    {
        _contextFactory = contextFactory;
        _sourceDb       = sourceDb;
        _stateService   = stateService;
    }

    public async Task RunAsync(int batchSize, bool dryRun)
    {
        Console.WriteLine(dryRun
                              ? "\n  \e[33;1m[DRY RUN]\e[0m Parsing videos without importing...\n"
                              : "\n  Starting video import...\n");

        await using var context = await _contextFactory.CreateDbContextAsync();

        var importedGames = await context.MobyGamesImportStates
                                         .Where(s => s.Status     == MobyGamesImportStatus.Imported &&
                                                     s.SoftwareId != null)
                                         .OrderBy(s => s.MobyGameId)
                                         .ToListAsync();

        Console.WriteLine($"  Found {importedGames.Count} imported games");

        var processedUrls = dryRun ? new HashSet<string>() : await _stateService.GetProcessedVideoUrlsAsync();

        Console.WriteLine($"  Already imported: {processedUrls.Count} videos\n");

        int totalVideos    = 0;
        int importedCount  = 0;
        int skippedCount   = 0;
        int failedCount    = 0;
        int noMediaPage    = 0;
        int gamesProcessed = 0;

        foreach(var game in importedGames.Take(batchSize))
        {
            gamesProcessed++;

            // Fetch the media page directly from the fixed chunk slot
            string mediaHtml = await _sourceDb.GetChunkBodyAsync(game.MobyGameId, NewGameRawFetcher.ChunkMedia);

            if(mediaHtml is null)
            {
                noMediaPage++;

                if(dryRun || noMediaPage <= 10)
                    Console.WriteLine($"  [{gamesProcessed}/{Math.Min(batchSize, importedGames.Count)}] {game.MobyGameId}: No scraped media page (run scrape-media-pages first)");

                if(noMediaPage == 10 && !dryRun)
                    Console.WriteLine("  ... suppressing further 'no media page' messages ...");

                continue;
            }

            var videos = MediaPageParser.Parse(mediaHtml);

            if(videos.Count == 0) continue;

            Console.WriteLine($"\n  [{gamesProcessed}] \e[36;1m{game.MobyGameId}\e[0m — {videos.Count} video(s)");

            totalVideos += videos.Count;

            foreach(var video in videos)
            {
                string dedupeKey = video.EmbedUrl;

                if(dryRun)
                {
                    Console.WriteLine($"    {video.Provider}: {video.VideoId} — {video.Title ?? "(no title)"}");

                    continue;
                }

                if(processedUrls.Contains(dedupeKey))
                {
                    skippedCount++;

                    continue;
                }

                var existingState = await _stateService.GetStateByVideoUrlAsync(dedupeKey);

                if(existingState is not null &&
                   existingState.Status == MobyGamesCoverDownloadStatus.Downloaded)
                {
                    skippedCount++;

                    continue;
                }

                if(existingState is null)
                {
                    existingState = new MobyGamesVideoImportState
                    {
                        MobyGameId       = game.MobyGameId,
                        SoftwareId       = game.SoftwareId!.Value,
                        VideoUrl         = dedupeKey,
                        Title            = video.Title,
                        Provider         = video.Provider,
                        ExtractedVideoId = video.VideoId,
                        Status           = MobyGamesCoverDownloadStatus.Pending
                    };

                    await _stateService.CreateStateAsync(existingState);
                }

                try
                {
                    // Check if this exact video already exists for this software
                    await using var dbContext = await _contextFactory.CreateDbContextAsync();

                    bool alreadyExists = await dbContext.SoftwareVideos
                                                       .AnyAsync(v => v.SoftwareId == game.SoftwareId!.Value &&
                                                                      v.Provider   == video.Provider &&
                                                                      v.VideoId    == video.VideoId);

                    if(alreadyExists)
                    {
                        existingState.Status      = MobyGamesCoverDownloadStatus.Skipped;
                        existingState.ProcessedOn = DateTime.UtcNow;
                        await _stateService.UpdateStateAsync(existingState);
                        skippedCount++;

                        Console.WriteLine($"    {video.Provider}: {video.VideoId} — already exists, skipped");

                        continue;
                    }

                    var softwareVideo = new SoftwareVideo
                    {
                        SoftwareId = game.SoftwareId!.Value,
                        Provider   = video.Provider,
                        VideoId    = video.VideoId,
                        Title      = video.Title
                    };

                    dbContext.SoftwareVideos.Add(softwareVideo);
                    await dbContext.SaveChangesAsync();

                    existingState.Status          = MobyGamesCoverDownloadStatus.Downloaded;
                    existingState.SoftwareVideoId = softwareVideo.Id;
                    existingState.ProcessedOn     = DateTime.UtcNow;
                    await _stateService.UpdateStateAsync(existingState);

                    importedCount++;
                    processedUrls.Add(dedupeKey);

                    Console.WriteLine($"    {video.Provider}: {video.VideoId} — \e[32mOK\e[0m ({video.Title ?? "no title"})");
                }
                catch(Exception ex)
                {
                    existingState.Status       = MobyGamesCoverDownloadStatus.Failed;
                    existingState.ErrorMessage = ex.Message.Length > 1024 ? ex.Message[..1024] : ex.Message;
                    existingState.ProcessedOn  = DateTime.UtcNow;
                    await _stateService.UpdateStateAsync(existingState);
                    failedCount++;

                    Console.WriteLine($"    {video.Provider}: {video.VideoId} — \e[31mFAILED\e[0m ({ex.Message})");
                }
            }
        }

        Console.WriteLine("\n  ────────────────────────────────────");

        if(dryRun)
        {
            Console.WriteLine("  \e[33;1m[DRY RUN]\e[0m No changes made");
            Console.WriteLine($"    Games scanned:       {gamesProcessed}");
            Console.WriteLine($"    No media page:       {noMediaPage}");
            Console.WriteLine($"    Total videos found:  {totalVideos}");
        }
        else
        {
            Console.WriteLine($"    Games processed:     {gamesProcessed}");
            Console.WriteLine($"    No media page:       {noMediaPage}");
            Console.WriteLine($"    Total videos found:  {totalVideos}");
            Console.WriteLine($"    Imported:            {importedCount}");
            Console.WriteLine($"    Skipped (existing):  {skippedCount}");
            Console.WriteLine($"    Failed:              {failedCount}");
        }

        Console.WriteLine("  ────────────────────────────────────\n");
    }
}
