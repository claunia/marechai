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
///     Scrapes promo art pages from the new MobyGames site and stores HTML in mobygames_raw.
///     Phase 0: resolves slug → numeric ID, fetches /promo/ page, stores in source DB.
/// </summary>
public class PromoArtScraper
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService             _sourceDb;
    readonly MobyGamesHttpClient               _httpClient;

    public PromoArtScraper(
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
                              ? "\n  \e[33;1m[DRY RUN]\e[0m Detecting games with promo art...\n"
                              : "\n  Starting promo art page scraping...\n");

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Get imported games that have promo art (we detect from old Main tab HTML)
        var importedGames = await context.MobyGamesImportStates
                                         .Where(s => s.Status     == MobyGamesImportStatus.Imported &&
                                                     s.SoftwareId != null)
                                         .OrderBy(s => s.MobyGameId)
                                         .ToListAsync();

        Console.WriteLine($"  Found {importedGames.Count} imported games");

        int scraped       = 0;
        int skipped       = 0;
        int noPromo       = 0;
        int failed        = 0;
        int gamesChecked  = 0;

        foreach(var game in importedGames.Take(batchSize))
        {
            gamesChecked++;

            // Check if this game has promo art from the Main tab.
            // Supports both legacy nav-tabs and the new Vue layout (anchor to /game/{id}/{slug}/promo/).
            var rows = await _sourceDb.GetRowsForGameAsync(game.MobyGameId);

            bool hasPromoArt = false;

            foreach(var row in rows)
            {
                var (tab, layout) = TabDetector.DetectWithLayout(row.Body);

                if(tab != MobyTab.Main) continue;

                if(layout == MobyLayout.New)
                {
                    hasPromoArt = NewSite.MediaPresenceDetector.HasPromoArt(row.Body);
                }
                else
                {
                    // Active: <li><a href=".../.../promo">Promo Art</a></li>
                    // Disabled: <li class="disabled"><a href="#">Promo Art</a></li>
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(row.Body);

                    var promoLinks = doc.DocumentNode.SelectNodes("//ul[contains(@class,'nav-tabs')]//li/a");

                    if(promoLinks is not null)
                    {
                        foreach(var link in promoLinks)
                        {
                            string text = link.InnerText.Trim();
                            string href = link.GetAttributeValue("href", "#");

                            if(text == "Promo Art" && href != "#" && href.Contains("/promo"))
                            {
                                hasPromoArt = true;
                                break;
                            }
                        }
                    }
                }

                break;
            }

            if(!hasPromoArt)
            {
                noPromo++;

                if(gamesChecked <= 5 || dryRun)
                    Console.WriteLine($"  [{gamesChecked}/{Math.Min(batchSize, importedGames.Count)}] {game.MobyGameId}: No promo art tab");

                continue;
            }

            Console.WriteLine($"  [{gamesChecked}/{Math.Min(batchSize, importedGames.Count)}] \e[36;1m{game.MobyGameId}\e[0m: Has promo art");

            // Check if we already scraped this game's promo page
            // New site pages contain /promo/group- pattern (old site doesn't)
            bool alreadyScraped = rows.Any(r => r.Body.Contains("/promo/group-"));

            if(alreadyScraped)
            {
                Console.WriteLine($"    → Already scraped, skipping");
                skipped++;
                continue;
            }

            if(dryRun)
            {
                Console.WriteLine($"  [{gamesChecked}] {game.MobyGameId}: Has promo art (would scrape)");
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

            // Fetch promo page from new MobyGames
            // URL must include slug: /game/{id}/{slug}/promo/ — without it, the path is misinterpreted
            string cleanSlug = game.MobyGameId.TrimStart('-');

            string promoUrl = $"https://www.mobygames.com/game/{game.MobyNumericId}/{cleanSlug}/promo/";

            Console.Write($"  [{gamesChecked}] {game.MobyGameId}: Fetching promo page...");

            string html = await _httpClient.FetchPageAsync(promoUrl);

            if(html is null)
            {
                Console.WriteLine(" \e[31mFAILED\e[0m");
                failed++;
                continue;
            }

            // Store in mobygames_raw as a new chunk
            int maxChunk = rows.Count > 0 ? rows.Max(r => r.Chunk) : 0;

            await _sourceDb.InsertRowAsync(game.MobyGameId, maxChunk + 1, html);

            Console.WriteLine(" \e[32mOK\e[0m");
            scraped++;
        }

        Console.WriteLine("\n  ────────────────────────────────────");
        Console.WriteLine($"    Games checked:    {gamesChecked}");
        Console.WriteLine($"    Scraped:          {scraped}");
        Console.WriteLine($"    Skipped (exists): {skipped}");
        Console.WriteLine($"    No promo art:     {noPromo}");
        Console.WriteLine($"    Failed:           {failed}");
        Console.WriteLine("  ────────────────────────────────────\n");
    }
}
