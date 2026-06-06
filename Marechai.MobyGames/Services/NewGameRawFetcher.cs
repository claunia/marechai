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
using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.MobyGames.Parsers.NewSite;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Per-game raw HTML fetcher for the discovery → raw pipeline. Walks rows from
///     <c>MobyGamesDiscoveredGames</c> that have <c>RawFetchedAt IS NULL</c> and pulls up to six
///     page chunks per game (main, credits, releases, specs, plus optional covers and reviews)
///     into the existing <c>mobygames_raw</c> table — matching the same chunk layout that the
///     legacy 2019 dump uses for the first four chunks.
///     <para>
///         Any game whose slug already exists in <c>mobygames_raw</c> (overlap with the legacy dump
///         or a previous run) is marked <c>SkippedReason = "already-in-raw"</c> instead of refetched.
///         Failures on the main page (chunk 0) are fatal for the game — no partial inserts are
///         written. Failures on credits / releases / specs / covers / reviews are tolerated as 404s
///         (DLC entries often have no credits sub-page).
///     </para>
///     <para>
///         The covers (<see cref="ChunkCovers"/>) and reviews (<see cref="ChunkReviews"/>) chunks
///         are fetched <i>conditionally</i>: we run the cheap regex-based
///         <see cref="MediaPresenceDetector"/> on the main-page body and only request the sub-pages
///         when MobyGames advertises them in the main page's footer navigation. This avoids a 404
///         per game that has no cover / review data while still capturing everything that exists.
///     </para>
/// </summary>
public class NewGameRawFetcher
{
    const    int                   ChunkMain     = 0;
    const    int                   ChunkCredits  = 1;
    const    int                   ChunkReleases = 2;
    const    int                   ChunkSpecs    = 3;
    const    int                   ChunkCovers   = 4;
    const    int                   ChunkReviews  = 5;
    internal const int             ChunkPromo       = 10;
    internal const int             ChunkScreenshots = 11;
    internal const int             ChunkMedia       = 12;
    readonly DiscoveryStateService _discovery;
    readonly MobyGamesHttpClient   _http;
    readonly SourceDatabaseService _sourceDb;

    public NewGameRawFetcher(MobyGamesHttpClient   http,
                             SourceDatabaseService sourceDb,
                             DiscoveryStateService discovery)
    {
        _http      = http;
        _sourceDb  = sourceDb;
        _discovery = discovery;
    }

    public async Task RunAsync(int batchSize, bool dryRun, CancellationToken ct = default)
    {
        Console.WriteLine($"\e[36mScraping new MobyGames games (batch={batchSize}, dryRun={dryRun})\e[0m");

        int totalFetched = 0;
        int totalSkipped = 0;
        int totalErrored = 0;
        int totalGames   = 0;

        // Process a SINGLE batch of up to `batchSize` games per invocation. Re-running the command
        // (or wrapping it in a shell loop) picks up the next batch. This matches the user-visible
        // semantics of every other `--batch-size`-flavoured command in this CLI: "how many to do
        // this run", NOT "how many per inner loop iteration".
        List<MobyGamesDiscoveredGame> pending = await _discovery.GetPendingFetchAsync(batchSize);

        if(pending.Count == 0)
        {
            Console.WriteLine("\e[32mNo more pending games.\e[0m");
        }
        else
        {
            Console.WriteLine($"\e[36m  Processing batch of {pending.Count} games...\e[0m");

            foreach(MobyGamesDiscoveredGame game in pending)
            {
                ct.ThrowIfCancellationRequested();

                totalGames++;

                string slug      = game.Slug;
                int    numericId = game.NumericId;

                if(dryRun)
                {
                    Console.WriteLine($"    [dry-run] Would fetch slug={slug} id={numericId}");

                    continue;
                }

                // Skip games already in mobygames_raw (overlap with the 2019 dump or a previous run).
                if(await _sourceDb.GameExistsAsync(slug))
                {
                    await _discovery.MarkSkippedAsync(slug, "already-in-raw");
                    totalSkipped++;

                    continue;
                }

                string baseUrl = $"https://www.mobygames.com/game/{numericId}/{slug}/";

                // Fetch the main page first — failure here aborts the game (no partial inserts).
                string mainBody = await _http.FetchPageAsync(baseUrl);

                if(string.IsNullOrEmpty(mainBody))
                {
                    await _discovery.MarkErrorAsync(slug, "main page fetch failed");
                    totalErrored++;
                    Console.WriteLine($"    \e[31m✗\e[0m {slug} — main page fetch failed");

                    continue;
                }

                try
                {
                    await _sourceDb.InsertRowAsync(slug, ChunkMain, mainBody);
                }
                catch(Exception ex)
                {
                    await _discovery.MarkErrorAsync(slug, $"db insert failed (main): {ex.Message}");
                    totalErrored++;
                    Console.WriteLine($"    \e[31m✗\e[0m {slug} — db insert failed: {ex.Message}");

                    continue;
                }

                // Credits / releases / specs are best-effort. 404 is fine (DLC often has no credits).
                await TryFetchAndInsertAsync(slug, ChunkCredits,  baseUrl + "credits/");
                await TryFetchAndInsertAsync(slug, ChunkReleases, baseUrl + "releases/");
                await TryFetchAndInsertAsync(slug, ChunkSpecs,    baseUrl + "specs/");

                // Covers and reviews are gated on the main-page footer indicating their presence.
                // The presence detector is a cheap regex scan; we avoid fetching a guaranteed 404
                // when MobyGames has not advertised the sub-page on the main game page.
                if(MediaPresenceDetector.HasCoverArt(mainBody))
                    await TryFetchAndInsertAsync(slug, ChunkCovers, baseUrl + "covers/");

                if(MediaPresenceDetector.HasReviews(mainBody))
                    await TryFetchAndInsertAsync(slug, ChunkReviews, baseUrl + "reviews/");

                await _discovery.MarkFetchedAsync(slug);
                totalFetched++;

                Console.WriteLine($"    \e[32m✓\e[0m {slug}");
            }
        }

        Console.WriteLine("");
        Console.WriteLine("\e[32;1mNew-game raw fetch complete.\e[0m");
        Console.WriteLine($"  Games processed: {totalGames}");
        Console.WriteLine($"  Fetched:         {totalFetched}");
        Console.WriteLine($"  Skipped:         {totalSkipped}");
        Console.WriteLine($"  Errored:         {totalErrored}");

        if(!dryRun) await _discovery.PrintStatusAsync();
    }

    async Task TryFetchAndInsertAsync(string slug, int chunk, string url)
    {
        string body = await _http.FetchPageAsync(url);
        if(string.IsNullOrEmpty(body)) return;

        try
        {
            await _sourceDb.InsertRowAsync(slug, chunk, body);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"    \e[33m!\e[0m {slug} chunk {chunk} db insert failed: {ex.Message}");
        }
    }
}
