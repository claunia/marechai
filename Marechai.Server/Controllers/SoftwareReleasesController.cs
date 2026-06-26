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
using Marechai.Server.Helpers;
using Marechai.Server.Services;
using Marechai.Server.Suggestions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/software/releases")]
[ApiController]
public class SoftwareReleasesController(MarechaiContext                   context,
                                        SoftwareAttributeTranslationCache attrCache) : ControllerBase
{
    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetReleasesCountAsync([FromQuery] string search = null)
    {
        IQueryable<SoftwareRelease> query = context.SoftwareReleases;

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => (r.Title != null && r.Title.Contains(search)) ||
                                     (r.Software != null && r.Software.Name.Contains(search)) ||
                                     (r.Platform != null && r.Platform.Name.Contains(search)) ||
                                     (r.Publisher != null && r.Publisher.Name.Contains(search)));

        return query.CountAsync();
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareReleaseDto>> GetAsync([FromQuery] int? skip   = null, [FromQuery] int? take = null,
                                                   [FromQuery] string search = null,
                                                   [FromQuery] string sortBy = null,
                                                   [FromQuery] bool sortDescending = false)
    {
        IQueryable<SoftwareRelease> query = context.SoftwareReleases;

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => (r.Title != null && r.Title.Contains(search)) ||
                                     (r.Software != null && r.Software.Name.Contains(search)) ||
                                     (r.Platform != null && r.Platform.Name.Contains(search)) ||
                                     (r.Publisher != null && r.Publisher.Name.Contains(search)));

        query = sortBy switch
        {
            "Title"       => sortDescending ? query.OrderByDescending(r => MarechaiContext.NaturalSortKey(r.Title))          : query.OrderBy(r => MarechaiContext.NaturalSortKey(r.Title)),
            "Software"    => sortDescending ? query.OrderByDescending(r => MarechaiContext.NaturalSortKey(r.Software.Name))  : query.OrderBy(r => MarechaiContext.NaturalSortKey(r.Software.Name)),
            "Platform"    => sortDescending ? query.OrderByDescending(r => MarechaiContext.NaturalSortKey(r.Platform.Name))  : query.OrderBy(r => MarechaiContext.NaturalSortKey(r.Platform.Name)),
            "Publisher"   => sortDescending ? query.OrderByDescending(r => MarechaiContext.NaturalSortKey(r.Publisher.Name)) : query.OrderBy(r => MarechaiContext.NaturalSortKey(r.Publisher.Name)),
            "ReleaseDate" => sortDescending ? query.OrderByDescending(r => r.ReleaseDate) : query.OrderBy(r => r.ReleaseDate),
            _ => query.OrderBy(r => r.Software != null ? r.Software.Name : r.Title)
                      .ThenBy(r => r.SoftwareVersion != null ? r.SoftwareVersion.VersionString : null)
        };

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.Select(r => new SoftwareReleaseDto
                     {
                         Id                = r.Id,
                         Title             = r.Title,
                         SoftwareId        = r.SoftwareId,
                         Software          = r.Software.Name,
                         SoftwareCompilationId = r.SoftwareCompilationId,
                         SoftwareCompilation   = r.SoftwareCompilation.Name,
                         SoftwareVersionId = r.SoftwareVersionId,
                         SoftwareVersion   = r.SoftwareVersion.VersionString,
                         PlatformId        = r.PlatformId,
                         Platform          = r.Platform.Name,
                         Regions           = r.Regions.Select(rg => new UnM49BySoftwareReleaseDto
                         {
                             SoftwareReleaseId = rg.SoftwareReleaseId,
                             UnM49Id           = rg.UnM49Id,
                             RegionName        = rg.UnM49.Name
                         }).ToList(),
                         Languages         = r.Languages.Select(lg => new LanguageBySoftwareReleaseDto
                         {
                             SoftwareReleaseId = lg.SoftwareReleaseId,
                             LanguageCode      = lg.LanguageCode,
                             Language          = lg.Language.ReferenceName
                         }).ToList(),
                         PublisherId       = r.PublisherId,
                         Publisher         = r.Publisher.Name,
                         ReleaseDate       = r.ReleaseDate,
                         ReleaseDatePrecision = r.ReleaseDatePrecision
                     })
                    .ToListAsync();
    }

    [HttpGet("/software/versions/{versionId:ulong}/releases/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetReleasesCountByVersionAsync(ulong versionId, [FromQuery] string search = null)
    {
        IQueryable<SoftwareRelease> query = context.SoftwareReleases.Where(r => r.SoftwareVersionId == versionId);

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => (r.Title != null && r.Title.Contains(search)) ||
                                     (r.Platform != null && r.Platform.Name.Contains(search)) ||
                                     (r.Publisher != null && r.Publisher.Name.Contains(search)));

        return query.CountAsync();
    }

    [HttpGet("/software/versions/{versionId:ulong}/releases")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareReleaseDto>> GetByVersionAsync(ulong versionId, [FromQuery] int? skip   = null,
                                                            [FromQuery] int?    take   = null,
                                                            [FromQuery] string  search = null)
    {
        IQueryable<SoftwareRelease> query = context.SoftwareReleases.Where(r => r.SoftwareVersionId == versionId);

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => (r.Title != null && r.Title.Contains(search)) ||
                                     (r.Platform != null && r.Platform.Name.Contains(search)) ||
                                     (r.Publisher != null && r.Publisher.Name.Contains(search)));

        query = query.OrderBy(r => r.ReleaseDate);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.Select(r => new SoftwareReleaseDto
                     {
                         Id                = r.Id,
                         Title             = r.Title,
                         SoftwareId        = r.SoftwareId,
                         Software          = r.Software.Name,
                         SoftwareCompilationId = r.SoftwareCompilationId,
                         SoftwareCompilation   = r.SoftwareCompilation.Name,
                         SoftwareVersionId = r.SoftwareVersionId,
                         SoftwareVersion   = r.SoftwareVersion.VersionString,
                         PlatformId        = r.PlatformId,
                         Platform          = r.Platform.Name,
                         Regions           = r.Regions.Select(rg => new UnM49BySoftwareReleaseDto
                         {
                             SoftwareReleaseId = rg.SoftwareReleaseId,
                             UnM49Id           = rg.UnM49Id,
                             RegionName        = rg.UnM49.Name
                         }).ToList(),
                         Languages         = r.Languages.Select(lg => new LanguageBySoftwareReleaseDto
                         {
                             SoftwareReleaseId = lg.SoftwareReleaseId,
                             LanguageCode      = lg.LanguageCode,
                             Language          = lg.Language.ReferenceName
                         }).ToList(),
                         PublisherId       = r.PublisherId,
                         Publisher         = r.Publisher.Name,
                         ReleaseDate       = r.ReleaseDate,
                         ReleaseDatePrecision = r.ReleaseDatePrecision
                     })
                    .ToListAsync();
    }

    [HttpGet("/software/{softwareId:ulong}/releases/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetReleasesCountBySoftwareAsync(ulong softwareId, [FromQuery] string search = null)
    {
        IQueryable<SoftwareRelease> query = context.SoftwareReleases
                                                   .Where(r => r.SoftwareId == softwareId);

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => (r.Title != null && r.Title.Contains(search)) ||
                                     (r.Platform != null && r.Platform.Name.Contains(search)) ||
                                     (r.Publisher != null && r.Publisher.Name.Contains(search)));

        return query.CountAsync();
    }

    [HttpGet("/software/{softwareId:ulong}/releases")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareReleaseDto>> GetBySoftwareAsync(ulong softwareId, [FromQuery] int? skip   = null,
                                                             [FromQuery] int?    take   = null,
                                                             [FromQuery] string  search = null)
    {
        IQueryable<SoftwareRelease> query = context.SoftwareReleases
                                                   .Where(r => r.SoftwareId == softwareId);

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => (r.Title != null && r.Title.Contains(search)) ||
                                     (r.Platform != null && r.Platform.Name.Contains(search)) ||
                                     (r.Publisher != null && r.Publisher.Name.Contains(search)));

        query = query.OrderBy(r => r.SoftwareVersion.VersionString)
                     .ThenBy(r => r.ReleaseDate);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.Select(r => new SoftwareReleaseDto
                     {
                         Id                = r.Id,
                         Title             = r.Title,
                         SoftwareId        = r.SoftwareId,
                         Software          = r.Software.Name,
                         SoftwareCompilationId = r.SoftwareCompilationId,
                         SoftwareCompilation   = r.SoftwareCompilation.Name,
                         SoftwareVersionId = r.SoftwareVersionId,
                         SoftwareVersion   = r.SoftwareVersion.VersionString,
                         PlatformId        = r.PlatformId,
                         Platform          = r.Platform.Name,
                         Regions           = r.Regions.Select(rg => new UnM49BySoftwareReleaseDto
                         {
                             SoftwareReleaseId = rg.SoftwareReleaseId,
                             UnM49Id           = rg.UnM49Id,
                             RegionName        = rg.UnM49.Name
                         }).ToList(),
                         Languages         = r.Languages.Select(lg => new LanguageBySoftwareReleaseDto
                         {
                             SoftwareReleaseId = lg.SoftwareReleaseId,
                             LanguageCode      = lg.LanguageCode,
                             Language          = lg.Language.ReferenceName
                         }).ToList(),
                         PublisherId       = r.PublisherId,
                         Publisher         = r.Publisher.Name,
                         ReleaseDate       = r.ReleaseDate,
                         ReleaseDatePrecision = r.ReleaseDatePrecision
                     })
                    .ToListAsync();
    }

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwareReleaseDto> GetAsync(ulong id) => context.SoftwareReleases.Where(r => r.Id == id)
                                                                 .Select(r => new SoftwareReleaseDto
                                                                  {
                                                                      Id                = r.Id,
                                                                      Title             = r.Title,
                                                                      SoftwareId        = r.SoftwareId,
                                                                      Software          = r.Software.Name,
                                                                      SoftwareCompilationId = r.SoftwareCompilationId,
                                                                      SoftwareCompilation   = r.SoftwareCompilation.Name,
                                                                      SoftwareVersionId = r.SoftwareVersionId,
                                                                      SoftwareVersion   = r.SoftwareVersion.VersionString,
                                                                      PlatformId        = r.PlatformId,
                                                                      Platform          = r.Platform.Name,
                                                                      Regions           = r.Regions.Select(rg => new UnM49BySoftwareReleaseDto
                                                                      {
                                                                          SoftwareReleaseId = rg.SoftwareReleaseId,
                                                                          UnM49Id           = rg.UnM49Id,
                                                                          RegionName        = rg.UnM49.Name
                                                                      }).ToList(),
                                                                      Languages         = r.Languages.Select(lg => new LanguageBySoftwareReleaseDto
                                                                      {
                                                                          SoftwareReleaseId = lg.SoftwareReleaseId,
                                                                          LanguageCode      = lg.LanguageCode,
                                                                          Language          = lg.Language.ReferenceName
                                                                      }).ToList(),
                                                                      PublisherId       = r.PublisherId,
                                                                      Publisher         = r.Publisher.Name,
                                                                      ReleaseDate       = r.ReleaseDate,
                                                                      ReleaseDatePrecision = r.ReleaseDatePrecision
                                                                  })
                                                                 .FirstOrDefaultAsync();

    [HttpGet("{releaseId:ulong}/attributes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareAttributeDto>> GetAttributesAsync(ulong releaseId,
                                                                     [FromQuery] string lang = null)
    {
        List<SoftwareAttributeDto> attributes = await context.SoftwareAttributes
           .Where(a => a.SoftwareReleaseId == releaseId)
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
           .OrderBy(a => a.Category)
           .ThenBy(a => a.Key)
           .ToListAsync();

        // Importer stores attribute keys/values with U+00A0 (non-breaking space) — normalise here
        // before lookup. Rating-category attributes are returned verbatim (only normalised); every
        // other category routes through the SoftwareAttributeTranslationCache which falls back to
        // the original normalised text when no translation row exists for the requested language.
        string resolvedLang = LanguageResolver.Resolve(HttpContext, lang);

        const string ratingCategory = SoftwareReleaseSuggestionApplier.AttributeCategoryRating;

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

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwareReleaseDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwareRelease model = await context.SoftwareReleases.FindAsync(id);

        if(model is null) return NotFound();

        // Cannot move a release between Software, SoftwareCompilation, or a different
        // SoftwareCompilation after creation
        if(model.SoftwareCompilationId != dto.SoftwareCompilationId)
            return Problem(detail: "Cannot change the compilation associated with a release.", statusCode: StatusCodes.Status400BadRequest);

        // Cannot change SoftwareId after creation
        if(model.SoftwareId != dto.SoftwareId)
            return Problem(detail: "Cannot change the software associated with a release.", statusCode: StatusCodes.Status400BadRequest);

        model.Title             = dto.Title;
        model.SoftwareVersionId = dto.SoftwareVersionId;
        model.PlatformId        = dto.PlatformId;
        model.PublisherId       = dto.PublisherId;
        model.ReleaseDate       = dto.ReleaseDate;
        model.ReleaseDatePrecision = dto.ReleaseDatePrecision;

        string newsName = await BuildSoftwareReleaseNewsNameAsync(model);

        await context.News.AddAsync(new News
        {
            AddedId = (long)model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedSoftwareReleaseInDb,
            Name    = newsName
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwareReleaseDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwareRelease
        {
            Title             = dto.Title,
            SoftwareCompilationId = dto.SoftwareCompilationId,
            SoftwareId        = dto.SoftwareCompilationId.HasValue ? null : dto.SoftwareId,
            SoftwareVersionId = dto.SoftwareCompilationId.HasValue ? null : dto.SoftwareVersionId,
            PlatformId        = dto.PlatformId,
            PublisherId       = dto.PublisherId,
            ReleaseDate       = dto.ReleaseDate,
            ReleaseDatePrecision = dto.ReleaseDatePrecision
        };

        await context.SoftwareReleases.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        string newsName = await BuildSoftwareReleaseNewsNameAsync(model);

        await context.News.AddAsync(new News
        {
            AddedId = (long)model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.NewSoftwareReleaseInDb,
            Name    = newsName
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
        SoftwareRelease item = await context.SoftwareReleases.FindAsync(id);

        if(item is null) return NotFound();

        // Capture entity-display label BEFORE Remove + SaveChanges so the stale-cascade
        // notification can render a useful name. Mirrors MagazineIssuesController convention.
        string entityName = item.Title;

        context.SoftwareReleases.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.SoftwareRelease, (long)id, entityName);

        return Ok();
    }

    // --- Compilation listing endpoints ---

    [HttpGet("compilations")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareReleaseDto>> GetCompilationsAsync() => context.SoftwareReleases
       .Where(r => r.SoftwareCompilationId != null)
       .OrderBy(r => r.Title)
       .Select(r => new SoftwareReleaseDto
        {
            Id                = r.Id,
            Title             = r.Title,
            SoftwareId        = r.SoftwareId,
            Software          = r.Software.Name,
            SoftwareCompilationId = r.SoftwareCompilationId,
            SoftwareCompilation   = r.SoftwareCompilation.Name,
            SoftwareVersionId = r.SoftwareVersionId,
            PlatformId        = r.PlatformId,
            Platform          = r.Platform.Name,
            Regions           = r.Regions.Select(rg => new UnM49BySoftwareReleaseDto
            {
                SoftwareReleaseId = rg.SoftwareReleaseId,
                UnM49Id           = rg.UnM49Id,
                RegionName        = rg.UnM49.Name
            }).ToList(),
            Languages         = r.Languages.Select(lg => new LanguageBySoftwareReleaseDto
            {
                SoftwareReleaseId = lg.SoftwareReleaseId,
                LanguageCode      = lg.LanguageCode,
                Language          = lg.Language.ReferenceName
            }).ToList(),
            PublisherId       = r.PublisherId,
            Publisher         = r.Publisher.Name,
            ReleaseDate       = r.ReleaseDate,
            ReleaseDatePrecision = r.ReleaseDatePrecision
        })
       .ToListAsync();

    [HttpGet("/software/{softwareId:ulong}/compilations")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareReleaseDto>> GetCompilationsForSoftwareAsync(ulong softwareId) =>
        // Single round-trip: compose the releaseId set as a subquery directly inside
        // the projection's Where clause instead of materializing IDs first and then
        // running a separate Contains(...) query.
        context.SoftwareReleases
               .Where(r => r.SoftwareCompilationId != null &&
                           (context.SoftwareVersionBySoftwareCompilation
                                   .Any(x => x.SoftwareCompilationId == r.SoftwareCompilationId &&
                                             x.SoftwareVersion.SoftwareId == softwareId) ||
                            context.SoftwareBySoftwareCompilation
                                   .Any(x => x.SoftwareCompilationId == r.SoftwareCompilationId &&
                                             x.SoftwareId == softwareId)))
               .OrderBy(r => r.Title)
               .Select(r => new SoftwareReleaseDto
                {
                    Id                = r.Id,
                    Title             = r.Title,
                    SoftwareId        = r.SoftwareId,
                    Software          = r.Software.Name,
                    SoftwareCompilationId = r.SoftwareCompilationId,
                    SoftwareCompilation   = r.SoftwareCompilation.Name,
                    SoftwareVersionId = r.SoftwareVersionId,
                    PlatformId        = r.PlatformId,
                    Platform          = r.Platform.Name,
                    Regions = r.Regions.Select(rg => new UnM49BySoftwareReleaseDto
                                        {
                                            SoftwareReleaseId = rg.SoftwareReleaseId,
                                            UnM49Id           = rg.UnM49Id,
                                            RegionName        = rg.UnM49.Name
                                        })
                                       .ToList(),
                    Languages = r.Languages.Select(lg => new LanguageBySoftwareReleaseDto
                                            {
                                                SoftwareReleaseId = lg.SoftwareReleaseId,
                                                LanguageCode      = lg.LanguageCode,
                                                Language          = lg.Language.ReferenceName
                                            })
                                           .ToList(),
                    PublisherId          = r.PublisherId,
                    Publisher            = r.Publisher.Name,
                    ReleaseDate          = r.ReleaseDate,
                    ReleaseDatePrecision = r.ReleaseDatePrecision
                })
               .ToListAsync();

    async Task<string> BuildSoftwareReleaseNewsNameAsync(SoftwareRelease model)
    {
        string baseName;

        // Single-version release: build from version info
        if(model.SoftwareVersionId is not null)
        {
            SoftwareVersion version = await context.SoftwareVersions.Include(v => v.Software)
                                                   .FirstOrDefaultAsync(v => v.Id == model.SoftwareVersionId);

            if(version?.Software is not null)
                baseName = $"{version.Software.Name} {version.VersionString}";
            else if(version is not null)
                baseName = version.VersionString;
            else
                baseName = "";
        }
        // Versionless single release: build from software name
        else if(model.SoftwareCompilationId is null && model.SoftwareId is not null)
        {
            Software software = await context.Softwares.FindAsync(model.SoftwareId);
            baseName = software?.Name ?? "";
        }
        // Compilation release: use the umbrella compilation's name
        else
        {
            SoftwareCompilation compilation = await context.SoftwareCompilations
                                                            .FindAsync(model.SoftwareCompilationId);
            baseName = compilation?.Name ?? "Compilation";
        }

        // The game's name always takes precedence; the release's own Title, if set, is a qualifier.
        string name = string.IsNullOrWhiteSpace(model.Title)
                           ? baseName
                           : string.IsNullOrWhiteSpace(baseName)
                               ? model.Title
                               : $"{baseName} ({model.Title})";

        if(model.PlatformId is not null)
        {
            SoftwarePlatform platform = await context.SoftwarePlatforms.FindAsync(model.PlatformId);

            if(platform is not null)
                name = string.IsNullOrEmpty(name) ? platform.Name : $"{name} ({platform.Name})";
        }

        return name;
    }

    // --- Region junction endpoints ---

    [HttpGet("{releaseId:ulong}/regions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<UnM49BySoftwareReleaseDto>> GetRegionsAsync(ulong releaseId) =>
        context.UnM49BySoftwareRelease
               .Where(x => x.SoftwareReleaseId == releaseId)
               .OrderBy(x => x.UnM49.Type)
               .ThenBy(x => x.UnM49.Name)
               .Select(x => new UnM49BySoftwareReleaseDto
                {
                    SoftwareReleaseId = x.SoftwareReleaseId,
                    UnM49Id           = x.UnM49Id,
                    RegionName        = x.UnM49.Name
                })
               .ToListAsync();

    [HttpPost("{releaseId:ulong}/regions")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddRegionAsync(ulong releaseId,
                                                   [FromBody] UnM49BySoftwareReleaseDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareRelease release = await context.SoftwareReleases.FindAsync(releaseId);

        if(release is null) return NotFound();

        bool exists = await context.UnM49BySoftwareRelease
                                   .AnyAsync(x => x.SoftwareReleaseId == releaseId
                                               && x.UnM49Id           == dto.UnM49Id);

        if(exists) return Problem(detail: "This region is already assigned to the release.", statusCode: StatusCodes.Status400BadRequest);

        bool regionExists = await context.UnM49.AnyAsync(r => r.Id == dto.UnM49Id);

        if(!regionExists) return Problem(detail: "The specified region does not exist.", statusCode: StatusCodes.Status400BadRequest);

        await context.UnM49BySoftwareRelease.AddAsync(new UnM49BySoftwareRelease
        {
            SoftwareReleaseId = releaseId,
            UnM49Id           = dto.UnM49Id
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{releaseId:ulong}/regions/{regionId:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveRegionAsync(ulong releaseId, short regionId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        UnM49BySoftwareRelease entry =
            await context.UnM49BySoftwareRelease
                         .FirstOrDefaultAsync(x => x.SoftwareReleaseId == releaseId
                                                && x.UnM49Id           == regionId);

        if(entry is null) return NotFound();

        context.UnM49BySoftwareRelease.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    // --- Language junction endpoints ---

    [HttpGet("{releaseId:ulong}/languages")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<LanguageBySoftwareReleaseDto>> GetLanguagesAsync(ulong releaseId) =>
        context.LanguageBySoftwareRelease
               .Where(x => x.SoftwareReleaseId == releaseId)
               .OrderBy(x => x.Language.ReferenceName)
               .Select(x => new LanguageBySoftwareReleaseDto
                {
                    SoftwareReleaseId = x.SoftwareReleaseId,
                    LanguageCode      = x.LanguageCode,
                    Language          = x.Language.ReferenceName
                })
               .ToListAsync();

    [HttpPost("{releaseId:ulong}/languages")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddLanguageAsync(ulong releaseId,
                                                     [FromBody] LanguageBySoftwareReleaseDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareRelease release = await context.SoftwareReleases.FindAsync(releaseId);

        if(release is null) return NotFound();

        bool exists = await context.LanguageBySoftwareRelease
                                   .AnyAsync(x => x.SoftwareReleaseId == releaseId
                                               && x.LanguageCode      == dto.LanguageCode);

        if(exists) return Problem(detail: "This language is already assigned to the release.", statusCode: StatusCodes.Status400BadRequest);

        bool languageExists = await context.Iso639.AnyAsync(l => l.Id == dto.LanguageCode);

        if(!languageExists) return Problem(detail: "The specified language does not exist.", statusCode: StatusCodes.Status400BadRequest);

        await context.LanguageBySoftwareRelease.AddAsync(new LanguageBySoftwareRelease
        {
            SoftwareReleaseId = releaseId,
            LanguageCode      = dto.LanguageCode
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{releaseId:ulong}/languages/{languageCode}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveLanguageAsync(ulong releaseId, string languageCode)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        LanguageBySoftwareRelease entry =
            await context.LanguageBySoftwareRelease
                         .FirstOrDefaultAsync(x => x.SoftwareReleaseId == releaseId
                                                && x.LanguageCode      == languageCode);

        if(entry is null) return NotFound();

        context.LanguageBySoftwareRelease.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
