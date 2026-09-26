using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Parsers;
using NewSite = Marechai.MobyGames.Parsers.NewSite;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Scrapes screenshot pages from the new MobyGames site and stores HTML in mobygames_raw.
///     Phase 1: resolves slug → numeric ID, fetches /screenshots/ page, stores in source DB.
/// </summary>
public class ScreenshotScraper
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly MobyGamesHttpClient               _httpClient;

    public ScreenshotScraper(
        IDbContextFactory<MarechaiContext> contextFactory,
        SourceDatabaseService             sourceDb,
        MobyGamesHttpClient               httpClient)
    {
        _contextFactory = contextFactory;
        _sourceDb       = sourceDb;
        _httpClient     = httpClient;
    }

    public async Task RunAsync(int batchSize, bool dryRun, GameScope scope = default)
    {
        Console.WriteLine(dryRun
                              ? "\n  \e[33;1m[DRY RUN]\e[0m Detecting games with screenshots...\n"
                              : "\n  Starting screenshot page scraping...\n");

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get the imported games this pass still has to visit
        var passState = new MediaPassStateService(_contextFactory);

        var importedGames = await ImportedGameSelector.SelectAsync(context, passState,
                                                                   MobyGamesMediaPass.ScreenshotPages, scope,
                                                                   batchSize);

        Console.WriteLine($"  Scope: {scope.Describe()}");
        Console.WriteLine($"  Games to process in this batch: {importedGames.Count}");

        int scraped      = 0;
        int skipped      = 0;
        int noScreenshots = 0;
        int failed       = 0;
        int gamesChecked = 0;
        var visited      = new List<string>();

        foreach(var game in importedGames)
        {
            gamesChecked++;

            // Check if this game has screenshots from the Main tab.
            // Supports both the legacy nav-tabs layout (active "Screenshots" tab)
            // and the new Vue-based layout (anchor to /game/{id}/{slug}/screenshots/).
            var rows = await _sourceDb.GetRowsForGameAsync(game.MobyGameId);

            bool hasScreenshots = false;
            string mainHtml      = null;
            MobyLayout mainLayout = MobyLayout.Unknown;

            foreach(var row in rows)
            {
                var (tab, layout) = TabDetector.DetectWithLayout(row.Body);

                if(tab != MobyTab.Main) continue;

                mainHtml  = row.Body;
                mainLayout = layout;

                if(layout == MobyLayout.New)
                {
                    hasScreenshots = NewSite.MediaPresenceDetector.HasScreenshots(row.Body);
                }
                else
                {
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(row.Body);

                    var tabLinks = doc.DocumentNode.SelectNodes("//ul[contains(@class,'nav-tabs')]//li/a");

                    if(tabLinks is not null)
                    {
                        foreach(var link in tabLinks)
                        {
                            string text = link.InnerText.Trim();
                            string href = link.GetAttributeValue("href", "#");

                            if(text == "Screenshots" && href != "#" && href.Contains("/screenshots"))
                            {
                                hasScreenshots = true;

                                break;
                            }
                        }
                    }
                }

                break;
            }

            if(!hasScreenshots)
            {
                noScreenshots++;
                visited.Add(game.MobyGameId);

                if(gamesChecked <= 5 || dryRun)
                    Console.WriteLine($"  [{gamesChecked}/{Math.Min(batchSize, importedGames.Count)}] {game.MobyGameId}: No screenshots tab");

                continue;
            }

            Console.WriteLine($"  [{gamesChecked}/{Math.Min(batchSize, importedGames.Count)}] \e[36;1m{game.MobyGameId}\e[0m: Has screenshots");

            // Check if we already scraped this game's screenshots page
            // The gtag content_type "game-list-screenshots" is always present on screenshot list pages,
            // even if the page has no actual screenshot platform sections
            bool alreadyScraped = rows.Any(r => r.Body.Contains("game-list-screenshots"));

            if(alreadyScraped)
            {
                Console.WriteLine("    → Already scraped, skipping");
                skipped++;
                visited.Add(game.MobyGameId);

                continue;
            }

            if(dryRun)
            {
                Console.WriteLine($"  [{gamesChecked}] {game.MobyGameId}: Has screenshots (would scrape)");
                scraped++;

                continue;
            }

            // Resolve numeric ID if not already known
            // Note: old MobyGames HTML uses slug-only URLs with no numeric ID,
            // so extraction from stored HTML only works for new-site pages
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
                // Game likely removed from MobyGames — skip silently
                failed++;

                continue;
            }

            // Fetch screenshots page from new MobyGames
            // URL must include slug: /game/{id}/{slug}/screenshots/ — without it, the path is misinterpreted
            string cleanSlug = game.MobyGameId.TrimStart('-');

            string screenshotsUrl =
                $"https://www.mobygames.com/game/{game.MobyNumericId}/{cleanSlug}/screenshots/";

            Console.Write($"  [{gamesChecked}] {game.MobyGameId}: Fetching screenshots page...");

            string html = await _httpClient.FetchPageAsync(screenshotsUrl);

            if(html is null)
            {
                Console.WriteLine(" \e[31mFAILED\e[0m");
                failed++;

                continue;
            }

            // Store in mobygames_raw as a new chunk
            await _sourceDb.InsertRowAsync(game.MobyGameId, NewGameRawFetcher.ChunkScreenshots, html);

            // The page is new, so download-screenshots has work to do on this game again.
            await passState.ClearVisitedAsync(MobyGamesMediaPass.Screenshots, game.MobyGameId);

            Console.WriteLine(" \e[32mOK\e[0m");
            scraped++;
            visited.Add(game.MobyGameId);
        }

        // Failed fetches are deliberately not marked, so they are retried on the next run.
        if(!dryRun) await passState.MarkVisitedAsync(MobyGamesMediaPass.ScreenshotPages, visited);

        Console.WriteLine("\n  ────────────────────────────────────");
        Console.WriteLine($"    Games checked:    {gamesChecked}");
        Console.WriteLine($"    Scraped:          {scraped}");
        Console.WriteLine($"    Skipped (exists): {skipped}");
        Console.WriteLine($"    No screenshots:   {noScreenshots}");
        Console.WriteLine($"    Failed:           {failed}");
        Console.WriteLine("  ────────────────────────────────────\n");
    }
}
