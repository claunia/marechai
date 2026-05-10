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
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Helpers;
using Marechai.Database.Models;
using Marechai.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/search")]
[ApiController]
public class SearchController(MarechaiContext context, FuzzySearchService fuzzy) : ControllerBase
{
    /// <summary>Quick autocomplete used by the nav-bar SearchBar. Returns up to <paramref name="take"/> ranked results.</summary>
    [HttpGet("autocomplete")]
    [AllowAnonymous]
    [ProducesResponseType<List<SearchResultDto>>(StatusCodes.Status200OK)]
    public async Task<List<SearchResultDto>> AutocompleteAsync([FromQuery] string q,
                                                               [FromQuery] int? entityType = null,
                                                               [FromQuery] int take = 8)
    {
        if(string.IsNullOrWhiteSpace(q) || q.Length < 3) return [];

        string normalized = SearchIndexUpdater.Normalize(q);
        if(string.IsNullOrWhiteSpace(normalized)) return [];

        IQueryable<SearchEntry> baseQ = context.SearchEntries.AsNoTracking();
        if(entityType is { } et)
        {
            byte etByte = (byte)et;
            baseQ = baseQ.Where(e => (byte)e.EntityType == etByte);
        }

        // Layer 1: prefix LIKE (sub-20ms, served by the BTREE on NormalizedName(255)).
        string likePrefix = EscapeLike(normalized) + "%";
        List<SearchEntry> prefixHits = await baseQ
                                            .Where(e => EF.Functions.Like(e.NormalizedName, likePrefix))
                                            .Take(40)
                                            .ToListAsync();

        // Layer 2: FULLTEXT BOOLEAN-mode partial token match (handles "amst" matching "Amstrad"
        // and word-position-independent matching like "ii apple" → "Apple II").
        List<SearchEntry> fulltextHits = await ExecuteFulltextAsync(normalized, entityType, 40);

        // Layer 3: Soundex phonetic match — typo tolerance for cases prefix/FULLTEXT miss
        // ("comodore"→"commodore", "appel"→"apple").
        string soundex = SearchIndexUpdater.Soundex(normalized);
        List<SearchEntry> soundexHits = await ExecuteSoundexAsync(soundex, entityType, 40);

        // De-duplicate by primary key.
        Dictionary<long, SearchEntry> dedup = new(prefixHits.Count + fulltextHits.Count + soundexHits.Count);
        foreach(SearchEntry e in prefixHits) dedup.TryAdd(e.Id, e);
        foreach(SearchEntry e in fulltextHits) dedup.TryAdd(e.Id, e);
        foreach(SearchEntry e in soundexHits) dedup.TryAdd(e.Id, e);
        if(dedup.Count == 0) return [];

        // Layer 4: in-process Jaro-Winkler re-rank.
        List<(SearchEntry entry, double score)> ranked = dedup.Values
                                                              .Select(e => (e, fuzzy.Score(normalized, e.NormalizedName ?? string.Empty)))
                                                              .OrderByDescending(p => p.Item2)
                                                              .ThenBy(p => p.Item1.DisplayName)
                                                              .Take(Math.Clamp(take, 1, 50))
                                                              .ToList();

        Dictionary<short, string> countryNames = await GetCountryNameMapAsync(ranked.Select(r => r.entry.CountryId));

        return ranked.Select(r => Project(r.entry, r.score, countryNames)).ToList();
    }

    /// <summary>Full results page used by /search and /search/advanced. Applies post-filters and groups by entity type.</summary>
    [HttpPost("results")]
    [AllowAnonymous]
    [ProducesResponseType<SearchResultsPageDto>(StatusCodes.Status200OK)]
    public async Task<SearchResultsPageDto> SearchAsync([FromBody] SearchRequestDto req)
    {
        var page = new SearchResultsPageDto();
        if(req == null) return page;

        // A request with no query AND no filters at all has nothing to do.
        bool hasQuery   = !string.IsNullOrWhiteSpace(req.Query);
        bool hasFilters = (req.EntityTypes is { Length: > 0 }) ||
                          req.YearFrom.HasValue || req.YearTo.HasValue ||
                          req.CountryId.HasValue || req.HasImage.HasValue ||
                          req.YearKnown.HasValue || req.CountryKnown.HasValue ||
                          req.CompanyId.HasValue ||
                          req.SoftwareKind.HasValue ||
                          req.IncludesSoftwareId.HasValue ||
                          !string.IsNullOrWhiteSpace(req.Letter) ||
                          req.InCollection.HasValue ||
                          !string.IsNullOrWhiteSpace(req.Contains) ||
                          !string.IsNullOrWhiteSpace(req.NotContains);
        if(!hasQuery && !hasFilters) return page;

        string normalized = hasQuery ? SearchIndexUpdater.Normalize(req.Query) : string.Empty;

        IQueryable<SearchEntry> baseQ = context.SearchEntries.AsNoTracking();

        // SoftwareKind implies entity-type ∈ {Software=12, SoftwareCompilation=13}; merge with explicit EntityTypes.
        // IncludesSoftwareId implies entity-type = SoftwareCompilation=13 (only compilations contain other software).
        int[] effectiveTypes = req.EntityTypes;
        if(req.IncludesSoftwareId.HasValue)
        {
            effectiveTypes = (effectiveTypes is { Length: > 0 }
                                  ? effectiveTypes.Where(t => t == 13).ToArray()
                                  : null) ?? [13];
            if(effectiveTypes.Length == 0) return page; // incompatible with explicit non-compilation types
        }
        if(req.SoftwareKind.HasValue)
        {
            effectiveTypes = (effectiveTypes is { Length: > 0 }
                                  ? effectiveTypes.Where(t => t == 12 || t == 13).ToArray()
                                  : null) ?? [12, 13];
            if(effectiveTypes.Length == 0) return page; // user picked types incompatible with software-kind filter
        }

        if(effectiveTypes is { Length: > 0 })
        {
            byte[] etBytes = effectiveTypes.Select(t => (byte)t).ToArray();
            baseQ = baseQ.Where(e => etBytes.Contains((byte)e.EntityType));
        }

        if(req.YearFrom.HasValue) baseQ  = baseQ.Where(e => e.Year >= req.YearFrom);
        if(req.YearTo.HasValue) baseQ    = baseQ.Where(e => e.Year <= req.YearTo);
        if(req.CountryId.HasValue) baseQ = baseQ.Where(e => e.CountryId == req.CountryId);
        if(req.HasImage.HasValue) baseQ  = baseQ.Where(e => e.HasImage == req.HasImage);
        if(req.YearKnown == true) baseQ      = baseQ.Where(e => e.Year != null);
        if(req.YearKnown == false) baseQ     = baseQ.Where(e => e.Year == null);
        if(req.CountryKnown == true) baseQ   = baseQ.Where(e => e.CountryId != null);
        if(req.CountryKnown == false) baseQ  = baseQ.Where(e => e.CountryId == null);
        if(req.CompanyId.HasValue) baseQ     = baseQ.Where(e => e.CompanyId == req.CompanyId);
        if(req.SoftwareKind.HasValue)
        {
            byte k = (byte)req.SoftwareKind.Value;
            baseQ = baseQ.Where(e => e.Kind == k);
        }
        if(req.IncludesSoftwareId.HasValue)
        {
            ulong wantedSwId = (ulong)req.IncludesSoftwareId.Value;
            // For each compilation row (EntityType=13), EntityId is the SoftwareRelease.Id (cast to long).
            // Match when the compilation contains the wanted software via SoftwareBySoftwareRelease.
            IQueryable<long> compilationIdsContaining = context.SoftwareBySoftwareRelease
                                                               .Where(s => s.SoftwareId == wantedSwId)
                                                               .Select(s => (long)s.ReleaseId);
            baseQ = baseQ.Where(e => (byte)e.EntityType == 13 && compilationIdsContaining.Contains(e.EntityId));
        }
        if(!string.IsNullOrWhiteSpace(req.Letter))
        {
            string letter = req.Letter.Trim().ToLowerInvariant();
            if(letter == "#")
            {
                // Non-alphabetic first character (digit or other normalized char).
                baseQ = baseQ.Where(e => e.NormalizedName != "" &&
                                         (EF.Functions.Like(e.NormalizedName, "0%") || EF.Functions.Like(e.NormalizedName, "1%") ||
                                          EF.Functions.Like(e.NormalizedName, "2%") || EF.Functions.Like(e.NormalizedName, "3%") ||
                                          EF.Functions.Like(e.NormalizedName, "4%") || EF.Functions.Like(e.NormalizedName, "5%") ||
                                          EF.Functions.Like(e.NormalizedName, "6%") || EF.Functions.Like(e.NormalizedName, "7%") ||
                                          EF.Functions.Like(e.NormalizedName, "8%") || EF.Functions.Like(e.NormalizedName, "9%")));
            }
            else if(letter.Length == 1 && letter[0] >= 'a' && letter[0] <= 'z')
            {
                string p = letter + "%";
                baseQ = baseQ.Where(e => EF.Functions.Like(e.NormalizedName, p));
            }
        }

        // InCollection requires authenticated user; silently ignored if no Sid claim.
        if(req.InCollection.HasValue)
        {
            string userId = User.FindFirstValue(ClaimTypes.Sid);
            if(!string.IsNullOrEmpty(userId))
            {
                baseQ = ApplyCollectionFilter(baseQ, userId, req.InCollection.Value);
            }
        }

        Dictionary<long, SearchEntry> dedup;

        if(hasQuery && !string.IsNullOrWhiteSpace(normalized))
        {
            // Candidate seed: prefix LIKE + FULLTEXT + Soundex, capped per source per type to keep memory bounded.
            string likePrefix = EscapeLike(normalized) + "%";
            List<SearchEntry> prefixHits = await baseQ
                                                .Where(e => EF.Functions.Like(e.NormalizedName, likePrefix))
                                                .Take(800)
                                                .ToListAsync();

            List<SearchEntry> fulltextHits = await ExecuteFulltextAsync(normalized,
                                                                        effectiveTypes is { Length: 1 } single
                                                                            ? single[0]
                                                                            : null,
                                                                        800);

            string soundex = SearchIndexUpdater.Soundex(normalized);
            List<SearchEntry> soundexHits = await ExecuteSoundexAsync(soundex,
                                                                      effectiveTypes is { Length: 1 } single2
                                                                          ? single2[0]
                                                                          : null,
                                                                      800);

            // Honor the multi-type filter manually for the FULLTEXT/Soundex branches when EntityTypes.Length > 1.
            if(effectiveTypes is { Length: > 1 })
            {
                HashSet<byte> set = new(effectiveTypes.Select(t => (byte)t));
                fulltextHits = fulltextHits.Where(e => set.Contains((byte)e.EntityType)).ToList();
                soundexHits  = soundexHits.Where(e => set.Contains((byte)e.EntityType)).ToList();
            }

            // Apply other filters to fulltextHits + soundexHits (which bypassed the IQueryable filters above).
            IEnumerable<SearchEntry> ftFiltered = fulltextHits.Concat(soundexHits);
            if(req.YearFrom.HasValue) ftFiltered  = ftFiltered.Where(e => e.Year >= req.YearFrom);
            if(req.YearTo.HasValue) ftFiltered    = ftFiltered.Where(e => e.Year <= req.YearTo);
            if(req.CountryId.HasValue) ftFiltered = ftFiltered.Where(e => e.CountryId == req.CountryId);
            if(req.HasImage.HasValue) ftFiltered  = ftFiltered.Where(e => e.HasImage == req.HasImage);
            if(req.YearKnown == true) ftFiltered    = ftFiltered.Where(e => e.Year != null);
            if(req.YearKnown == false) ftFiltered   = ftFiltered.Where(e => e.Year == null);
            if(req.CountryKnown == true) ftFiltered  = ftFiltered.Where(e => e.CountryId != null);
            if(req.CountryKnown == false) ftFiltered = ftFiltered.Where(e => e.CountryId == null);
            if(req.CompanyId.HasValue) ftFiltered = ftFiltered.Where(e => e.CompanyId == req.CompanyId);
            if(req.SoftwareKind.HasValue)
            {
                byte k = (byte)req.SoftwareKind.Value;
                ftFiltered = ftFiltered.Where(e => e.Kind == k);
            }
            if(req.IncludesSoftwareId.HasValue)
            {
                ulong wantedSwId = (ulong)req.IncludesSoftwareId.Value;
                HashSet<long> compIds = (await context.SoftwareBySoftwareRelease
                                                      .Where(s => s.SoftwareId == wantedSwId)
                                                      .Select(s => (long)s.ReleaseId)
                                                      .ToListAsync()).ToHashSet();
                ftFiltered = ftFiltered.Where(e => (byte)e.EntityType == 13 && compIds.Contains(e.EntityId));
            }
            if(!string.IsNullOrWhiteSpace(req.Letter))
            {
                string letter = req.Letter.Trim().ToLowerInvariant();
                if(letter == "#")
                    ftFiltered = ftFiltered.Where(e => !string.IsNullOrEmpty(e.NormalizedName) &&
                                                       e.NormalizedName[0] >= '0' && e.NormalizedName[0] <= '9');
                else if(letter.Length == 1 && letter[0] >= 'a' && letter[0] <= 'z')
                    ftFiltered = ftFiltered.Where(e => !string.IsNullOrEmpty(e.NormalizedName) &&
                                                       e.NormalizedName[0] == letter[0]);
            }
            // InCollection on the FULLTEXT/Soundex branch: cheaper to intersect by id once we have the user id.
            if(req.InCollection.HasValue)
            {
                string userId = User.FindFirstValue(ClaimTypes.Sid);
                if(!string.IsNullOrEmpty(userId))
                {
                    HashSet<(byte type, long id)> ownedKeys = await GetOwnedKeysAsync(userId);
                    ftFiltered = req.InCollection.Value
                                     ? ftFiltered.Where(e => ownedKeys.Contains(((byte)e.EntityType, e.EntityId)))
                                     : ftFiltered.Where(e => !ownedKeys.Contains(((byte)e.EntityType, e.EntityId)));
                }
            }

            // Merge candidates.
            dedup = new Dictionary<long, SearchEntry>(prefixHits.Count + fulltextHits.Count + soundexHits.Count);
            foreach(SearchEntry e in prefixHits) dedup.TryAdd(e.Id, e);
            foreach(SearchEntry e in ftFiltered) dedup.TryAdd(e.Id, e);
        }
        else
        {
            // Filter-only search (no query): pull straight from the indexed columns up to the take limit.
            // Cap at 2000 to bound memory; per-category totals reflect this cap, not the absolute DB count.
            List<SearchEntry> rows = await baseQ
                                          .OrderBy(e => e.EntityType)
                                          .ThenBy(e => e.DisplayName)
                                          .Take(2000)
                                          .ToListAsync();
            dedup = new Dictionary<long, SearchEntry>(rows.Count);
            foreach(SearchEntry e in rows) dedup.TryAdd(e.Id, e);
        }

        // Post-filter: contains / not-contains / exact (apply regardless of search mode).
        IEnumerable<SearchEntry> candidates = dedup.Values;

        if(hasQuery && req.ExactMatch)
        {
            candidates = candidates.Where(e => (e.NormalizedName ?? string.Empty)
                                          .Contains(normalized, StringComparison.Ordinal));
        }

        if(!string.IsNullOrWhiteSpace(req.Contains))
        {
            string c = SearchIndexUpdater.Normalize(req.Contains);
            if(c.Length > 0)
                candidates = candidates.Where(e => (e.NormalizedName ?? string.Empty).Contains(c, StringComparison.Ordinal));
        }

        if(!string.IsNullOrWhiteSpace(req.NotContains))
        {
            string nc = SearchIndexUpdater.Normalize(req.NotContains);
            if(nc.Length > 0)
                candidates = candidates.Where(e => !(e.NormalizedName ?? string.Empty).Contains(nc, StringComparison.Ordinal));
        }

        // Determine sort order. Default = "relevance" when query present, "name_asc" when filter-only.
        string sort = (req.Sort ?? string.Empty).Trim().ToLowerInvariant();
        if(string.IsNullOrEmpty(sort)) sort = hasQuery ? "relevance" : "name_asc";

        List<(SearchEntry entry, double score)> ranked;
        if(sort == "relevance" && hasQuery)
        {
            ranked = candidates
                .Select(e => (e, fuzzy.Score(normalized, e.NormalizedName ?? string.Empty)))
                .Where(p => p.Item2 > 0d)
                .OrderByDescending(p => p.Item2)
                .ThenBy(p => p.Item1.DisplayName)
                .ToList();
        }
        else
        {
            IEnumerable<(SearchEntry entry, double score)> withScores =
                candidates.Select(e => (e, hasQuery ? fuzzy.Score(normalized, e.NormalizedName ?? string.Empty) : 0d));
            // When ranking is requested non-relevance with a query present, drop zero-score noise.
            if(hasQuery) withScores = withScores.Where(p => p.Item2 > 0d);

            ranked = sort switch
            {
                "name_desc"  => withScores.OrderByDescending(p => p.Item1.DisplayName).ToList(),
                "year_desc"  => withScores.OrderByDescending(p => p.Item1.Year ?? int.MinValue)
                                          .ThenBy(p => p.Item1.DisplayName).ToList(),
                "year_asc"   => withScores.OrderBy(p => p.Item1.Year ?? int.MaxValue)
                                          .ThenBy(p => p.Item1.DisplayName).ToList(),
                _            => withScores.OrderBy(p => p.Item1.EntityType)
                                          .ThenBy(p => p.Item1.DisplayName).ToList()
            };
        }

        // Per-type totals across the FULL ranked set (not page-limited).
        page.TotalByType = ranked
                           .GroupBy(r => (int)r.entry.EntityType)
                           .ToDictionary(g => g.Key, g => g.Count());

        int skip = Math.Max(0, req.Skip);
        int take = Math.Clamp(req.Take, 1, 500);
        List<(SearchEntry entry, double score)> windowed = ranked.Skip(skip).Take(take).ToList();

        Dictionary<short, string> countryNames =
            await GetCountryNameMapAsync(windowed.Select(p => p.entry.CountryId));

        page.Results = windowed.Select(p => Project(p.entry, p.score, countryNames)).ToList();
        return page;
    }

    async Task<List<SearchEntry>> ExecuteFulltextAsync(string normalizedQuery, int? entityType, int limit)
    {
        if(string.IsNullOrWhiteSpace(normalizedQuery)) return [];

        // MariaDB's default FULLTEXT parser tokenizes on whitespace + has ft_min_word_len=3 (InnoDB default).
        // We build a BOOLEAN MODE query: each query token of length >= 3 becomes "+token*" (required + prefix wildcard);
        // shorter tokens are dropped because the index can't match them. The prefix-LIKE branch covers those cases.
        string booleanQuery = BuildBooleanQuery(normalizedQuery);
        if(string.IsNullOrEmpty(booleanQuery)) return [];

        try
        {
            FormattableString sql = entityType.HasValue
                                        ? (FormattableString)$@"
                                          SELECT * FROM `SearchEntries`
                                          WHERE MATCH(`NormalizedName`) AGAINST({booleanQuery} IN BOOLEAN MODE)
                                            AND `EntityType` = {(byte)entityType.Value}
                                          LIMIT {limit}"
                                        : $@"
                                          SELECT * FROM `SearchEntries`
                                          WHERE MATCH(`NormalizedName`) AGAINST({booleanQuery} IN BOOLEAN MODE)
                                          LIMIT {limit}";

            return await context.SearchEntries.FromSqlInterpolated(sql).AsNoTracking().ToListAsync();
        }
        catch
        {
            // FULLTEXT can fail if the table is empty / index not yet ready / parser unavailable.
            return [];
        }
    }

    /// <summary>Build a MariaDB BOOLEAN-MODE FULLTEXT query from a normalized user input.</summary>
    static string BuildBooleanQuery(string normalized)
    {
        // Strip BOOLEAN-MODE operators that could otherwise be interpreted as syntax.
        string sanitized = new string(normalized.Where(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or ' ').ToArray());
        string[] tokens  = sanitized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if(tokens.Length == 0) return string.Empty;

        // Keep only tokens >= 3 chars (InnoDB ft_min_word_len default); each becomes a required prefix.
        IEnumerable<string> usable = tokens.Where(t => t.Length >= 3).Select(t => "+" + t + "*");
        return string.Join(" ", usable);
    }

    /// <summary>Phonetic candidate lookup: indexed by Soundex column. Cheap typo tolerance for names.</summary>
    async Task<List<SearchEntry>> ExecuteSoundexAsync(string soundex, int? entityType, int limit)
    {
        if(string.IsNullOrWhiteSpace(soundex)) return [];

        IQueryable<SearchEntry> q = context.SearchEntries.AsNoTracking()
                                           .Where(e => e.Soundex == soundex);
        if(entityType.HasValue)
        {
            byte etByte = (byte)entityType.Value;
            q = q.Where(e => (byte)e.EntityType == etByte);
        }

        return await q.Take(limit).ToListAsync();
    }

    async Task<Dictionary<short, string>> GetCountryNameMapAsync(IEnumerable<short?> ids)
    {
        short[] distinct = ids.Where(i => i.HasValue).Select(i => i!.Value).Distinct().ToArray();
        if(distinct.Length == 0) return new Dictionary<short, string>();

        return await context.Iso31661Numeric.AsNoTracking()
                            .Where(c => distinct.Contains(c.Id))
                            .ToDictionaryAsync(c => c.Id, c => c.Name);
    }

    static SearchResultDto Project(SearchEntry e, double score, IReadOnlyDictionary<short, string> countryNames)
    {
        string countryName = null;
        if(e.CountryId.HasValue) countryNames.TryGetValue(e.CountryId.Value, out countryName);

        return new SearchResultDto
        {
            EntityType  = (int)e.EntityType,
            EntityId    = e.EntityId,
            DisplayName = e.DisplayName,
            AltName     = e.AltName,
            Year        = e.Year,
            CountryId   = e.CountryId,
            CountryName = countryName,
            HasImage    = e.HasImage,
            Score       = score
        };
    }

    static string EscapeLike(string s) => s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    /// <summary>
    ///     Applies the in-collection (intersect when <paramref name="intersect"/>=true, exclude when false)
    ///     filter to the given query. Joins per-entity-type into the user's collection tables:
    ///     Books → CollectedBooks, Documents → CollectedDocuments, Magazines → CollectedMagazineIssues
    ///     (via MagazineIssue.MagazineId), Software/SoftwareCompilation → CollectedSoftwareReleases
    ///     (via SoftwareRelease.SoftwareId for Software), Computer/Console/Smartphone → OwnedMachines.
    ///     Other entity types (Company, Gpu, Processor, SoundSynth, Person) are not collectible:
    ///     <list type="bullet">
    ///         <item><description>intersect=true → those rows are filtered out (no possible collection match).</description></item>
    ///         <item><description>intersect=false → those rows are kept (vacuously "not in collection").</description></item>
    ///     </list>
    /// </summary>
    IQueryable<SearchEntry> ApplyCollectionFilter(IQueryable<SearchEntry> q, string userId, bool intersect)
    {
        // Sub-queries for collected ids per entity-type. All return long ids.
        IQueryable<long> bookIds      = context.CollectedBooks.Where(c => c.UserId == userId).Select(c => c.BookId);
        IQueryable<long> documentIds  = context.CollectedDocuments.Where(c => c.UserId == userId).Select(c => c.DocumentId);
        IQueryable<long> magazineIds  = context.CollectedMagazineIssues.Where(c => c.UserId == userId)
                                               .Join(context.MagazineIssues, ci => ci.MagazineIssueId, mi => mi.Id, (_, mi) => mi.MagazineId)
                                               .Distinct();
        IQueryable<long> compilationIds = context.CollectedSoftwareReleases.Where(c => c.UserId == userId)
                                                 .Select(c => (long)c.SoftwareReleaseId);
        IQueryable<long> softwareIds  = context.CollectedSoftwareReleases.Where(c => c.UserId == userId)
                                               .Join(context.SoftwareReleases, c => c.SoftwareReleaseId, r => r.Id,
                                                     (_, r) => r.SoftwareId)
                                               .Where(s => s != null)
                                               .Select(s => (long)s.Value)
                                               .Distinct();
        IQueryable<long> machineIds   = context.OwnedMachines.Where(c => c.UserId == userId).Select(c => (long)c.MachineId);

        if(intersect)
        {
            // Keep rows that match the per-type collection sub-query.
            return q.Where(e =>
                ((byte)e.EntityType == 5  && bookIds.Contains(e.EntityId))           ||
                ((byte)e.EntityType == 6  && documentIds.Contains(e.EntityId))       ||
                ((byte)e.EntityType == 7  && magazineIds.Contains(e.EntityId))       ||
                ((byte)e.EntityType == 12 && softwareIds.Contains(e.EntityId))       ||
                ((byte)e.EntityType == 13 && compilationIds.Contains(e.EntityId))    ||
                (((byte)e.EntityType == 2 || (byte)e.EntityType == 3 || (byte)e.EntityType == 4)
                                          && machineIds.Contains(e.EntityId)));
        }

        // Exclude: drop rows that ARE in the collection. Non-collectible types pass through.
        return q.Where(e =>
            !((byte)e.EntityType == 5  && bookIds.Contains(e.EntityId))           &&
            !((byte)e.EntityType == 6  && documentIds.Contains(e.EntityId))       &&
            !((byte)e.EntityType == 7  && magazineIds.Contains(e.EntityId))       &&
            !((byte)e.EntityType == 12 && softwareIds.Contains(e.EntityId))       &&
            !((byte)e.EntityType == 13 && compilationIds.Contains(e.EntityId))    &&
            !(((byte)e.EntityType == 2 || (byte)e.EntityType == 3 || (byte)e.EntityType == 4)
                                       && machineIds.Contains(e.EntityId)));
    }

    /// <summary>
    ///     Returns the set of (entityType, entityId) pairs the user has collected. Used by the in-process
    ///     filter on the FULLTEXT/Soundex branch to avoid building a separate IQueryable per type.
    /// </summary>
    async Task<HashSet<(byte type, long id)>> GetOwnedKeysAsync(string userId)
    {
        var set = new HashSet<(byte, long)>();
        foreach(long id in await context.CollectedBooks.Where(c => c.UserId == userId).Select(c => c.BookId).ToListAsync())
            set.Add((5, id));
        foreach(long id in await context.CollectedDocuments.Where(c => c.UserId == userId).Select(c => c.DocumentId).ToListAsync())
            set.Add((6, id));
        foreach(long id in await context.CollectedMagazineIssues.Where(c => c.UserId == userId)
                                       .Join(context.MagazineIssues, ci => ci.MagazineIssueId, mi => mi.Id, (_, mi) => mi.MagazineId)
                                       .Distinct().ToListAsync())
            set.Add((7, id));
        // Software (12): map releases → parent software id.
        foreach(long id in await context.CollectedSoftwareReleases.Where(c => c.UserId == userId)
                                       .Join(context.SoftwareReleases, c => c.SoftwareReleaseId, r => r.Id, (_, r) => r.SoftwareId)
                                       .Where(s => s != null).Select(s => (long)s.Value).Distinct().ToListAsync())
            set.Add((12, id));
        // Compilation (13): the release id itself.
        foreach(ulong id in await context.CollectedSoftwareReleases.Where(c => c.UserId == userId).Select(c => c.SoftwareReleaseId).ToListAsync())
            set.Add((13, (long)id));
        foreach(int id in await context.OwnedMachines.Where(c => c.UserId == userId).Select(c => c.MachineId).ToListAsync())
        {
            // We don't know which Machine.Type this id is; add all three slots, the caller filters by entity type.
            set.Add((2, id));
            set.Add((3, id));
            set.Add((4, id));
        }
        return set;
    }
}
