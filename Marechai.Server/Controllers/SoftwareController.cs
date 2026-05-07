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
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Server.Helpers;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Marechai.Server.Controllers;

[Route("/software")]
[ApiController]
public class SoftwareController(MarechaiContext context, IMemoryCache cache, UserManager<ApplicationUser> userManager) : ControllerBase
{
    // Cache key + duration for the global Marechai score ranking.
    // The full catalog ranking changes only when reviews/ratings are
    // added; recomputing it on every page load was the dominant cost
    // of /software/{id}/marechai-score.
    const           string   MARECHAI_RANKING_CACHE_KEY = "marechai:ranking";
    static readonly TimeSpan _marechaiRankingTtl        = TimeSpan.FromMinutes(5);

    // Cache keys + TTL for the /software landing-page catalog endpoints.
    // These query the *whole* catalog (all genres, all platforms, all spec
    // values, year range) and barely change between requests, so a short
    // memory cache turns 4 expensive queries into ~0 ms hits.
    const           string   SOFTWARE_GENRES_CACHE_KEY    = "software:genres";
    internal const  string   SOFTWARE_SPECS_CACHE_KEY     = "software:specs";
    const           string   SOFTWARE_PLATFORMS_CACHE_KEY = "software:platforms";
    const           string   SOFTWARE_YEARS_CACHE_KEY     = "software:years";
    const           string   SOFTWARE_COUNT_CACHE_KEY     = "software:count";
    const           string   SOFTWARE_COMPANIES_CACHE_KEY = "software:companies";
    static readonly TimeSpan _catalogCacheTtl             = TimeSpan.FromMinutes(5);

    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetSoftwareCountAsync([FromQuery] string search = null)
    {
        // Fast path: no search filter — use the cached total.
        if(string.IsNullOrWhiteSpace(search))
        {
            if(cache.TryGetValue(SOFTWARE_COUNT_CACHE_KEY, out int cachedCount)) return cachedCount;

            // Single round-trip: count software + compilation releases as a
            // 2-element projection (was two awaited CountAsync calls).
            var totals = await context.Softwares
                                      .Select(_ => 1)
                                      .GroupBy(_ => 1)
                                      .Select(g => new
                                       {
                                           Software = g.Count(),
                                           Compilations =
                                               context.SoftwareReleases.Count(r => r.IsCompilation)
                                       })
                                      .FirstOrDefaultAsync();

            int total = (totals?.Software ?? 0) + (totals?.Compilations ?? 0);
            cache.Set(SOFTWARE_COUNT_CACHE_KEY, total, _catalogCacheTtl);

            return total;
        }

        IQueryable<Database.Models.Software> query = context.Softwares;
        query = query.Where(s => s.Name.Contains(search));

        int softwareCount = await query.CountAsync();

        IQueryable<SoftwareRelease> compQuery =
            context.SoftwareReleases.Where(r => r.IsCompilation && r.Title.Contains(search));

        int compilationCount = await compQuery.CountAsync();

        return softwareCount + compilationCount;
    }

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMinimumYearAsync()
    {
        (int min, _) = await GetYearRangeAsync();

        return min;
    }

    [HttpGet("maximum-year")]
    [AllowAnonymous]
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
    public async Task<List<SoftwareDto>> GetSoftwareByLetterAsync(char c)
    {
        // Single SQL round-trip: UNION ALL the software + compilations projection
        // (was two sequential awaited queries before merging in memory).
        IQueryable<SoftwareDto> softwareQuery = context.Softwares
           .Where(s => EF.Functions.Like(s.Name, $"{c}%"))
           .Select(s => new SoftwareDto
            {
                Id           = s.Id,
                Name         = s.Name,
                FamilyId     = s.FamilyId,
                Family       = s.Family.Name,
                Kind         = s.Kind,
                IsCompilation = false,
                FrontCoverId = context.SoftwareCovers
                                      .Where(c2 => (c2.Release.SoftwareId == s.Id ||
                                                     c2.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                    c2.Type == SoftwareCoverType.Front)
                                      .Select(c2 => (Guid?)c2.Id)
                                      .FirstOrDefault()
            });

        IQueryable<SoftwareDto> compilationsQuery = context.SoftwareReleases
           .Where(r => r.IsCompilation && EF.Functions.Like(r.Title, $"{c}%"))
           .Select(r => new SoftwareDto
            {
                Id            = r.Id,
                Name          = r.Title,
                FamilyId      = null,
                Family        = null,
                Kind          = default,
                IsCompilation = true,
                FrontCoverId  = r.Covers
                                 .Where(cv => cv.Type == SoftwareCoverType.Front)
                                 .Select(cv => (Guid?)cv.Id)
                                 .FirstOrDefault()
            });

        List<SoftwareDto> combined = await softwareQuery.Concat(compilationsQuery).ToListAsync();

        return combined.OrderBy(s => s.Name, NaturalStringComparer.Instance).ToList();
    }

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareByYearAsync(int year)
    {
        // Single SQL round-trip via Concat (UNION ALL).
        IQueryable<SoftwareDto> softwareQuery = context.Softwares
           .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.ReleaseDate != null &&
                                                               r.ReleaseDate.Value.Year == year))
                    || s.DirectReleases.Any(r => r.ReleaseDate != null &&
                                                 r.ReleaseDate.Value.Year == year))
           .Select(s => new SoftwareDto
            {
                Id            = s.Id,
                Name          = s.Name,
                FamilyId      = s.FamilyId,
                Family        = s.Family.Name,
                Kind          = s.Kind,
                IsCompilation = false,
                FrontCoverId = context.SoftwareCovers
                                      .Where(c => (c.Release.SoftwareId == s.Id ||
                                                    c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                   c.Type == SoftwareCoverType.Front)
                                      .Select(c => (Guid?)c.Id)
                                      .FirstOrDefault()
            });

        IQueryable<SoftwareDto> compilationsQuery = context.SoftwareReleases
           .Where(r => r.IsCompilation &&
                        r.ReleaseDate != null &&
                        r.ReleaseDate.Value.Year == year)
           .Select(r => new SoftwareDto
            {
                Id            = r.Id,
                Name          = r.Title,
                FamilyId      = null,
                Family        = null,
                Kind          = default,
                IsCompilation = true,
                FrontCoverId  = r.Covers
                                 .Where(cv => cv.Type == SoftwareCoverType.Front)
                                 .Select(cv => (Guid?)cv.Id)
                                 .FirstOrDefault()
            });

        List<SoftwareDto> combined = await softwareQuery.Concat(compilationsQuery).ToListAsync();

        return combined.OrderBy(s => s.Name, NaturalStringComparer.Instance).ToList();
    }

    [HttpGet("by-platform/{platformId:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareByPlatformAsync(ulong platformId)
    {
        // Single SQL round-trip via Concat (UNION ALL).
        IQueryable<SoftwareDto> softwareQuery = context.Softwares
           .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.PlatformId == platformId))
                    || s.DirectReleases.Any(r => r.PlatformId == platformId))
           .Select(s => new SoftwareDto
            {
                Id            = s.Id,
                Name          = s.Name,
                FamilyId      = s.FamilyId,
                Family        = s.Family.Name,
                Kind          = s.Kind,
                IsCompilation = false,
                FrontCoverId = context.SoftwareCovers
                                      .Where(c => (c.Release.SoftwareId == s.Id ||
                                                    c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                   c.Type == SoftwareCoverType.Front)
                                      .Select(c => (Guid?)c.Id)
                                      .FirstOrDefault()
            });

        IQueryable<SoftwareDto> compilationsQuery = context.SoftwareReleases
           .Where(r => r.IsCompilation && r.PlatformId == platformId)
           .Select(r => new SoftwareDto
            {
                Id            = r.Id,
                Name          = r.Title,
                FamilyId      = null,
                Family        = null,
                Kind          = default,
                IsCompilation = true,
                FrontCoverId  = r.Covers
                                 .Where(cv => cv.Type == SoftwareCoverType.Front)
                                 .Select(cv => (Guid?)cv.Id)
                                 .FirstOrDefault()
            });

        List<SoftwareDto> combined = await softwareQuery.Concat(compilationsQuery).ToListAsync();

        return combined.OrderBy(s => s.Name, NaturalStringComparer.Instance).ToList();
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
                                            [FromQuery] bool sortDescending = false)
    {
        IQueryable<Database.Models.Software> baseSoftwareQuery = context.Softwares;

        if(!string.IsNullOrWhiteSpace(search))
            baseSoftwareQuery = baseSoftwareQuery.Where(s => s.Name.Contains(search));

        IQueryable<SoftwareDto> softwareQuery = baseSoftwareQuery.Select(s => new SoftwareDto
        {
            Id            = s.Id,
            Name          = s.Name,
            FamilyId      = s.FamilyId,
            Family        = s.Family.Name,
            Kind          = s.Kind,
            IsCompilation = false,
            FrontCoverId  = context.SoftwareCovers
                                   .Where(c => (c.Release.SoftwareId == s.Id ||
                                                 c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                c.Type == SoftwareCoverType.Front)
                                   .Select(c => (Guid?)c.Id)
                                   .FirstOrDefault()
        });

        IQueryable<SoftwareRelease> baseCompQuery = context.SoftwareReleases.Where(r => r.IsCompilation);

        if(!string.IsNullOrWhiteSpace(search))
            baseCompQuery = baseCompQuery.Where(r => r.Title.Contains(search));

        IQueryable<SoftwareDto> compilationsQuery = baseCompQuery.Select(r => new SoftwareDto
        {
            Id            = r.Id,
            Name          = r.Title,
            FamilyId      = null,
            Family        = null,
            Kind          = default,
            IsCompilation = true,
            FrontCoverId  = r.Covers
                             .Where(cv => cv.Type == SoftwareCoverType.Front)
                             .Select(cv => (Guid?)cv.Id)
                             .FirstOrDefault()
        });

        // Single SQL round-trip via Concat (UNION ALL).
        List<SoftwareDto> combined = await softwareQuery.Concat(compilationsQuery).ToListAsync();

        IEnumerable<SoftwareDto> merged = combined;

        merged = sortBy switch
        {
            "Name"         => sortDescending ? merged.OrderByDescending(s => s.Name, NaturalStringComparer.Instance)        : merged.OrderBy(s => s.Name, NaturalStringComparer.Instance),
            "Family"       => sortDescending ? merged.OrderByDescending(s => s.Family, NaturalStringComparer.Instance)      : merged.OrderBy(s => s.Family, NaturalStringComparer.Instance),
            "Kind"         => sortDescending ? merged.OrderByDescending(s => s.Kind)                                         : merged.OrderBy(s => s.Kind),
            "BaseSoftware" => sortDescending ? merged.OrderByDescending(s => s.BaseSoftware, NaturalStringComparer.Instance) : merged.OrderBy(s => s.BaseSoftware, NaturalStringComparer.Instance),
            _              => merged.OrderBy(s => s.Name, NaturalStringComparer.Instance)
        };

        if(skip.HasValue) merged = merged.Skip(skip.Value);

        if(take.HasValue) merged = merged.Take(take.Value);

        return merged.ToList();
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
                                                               SuccessorId = context.Softwares
                                                                                    .Where(x => x.PredecessorId == s.Id)
                                                                                    .Select(x => (ulong?)x.Id)
                                                                                    .FirstOrDefault(),
                                                               Successor = context.Softwares
                                                                                  .Where(x => x.PredecessorId == s.Id)
                                                                                  .Select(x => x.Name)
                                                                                  .FirstOrDefault(),
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
        model.Kind              = dto.Kind;
        model.BaseSoftwareId    = dto.BaseSoftwareId;

        if(model.Kind == SoftwareKind.Dlc && model.BaseSoftwareId is null)
            return BadRequest("DLC / Addon software must have a base software.");

        if(model.Kind != SoftwareKind.Dlc && model.BaseSoftwareId is not null)
            return BadRequest("Only DLC / Addon software can have a base software.");

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
            Kind              = dto.Kind,
            BaseSoftwareId    = dto.BaseSoftwareId
        };

        if(model.Kind == SoftwareKind.Dlc && model.BaseSoftwareId is null)
            return BadRequest("DLC / Addon software must have a base software.");

        if(model.Kind != SoftwareKind.Dlc && model.BaseSoftwareId is not null)
            return BadRequest("Only DLC / Addon software can have a base software.");

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

        context.Softwares.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

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
        if(targetId == sourceId) return BadRequest("Cannot merge a software entry into itself.");

        Software target = await context.Softwares.FindAsync(targetId);

        if(target is null) return NotFound("Target software not found.");

        Software source = await context.Softwares.FindAsync(sourceId);

        if(source is null) return NotFound("Source software not found.");

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
        List<ulong> sourceCompilationReleaseIds = await context.SoftwareBySoftwareRelease
                                                               .Where(s => s.SoftwareId == sourceId)
                                                               .Select(s => s.ReleaseId)
                                                               .ToListAsync();

        HashSet<ulong> targetCompilationReleaseIds = (await context.SoftwareBySoftwareRelease
                                                                   .Where(s => s.SoftwareId == targetId)
                                                                   .Select(s => s.ReleaseId)
                                                                   .ToListAsync())
           .ToHashSet();

        int compilationDuplicates =
            sourceCompilationReleaseIds.Count(rid => targetCompilationReleaseIds.Contains(rid));

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
            CompilationReferencesTotal     = sourceCompilationReleaseIds.Count,
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

        if(targetId == sourceId) return BadRequest("Cannot merge a software entry into itself.");

        Software target = await context.Softwares.FindAsync(targetId);

        if(target is null) return NotFound("Target software not found.");

        Software source = await context.Softwares.FindAsync(sourceId);

        if(source is null) return NotFound("Source software not found.");

        if((source.Kind == SoftwareKind.Dlc) != (target.Kind == SoftwareKind.Dlc))
            return Conflict("Cannot merge DLC / Addon software with non-DLC software.");

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

            // 8. Merge SoftwareBySoftwareRelease compilation references (composite PK — must remove+add)
            List<SoftwareBySoftwareRelease> sourceCompilationRefs =
                await context.SoftwareBySoftwareRelease.Where(s => s.SoftwareId == sourceId).ToListAsync();

            HashSet<ulong> targetCompilationReleaseIds = (await context.SoftwareBySoftwareRelease
                                                                       .Where(s => s.SoftwareId == targetId)
                                                                       .Select(s => s.ReleaseId)
                                                                       .ToListAsync())
               .ToHashSet();

            foreach(SoftwareBySoftwareRelease compRef in sourceCompilationRefs)
            {
                context.SoftwareBySoftwareRelease.Remove(compRef);

                if(!targetCompilationReleaseIds.Contains(compRef.ReleaseId))
                {
                    context.SoftwareBySoftwareRelease.Add(new SoftwareBySoftwareRelease
                    {
                        ReleaseId  = compRef.ReleaseId,
                        SoftwareId = targetId
                    });

                    targetCompilationReleaseIds.Add(compRef.ReleaseId);
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
                target.PredecessorId = source.PredecessorId;

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
            await context.SaveChangesAsync();

            // 10. Delete source software
            context.Softwares.Remove(source);
            await context.SaveChangesAsync();

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
    public Task<List<SoftwareDto>> GetAddonsAsync(ulong id) => context.Softwares
       .Where(s => s.BaseSoftwareId == id)
       .OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            Kind              = s.Kind,
            BaseSoftwareId    = s.BaseSoftwareId,
            BaseSoftware      = s.BaseSoftware.Name,
            FrontCoverId = context.SoftwareCovers
                                  .Where(c => (c.Release.SoftwareId == s.Id ||
                                                c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                               c.Type == SoftwareCoverType.Front)
                                  .Select(c => (Guid?)c.Id)
                                  .FirstOrDefault()
        })
       .ToListAsync();

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

        context.SoftwareDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpGet("genres")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareGenreDto>> GetAllGenresAsync()
    {
        if(cache.TryGetValue(SOFTWARE_GENRES_CACHE_KEY, out List<SoftwareGenreDto> cached) && cached is not null)
            return cached;

        List<SoftwareGenreDto> genres = await context.SoftwareGenres
                                                     .Where(g => g.Softwares.Any())
                                                     .OrderBy(g => g.Type)
                                                     .ThenBy(g => g.Name)
                                                     .Select(g => new SoftwareGenreDto
                                                      {
                                                          Id       = g.Id,
                                                          Name     = g.Name,
                                                          Type     = (int)g.Type,
                                                          TypeName = g.Type.ToString()
                                                      })
                                                     .ToListAsync();

        cache.Set(SOFTWARE_GENRES_CACHE_KEY, genres, _catalogCacheTtl);

        return genres;
    }

    [HttpGet("by-genre/{genreId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareByGenreAsync(int genreId)
    {
        // Single SQL round-trip via Concat (UNION ALL).
        IQueryable<SoftwareDto> softwareQuery = context.Softwares
           .Where(s => s.Genres.Any(g => g.GenreId == genreId))
           .Select(s => new SoftwareDto
            {
                Id            = s.Id,
                Name          = s.Name,
                FamilyId      = s.FamilyId,
                Family        = s.Family.Name,
                Kind          = s.Kind,
                IsCompilation = false,
                FrontCoverId = context.SoftwareCovers
                                      .Where(c => (c.Release.SoftwareId == s.Id ||
                                                    c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                   c.Type == SoftwareCoverType.Front)
                                      .Select(c => (Guid?)c.Id)
                                      .FirstOrDefault()
            });

        IQueryable<SoftwareDto> compilationsQuery = context.SoftwareReleases
           .Where(r => r.IsCompilation &&
                       (r.IncludedSoftware.Any(s => s.Software.Genres.Any(g => g.GenreId == genreId)) ||
                        r.IncludedVersions.Any(v => v.SoftwareVersion.Software.Genres.Any(g => g.GenreId == genreId))))
           .Select(r => new SoftwareDto
            {
                Id            = r.Id,
                Name          = r.Title,
                FamilyId      = null,
                Family        = null,
                Kind          = default,
                IsCompilation = true,
                FrontCoverId  = r.Covers
                                 .Where(cv => cv.Type == SoftwareCoverType.Front)
                                 .Select(cv => (Guid?)cv.Id)
                                 .FirstOrDefault()
            });

        List<SoftwareDto> combined = await softwareQuery.Concat(compilationsQuery).ToListAsync();

        return combined.OrderBy(s => s.Name, NaturalStringComparer.Instance).ToList();
    }

    [HttpGet("specifications")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareSpecKeyDto>> GetSpecificationsAsync()
    {
        if(cache.TryGetValue(SOFTWARE_SPECS_CACHE_KEY, out List<SoftwareSpecKeyDto> cached) && cached is not null)
            return cached;

        var raw = await context.SoftwareAttributes
                               .Where(a => a.Category == "Spec" && a.Key != "Notes")
                               .Select(a => new { a.Key, a.Value })
                               .Distinct()
                               .ToListAsync();

        List<SoftwareSpecKeyDto> result = raw.GroupBy(a => a.Key)
                                             .OrderBy(g => g.Key)
                                             .Select(g => new SoftwareSpecKeyDto
                                              {
                                                  Key    = g.Key,
                                                  Values = g.Select(a => a.Value).OrderBy(v => v).ToList()
                                              })
                                             .ToList();

        cache.Set(SOFTWARE_SPECS_CACHE_KEY, result, _catalogCacheTtl);

        return result;
    }

    [HttpGet("by-spec")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareBySpecAsync([FromQuery] string key, [FromQuery] string value)
    {
        if(key == "Notes") return [];

        // Single SQL round-trip via Concat (UNION ALL).
        IQueryable<SoftwareDto> softwareQuery = context.Softwares
               .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.Attributes
                                                                     .Any(a => a.Category == "Spec" &&
                                                                              a.Key   == key         &&
                                                                              a.Value == value)))
                         || s.DirectReleases.Any(r => r.Attributes
                                                       .Any(a => a.Category == "Spec" &&
                                                                  a.Key   == key       &&
                                                                  a.Value == value)))
               .Select(s => new SoftwareDto
                {
                    Id            = s.Id,
                    Name          = s.Name,
                    FamilyId      = s.FamilyId,
                    Family        = s.Family.Name,
                    Kind          = s.Kind,
                    IsCompilation = false,
                    FrontCoverId = context.SoftwareCovers
                                          .Where(c => (c.Release.SoftwareId == s.Id ||
                                                        c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                       c.Type == SoftwareCoverType.Front)
                                          .Select(c => (Guid?)c.Id)
                                          .FirstOrDefault()
                });

        IQueryable<SoftwareDto> compilationsQuery = context.SoftwareReleases
           .Where(r => r.IsCompilation &&
                        r.Attributes.Any(a => a.Category == "Spec" &&
                                              a.Key   == key       &&
                                              a.Value == value))
           .Select(r => new SoftwareDto
            {
                Id            = r.Id,
                Name          = r.Title,
                FamilyId      = null,
                Family        = null,
                Kind          = default,
                IsCompilation = true,
                FrontCoverId  = r.Covers
                                 .Where(cv => cv.Type == SoftwareCoverType.Front)
                                 .Select(cv => (Guid?)cv.Id)
                                 .FirstOrDefault()
            });

        List<SoftwareDto> combined = await softwareQuery.Concat(compilationsQuery).ToListAsync();

        return combined.OrderBy(s => s.Name, NaturalStringComparer.Instance).ToList();
    }

    [HttpGet("/software/{softwareId:ulong}/genres")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareGenreDto>> GetGenresAsync(ulong softwareId) => context.GenresBySoftware
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

    [HttpGet("/software/{softwareId:ulong}/attributes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareAttributeDto>> GetAttributesAsync(ulong softwareId) => context.SoftwareAttributes
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

    [HttpGet("/software/{softwareId:ulong}/credits")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<PersonBySoftwareDto>> GetCreditsAsync(ulong softwareId) =>
        (await context.PeopleBySoftware
                      .Where(p => p.SoftwareId == softwareId)
                      .Select(p => new PersonBySoftwareDto
                       {
                           Id           = p.Id,
                           PersonId     = p.PersonId,
                           SoftwareId   = p.SoftwareId,
                           Role         = p.DocumentRole != null ? p.DocumentRole.Name : p.Role,
                           SoftwareName = p.Software.Name,
                           Name         = p.Person.Name,
                           Surname      = p.Person.Surname,
                           Alias        = p.Person.Alias,
                           DisplayName  = p.Person.DisplayName
                       })
                      .ToListAsync()).OrderBy(p => p.Role)
           .ThenBy(p => p.FullName)
           .ToList();

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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoftwareUserRatingDto>> GetMyRatingAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        SoftwareUserRating rating =
            await context.SoftwareUserRatings.FirstOrDefaultAsync(r => r.UserId == userId && r.SoftwareId == id);

        if(rating is null) return NotFound();

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
            return BadRequest("Rating must be between 0 and 5.");

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

        await context.SaveChangesAsync();

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
            await context.SaveChangesAsync();
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
                UpdatedOn       = r.UpdatedOn
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

        if(exists) return Conflict("You have already reviewed this software.");

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

        await context.SaveChangesAsync();

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

        await context.SaveChangesAsync();

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
        await context.SaveChangesAsync();

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

        await context.SaveChangesAsync();

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
            await context.SaveChangesAsync();
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

        if(alreadyReported) return Conflict("You have already reported this review.");

        context.ReviewReports.Add(new ReviewReport
        {
            ReporterId  = userId,
            ReviewId    = reviewId,
            Reason      = dto.Reason,
            Explanation = dto.Explanation
        });

        await context.SaveChangesAsync();

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
        // Single round-trip: pull both critic and user averages in one query
        // off the Software entity (was two sequential Average() awaits).
        var avg = await context.Softwares
                               .Where(s => s.Id == id)
                               .Select(s => new
                                {
                                    CriticAvg = (double?)s.CriticReviews
                                                          .Where(r => r.NormalizedScore != null)
                                                          .Average(r => (double?)r.NormalizedScore),
                                    UserAvg = (double?)s.UserRatings.Average(r => (double?)r.Rating)
                                })
                               .FirstOrDefaultAsync();

        double? criticAvg = avg?.CriticAvg;
        double? userAvg   = avg?.UserAvg;

        // Critic score: average of NormalizedScore (0-100), scaled to 0-10
        double? criticScore = criticAvg.HasValue ? Math.Round(criticAvg.Value / 10.0, 1) : null;

        // User score: average of Rating (0-5), scaled to 0-10
        double? userScore = userAvg.HasValue ? Math.Round(userAvg.Value * 2.0, 1) : null;

        // Marechai score: average of available components
        double? marechaiScore = null;

        if(criticScore.HasValue && userScore.HasValue)
            marechaiScore = Math.Round((criticScore.Value + userScore.Value) / 2.0, 1);
        else if(criticScore.HasValue)
            marechaiScore = criticScore.Value;
        else if(userScore.HasValue)
            marechaiScore = userScore.Value;

        // Rank — looked up from a 5-minute cached dictionary so we don't
        // re-rank the entire catalog on every page load.
        int?  rank        = null;
        int   totalRanked = 0;

        if(marechaiScore.HasValue)
        {
            Dictionary<ulong, int> ranking = await GetMarechaiRankingAsync();

            totalRanked = ranking.Count;

            if(ranking.TryGetValue(id, out int found)) rank = found;
        }

        return new MarechaiScoreDto
        {
            CriticScore   = criticScore,
            UserScore     = userScore,
            MarechaiScore = marechaiScore,
            Rank          = rank,
            TotalRanked   = totalRanked
        };
    }

    /// <summary>
    /// Returns a software-id → rank dictionary spanning every software with at least
    /// one critic review or user rating. Cached for <see cref="_marechaiRankingTtl"/>
    /// so we don't re-rank the entire catalog on every page load.
    /// </summary>
    async Task<Dictionary<ulong, int>> GetMarechaiRankingAsync()
    {
        if(cache.TryGetValue(MARECHAI_RANKING_CACHE_KEY, out Dictionary<ulong, int> cached) && cached is not null)
            return cached;

        var allScores = await context.Softwares
                                     .Select(s => new
                                      {
                                          s.Id,
                                          CriticAvg = s.CriticReviews
                                                       .Where(r => r.NormalizedScore != null)
                                                       .Select(r => (double?)r.NormalizedScore)
                                                       .Average(),
                                          UserAvg = s.UserRatings
                                                     .Select(r => (double?)r.Rating)
                                                     .Average()
                                      })
                                     .Where(s => s.CriticAvg != null || s.UserAvg != null)
                                     .ToListAsync();

        var ranking = allScores.Select(s =>
                              {
                                  double? c = s.CriticAvg.HasValue ? s.CriticAvg.Value / 10.0 : null;
                                  double? u = s.UserAvg.HasValue ? s.UserAvg.Value * 2.0 : null;

                                  double score;

                                  if(c.HasValue && u.HasValue)
                                      score = (c.Value + u.Value) / 2.0;
                                  else if(c.HasValue)
                                      score = c.Value;
                                  else
                                      score = u!.Value;

                                  return new { s.Id, Score = Math.Round(score, 1) };
                              })
                              .OrderByDescending(s => s.Score)
                              .ThenBy(s => s.Id)
                              .Select((s, i) => new { s.Id, Rank = i + 1 })
                              .ToDictionary(x => x.Id, x => x.Rank);

        cache.Set(MARECHAI_RANKING_CACHE_KEY, ranking, _marechaiRankingTtl);

        return ranking;
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
            return $"photos/avatars/thumbs/jpeg/hd/{user.AvatarGuid}.jpg";

        return null;
    }
}
