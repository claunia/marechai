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

    /// <summary>
    ///     Page levels walked when deriving a compilation's base game: the contained item itself, plus
    ///     nested "This Compilation Includes" entries (compilations can contain compilations, e.g.
    ///     Season Pass → costume set → per-character costume). Bounded further by the fetch budget.
    /// </summary>
    const int MaxDerivationDepth = 4;

    /// <summary>Live page fetches allowed per compilation-base derivation, to bound Cloudflare round-trips.</summary>
    const int MaxDerivationFetches = 30;

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

    public async Task RunAsync(int batchSize, bool dryRun, bool recheck = false)
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

        // Find software entries that have the DLC genre but no BaseSoftwareId set. DLCs already
        // examined with a definitive "no base game" outcome are skipped unless --recheck, so
        // batches advance instead of re-picking the same unlinkable entries forever.
        List<Software> unlinkedDlcs = await context.Softwares
            .Where(s => s.BaseSoftwareId == null &&
                        context.GenresBySoftware.Any(g => g.SoftwareId == s.Id &&
                                                          dlcGenreIds.Contains(g.GenreId)) &&
                        (recheck || !context.MobyGamesImportStates.Any(st => st.SoftwareId == s.Id &&
                                                                             st.DlcRelationCheckedAt != null)))
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
                    // Cheapest source: mine the cached chunk for any /game/N/slug/ link.
                    // Post-2019 games have NO slug-redirect (live /game/<slug>/ returns
                    // "Error - MobyGames"), so HTTP resolution fails outright; but their
                    // cached HTML is full of internal sub-page anchors that carry the
                    // numeric ID. Try this BEFORE any HTTP round-trip.
                    numericId = TryExtractNumericIdFromCachedChunks(importState.MobyGameId);

                    if(numericId is not null)
                        Console.Write($" numericId={numericId} (from cached chunk)...");
                }

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
                }

                if(numericId is not null && importState.MobyNumericId != numericId)
                {
                    importState.MobyNumericId = numericId;
                    await context.SaveChangesAsync();
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

                // Only a successfully fetched game page makes "no base game" a definitive outcome;
                // an empty body or a Cloudflare challenge page must be retried on a later run.
                bool livePageParsed = false;

                if(baseGameMobyId is null)
                {
                    string url  = $"https://www.mobygames.com/game/{numericId}/{slug}/";
                    string html = await _httpClient.FetchPageAsync(url);
                    livePageParsed = IsGamePage(html, numericId.Value);
                    (baseGameMobyId, baseGameSlug) = NewSiteMainPageParser.ParseBaseGame(html);
                }

                if(baseGameMobyId is null)
                {
                    if(livePageParsed)
                    {
                        await StampCheckedAsync(context, [dlc.Id], dryRun);
                        Console.WriteLine(" No base game found on page.");
                    }
                    else
                    {
                        Console.WriteLine(" No base game found (page unavailable, will retry).");
                    }

                    skipped++;

                    continue;
                }

                Console.Write($" parent MobyID={baseGameMobyId}, slug={baseGameSlug ?? "?"}" +
                              (fromCache ? " (cached)..." : " (live)..."));

                // Try to find the parent in the DB first, then fall back to raw rows, and finally
                // live-site scraping if it has never been stored. The parent may turn out to be a
                // compilation (e.g. a Season Pass the DLC is "Included in"), which is not a base game.
                ParentResolution parent = await ResolveParentAsync(context, baseGameMobyId, baseGameSlug, dryRun);

                if(parent.CompilationId is not null)
                {
                    ulong? derivedBase = await LinkThroughCompilationAsync(context, parent.CompilationId.Value, dlc,
                                                                           dryRun);

                    if(derivedBase is null)
                        skipped++;
                    else
                        linked++;

                    continue;
                }

                ulong? baseSoftwareId = parent.SoftwareId;

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

    /// <summary>
    ///     What a DLC's parent link resolved to: a regular Software (a real base game) or a
    ///     compilation such as a Season Pass. A dry-run placeholder uses SoftwareId = 0.
    /// </summary>
    readonly record struct ParentResolution(ulong? SoftwareId, ulong? CompilationId)
    {
        public bool IsResolved => SoftwareId is not null || CompilationId is not null;
    }

    async Task<ParentResolution> ResolveParentAsync(MarechaiContext context, int? baseGameMobyId,
                                                    string baseGameSlug, bool dryRun)
    {
        ParentResolution found = await FindParentInDbAsync(context, baseGameMobyId, baseGameSlug);

        if(found.IsResolved)
            return found;

        if(string.IsNullOrWhiteSpace(baseGameSlug))
            return default;

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

                return new ParentResolution(0, null);
            }

            ParentResolution imported = await ImportParentBySlugAsync(context, trySlug);

            // A compilation import yields no SoftwareId; it must NOT fall through to the live
            // scrape below, which would import (and formerly duplicate) the compilation again.
            if(imported.IsResolved)
                return imported;
        }

        if(dryRun)
        {
            Console.Write(" [dry-run: would scrape live site and import]");

            return new ParentResolution(0, null);
        }

        int? numericId = baseGameMobyId;

        if(numericId is null)
            numericId = await _httpClient.ResolveNumericGameIdAsync(trimmedSlug);

        if(numericId is null)
            return default;

        string url  = $"https://www.mobygames.com/game/{numericId}/{trimmedSlug}/";
        string html = await _httpClient.FetchPageAsync(url);

        if(string.IsNullOrWhiteSpace(html))
            return default;

        Console.Write($" scraping live base game '{trimmedSlug}'...");

        // Evict any stale rows (e.g. cached 404 pages from 2019 captures of slugs that didn't
        // exist yet) under both slug variants before inserting fresh chunks. InsertRowAsync
        // uses INSERT IGNORE, so without the delete, stale rows would survive and the
        // subsequent ImportGameBySlugAsync would re-read the same garbage.
        await _sourceDb.DeleteAllChunksAsync(trimmedSlug);
        await _sourceDb.DeleteAllChunksAsync($"-{trimmedSlug}");

        await ScrapeGameToRawAsync(trimmedSlug, numericId.Value, html);

        return await ImportParentBySlugAsync(context, trimmedSlug);
    }

    /// <summary>
    ///     Imports a slug from <c>mobygames_raw</c> and reports whether it became a Software or a
    ///     compilation (compilation imports leave the state's SoftwareId null).
    /// </summary>
    async Task<ParentResolution> ImportParentBySlugAsync(MarechaiContext context, string slug)
    {
        ulong? softwareId = await _importService.ImportGameBySlugAsync(slug);

        if(softwareId is not null)
            return new ParentResolution(softwareId, null);

        ulong? compilationId = await ImportService.FindImportedCompilationIdAsync(context, slug);

        return new ParentResolution(null, compilationId);
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
    ///     Tries to find an already-imported parent by its MobyGames numeric ID or slug in
    ///     MobyGamesImportState. A state pointing at a Software wins over one pointing at a
    ///     compilation; a compilation is only returned if its row still exists.
    /// </summary>
    static async Task<ParentResolution> FindParentInDbAsync(MarechaiContext context, int? baseGameMobyId,
                                                            string baseGameSlug)
    {
        string[] slugs = string.IsNullOrWhiteSpace(baseGameSlug)
                             ? []
                             : new[] { baseGameSlug, baseGameSlug.TrimStart('-'), $"-{baseGameSlug.TrimStart('-')}" }
                              .Distinct(StringComparer.Ordinal)
                              .ToArray();

        var states = await context.MobyGamesImportStates
                                  .Where(s => s.Status == MobyGamesImportStatus.Imported &&
                                              (baseGameMobyId != null && s.MobyNumericId == baseGameMobyId ||
                                               slugs.Contains(s.MobyGameId)))
                                  .Select(s => new { s.SoftwareId, s.SoftwareCompilationId })
                                  .ToListAsync();

        ulong? softwareId = states.FirstOrDefault(s => s.SoftwareId is not null)?.SoftwareId;

        if(softwareId is not null)
            return new ParentResolution(softwareId, null);

        foreach(ulong compilationId in states.Where(s => s.SoftwareCompilationId is not null)
                                             .Select(s => s.SoftwareCompilationId.Value)
                                             .Distinct())
        {
            if(await context.SoftwareCompilations.AnyAsync(c => c.Id == compilationId))
                return new ParentResolution(null, compilationId);
        }

        return default;
    }

    /// <summary>
    ///     Handles a DLC whose parent is a compilation (typically a Season Pass). The compilation's
    ///     base game is taken from <see cref="SoftwareCompilation.BaseSoftwareId" /> or derived from
    ///     its contained DLCs; when found it is stored on the compilation and inherited by every
    ///     unlinked DLC the compilation contains. When no base can be derived, those DLCs are stamped
    ///     so later runs skip them.
    /// </summary>
    /// <returns>The base game's Software ID, or <c>null</c> if none could be determined.</returns>
    async Task<ulong?> LinkThroughCompilationAsync(MarechaiContext context, ulong compilationId, Software dlc,
                                                   bool dryRun)
    {
        SoftwareCompilation compilation = await context.SoftwareCompilations.FirstAsync(c => c.Id == compilationId);

        Console.Write($" included in compilation #{compilationId} ({compilation.Name})...");

        // The DLC page says it is included in this compilation; make sure the junction reflects
        // that even if the compilation was imported before the DLC existed locally.
        bool dlcIsMember = await context.SoftwareBySoftwareCompilation
                                        .AnyAsync(j => j.SoftwareCompilationId == compilationId &&
                                                       j.SoftwareId            == dlc.Id);

        if(!dlcIsMember && !dryRun)
        {
            context.SoftwareBySoftwareCompilation.Add(new SoftwareBySoftwareCompilation
            {
                SoftwareCompilationId = compilationId,
                SoftwareId            = dlc.Id
            });

            await context.SaveChangesAsync();
        }

        // A compilation can nest other compilations (Season Pass → costume-set compilation → costume
        // DLCs), so gather the whole tree: every descendant compilation and every software in any of them.
        (List<ulong> treeCompilationIds, List<ulong> containedIds) = await CollectCompilationTreeAsync(context, compilationId);

        if(!containedIds.Contains(dlc.Id)) containedIds.Add(dlc.Id);

        ulong? baseId     = compilation.BaseSoftwareId;
        bool   definitive = true;

        if(baseId is null)
            baseId = await FindAncestorBaseSoftwareIdAsync(context, compilationId);

        if(baseId is null)
            (baseId, definitive) = await DeriveCompilationBaseAsync(context, treeCompilationIds, containedIds, dryRun);

        List<Software> unlinkedMembers = await context.Softwares
                                                      .Where(s => containedIds.Contains(s.Id) &&
                                                                  s.Kind           == SoftwareKind.Dlc &&
                                                                  s.BaseSoftwareId == null)
                                                      .ToListAsync();

        if(baseId is null)
        {
            if(definitive)
            {
                await StampCheckedAsync(context, unlinkedMembers.Select(s => s.Id).Append(dlc.Id), dryRun);
                Console.WriteLine($" base game unknown; marked {unlinkedMembers.Count} DLC(s) as checked.");
            }
            else
            {
                Console.WriteLine(" base game unknown (lookup budget exhausted, will retry).");
            }

            return null;
        }

        // A bundle that contains the base game itself (e.g. a GOTY edition) is not add-on
        // content for it, so only record a compilation's base when the base is not in its tree.
        bool baseIsMember = containedIds.Contains(baseId.Value);

        List<SoftwareCompilation> unlinkedCompilations = baseIsMember
                                                             ? []
                                                             : await context.SoftwareCompilations
                                                                            .Where(c => treeCompilationIds.Contains(c.Id) &&
                                                                                        c.BaseSoftwareId == null)
                                                                            .ToListAsync();

        if(dryRun)
        {
            Console.WriteLine($" Would set base game Software ID: {baseId} on {unlinkedCompilations.Count} " +
                              $"compilation(s) and link {unlinkedMembers.Count} DLC(s)");

            return baseId;
        }

        foreach(SoftwareCompilation nested in unlinkedCompilations)
            nested.BaseSoftwareId = baseId;

        int linkedCount = 0;

        foreach(Software member in unlinkedMembers.Where(m => m.Id != baseId))
        {
            member.BaseSoftwareId = baseId;
            linkedCount++;
        }

        if(dlc.BaseSoftwareId is null && dlc.Id != baseId)
            dlc.BaseSoftwareId = baseId;

        await context.SaveChangesAsync();

        Console.WriteLine($" base game Software ID: {baseId}; set on {unlinkedCompilations.Count} compilation(s), " +
                          $"linked {linkedCount} DLC(s)");

        return baseId;
    }

    /// <summary>
    ///     Returns the base game of the nearest enclosing compilation that has one (e.g. a costume-set
    ///     compilation inside a Season Pass whose base game is already known), walking parents cycle-safely.
    /// </summary>
    static async Task<ulong?> FindAncestorBaseSoftwareIdAsync(MarechaiContext context, ulong compilationId)
    {
        var visited  = new HashSet<ulong> { compilationId };
        var frontier = new List<ulong> { compilationId };

        while(frontier.Count > 0)
        {
            List<ulong> parents = await context.SoftwareCompilationBySoftwareCompilation
                                               .Where(j => frontier.Contains(j.ChildCompilationId))
                                               .Select(j => j.ParentCompilationId)
                                               .ToListAsync();

            frontier = parents.Where(visited.Add).ToList();

            if(frontier.Count == 0) break;

            ulong? baseId = await context.SoftwareCompilations
                                         .Where(c => frontier.Contains(c.Id) && c.BaseSoftwareId != null)
                                         .Select(c => c.BaseSoftwareId)
                                         .FirstOrDefaultAsync();

            if(baseId is not null)
            {
                Console.Write(" inherited from an enclosing compilation...");

                return baseId;
            }
        }

        return null;
    }

    /// <summary>
    ///     Walks <see cref="SoftwareCompilationBySoftwareCompilation" /> from <paramref name="rootId" /> down
    ///     through every nested compilation (cycle-safe) and returns all compilation IDs in the tree,
    ///     including the root, plus every Software contained by any of them.
    /// </summary>
    static async Task<(List<ulong> CompilationIds, List<ulong> SoftwareIds)> CollectCompilationTreeAsync(
        MarechaiContext context, ulong rootId)
    {
        var compilationIds = new HashSet<ulong> { rootId };
        var frontier       = new List<ulong> { rootId };

        while(frontier.Count > 0)
        {
            List<ulong> children = await context.SoftwareCompilationBySoftwareCompilation
                                                .Where(j => frontier.Contains(j.ParentCompilationId))
                                                .Select(j => j.ChildCompilationId)
                                                .ToListAsync();

            frontier = children.Where(compilationIds.Add).ToList();
        }

        List<ulong> ids = compilationIds.ToList();

        List<ulong> softwareIds = await context.SoftwareBySoftwareCompilation
                                               .Where(j => ids.Contains(j.SoftwareCompilationId))
                                               .Select(j => j.SoftwareId)
                                               .Distinct()
                                               .ToListAsync();

        return (ids, softwareIds);
    }

    /// <summary>
    ///     Derives the base game of a compilation tree: first by majority vote over nested compilations
    ///     and contained DLCs that already have a base game, then by reading the MobyGames pages of
    ///     contained DLCs and nested compilations for a strict parent link, descending through
    ///     "This Compilation Includes" for bundles like costume sets.
    /// </summary>
    /// <returns>
    ///     The base Software ID (0 as a dry-run placeholder when it would have to be imported), and
    ///     whether a <c>null</c> result is definitive (false when the live-fetch budget ran out).
    /// </returns>
    async Task<(ulong? BaseId, bool Definitive)> DeriveCompilationBaseAsync(MarechaiContext context,
        List<ulong> compilationIds, List<ulong> containedIds, bool dryRun)
    {
        List<ulong> softwareVotes = await context.Softwares
                                                 .Where(s => containedIds.Contains(s.Id) && s.BaseSoftwareId != null)
                                                 .Select(s => s.BaseSoftwareId.Value)
                                                 .ToListAsync();

        List<ulong> compilationVotes = await context.SoftwareCompilations
                                                    .Where(c => compilationIds.Contains(c.Id) && c.BaseSoftwareId != null)
                                                    .Select(c => c.BaseSoftwareId.Value)
                                                    .ToListAsync();

        var vote = softwareVotes.Concat(compilationVotes)
                                .GroupBy(id => id)
                                .Select(g => new { BaseId = g.Key, Count = g.Count() })
                                .OrderByDescending(g => g.Count)
                                .FirstOrDefault();

        if(vote is not null)
        {
            Console.Write($" derived from {vote.Count} existing link(s) in the compilation tree...");

            return (vote.BaseId, true);
        }

        // Page walk: contained software first (their pages carry "Base Game"), then nested
        // compilations (their pages list more members under "This Compilation Includes").
        var states = await context.MobyGamesImportStates
                                  .Where(s => s.SoftwareId != null && containedIds.Contains(s.SoftwareId.Value) ||
                                              s.SoftwareCompilationId != null &&
                                              compilationIds.Contains(s.SoftwareCompilationId.Value))
                                  .Select(s => new { s.MobyGameId, s.MobyNumericId, IsCompilation = s.SoftwareId == null })
                                  .ToListAsync();

        var walk = new DerivationWalk();

        foreach(var state in states.OrderBy(s => s.IsCompilation))
        {
            int? numericId = state.MobyNumericId ?? TryExtractNumericIdFromCachedChunks(state.MobyGameId);

            (int Id, string Slug)? parent = await FindStrictParentAsync(state.MobyGameId, numericId, 0, walk);

            if(parent is null) continue;

            Console.Write($" derived parent MobyID={parent.Value.Id} from contained pages...");

            ParentResolution resolved = await ResolveParentAsync(context, parent.Value.Id, parent.Value.Slug, dryRun);

            if(resolved.SoftwareId is not null)
                return (resolved.SoftwareId, true);
        }

        return (null, !walk.BudgetExhausted);
    }

    sealed class DerivationWalk
    {
        public readonly HashSet<string> Visited = new(StringComparer.Ordinal);
        public          bool            BudgetExhausted;
        public          int             FetchesLeft = MaxDerivationFetches;
    }

    /// <summary>
    ///     Reads a game's main page (cached chunk 0, or live when the cache is missing or legacy
    ///     layout) and returns its strict parent link — anything but "Included in". If there is none,
    ///     descends into the page's "This Compilation Includes" entries up to
    ///     <see cref="MaxDerivationDepth" />. Live pages are not written to the cache.
    /// </summary>
    async Task<(int Id, string Slug)?> FindStrictParentAsync(string slug, int? numericId, int depth,
                                                             DerivationWalk walk)
    {
        if(string.IsNullOrWhiteSpace(slug)) return null;

        string trimmed = slug.TrimStart('-');

        if(!walk.Visited.Add(trimmed)) return null;

        string body = null;

        foreach(string trySlug in new[] { slug, trimmed, $"-{trimmed}" }.Distinct(StringComparer.Ordinal))
        {
            var rows = await _sourceDb.GetRowsForGameAsync(trySlug);
            body = rows.FirstOrDefault(r => r.Chunk == 0)?.Body;

            if(body is not null) break;
        }

        (int? id, string parentSlug, bool strict) =
            body is null ? (null, null, false) : NewSiteMainPageParser.ParseParentGame(body);

        bool stale = body is null || TabDetector.DetectWithLayout(body).Layout == MobyLayout.Old;

        if(!(id is not null && strict) && stale && numericId is not null)
        {
            if(walk.FetchesLeft <= 0)
            {
                walk.BudgetExhausted = true;

                return null;
            }

            walk.FetchesLeft--;

            string html = await _httpClient.FetchPageAsync($"https://www.mobygames.com/game/{numericId}/{trimmed}/");

            if(IsGamePage(html, numericId.Value))
            {
                body                     = html;
                (id, parentSlug, strict) = NewSiteMainPageParser.ParseParentGame(body);
            }
        }

        if(id is not null && strict) return (id.Value, parentSlug);

        if(body is null || depth >= MaxDerivationDepth - 1) return null;

        foreach((int childId, string childSlug) in NewSiteMainPageParser.ParseCompilationIncludes(body))
        {
            (int Id, string Slug)? found = await FindStrictParentAsync(childSlug, childId, depth + 1, walk);

            if(found is not null) return found;
        }

        return null;
    }

    /// <summary>True when <paramref name="html" /> is a real game page for the given ID (not empty, not a challenge page).</summary>
    static bool IsGamePage(string html, int numericId) =>
        !string.IsNullOrWhiteSpace(html) && html.Contains($"/game/{numericId}/", StringComparison.Ordinal);

    /// <summary>Records a definitive "no base game" outcome on the import states of the given DLCs.</summary>
    static async Task StampCheckedAsync(MarechaiContext context, IEnumerable<ulong> softwareIds, bool dryRun)
    {
        if(dryRun) return;

        List<ulong> ids = softwareIds.Distinct().ToList();
        DateTime    now = DateTime.UtcNow;

        await context.MobyGamesImportStates
                     .Where(s => s.SoftwareId != null && ids.Contains(s.SoftwareId.Value))
                     .ExecuteUpdateAsync(u => u.SetProperty(s => s.DlcRelationCheckedAt, now));
    }

    /// <summary>
    ///     Mines the cached <c>mobygames_raw</c> chunks for the slug to extract its own
    ///     MobyGames numeric ID. Post-2019 game pages have NO working slug-only redirect
    ///     URL (<c>/game/&lt;slug&gt;/</c> returns "Error - MobyGames"), so HTTP-based
    ///     resolution fails outright; but every internal sub-page anchor on the cached
    ///     page carries <c>/game/{N}/{slug}/...</c>. The most frequent (id, slug) pair
    ///     matching the requested slug wins, ignoring stray cross-references to other
    ///     games.
    /// </summary>
    int? TryExtractNumericIdFromCachedChunks(string mobyGameId)
    {
        if(string.IsNullOrWhiteSpace(mobyGameId)) return null;

        string trimmedSlug = mobyGameId.TrimStart('-');

        foreach(string trySlug in new[] { mobyGameId, trimmedSlug, $"-{trimmedSlug}" }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.Ordinal))
        {
            // Sync over the existing async helper because we're called from a sync helper
            // chain; the per-DLC cost is one DB call and we already pay it elsewhere in the
            // loop.
            var rows = _sourceDb.GetRowsForGameAsync(trySlug).GetAwaiter().GetResult();

            if(rows.Count == 0) continue;

            var counts = new Dictionary<int, int>();

            foreach(var row in rows)
            {
                if(string.IsNullOrWhiteSpace(row.Body)) continue;

                foreach(System.Text.RegularExpressions.Match m in
                    System.Text.RegularExpressions.Regex.Matches(
                        row.Body,
                        $@"/game/(\d+)/{System.Text.RegularExpressions.Regex.Escape(trimmedSlug)}/",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    if(int.TryParse(m.Groups[1].Value, out int id))
                        counts[id] = counts.GetValueOrDefault(id) + 1;
                }
            }

            if(counts.Count == 0) continue;

            return counts.OrderByDescending(kv => kv.Value).First().Key;
        }

        return null;
    }
}
