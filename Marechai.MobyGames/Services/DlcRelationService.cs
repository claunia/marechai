using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Parsers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class DlcRelationService
{
    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly MobyGamesHttpClient               _httpClient;
    readonly ImportService                     _importService;
    readonly SourceDatabaseService             _sourceDb;

    public DlcRelationService(IDbContextFactory<MarechaiContext> contextFactory, MobyGamesHttpClient httpClient,
                               ImportService importService, SourceDatabaseService sourceDb)
    {
        _contextFactory = contextFactory;
        _httpClient     = httpClient;
        _importService  = importService;
        _sourceDb       = sourceDb;
    }

    public async Task RunAsync(int batchSize, bool dryRun)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Find the "DLC / add-on" genre IDs
        // Note: old MobyGames HTML uses &nbsp; (U+00A0) around the slash, so match both variants
        List<int> dlcGenreIds = await context.SoftwareGenres
            .Where(g => g.Name.Contains("DLC") && g.Name.Contains("add-on") ||
                        g.Name == "Add-on")
            .Select(g => g.Id)
            .ToListAsync();

        if(dlcGenreIds.Count == 0)
        {
            Console.WriteLine("No 'DLC / add-on' or 'Add-on' genre found in database.");

            return;
        }

        // Find software entries that have the DLC genre but no BaseSoftwareId set
        List<Software> unlinkedDlcs = await context.Softwares
            .Where(s => s.BaseSoftwareId == null &&
                        context.GenresBySoftware.Any(g => g.SoftwareId == s.Id &&
                                                          dlcGenreIds.Contains(g.GenreId)))
            .OrderBy(s => s.Id)
            .Take(batchSize)
            .ToListAsync();

        Console.WriteLine($"Found {unlinkedDlcs.Count} unlinked DLC entries (batch={batchSize})");

        int linked = 0, skipped = 0, failed = 0;

        foreach(Software dlc in unlinkedDlcs)
        {
            // Update Kind to Dlc if not already
            if(dlc.Kind != SoftwareKind.Dlc)
            {
                dlc.Kind = SoftwareKind.Dlc;

                if(!dryRun)
                    await context.SaveChangesAsync();

                Console.Write($"  [{dlc.Id}] {dlc.Name} (Kind→Dlc)...");
            }
            else
            {
                Console.Write($"  [{dlc.Id}] {dlc.Name}...");
            }

            // Find the MobyGames import state for this software
            MobyGamesImportState importState = await context.MobyGamesImportStates
                .FirstOrDefaultAsync(s => s.SoftwareId == dlc.Id);

            if(importState is null)
            {
                Console.WriteLine(" No MobyGames import state, skipping.");
                skipped++;

                continue;
            }

            try
            {
                // Get numeric ID
                int? numericId = importState.MobyNumericId;

                if(numericId is null)
                {
                    numericId = await _httpClient.ResolveNumericGameIdAsync(importState.MobyGameId);

                    if(numericId is not null)
                    {
                        importState.MobyNumericId = numericId;
                        await context.SaveChangesAsync();
                    }
                }

                if(numericId is null)
                {
                    Console.WriteLine(" Could not resolve numeric ID, skipping.");
                    skipped++;

                    continue;
                }

                // Fetch new-site page to find base game
                string slug = importState.MobyGameId.TrimStart('-');
                string url  = $"https://www.mobygames.com/game/{numericId}/{slug}/";
                string html = await _httpClient.FetchPageAsync(url);

                (int? baseGameMobyId, string baseGameSlug) = NewSiteMainPageParser.ParseBaseGame(html);

                if(baseGameMobyId is null)
                {
                    Console.WriteLine(" No base game found on page.");
                    skipped++;

                    continue;
                }

                Console.Write($" base game MobyID={baseGameMobyId}, slug={baseGameSlug ?? "?"}...");

                // Try to find base game in our DB by slug in MobyGamesImportState
                ulong? baseSoftwareId = await FindBaseSoftwareIdAsync(context, baseGameSlug);

                // If not found by slug, try importing from mobygames_raw
                if(baseSoftwareId is null && baseGameSlug is not null)
                {
                    // Try slug variations in mobygames_raw: plain, with -, with platform/ prefix
                    string[] slugsToTry = [baseGameSlug, $"-{baseGameSlug}"];

                    foreach(string trySlug in slugsToTry)
                    {
                        var rows = await _sourceDb.GetRowsForGameAsync(trySlug);

                        if(rows.Count > 0)
                        {
                            Console.Write($" importing '{trySlug}'...");

                            if(!dryRun)
                            {
                                baseSoftwareId = await _importService.ImportGameBySlugAsync(trySlug);
                            }
                            else
                            {
                                Console.Write(" [dry-run: would import]");
                                baseSoftwareId = 0; // placeholder for dry-run
                            }

                            break;
                        }
                    }
                }

                if(baseSoftwareId is null)
                {
                    Console.WriteLine($" Base game not in mobygames_raw. Needs scraping first.");
                    skipped++;

                    continue;
                }

                if(dryRun)
                {
                    Console.WriteLine($" Would link to base game Software ID: {baseSoftwareId}");
                    linked++;

                    continue;
                }

                dlc.BaseSoftwareId = baseSoftwareId;
                await context.SaveChangesAsync();
                Console.WriteLine($" Linked to base game Software ID: {baseSoftwareId}");
                linked++;
            }
            catch(Exception ex)
            {
                Console.WriteLine($" Error: {ex.Message}");
                failed++;
            }
        }

        Console.WriteLine($"\nDone: {linked} linked, {skipped} skipped, {failed} failed");
    }

    /// <summary>
    ///     Tries to find the Software ID for a base game by looking up its slug
    ///     in MobyGamesImportState.MobyGameId (with and without leading dash).
    /// </summary>
    static async Task<ulong?> FindBaseSoftwareIdAsync(MarechaiContext context, string baseGameSlug)
    {
        if(string.IsNullOrWhiteSpace(baseGameSlug)) return null;

        // Try exact slug
        MobyGamesImportState state = await context.MobyGamesImportStates
            .FirstOrDefaultAsync(s => s.MobyGameId == baseGameSlug &&
                                      s.Status == MobyGamesImportStatus.Imported);

        if(state?.SoftwareId is not null) return state.SoftwareId;

        // Try with leading dash
        string dashed = $"-{baseGameSlug}";
        state = await context.MobyGamesImportStates
            .FirstOrDefaultAsync(s => s.MobyGameId == dashed &&
                                      s.Status == MobyGamesImportStatus.Imported);

        return state?.SoftwareId;
    }
}
