/*******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ---------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] -----------------------------------------------------------
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
// ---------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Server.Helpers;
using Marechai.Server.Services;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;

namespace Marechai.Server.Controllers;

[Route("/software")]
[ApiController]
public class SoftwareController(MarechaiContext context, IMemoryCache cache, UserManager<ApplicationUser> userManager,
                                SoftwareGenreTranslationCache                          genreCache,
                                SoftwareAttributeTranslationCache                      attrCache,
                                Marechai.Server.Services.Rankings.RankingsComputationState rankingsState,
                                Marechai.Server.Services.Rankings.MarechaiRankingsCalculator rankingsCalculator)
    : ControllerBase
{
    // ── Marechai score fairness knobs ────────────────────────────────────────
    // Re-exposed here as local aliases so existing comments and tooling still
    // resolve the names; the canonical values now live on the calculator so the
    // background worker and any future scoring code share a single source of truth.
    const int MARECHAI_RANKING_MIN_CRITIC_COUNT =
        Marechai.Server.Services.Rankings.MarechaiRankingsCalculator.MARECHAI_RANKING_MIN_CRITIC_COUNT;
    const int MARECHAI_RANKING_MIN_USER_COUNT =
        Marechai.Server.Services.Rankings.MarechaiRankingsCalculator.MARECHAI_RANKING_MIN_USER_COUNT;

    // Cache keys + TTL for the /software landing-page catalog endpoints.
    // These query the *whole* catalog (all genres, all platforms, all spec
    // values, year range) and barely change between requests, so a short
    // memory cache turns 4 expensive queries into ~0 ms hits.
    const           string   SOFTWARE_GENRES_CACHE_KEY    = "software:genres";
    const           string   SOFTWARE_ALL_GENRES_CACHE_KEY = "software:genres:all";
    internal const  string   SOFTWARE_SPECS_CACHE_KEY     = "software:specs";
    const           string   SOFTWARE_PLATFORMS_CACHE_KEY = "software:platforms";
    const           string   SOFTWARE_YEARS_CACHE_KEY     = "software:years";
    const           string   SOFTWARE_COUNT_CACHE_KEY     = "software:count";
    const           string   SOFTWARE_ADMIN_COUNT_CACHE_KEY = "admin:software:count";
    const           string   SOFTWARE_COMPANIES_CACHE_KEY = "software:companies";
    static readonly TimeSpan _catalogCacheTtl             = TimeSpan.FromMinutes(5);

    // Compilations have no Kind column; we treat them as if they were Game-kind
    // when filtering, since the overwhelming majority of compilations are game
    // collections. With no kind filter (or kind=Game) compilations are included;
    // any other kind excludes them.
    static bool ShouldIncludeCompilations(SoftwareKind? kind) =>
        kind is null or SoftwareKind.Game;

    /// <summary>
    /// Backfills <see cref="SoftwareDto.FrontCoverId"/> on a materialized page of
    /// software/compilation rows in two bounded SQL round-trips, replacing the
    /// per-row correlated subquery that EF Core used to emit. With take=24 the
    /// old shape fired 24 correlated lookups across SoftwareCovers ⋈ SoftwareReleases
    /// ⋈ SoftwareVersions and could not use a single index because of the
    /// (Release.SoftwareId OR Release.SoftwareVersion.SoftwareId) OR clause.
    /// The replacement uses two index-friendly IN/GROUP BY queries (direct
    /// release link + indirect via version) plus a third for compilations, all
    /// bounded by the paginated id set.
    /// </summary>
    async Task PopulateFrontCoverIdsAsync(List<SoftwareDto> list, CancellationToken ct)
    {
        if(list.Count == 0) return;

        List<ulong> softwareIds = list.Where(d => !d.IsCompilation)
                                      .Select(d => d.Id)
                                      .Distinct()
                                      .ToList();

        List<ulong> compilationIds = list.Where(d => d.IsCompilation)
                                         .Select(d => d.Id)
                                         .Distinct()
                                         .ToList();

        var softwareCovers = new Dictionary<ulong, Guid>();

        if(softwareIds.Count > 0)
        {
            // SoftwareCovers.SoftwareId is the direct, always-populated link to the owning
            // Software, regardless of whether (or which) release the cover has been attached
            // to — no need to traverse Release/SoftwareVersion to find it.
            var rows = await context.SoftwareCovers
                                    .Where(sc => sc.Type == SoftwareCoverType.Front &&
                                                 sc.SoftwareId.HasValue &&
                                                 softwareIds.Contains(sc.SoftwareId.Value))
                                    .Select(sc => new
                                     {
                                         SoftwareId = sc.SoftwareId.Value,
                                         CoverId = sc.Id
                                     })
                                    .ToListAsync(ct);

            foreach(IGrouping<ulong, Guid> g in rows.GroupBy(x => x.SoftwareId, x => x.CoverId))
                softwareCovers[g.Key] = g.Min();
        }

        var compilationCovers = new Dictionary<ulong, Guid>();

        if(compilationIds.Count > 0)
        {
            // Compilation rows carry the SoftwareCompilation.Id as their dto Id, so look up
            // covers directly via SoftwareCovers.SoftwareCompilationId (single indexed column).
            var rows = await context.SoftwareCovers
                                    .Where(sc => sc.Type == SoftwareCoverType.Front &&
                                                 sc.SoftwareCompilationId.HasValue &&
                                                 compilationIds.Contains(sc.SoftwareCompilationId.Value))
                                    .Select(sc => new
                                     {
                                         CompilationId = sc.SoftwareCompilationId.Value,
                                         CoverId       = sc.Id
                                     })
                                    .ToListAsync(ct);

            foreach(IGrouping<ulong, Guid> g in rows.GroupBy(x => x.CompilationId, x => x.CoverId))
                compilationCovers[g.Key] = g.Min();
        }

        foreach(SoftwareDto dto in list)
        {
            if(dto.IsCompilation)
            {
                if(compilationCovers.TryGetValue(dto.Id, out Guid coverId))
                    dto.FrontCoverId = coverId;
            }
            else
            {
                if(softwareCovers.TryGetValue(dto.Id, out Guid coverId))
                    dto.FrontCoverId = coverId;
            }
        }
    }

    static string CountCacheKey(SoftwareKind? kind) =>
        kind is null ? SOFTWARE_COUNT_CACHE_KEY : $"{SOFTWARE_COUNT_CACHE_KEY}:{(int)kind.Value}";

    static string AdminCountCacheKey(SoftwareKind? kind) =>
        kind is null
            ? SOFTWARE_ADMIN_COUNT_CACHE_KEY
            : $"{SOFTWARE_ADMIN_COUNT_CACHE_KEY}:{(int)kind.Value}";

    /// <summary>
    ///     EF projection shape for the orphan-addons grid. Internal record so the
    ///     <c>IQueryable&lt;AddonRow&gt;</c> sort switch in
    ///     <see cref="GetAdminAddonsPagedAsync"/> can name the type (anonymous-type
    ///     projections defeat the sortBy switch's branch unification).
    /// </summary>
    sealed class AddonRow
    {
        public ulong         Id             { get; set; }
        public string        Name           { get; set; }
        public SoftwareKind  Kind           { get; set; }
        public ulong?        BaseSoftwareId { get; set; }
        public string        BaseName       { get; set; }
        public bool          HasDlcGenre    { get; set; }
        public int           ReasonCode     { get; set; }
    }

    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetSoftwareCountAsync([FromQuery] string search = null,
                                                 [FromQuery] SoftwareKind? kind = null)
    {
        bool   includeCompilations = ShouldIncludeCompilations(kind);
        string cacheKey            = CountCacheKey(kind);

        // Fast path: no search filter — use the cached total.
        if(string.IsNullOrWhiteSpace(search))
        {
            if(cache.TryGetValue(cacheKey, out int cachedCount)) return cachedCount;

            IQueryable<Database.Models.Software> baseQuery = context.Softwares;

            if(kind.HasValue) baseQuery = baseQuery.Where(s => s.Kind == kind.Value);

            int softwareTotal     = await baseQuery.CountAsync();
            int compilationsTotal = includeCompilations
                                        ? await context.SoftwareCompilations.CountAsync()
                                        : 0;

            int total = softwareTotal + compilationsTotal;
            cache.Set(cacheKey, total, _catalogCacheTtl);

            return total;
        }

        IQueryable<Database.Models.Software> query = context.Softwares;
        query = query.Where(s => s.Name.Contains(search));

        if(kind.HasValue) query = query.Where(s => s.Kind == kind.Value);

        int softwareCount = await query.CountAsync();

        if(!includeCompilations) return softwareCount;

        IQueryable<SoftwareCompilation> compQuery =
            context.SoftwareCompilations.Where(c => c.Name.Contains(search));

        int compilationCount = await compQuery.CountAsync();

        return softwareCount + compilationCount;
    }

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [OutputCache(Duration = 300)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMinimumYearAsync()
    {
        (int min, _) = await GetYearRangeAsync();

        return min;
    }

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [OutputCache(Duration = 300)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMaximumYearAsync()
    {
        (_, int max) = await GetYearRangeAsync();

        return max;
    }

    /// <summary>
    /// Returns the (min, max) ReleaseDate year across all releases, computed in
    /// a single round-trip and cached. The Index page asks for both and they
    /// only change when a release is added/edited, so 5 min caching is safe.
    /// </summary>
    async Task<(int Min, int Max)> GetYearRangeAsync()
    {
        if(cache.TryGetValue(SOFTWARE_YEARS_CACHE_KEY, out (int Min, int Max) cached)) return cached;

        var range = await context.SoftwareReleases
                                 .Where(r => r.ReleaseDate.HasValue && r.ReleaseDate.Value.Year > 1000)
                                 .GroupBy(_ => 1)
                                 .Select(g => new
                                  {
                                      Min = g.Min(r => r.ReleaseDate.Value.Year),
                                      Max = g.Max(r => r.ReleaseDate.Value.Year)
                                  })
                                 .FirstOrDefaultAsync();

        var result = range is null ? (0, 0) : (range.Min, range.Max);
        cache.Set(SOFTWARE_YEARS_CACHE_KEY, result, _catalogCacheTtl);

        return result;
    }

    [HttpGet("by-letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareByLetterAsync(char c,
                                                                  [FromQuery] SoftwareKind? kind = null,
                                                                  [FromQuery] int? skip = null,
                                                                  [FromQuery] int? take = null,
                                                                  CancellationToken cancellationToken = default)
    {
        IQueryable<SoftwareDto> combined = BuildByLetterQuery(c, kind);

        IQueryable<SoftwareDto> ordered = combined.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);

        if(take.HasValue) ordered = ordered.Take(take.Value);

        List<SoftwareDto> list = await ordered.ToListAsync(cancellationToken);

        await PopulateFrontCoverIdsAsync(list, cancellationToken);

        return list;
    }

    [HttpGet("by-letter/{c}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetSoftwareByLetterCountAsync(char c, [FromQuery] SoftwareKind? kind = null,
                                                   CancellationToken cancellationToken = default) =>
        BuildByLetterQuery(c, kind).CountAsync(cancellationToken);

    IQueryable<SoftwareDto> BuildByLetterQuery(char c, SoftwareKind? kind)
    {
        // Single SQL round-trip: UNION ALL the software + compilations projection
        // (was two sequential awaited queries before merging in memory).
        IQueryable<Database.Models.Software> baseSoftwareQuery =
            context.Softwares.Where(s => EF.Functions.Like(s.Name, $"{c}%"));

        if(kind.HasValue) baseSoftwareQuery = baseSoftwareQuery.Where(s => s.Kind == kind.Value);

        IQueryable<SoftwareDto> softwareQuery = baseSoftwareQuery.Select(s => new SoftwareDto
        {
            Id            = s.Id,
            Name          = s.Name,
            FamilyId      = s.FamilyId,
            Family        = s.Family.Name,
            Kind          = s.Kind,
            IsCompilation = false
        });

        if(!ShouldIncludeCompilations(kind)) return softwareQuery;

        IQueryable<SoftwareDto> compilationsQuery = context.SoftwareCompilations
           .Where(comp => EF.Functions.Like(comp.Name, $"{c}%"))
           .Select(comp => new SoftwareDto
            {
                Id            = comp.Id,
                Name          = comp.Name,
                FamilyId      = null,
                Family        = null,
                Kind          = default,
                IsCompilation = true
            });

        return softwareQuery.Concat(compilationsQuery);
    }

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareByYearAsync(int year,
                                                                [FromQuery] SoftwareKind? kind = null,
                                                                [FromQuery] int? skip = null,
                                                                [FromQuery] int? take = null,
                                                                CancellationToken cancellationToken = default)
    {
        IQueryable<SoftwareDto> combined = BuildByYearQuery(year, kind);

        IQueryable<SoftwareDto> ordered = combined.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);

        if(take.HasValue) ordered = ordered.Take(take.Value);

        List<SoftwareDto> list = await ordered.ToListAsync(cancellationToken);

        await PopulateFrontCoverIdsAsync(list, cancellationToken);

        return list;
    }

    [HttpGet("by-year/{year:int}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetSoftwareByYearCountAsync(int year, [FromQuery] SoftwareKind? kind = null,
                                                 CancellationToken cancellationToken = default) =>
        BuildByYearQuery(year, kind).CountAsync(cancellationToken);

    IQueryable<SoftwareDto> BuildByYearQuery(int year, SoftwareKind? kind)
    {
        // Single SQL round-trip via Concat (UNION ALL).
        IQueryable<Database.Models.Software> baseSoftwareQuery = context.Softwares
           .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.ReleaseDate != null &&
                                                               r.ReleaseDate.Value.Year == year))
                    || s.DirectReleases.Any(r => r.ReleaseDate != null &&
                                                 r.ReleaseDate.Value.Year == year));

        if(kind.HasValue) baseSoftwareQuery = baseSoftwareQuery.Where(s => s.Kind == kind.Value);

        IQueryable<SoftwareDto> softwareQuery = baseSoftwareQuery.Select(s => new SoftwareDto
        {
            Id            = s.Id,
            Name          = s.Name,
            FamilyId      = s.FamilyId,
            Family        = s.Family.Name,
            Kind          = s.Kind,
            IsCompilation = false
        });

        if(!ShouldIncludeCompilations(kind)) return softwareQuery;

        IQueryable<SoftwareDto> compilationsQuery = context.SoftwareCompilations
           .Where(comp => comp.Releases.Any(r => r.ReleaseDate != null && r.ReleaseDate.Value.Year == year))
           .Select(comp => new SoftwareDto
            {
                Id            = comp.Id,
                Name          = comp.Name,
                FamilyId      = null,
                Family        = null,
                Kind          = default,
                IsCompilation = true
            });

        return softwareQuery.Concat(compilationsQuery);
    }

    [HttpGet("by-platform/{platformId:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareByPlatformAsync(ulong platformId,
                                                                    [FromQuery] SoftwareKind? kind = null,
                                                                    [FromQuery] int? skip = null,
                                                                    [FromQuery] int? take = null,
                                                                    CancellationToken cancellationToken = default)
    {
        IQueryable<SoftwareDto> combined = BuildByPlatformQuery(platformId, kind);

        IQueryable<SoftwareDto> ordered = combined.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);

        if(take.HasValue) ordered = ordered.Take(take.Value);

        List<SoftwareDto> list = await ordered.ToListAsync(cancellationToken);

        await PopulateFrontCoverIdsAsync(list, cancellationToken);

        return list;
    }

    [HttpGet("by-platform/{platformId:ulong}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetSoftwareByPlatformCountAsync(ulong platformId, [FromQuery] SoftwareKind? kind = null,
                                                     CancellationToken cancellationToken = default) =>
        BuildByPlatformQuery(platformId, kind).CountAsync(cancellationToken);

    IQueryable<SoftwareDto> BuildByPlatformQuery(ulong platformId, SoftwareKind? kind)
    {
        // Single SQL round-trip via Concat (UNION ALL).
        IQueryable<Database.Models.Software> baseSoftwareQuery = context.Softwares
           .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.PlatformId == platformId))
                    || s.DirectReleases.Any(r => r.PlatformId == platformId));

        if(kind.HasValue) baseSoftwareQuery = baseSoftwareQuery.Where(s => s.Kind == kind.Value);

        IQueryable<SoftwareDto> softwareQuery = baseSoftwareQuery.Select(s => new SoftwareDto
        {
            Id            = s.Id,
            Name          = s.Name,
            FamilyId      = s.FamilyId,
            Family        = s.Family.Name,
            Kind          = s.Kind,
            IsCompilation = false
        });

        if(!ShouldIncludeCompilations(kind)) return softwareQuery;

        IQueryable<SoftwareDto> compilationsQuery = context.SoftwareCompilations
           .Where(comp => comp.Releases.Any(r => r.PlatformId == platformId))
           .Select(comp => new SoftwareDto
            {
                Id            = comp.Id,
                Name          = comp.Name,
                FamilyId      = null,
                Family        = null,
                Kind          = default,
                IsCompilation = true
            });

        return softwareQuery.Concat(compilationsQuery);
    }

    [HttpGet("companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<CompanyDto>> GetSoftwareCompaniesAsync()
    {
        if(cache.TryGetValue(SOFTWARE_COMPANIES_CACHE_KEY, out List<CompanyDto> cached) && cached is not null)
            return cached;

        List<CompanyDto> companies = await context.SoftwareCompanyRoles
                                                  .Select(cr => cr.Company)
                                                  .Union(context.SoftwareReleases.Select(sr => sr.Publisher))
                                                  .Distinct()
                                                  .Include(c => c.Logos)
                                                  .OrderBy(c => MarechaiContext.NaturalSortKey(c.Name))
                                                  .Select(c => new CompanyDto
                                                   {
                                                       Id       = c.Id,
                                                       LastLogo = c.Logos.OrderByDescending(l => l.Year)
                                                                         .FirstOrDefault().Guid,
                                                       Name     = c.Name
                                                   })
                                                  .ToListAsync();

        cache.Set(SOFTWARE_COMPANIES_CACHE_KEY, companies, _catalogCacheTtl);

        return companies;
    }

    [HttpGet("companies/letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<CompanyDto>> GetSoftwareCompaniesByLetterAsync(char c)
    {
        // Filter from the cached full list when available — avoids hitting the
        // DB for the per-letter pages.
        if(cache.TryGetValue(SOFTWARE_COMPANIES_CACHE_KEY, out List<CompanyDto> cached) && cached is not null)
            return cached.Where(co => !string.IsNullOrEmpty(co.Name) && char.ToUpperInvariant(co.Name[0]) ==
                                          char.ToUpperInvariant(c))
                         .ToList();

        return await context.SoftwareCompanyRoles
                            .Select(cr => cr.Company)
                            .Union(context.SoftwareReleases.Select(sr => sr.Publisher))
                            .Distinct()
                            .Include(co2 => co2.Logos)
                            .Where(co => EF.Functions.Like(co.Name, $"{c}%"))
                            .OrderBy(co => MarechaiContext.NaturalSortKey(co.Name))
                            .Select(co => new CompanyDto
                             {
                                 Id       = co.Id,
                                 LastLogo = co.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
                                 Name     = co.Name
                             })
                            .ToListAsync();
    }

    [HttpGet("/software/{softwareId:ulong}/companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareCompanyRoleDto>> GetCompaniesAsync(ulong softwareId) => context.SoftwareCompanyRoles
       .Where(cr => cr.SoftwareId == softwareId)
       .Select(cr => new SoftwareCompanyRoleDto
        {
            SoftwareId = cr.SoftwareId,
            Software   = cr.Software.Name,
            CompanyId  = cr.CompanyId,
            Company    = cr.Company.Name,
            RoleId     = cr.RoleId,
            Role       = cr.Role.Name
        })
       .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetAsync([FromQuery] int? skip   = null, [FromQuery] int? take = null,
                                                  [FromQuery] string search = null,
                                                  [FromQuery] string sortBy = null,
                                                  [FromQuery] bool sortDescending = false,
                                                  [FromQuery] SoftwareKind? kind = null,
                                                  CancellationToken cancellationToken = default)
    {
        IQueryable<SoftwareDto> combined = BuildAllSoftwareQuery(search, kind);

        // Sort + paginate at the SQL level (was: ToListAsync + in-memory
        // OrderBy(NaturalStringComparer)+Skip+Take). NaturalSortKey() is a
        // MariaDB function shipped via the AddNaturalSortKeyFunction migration.
        // Note: the projection does not populate BaseSoftware, so the
        // sortBy="BaseSoftware" branch was effectively a no-op (null-sort);
        // we keep that behavior by falling back to Name.
        IQueryable<SoftwareDto> ordered = sortBy switch
        {
            "Name"   => sortDescending ? combined.OrderByDescending(s => MarechaiContext.NaturalSortKey(s.Name))
                                       : combined.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name)),
            "Family" => sortDescending ? combined.OrderByDescending(s => MarechaiContext.NaturalSortKey(s.Family))
                                       : combined.OrderBy(s => MarechaiContext.NaturalSortKey(s.Family)),
            "Kind"   => sortDescending ? combined.OrderByDescending(s => s.Kind)
                                       : combined.OrderBy(s => s.Kind),
            _        => combined.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
        };

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);

        if(take.HasValue) ordered = ordered.Take(take.Value);

        List<SoftwareDto> list = await ordered.ToListAsync(cancellationToken);

        await PopulateFrontCoverIdsAsync(list, cancellationToken);

        return list;
    }

    IQueryable<SoftwareDto> BuildAllSoftwareQuery(string search, SoftwareKind? kind)
    {
        IQueryable<Database.Models.Software> baseSoftwareQuery = context.Softwares;

        if(!string.IsNullOrWhiteSpace(search))
            baseSoftwareQuery = baseSoftwareQuery.Where(s => s.Name.Contains(search));

        if(kind.HasValue)
            baseSoftwareQuery = baseSoftwareQuery.Where(s => s.Kind == kind.Value);

        IQueryable<SoftwareDto> softwareQuery = baseSoftwareQuery.Select(s => new SoftwareDto
        {
            Id            = s.Id,
            Name          = s.Name,
            FamilyId      = s.FamilyId,
            Family        = s.Family.Name,
            Kind          = s.Kind,
            IsCompilation = false
        });

        if(!ShouldIncludeCompilations(kind)) return softwareQuery;

        IQueryable<SoftwareCompilation> baseCompQuery = context.SoftwareCompilations;

        if(!string.IsNullOrWhiteSpace(search))
            baseCompQuery = baseCompQuery.Where(comp => comp.Name.Contains(search));

        IQueryable<SoftwareDto> compilationsQuery = baseCompQuery.Select(comp => new SoftwareDto
        {
            Id            = comp.Id,
            Name          = comp.Name,
            FamilyId      = null,
            Family        = null,
            Kind          = default,
            IsCompilation = true
        });

        // Single SQL round-trip via Concat (UNION ALL).
        return softwareQuery.Concat(compilationsQuery);
    }

    /// <summary>
    /// Admin-only paged list used by /admin/software. Lean projection
    /// (Id/Name/FamilyId/Family/Kind only) — no FrontCoverId sub-query, no
    /// compilations UNION. Significantly faster than <see cref="GetAsync"/>
    /// because the public endpoint pays for cover lookups + compilations
    /// merging that the admin grid never displays.
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin, UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetAdminPagedAsync([FromQuery] int? skip   = null,
                                                      [FromQuery] int? take = null,
                                                      [FromQuery] string search = null,
                                                      [FromQuery] string sortBy = null,
                                                      [FromQuery] bool sortDescending = false,
                                                      [FromQuery] SoftwareKind? kind = null,
                                                      CancellationToken cancellationToken = default)
    {
        IQueryable<Database.Models.Software> baseQuery = context.Softwares;

        if(!string.IsNullOrWhiteSpace(search)) baseQuery = baseQuery.Where(s => s.Name.Contains(search));

        if(kind.HasValue) baseQuery = baseQuery.Where(s => s.Kind == kind.Value);

        IQueryable<SoftwareDto> projected = baseQuery.Select(s => new SoftwareDto
        {
            Id            = s.Id,
            Name          = s.Name,
            FamilyId      = s.FamilyId,
            Family        = s.Family.Name,
            Kind          = s.Kind,
            IsCompilation = false,
            FrontCoverId  = null
        });

        IQueryable<SoftwareDto> ordered = sortBy switch
        {
            "Name"   => sortDescending ? projected.OrderByDescending(s => MarechaiContext.NaturalSortKey(s.Name))
                                       : projected.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name)),
            "Family" => sortDescending ? projected.OrderByDescending(s => MarechaiContext.NaturalSortKey(s.Family))
                                       : projected.OrderBy(s => MarechaiContext.NaturalSortKey(s.Family)),
            "Kind"   => sortDescending ? projected.OrderByDescending(s => s.Kind)
                                       : projected.OrderBy(s => s.Kind),
            _        => projected.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
        };

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);

        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Admin-only count companion to <see cref="GetAdminPagedAsync"/>.
    /// Software-only (no compilations); cached separately from the public
    /// <see cref="GetSoftwareCountAsync"/> because the totals differ.
    /// </summary>
    [HttpGet("admin/count")]
    [Authorize(Roles = "Admin, UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetAdminSoftwareCountAsync([FromQuery] string search = null,
                                                      [FromQuery] SoftwareKind? kind = null,
                                                      CancellationToken cancellationToken = default)
    {
        string cacheKey = AdminCountCacheKey(kind);

        // Fast path: no search filter — use the cached total.
        if(string.IsNullOrWhiteSpace(search))
        {
            if(cache.TryGetValue(cacheKey, out int cachedCount)) return cachedCount;

            IQueryable<Database.Models.Software> baseQuery = context.Softwares;

            if(kind.HasValue) baseQuery = baseQuery.Where(s => s.Kind == kind.Value);

            int total = await baseQuery.CountAsync(cancellationToken);
            cache.Set(cacheKey, total, _catalogCacheTtl);

            return total;
        }

        IQueryable<Database.Models.Software> query = context.Softwares.Where(s => s.Name.Contains(search));

        if(kind.HasValue) query = query.Where(s => s.Kind == kind.Value);

        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Builds the base Software query used by both <see cref="GetAdminDuplicateGroupsAsync"/>
    /// and <see cref="GetAdminDuplicateGroupsCountAsync"/>. Filters by <paramref name="kind"/>
    /// (when set) and excludes <see cref="SoftwareKind.Dlc"/> when <paramref name="excludeDlc"/>
    /// is true. The kind filter and the excludeDlc flag are independent — when a specific Kind
    /// is selected, excludeDlc has no effect (the Kind filter already isolates the chosen kind).
    /// </summary>
    IQueryable<Database.Models.Software> BuildDuplicateBaseQuery(SoftwareKind? kind, bool excludeDlc)
    {
        IQueryable<Database.Models.Software> baseQuery = context.Softwares;

        if(kind.HasValue)
            baseQuery = baseQuery.Where(s => s.Kind == kind.Value);
        else if(excludeDlc)
            baseQuery = baseQuery.Where(s => s.Kind != SoftwareKind.Dlc);

        return baseQuery;
    }

    /// <summary>
    /// Admin-only paged list of pseudoduplicate Software groups, used by
    /// /admin/software/duplicates. Two Software rows are pseudoduplicates when
    /// <see cref="MarechaiContext.NormalizeForDuplicate"/> returns the same key —
    /// the SQL function strips every parenthesised and bracketed group, collapses
    /// whitespace and lower-cases, so "Game", "Game", "Game (Limited Edition)"
    /// and "Game (Collector's Edition)" all share the key "game".
    /// </summary>
    /// <remarks>
    /// Pagination applies to the *groups*, not the items. The grid in
    /// /admin/software/duplicates pages over groups directly.
    /// </remarks>
    [HttpGet("admin/duplicates")]
    [Authorize(Roles = "Admin, UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDuplicateGroupDto>> GetAdminDuplicateGroupsAsync(
        [FromQuery] int? skip = null,
        [FromQuery] int? take = null,
        [FromQuery] SoftwareKind? kind = null,
        [FromQuery] bool excludeDlc = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Database.Models.Software> baseQuery = BuildDuplicateBaseQuery(kind, excludeDlc);

        // Step 1: materialise the list of duplicate normalisation keys we want to
        // show on this page. GROUP BY NormalizeForDuplicate(Name) HAVING COUNT > 1.
        IQueryable<string> groupKeysQuery = baseQuery
            .GroupBy(s => MarechaiContext.NormalizeForDuplicate(s.Name))
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .OrderBy(k => k);

        if(skip.HasValue) groupKeysQuery = (IOrderedQueryable<string>)groupKeysQuery.Skip(skip.Value);

        if(take.HasValue) groupKeysQuery = (IOrderedQueryable<string>)groupKeysQuery.Take(take.Value);

        List<string> pageKeys = await groupKeysQuery.ToListAsync(cancellationToken);

        if(pageKeys.Count == 0) return [];

        HashSet<string> pageKeySet = pageKeys.ToHashSet();

        // Step 2: pull every Software whose normalised key falls on this page, in one query.
        var rows = await baseQuery
                        .Where(s => pageKeySet.Contains(MarechaiContext.NormalizeForDuplicate(s.Name)))
                        .Select(s => new
                         {
                             NormalizedName = MarechaiContext.NormalizeForDuplicate(s.Name),
                             s.Id,
                             s.Name,
                             s.Kind
                         })
                        .ToListAsync(cancellationToken);

        ulong[] softwareIds = rows.Select(r => r.Id).Distinct().ToArray();

        // Step 3: per-software release aggregates — count, earliest date, platforms.
        var releaseAggs = await context.SoftwareReleases
                                       .Where(r => r.SoftwareId.HasValue && softwareIds.Contains(r.SoftwareId.Value))
                                       .GroupBy(r => r.SoftwareId!.Value)
                                       .Select(g => new
                                        {
                                            SoftwareId   = g.Key,
                                            Count        = g.Count(),
                                            EarliestDate = g.Min(r => r.ReleaseDate)
                                        })
                                       .ToDictionaryAsync(g => g.SoftwareId, cancellationToken);

        // Step 4: distinct platform names per software (separate query to avoid a
        // GROUP_CONCAT-shaped projection that EF Core/Pomelo translates inconsistently).
        var platformsBySoftware = (await context.SoftwareReleases
                                                .Where(r => r.SoftwareId.HasValue          &&
                                                            softwareIds.Contains(r.SoftwareId.Value) &&
                                                            r.PlatformId != null)
                                                .Select(r => new
                                                 {
                                                     SoftwareId   = r.SoftwareId!.Value,
                                                     PlatformName = r.Platform.Name
                                                 })
                                                .Distinct()
                                                .ToListAsync(cancellationToken))
           .GroupBy(p => p.SoftwareId)
           .ToDictionary(g => g.Key,
                         g => g.Select(p => p.PlatformName)
                               .Where(n => !string.IsNullOrWhiteSpace(n))
                               .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                               .ToList());

        // Step 5: assemble groups in the same order as Step 1.
        const int maxPlatformNamesShown = 5;
        Dictionary<string, List<SoftwareDuplicateItemDto>> itemsByKey = new(StringComparer.Ordinal);

        foreach(var row in rows)
        {
            string platforms = string.Empty;
            if(platformsBySoftware.TryGetValue(row.Id, out List<string> platformList) && platformList.Count > 0)
            {
                if(platformList.Count <= maxPlatformNamesShown)
                    platforms = string.Join(", ", platformList);
                else
                    platforms = string.Join(", ", platformList.Take(maxPlatformNamesShown)) +
                                $" +{platformList.Count - maxPlatformNamesShown} more";
            }

            int? earliestYear = null;
            int  count        = 0;
            if(releaseAggs.TryGetValue(row.Id, out var agg))
            {
                count        = agg.Count;
                earliestYear = agg.EarliestDate?.Year;
            }

            var item = new SoftwareDuplicateItemDto
            {
                Id                  = row.Id,
                Name                = row.Name,
                Kind                = row.Kind,
                EarliestReleaseYear = earliestYear,
                ReleasesCount       = count,
                Platforms           = platforms
            };

            if(!itemsByKey.TryGetValue(row.NormalizedName, out List<SoftwareDuplicateItemDto> bucket))
            {
                bucket                          = [];
                itemsByKey[row.NormalizedName] = bucket;
            }

            bucket.Add(item);
        }

        return pageKeys.Select(k => new SoftwareDuplicateGroupDto
                        {
                            NormalizedName = k,
                            Items = itemsByKey.TryGetValue(k, out List<SoftwareDuplicateItemDto> items)
                                        ? items.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToList()
                                        : []
                        })
                       .ToList();
    }

    /// <summary>
    /// Count of pseudoduplicate Software groups under the same filters as
    /// <see cref="GetAdminDuplicateGroupsAsync"/>. Not cached — admins expect the
    /// number to drop as soon as they complete a merge.
    /// </summary>
    [HttpGet("admin/duplicates/count")]
    [Authorize(Roles = "Admin, UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetAdminDuplicateGroupsCountAsync(
        [FromQuery] SoftwareKind? kind = null,
        [FromQuery] bool excludeDlc = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Database.Models.Software> baseQuery = BuildDuplicateBaseQuery(kind, excludeDlc);

        return baseQuery.GroupBy(s => MarechaiContext.NormalizeForDuplicate(s.Name))
                        .Where(g => g.Count() > 1)
                        .CountAsync(cancellationToken);
    }

    // -----------------------------------------------------------------------
    // /admin/software/orphan-addons backend.
    //
    // Surfaces two row categories that should be linked to a base software:
    //   1) Kind == Dlc rows whose BaseSoftwareId is broken.
    //   2) Kind == Game rows that carry a "DLC / add-on" genre — misclassified
    //      at import time (the MobyGames CLI fixup in
    //      Marechai.MobyGames/Services/DlcRelationService.cs handles these in
    //      batch; this UI does the same job interactively).
    //
    // Five endpoints:
    //   GET   /software/admin/addons              (paged grid)
    //   GET   /software/admin/addons/count        (grid total)
    //   GET   /software/admin/addons/candidates   (autocomplete inside dialog)
    //   PATCH /software/{id}/base-software        (single link)
    //   PATCH /software/admin/addons/base-software-bulk  (bulk link)
    // -----------------------------------------------------------------------

    const           string   ADMIN_DLC_GENRE_IDS_CACHE_KEY = "admin.addons.dlc-genre-ids";
    static readonly TimeSpan _adminAddonGenreCacheTtl       = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     Returns the set of <see cref="SoftwareGenre"/> ids whose name marks a
    ///     row as a DLC / add-on. Mirrors the predicate in
    ///     <c>Marechai.MobyGames/Services/DlcRelationService.cs</c> lines 32‑37
    ///     (the two <c>Contains</c> clauses cover the U+00A0 variant from old
    ///     MobyGames HTML). Cached for <see cref="_adminAddonGenreCacheTtl"/>
    ///     since these ids change at most when seed data is updated.
    /// </summary>
    async Task<int[]> GetDlcGenreIdsAsync(CancellationToken cancellationToken)
    {
        if(cache.TryGetValue(ADMIN_DLC_GENRE_IDS_CACHE_KEY, out int[] cached)) return cached;

        int[] ids = await context.SoftwareGenres
                                 .Where(g => g.Name.Contains("DLC") && g.Name.Contains("add-on") ||
                                             g.Name == "Add-on")
                                 .Select(g => g.Id)
                                 .ToArrayAsync(cancellationToken);

        cache.Set(ADMIN_DLC_GENRE_IDS_CACHE_KEY, ids, _adminAddonGenreCacheTtl);

        return ids;
    }

    /// <summary>
    ///     Server-side prefix normaliser that mirrors the MariaDB
    ///     <c>NormalizeForMatch</c> function: lowercases, replaces every
    ///     non-alphanumeric / non-whitespace char with a space, then collapses
    ///     whitespace runs. Used to canonicalise the <c>prefix</c> query
    ///     parameter before passing it through a server-evaluated comparison.
    /// </summary>
    static string NormalizePrefixForMatch(string input)
    {
        if(string.IsNullOrWhiteSpace(input)) return string.Empty;

        var sb = new StringBuilder(input.Length);

        foreach(char c in input)
        {
            if(char.IsLetterOrDigit(c))
                sb.Append(char.ToLowerInvariant(c));
            else
                sb.Append(' ');
        }

        return string.Join(' ',
                           sb.ToString()
                             .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    /// <summary>
    ///     Escapes MariaDB <c>LIKE</c> metacharacters (<c>%</c>, <c>_</c>,
    ///     backslash) so a user-supplied prefix is matched as a literal string
    ///     prefix rather than a wildcard pattern. The default <c>LIKE</c>
    ///     escape char in MariaDB is backslash.
    /// </summary>
    static string EscapeLikePrefix(string input) =>
        input.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    /// <summary>
    ///     Builds the union predicate for the orphan-addons grid: every
    ///     <see cref="SoftwareKind.Dlc"/> row (linked or not, broken or not)
    ///     plus every <see cref="SoftwareKind.Game"/> row that carries a
    ///     DLC / add-on genre. The optional <paramref name="onlyOrphans"/>
    ///     argument tightens the predicate to only rows whose computed
    ///     <see cref="AddonOrphanReason"/> would be non-<see cref="AddonOrphanReason.Linked"/>.
    /// </summary>
    /// <remarks>
    ///     The DLC genre predicate (the <c>EXISTS</c> sub-select on
    ///     <c>GenresBySoftware</c>) is repeated in both the base filter and the
    ///     projection's <c>HasDlcGenre</c> column. Pomelo / EF Core does NOT
    ///     CSE the two identical sub-queries, so the SQL plan executes the
    ///     genre lookup twice per row. Acceptable for an admin-only page on a
    ///     dataset of a few thousand DLCs; if it becomes hot, materialise the
    ///     genre set into a CTE.
    /// </remarks>
    IQueryable<Database.Models.Software> BuildOrphanQuery(int[] dlcGenreIds, bool onlyOrphans)
    {
        IQueryable<Database.Models.Software> baseQuery = context.Softwares
            .Where(s => s.Kind == SoftwareKind.Dlc ||
                        s.Kind == SoftwareKind.Game && context.GenresBySoftware
                                                              .Any(g => g.SoftwareId == s.Id &&
                                                                        dlcGenreIds.Contains(g.GenreId)));

        if(!onlyOrphans) return baseQuery;

        // A DLC's name is considered to "match" its base game when, after
        // NormalizeForMatch (which strips colons / dashes / punctuation and
        // lowercases), the DLC name either equals the base name or starts
        // with the base name followed by a whitespace boundary. Equality with
        // the base name alone never holds for legitimate DLCs (they always
        // add a suffix like " Acceleration Pack") so the previous equality
        // check flagged *every* correctly-linked DLC as a name mismatch.
        // Using `base + ' '` as a LIKE prefix avoids the false positive where
        // `Sonic` would otherwise be a prefix of `Sonicate Deluxe`.
        return baseQuery.Where(s => s.Kind == SoftwareKind.Game ||
                                    s.Kind == SoftwareKind.Dlc &&
                                    (s.BaseSoftwareId == null ||
                                     s.BaseSoftwareId == s.Id ||
                                     s.BaseSoftware == null ||
                                     s.BaseSoftware.Kind == SoftwareKind.Dlc ||
                                     MarechaiContext.NormalizeForMatch(s.Name) !=
                                     MarechaiContext.NormalizeForMatch(s.BaseSoftware.Name) &&
                                     !EF.Functions.Like(MarechaiContext.NormalizeForMatch(s.Name),
                                                        MarechaiContext.NormalizeForMatch(s.BaseSoftware.Name) +
                                                        " %")));
    }

    /// <summary>
    ///     Admin-only paged list backing <c>/admin/software/orphan-addons</c>.
    ///     Returns <see cref="SoftwareAddonDto"/> rows with a server-computed
    ///     <see cref="AddonOrphanReason"/> column suitable for the grid's
    ///     orphan-reason chip.
    /// </summary>
    [HttpGet("admin/addons")]
    [Authorize(Roles = "Admin, UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareAddonDto>> GetAdminAddonsPagedAsync(
        [FromQuery] int?  skip            = null,
        [FromQuery] int?  take            = null,
        [FromQuery] string search         = null,
        [FromQuery] string sortBy         = null,
        [FromQuery] bool   sortDescending = false,
        [FromQuery] bool   onlyOrphans    = true,
        CancellationToken  cancellationToken = default)
    {
        int[] dlcGenreIds = await GetDlcGenreIdsAsync(cancellationToken);

        IQueryable<Database.Models.Software> baseQuery = BuildOrphanQuery(dlcGenreIds, onlyOrphans);

        if(!string.IsNullOrWhiteSpace(search)) baseQuery = baseQuery.Where(s => s.Name.Contains(search));

        // First-match-wins reason code, identical ordering to AddonOrphanReason:
        //   6 = MisclassifiedAsGame (precedes everything for Kind == Game rows)
        //   1 = NoBase
        //   2 = SelfReference
        //   3 = DanglingFk
        //   4 = ChainedDlc
        //   5 = NameMismatch
        //   0 = Linked
        // Cast to (int) so EF emits a plain INTEGER expression — using (byte)
        // forces a tinyint cast that Pomelo sometimes mishandles inside CASE.
        IQueryable<AddonRow> projected = baseQuery.Select(s => new AddonRow
        {
            Id             = s.Id,
            Name           = s.Name,
            Kind           = s.Kind,
            BaseSoftwareId = s.BaseSoftwareId,
            BaseName       = s.BaseSoftware != null ? s.BaseSoftware.Name : null,
            HasDlcGenre    = context.GenresBySoftware
                                    .Any(g => g.SoftwareId == s.Id && dlcGenreIds.Contains(g.GenreId)),
            ReasonCode = s.Kind == SoftwareKind.Game ? 6 :
                         s.BaseSoftwareId == null    ? 1 :
                         s.BaseSoftwareId == s.Id    ? 2 :
                         s.BaseSoftware   == null    ? 3 :
                         s.BaseSoftware.Kind == SoftwareKind.Dlc ? 4 :
                         MarechaiContext.NormalizeForMatch(s.Name) !=
                         MarechaiContext.NormalizeForMatch(s.BaseSoftware.Name) &&
                         !EF.Functions.Like(MarechaiContext.NormalizeForMatch(s.Name),
                                            MarechaiContext.NormalizeForMatch(s.BaseSoftware.Name) +
                                            " %") ? 5 : 0
        });

        IQueryable<AddonRow> ordered = sortBy switch
        {
            "Name"             => sortDescending
                                      ? projected.OrderByDescending(p => MarechaiContext.NaturalSortKey(p.Name))
                                      : projected.OrderBy(p => MarechaiContext.NaturalSortKey(p.Name)),
            "Kind"             => sortDescending ? projected.OrderByDescending(p => p.Kind)
                                                 : projected.OrderBy(p => p.Kind),
            "BaseSoftwareName" => sortDescending
                                      ? projected.OrderByDescending(p => MarechaiContext.NaturalSortKey(p.BaseName ?? string.Empty))
                                      : projected.OrderBy(p => MarechaiContext.NaturalSortKey(p.BaseName ?? string.Empty)),
            "OrphanReason"     => sortDescending ? projected.OrderByDescending(p => p.ReasonCode)
                                                 : projected.OrderBy(p => p.ReasonCode),
            _                  => projected.OrderBy(p => MarechaiContext.NaturalSortKey(p.Name))
        };

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);

        if(take.HasValue) ordered = ordered.Take(take.Value);

        List<AddonRow> rows = await ordered.ToListAsync(cancellationToken);

        return rows.Select(r => new SoftwareAddonDto
                    {
                        Id               = r.Id,
                        Name             = r.Name,
                        Kind             = r.Kind,
                        HasDlcGenre      = r.HasDlcGenre,
                        BaseSoftwareId   = r.BaseSoftwareId,
                        BaseSoftwareName = r.BaseName,
                        OrphanReason     = (AddonOrphanReason)r.ReasonCode
                    })
                   .ToList();
    }

    /// <summary>
    ///     Count companion for <see cref="GetAdminAddonsPagedAsync"/>. Not
    ///     cached — admins expect the total to drop as soon as they complete a
    ///     link operation.
    /// </summary>
    [HttpGet("admin/addons/count")]
    [Authorize(Roles = "Admin, UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetAdminAddonsCountAsync([FromQuery] string search      = null,
                                                    [FromQuery] bool   onlyOrphans = true,
                                                    CancellationToken cancellationToken = default)
    {
        int[] dlcGenreIds = await GetDlcGenreIdsAsync(cancellationToken);

        IQueryable<Database.Models.Software> query = BuildOrphanQuery(dlcGenreIds, onlyOrphans);

        if(!string.IsNullOrWhiteSpace(search)) query = query.Where(s => s.Name.Contains(search));

        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    ///     Backs the autocomplete inside the link dialog. Returns up to
    ///     <paramref name="take"/> software rows that are valid base candidates:
    ///     <c>Kind != Dlc</c> AND not carrying a DLC / add-on genre (so the
    ///     operator can't pick another orphan as a parent).
    /// </summary>
    /// <param name="search">Optional substring match on <c>Name</c>.</param>
    /// <param name="prefix">Optional pre-tokenised name prefix to compare against <c>NormalizeForMatch(Name)</c>.</param>
    /// <param name="mode"><c>"exact"</c>, <c>"startswith"</c>, or <c>"off"</c> (default).</param>
    /// <param name="take">Result cap; defaults to 50, clamped to [1, 200].</param>
    [HttpGet("admin/addons/candidates")]
    [Authorize(Roles = "Admin, UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetAddonCandidatesAsync(
        [FromQuery] string search = null,
        [FromQuery] string prefix = null,
        [FromQuery] string mode   = "off",
        [FromQuery] int?   take   = null,
        CancellationToken cancellationToken = default)
    {
        int[] dlcGenreIds = await GetDlcGenreIdsAsync(cancellationToken);

        // Valid bases: not a DLC and not a misclassified Game-with-DLC-genre.
        IQueryable<Database.Models.Software> query = context.Softwares
            .Where(s => s.Kind != SoftwareKind.Dlc &&
                        !context.GenresBySoftware.Any(g => g.SoftwareId == s.Id &&
                                                           dlcGenreIds.Contains(g.GenreId)));

        if(!string.IsNullOrWhiteSpace(search)) query = query.Where(s => s.Name.Contains(search));

        string normalizedPrefix = NormalizePrefixForMatch(prefix);

        switch((mode ?? "off").ToLowerInvariant())
        {
            case "exact" when !string.IsNullOrEmpty(normalizedPrefix):
                query = query.Where(s => MarechaiContext.NormalizeForMatch(s.Name) == normalizedPrefix);

                break;
            case "startswith" when !string.IsNullOrEmpty(normalizedPrefix):
                string likePattern = EscapeLikePrefix(normalizedPrefix) + "%";
                query = query.Where(s => EF.Functions.Like(MarechaiContext.NormalizeForMatch(s.Name), likePattern));

                break;
        }

        int resolvedTake = Math.Clamp(take ?? 50, 1, 200);

        return await query.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
                          .Take(resolvedTake)
                          .Select(s => new SoftwareDto
                           {
                               Id            = s.Id,
                               Name          = s.Name,
                               FamilyId      = s.FamilyId,
                               Family        = s.Family.Name,
                               Kind          = s.Kind,
                               IsCompilation = false,
                               FrontCoverId  = null
                           })
                          .ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Sets (or clears) the base software of a single Software row. When
    ///     <see cref="SetBaseSoftwareRequestDto.BaseSoftwareId"/> is non-null
    ///     and the source row is not already <see cref="SoftwareKind.Dlc"/>,
    ///     the row's <c>Kind</c> is also promoted to <see cref="SoftwareKind.Dlc"/>
    ///     in the same transaction (mirrors the import-time fixup in
    ///     <c>Marechai.MobyGames/Services/DlcRelationService.cs</c>).
    /// </summary>
    [HttpPatch("{id:ulong}/base-software")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> SetBaseSoftwareAsync(ulong id, [FromBody] SetBaseSoftwareRequestDto request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        Software model = await context.Softwares.FindAsync([id]);

        if(model is null) return NotFound();

        int[] dlcGenreIds = await GetDlcGenreIdsAsync(HttpContext.RequestAborted);

        bool hasDlcGenre = await context.GenresBySoftware
                                        .AnyAsync(g => g.SoftwareId == id && dlcGenreIds.Contains(g.GenreId),
                                                  HttpContext.RequestAborted);

        // Same scope as the orphan-addons grid: we only allow this PATCH on
        // rows that page would list (a DLC, or a Game carrying a DLC genre).
        if(model.Kind != SoftwareKind.Dlc && !(model.Kind == SoftwareKind.Game && hasDlcGenre))
            return Problem(detail: "This endpoint only accepts DLC rows or Game rows carrying a DLC / add-on genre.", statusCode: StatusCodes.Status400BadRequest);

        ActionResult validationError = await ApplyBaseSoftwareLinkAsync(model, request.BaseSoftwareId, dlcGenreIds,
                                                                       HttpContext.RequestAborted);

        if(validationError is not null) return validationError;

        await context.News.AddAsync(new News
        {
            AddedId = (long)model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedSoftwareInDb,
            Name    = model.Name
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    /// <summary>
    ///     Bulk variant of <see cref="SetBaseSoftwareAsync"/>: applies the
    ///     same link operation to every id in
    ///     <see cref="BulkSetBaseSoftwareRequestDto.SoftwareIds"/>. Per-row
    ///     failures are reported in the response rather than rolling back the
    ///     whole batch — the operator can re-run the failing rows manually.
    /// </summary>
    [HttpPatch("admin/addons/base-software-bulk")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BulkSetBaseSoftwareResultDto>> SetBaseSoftwareBulkAsync(
        [FromBody] BulkSetBaseSoftwareRequestDto request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(request.SoftwareIds is null || request.SoftwareIds.Count == 0)
            return Problem(detail: "softwareIds must contain at least one id.", statusCode: StatusCodes.Status400BadRequest);

        int[] dlcGenreIds = await GetDlcGenreIdsAsync(HttpContext.RequestAborted);

        var result = new BulkSetBaseSoftwareResultDto
        {
            Failed = []
        };

        List<ulong> distinctIds = request.SoftwareIds.Distinct().ToList();

        Dictionary<ulong, Software> models = await context.Softwares
                                                          .Where(s => distinctIds.Contains(s.Id))
                                                          .ToDictionaryAsync(s => s.Id, HttpContext.RequestAborted);

        // Pre-compute which rows carry a DLC genre in a single query.
        HashSet<ulong> dlcGenreRows = (await context.GenresBySoftware
                                                    .Where(g => distinctIds.Contains(g.SoftwareId) &&
                                                                dlcGenreIds.Contains(g.GenreId))
                                                    .Select(g => g.SoftwareId)
                                                    .Distinct()
                                                    .ToListAsync(HttpContext.RequestAborted))
                                     .ToHashSet();

        foreach(ulong sourceId in distinctIds)
        {
            if(!models.TryGetValue(sourceId, out Software model))
            {
                result.Failed.Add(new BulkSetBaseSoftwareFailureDto
                {
                    Id     = sourceId,
                    Reason = "Software not found."
                });

                continue;
            }

            if(model.Kind != SoftwareKind.Dlc &&
               !(model.Kind == SoftwareKind.Game && dlcGenreRows.Contains(sourceId)))
            {
                result.Failed.Add(new BulkSetBaseSoftwareFailureDto
                {
                    Id     = sourceId,
                    Reason = "Row is neither DLC nor a Game with a DLC / add-on genre."
                });

                continue;
            }

            ActionResult validationError = await ApplyBaseSoftwareLinkAsync(model, request.BaseSoftwareId, dlcGenreIds,
                                                                           HttpContext.RequestAborted);

            if(validationError is BadRequestObjectResult bad)
            {
                result.Failed.Add(new BulkSetBaseSoftwareFailureDto
                {
                    Id     = sourceId,
                    Reason = bad.Value?.ToString() ?? "Validation failed."
                });

                continue;
            }

            await context.News.AddAsync(new News
            {
                AddedId = (long)model.Id,
                Date    = DateTime.UtcNow,
                Type    = NewsType.UpdatedSoftwareInDb,
                Name    = model.Name
            });

            result.Updated++;
        }

        if(result.Updated > 0) await context.SaveChangesWithUserAsync(userId);

        return result;
    }

    /// <summary>
    ///     Shared mutation logic for the single + bulk PATCH endpoints.
    ///     Mutates <paramref name="model"/> in place (BaseSoftwareId, possibly
    ///     Kind) without saving — the caller batches the <see cref="News"/>
    ///     row and <c>SaveChangesWithUserAsync</c>. Returns a non-null
    ///     <see cref="BadRequestObjectResult"/> on validation failure;
    ///     <c>null</c> means the in-memory model was successfully updated.
    /// </summary>
    async Task<ActionResult> ApplyBaseSoftwareLinkAsync(Software model, ulong? baseSoftwareId, int[] dlcGenreIds,
                                                       CancellationToken cancellationToken)
    {
        if(!baseSoftwareId.HasValue)
        {
            // Clearing the link. The Software.UpdateAsync invariant requires
            // BaseSoftwareId on a DLC row, so we refuse to leave the row in an
            // invalid state — the operator must reclassify Kind via the full
            // edit page if they really want to unlink.
            if(model.Kind == SoftwareKind.Dlc)
                return Problem(detail: "Cannot clear base software on a DLC row. Change Kind first via the full edit page.", statusCode: StatusCodes.Status400BadRequest);

            model.BaseSoftwareId = null;

            return null;
        }

        ulong baseId = baseSoftwareId.Value;

        if(baseId == model.Id) return Problem(detail: "Software cannot reference itself as its base.", statusCode: StatusCodes.Status400BadRequest);

        Software baseSw = await context.Softwares.FindAsync([baseId], cancellationToken);

        if(baseSw is null) return Problem(detail: "Base software not found.", statusCode: StatusCodes.Status400BadRequest);

        if(baseSw.Kind == SoftwareKind.Dlc)
            return Problem(detail: "Base software is itself a DLC / add-on; chained DLCs are not allowed.", statusCode: StatusCodes.Status400BadRequest);

        bool baseHasDlcGenre = await context.GenresBySoftware
                                            .AnyAsync(g => g.SoftwareId == baseId &&
                                                           dlcGenreIds.Contains(g.GenreId), cancellationToken);

        if(baseHasDlcGenre)
            return Problem(detail: "Base software is a misclassified Game carrying a DLC / add-on genre.", statusCode: StatusCodes.Status400BadRequest);

        model.BaseSoftwareId = baseId;

        if(model.Kind != SoftwareKind.Dlc) model.Kind = SoftwareKind.Dlc;

        return null;
    }

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwareDto> GetAsync(ulong id) => context.Softwares.Where(s => s.Id == id)
                                                          .Select(s => new SoftwareDto
                                                           {
                                                               Id                = s.Id,
                                                               Name              = s.Name,
                                                               FamilyId          = s.FamilyId,
                                                               Family            = s.Family.Name,
                                                               PredecessorId     = s.PredecessorId,
                                                               Predecessor       = s.Predecessor.Name,
                                                               RelationshipType  = s.RelationshipType,
                                                               Successors = context.Softwares
                                                                                   .Where(x => x.PredecessorId == s.Id)
                                                                                   .Select(x => new SoftwareSuccessorDto
                                                                                    {
                                                                                        Id               = x.Id,
                                                                                        Name             = x.Name,
                                                                                        RelationshipType = x.RelationshipType
                                                                                    })
                                                                                   .ToList(),
                                                               Kind              = s.Kind,
                                                               BaseSoftwareId    = s.BaseSoftwareId,
                                                               BaseSoftware      = s.BaseSoftware.Name
                                                           })
                                                          .FirstOrDefaultAsync();

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwareDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Software model = await context.Softwares.FindAsync(id);

        if(model is null) return NotFound();

        model.Name              = dto.Name;
        model.FamilyId          = dto.FamilyId;
        model.PredecessorId     = dto.PredecessorId;
        model.RelationshipType  = dto.RelationshipType;
        model.Kind              = dto.Kind;
        model.BaseSoftwareId    = dto.BaseSoftwareId;

        if(model.Kind == SoftwareKind.Dlc && model.BaseSoftwareId is null)
            return Problem(detail: "DLC / Addon software must have a base software.", statusCode: StatusCodes.Status400BadRequest);

        if(model.Kind != SoftwareKind.Dlc && model.BaseSoftwareId is not null)
            return Problem(detail: "Only DLC / Addon software can have a base software.", statusCode: StatusCodes.Status400BadRequest);

        await context.News.AddAsync(new News
        {
            AddedId = (long)model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedSoftwareInDb,
            Name    = dto.Name
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwareDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Software
        {
            Name              = dto.Name,
            FamilyId          = dto.FamilyId,
            PredecessorId     = dto.PredecessorId,
            RelationshipType  = dto.RelationshipType,
            Kind              = dto.Kind,
            BaseSoftwareId    = dto.BaseSoftwareId
        };

        if(model.Kind == SoftwareKind.Dlc && model.BaseSoftwareId is null)
            return Problem(detail: "DLC / Addon software must have a base software.", statusCode: StatusCodes.Status400BadRequest);

        if(model.Kind != SoftwareKind.Dlc && model.BaseSoftwareId is not null)
            return Problem(detail: "Only DLC / Addon software can have a base software.", statusCode: StatusCodes.Status400BadRequest);

        await context.Softwares.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        await context.News.AddAsync(new News
        {
            AddedId = (long)model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.NewSoftwareInDb,
            Name    = dto.Name
        });

        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Software item = await context.Softwares.FindAsync(id);

        if(item is null) return NotFound();

        string entityName = item.Name;

        context.Softwares.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        // Mark any pending suggestions for this Software as Stale and notify the suggesting users.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.Software, (long)id, entityName);

        // Cascade: also mark stale every per-language description suggestion for this Software.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.SoftwareDescription, (long)id, entityName);

        return Ok();
    }

    [HttpGet("{targetId:ulong}/merge-preview/{sourceId:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SoftwareMergePreviewDto>> GetMergePreviewAsync(ulong targetId, ulong sourceId)
    {
        if(targetId == sourceId) return Problem(detail: "Cannot merge a software entry into itself.", statusCode: StatusCodes.Status400BadRequest);

        Software target = await context.Softwares.FindAsync(targetId);

        if(target is null) return Problem(detail: "Target software not found.", statusCode: StatusCodes.Status404NotFound);

        Software source = await context.Softwares.FindAsync(sourceId);

        if(source is null) return Problem(detail: "Source software not found.", statusCode: StatusCodes.Status404NotFound);

        // Extract suggested release title from name difference
        string suggestedTitle = source.Name;

        if(source.Name.StartsWith(target.Name, StringComparison.OrdinalIgnoreCase) &&
           source.Name.Length > target.Name.Length)
        {
            suggestedTitle = source.Name[target.Name.Length..].TrimStart(' ', ':', '-', '(').TrimEnd(')').Trim();

            if(string.IsNullOrWhiteSpace(suggestedTitle)) suggestedTitle = source.Name;
        }

        // Count versions
        int versionsCount = await context.SoftwareVersions.CountAsync(v => v.SoftwareId == sourceId);

        // Count direct releases
        int directReleasesCount =
            await context.SoftwareReleases.CountAsync(r => r.SoftwareId == sourceId);

        int directReleasesWithoutTitleCount =
            await context.SoftwareReleases.CountAsync(r => r.SoftwareId == sourceId &&
                                                           (r.Title == null || r.Title == ""));

        // Count company roles and duplicates
        List<SoftwareCompanyRole> sourceCompanyRoles =
            await context.SoftwareCompanyRoles.Where(cr => cr.SoftwareId == sourceId).ToListAsync();

        HashSet<(int, string)> targetCompanyRoleKeys = (await context.SoftwareCompanyRoles
                                                                     .Where(cr => cr.SoftwareId == targetId)
                                                                     .Select(cr => new { cr.CompanyId, cr.RoleId })
                                                                     .ToListAsync())
           .Select(cr => (cr.CompanyId, cr.RoleId))
           .ToHashSet();

        int companyRolesDuplicates =
            sourceCompanyRoles.Count(cr => targetCompanyRoleKeys.Contains((cr.CompanyId, cr.RoleId)));

        // Count screenshots
        int screenshotsCount = await context.SoftwareScreenshots.CountAsync(s => s.SoftwareId == sourceId);

        // Count descriptions and duplicates
        List<string> sourceDescriptionLangs = await context.SoftwareDescriptions
                                                           .Where(d => d.SoftwareId == sourceId)
                                                           .Select(d => d.LanguageCode)
                                                           .ToListAsync();

        List<string> targetDescriptionLangs = await context.SoftwareDescriptions
                                                           .Where(d => d.SoftwareId == targetId)
                                                           .Select(d => d.LanguageCode)
                                                           .ToListAsync();

        int descriptionsDuplicates = sourceDescriptionLangs.Count(sl => targetDescriptionLangs.Contains(sl));

        // Count genres and duplicates
        List<int> sourceGenreIds =
            await context.GenresBySoftware.Where(g => g.SoftwareId == sourceId).Select(g => g.GenreId).ToListAsync();

        HashSet<int> targetGenreIds =
            (await context.GenresBySoftware.Where(g => g.SoftwareId == targetId).Select(g => g.GenreId).ToListAsync())
           .ToHashSet();

        int genresDuplicates = sourceGenreIds.Count(gid => targetGenreIds.Contains(gid));

        // Count credits and duplicates
        var sourceCredits = await context.PeopleBySoftware
                                         .Where(p => p.SoftwareId == sourceId)
                                         .Select(p => new { p.PersonId, p.Role })
                                         .ToListAsync();

        var targetCredits = await context.PeopleBySoftware
                                         .Where(p => p.SoftwareId == targetId)
                                         .Select(p => new { p.PersonId, p.Role })
                                         .ToListAsync();

        // MariaDB comparison is case-insensitive; replicate that here
        HashSet<(int, string)> targetCreditKeys = targetCredits
                                                 .Select(c => (c.PersonId, c.Role.ToLowerInvariant()))
                                                 .ToHashSet();

        int creditsDuplicates =
            sourceCredits.Count(c => targetCreditKeys.Contains((c.PersonId, c.Role.ToLowerInvariant())));

        // Count compilation references and duplicates
        List<ulong> sourceCompilationIds = await context.SoftwareBySoftwareCompilation
                                                        .Where(s => s.SoftwareId == sourceId)
                                                        .Select(s => s.SoftwareCompilationId)
                                                        .ToListAsync();

        HashSet<ulong> targetCompilationIds = (await context.SoftwareBySoftwareCompilation
                                                            .Where(s => s.SoftwareId == targetId)
                                                            .Select(s => s.SoftwareCompilationId)
                                                            .ToListAsync())
           .ToHashSet();

        int compilationDuplicates =
            sourceCompilationIds.Count(rid => targetCompilationIds.Contains(rid));

        // Count promo art
        int promoArtCount = await context.SoftwarePromoArt.CountAsync(p => p.SoftwareId == sourceId);

        // Count videos and duplicates
        var sourceVideos = await context.SoftwareVideos
                                        .Where(v => v.SoftwareId == sourceId)
                                        .Select(v => new { v.Provider, v.VideoId })
                                        .ToListAsync();

        HashSet<(string, string)> targetVideoKeys = (await context.SoftwareVideos
                                                                   .Where(v => v.SoftwareId == targetId)
                                                                   .Select(v => new { v.Provider, v.VideoId })
                                                                   .ToListAsync())
           .Select(v => (v.Provider, v.VideoId))
           .ToHashSet();

        int videosDuplicates = sourceVideos.Count(v => targetVideoKeys.Contains((v.Provider, v.VideoId)));

        return new SoftwareMergePreviewDto
        {
            TargetId                       = targetId,
            TargetName                     = target.Name,
            SourceId                       = sourceId,
            SourceName                     = source.Name,
            SuggestedReleaseTitle          = suggestedTitle,
            VersionsCount                  = versionsCount,
            DirectReleasesCount            = directReleasesCount,
            DirectReleasesWithoutTitleCount = directReleasesWithoutTitleCount,
            CompanyRolesTotal              = sourceCompanyRoles.Count,
            CompanyRolesDuplicates         = companyRolesDuplicates,
            ScreenshotsCount               = screenshotsCount,
            DescriptionsTotal              = sourceDescriptionLangs.Count,
            DescriptionsDuplicates         = descriptionsDuplicates,
            GenresTotal                    = sourceGenreIds.Count,
            GenresDuplicates               = genresDuplicates,
            CreditsTotal                   = sourceCredits.Count,
            CreditsDuplicates              = creditsDuplicates,
            CompilationReferencesTotal     = sourceCompilationIds.Count,
            CompilationReferencesDuplicates = compilationDuplicates,
            PromoArtCount                  = promoArtCount,
            VideosTotal                    = sourceVideos.Count,
            VideosDuplicates               = videosDuplicates
        };
    }

    [HttpPost("{targetId:ulong}/merge/{sourceId:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> MergeAsync(ulong targetId, ulong sourceId, [FromQuery] string releaseTitle = null)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(targetId == sourceId) return Problem(detail: "Cannot merge a software entry into itself.", statusCode: StatusCodes.Status400BadRequest);

        Software target = await context.Softwares.FindAsync(targetId);

        if(target is null) return Problem(detail: "Target software not found.", statusCode: StatusCodes.Status404NotFound);

        Software source = await context.Softwares.FindAsync(sourceId);

        if(source is null) return Problem(detail: "Source software not found.", statusCode: StatusCodes.Status404NotFound);

        if((source.Kind == SoftwareKind.Dlc) != (target.Kind == SoftwareKind.Dlc))
            return Problem(detail: "Cannot merge DLC / Addon software with non-DLC software.", statusCode: StatusCodes.Status409Conflict);

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 1. Re-parent SoftwareVersions
            List<SoftwareVersion> sourceVersions =
                await context.SoftwareVersions.Where(v => v.SoftwareId == sourceId).ToListAsync();

            foreach(SoftwareVersion version in sourceVersions)
                version.SoftwareId = targetId;

            // 2. Re-parent direct SoftwareReleases, set title on untitled ones
            List<SoftwareRelease> sourceDirectReleases =
                await context.SoftwareReleases.Where(r => r.SoftwareId == sourceId).ToListAsync();

            foreach(SoftwareRelease release in sourceDirectReleases)
            {
                if(string.IsNullOrEmpty(release.Title) && !string.IsNullOrEmpty(releaseTitle))
                    release.Title = releaseTitle;

                release.SoftwareId = targetId;
            }

            // 3. Merge SoftwareCompanyRoles (composite PK — must remove+add)
            List<SoftwareCompanyRole> sourceCompanyRoles =
                await context.SoftwareCompanyRoles.Where(cr => cr.SoftwareId == sourceId).ToListAsync();

            HashSet<(int, string)> targetCompanyRoleKeys = (await context.SoftwareCompanyRoles
                                                                         .Where(cr => cr.SoftwareId == targetId)
                                                                         .Select(cr => new { cr.CompanyId, cr.RoleId })
                                                                         .ToListAsync())
               .Select(cr => (cr.CompanyId, cr.RoleId))
               .ToHashSet();

            foreach(SoftwareCompanyRole cr in sourceCompanyRoles)
            {
                context.SoftwareCompanyRoles.Remove(cr);

                if(!targetCompanyRoleKeys.Contains((cr.CompanyId, cr.RoleId)))
                {
                    context.SoftwareCompanyRoles.Add(new SoftwareCompanyRole
                    {
                        SoftwareId = targetId,
                        CompanyId  = cr.CompanyId,
                        RoleId     = cr.RoleId
                    });

                    targetCompanyRoleKeys.Add((cr.CompanyId, cr.RoleId));
                }
            }

            // 4. Re-parent SoftwareScreenshots (no dedup, Guid PK)
            List<SoftwareScreenshot> sourceScreenshots =
                await context.SoftwareScreenshots.Where(s => s.SoftwareId == sourceId).ToListAsync();

            foreach(SoftwareScreenshot screenshot in sourceScreenshots)
                screenshot.SoftwareId = targetId;

            // 4b. Re-parent SoftwarePromoArt (no dedup, Guid PK)
            List<SoftwarePromoArt> sourcePromoArt =
                await context.SoftwarePromoArt.Where(p => p.SoftwareId == sourceId).ToListAsync();

            foreach(SoftwarePromoArt promo in sourcePromoArt)
                promo.SoftwareId = targetId;

            // 4c. Merge SoftwareVideos (unique on SoftwareId+Provider+VideoId)
            List<SoftwareVideo> sourceVideos =
                await context.SoftwareVideos.Where(v => v.SoftwareId == sourceId).ToListAsync();

            HashSet<(string, string)> targetVideoKeys = (await context.SoftwareVideos
                                                                       .Where(v => v.SoftwareId == targetId)
                                                                       .Select(v => new { v.Provider, v.VideoId })
                                                                       .ToListAsync())
               .Select(v => (v.Provider, v.VideoId))
               .ToHashSet();

            foreach(SoftwareVideo video in sourceVideos)
            {
                if(targetVideoKeys.Contains((video.Provider, video.VideoId)))
                    context.SoftwareVideos.Remove(video);
                else
                {
                    video.SoftwareId = targetId;
                    targetVideoKeys.Add((video.Provider, video.VideoId));
                }
            }

            // 5. Merge SoftwareDescriptions (unique on SoftwareId+LanguageCode)
            List<SoftwareDescription> sourceDescriptions =
                await context.SoftwareDescriptions.Where(d => d.SoftwareId == sourceId).ToListAsync();

            HashSet<string> targetDescriptionLangs = (await context.SoftwareDescriptions
                                                                   .Where(d => d.SoftwareId == targetId)
                                                                   .Select(d => d.LanguageCode)
                                                                   .ToListAsync())
               .ToHashSet();

            foreach(SoftwareDescription desc in sourceDescriptions)
            {
                if(targetDescriptionLangs.Contains(desc.LanguageCode))
                    context.SoftwareDescriptions.Remove(desc);
                else
                {
                    desc.SoftwareId = targetId;
                    targetDescriptionLangs.Add(desc.LanguageCode);
                }
            }

            // 6. Merge GenreBySoftware (composite PK — must remove+add)
            List<GenreBySoftware> sourceGenres =
                await context.GenresBySoftware.Where(g => g.SoftwareId == sourceId).ToListAsync();

            HashSet<int> targetGenreIds =
                (await context.GenresBySoftware.Where(g => g.SoftwareId == targetId).Select(g => g.GenreId)
                              .ToListAsync())
               .ToHashSet();

            foreach(GenreBySoftware genre in sourceGenres)
            {
                context.GenresBySoftware.Remove(genre);

                if(!targetGenreIds.Contains(genre.GenreId))
                {
                    context.GenresBySoftware.Add(new GenreBySoftware
                    {
                        SoftwareId = targetId,
                        GenreId    = genre.GenreId
                    });

                    targetGenreIds.Add(genre.GenreId);
                }
            }

            // 7. Merge PeopleBySoftware (unique on SoftwareId+PersonId+Role, case-insensitive)
            List<PeopleBySoftware> sourceCredits =
                await context.PeopleBySoftware.Where(p => p.SoftwareId == sourceId).ToListAsync();

            HashSet<(int, string)> targetCreditKeys = (await context.PeopleBySoftware
                                                                    .Where(p => p.SoftwareId == targetId)
                                                                    .Select(p => new { p.PersonId, p.Role })
                                                                    .ToListAsync())
               .Select(c => (c.PersonId, c.Role.ToLowerInvariant()))
               .ToHashSet();

            foreach(PeopleBySoftware credit in sourceCredits)
            {
                if(targetCreditKeys.Contains((credit.PersonId, credit.Role.ToLowerInvariant())))
                    context.PeopleBySoftware.Remove(credit);
                else
                {
                    credit.SoftwareId = targetId;
                    targetCreditKeys.Add((credit.PersonId, credit.Role.ToLowerInvariant()));
                }
            }

            // 8. Merge SoftwareBySoftwareCompilation compilation references (composite PK — must remove+add)
            List<SoftwareBySoftwareCompilation> sourceCompilationRefs =
                await context.SoftwareBySoftwareCompilation.Where(s => s.SoftwareId == sourceId).ToListAsync();

            HashSet<ulong> targetCompilationIds = (await context.SoftwareBySoftwareCompilation
                                                                .Where(s => s.SoftwareId == targetId)
                                                                .Select(s => s.SoftwareCompilationId)
                                                                .ToListAsync())
               .ToHashSet();

            foreach(SoftwareBySoftwareCompilation compRef in sourceCompilationRefs)
            {
                context.SoftwareBySoftwareCompilation.Remove(compRef);

                if(!targetCompilationIds.Contains(compRef.SoftwareCompilationId))
                {
                    context.SoftwareBySoftwareCompilation.Add(new SoftwareBySoftwareCompilation
                    {
                        SoftwareCompilationId = compRef.SoftwareCompilationId,
                        SoftwareId            = targetId
                    });

                    targetCompilationIds.Add(compRef.SoftwareCompilationId);
                }
            }

            // 9. Update MobyGames tracking tables
            List<MobyGamesImportState> importStates =
                await context.MobyGamesImportStates.Where(s => s.SoftwareId == sourceId).ToListAsync();

            foreach(MobyGamesImportState state in importStates)
                state.SoftwareId = targetId;

            List<MobyGamesCoverDownloadState> coverStates =
                await context.MobyGamesCoverDownloadStates.Where(s => s.SoftwareId == sourceId).ToListAsync();

            foreach(MobyGamesCoverDownloadState state in coverStates)
                state.SoftwareId = targetId;

            List<MobyGamesPromoArtDownloadState> promoArtStates =
                await context.MobyGamesPromoArtDownloadStates.Where(s => s.SoftwareId == sourceId).ToListAsync();

            foreach(MobyGamesPromoArtDownloadState state in promoArtStates)
                state.SoftwareId = targetId;

            List<MobyGamesVideoImportState> videoImportStates =
                await context.MobyGamesVideoImportStates.Where(s => s.SoftwareId == sourceId).ToListAsync();

            foreach(MobyGamesVideoImportState state in videoImportStates)
                state.SoftwareId = targetId;

            // 9b. Transfer predecessor: if target has no predecessor but source does, adopt it
            if(target.PredecessorId is null && source.PredecessorId is not null)
            {
                target.PredecessorId    = source.PredecessorId;
                target.RelationshipType = source.RelationshipType;
            }

            // 9c. Re-point any software that had source as predecessor to target
            List<Software> successorsOfSource =
                await context.Softwares.Where(s => s.PredecessorId == sourceId && s.Id != targetId).ToListAsync();

            foreach(Software successor in successorsOfSource)
                successor.PredecessorId = targetId;

            // 9d. Transfer base software: if target has no base software but source does, adopt it
            if(target.BaseSoftwareId is null && source.BaseSoftwareId is not null)
                target.BaseSoftwareId = source.BaseSoftwareId;

            // 9e. Re-point any software that had source as base software to target
            List<Software> addonsOfSource =
                await context.Softwares.Where(s => s.BaseSoftwareId == sourceId && s.Id != targetId).ToListAsync();

            foreach(Software addon in addonsOfSource)
                addon.BaseSoftwareId = targetId;

            // Flush all changes before deleting the source to avoid FK violations
            await context.SaveChangesWithUserAsync(userId);

            // 10. Delete source software
            context.Softwares.Remove(source);
            await context.SaveChangesWithUserAsync(userId);

            await transaction.CommitAsync();

            return Ok();
        }
        catch(Exception ex)
        {
            // Transaction auto-rolls back on dispose if not committed
            return StatusCode(StatusCodes.Status500InternalServerError,
                              $"Merge failed: {ex.Message}");
        }
    }

    [HttpGet("{id:ulong}/addons")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetAddonsAsync(ulong id, CancellationToken cancellationToken = default)
    {
        List<SoftwareDto> list = await context.Softwares
                                              .Where(s => s.BaseSoftwareId == id)
                                              .OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
                                              .Select(s => new SoftwareDto
                                               {
                                                   Id             = s.Id,
                                                   Name           = s.Name,
                                                   FamilyId       = s.FamilyId,
                                                   Family         = s.Family.Name,
                                                   Kind           = s.Kind,
                                                   BaseSoftwareId = s.BaseSoftwareId,
                                                   BaseSoftware   = s.BaseSoftware.Name,
                                                   IsCompilation  = false
                                               })
                                              .ToListAsync(cancellationToken);

        await PopulateFrontCoverIdsAsync(list, cancellationToken);

        return list;
    }

    [HttpGet("{id:ulong}/descriptions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDescriptionDto>> GetDescriptionsAsync(ulong id) => context.SoftwareDescriptions
       .Where(d => d.SoftwareId == id)
       .Select(d => new SoftwareDescriptionDto
        {
            Id           = d.Id,
            SoftwareId   = d.SoftwareId,
            Html         = d.Html,
            Markdown     = d.Text,
            LanguageCode = d.LanguageCode,
            Language     = d.Language.ReferenceName
        })
       .ToListAsync();

    [HttpGet("{id:ulong}/description")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<SoftwareDescriptionDto> GetDescriptionAsync(ulong id, [FromQuery] string lang = "eng")
    {
        SoftwareDescriptionDto description = await context.SoftwareDescriptions
                                                          .Where(d => d.SoftwareId == id && d.LanguageCode == lang)
                                                          .Select(d => new SoftwareDescriptionDto
                                                           {
                                                               Id           = d.Id,
                                                               SoftwareId   = d.SoftwareId,
                                                               Html         = d.Html,
                                                               Markdown     = d.Text,
                                                               LanguageCode = d.LanguageCode,
                                                               Language     = d.Language.ReferenceName
                                                           })
                                                          .FirstOrDefaultAsync();

        // Fallback to English if requested language not found
        if(description is null && lang != "eng")
            description = await context.SoftwareDescriptions
                                       .Where(d => d.SoftwareId == id && d.LanguageCode == "eng")
                                       .Select(d => new SoftwareDescriptionDto
                                        {
                                            Id           = d.Id,
                                            SoftwareId   = d.SoftwareId,
                                            Html         = d.Html,
                                            Markdown     = d.Text,
                                            LanguageCode = d.LanguageCode,
                                            Language     = d.Language.ReferenceName
                                        })
                                       .FirstOrDefaultAsync();

        return description;
    }

    [HttpGet("{id:ulong}/description/text")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetDescriptionTextAsync(ulong id, [FromQuery] string lang = "eng")
    {
        SoftwareDescription description =
            await context.SoftwareDescriptions.FirstOrDefaultAsync(d => d.SoftwareId   == id &&
                                                                        d.LanguageCode == lang);

        // Fallback to English if requested language not found
        if(description is null && lang != "eng")
            description = await context.SoftwareDescriptions.FirstOrDefaultAsync(d => d.SoftwareId   == id &&
                              d.LanguageCode == "eng");

        return description?.Html ?? description?.Text;
    }

    [HttpPost("{id:ulong}/description")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateOrUpdateDescriptionAsync(
        ulong id, [FromBody] SoftwareDescriptionDto description)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareDescription current = await context.SoftwareDescriptions
                                                   .FirstOrDefaultAsync(d => d.SoftwareId   == id &&
                                                                             d.LanguageCode == description.LanguageCode);

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        string             html     = Markdown.ToHtml(description.Markdown, pipeline);

        if(current is null)
        {
            current = new SoftwareDescription
            {
                SoftwareId   = id,
                LanguageCode = description.LanguageCode,
                Html         = html,
                Text         = description.Markdown
            };

            await context.SoftwareDescriptions.AddAsync(current);
        }
        else
        {
            current.Html = html;
            current.Text = description.Markdown;
        }

        await context.SaveChangesWithUserAsync(userId);

        return current.Id;
    }

    [HttpDelete("{id:ulong}/description/{languageCode}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteDescriptionAsync(ulong id, string languageCode)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareDescription description = await context.SoftwareDescriptions
                                                       .FirstOrDefaultAsync(d => d.SoftwareId   == id &&
                                                                                 d.LanguageCode == languageCode);

        if(description is null) return NotFound();

        // Capture display data BEFORE the cascade, while the Software + language rows are still
        // available for the system message body.
        string softwareName = await context.Softwares.AsNoTracking()
                                           .Where(s => s.Id == id)
                                           .Select(s => s.Name)
                                           .FirstOrDefaultAsync();
        string langName = await context.Iso639.AsNoTracking()
                                       .Where(l => l.Id == languageCode)
                                       .Select(l => l.ReferenceName)
                                       .FirstOrDefaultAsync();
        string subkeyLabel = $"({langName ?? languageCode} description)";

        context.SoftwareDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntitySubkeyAsync(
            context, Marechai.Data.SuggestionEntityType.SoftwareDescription,
            (long)id, languageCode, softwareName ?? $"#{id}", subkeyLabel);

        return Ok();
    }

    [HttpGet("genres")]
    [AllowAnonymous]
    [OutputCache(Duration = 300, VaryByQueryKeys = ["lang", "includeUnused"])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareGenreDto>> GetAllGenresAsync([FromQuery] string lang         = null,
                                                                [FromQuery] bool   includeUnused = false)
    {
        // The translated-name lookup uses the in-memory SoftwareGenreTranslationCache, so there is no
        // per-language IMemoryCache layer here — the cache is already in memory and lookups are O(1).
        // The DB-derived gating subset (`Where(g => g.Softwares.Any())` + ordering + projection)
        // is cached separately under SOFTWARE_GENRES_CACHE_KEY so repeated /software landing-page
        // hits don't re-run the EXISTS subquery over the whole catalog.
        // When includeUnused=true (admin pickers) we serve the ungated full list and cache it
        // under SOFTWARE_ALL_GENRES_CACHE_KEY so freshly-seeded genres with no software links
        // (e.g. the Category-type rows from AddSoftwareGenreCategoriesSeed) are still pickable.
        string resolvedLang = ResolveGenreLanguage(lang);

        await genreCache.EnsureLoadedAsync(HttpContext.RequestAborted);

        string genresCacheKey = includeUnused ? SOFTWARE_ALL_GENRES_CACHE_KEY : SOFTWARE_GENRES_CACHE_KEY;

        // Cache only the raw (id, canonical name, type) tuples — the canonical names are
        // immutable across requests, translation runs per-request against the in-memory
        // cache (~150 dictionary lookups). Keying the IMemoryCache entry on lang would
        // multiply the cached payload by N supported languages for no real win.
        if(!cache.TryGetValue(genresCacheKey,
                              out List<(int Id, string Name, int Type, string TypeName)> cached) ||
           cached is null)
        {
            IQueryable<Database.Models.SoftwareGenre> q = context.SoftwareGenres;

            if(!includeUnused) q = q.Where(g => g.Softwares.Any());

            var raw = await q.OrderBy(g => g.Type)
                             .ThenBy(g => g.Name)
                             .Select(g => new
                              {
                                  g.Id,
                                  g.Name,
                                  Type     = (int)g.Type,
                                  TypeName = g.Type.ToString()
                              })
                             .ToListAsync();

            cached = raw.Select(r => (r.Id, r.Name, r.Type, r.TypeName)).ToList();

            cache.Set(genresCacheKey, cached, _catalogCacheTtl);
        }

        // Importer stores genre names with U+00A0 (non-breaking space); normalise before applying the
        // translation lookup so the cache (which holds the canonical English name from the DB) returns
        // a consistent value when no translation exists for the requested language. The cache falls
        // back to a per-id DB load on miss (e.g. genres added between worker ticks).
        // Always build a fresh result list — mutating the cached tuples is impossible (they're
        // structs) but we still need to materialise translated SoftwareGenreDto instances per call.
        var genres = new List<SoftwareGenreDto>(cached.Count);

        foreach((int Id, string Name, int Type, string TypeName) g in cached)
        {
            string translated =
                await genreCache.GetNameAsync(g.Id, resolvedLang, HttpContext.RequestAborted);

            string displayName = string.IsNullOrEmpty(translated)
                                     ? g.Name?.Replace('\u00A0', ' ')
                                     : translated.Replace('\u00A0', ' ');

            genres.Add(new SoftwareGenreDto
            {
                Id       = g.Id,
                Name     = displayName,
                Type     = g.Type,
                TypeName = g.TypeName
            });
        }

        return genres;
    }

    [HttpGet("by-genre/{genreId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareByGenreAsync(int genreId,
                                                                 [FromQuery] SoftwareKind? kind = null,
                                                                 [FromQuery] int? skip = null,
                                                                 [FromQuery] int? take = null,
                                                                 CancellationToken cancellationToken = default)
    {
        IQueryable<SoftwareDto> combined = BuildByGenreQuery(genreId, kind);

        IQueryable<SoftwareDto> ordered = combined.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);

        if(take.HasValue) ordered = ordered.Take(take.Value);

        List<SoftwareDto> list = await ordered.ToListAsync(cancellationToken);

        await PopulateFrontCoverIdsAsync(list, cancellationToken);

        return list;
    }

    [HttpGet("by-genre/{genreId:int}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetSoftwareByGenreCountAsync(int genreId, [FromQuery] SoftwareKind? kind = null,
                                                  CancellationToken cancellationToken = default) =>
        BuildByGenreQuery(genreId, kind).CountAsync(cancellationToken);

    IQueryable<SoftwareDto> BuildByGenreQuery(int genreId, SoftwareKind? kind)
    {
        // Single SQL round-trip via Concat (UNION ALL).
        IQueryable<Database.Models.Software> baseSoftwareQuery = context.Softwares
           .Where(s => s.Genres.Any(g => g.GenreId == genreId));

        if(kind.HasValue) baseSoftwareQuery = baseSoftwareQuery.Where(s => s.Kind == kind.Value);

        IQueryable<SoftwareDto> softwareQuery = baseSoftwareQuery.Select(s => new SoftwareDto
        {
            Id            = s.Id,
            Name          = s.Name,
            FamilyId      = s.FamilyId,
            Family        = s.Family.Name,
            Kind          = s.Kind,
            IsCompilation = false
        });

        if(!ShouldIncludeCompilations(kind)) return softwareQuery;

        IQueryable<SoftwareDto> compilationsQuery = context.SoftwareCompilations
           .Where(comp => comp.IncludedSoftware.Any(s => s.Software.Genres.Any(g => g.GenreId == genreId)) ||
                          comp.IncludedVersions.Any(v => v.SoftwareVersion.Software.Genres.Any(g => g.GenreId == genreId)))
           .Select(comp => new SoftwareDto
            {
                Id            = comp.Id,
                Name          = comp.Name,
                FamilyId      = null,
                Family        = null,
                Kind          = default,
                IsCompilation = true
            });

        return softwareQuery.Concat(compilationsQuery);
    }

    [HttpGet("specifications")]
    [AllowAnonymous]
    [OutputCache(Duration = 300, VaryByQueryKeys = ["lang"])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareSpecKeyDto>> GetSpecificationsAsync([FromQuery] string lang = null)
    {
        // The DB-derived list (keys + grouped values) is cached normalised in English under a
        // single key. Translation runs per-request against the in-memory cache, which is cheap
        // (dict lookup per string × ~hundreds of values). Keying the IMemoryCache entry on lang
        // would multiply the cached payload by N supported languages for no real win.
        if(!cache.TryGetValue(SOFTWARE_SPECS_CACHE_KEY,
                              out List<(string Key, List<string> Values)> cached) || cached is null)
        {
            var raw = await context.SoftwareAttributes
                                   .Where(a => a.Category == "Spec" && a.Key != "Notes")
                                   .Select(a => new { a.Key, a.Value })
                                   .Distinct()
                                   .ToListAsync();

            cached = raw.GroupBy(a => SoftwareAttributeTranslationCache.NormalizeText(a.Key))
                        .OrderBy(g => g.Key)
                        .Select(g => (Key: g.Key,
                                      Values: g.Select(a => SoftwareAttributeTranslationCache.NormalizeText(a.Value))
                                               .Distinct()
                                               .OrderBy(v => v)
                                               .ToList()))
                        .ToList();

            cache.Set(SOFTWARE_SPECS_CACHE_KEY, cached, _catalogCacheTtl);
        }

        string resolvedLang = ResolveGenreLanguage(lang);

        // Always populate Display* — when resolvedLang == "eng" the cache returns the canonical
        // text verbatim, so DisplayKey == Key (free, no allocation in the cache path). The cache
        // falls back to a per-id DB load on miss (e.g. pool strings added between worker ticks).
        var result = new List<SoftwareSpecKeyDto>(cached.Count);

        foreach((string Key, List<string> Values) s in cached)
        {
            string displayKey =
                await attrCache.GetTranslatedAsync(s.Key, resolvedLang, HttpContext.RequestAborted);

            var displayValues = new List<string>(s.Values.Count);

            foreach(string v in s.Values)
                displayValues.Add(await attrCache.GetTranslatedAsync(v, resolvedLang,
                                                                     HttpContext.RequestAborted));

            result.Add(new SoftwareSpecKeyDto
            {
                Key           = s.Key,
                Values        = s.Values,
                DisplayKey    = displayKey,
                DisplayValues = displayValues
            });
        }

        return result;
    }

    [HttpGet("by-spec")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareBySpecAsync([FromQuery] string key, [FromQuery] string value,
                                                                [FromQuery] SoftwareKind? kind = null,
                                                                [FromQuery] int? skip = null,
                                                                [FromQuery] int? take = null,
                                                                CancellationToken cancellationToken = default)
    {
        if(key == "Notes") return [];

        IQueryable<SoftwareDto> combined = BuildBySpecQuery(key, value, kind);

        IQueryable<SoftwareDto> ordered = combined.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);

        if(take.HasValue) ordered = ordered.Take(take.Value);

        List<SoftwareDto> list = await ordered.ToListAsync(cancellationToken);

        await PopulateFrontCoverIdsAsync(list, cancellationToken);

        return list;
    }

    [HttpGet("by-spec/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetSoftwareBySpecCountAsync([FromQuery] string key, [FromQuery] string value,
                                                 [FromQuery] SoftwareKind? kind = null,
                                                 CancellationToken cancellationToken = default)
    {
        if(key == "Notes") return Task.FromResult(0);

        return BuildBySpecQuery(key, value, kind).CountAsync(cancellationToken);
    }

    IQueryable<SoftwareDto> BuildBySpecQuery(string key, string value, SoftwareKind? kind)
    {
        // Single SQL round-trip via Concat (UNION ALL).
        IQueryable<Database.Models.Software> baseSoftwareQuery = context.Softwares
               .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.Attributes
                                                                     .Any(a => a.Category == "Spec" &&
                                                                              a.Key   == key         &&
                                                                              a.Value == value)))
                         || s.DirectReleases.Any(r => r.Attributes
                                                       .Any(a => a.Category == "Spec" &&
                                                                  a.Key   == key       &&
                                                                  a.Value == value)));

        if(kind.HasValue) baseSoftwareQuery = baseSoftwareQuery.Where(s => s.Kind == kind.Value);

        IQueryable<SoftwareDto> softwareQuery = baseSoftwareQuery.Select(s => new SoftwareDto
        {
            Id            = s.Id,
            Name          = s.Name,
            FamilyId      = s.FamilyId,
            Family        = s.Family.Name,
            Kind          = s.Kind,
            IsCompilation = false
        });

        if(!ShouldIncludeCompilations(kind)) return softwareQuery;

        IQueryable<SoftwareDto> compilationsQuery = context.SoftwareCompilations
           .Where(comp => comp.Releases.Any(r => r.Attributes.Any(a => a.Category == "Spec" &&
                                                                        a.Key   == key       &&
                                                                        a.Value == value)))
           .Select(comp => new SoftwareDto
            {
                Id            = comp.Id,
                Name          = comp.Name,
                FamilyId      = null,
                Family        = null,
                Kind          = default,
                IsCompilation = true
            });

        return softwareQuery.Concat(compilationsQuery);
    }

    [HttpGet("/software/{softwareId:ulong}/genres")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareGenreDto>> GetGenresAsync(ulong softwareId, [FromQuery] string lang = null)
    {
        string resolvedLang = ResolveGenreLanguage(lang);

        await genreCache.EnsureLoadedAsync(HttpContext.RequestAborted);

        List<SoftwareGenreDto> genres = await context.GenresBySoftware
                                                     .Where(gs => gs.SoftwareId == softwareId)
                                                     .Select(gs => new SoftwareGenreDto
                                                      {
                                                          Id       = gs.Genre.Id,
                                                          Name     = gs.Genre.Name,
                                                          Type     = (int)gs.Genre.Type,
                                                          TypeName = gs.Genre.Type.ToString()
                                                      })
                                                     .OrderBy(g => g.Type)
                                                     .ThenBy(g => g.Name)
                                                     .ToListAsync();

        // Importer stores genre names with U+00A0 (non-breaking space); normalise before applying the
        // translation lookup so the cache (which holds the canonical English name from the DB) returns
        // a consistent value when no translation exists for the requested language. The cache falls
        // back to a per-id DB load on miss (e.g. genres added between worker ticks).
        foreach(SoftwareGenreDto g in genres)
        {
            string translated =
                await genreCache.GetNameAsync(g.Id, resolvedLang, HttpContext.RequestAborted);

            g.Name = string.IsNullOrEmpty(translated)
                         ? g.Name?.Replace('\u00A0', ' ')
                         : translated.Replace('\u00A0', ' ');
        }

        return genres;
    }

    [HttpGet("/software/{softwareId:ulong}/alternative-titles")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareAlternativeTitleDto>> GetAlternativeTitlesAsync(ulong softwareId,
        [FromQuery] string lang = null)
    {
        string langCode  = LanguageResolver.Resolve(HttpContext, lang);
        bool   isEnglish = string.Equals(langCode, "eng", StringComparison.Ordinal);

        IQueryable<SoftwareAlternativeTitle> source = context.SoftwareAlternativeTitles
                                                              .Where(t => t.SoftwareId == softwareId)
                                                              .OrderBy(t => t.Id);

        // English fast-path: skip the correlated translation sub-query entirely. Comment and
        // CanonicalComment are identical in this case.
        if(isEnglish)
            return source.Select(t => new SoftwareAlternativeTitleDto
                          {
                              Id               = t.Id,
                              SoftwareId       = t.SoftwareId,
                              Title            = t.Title,
                              Comment          = t.Comment,
                              CanonicalComment = t.Comment
                          })
                         .ToListAsync();

        return source.Select(t => new SoftwareAlternativeTitleDto
                      {
                          Id         = t.Id,
                          SoftwareId = t.SoftwareId,
                          Title      = t.Title,
                          Comment = t.Comment == null
                                        ? null
                                        : (context.SoftwareAlternativeTitleCommentTranslations
                                                  .Where(c => c.CommentText  == t.Comment &&
                                                              c.LanguageCode == langCode)
                                                  .Select(c => c.Translation)
                                                  .FirstOrDefault() ?? t.Comment),
                          CanonicalComment = t.Comment
                      })
                     .ToListAsync();
    }

    [HttpGet("/software/{softwareId:ulong}/external-ids")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareExternalIdDto>> GetExternalIdsAsync(ulong softwareId) =>
        context.SoftwareExternalIds.Where(e => e.SoftwareId == softwareId)
               .OrderBy(e => e.Id)
               .Select(e => new SoftwareExternalIdDto
                {
                    Id               = e.Id,
                    SoftwareId       = e.SoftwareId,
                    ExternalSiteId   = e.ExternalSiteId,
                    ExternalSiteName = e.ExternalSite.Name,
                    ExternalId       = e.ExternalId
                })
               .ToListAsync();

    [HttpGet("/software/{softwareId:ulong}/similar")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareSimilarToDto>> GetSimilarSoftwareAsync(ulong softwareId) =>
        context.SoftwareSimilarTo
               .Where(e => e.SoftwareId == softwareId || e.SimilarSoftwareId == softwareId)
               .Select(e => e.SoftwareId == softwareId
                                ? new SoftwareSimilarToDto
                                {
                                    SoftwareId          = softwareId,
                                    SimilarSoftwareId   = e.SimilarSoftwareId,
                                    SimilarSoftwareName = e.SimilarSoftware.Name
                                }
                                : new SoftwareSimilarToDto
                                {
                                    SoftwareId          = softwareId,
                                    SimilarSoftwareId   = e.SoftwareId,
                                    SimilarSoftwareName = e.Software.Name
                                })
               .OrderBy(e => e.SimilarSoftwareName)
               .ToListAsync();

    [HttpGet("/software/{softwareId:ulong}/attributes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareAttributeDto>> GetAttributesAsync(ulong softwareId,
                                                                     [FromQuery] string lang = null)
    {
        List<SoftwareAttributeDto> attributes = await context.SoftwareAttributes
           .Where(a => a.SoftwareRelease.SoftwareId == softwareId)
           .Select(a => new SoftwareAttributeDto
            {
                Id                = a.Id,
                SoftwareReleaseId = a.SoftwareReleaseId,
                Category          = a.Category,
                Key               = a.Key,
                Value             = a.Value,
                PlatformName      = a.SoftwareRelease.Platform.Name,
                RegionNames       = string.Join(", ", a.SoftwareRelease.Regions.Select(r => r.UnM49.Name))
            })
           .OrderBy(a => a.PlatformName)
           .ThenBy(a => a.Category)
           .ThenBy(a => a.Key)
           .ToListAsync();

        // Importer stores attribute keys/values with U+00A0 (non-breaking space) — normalise here
        // before lookup. Rating-category attributes are returned verbatim (only normalised); every
        // other category is translated via the SoftwareAttributeTranslationCache. The cache falls
        // back to the (normalised) original text when no translation row exists for the requested
        // language, so callers always receive a populated string.
        string resolvedLang = ResolveGenreLanguage(lang);

        const string ratingCategory = Suggestions.SoftwareReleaseSuggestionApplier.AttributeCategoryRating;

        foreach(SoftwareAttributeDto a in attributes)
        {
            if(string.Equals(a.Category, ratingCategory, StringComparison.Ordinal))
            {
                a.Key   = SoftwareAttributeTranslationCache.NormalizeText(a.Key);
                a.Value = SoftwareAttributeTranslationCache.NormalizeText(a.Value);

                continue;
            }

            a.Key   = await attrCache.GetTranslatedAsync(a.Key,   resolvedLang, HttpContext.RequestAborted);
            a.Value = await attrCache.GetTranslatedAsync(a.Value, resolvedLang, HttpContext.RequestAborted);
        }

        return attributes;
    }

    [HttpGet("/software/{softwareId:ulong}/credits")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<PersonBySoftwareDto>> GetCreditsAsync(ulong softwareId,
                                                                 [FromQuery] string lang = null)
    {
        string langCode  = LanguageResolver.Resolve(HttpContext, lang);
        bool   isEnglish = string.Equals(langCode, "eng", StringComparison.Ordinal);

        IQueryable<PeopleBySoftware> source = context.PeopleBySoftware.Where(p => p.SoftwareId == softwareId);

        // English fast-path: skip the correlated translation sub-query entirely. Role and
        // CanonicalRole are identical in this case.
        List<PersonBySoftwareDto> credits = isEnglish
                                                ? await source.Select(p => new PersonBySoftwareDto
                                                              {
                                                                  Id            = p.Id,
                                                                  PersonId      = p.PersonId,
                                                                  SoftwareId    = p.SoftwareId,
                                                                  Role          = p.DocumentRole != null
                                                                                      ? p.DocumentRole.Name
                                                                                      : p.Role,
                                                                  CanonicalRole = p.DocumentRole != null
                                                                                      ? p.DocumentRole.Name
                                                                                      : p.Role,
                                                                  SoftwareName  = p.Software.Name,
                                                                  Name          = p.Person.Name,
                                                                  Surname       = p.Person.Surname,
                                                                  Alias         = p.Person.Alias,
                                                                  DisplayName   = p.Person.DisplayName
                                                              })
                                                              .ToListAsync()
                                                : await source.Select(p => new PersonBySoftwareDto
                                                              {
                                                                  Id           = p.Id,
                                                                  PersonId     = p.PersonId,
                                                                  SoftwareId   = p.SoftwareId,
                                                                  Role = (p.DocumentRole != null
                                                                              ? p.DocumentRole.Name
                                                                              : p.Role) == null
                                                                             ? null
                                                                             : (context.PeopleBySoftwareRoleTranslations
                                                                                       .Where(t => t.RoleText ==
                                                                                                   (p.DocumentRole !=
                                                                                                    null
                                                                                                        ? p.DocumentRole
                                                                                                            .Name
                                                                                                        : p.Role) &&
                                                                                                   t.LanguageCode ==
                                                                                                   langCode)
                                                                                       .Select(t => t.Translation)
                                                                                       .FirstOrDefault() ??
                                                                                (p.DocumentRole != null
                                                                                     ? p.DocumentRole.Name
                                                                                     : p.Role)),
                                                                  CanonicalRole = p.DocumentRole != null
                                                                                      ? p.DocumentRole.Name
                                                                                      : p.Role,
                                                                  SoftwareName = p.Software.Name,
                                                                  Name         = p.Person.Name,
                                                                  Surname      = p.Person.Surname,
                                                                  Alias        = p.Person.Alias,
                                                                  DisplayName  = p.Person.DisplayName
                                                              })
                                                              .ToListAsync();

        await BackfillCreditFrontCoverIdAsync(credits, softwareId);

        return credits.OrderBy(p => p.Role, StringComparer.CurrentCultureIgnoreCase)
                      .ThenBy(p => p.FullName, StringComparer.CurrentCultureIgnoreCase)
                      .ToList();
    }

    /// <summary>
    ///     Sets <see cref="PersonBySoftwareDto.FrontCoverId" /> on every credit row in
    ///     <paramref name="credits" /> to the front cover of
    ///     <paramref name="softwareId" /> (the single Software whose credits this list
    ///     describes). One bounded lookup via <see cref="SoftwareCoverLookup" />.
    /// </summary>
    async Task BackfillCreditFrontCoverIdAsync(List<PersonBySoftwareDto> credits, ulong softwareId)
    {
        if(credits.Count == 0) return;

        Dictionary<ulong, Guid> covers =
            await SoftwareCoverLookup.LookupFrontCoversAsync(context, new[] { softwareId });

        if(!covers.TryGetValue(softwareId, out Guid coverId)) return;

        foreach(PersonBySoftwareDto dto in credits)
            dto.FrontCoverId = coverId;
    }

    /// <summary>
    ///     Returns the DISTINCT role-pair entries currently used in the
    ///     <c>PeopleBySoftware</c> junction. Each entry carries both the localized
    ///     <see cref="SoftwareCreditRoleDto.Role" /> (for the autocomplete display in the
    ///     contributor's UI language, with English fallback when no translation row exists yet)
    ///     and the canonical English <see cref="SoftwareCreditRoleDto.CanonicalRole" /> (for
    ///     the credits-suggestion dialog to send canonical English on the wire regardless of UI
    ///     locale). Used as the autocomplete data source for the credits role field, allowing
    ///     reuse of established role labels while still permitting custom new entries.
    /// </summary>
    [HttpGet("/software/credits/roles")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareCreditRoleDto>> GetCreditsRolesAsync([FromQuery] string lang = null)
    {
        string langCode  = LanguageResolver.Resolve(HttpContext, lang);
        bool   isEnglish = string.Equals(langCode, "eng", StringComparison.Ordinal);

        IQueryable<string> distinctRoles = context.PeopleBySoftware.AsNoTracking()
                                                  .Where(r => !string.IsNullOrEmpty(r.Role))
                                                  .Select(r => r.Role)
                                                  .Distinct();

        // English fast-path: skip the correlated translation sub-query.
        if(isEnglish)
            return distinctRoles.OrderBy(r => r)
                                .Select(r => new SoftwareCreditRoleDto
                                 {
                                     Role          = r,
                                     CanonicalRole = r
                                 })
                                .ToListAsync();

        return distinctRoles.Select(r => new SoftwareCreditRoleDto
                             {
                                 Role = context.PeopleBySoftwareRoleTranslations
                                               .Where(t => t.RoleText == r && t.LanguageCode == langCode)
                                               .Select(t => t.Translation)
                                               .FirstOrDefault() ?? r,
                                 CanonicalRole = r
                             })
                            .OrderBy(r => r.CanonicalRole)
                            .ToListAsync();
    }

    [HttpGet("{id:ulong}/critic-reviews")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<SoftwareCriticReviewDto>> GetCriticReviewsAsync(ulong id) =>
        await context.SoftwareCriticReviews
                     .Where(r => r.SoftwareId == id)
                     .OrderByDescending(r => r.NormalizedScore)
                     .Select(r => new SoftwareCriticReviewDto
                      {
                          Id                   = r.Id,
                          SoftwareId           = r.SoftwareId,
                          MagazineId           = r.MagazineId,
                          MagazineTitle        = r.Magazine.Title,
                          PlatformId           = r.PlatformId,
                          PlatformName         = r.Platform != null ? r.Platform.Name : null,
                          NormalizedScore      = r.NormalizedScore,
                          OriginalScore        = r.OriginalScore,
                          OriginalScoreMaximum = r.OriginalScoreMaximum,
                          ReviewText           = r.ReviewText,
                          ReviewDate           = r.ReviewDate,
                          ReviewDatePrecision  = r.ReviewDatePrecision,
                          ReviewUrl            = r.ReviewUrl
                      })
                     .ToListAsync();

    [HttpGet("{id:ulong}/critic-reviews/summary")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<CriticReviewSummaryDto> GetCriticReviewSummaryAsync(ulong id)
    {
        var reviews = await context.SoftwareCriticReviews
                                   .Where(r => r.SoftwareId == id)
                                   .Select(r => new
                                    {
                                        r.NormalizedScore,
                                        r.PlatformId,
                                        PlatformName = r.Platform != null ? r.Platform.Name : null
                                    })
                                   .ToListAsync();

        var scored = reviews.Where(r => r.NormalizedScore.HasValue).ToList();

        return new CriticReviewSummaryDto
        {
            AverageScore = scored.Count > 0 ? scored.Average(r => r.NormalizedScore.Value) : null,
            TotalReviews = reviews.Count,
            ByPlatform = reviews.GroupBy(r => new { r.PlatformId, r.PlatformName })
                                .Select(g =>
                                 {
                                     var platformScored = g.Where(r => r.NormalizedScore.HasValue).ToList();

                                     return new PlatformReviewSummaryDto
                                     {
                                         PlatformId   = g.Key.PlatformId,
                                         PlatformName = g.Key.PlatformName,
                                         AverageScore = platformScored.Count > 0
                                                            ? platformScored.Average(r => r.NormalizedScore.Value)
                                                            : null,
                                         ReviewCount = g.Count()
                                     };
                                 })
                                .OrderByDescending(p => p.AverageScore)
                                .ToList()
        };
    }

    // ── User Ratings ──

    [HttpGet("{id:ulong}/user-ratings/summary")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<UserReviewSummaryDto> GetUserReviewSummaryAsync(ulong id)
    {
        // Single round-trip: aggregate ratings + count reviews via grouped subqueries.
        // Was previously two awaited queries (Select+ToListAsync then CountAsync)
        // which doubled the latency of this endpoint.
        var summary = await context.Softwares
                                   .Where(s => s.Id == id)
                                   .Select(s => new
                                    {
                                        AvgRating    = (double?)s.UserRatings.Average(r => (double?)r.Rating),
                                        TotalRatings = s.UserRatings.Count(),
                                        TotalReviews = s.UserReviews.Count()
                                    })
                                   .FirstOrDefaultAsync();

        if(summary is null)
            return new UserReviewSummaryDto
            {
                AverageRating = null,
                TotalRatings  = 0,
                TotalReviews  = 0
            };

        return new UserReviewSummaryDto
        {
            AverageRating = summary.AvgRating,
            TotalRatings  = summary.TotalRatings,
            TotalReviews  = summary.TotalReviews
        };
    }

    [HttpGet("{id:ulong}/user-ratings/me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SoftwareUserRatingDto>> GetMyRatingAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        SoftwareUserRating rating =
            await context.SoftwareUserRatings.FirstOrDefaultAsync(r => r.UserId == userId && r.SoftwareId == id);

        if(rating is null) return NoContent();

        return Ok(new SoftwareUserRatingDto
        {
            UserId     = rating.UserId,
            SoftwareId = rating.SoftwareId,
            Rating     = rating.Rating,
            CreatedOn  = rating.CreatedOn,
            UpdatedOn  = rating.UpdatedOn
        });
    }

    [HttpPut("{id:ulong}/user-ratings/me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertMyRatingAsync(ulong id, [FromBody] SetRatingRequest dto)
    {
        if(dto.Rating < 0 || dto.Rating > 5)
            return Problem(detail: "Rating must be between 0 and 5.", statusCode: StatusCodes.Status400BadRequest);

        // Snap to nearest 0.5
        float snapped = MathF.Round(dto.Rating * 2f) / 2f;

        string userId = User.FindFirstValue(ClaimTypes.Sid);

        SoftwareUserRating existing =
            await context.SoftwareUserRatings.FirstOrDefaultAsync(r => r.UserId == userId && r.SoftwareId == id);

        if(existing is not null)
        {
            existing.Rating = snapped;
        }
        else
        {
            context.SoftwareUserRatings.Add(new SoftwareUserRating
            {
                UserId     = userId,
                SoftwareId = id,
                Rating     = snapped
            });
        }

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:ulong}/user-ratings/me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteMyRatingAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        SoftwareUserRating rating =
            await context.SoftwareUserRatings.FirstOrDefaultAsync(r => r.UserId == userId && r.SoftwareId == id);

        if(rating is not null)
        {
            context.SoftwareUserRatings.Remove(rating);
            await context.SaveChangesWithUserAsync(userId);
        }

        return NoContent();
    }

    // ── User Reviews ──

    [HttpGet("{id:ulong}/user-reviews")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<SoftwareUserReviewDto>> GetUserReviewsAsync(ulong id)
    {
        string callerId = User.FindFirstValue(ClaimTypes.Sid);
        bool   isAdmin  = User.IsInRole("Admin") || User.IsInRole("UberAdmin");

        // Single round-trip: pull each reviewer's rating in the same projection
        // (was previously two awaited queries: reviews then a batched ratings dict).
        var reviews = await context.SoftwareUserReviews
                                   .Where(r => r.SoftwareId == id)
                                   .OrderByDescending(r => r.CreatedOn)
                                   .Select(r => new
                                    {
                                        r.Id,
                                        r.UserId,
                                        UserName    = r.User != null ? r.User.UserName : null,
                                        DisplayName = r.User != null ? r.User.DisplayName : null,
                                        r.User,
                                        r.SoftwareId,
                                        SoftwareName = r.Software.Name,
                                        r.TheGood,
                                        r.TheBad,
                                        r.TheUgly,
                                        r.IsAnonymous,
                                        r.CreatedOn,
                                        r.UpdatedOn,
                                        ThumbsUp    = r.Votes.Count(v => v.IsUpvote),
                                        ThumbsDown  = r.Votes.Count(v => !v.IsUpvote),
                                        ReportCount = r.Reports.Count,
                                        CallerVote = callerId != null
                                                         ? r.Votes
                                                            .Where(v => v.UserId == callerId)
                                                            .Select(v => (bool?)v.IsUpvote)
                                                            .FirstOrDefault()
                                                         : null,
                                        ReviewerRating = r.UserId != null
                                                             ? context.SoftwareUserRatings
                                                                      .Where(rt => rt.SoftwareId == id &&
                                                                                   rt.UserId     == r.UserId)
                                                                      .Select(rt => (float?)rt.Rating)
                                                                      .FirstOrDefault()
                                                             : null
                                    })
                                   .ToListAsync();

        // Batch lookup of Collaborator role membership across all reviewer ids in this batch.
        var reviewerIds = reviews.Where(r => r.UserId != null).Select(r => r.UserId).Distinct().ToHashSet(StringComparer.Ordinal);
        var collaboratorIds = new HashSet<string>(StringComparer.Ordinal);

        if(reviewerIds.Count > 0)
        {
            foreach(ApplicationUser u in await userManager.GetUsersInRoleAsync("Collaborator"))
                if(reviewerIds.Contains(u.Id)) collaboratorIds.Add(u.Id);
        }

        return reviews.Select(r =>
        {
            bool showIdentity = !r.IsAnonymous || isAdmin || r.UserId == callerId;

            return new SoftwareUserReviewDto
            {
                Id              = r.Id,
                UserId          = showIdentity ? r.UserId : null,
                UserName        = showIdentity ? r.UserName : null,
                DisplayName     = showIdentity ? r.DisplayName : null,
                AvatarUrl       = showIdentity && r.User != null ? GetAvatarUrl(r.User) : null,
                SoftwareId      = r.SoftwareId,
                SoftwareName    = r.SoftwareName,
                TheGood         = r.TheGood,
                TheBad          = r.TheBad,
                TheUgly         = r.TheUgly,
                IsAnonymous     = r.IsAnonymous,
                Rating          = r.ReviewerRating,
                ThumbsUp        = r.ThumbsUp,
                ThumbsDown      = r.ThumbsDown,
                CurrentUserVote = r.CallerVote,
                ReportCount     = isAdmin ? r.ReportCount : 0,
                CreatedOn       = r.CreatedOn,
                UpdatedOn       = r.UpdatedOn,
                IsCollaborator  = showIdentity && r.UserId != null && collaboratorIds.Contains(r.UserId)
            };
        }).ToList();
    }

    [HttpPost("{id:ulong}/user-reviews")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SoftwareUserReviewDto>> CreateUserReviewAsync(ulong id,
        [FromBody] SoftwareUserReviewDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        bool exists = await context.SoftwareUserReviews.AnyAsync(r => r.UserId == userId && r.SoftwareId == id);

        if(exists) return Problem(detail: "You have already reviewed this software.", statusCode: StatusCodes.Status409Conflict);

        var review = new SoftwareUserReview
        {
            UserId      = userId,
            SoftwareId  = id,
            TheGood     = dto.TheGood,
            TheBad      = dto.TheBad,
            TheUgly     = dto.TheUgly,
            IsAnonymous = dto.IsAnonymous
        };

        context.SoftwareUserReviews.Add(review);

        // Also upsert rating if provided
        if(dto.Rating is >= 0 and <= 5)
        {
            SoftwareUserRating existingRating =
                await context.SoftwareUserRatings.FirstOrDefaultAsync(r => r.UserId == userId && r.SoftwareId == id);

            if(existingRating is not null)
                existingRating.Rating = dto.Rating.Value;
            else
                context.SoftwareUserRatings.Add(new SoftwareUserRating
                {
                    UserId     = userId,
                    SoftwareId = id,
                    Rating     = dto.Rating.Value
                });
        }

        await context.SaveChangesWithUserAsync(userId);

        dto.Id = review.Id;

        return CreatedAtAction(null, dto);
    }

    [HttpPut("{id:ulong}/user-reviews/{reviewId:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserReviewAsync(ulong id, long reviewId,
                                                           [FromBody] SoftwareUserReviewDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        bool   isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");

        SoftwareUserReview review = await context.SoftwareUserReviews.FindAsync(reviewId);

        if(review is null || review.SoftwareId != id) return NotFound();

        if(review.UserId != userId && !isAdmin)
            return Problem("You do not have permission to edit this review.", statusCode: StatusCodes.Status403Forbidden);

        review.TheGood     = dto.TheGood;
        review.TheBad      = dto.TheBad;
        review.TheUgly     = dto.TheUgly;
        review.IsAnonymous = dto.IsAnonymous;

        // Also upsert the review author's rating if provided and this is the author editing
        if(dto.Rating is >= 0 and <= 5 && review.UserId == userId)
        {
            SoftwareUserRating existingRating =
                await context.SoftwareUserRatings.FirstOrDefaultAsync(r => r.UserId == userId && r.SoftwareId == id);

            if(existingRating is not null)
                existingRating.Rating = dto.Rating.Value;
            else
                context.SoftwareUserRatings.Add(new SoftwareUserRating
                {
                    UserId     = userId,
                    SoftwareId = id,
                    Rating     = dto.Rating.Value
                });
        }

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:ulong}/user-reviews/{reviewId:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUserReviewAsync(ulong id, long reviewId)
    {
        string userId  = User.FindFirstValue(ClaimTypes.Sid);
        bool   isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");

        SoftwareUserReview review = await context.SoftwareUserReviews.FindAsync(reviewId);

        if(review is null || review.SoftwareId != id) return NotFound();

        if(review.UserId != userId && !isAdmin)
            return Problem("You do not have permission to delete this review.", statusCode: StatusCodes.Status403Forbidden);

        context.SoftwareUserReviews.Remove(review);
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    // ── Review Votes ──

    [HttpPost("{id:ulong}/user-reviews/{reviewId:long}/vote")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VoteOnReviewAsync(ulong id, long reviewId, [FromBody] SoftwareUserReviewVoteDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        SoftwareUserReview review = await context.SoftwareUserReviews.FindAsync(reviewId);

        if(review is null || review.SoftwareId != id) return NotFound();

        if(review.UserId == userId)
            return Problem("You cannot vote on your own review.", statusCode: StatusCodes.Status403Forbidden);

        SoftwareUserReviewVote existing =
            await context.SoftwareUserReviewVotes.FirstOrDefaultAsync(v => v.UserId == userId && v.ReviewId == reviewId);

        if(existing is not null)
            existing.IsUpvote = dto.IsUpvote;
        else
            context.SoftwareUserReviewVotes.Add(new SoftwareUserReviewVote
            {
                UserId   = userId,
                ReviewId = reviewId,
                IsUpvote = dto.IsUpvote
            });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:ulong}/user-reviews/{reviewId:long}/vote")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveVoteAsync(ulong id, long reviewId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        SoftwareUserReviewVote vote =
            await context.SoftwareUserReviewVotes.FirstOrDefaultAsync(v => v.UserId == userId && v.ReviewId == reviewId);

        if(vote is not null)
        {
            context.SoftwareUserReviewVotes.Remove(vote);
            await context.SaveChangesWithUserAsync(userId);
        }

        return NoContent();
    }

    // ── Review Reports ──

    [HttpPost("{id:ulong}/user-reviews/{reviewId:long}/report")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReportReviewAsync(ulong id, long reviewId,
                                                       [FromBody] CreateReviewReportRequest dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        SoftwareUserReview review =
            await context.SoftwareUserReviews.Include(r => r.Software).FirstOrDefaultAsync(r => r.Id == reviewId);

        if(review is null || review.SoftwareId != id) return NotFound();

        if(review.UserId == userId)
            return Problem("You cannot report your own review.", statusCode: StatusCodes.Status403Forbidden);

        bool alreadyReported =
            await context.ReviewReports.AnyAsync(r => r.ReporterId == userId && r.ReviewId == reviewId);

        if(alreadyReported) return Problem(detail: "You have already reported this review.", statusCode: StatusCodes.Status409Conflict);

        context.ReviewReports.Add(new ReviewReport
        {
            ReporterId  = userId,
            ReviewId    = reviewId,
            Reason      = dto.Reason,
            Explanation = dto.Explanation
        });

        await context.SaveChangesWithUserAsync(userId);

        // Notify all admins/uberadmins via the messaging system. Sent post-save so the report row exists when admins
        // click through. Failures are non-fatal: the report itself is what matters.
        try
        {
            string explanationBlock = string.IsNullOrWhiteSpace(dto.Explanation)
                                          ? string.Empty
                                          : $"\n\n> {dto.Explanation}\n";

            await MessageDispatcher.SendSystemMessageToAdminsAsync(context,
                userManager,
                subject: $"Review reported: {review.Software.Name}",
                body:
                $"A user review on **{review.Software.Name}** has been reported as **{dto.Reason}**.{explanationBlock}\n\n" +
                "[Open the Review Reports page](/admin/review-reports)");
        }
        catch
        {
            // Swallow — report is persisted; admins will still see it on the Review Reports page.
        }

        return Created();
    }

    // ── Marechai Score ──

    [HttpGet("{id:ulong}/marechai-score")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<MarechaiScoreDto> GetMarechaiScoreAsync(ulong id)
    {
        // Single-query lookup against the precomputed SoftwareScores table populated
        // by MarechaiRankingsWorker. CriticScore / UserScore are the per-side raw
        // averages normalised to the 0-10 scale; MarechaiScore is the Bayesian-shrunken
        // value. Items below the fairness threshold (>= MIN_CRITIC_COUNT critics OR
        // >= MIN_USER_COUNT users) are absent from the table and surface as nulls.
        var row = await context.SoftwareScores
                               .AsNoTracking()
                               .Where(s => s.SoftwareId == id)
                               .Select(s => new
                                {
                                    s.Score,
                                    s.GlobalRank,
                                    s.CriticAverage,
                                    s.UserStarAverage
                                })
                               .FirstOrDefaultAsync();

        int totalRanked = await context.SoftwareScores.AsNoTracking().CountAsync();

        if(row is null)
            return new MarechaiScoreDto
            {
                CriticScore   = null,
                UserScore     = null,
                MarechaiScore = null,
                Rank          = null,
                TotalRanked   = totalRanked
            };

        return new MarechaiScoreDto
        {
            CriticScore   = row.CriticAverage.HasValue   ? Math.Round(row.CriticAverage.Value   / 10.0, 1) : null,
            UserScore     = row.UserStarAverage.HasValue ? Math.Round(row.UserStarAverage.Value * 2.0,  1) : null,
            MarechaiScore = Math.Round(row.Score, 1),
            Rank          = row.GlobalRank,
            TotalRanked   = totalRanked
        };
    }

    // ── Marechai Rankings: persisted multi-dimensional rankings ──────────────
    // The legacy in-memory snapshot + per-filter cache has been replaced by
    // three DB-backed tables (SoftwareScores, RankingDefinitions, RankingEntries)
    // populated by MarechaiRankingsWorker on startup + every 24 h. The read
    // endpoints below project from those tables; the per-request translation
    // pipeline runs at the controller layer so that newly-translated genre
    // names surface immediately without needing a rankings recompute.

    /// <summary>
    ///     Builds the localised display name for a ranking dimension. Overall is a fixed
    ///     literal (translated client-side); genres are translated via the in-memory
    ///     <see cref="SoftwareGenreTranslationCache" /> with English fallback and NBSP
    ///     normalisation, matching the convention of <c>GetAllGenresAsync</c>; platforms
    ///     are proper nouns and have no translation table — they pass through verbatim.
    /// </summary>
    async Task<string> BuildDimensionDisplayNameAsync(RankingDimension  dimension,
                                                       long?             dimensionId,
                                                       string            rawGenreName,
                                                       string            rawPlatformName,
                                                       string            resolvedLang,
                                                       CancellationToken ct)
    {
        switch(dimension)
        {
            case RankingDimension.All:
                return "Overall";

            case RankingDimension.Genre:
            {
                if(!dimensionId.HasValue) return rawGenreName ?? string.Empty;

                string translated =
                    await genreCache.GetNameAsync((int)dimensionId.Value, resolvedLang, ct);

                string display = string.IsNullOrEmpty(translated) ? rawGenreName : translated;

                return display?.Replace('\u00A0', ' ') ?? string.Empty;
            }

            case RankingDimension.Platform:
                return rawPlatformName ?? string.Empty;

            default:
                return string.Empty;
        }
    }

    /// <summary>
    ///     Index of every available ranking the worker has materialised. Returned alongside
    ///     a freshness snapshot so the frontend can render a "rankings are being computed"
    ///     banner on a fresh install (LastComputedAt null AND IsComputing true). Sorted:
    ///     All first, then per-genre rankings alphabetically by translated genre name, then
    ///     per-platform rankings alphabetically by raw platform name.
    /// </summary>
    [HttpGet("rankings/index")]
    [AllowAnonymous]
    [OutputCache(Duration = 300, VaryByQueryKeys = ["lang"])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<RankingIndexResponseDto> GetRankingsIndexAsync([FromQuery] string lang = null,
                                                                     CancellationToken cancellationToken = default)
    {
        string resolvedLang = ResolveGenreLanguage(lang);

        await genreCache.EnsureLoadedAsync(cancellationToken);

        // Pull every definition in one round-trip, joined to the genre / platform tables
        // for the raw display name. Genre names are translated below; platform names pass
        // through as-is (no translation table — proper nouns).
        var defs = await context.RankingDefinitions
                                .AsNoTracking()
                                .OrderBy(d => d.Dimension)
                                .ThenBy(d => d.Id)
                                .Select(d => new
                                 {
                                     d.Id,
                                     d.Dimension,
                                     d.DimensionId,
                                     d.EntryCount,
                                     d.ComputedAt,
                                     GenreName = d.Dimension == RankingDimension.Genre && d.DimensionId.HasValue
                                                     ? context.SoftwareGenres
                                                              .Where(g => g.Id == (int)d.DimensionId.Value)
                                                              .Select(g => g.Name)
                                                              .FirstOrDefault()
                                                     : null,
                                     GenreType = d.Dimension == RankingDimension.Genre && d.DimensionId.HasValue
                                                     ? context.SoftwareGenres
                                                              .Where(g => g.Id == (int)d.DimensionId.Value)
                                                              .Select(g => (byte?)g.Type)
                                                              .FirstOrDefault()
                                                     : null,
                                     PlatformName = d.Dimension == RankingDimension.Platform && d.DimensionId.HasValue
                                                        ? context.SoftwarePlatforms
                                                                 .Where(p => p.Id == (ulong)d.DimensionId.Value)
                                                                 .Select(p => p.Name)
                                                                 .FirstOrDefault()
                                                        : null
                                 })
                                .ToListAsync(cancellationToken);

        var entries = new List<RankingIndexEntryDto>(defs.Count);

        foreach(var d in defs)
        {
            string name = await BuildDimensionDisplayNameAsync(d.Dimension,
                                                               d.DimensionId,
                                                               d.GenreName,
                                                               d.PlatformName,
                                                               resolvedLang,
                                                               cancellationToken);

            entries.Add(new RankingIndexEntryDto
            {
                Id            = d.Id,
                Dimension     = (byte)d.Dimension,
                DimensionId   = d.DimensionId,
                DimensionName = name,
                EntryCount    = d.EntryCount,
                GenreType     = d.GenreType,
                ComputedAt    = d.ComputedAt
            });
        }

        // Stable secondary sort by translated name within each dimension. The DB pass
        // returned them ordered by (Dimension, Id); we now re-sort the genre + platform
        // groups by display name so the UI shows them alphabetically in the requested
        // language.
        List<RankingIndexEntryDto> sorted = entries
                                            .OrderBy(e => e.Dimension)
                                            .ThenBy(e => e.DimensionName,
                                                    StringComparer.CurrentCultureIgnoreCase)
                                            .ToList();

        return new RankingIndexResponseDto
        {
            Status = new RankingsStatusDto
            {
                LastComputedAt = rankingsState.LastComputedAt,
                IsComputing    = rankingsState.IsComputing,
                TotalRankings  = sorted.Count
            },
            Rankings = sorted
        };
    }

    /// <summary>
    ///     Top-N entries for a single ranking by <c>RankingDefinitions.Id</c>. Hard-capped
    ///     server-side by what the worker stored (MARECHAI_RANKING_TOP_N = 250). Returns
    ///     404 if the id does not match a materialised definition.
    /// </summary>
    [HttpGet("rankings/{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<SoftwareRankingDto>>> GetRankingAsync(
        int id, CancellationToken cancellationToken = default)
    {
        // Validate the definition exists so the FE can show a "not found" page rather
        // than rendering an empty table for a stale URL.
        bool exists = await context.RankingDefinitions
                                   .AsNoTracking()
                                   .AnyAsync(d => d.Id == (uint)id, cancellationToken);

        if(!exists) return NotFound();

        var rows = await context.RankingEntries
                                .AsNoTracking()
                                .Where(e => e.RankingDefinitionId == (uint)id)
                                .OrderBy(e => e.Rank)
                                .Join(context.Softwares,
                                      e => e.SoftwareId,
                                      s => s.Id,
                                      (e, s) => new
                                      {
                                          e.Rank,
                                          e.Score,
                                          s.Id,
                                          s.Name,
                                          s.Kind,
                                          Family = s.Family != null ? s.Family.Name : null,
                                          CriticAverage = s.CriticReviews
                                                           .Where(r => r.NormalizedScore != null)
                                                           .Select(r => (double?)r.NormalizedScore)
                                                           .Average(),
                                          UserAverage = s.UserRatings
                                                         .Select(r => (double?)r.Rating)
                                                         .Average(),
                                          CriticCount = s.CriticReviews.Count(r => r.NormalizedScore != null),
                                          UserCount   = s.UserRatings.Count
                                      })
                                .ToListAsync(cancellationToken);

        List<SoftwareRankingDto> ranked = rows.Select(r => new SoftwareRankingDto
                                                      {
                                                          Rank              = r.Rank,
                                                          SoftwareId        = r.Id,
                                                          Name              = r.Name,
                                                          Kind              = r.Kind,
                                                          Family            = r.Family,
                                                          MarechaiScore     = Math.Round(r.Score, 1),
                                                          CriticAverage     = r.CriticAverage,
                                                          UserStarAverage   = r.UserAverage,
                                                          CriticReviewCount = r.CriticCount,
                                                          UserRatingCount   = r.UserCount
                                                      })
                                              .ToList();

        // Backfill FrontCoverId via the existing helper that drives the rest of the
        // software-listing UI. The shim's Name is left empty because the helper only
        // needs (Id, IsCompilation) to issue its batched cover lookup.
        List<SoftwareDto> coverShim = ranked.Select(r => new SoftwareDto
                                             {
                                                 Id            = r.SoftwareId,
                                                 Name          = string.Empty,
                                                 IsCompilation = false
                                             })
                                            .ToList();

        await PopulateFrontCoverIdsAsync(coverShim, cancellationToken);

        for(int i = 0; i < ranked.Count; i++) ranked[i].FrontCoverId = coverShim[i].FrontCoverId;

        return ranked;
    }

    /// <summary>
    ///     Every ranking the given software appears in. Drives the chip row under the
    ///     Marechai-score banner on the software detail page. Genre dimension names are
    ///     translated; platform names are passed through as-is. Sorted by dimension (All →
    ///     Genre → Platform) and then by rank ascending.
    /// </summary>
    [HttpGet("{softwareId:ulong}/rankings")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<SoftwareRankingPlacementDto>> GetSoftwareRankingPlacementsAsync(
        ulong softwareId, [FromQuery] string lang = null, CancellationToken cancellationToken = default)
    {
        string resolvedLang = ResolveGenreLanguage(lang);

        await genreCache.EnsureLoadedAsync(cancellationToken);

        var rows = await context.RankingEntries
                                .AsNoTracking()
                                .Where(e => e.SoftwareId == softwareId)
                                .Join(context.RankingDefinitions,
                                      e => e.RankingDefinitionId,
                                      d => d.Id,
                                      (e, d) => new
                                      {
                                          RankingId = d.Id,
                                          d.Dimension,
                                          d.DimensionId,
                                          d.EntryCount,
                                          e.Rank,
                                          e.Score,
                                          GenreName = d.Dimension == RankingDimension.Genre && d.DimensionId.HasValue
                                                          ? context.SoftwareGenres
                                                                   .Where(g => g.Id == (int)d.DimensionId.Value)
                                                                   .Select(g => g.Name)
                                                                   .FirstOrDefault()
                                                          : null,
                                          PlatformName =
                                              d.Dimension == RankingDimension.Platform && d.DimensionId.HasValue
                                                  ? context.SoftwarePlatforms
                                                           .Where(p => p.Id == (ulong)d.DimensionId.Value)
                                                           .Select(p => p.Name)
                                                           .FirstOrDefault()
                                                  : null
                                      })
                                .ToListAsync(cancellationToken);

        var placements = new List<SoftwareRankingPlacementDto>(rows.Count);

        foreach(var r in rows)
        {
            string name = await BuildDimensionDisplayNameAsync(r.Dimension,
                                                               r.DimensionId,
                                                               r.GenreName,
                                                               r.PlatformName,
                                                               resolvedLang,
                                                               cancellationToken);

            placements.Add(new SoftwareRankingPlacementDto
            {
                RankingId     = r.RankingId,
                Dimension     = (byte)r.Dimension,
                DimensionId   = r.DimensionId,
                DimensionName = name,
                Rank          = r.Rank,
                EntryCount    = r.EntryCount,
                Score         = Math.Round(r.Score, 1)
            });
        }

        return placements
              .OrderBy(p => p.Dimension)
              .ThenBy(p => p.Rank)
              .ToList();
    }

    /// <summary>
    ///     Fires a background recompute of the persisted rankings tables. Returns 202 on
    ///     accept, 409 when a recompute is already in flight. UberAdmin-gated insurance for
    ///     ops; the worker also recomputes automatically every 24 hours.
    /// </summary>
    [HttpPost("rankings/recompute")]
    [Authorize(Roles = "UberAdmin")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult RecomputeRankings()
    {
        if(rankingsState.IsComputing)
            return Conflict(new
            {
                error = "A rankings recompute is already in progress."
            });

        // Fire-and-forget on a non-request scope. The calculator owns its own
        // IDbContextFactory so the request-scoped MarechaiContext does not need to
        // outlive this call. Worker / calculator handle their own logging on failure.
        _ = Task.Run(async () =>
                     {
                         try { await rankingsCalculator.ComputeAllAsync(CancellationToken.None); }
                         catch
                         {
                             // Swallow so an unobserved task exception doesn't crash the
                             // host. The calculator logs failures via ILogger.
                         }
                     });

        return Accepted();
    }

    static string GetAvatarUrl(ApplicationUser user)
    {
        if(user is null) return null;

        if(user.UseGravatar)
        {
            if(string.IsNullOrWhiteSpace(user.Email)) return null;

            string email     = user.Email.Trim().ToLowerInvariant();
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(email));

            var sb = new StringBuilder(hashBytes.Length * 2);

            foreach(byte b in hashBytes)
                sb.Append(b.ToString("x2"));

            return $"https://www.gravatar.com/avatar/{sb}?s=256&d=identicon";
        }

        if(user.AvatarGuid.HasValue)
            return $"photos/avatars/thumbs/jpeg/4k/{user.AvatarGuid}.jpg";

        return null;
    }

    /// <summary>
    ///     Resolves the requested ISO 639-3 language code for the genre endpoints. Delegates to
    ///     <see cref="Marechai.Server.Helpers.LanguageResolver.Resolve" /> — kept as a thin wrapper
    ///     so the existing call sites in this controller need no edits.
    /// </summary>
    string ResolveGenreLanguage(string explicitLang) =>
        Marechai.Server.Helpers.LanguageResolver.Resolve(HttpContext, explicitLang);
}
