using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Scrapes media (video) pages from the new MobyGames site and stores HTML in mobygames_raw.
///     Phase 1: resolves slug → numeric ID, fetches /media/ page, stores in source DB.
/// </summary>
public class MediaScraper
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly MobyGamesHttpClient               _httpClient;

    public MediaScraper(
        IDbContextFactory<MarechaiContext> contextFactory,
        SourceDatabaseService             sourceDb,
        MobyGamesHttpClient               httpClient)
    {
        _contextFactory = contextFactory;
        _sourceDb       = sourceDb;
        _httpClient     = httpClient;
    }

    public async Task RunAsync(int batchSize, bool dryRun)
    {
        Console.WriteLine(dryRun
                              ? "\n  \e[33;1m[DRY RUN]\e[0m Detecting games with media tab...\n"
                              : "\n  Starting media page scraping...\n");

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get imported games
        var importedGames = await context.MobyGamesImportStates
                                         .Where(s => s.Status     == MobyGamesImportStatus.Imported &&
                                                     s.SoftwareId != null)
                                         .OrderBy(s => s.MobyGameId)
                                         .ToListAsync();

        Console.WriteLine($"  Found {importedGames.Count} imported games");

        int scraped  = 0;
        int skipped  = 0;
        int noMedia  = 0;
        int failed   = 0;
        int gamesChecked = 0;

        foreach(var game in importedGames.Take(batchSize))
        {
            gamesChecked++;

            var rows = await _sourceDb.GetRowsForGameAsync(game.MobyGameId);

            // Check if we already scraped this game's media page
            // The gtag content_type "game-list-media" is present on media pages
            bool alreadyScraped = rows.Any(r => r.Body.Contains("game-list-media"));

            if(alreadyScraped)
            {
                if(gamesChecked <= 5 || dryRun)
                    Console.WriteLine($"  [{gamesChecked}/{Math.Min(batchSize, importedGames.Count)}] {game.MobyGameId}: Already scraped, skipping");

                skipped++;

                continue;
            }

            if(dryRun)
            {
                Console.WriteLine($"  [{gamesChecked}] {game.MobyGameId}: Would scrape media page");
                scraped++;

                continue;
            }

            // Resolve numeric ID if not already known
            if(game.MobyNumericId is null)
            {
                foreach(var row in rows)
                {
                    int? extractedId = MobyGamesHttpClient.ExtractNumericGameIdFromHtml(row.Body);

                    if(extractedId is not null)
                    {
                        game.MobyNumericId = extractedId;
                        await context.SaveChangesAsync();

                        break;
                    }
                }
            }

            if(game.MobyNumericId is null && _httpClient is not null)
            {
                int? numericId = await _httpClient.ResolveNumericGameIdAsync(game.MobyGameId);

                if(numericId is not null)
                {
                    game.MobyNumericId = numericId;
                    await context.SaveChangesAsync();
                }
            }

            if(game.MobyNumericId is null)
            {
                failed++;

                continue;
            }

            // Fetch media page from new MobyGames
            string cleanSlug = game.MobyGameId.TrimStart('-');

            string mediaUrl =
                $"https://www.mobygames.com/game/{game.MobyNumericId}/{cleanSlug}/media/";

            Console.Write($"  [{gamesChecked}] {game.MobyGameId}: Fetching media page...");

            string html = await _httpClient.FetchPageAsync(mediaUrl);

            if(html is null)
            {
                Console.WriteLine(" \e[31mFAILED\e[0m");
                failed++;

                continue;
            }

            // Check if the page actually has video content (lazyframe divs)
            // Some games have a media page but no videos — just soundtracks
            bool hasVideos = html.Contains("lazyframe", StringComparison.OrdinalIgnoreCase);

            // Store in mobygames_raw as a new chunk (even without videos, to avoid re-fetching)
            await _sourceDb.InsertRowAsync(game.MobyGameId, NewGameRawFetcher.ChunkMedia, html);

            if(hasVideos)
            {
                Console.WriteLine(" \e[32mOK\e[0m (has videos)");
                scraped++;
            }
            else
            {
                Console.WriteLine(" \e[33mOK\e[0m (no videos)");
                noMedia++;
            }
        }

        Console.WriteLine("\n  ────────────────────────────────────");
        Console.WriteLine($"    Games checked:      {gamesChecked}");
        Console.WriteLine($"    With videos:        {scraped}");
        Console.WriteLine($"    No videos:          {noMedia}");
        Console.WriteLine($"    Skipped (exists):   {skipped}");
        Console.WriteLine($"    Failed:             {failed}");
        Console.WriteLine("  ────────────────────────────────────\n");
    }
}
