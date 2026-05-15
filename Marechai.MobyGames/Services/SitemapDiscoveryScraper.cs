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
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Marechai.MobyGames.Parsers;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Discovers new MobyGames games by walking the sitemaps hosted on the DigitalOcean Spaces CDN
///     (<c>sfo3.digitaloceanspaces.com</c>) — bypasses Cloudflare entirely, no pagination cap.
///     <para>
///         Phase 1: GET <c>https://www.mobygames.com/sitemap_index.xml</c> → list of ~113 sub-sitemap
///         URLs on the DigitalOcean CDN. Phase 2: for each sub-sitemap, GET the gzipped XML, decompress
///         with <see cref="GZipStream" />, stream-parse with <see cref="SitemapParser" />, and upsert
///         every URL matching the main-game-page shape (<c>/game/{id}/{slug}/</c>) whose
///         <c>&lt;lastmod&gt;</c> year falls inside the requested window.
///     </para>
///     <para>
///         The year filter is a coarse first cut: <c>&lt;lastmod&gt;</c> is the record's last edit date,
///         not the release date. A 1990 game can have <c>lastmod=2024</c> if anyone touched its page.
///         The per-game raw fetcher (<c>NewGameRawFetcher</c>) eliminates those by checking
///         <c>mobygames_raw</c> for the slug before fetching.
///     </para>
/// </summary>
public class SitemapDiscoveryScraper
{
    const    string                IndexUrl = "https://www.mobygames.com/sitemap_index.xml";
    readonly MobyGamesHttpClient   _http;
    readonly DiscoveryStateService _state;

    public SitemapDiscoveryScraper(MobyGamesHttpClient http, DiscoveryStateService state)
    {
        _http  = http;
        _state = state;
    }

    public async Task RunAsync(int fromYear, int toYear, bool dryRun, CancellationToken ct = default)
    {
        Console.WriteLine($"\e[36mFetching sitemap index from {IndexUrl}\e[0m");

        byte[] indexBytes = await _http.FetchBytesAsync(IndexUrl);

        if(indexBytes is null || indexBytes.Length == 0)
        {
            Console.WriteLine("\e[31m  Failed to fetch sitemap index — aborting.\e[0m");

            return;
        }

        IReadOnlyList<string> subSitemaps;

        await using(var ms = new MemoryStream(indexBytes))
            subSitemaps = SitemapParser.ParseIndex(ms);

        Console.WriteLine($"\e[36m  Index lists {subSitemaps.Count} sub-sitemaps.\e[0m");
        Console.WriteLine($"\e[36m  Year filter: lastmod year in [{fromYear} .. {toYear}].\e[0m");

        if(dryRun)
            Console.WriteLine("\e[33m  --dry-run: no rows will be written to MobyGamesDiscoveredGames.\e[0m");

        int totalScanned     = 0;
        int totalMatchedYear = 0;
        int totalInserted    = 0;
        int totalUpdated     = 0;
        int totalGamePages   = 0;

        for(int i = 0; i < subSitemaps.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            string url = subSitemaps[i];

            Console.WriteLine($"\e[36m[{i + 1,3}/{subSitemaps.Count}] {url}\e[0m");

            byte[] gz = await _http.FetchBytesAsync(url);

            if(gz is null || gz.Length == 0)
            {
                Console.WriteLine("\e[33m    Skipped (fetch failed).\e[0m");

                continue;
            }

            int scanned   = 0;
            int gamePages = 0;
            int matched   = 0;
            int inserted  = 0;
            int updated   = 0;

            var batch = new List<(string Slug, int NumericId, DateTime? LastMod)>(capacity: 1024);

            try
            {
                await using var raw  = new MemoryStream(gz);
                await using var gzip = new GZipStream(raw, CompressionMode.Decompress);

                foreach(SitemapParser.SitemapEntry entry in SitemapParser.ParseSubSitemap(gzip))
                {
                    scanned++;

                    (int NumericId, string Slug)? hit = SitemapParser.TryExtractGameSlug(entry.Url);
                    if(hit is null) continue;

                    gamePages++;

                    int? year = entry.LastMod?.Year;
                    if(year is null || year < fromYear || year > toYear) continue;

                    matched++;

                    batch.Add((hit.Value.Slug, hit.Value.NumericId, entry.LastMod));

                    if(batch.Count >= 1000 && !dryRun)
                    {
                        (int ins, int upd) = await _state.UpsertBatchAsync(batch);
                        inserted += ins;
                        updated  += upd;
                        batch.Clear();
                    }
                }

                if(batch.Count > 0 && !dryRun)
                {
                    (int ins, int upd) = await _state.UpsertBatchAsync(batch);
                    inserted += ins;
                    updated  += upd;
                    batch.Clear();
                }
            }
            catch(InvalidDataException ex)
            {
                Console.WriteLine($"\e[33m    Skipped (gzip / xml error: {ex.Message}).\e[0m");

                continue;
            }
            catch(System.Xml.XmlException ex)
            {
                Console.WriteLine($"\e[33m    Skipped (xml error: {ex.Message}).\e[0m");

                continue;
            }

            totalScanned     += scanned;
            totalGamePages   += gamePages;
            totalMatchedYear += matched;
            totalInserted    += inserted;
            totalUpdated     += updated;

            if(dryRun)
                Console.WriteLine($"    {scanned} urls, {gamePages} game pages, {matched} in year range (would upsert)");
            else
                Console.WriteLine(
                    $"    {scanned} urls, {gamePages} game pages, {matched} in year range, {inserted} inserted, {updated} updated");
        }

        Console.WriteLine("");
        Console.WriteLine("\e[32;1mDiscovery complete.\e[0m");
        Console.WriteLine($"  Sitemap URLs scanned:   {totalScanned}");
        Console.WriteLine($"  Main game pages:        {totalGamePages}");
        Console.WriteLine($"  Within year range:      {totalMatchedYear}");

        if(!dryRun)
        {
            Console.WriteLine($"  Inserted: {totalInserted}");
            Console.WriteLine($"  Updated:  {totalUpdated}");
            await _state.PrintStatusAsync();
        }
    }
}
