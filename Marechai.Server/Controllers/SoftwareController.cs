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
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/software")]
[ApiController]
public class SoftwareController(MarechaiContext context) : ControllerBase
{
    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetSoftwareCountAsync([FromQuery] string search = null)
    {
        IQueryable<Database.Models.Software> query = context.Softwares;

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Name.Contains(search));

        return query.CountAsync();
    }

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetMinimumYearAsync() => context.SoftwareReleases
                                                     .Where(r => r.ReleaseDate.HasValue &&
                                                                 r.ReleaseDate.Value.Year > 1000)
                                                     .MinAsync(r => r.ReleaseDate.Value.Year);

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetMaximumYearAsync() => context.SoftwareReleases
                                                     .Where(r => r.ReleaseDate.HasValue &&
                                                                 r.ReleaseDate.Value.Year > 1000)
                                                     .MaxAsync(r => r.ReleaseDate.Value.Year);

    [HttpGet("by-letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareByLetterAsync(char c) => context.Softwares
       .Where(s => EF.Functions.Like(s.Name, $"{c}%"))
       .OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            IsOperatingSystem = s.IsOperatingSystem,
            IsGame            = s.IsGame,
            FrontCoverId = context.SoftwareCovers
                                  .Where(c2 => (c2.Release.SoftwareId == s.Id ||
                                                 c2.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                c2.Type == SoftwareCoverType.Front)
                                  .Select(c2 => (Guid?)c2.Id)
                                  .FirstOrDefault()
        })
       .ToListAsync();

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareByYearAsync(int year) => context.Softwares
       .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.ReleaseDate != null &&
                                                           r.ReleaseDate.Value.Year == year))
                || s.DirectReleases.Any(r => r.ReleaseDate != null &&
                                             r.ReleaseDate.Value.Year == year))
       .OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            IsOperatingSystem = s.IsOperatingSystem,
            IsGame            = s.IsGame,
            FrontCoverId = context.SoftwareCovers
                                  .Where(c => (c.Release.SoftwareId == s.Id ||
                                                c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                               c.Type == SoftwareCoverType.Front)
                                  .Select(c => (Guid?)c.Id)
                                  .FirstOrDefault()
        })
       .ToListAsync();

    [HttpGet("by-platform/{platformId:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareByPlatformAsync(ulong platformId) => context.Softwares
       .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.PlatformId == platformId))
                || s.DirectReleases.Any(r => r.PlatformId == platformId))
       .OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            IsOperatingSystem = s.IsOperatingSystem,
            IsGame            = s.IsGame,
            FrontCoverId = context.SoftwareCovers
                                  .Where(c => (c.Release.SoftwareId == s.Id ||
                                                c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                               c.Type == SoftwareCoverType.Front)
                                  .Select(c => (Guid?)c.Id)
                                  .FirstOrDefault()
        })
       .ToListAsync();

    [HttpGet("companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetSoftwareCompaniesAsync() =>
        context.SoftwareCompanyRoles
               .Select(cr => cr.Company)
               .Union(context.SoftwareReleases.Select(sr => sr.Publisher))
               .Distinct()
               .Include(c => c.Logos)
               .OrderBy(c => MarechaiContext.NaturalSortKey(c.Name))
               .Select(c => new CompanyDto
                {
                    Id       = c.Id,
                    LastLogo = c.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
                    Name     = c.Name
                })
               .ToListAsync();

    [HttpGet("companies/letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetSoftwareCompaniesByLetterAsync(char c) =>
        context.SoftwareCompanyRoles
               .Select(cr => cr.Company)
               .Union(context.SoftwareReleases.Select(sr => sr.Publisher))
               .Distinct()
               .Include(c => c.Logos)
               .Where(co => EF.Functions.Like(co.Name, $"{c}%"))
               .OrderBy(co => MarechaiContext.NaturalSortKey(co.Name))
               .Select(co => new CompanyDto
                {
                    Id       = co.Id,
                    LastLogo = co.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
                    Name     = co.Name
                })
               .ToListAsync();

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
    public Task<List<SoftwareDto>> GetAsync([FromQuery] int? skip   = null, [FromQuery] int? take = null,
                                            [FromQuery] string search = null,
                                            [FromQuery] string sortBy = null,
                                            [FromQuery] bool sortDescending = false)
    {
        IQueryable<Database.Models.Software> query = context.Softwares;

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Name.Contains(search));

        query = sortBy switch
        {
            "Name"              => sortDescending ? query.OrderByDescending(s => MarechaiContext.NaturalSortKey(s.Name))        : query.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name)),
            "Family"            => sortDescending ? query.OrderByDescending(s => MarechaiContext.NaturalSortKey(s.Family.Name)) : query.OrderBy(s => MarechaiContext.NaturalSortKey(s.Family.Name)),
            "IsOperatingSystem" => sortDescending ? query.OrderByDescending(s => s.IsOperatingSystem) : query.OrderBy(s => s.IsOperatingSystem),
            "IsGame"            => sortDescending ? query.OrderByDescending(s => s.IsGame)            : query.OrderBy(s => s.IsGame),
            _                   => query.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
        };

        if(skip.HasValue) query = query.Skip(skip.Value);

        if(take.HasValue) query = query.Take(take.Value);

        return query.Select(s => new SoftwareDto
                     {
                         Id                = s.Id,
                         Name              = s.Name,
                         FamilyId          = s.FamilyId,
                         Family            = s.Family.Name,
                         IsOperatingSystem = s.IsOperatingSystem,
                         IsGame            = s.IsGame,
                         FrontCoverId = context.SoftwareCovers
                                               .Where(c => (c.Release.SoftwareId == s.Id ||
                                                             c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                            c.Type == SoftwareCoverType.Front)
                                               .Select(c => (Guid?)c.Id)
                                               .FirstOrDefault()
                     })
                    .ToListAsync();
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
                                                               IsOperatingSystem = s.IsOperatingSystem,
                                                               IsGame            = s.IsGame
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
        model.IsOperatingSystem = dto.IsOperatingSystem;
        model.IsGame            = dto.IsGame;

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
            IsOperatingSystem = dto.IsOperatingSystem,
            IsGame            = dto.IsGame
        };

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
            CompilationReferencesDuplicates = compilationDuplicates
        };
    }

    [HttpPost("{targetId:ulong}/merge/{sourceId:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> MergeAsync(ulong targetId, ulong sourceId, [FromQuery] string releaseTitle = null)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(targetId == sourceId) return BadRequest("Cannot merge a software entry into itself.");

        Software target = await context.Softwares.FindAsync(targetId);

        if(target is null) return NotFound("Target software not found.");

        Software source = await context.Softwares.FindAsync(sourceId);

        if(source is null) return NotFound("Source software not found.");

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

            // 9b. Transfer predecessor: if target has no predecessor but source does, adopt it
            if(target.PredecessorId is null && source.PredecessorId is not null)
                target.PredecessorId = source.PredecessorId;

            // 9c. Re-point any software that had source as predecessor to target
            List<Software> successorsOfSource =
                await context.Softwares.Where(s => s.PredecessorId == sourceId && s.Id != targetId).ToListAsync();

            foreach(Software successor in successorsOfSource)
                successor.PredecessorId = targetId;

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
    public Task<List<SoftwareGenreDto>> GetAllGenresAsync() => context.SoftwareGenres
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

    [HttpGet("by-genre/{genreId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareByGenreAsync(int genreId) => context.Softwares
       .Where(s => s.Genres.Any(g => g.GenreId == genreId))
       .OrderBy(s => s.Name)
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            IsOperatingSystem = s.IsOperatingSystem,
            IsGame            = s.IsGame,
            FrontCoverId = context.SoftwareCovers
                                  .Where(c => (c.Release.SoftwareId == s.Id ||
                                                c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                               c.Type == SoftwareCoverType.Front)
                                  .Select(c => (Guid?)c.Id)
                                  .FirstOrDefault()
        })
       .ToListAsync();

    [HttpGet("specifications")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareSpecKeyDto>> GetSpecificationsAsync()
    {
        var raw = await context.SoftwareAttributes
                               .Where(a => a.Category == "Spec" && a.Key != "Notes")
                               .Select(a => new { a.Key, a.Value })
                               .Distinct()
                               .ToListAsync();

        return raw.GroupBy(a => a.Key)
                  .OrderBy(g => g.Key)
                  .Select(g => new SoftwareSpecKeyDto
                   {
                       Key    = g.Key,
                       Values = g.Select(a => a.Value).OrderBy(v => v).ToList()
                   })
                  .ToList();
    }

    [HttpGet("by-spec")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareBySpecAsync([FromQuery] string key, [FromQuery] string value)
    {
        if(key == "Notes") return [];

        return await context.Softwares
               .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.Attributes
                                                                     .Any(a => a.Category == "Spec" &&
                                                                              a.Key   == key         &&
                                                                              a.Value == value)))
                         || s.DirectReleases.Any(r => r.Attributes
                                                       .Any(a => a.Category == "Spec" &&
                                                                  a.Key   == key       &&
                                                                  a.Value == value)))
               .OrderBy(s => s.Name)
               .Select(s => new SoftwareDto
                {
                    Id                = s.Id,
                    Name              = s.Name,
                    FamilyId          = s.FamilyId,
                    Family            = s.Family.Name,
                    IsOperatingSystem = s.IsOperatingSystem,
                    IsGame            = s.IsGame,
                    FrontCoverId = context.SoftwareCovers
                                          .Where(c => (c.Release.SoftwareId == s.Id ||
                                                        c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                       c.Type == SoftwareCoverType.Front)
                                          .Select(c => (Guid?)c.Id)
                                          .FirstOrDefault()
                })
               .ToListAsync();
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
}
