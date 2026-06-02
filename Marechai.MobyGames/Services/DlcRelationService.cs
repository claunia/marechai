using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Parsers;
using Marechai.MobyGames.Parsers.NewSite;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

public class DlcRelationService
{
    const int                               ChunkMain     = 0;
    const int                               ChunkCredits  = 1;
    const int                               ChunkReleases = 2;
    const int                               ChunkSpecs    = 3;
    const int                               ChunkCovers   = 4;
    const int                               ChunkReviews  = 5;
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

                    // Slug-based resolution fails for legacy rows where the slug was truncated
                    // to 64 chars in the old mobygames_raw schema, or where MobyGames editors
                    // have since renamed the title (the old slug now 404s while the numeric ID
                    // remains valid). Fall back to name-based search using the Software.Name.
                    if(numericId is null)
                    {
                        Console.Write(" slug failed, trying name search...");
                        numericId = await _httpClient.ResolveNumericGameIdByNameAsync(dlc.Name);
                    }

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

                // Fetch new-site page to find base game. Prefer the cached chunk-0 HTML in
                // mobygames_raw (avoids a redundant Cloudflare round-trip) and only fall back
                // to a live fetch when the cache is absent or the cached HTML doesn't contain
                // a parseable "Base Game" link (e.g. legacy-layout rows).
                string slug = importState.MobyGameId.TrimStart('-');

                // Refresh the DLC's own cached chunk-0 if it's still legacy layout. We always
                // want NewSiteMainPageParser.ParseBaseGame operating on current new-layout
                // HTML so the strict "<b>Base Game</b>" / "<b>Included in</b>" sidebar
                // detection is reliable; the legacy chunk shape doesn't expose that block in
                // a form the parser recognises, and stale 2019 captures often disagree with
                // the live page anyway.
                foreach(string refreshSlug in new[] { importState.MobyGameId, slug, $"-{slug}" }
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct(StringComparer.Ordinal))
                {
                    var existingRows = await _sourceDb.GetRowsForGameAsync(refreshSlug);
                    var existingMain = existingRows.FirstOrDefault(r => r.Chunk == 0);

                    if(existingMain is null) continue;

                    if(TabDetector.DetectWithLayout(existingMain.Body).Layout != MobyLayout.Old) continue;

                    Console.Write(" refreshing DLC cache to new layout...");

                    try
                    {
                        string url      = $"https://www.mobygames.com/game/{numericId}/{slug}/";
                        string liveBody = await _httpClient.FetchPageAsync(url);

                        if(!string.IsNullOrWhiteSpace(liveBody))
                        {
                            await _sourceDb.DeleteAllChunksAsync(refreshSlug);
                            await _sourceDb.InsertRowAsync(slug, 0, liveBody);
                        }
                        else
                        {
                            Console.Write(" (live fetch returned empty, keeping stale cache)");
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.Write($" (refresh failed: {ex.Message}, keeping stale cache)");
                    }

                    break;
                }

                int?   baseGameMobyId = null;
                string baseGameSlug   = null;
                bool   fromCache      = false;

                foreach(string trySlug in new[] { importState.MobyGameId, slug, $"-{slug}" }
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct(StringComparer.Ordinal))
                {
                    var cachedRows = await _sourceDb.GetRowsForGameAsync(trySlug);
                    var mainRow    = cachedRows.FirstOrDefault(r => r.Chunk == 0);

                    if(mainRow is null) continue;

                    (baseGameMobyId, baseGameSlug) = NewSiteMainPageParser.ParseBaseGame(mainRow.Body);

                    if(baseGameMobyId is not null)
                    {
                        fromCache = true;
                        break;
                    }
                }

                if(baseGameMobyId is null)
                {
                    string url  = $"https://www.mobygames.com/game/{numericId}/{slug}/";
                    string html = await _httpClient.FetchPageAsync(url);
                    (baseGameMobyId, baseGameSlug) = NewSiteMainPageParser.ParseBaseGame(html);
                }

                if(baseGameMobyId is null)
                {
                    Console.WriteLine(" No base game found on page.");
                    skipped++;

                    continue;
                }

                Console.Write($" base game MobyID={baseGameMobyId}, slug={baseGameSlug ?? "?"}" +
                              (fromCache ? " (cached)..." : " (live)..."));

                // Try to find the base game in the DB first, then fall back to raw rows,
                // and finally live-site scraping if the base game has never been stored.
                ulong? baseSoftwareId = await ResolveBaseSoftwareIdAsync(context, baseGameMobyId, baseGameSlug,
                                                                         dryRun);

                if(baseSoftwareId is null)
                {
                    Console.WriteLine(" Could not resolve base game.");
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
                Console.WriteLine($" Error: {ex.GetType().FullName}: {ex.Message}");

                // EF's DbUpdateException carries the entities that were in the failing batch.
                // Surface each entry's type, key, and string-valued properties (with lengths)
                // so we can identify which column exceeded its size limit without guessing.
                if(ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx && dbEx.Entries is { Count: > 0 })
                {
                    Console.WriteLine($"    Failing entities ({dbEx.Entries.Count}):");

                    foreach(var entry in dbEx.Entries)
                    {
                        Console.WriteLine($"      [{entry.State}] {entry.Entity.GetType().Name}");

                        foreach(var prop in entry.Properties)
                        {
                            object val = prop.CurrentValue;

                            if(val is string s)
                                Console.WriteLine($"        {prop.Metadata.Name} (len={s.Length}): {s}");
                            else if(val is not null)
                                Console.WriteLine($"        {prop.Metadata.Name}: {val}");
                        }
                    }
                }

                Exception inner = ex.InnerException;

                while(inner != null)
                {
                    Console.WriteLine($"    caused by {inner.GetType().FullName}: {inner.Message}");
                    inner = inner.InnerException;
                }

                if(ex.StackTrace != null) Console.WriteLine(ex.StackTrace);

                failed++;
            }
        }

        Console.WriteLine($"\nDone: {linked} linked, {skipped} skipped, {failed} failed");
    }

    async Task<ulong?> ResolveBaseSoftwareIdAsync(MarechaiContext context, int? baseGameMobyId,
                                                  string baseGameSlug, bool dryRun)
    {
        ulong? baseSoftwareId = await FindBaseSoftwareIdAsync(context, baseGameMobyId, baseGameSlug);

        if(baseSoftwareId is not null)
            return baseSoftwareId;

        if(string.IsNullOrWhiteSpace(baseGameSlug))
            return null;

        string trimmedSlug = baseGameSlug.TrimStart('-');

        foreach(string trySlug in new[] { trimmedSlug, $"-{trimmedSlug}" }
                    .Distinct(StringComparer.Ordinal))
        {
            var rows = await _sourceDb.GetRowsForGameAsync(trySlug);

            if(rows.Count == 0)
                continue;

            // Refresh stale legacy-layout caches before importing. 2019-era captures often
            // disagree with the live page (e.g. Assassin's Creed IV: Black Flag was
            // mis-tagged with Genre=Compilation back then; MobyGames editors corrected
            // it to Action since). Importing from the stale chunk routes the base game
            // through ImportCompilationAsync, which then tries to merge/delete an
            // existing Software row that has dependent SoftwareBySoftwareRelease links —
            // FK-protected, fails, and repeats for every sibling DLC. Re-scrape from
            // live once and reimport from the fresh new-layout chunks.
            var mainRow = rows.FirstOrDefault(r => r.Chunk == 0);

            if(!dryRun                                                          &&
               mainRow is not null                                              &&
               baseGameMobyId is not null                                       &&
               TabDetector.DetectWithLayout(mainRow.Body).Layout == MobyLayout.Old)
            {
                Console.Write($" refreshing stale legacy-layout cache for '{trySlug}'...");

                try
                {
                    string liveUrl  = $"https://www.mobygames.com/game/{baseGameMobyId}/{trimmedSlug}/";
                    string liveMain = await _httpClient.FetchPageAsync(liveUrl);

                    if(!string.IsNullOrWhiteSpace(liveMain))
                    {
                        await _sourceDb.DeleteAllChunksAsync(trySlug);
                        await ScrapeGameToRawAsync(trySlug, baseGameMobyId.Value, liveMain);
                    }
                    else
                    {
                        Console.Write(" (live fetch returned empty, falling through to stale cache)");
                    }
                }
                catch(Exception ex)
                {
                    Console.Write($" (refresh failed: {ex.Message}, falling through to stale cache)");
                }
            }

            Console.Write($" importing '{trySlug}'...");

            if(dryRun)
            {
                Console.Write(" [dry-run: would import]");

                return 0;
            }

            baseSoftwareId = await _importService.ImportGameBySlugAsync(trySlug);

            if(baseSoftwareId is not null)
                return baseSoftwareId;
        }

        if(dryRun)
        {
            Console.Write(" [dry-run: would scrape live site and import]");

            return 0;
        }

        int? numericId = baseGameMobyId;

        if(numericId is null)
            numericId = await _httpClient.ResolveNumericGameIdAsync(trimmedSlug);

        if(numericId is null)
            return null;

        string url  = $"https://www.mobygames.com/game/{numericId}/{trimmedSlug}/";
        string html = await _httpClient.FetchPageAsync(url);

        if(string.IsNullOrWhiteSpace(html))
            return null;

        Console.Write($" scraping live base game '{trimmedSlug}'...");

        await ScrapeGameToRawAsync(trimmedSlug, numericId.Value, html);

        baseSoftwareId = await _importService.ImportGameBySlugAsync(trimmedSlug);

        return baseSoftwareId;
    }

    async Task ScrapeGameToRawAsync(string slug, int numericId, string mainBody)
    {
        string baseUrl = $"https://www.mobygames.com/game/{numericId}/{slug}/";

        try
        {
            await _sourceDb.InsertRowAsync(slug, ChunkMain, mainBody);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"  Warning: db insert failed (main): {ex.GetType().FullName}: {ex.Message}");

            for(Exception inner = ex.InnerException; inner != null; inner = inner.InnerException)
                Console.WriteLine($"    caused by {inner.GetType().FullName}: {inner.Message}");
        }

        await TryFetchAndInsertAsync(slug, ChunkCredits,  baseUrl + "credits/");
        await TryFetchAndInsertAsync(slug, ChunkReleases, baseUrl + "releases/");
        await TryFetchAndInsertAsync(slug, ChunkSpecs,    baseUrl + "specs/");

        if(MediaPresenceDetector.HasCoverArt(mainBody))
            await TryFetchAndInsertAsync(slug, ChunkCovers, baseUrl + "covers/");

        if(MediaPresenceDetector.HasReviews(mainBody))
            await TryFetchAndInsertAsync(slug, ChunkReviews, baseUrl + "reviews/");
    }

    async Task TryFetchAndInsertAsync(string slug, int chunk, string url)
    {
        string body = await _httpClient.FetchPageAsync(url);

        if(string.IsNullOrWhiteSpace(body))
            return;

        try
        {
            await _sourceDb.InsertRowAsync(slug, chunk, body);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"  Warning: db insert failed (chunk {chunk}): {ex.GetType().FullName}: {ex.Message}");

            for(Exception inner = ex.InnerException; inner != null; inner = inner.InnerException)
                Console.WriteLine($"    caused by {inner.GetType().FullName}: {inner.Message}");
        }
    }

    /// <summary>
    ///     Tries to find the Software ID for a base game by looking up its numeric ID or slug
    ///     in MobyGamesImportState.
    /// </summary>
    static async Task<ulong?> FindBaseSoftwareIdAsync(MarechaiContext context, int? baseGameMobyId,
                                                      string baseGameSlug)
    {
        if(baseGameMobyId is not null)
        {
            MobyGamesImportState numericState = await context.MobyGamesImportStates
                .FirstOrDefaultAsync(s => s.MobyNumericId == baseGameMobyId &&
                                          s.Status == MobyGamesImportStatus.Imported);

            if(numericState?.SoftwareId is not null) return numericState.SoftwareId;
        }

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
