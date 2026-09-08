using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Models;
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

        var c              = new MediaCounters();
        int gamesProcessed = 0;
        int total          = Math.Min(batchSize, importedGames.Count);

        foreach(var game in importedGames.Take(batchSize))
        {
            gamesProcessed++;

            // Fetch the media page directly from the fixed chunk slot
            string mediaHtml = await _sourceDb.GetChunkBodyAsync(game.MobyGameId, NewGameRawFetcher.ChunkMedia);

            var rows = mediaHtml is null
                           ? new List<MobyGamesRawRow>()
                           : [new MobyGamesRawRow { Id = game.MobyGameId, Chunk = NewGameRawFetcher.ChunkMedia, Body = mediaHtml }];

            await ProcessGameAsync(game, rows, processedUrls, dryRun, c, $"[{gamesProcessed}/{total}]");
        }

        Console.WriteLine("\n  ────────────────────────────────────");

        if(dryRun)
        {
            Console.WriteLine("  \e[33;1m[DRY RUN]\e[0m No changes made");
            Console.WriteLine($"    Games scanned:       {gamesProcessed}");
            Console.WriteLine($"    No media page:       {c.NoPage}");
            Console.WriteLine($"    Total videos found:  {c.Total}");
        }
        else
        {
            Console.WriteLine($"    Games processed:     {gamesProcessed}");
            Console.WriteLine($"    No media page:       {c.NoPage}");
            Console.WriteLine($"    Total videos found:  {c.Total}");
            Console.WriteLine($"    Imported:            {c.Added}");
            Console.WriteLine($"    Skipped (existing):  {c.Skipped}");
            Console.WriteLine($"    Failed:              {c.Failed}");
        }

        Console.WriteLine("  ────────────────────────────────────\n");
    }

    /// <summary>
    ///     Imports every video of ONE imported game not yet recorded as <c>Downloaded</c> in
    ///     <see cref="MobyGamesVideoImportState" />. The media page is taken from
    ///     <paramref name="rows" /> (chunk <see cref="NewGameRawFetcher.ChunkMedia" />), which may
    ///     come from the source DB or from a live re-download held in memory. Dedupes by embed URL
    ///     and by the unique (SoftwareId, Provider, VideoId) key.
    /// </summary>
    public async Task<bool> ProcessGameAsync(MobyGamesImportState game, List<MobyGamesRawRow> rows,
                                             HashSet<string> processedUrls, bool dryRun, MediaCounters c,
                                             string progress = "")
    {

        string mediaHtml = rows.FirstOrDefault(r => r.Chunk == NewGameRawFetcher.ChunkMedia)?.Body;

        if(mediaHtml is null)
        {
            c.NoPage++;

            if(dryRun || c.NoPage <= 10)
                Console.WriteLine($"  {progress} {game.MobyGameId}: No scraped media page (run scrape-media-pages first)");

            if(c.NoPage == 10 && !dryRun)
                Console.WriteLine("  ... suppressing further 'no media page' messages ...");

            return true;
        }

        var videos = MediaPageParser.Parse(mediaHtml);

        if(videos.Count == 0) return true;

        Console.WriteLine($"\n  {progress} \e[36;1m{game.MobyGameId}\e[0m — {videos.Count} video(s)");

        c.Total += videos.Count;

        foreach(var video in videos)
        {
            string dedupeKey = video.EmbedUrl;

            if(dryRun)
            {
                if(processedUrls.Contains(dedupeKey))
                {
                    c.Skipped++;

                    continue;
                }

                Console.WriteLine($"    {video.Provider}: {video.VideoId} — {video.Title ?? "(no title)"}");
                c.WouldAdd++;

                continue;
            }

            if(processedUrls.Contains(dedupeKey))
            {
                c.Skipped++;

                continue;
            }

            var existingState = await _stateService.GetStateByVideoUrlAsync(dedupeKey);

            if(existingState is not null &&
               existingState.Status == MobyGamesCoverDownloadStatus.Downloaded)
            {
                c.Skipped++;

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
                    c.Skipped++;

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

                c.Added++;
                processedUrls.Add(dedupeKey);

                Console.WriteLine($"    {video.Provider}: {video.VideoId} — \e[32mOK\e[0m ({video.Title ?? "no title"})");
            }
            catch(Exception ex)
            {
                existingState.Status       = MobyGamesCoverDownloadStatus.Failed;
                existingState.ErrorMessage = ex.Message.Length > 1024 ? ex.Message[..1024] : ex.Message;
                existingState.ProcessedOn  = DateTime.UtcNow;
                await _stateService.UpdateStateAsync(existingState);
                c.Failed++;

                Console.WriteLine($"    {video.Provider}: {video.VideoId} — \e[31mFAILED\e[0m ({ex.Message})");
            }
        }

        return true;
    }

}
