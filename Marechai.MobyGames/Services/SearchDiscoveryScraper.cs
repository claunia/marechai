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
using Marechai.MobyGames.Parsers;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Walks the MobyGames new-site year-filtered search results and seeds the
///     <c>MobyGamesDiscoveredGames</c> table. Replaces the old sitemap-based discovery, which used
///     edit-date <c>lastmod</c> entries that don't reliably correlate with release year.
///     <para>
///         Discovery hits the MobyPlus <c>?export=json</c> endpoint once per year, which returns
///         every matching game in a single response — no pagination. Falling back to the paginated
///         <c>&lt;game-browser&gt;</c> SSR envelope would require an authenticated MobyPlus session
///         AND opting into the v2 browser AND walking dozens of pages per year, so we deliberately
///         depend on the export endpoint here. <see cref="MobyGamesBrowser"/> still owns the
///         Cloudflare/Turnstile and login flow.
///     </para>
/// </summary>
public sealed class SearchDiscoveryScraper
{
    readonly MobyGamesBrowser      _browser;
    readonly DiscoveryStateService _state;

    public SearchDiscoveryScraper(MobyGamesBrowser browser, DiscoveryStateService state)
    {
        _browser = browser;
        _state   = state;
    }

    /// <summary>
    ///     Discover all games released in [<paramref name="fromYear"/>, <paramref name="toYear"/>]
    ///     (inclusive in both directions).
    /// </summary>
    /// <param name="dryRun">
    ///     If <c>true</c>, parses every page but does NOT write to the database (useful for smoke-testing).
    /// </param>
    public async Task RunAsync(int fromYear, int toYear, bool dryRun, CancellationToken ct = default)
    {
        await _browser.InitializeAsync(ct);

        Console.WriteLine($"  Discovering MobyGames releases {fromYear}..{toYear} (dryRun={dryRun})");

        int totalInserted = 0;
        int totalUpdated  = 0;
        int totalSeen     = 0;

        for(int year = fromYear; year <= toYear; year++)
        {
            ct.ThrowIfCancellationRequested();

            string json;

            try
            {
                json = await _browser.FetchSearchExportJsonAsync(year, ct);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\e[31m  Error fetching {year}: {ex.Message}\e[0m");

                continue;
            }

            IReadOnlyList<SearchResultsPageParser.SearchResultGame> games =
                SearchResultsPageParser.ParseExport(json);

            if(games is null)
            {
                Console.WriteLine(
                    $"\e[33m  Warning: malformed export JSON for {year} — skipping the year.\e[0m");

                continue;
            }

            // Build the upsert batch.
            var batch = new List<(string Slug, int NumericId, string Title, string Developer, int? Year)>(
                games.Count);

            foreach(SearchResultsPageParser.SearchResultGame g in games)
            {
                if(string.IsNullOrWhiteSpace(g.Slug)) continue;
                if(g.NumericId <= 0) continue;

                batch.Add((g.Slug, g.NumericId, g.Title, g.Developer, g.ReleaseYear ?? year));
            }

            int yearInserted = 0;
            int yearUpdated  = 0;

            if(!dryRun && batch.Count > 0)
                (yearInserted, yearUpdated) = await _state.UpsertBatchAsync(batch);

            Console.WriteLine($"  {year}: games={games.Count,5} usable={batch.Count,5} " +
                              $"(ins {yearInserted,5}, upd {yearUpdated,5})");

            totalSeen     += batch.Count;
            totalInserted += yearInserted;
            totalUpdated  += yearUpdated;
        }

        Console.WriteLine();
        Console.WriteLine($"  Done. seen={totalSeen}, inserted={totalInserted}, updated={totalUpdated}");
    }
}
