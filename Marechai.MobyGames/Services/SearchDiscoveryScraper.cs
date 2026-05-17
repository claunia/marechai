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
///         Discovery walks the Vue SPA's <c>?format=json</c> endpoint page-by-page. The
///         <c>?export=json</c> endpoint this scraper used to call hard-caps at 2500 games per
///         query and silently ignores the path's <c>page:N</c> segment, so any year with more
///         than 2500 releases (every year from at least 2019 onwards) was being truncated.
///         The page size is lifted from the default 18 to the server-enforced maximum of 100
///         via the <c>perPage=100</c> preference cookie.
///         <see cref="MobyGamesBrowser.TryAttachCookiesAsync"/> owns the Cloudflare/Turnstile and
///         login flow and exits as soon as the cookies are copied into the HTTP client.
///     </para>
/// </summary>
public sealed class SearchDiscoveryScraper
{
    readonly MobyGamesHttpClient   _http;
    readonly DiscoveryStateService _state;

    public SearchDiscoveryScraper(MobyGamesHttpClient http, DiscoveryStateService state)
    {
        _http  = http;
        _state = state;
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
        // Bump the page size from the SPA default of 18 to the server-enforced ceiling of 100.
        // Anything higher than 100 is silently clamped server-side.
        _http.SetPreferenceCookie("perPage", "100");

        Console.WriteLine($"  Discovering MobyGames releases {fromYear}..{toYear} (dryRun={dryRun})");

        int totalInserted = 0;
        int totalUpdated  = 0;
        int totalSeen     = 0;

        for(int year = fromYear; year <= toYear; year++)
        {
            ct.ThrowIfCancellationRequested();

            // De-dupe across pages within a year so that if MobyGames ever silently ignores the
            // path's `page:N` segment we don't fetch the same page forever.
            var yearSeen = new HashSet<int>();

            int yearTotalGames    = 0;
            int yearUsable        = 0;
            int yearInsertedTotal = 0;
            int yearUpdatedTotal  = 0;
            int reportedTotal     = -1;
            int reportedPageCount = -1;

            // Hard ceiling on pages just to keep us out of trouble if maxPages is missing AND a
            // page comes back full of new ids. With perPage=100 this is room for 100k games/year,
            // far above any plausible MobyGames count.
            const int maxPagesGuard = 1000;

            int page = 1;

            while(page <= maxPagesGuard)
            {
                ct.ThrowIfCancellationRequested();

                // The server returns `maxPages: 1000000` as a sentinel regardless of `total`, so
                // never trust it for display. Compute the real page count from total/perPage on the
                // first response and reuse it thereafter.
                string pageOfLabel = reportedPageCount > 0 ? reportedPageCount.ToString() : "?";

                Console.WriteLine($"    {year} parsing page {page} of {pageOfLabel}...");

                string json;

                try
                {
                    json = await _http.FetchSearchPageJsonAsync(year, page);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"\e[31m  Error fetching {year} page {page}: {ex.Message}\e[0m");

                    break;
                }

                SearchResultsPageParser.SearchResultsPage parsed = SearchResultsPageParser.ParseEnvelope(json);

                if(parsed is null)
                {
                    Console.WriteLine(
                        $"\e[33m  Warning: malformed search-page JSON for {year} page {page} — skipping the rest of the year.\e[0m");

                    break;
                }

                if(reportedTotal < 0)
                {
                    reportedTotal     = parsed.Total;
                    reportedPageCount = parsed.PerPage > 0
                        ? (parsed.Total + parsed.PerPage - 1) / parsed.PerPage
                        : -1;
                }

                int pageNewIds = 0;

                var batch = new List<(string Slug, int NumericId, string Title, string Developer, int? Year)>(
                    parsed.Games.Count);

                foreach(SearchResultsPageParser.SearchResultGame g in parsed.Games)
                {
                    if(string.IsNullOrWhiteSpace(g.Slug)) continue;
                    if(g.NumericId <= 0) continue;

                    if(!yearSeen.Add(g.NumericId)) continue;

                    pageNewIds++;
                    batch.Add((g.Slug, g.NumericId, g.Title, g.Developer, g.ReleaseYear ?? year));
                }

                yearTotalGames += parsed.Games.Count;
                yearUsable     += batch.Count;

                int pageInserted = 0;
                int pageUpdated  = 0;

                if(!dryRun && batch.Count > 0)
                    (pageInserted, pageUpdated) = await _state.UpsertBatchAsync(batch);

                yearInsertedTotal += pageInserted;
                yearUpdatedTotal  += pageUpdated;

                // Stop conditions, in priority order:
                //   1. The page returned zero new ids — we either hit the end or the server is
                //      ignoring our page parameter. Either way, stop.
                //   2. We reached the computed last page (ceil(total/perPage)). The server's own
                //      `maxPages` field is the sentinel 1000000 and useless for this.
                //   3. The page came back short of perPage. Classic "last page" indicator.
                if(pageNewIds == 0) break;
                if(reportedPageCount > 0 && page >= reportedPageCount) break;
                if(parsed.PerPage > 0 && parsed.Games.Count < parsed.PerPage) break;

                page++;
            }

            string totalLabel     = reportedTotal     >= 0 ? reportedTotal.ToString()     : "?";
            string pageCountLabel = reportedPageCount >  0 ? reportedPageCount.ToString() : "?";

            Console.WriteLine(
                $"  {year}: pages={page,3} games={yearTotalGames,6} usable={yearUsable,6} " +
                $"(ins {yearInsertedTotal,6}, upd {yearUpdatedTotal,6}) " +
                $"server total={totalLabel} pageCount={pageCountLabel}");

            totalSeen     += yearUsable;
            totalInserted += yearInsertedTotal;
            totalUpdated  += yearUpdatedTotal;
        }

        Console.WriteLine();
        Console.WriteLine($"  Done. seen={totalSeen}, inserted={totalInserted}, updated={totalUpdated}");
    }
}
