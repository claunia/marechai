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
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareReleaseDto>> GetAsync() => context.SoftwareReleases
                                                               .OrderBy(r => r.Software.Name)
                                                               .ThenBy(r => r.SoftwareVersion.VersionString)
                                                               .Select(r => new SoftwareReleaseDto
                                                                {
                                                                    Id                = r.Id,
                                                                    Title             = r.Title,
                                                                    IsCompilation     = r.IsCompilation,
                                                                    SoftwareId        = r.SoftwareId,
                                                                    Software          = r.Software.Name,
                                                                    SoftwareVersionId = r.SoftwareVersionId,
                                                                    SoftwareVersion   = r.SoftwareVersion.VersionString,
                                                                    VariantId         = r.VariantId,
                                                                    Variant           = r.Variant.Name,
                                                                    SubvariantId      = r.SubvariantId,
                                                                    Subvariant        = r.Subvariant.Name,
                                                                    PlatformId        = r.PlatformId,
                                                                    Platform          = r.Platform.Name,
                                                                    RegionId          = r.RegionId,
                                                                    Region            = r.Region.Name,
                                                                    PublisherId       = r.PublisherId,
                                                                    Publisher         = r.Publisher.Name,
                                                                    ReleaseDate       = r.ReleaseDate,
                                                                    ReleaseDatePrecision = r.ReleaseDatePrecision
                                                                })
                                                               .ToListAsync();

    [HttpGet("/software/versions/{versionId:ulong}/releases")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareReleaseDto>> GetByVersionAsync(ulong versionId) => context.SoftwareReleases
       .Where(r => r.SoftwareVersionId == versionId)
       .OrderBy(r => r.ReleaseDate)
       .Select(r => new SoftwareReleaseDto
        {
            Id                = r.Id,
            Title             = r.Title,
            IsCompilation     = r.IsCompilation,
            SoftwareId        = r.SoftwareId,
            Software          = r.Software.Name,
            SoftwareVersionId = r.SoftwareVersionId,
            SoftwareVersion   = r.SoftwareVersion.VersionString,
            VariantId         = r.VariantId,
            Variant           = r.Variant.Name,
            SubvariantId      = r.SubvariantId,
            Subvariant        = r.Subvariant.Name,
            PlatformId        = r.PlatformId,
            Platform          = r.Platform.Name,
            RegionId          = r.RegionId,
            Region            = r.Region.Name,
            PublisherId       = r.PublisherId,
            Publisher         = r.Publisher.Name,
            ReleaseDate       = r.ReleaseDate,
            ReleaseDatePrecision = r.ReleaseDatePrecision
        })
       .ToListAsync();

    [HttpGet("/software/{softwareId:ulong}/releases")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareReleaseDto>> GetBySoftwareAsync(ulong softwareId) => context.SoftwareReleases
       .Where(r => r.SoftwareId == softwareId && !r.IsCompilation)
       .OrderBy(r => r.SoftwareVersion.VersionString)
       .ThenBy(r => r.ReleaseDate)
       .Select(r => new SoftwareReleaseDto
        {
            Id                = r.Id,
            Title             = r.Title,
            IsCompilation     = r.IsCompilation,
            SoftwareId        = r.SoftwareId,
            Software          = r.Software.Name,
            SoftwareVersionId = r.SoftwareVersionId,
            SoftwareVersion   = r.SoftwareVersion.VersionString,
            VariantId         = r.VariantId,
            Variant           = r.Variant.Name,
            SubvariantId      = r.SubvariantId,
            Subvariant        = r.Subvariant.Name,
            PlatformId        = r.PlatformId,
            Platform          = r.Platform.Name,
            RegionId          = r.RegionId,
            Region            = r.Region.Name,
            PublisherId       = r.PublisherId,
            Publisher         = r.Publisher.Name,
            ReleaseDate       = r.ReleaseDate,
            ReleaseDatePrecision = r.ReleaseDatePrecision
        })
       .ToListAsync();

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
                                                                      VariantId         = r.VariantId,
                                                                      Variant           = r.Variant.Name,
                                                                      SubvariantId      = r.SubvariantId,
                                                                      Subvariant        = r.Subvariant.Name,
                                                                      PlatformId        = r.PlatformId,
                                                                      Platform          = r.Platform.Name,
                                                                      RegionId          = r.RegionId,
                                                                      Region            = r.Region.Name,
                                                                      PublisherId       = r.PublisherId,
                                                                      Publisher         = r.Publisher.Name,
                                                                      ReleaseDate       = r.ReleaseDate,
                                                                      ReleaseDatePrecision = r.ReleaseDatePrecision
                                                                  })
                                                                 .FirstOrDefaultAsync();

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
        model.VariantId         = dto.VariantId;
        model.SubvariantId      = dto.SubvariantId;
        model.PlatformId        = dto.PlatformId;
        model.RegionId          = dto.RegionId;
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
            VariantId         = dto.VariantId,
            SubvariantId      = dto.SubvariantId,
            PlatformId        = dto.PlatformId,
            RegionId          = dto.RegionId,
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
            RegionId          = r.RegionId,
            Region            = r.Region.Name,
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
        // Versioned compilations containing this software
        var versionedCompilations = context.SoftwareVersionBySoftwareRelease
                                          .Where(x => x.SoftwareVersion.SoftwareId == softwareId)
                                          .Select(x => x.Release);

        // Versionless compilations containing this software
        var versionlessCompilations = context.SoftwareBySoftwareRelease
                                             .Where(x => x.SoftwareId == softwareId)
                                             .Select(x => x.Release);

        return await versionedCompilations
                    .Union(versionlessCompilations)
                    .Distinct()
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
                         RegionId          = r.RegionId,
                         Region            = r.Region.Name,
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
}
