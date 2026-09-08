/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.MobyGames.Models;
using Marechai.MobyGames.Parsers.NewSite;
using Microsoft.EntityFrameworkCore;

namespace Marechai.MobyGames.Services;

/// <summary>
///     <c>update-year</c> orchestrator. For every ALREADY-IMPORTED game that MobyGames files under a
///     given year (<see cref="MobyGamesDiscoveredGame.ReleaseYear" />), re-downloads all of the
///     game's pages from the live site, replaces the cached chunks in <c>mobygames_raw</c>, and
///     adds only what is new: the English description (if the software has none), releases (via
///     <see cref="ImportService.ApplyRefreshAsync" />, deduplicated), covers, screenshots, promo art,
///     videos and critic reviews (via the per-game entry points of the existing media services,
///     which dedupe by MobyGames detail URL / natural keys).
///     <para>
///         It NEVER creates a <see cref="Software" />: games discovered for the year but not yet
///         imported are counted and left alone for the normal <c>scrape-new-games</c> /
///         <c>import</c> pipeline. Games whose state row has no <c>SoftwareId</c> (compilations) are
///         skipped as well. A successful pass stamps
///         <see cref="MobyGamesImportState.LastRefreshedAt" /> so re-running the command resumes
///         with the next batch; <c>--force</c> / <c>--since</c> re-select stamped rows.
///     </para>
///     <para>
///         All page fetches for a game happen BEFORE anything is written, and the cached chunks are
///         only replaced after the main page came back, so a 404 / rate-limit never destroys the
///         existing cache. In dry-run mode the live pages are fetched and diffed but nothing is
///         written to either database or to disk.
///     </para>
/// </summary>
public class YearRefreshService
{
    const int MaxConsecutiveMainFailures = 3;

    readonly IDbContextFactory<MarechaiContext> _contextFactory;
    readonly SourceDatabaseService              _sourceDb;
    readonly MobyGamesHttpClient                _http;
    readonly ImportService                      _importService;
    readonly CoverDownloadService               _covers;
    readonly ScreenshotDownloadService          _screenshots;
    readonly PromoArtDownloadService            _promoArt;
    readonly VideoImportService                 _videos;
    readonly ReviewImportService                _reviews;
    readonly CoverStateService                  _coverState;
    readonly ScreenshotStateService             _screenshotState;
    readonly PromoArtStateService               _promoState;
    readonly VideoStateService                  _videoState;

    public YearRefreshService(IDbContextFactory<MarechaiContext> contextFactory,
                              SourceDatabaseService              sourceDb,
                              MobyGamesHttpClient                http,
                              ImportService                      importService,
                              CoverDownloadService               covers,
                              ScreenshotDownloadService          screenshots,
                              PromoArtDownloadService            promoArt,
                              VideoImportService                 videos,
                              ReviewImportService                reviews,
                              CoverStateService                  coverState,
                              ScreenshotStateService             screenshotState,
                              PromoArtStateService               promoState,
                              VideoStateService                  videoState)
    {
        _contextFactory  = contextFactory;
        _sourceDb        = sourceDb;
        _http            = http;
        _importService   = importService;
        _covers          = covers;
        _screenshots     = screenshots;
        _promoArt        = promoArt;
        _videos          = videos;
        _reviews         = reviews;
        _coverState      = coverState;
        _screenshotState = screenshotState;
        _promoState      = promoState;
        _videoState      = videoState;
    }

    sealed class Candidate
    {
        public MobyGamesImportState State     { get; init; }
        public int                  NumericId { get; init; }
        public string               Slug      { get; init; }
    }

    /// <param name="year">MobyGames filing year to select games by. Ignored when <paramref name="onlySlug" /> is set.</param>
    /// <param name="onlySlug">
    ///     Refresh exactly this imported slug (either variant) regardless of year / stamp. The numeric
    ///     id comes from the state row, the cached main page, or a live slug resolution.
    /// </param>
    /// <param name="fromCache">
    ///     Skip the live download and run the same diff against the chunks already cached in
    ///     <c>mobygames_raw</c>. No network access, cache left untouched. Useful to re-apply the
    ///     deduplicated import after a parser fix, and as an idempotency check (a second pass must
    ///     add nothing).
    /// </param>
    public async Task RunAsync(int year, int batchSize, bool dryRun, bool downloadOnly, bool force,
                               DateTime? since, string onlySlug = null, bool fromCache = false)
    {
        string what = onlySlug is null ? $"imported games of {year}" : $"imported game '{onlySlug}'";

        if(fromCache) what += " from cached HTML (no network)";

        Console.WriteLine(dryRun
                              ? $"\n  \e[33;1m[DRY RUN]\e[0m Refreshing {what} (nothing will be written)...\n"
                              : $"\n  Refreshing {what}...\n");

        if(!fromCache && _http is null)
            throw new InvalidOperationException("A MobyGamesHttpClient is required unless --from-cache is used.");

        // ---- Selection -------------------------------------------------------------------
        List<Candidate> candidates;
        int noState = 0, compilationOnly = 0, alreadyRefreshed = 0, noNumericId = 0, notImported = 0;

        if(onlySlug is not null)
        {
            Candidate single = await SelectSingleAsync(onlySlug);

            if(single is null) return;

            candidates = [single];
            batchSize  = 1;
        }
        else
        await using(var context = await _contextFactory.CreateDbContextAsync())
        {
            var discovered = await context.MobyGamesDiscoveredGames
                                          .Where(d => d.ReleaseYear == year)
                                          .OrderBy(d => d.Slug)
                                          .Select(d => new { d.Slug, d.NumericId })
                                          .ToListAsync();

            Console.WriteLine($"  Discovered games filed under {year}: {discovered.Count}");

            var states = await context.MobyGamesImportStates
                                      .Where(s => s.Status == MobyGamesImportStatus.Imported)
                                      .ToListAsync();

            // Index by trimmed slug: state rows may be stored as "-slug" (legacy dump) while the
            // discovery table always holds "slug".
            var byTrimmed = new Dictionary<string, MobyGamesImportState>(StringComparer.Ordinal);

            foreach(MobyGamesImportState s in states)
                byTrimmed.TryAdd(s.MobyGameId.TrimStart('-'), s);

            var softwareIds = new HashSet<ulong>(await context.Softwares.Select(sw => sw.Id).ToListAsync());

            candidates = [];

            foreach(var d in discovered)
            {
                string trimmed = d.Slug.TrimStart('-');

                if(!byTrimmed.TryGetValue(trimmed, out MobyGamesImportState state))
                {
                    noState++;

                    continue;
                }

                if(state.SoftwareId is null)
                {
                    compilationOnly++;

                    continue;
                }

                if(!softwareIds.Contains(state.SoftwareId.Value))
                {
                    notImported++;

                    continue;
                }

                if(d.NumericId <= 0 && !fromCache)
                {
                    noNumericId++;

                    continue;
                }

                bool isStamped = state.LastRefreshedAt is not null &&
                                 !(since is not null && state.LastRefreshedAt < since);

                if(isStamped && !force)
                {
                    alreadyRefreshed++;

                    continue;
                }

                candidates.Add(new Candidate { State = state, NumericId = d.NumericId, Slug = trimmed });
            }
        }

        if(onlySlug is null)
        {
            Console.WriteLine($"  Eligible (imported, not yet refreshed): {candidates.Count}");
            Console.WriteLine($"    skipped: never imported={noState}, compilation-only={compilationOnly}, " +
                              $"software missing={notImported}, no numeric id={noNumericId}, " +
                              $"already refreshed={alreadyRefreshed}");
        }

        List<Candidate> batch = candidates.Take(batchSize).ToList();

        if(batch.Count == 0)
        {
            Console.WriteLine("\n  \e[32mNothing to refresh.\e[0m");

            return;
        }

        Console.WriteLine($"  Processing batch of {batch.Count} game(s)\n");

        // ---- Shared dedupe sets (loaded once; read-only, so also in dry-run) -------------
        HashSet<string> coverUrls      = await _coverState.GetProcessedCoverUrlsAsync();
        HashSet<string> screenshotUrls = await _screenshotState.GetProcessedScreenshotUrlsAsync();
        HashSet<string> promoUrls      = await _promoState.GetProcessedPromoUrlsAsync();
        HashSet<string> videoUrls      = await _videoState.GetProcessedVideoUrlsAsync();

        var coverCounters      = new MediaCounters();
        var screenshotCounters = new MediaCounters();
        var promoCounters      = new MediaCounters();
        var videoCounters      = new MediaCounters();

        int gamesDone = 0, fetchFailed = 0, failed = 0, stamped = 0;
        int descriptionsAdded = 0, releasesCreated = 0, releasesSkipped = 0, reviewsImported = 0;
        int consecutiveMainFailures = 0;
        bool aborted = false;

        // ---- Per game --------------------------------------------------------------------
        for(int i = 0; i < batch.Count; i++)
        {
            if(aborted) break;

            Candidate            cand  = batch[i];
            MobyGamesImportState state = cand.State;
            string               progress = $"[{i + 1}/{batch.Count}]";

            Console.WriteLine($"  {progress} \e[36;1m{cand.Slug}\e[0m (Software {state.SoftwareId}, MobyGames #{cand.NumericId})");

            try
            {
                // 1. Fetch everything into memory first. Main page failure aborts this game.
                Dictionary<int, string> chunks;

                if(fromCache)
                {
                    chunks = await LoadCachedChunksAsync(state.MobyGameId, cand.Slug);

                    if(chunks is null)
                    {
                        fetchFailed++;
                        Console.WriteLine("    \e[31m✗\e[0m no cached main page under either slug variant");

                        continue;
                    }

                    Console.WriteLine($"    cached chunks: {string.Join(", ", chunks.Keys.OrderBy(k => k))}");
                }
                else
                {
                    chunks = await FetchAllChunksAsync(cand.Slug, cand.NumericId);

                    if(chunks is null)
                    {
                        fetchFailed++;
                        consecutiveMainFailures++;
                        Console.WriteLine("    \e[31m✗\e[0m main page fetch failed (removed / renamed / rate-limited?) — cache untouched");

                        // The HTTP client already slept through its whole challenge backoff schedule
                        // and Cloudflare is STILL challenging this IP: nothing in this batch can
                        // succeed right now. Stop here; unstamped games are retried next run.
                        if(_http.LastRequestChallenged)
                        {
                            Console.WriteLine("\n  \e[31;1mABORTING: Cloudflare keeps challenging this IP after the full backoff — " +
                                              "let the traffic score decay and re-run later (or re-run cf-login).\e[0m");
                            aborted = true;

                            continue;
                        }

                        if(consecutiveMainFailures >= MaxConsecutiveMainFailures)
                        {
                            Console.WriteLine($"\n  \e[31;1mABORTING: {MaxConsecutiveMainFailures} consecutive main-page failures — " +
                                              "MobyGames is probably rate-limiting us. Re-run later.\e[0m");
                            aborted = true;
                        }

                        continue;
                    }

                    consecutiveMainFailures = 0;

                    Console.WriteLine($"    fetched chunks: {string.Join(", ", chunks.Keys.OrderBy(k => k))}");
                }

                // Rows are keyed under the STATE's MobyGameId (possibly the dashed legacy variant)
                // because every downstream service reads the cache by that key.
                List<MobyGamesRawRow> rows = chunks.OrderBy(kv => kv.Key)
                                                   .Select(kv => new MobyGamesRawRow
                                                    {
                                                        Id    = state.MobyGameId,
                                                        Chunk = kv.Key,
                                                        Body  = kv.Value
                                                    })
                                                   .ToList();

                // 2. Replace the cache (both slug variants) — only after a successful main fetch.
                if(!dryRun && !fromCache)
                {
                    var oldChunks = new SortedSet<int>(await _sourceDb.GetChunkNumbersAsync(cand.Slug));
                    oldChunks.UnionWith(await _sourceDb.GetChunkNumbersAsync($"-{cand.Slug}"));

                    await _sourceDb.DeleteAllChunksAsync(cand.Slug);
                    await _sourceDb.DeleteAllChunksAsync($"-{cand.Slug}");

                    foreach(MobyGamesRawRow row in rows)
                        await _sourceDb.InsertRowAsync(row.Id, row.Chunk, row.Body);

                    var newChunks = new SortedSet<int>(rows.Select(r => r.Chunk));
                    var dropped   = oldChunks.Except(newChunks).ToList();
                    var added     = newChunks.Except(oldChunks).ToList();

                    // Legacy dynamic slots (6-9, 13+) are expected to go away; a missing fixed slot
                    // (e.g. 12 = media) means the live page no longer advertises that sub-page.
                    Console.WriteLine($"    cache: chunks now [{string.Join(",", newChunks)}]" +
                                      (added.Count   > 0 ? $", added [{string.Join(",", added)}]"     : "") +
                                      (dropped.Count > 0 ? $", dropped [{string.Join(",", dropped)}]" : ""));
                }

                // 3. Description + releases (deduplicated; never touches ImportGameAsync).
                ParsedGame game = GameAssembler.Assemble(state.MobyGameId, rows);

                if(string.IsNullOrWhiteSpace(game.Name))
                    Console.WriteLine("    \e[33m!\e[0m could not parse a game name from the main page — continuing with sub-pages only");

                ImportService.RefreshResult r = await _importService.ApplyRefreshAsync(state.SoftwareId!.Value, game, dryRun);

                Console.WriteLine($"    description: {(r.DescriptionAdded ? (dryRun ? "would add" : "added") : r.DescriptionExisted ? "exists" : "none on MobyGames")}");
                Console.WriteLine($"    releases: {r.ReleasesOnTab} on tab, {(dryRun ? "would create" : "created")} {r.ReleasesCreated}, " +
                                  $"existing {r.SkippedStrong + r.SkippedStrict + r.SkippedLoose} " +
                                  $"(barcode/code {r.SkippedStrong}, exact {r.SkippedStrict}, loose {r.SkippedLoose})" +
                                  (r.SkippedNoPublisher > 0 ? $", no publisher {r.SkippedNoPublisher}" : ""));

                if(r.DescriptionAdded) descriptionsAdded++;

                releasesCreated += r.ReleasesCreated;
                releasesSkipped += r.SkippedStrong + r.SkippedStrict + r.SkippedLoose;

                // 4. Media. Each service dedupes per item; a false return means disk space ran out.
                if(!await _covers.ProcessGameAsync(state, rows, coverUrls, dryRun, downloadOnly, coverCounters, progress) ||
                   !await _screenshots.ProcessGameAsync(state, rows, screenshotUrls, dryRun, downloadOnly, screenshotCounters, progress) ||
                   !await _promoArt.ProcessGameAsync(state, rows, promoUrls, dryRun, downloadOnly, promoCounters, progress))
                {
                    aborted = true;
                    failed++;

                    break;
                }

                await _videos.ProcessGameAsync(state, rows, videoUrls, dryRun, videoCounters, progress);

                Console.Write("    reviews:");
                int imported = await _reviews.ImportForGameAsync(state.MobyGameId, state.SoftwareId.Value, rows, dryRun);

                if(imported > 0) reviewsImported += imported;

                // 5. Stamp.
                if(!dryRun)
                {
                    await using var stampContext = await _contextFactory.CreateDbContextAsync();

                    MobyGamesImportState fresh = await stampContext.MobyGamesImportStates
                                                                   .FirstOrDefaultAsync(s => s.Id == state.Id);

                    if(fresh is not null)
                    {
                        fresh.LastRefreshedAt = DateTime.UtcNow;
                        await stampContext.SaveChangesAsync();
                        stamped++;
                    }
                }

                gamesDone++;
                Console.WriteLine($"    \e[32m✓\e[0m done\n");
            }
            catch(Exception ex)
            {
                failed++;
                Console.WriteLine($"    \e[31m✗\e[0m {ex.GetType().Name}: {ex.Message}\n");
            }
        }

        // ---- Summary ---------------------------------------------------------------------
        Console.WriteLine("\n  ────────────────────────────────────");
        Console.WriteLine(dryRun ? "  \e[33;1m[DRY RUN]\e[0m No changes made" : "  Refresh complete");
        Console.WriteLine($"    Year:                 {year}");
        Console.WriteLine($"    Games in batch:       {batch.Count}  (eligible: {candidates.Count})");
        Console.WriteLine($"    Refreshed:            {gamesDone}{(dryRun ? "" : $"  (stamped {stamped})")}");
        Console.WriteLine($"    Main fetch failed:    {fetchFailed}");
        Console.WriteLine($"    Errors:               {failed}");
        Console.WriteLine($"    Descriptions added:   {descriptionsAdded}");
        Console.WriteLine($"    Releases created:     {releasesCreated}  (matched existing: {releasesSkipped})");
        Console.WriteLine($"    Covers:               {Fmt(coverCounters, dryRun)}");
        Console.WriteLine($"    Screenshots:          {Fmt(screenshotCounters, dryRun)}");
        Console.WriteLine($"    Promo art:            {Fmt(promoCounters, dryRun)}");
        Console.WriteLine($"    Videos:               {Fmt(videoCounters, dryRun)}");
        Console.WriteLine($"    Reviews imported:     {reviewsImported}");
        Console.WriteLine("  ────────────────────────────────────\n");

        if(aborted)
            Console.WriteLine("  \e[33mRun aborted early; unfinished games were not stamped and will be picked up next time.\e[0m");
    }

    /// <summary>
    ///     Resolves the <c>--slug</c> override to a single candidate: the Imported state row under
    ///     either slug variant, with a numeric id from the state row, the cached main page, or a live
    ///     resolution. Prints the reason and returns <c>null</c> when the slug is not refreshable.
    /// </summary>
    async Task<Candidate> SelectSingleAsync(string slug)
    {
        string trimmed = slug.TrimStart('-');

        await using var context = await _contextFactory.CreateDbContextAsync();

        MobyGamesImportState state = await context.MobyGamesImportStates
                                                  .FirstOrDefaultAsync(s => (s.MobyGameId == trimmed ||
                                                                             s.MobyGameId == "-" + trimmed) &&
                                                                            s.Status == MobyGamesImportStatus.Imported);

        if(state is null)
        {
            Console.WriteLine($"  \e[31mNo imported state row for '{trimmed}'. update-year never imports new games.\e[0m");

            return null;
        }

        if(state.SoftwareId is null)
        {
            Console.WriteLine($"  \e[31m'{trimmed}' is a compilation-only import (no SoftwareId); not supported.\e[0m");

            return null;
        }

        if(!await context.Softwares.AnyAsync(sw => sw.Id == state.SoftwareId.Value))
        {
            Console.WriteLine($"  \e[31mSoftware {state.SoftwareId} linked to '{trimmed}' no longer exists.\e[0m");

            return null;
        }

        int? numericId = state.MobyNumericId;

        if(numericId is null)
        {
            string cachedMain = await _sourceDb.GetChunkBodyAsync(state.MobyGameId, NewGameRawFetcher.ChunkMain);
            numericId = MobyGamesHttpClient.ExtractNumericGameIdFromHtml(cachedMain);
        }

        if(numericId is null && _http is not null)
            numericId = await _http.ResolveNumericGameIdAsync(trimmed);

        if(numericId is null or <= 0)
        {
            if(_http is null)
            {
                // --from-cache never needs the numeric id (no URLs are built).
                return new Candidate { State = state, NumericId = 0, Slug = trimmed };
            }

            Console.WriteLine($"  \e[31mCould not resolve the MobyGames numeric id for '{trimmed}'.\e[0m");

            return null;
        }

        return new Candidate { State = state, NumericId = numericId.Value, Slug = trimmed };
    }

    /// <summary>
    ///     <c>--from-cache</c> source: the chunks already in <c>mobygames_raw</c>, tried under the state
    ///     row's own key first and then under the other slug variant. <c>null</c> when no main chunk exists.
    /// </summary>
    async Task<Dictionary<int, string>> LoadCachedChunksAsync(string stateSlug, string trimmedSlug)
    {
        foreach(string key in new[] { stateSlug, trimmedSlug, $"-{trimmedSlug}" }.Distinct())
        {
            List<MobyGamesRawRow> rows = await _sourceDb.GetRowsForGameAsync(key);

            if(rows.Count == 0 || rows.All(r => r.Chunk != NewGameRawFetcher.ChunkMain)) continue;

            return rows.GroupBy(r => r.Chunk).ToDictionary(g => g.Key, g => g.First().Body);
        }

        return null;
    }

    static string Fmt(MediaCounters c, bool dryRun) =>
        dryRun
            ? $"{c.Total} on page, would download {c.WouldAdd}"
            : $"{c.Total} on page, new {c.Added}, existing {c.Skipped}, failed {c.Failed}";

    /// <summary>
    ///     Downloads every page of one game into memory. Returns <c>null</c> when the main page could
    ///     not be fetched (nothing else is attempted). Sub-page 404s are tolerated: the chunk is
    ///     simply absent. Media sub-pages are only requested when the main page advertises them.
    /// </summary>
    async Task<Dictionary<int, string>> FetchAllChunksAsync(string slug, int numericId)
    {
        string baseUrl = $"https://www.mobygames.com/game/{numericId}/{slug}/";

        string mainBody = await _http.FetchPageAsync(baseUrl);

        if(string.IsNullOrWhiteSpace(mainBody)) return null;

        var chunks = new Dictionary<int, string> { [NewGameRawFetcher.ChunkMain] = mainBody };

        async Task TryFetchAsync(int chunk, string suffix)
        {
            string body = await _http.FetchPageAsync(baseUrl + suffix);

            if(!string.IsNullOrWhiteSpace(body))
                chunks[chunk] = body;
        }

        await TryFetchAsync(NewGameRawFetcher.ChunkCredits,  "credits/");
        await TryFetchAsync(NewGameRawFetcher.ChunkReleases, "releases/");
        await TryFetchAsync(NewGameRawFetcher.ChunkSpecs,    "specs/");

        if(MediaPresenceDetector.HasCoverArt(mainBody))    await TryFetchAsync(NewGameRawFetcher.ChunkCovers,      "covers/");
        if(MediaPresenceDetector.HasReviews(mainBody))     await TryFetchAsync(NewGameRawFetcher.ChunkReviews,     "reviews/");
        if(MediaPresenceDetector.HasPromoArt(mainBody))    await TryFetchAsync(NewGameRawFetcher.ChunkPromo,       "promo/");
        if(MediaPresenceDetector.HasScreenshots(mainBody)) await TryFetchAsync(NewGameRawFetcher.ChunkScreenshots, "screenshots/");
        if(MediaPresenceDetector.HasMedia(mainBody))       await TryFetchAsync(NewGameRawFetcher.ChunkMedia,       "media/");

        return chunks;
    }
}
