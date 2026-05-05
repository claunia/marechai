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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/software/releases")]
[ApiController]
public class SoftwareReleasesController(MarechaiContext context) : ControllerBase
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
                                                   [FromQuery] string search = null)
    {
        IQueryable<SoftwareRelease> query = context.SoftwareReleases;

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => (r.Title != null && r.Title.Contains(search)) ||
                                     (r.Software != null && r.Software.Name.Contains(search)) ||
                                     (r.Platform != null && r.Platform.Name.Contains(search)) ||
                                     (r.Publisher != null && r.Publisher.Name.Contains(search)));

        query = query.OrderBy(r => r.Software != null ? r.Software.Name : r.Title)
                     .ThenBy(r => r.SoftwareVersion != null ? r.SoftwareVersion.VersionString : null);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.Select(r => new SoftwareReleaseDto
                     {
                         Id                = r.Id,
                         Title             = r.Title,
                         IsCompilation     = r.IsCompilation,
                         SoftwareId        = r.SoftwareId,
                         Software          = r.Software.Name,
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
                         IsCompilation     = r.IsCompilation,
                         SoftwareId        = r.SoftwareId,
                         Software          = r.Software.Name,
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
                                                   .Where(r => r.SoftwareId == softwareId && !r.IsCompilation);

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
                                                   .Where(r => r.SoftwareId == softwareId && !r.IsCompilation);

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
                         IsCompilation     = r.IsCompilation,
                         SoftwareId        = r.SoftwareId,
                         Software          = r.Software.Name,
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
                                                                      IsCompilation     = r.IsCompilation,
                                                                      SoftwareId        = r.SoftwareId,
                                                                      Software          = r.Software.Name,
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
    public Task<List<SoftwareAttributeDto>> GetAttributesAsync(ulong releaseId) => context.SoftwareAttributes
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

        // Enforce mutual exclusivity: cannot change IsCompilation after creation
        if(model.IsCompilation != dto.IsCompilation)
            return BadRequest("Cannot change a release between single-release and compilation modes.");

        // Cannot change SoftwareId after creation
        if(model.SoftwareId != dto.SoftwareId)
            return BadRequest("Cannot change the software associated with a release.");

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
            IsCompilation     = dto.IsCompilation,
            SoftwareId        = dto.IsCompilation ? null : dto.SoftwareId,
            SoftwareVersionId = dto.IsCompilation ? null : dto.SoftwareVersionId,
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

        context.SoftwareReleases.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    // --- Compilation junction endpoints ---

    [HttpGet("{releaseId:ulong}/versions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareVersionBySoftwareReleaseDto>> GetIncludedVersionsAsync(ulong releaseId) =>
        context.SoftwareVersionBySoftwareRelease
               .Where(x => x.ReleaseId == releaseId)
               .OrderBy(x => x.SoftwareVersion.Software.Name)
               .ThenBy(x => x.SoftwareVersion.VersionString)
               .Select(x => new SoftwareVersionBySoftwareReleaseDto
                {
                    ReleaseId         = x.ReleaseId,
                    SoftwareVersionId = x.SoftwareVersionId,
                    SoftwareVersion   = x.SoftwareVersion.VersionString,
                    SoftwareName      = x.SoftwareVersion.Software.Name
                })
               .ToListAsync();

    [HttpPost("{releaseId:ulong}/versions")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddIncludedVersionAsync(ulong releaseId,
                                                            [FromBody] SoftwareVersionBySoftwareReleaseDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareRelease release = await context.SoftwareReleases.FindAsync(releaseId);

        if(release is null) return NotFound();

        if(!release.IsCompilation)
            return BadRequest("Cannot add included versions to a non-compilation release.");

        // Ensure this is a versioned compilation (not a versionless one)
        bool hasIncludedSoftware = await context.SoftwareBySoftwareRelease
                                                .AnyAsync(x => x.ReleaseId == releaseId);

        if(hasIncludedSoftware)
            return BadRequest("Cannot add included versions to a versionless compilation. Use included software instead.");

        bool exists = await context.SoftwareVersionBySoftwareRelease
                                   .AnyAsync(x => x.ReleaseId         == releaseId
                                               && x.SoftwareVersionId == dto.SoftwareVersionId);

        if(exists) return BadRequest("This version is already included in the release.");

        bool versionExists = await context.SoftwareVersions.AnyAsync(v => v.Id == dto.SoftwareVersionId);

        if(!versionExists) return BadRequest("The specified software version does not exist.");

        await context.SoftwareVersionBySoftwareRelease.AddAsync(new SoftwareVersionBySoftwareRelease
        {
            ReleaseId         = releaseId,
            SoftwareVersionId = dto.SoftwareVersionId
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{releaseId:ulong}/versions/{versionId:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveIncludedVersionAsync(ulong releaseId, ulong versionId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareRelease release = await context.SoftwareReleases.FindAsync(releaseId);

        if(release is null) return NotFound();

        if(!release.IsCompilation)
            return BadRequest("Cannot remove included versions from a non-compilation release.");

        SoftwareVersionBySoftwareRelease entry =
            await context.SoftwareVersionBySoftwareRelease
                         .FirstOrDefaultAsync(x => x.ReleaseId         == releaseId
                                                && x.SoftwareVersionId == versionId);

        if(entry is null) return NotFound();

        context.SoftwareVersionBySoftwareRelease.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    // --- Compilation listing endpoints ---

    [HttpGet("compilations")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareReleaseDto>> GetCompilationsAsync() => context.SoftwareReleases
       .Where(r => r.IsCompilation)
       .OrderBy(r => r.Title)
       .Select(r => new SoftwareReleaseDto
        {
            Id                = r.Id,
            Title             = r.Title,
            IsCompilation     = r.IsCompilation,
            SoftwareId        = r.SoftwareId,
            Software          = r.Software.Name,
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
    public async Task<ActionResult<List<SoftwareReleaseDto>>> GetCompilationsForSoftwareAsync(ulong softwareId)
    {
        // Collect release IDs from both versioned and versionless compilations
        var versionedIds = context.SoftwareVersionBySoftwareRelease
                                  .Where(x => x.SoftwareVersion.SoftwareId == softwareId)
                                  .Select(x => x.ReleaseId);

        var versionlessIds = context.SoftwareBySoftwareRelease
                                    .Where(x => x.SoftwareId == softwareId)
                                    .Select(x => x.ReleaseId);

        var releaseIds = await versionedIds.Union(versionlessIds).Distinct().ToListAsync();

        return await context.SoftwareReleases
                    .Where(r => releaseIds.Contains(r.Id))
                    .OrderBy(r => r.Title)
                    .Select(r => new SoftwareReleaseDto
                     {
                         Id                = r.Id,
                         Title             = r.Title,
                         IsCompilation     = r.IsCompilation,
                         SoftwareId        = r.SoftwareId,
                         Software          = r.Software.Name,
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
    }

    // --- Versionless compilation junction endpoints ---

    [HttpGet("{releaseId:ulong}/software")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareBySoftwareReleaseDto>> GetIncludedSoftwareAsync(ulong releaseId) =>
        context.SoftwareBySoftwareRelease
               .Where(x => x.ReleaseId == releaseId)
               .OrderBy(x => x.Software.Name)
               .Select(x => new SoftwareBySoftwareReleaseDto
                {
                    ReleaseId    = x.ReleaseId,
                    SoftwareId   = x.SoftwareId,
                    SoftwareName = x.Software.Name
                })
               .ToListAsync();

    [HttpPost("{releaseId:ulong}/software")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddIncludedSoftwareAsync(ulong releaseId,
                                                             [FromBody] SoftwareBySoftwareReleaseDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareRelease release = await context.SoftwareReleases.FindAsync(releaseId);

        if(release is null) return NotFound();

        if(!release.IsCompilation)
            return BadRequest("Cannot add included software to a non-compilation release.");

        // Ensure this is a versionless compilation (not a versioned one)
        bool hasIncludedVersions = await context.SoftwareVersionBySoftwareRelease
                                                .AnyAsync(x => x.ReleaseId == releaseId);

        if(hasIncludedVersions)
            return BadRequest("Cannot add included software to a versioned compilation. Use included versions instead.");

        bool exists = await context.SoftwareBySoftwareRelease
                                   .AnyAsync(x => x.ReleaseId   == releaseId
                                               && x.SoftwareId  == dto.SoftwareId);

        if(exists) return BadRequest("This software is already included in the release.");

        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == dto.SoftwareId);

        if(!softwareExists) return BadRequest("The specified software does not exist.");

        await context.SoftwareBySoftwareRelease.AddAsync(new SoftwareBySoftwareRelease
        {
            ReleaseId  = releaseId,
            SoftwareId = dto.SoftwareId
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{releaseId:ulong}/software/{softwareId:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveIncludedSoftwareAsync(ulong releaseId, ulong softwareId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareRelease release = await context.SoftwareReleases.FindAsync(releaseId);

        if(release is null) return NotFound();

        if(!release.IsCompilation)
            return BadRequest("Cannot remove included software from a non-compilation release.");

        SoftwareBySoftwareRelease entry =
            await context.SoftwareBySoftwareRelease
                         .FirstOrDefaultAsync(x => x.ReleaseId  == releaseId
                                                && x.SoftwareId == softwareId);

        if(entry is null) return NotFound();

        context.SoftwareBySoftwareRelease.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    async Task<string> BuildSoftwareReleaseNewsNameAsync(SoftwareRelease model)
    {
        // Use Title if available
        if(!string.IsNullOrWhiteSpace(model.Title))
        {
            if(model.PlatformId is not null)
            {
                SoftwarePlatform platform = await context.SoftwarePlatforms.FindAsync(model.PlatformId);

                if(platform is not null) return $"{model.Title} ({platform.Name})";
            }

            return model.Title;
        }

        // Single-version release: build from version info
        if(model.SoftwareVersionId is not null)
        {
            string name = "";

            SoftwareVersion version = await context.SoftwareVersions.Include(v => v.Software)
                                                   .FirstOrDefaultAsync(v => v.Id == model.SoftwareVersionId);

            if(version?.Software is not null)
                name = $"{version.Software.Name} {version.VersionString}";
            else if(version is not null)
                name = version.VersionString;

            if(model.PlatformId is not null)
            {
                SoftwarePlatform platform = await context.SoftwarePlatforms.FindAsync(model.PlatformId);

                if(platform is not null)
                    name = string.IsNullOrEmpty(name) ? platform.Name : $"{name} ({platform.Name})";
            }

            return name;
        }

        // Versionless single release: build from software name
        if(!model.IsCompilation && model.SoftwareId is not null)
        {
            Software software = await context.Softwares.FindAsync(model.SoftwareId);
            string   name     = software?.Name ?? "";

            if(model.PlatformId is not null)
            {
                SoftwarePlatform platform = await context.SoftwarePlatforms.FindAsync(model.PlatformId);

                if(platform is not null)
                    name = string.IsNullOrEmpty(name) ? platform.Name : $"{name} ({platform.Name})";
            }

            return name;
        }

        // Compilation without title: build from included versions or software
        List<string> versionNames = await context.SoftwareVersionBySoftwareRelease
                                                 .Where(x => x.ReleaseId == model.Id)
                                                 .OrderBy(x => x.SoftwareVersion.Software.Name)
                                                 .Select(x => $"{x.SoftwareVersion.Software.Name} {x.SoftwareVersion.VersionString}")
                                                 .ToListAsync();

        List<string> softwareNames = await context.SoftwareBySoftwareRelease
                                                  .Where(x => x.ReleaseId == model.Id)
                                                  .OrderBy(x => x.Software.Name)
                                                  .Select(x => x.Software.Name)
                                                  .ToListAsync();

        List<string> allNames       = versionNames.Concat(softwareNames).ToList();
        string       compilationName = allNames.Count > 0 ? string.Join(" + ", allNames) : "Compilation";

        if(model.PlatformId is not null)
        {
            SoftwarePlatform plat = await context.SoftwarePlatforms.FindAsync(model.PlatformId);

            if(plat is not null) compilationName = $"{compilationName} ({plat.Name})";
        }

        return compilationName;
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

        if(exists) return BadRequest("This region is already assigned to the release.");

        bool regionExists = await context.UnM49.AnyAsync(r => r.Id == dto.UnM49Id);

        if(!regionExists) return BadRequest("The specified region does not exist.");

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

        if(exists) return BadRequest("This language is already assigned to the release.");

        bool languageExists = await context.Iso639.AnyAsync(l => l.Id == dto.LanguageCode);

        if(!languageExists) return BadRequest("The specified language does not exist.");

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
