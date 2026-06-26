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

[Route("/software-compilations")]
[ApiController]
public class SoftwareCompilationsController(MarechaiContext context) : ControllerBase
{
    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<int> GetCountAsync([FromQuery] string search = null)
    {
        IQueryable<SoftwareCompilation> query = context.SoftwareCompilations;

        if(!string.IsNullOrWhiteSpace(search)) query = query.Where(c => c.Name.Contains(search));

        return query.CountAsync();
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareCompilationDto>> GetAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                                        [FromQuery] string search = null)
    {
        IQueryable<SoftwareCompilation> query = context.SoftwareCompilations;

        if(!string.IsNullOrWhiteSpace(search)) query = query.Where(c => c.Name.Contains(search));

        query = query.OrderBy(c => MarechaiContext.NaturalSortKey(c.Name));

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.Select(c => ProjectToDto(c)).ToListAsync();
    }

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<SoftwareCompilationDto> GetAsync(ulong id) =>
        context.SoftwareCompilations.Where(c => c.Id == id).Select(c => ProjectToDto(c)).FirstOrDefaultAsync();

    static SoftwareCompilationDto ProjectToDto(SoftwareCompilation c) => new()
    {
        Id               = c.Id,
        Name             = c.Name,
        SoftwareId       = c.SoftwareId,
        Software         = c.Software.Name,
        MachineId        = c.MachineId,
        Machine          = c.Machine.Name,
        PredecessorId    = c.PredecessorId,
        Predecessor      = c.Predecessor.Name,
        RelationshipType = c.RelationshipType,
        Successors = c.Successors.Select(x => new SoftwareCompilationSuccessorDto
        {
            Id               = x.Id,
            Name             = x.Name,
            RelationshipType = x.RelationshipType
        }).ToList()
    };

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwareCompilationDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwareCompilation
        {
            Name             = dto.Name,
            SoftwareId       = dto.SoftwareId,
            MachineId        = dto.MachineId,
            PredecessorId    = dto.PredecessorId,
            RelationshipType = dto.RelationshipType
        };

        await context.SoftwareCompilations.AddAsync(model);

        await context.News.AddAsync(new News
        {
            Date = DateTime.UtcNow,
            Type = NewsType.NewSoftwareInDb,
            Name = model.Name
        });

        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwareCompilationDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareCompilation model = await context.SoftwareCompilations.FindAsync(id);

        if(model is null) return NotFound();

        model.Name             = dto.Name;
        model.SoftwareId       = dto.SoftwareId;
        model.MachineId        = dto.MachineId;
        model.PredecessorId    = dto.PredecessorId;
        model.RelationshipType = dto.RelationshipType;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareCompilation model = await context.SoftwareCompilations.FindAsync(id);

        if(model is null) return NotFound();

        context.SoftwareCompilations.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpGet("{id:ulong}/releases")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareReleaseDto>> GetReleasesAsync(ulong id) => context.SoftwareReleases
       .Where(r => r.SoftwareCompilationId == id)
       .OrderBy(r => r.ReleaseDate)
       .Select(r => new SoftwareReleaseDto
        {
            Id                    = r.Id,
            Title                 = r.Title,
            SoftwareCompilationId = r.SoftwareCompilationId,
            SoftwareCompilation   = r.SoftwareCompilation.Name,
            PlatformId            = r.PlatformId,
            Platform              = r.Platform.Name,
            PublisherId           = r.PublisherId,
            Publisher             = r.Publisher.Name,
            ReleaseDate           = r.ReleaseDate,
            ReleaseDatePrecision  = r.ReleaseDatePrecision
        })
       .ToListAsync();

    // --- Covers ---

    [HttpGet("{id:ulong}/covers")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareCoverDto>> GetCoversAsync(ulong id) => context.SoftwareCovers
       .Where(c => c.SoftwareCompilationId == id)
       .Select(c => new SoftwareCoverDto
        {
            Id                = c.Id,
            SoftwareId        = c.SoftwareId,
            SoftwareReleaseId = c.SoftwareReleaseId,
            GroupId           = c.GroupId,
            Type              = (int)c.Type,
            Caption           = c.Caption,
            OriginalExtension = c.OriginalExtension
        })
       .ToListAsync();

    // --- Included software / version junction endpoints ---

    [HttpGet("{id:ulong}/software")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareBySoftwareCompilationDto>> GetIncludedSoftwareAsync(ulong id) =>
        context.SoftwareBySoftwareCompilation
               .Where(x => x.SoftwareCompilationId == id)
               .OrderBy(x => x.Software.Name)
               .Select(x => new SoftwareBySoftwareCompilationDto
                {
                    SoftwareCompilationId = x.SoftwareCompilationId,
                    SoftwareId            = x.SoftwareId,
                    SoftwareName          = x.Software.Name
                })
               .ToListAsync();

    [HttpPost("{id:ulong}/software")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddIncludedSoftwareAsync(ulong id, [FromBody] SoftwareBySoftwareCompilationDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareCompilation compilation = await context.SoftwareCompilations.FindAsync(id);

        if(compilation is null) return NotFound();

        bool exists = await context.SoftwareBySoftwareCompilation
                                   .AnyAsync(x => x.SoftwareCompilationId == id && x.SoftwareId == dto.SoftwareId);

        if(exists) return Problem(detail: "This software is already included in the compilation.", statusCode: StatusCodes.Status400BadRequest);

        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == dto.SoftwareId);

        if(!softwareExists) return Problem(detail: "The specified software does not exist.", statusCode: StatusCodes.Status400BadRequest);

        await context.SoftwareBySoftwareCompilation.AddAsync(new SoftwareBySoftwareCompilation
        {
            SoftwareCompilationId = id,
            SoftwareId            = dto.SoftwareId
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:ulong}/software/{softwareId:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveIncludedSoftwareAsync(ulong id, ulong softwareId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareBySoftwareCompilation entry =
            await context.SoftwareBySoftwareCompilation
                         .FirstOrDefaultAsync(x => x.SoftwareCompilationId == id && x.SoftwareId == softwareId);

        if(entry is null) return NotFound();

        context.SoftwareBySoftwareCompilation.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpGet("{id:ulong}/versions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareVersionBySoftwareCompilationDto>> GetIncludedVersionsAsync(ulong id) =>
        context.SoftwareVersionBySoftwareCompilation
               .Where(x => x.SoftwareCompilationId == id)
               .OrderBy(x => x.SoftwareVersion.Software.Name)
               .ThenBy(x => x.SoftwareVersion.VersionString)
               .Select(x => new SoftwareVersionBySoftwareCompilationDto
                {
                    SoftwareCompilationId = x.SoftwareCompilationId,
                    SoftwareVersionId     = x.SoftwareVersionId,
                    SoftwareVersion       = x.SoftwareVersion.VersionString,
                    SoftwareName          = x.SoftwareVersion.Software.Name
                })
               .ToListAsync();

    [HttpPost("{id:ulong}/versions")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddIncludedVersionAsync(ulong id,
                                                            [FromBody] SoftwareVersionBySoftwareCompilationDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareCompilation compilation = await context.SoftwareCompilations.FindAsync(id);

        if(compilation is null) return NotFound();

        bool exists = await context.SoftwareVersionBySoftwareCompilation
                                   .AnyAsync(x => x.SoftwareCompilationId == id &&
                                                  x.SoftwareVersionId     == dto.SoftwareVersionId);

        if(exists) return Problem(detail: "This version is already included in the compilation.", statusCode: StatusCodes.Status400BadRequest);

        bool versionExists = await context.SoftwareVersions.AnyAsync(v => v.Id == dto.SoftwareVersionId);

        if(!versionExists) return Problem(detail: "The specified software version does not exist.", statusCode: StatusCodes.Status400BadRequest);

        await context.SoftwareVersionBySoftwareCompilation.AddAsync(new SoftwareVersionBySoftwareCompilation
        {
            SoftwareCompilationId = id,
            SoftwareVersionId     = dto.SoftwareVersionId
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:ulong}/versions/{versionId:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveIncludedVersionAsync(ulong id, ulong versionId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareVersionBySoftwareCompilation entry =
            await context.SoftwareVersionBySoftwareCompilation
                         .FirstOrDefaultAsync(x => x.SoftwareCompilationId == id &&
                                                    x.SoftwareVersionId     == versionId);

        if(entry is null) return NotFound();

        context.SoftwareVersionBySoftwareCompilation.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    // --- Nested-compilation junction endpoints (a child compilation can have multiple parents) ---

    [HttpGet("{id:ulong}/compilations")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareCompilationDto>> GetIncludedCompilationsAsync(ulong id) =>
        context.SoftwareCompilationBySoftwareCompilation
               .Where(x => x.ParentCompilationId == id)
               .OrderBy(x => x.ChildCompilation.Name)
               .Select(x => ProjectToDto(x.ChildCompilation))
               .ToListAsync();

    [HttpPost("{id:ulong}/compilations/{childId:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddIncludedCompilationAsync(ulong id, ulong childId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(id == childId)
            return Problem(detail: "A compilation cannot contain itself.", statusCode: StatusCodes.Status400BadRequest);

        bool parentExists = await context.SoftwareCompilations.AnyAsync(c => c.Id == id);
        bool childExists  = await context.SoftwareCompilations.AnyAsync(c => c.Id == childId);

        if(!parentExists || !childExists) return NotFound();

        // Cycle guard: reject if `id` is already a descendant of `childId`, walking the
        // containment junction breadth-first.
        var toVisit = new Queue<ulong>([childId]);
        var visited = new HashSet<ulong>();

        while(toVisit.Count > 0)
        {
            ulong current = toVisit.Dequeue();

            if(current == id)
                return Problem(detail: "Adding this compilation would create a containment cycle.", statusCode: StatusCodes.Status400BadRequest);

            if(!visited.Add(current)) continue;

            List<ulong> children = await context.SoftwareCompilationBySoftwareCompilation
                                                 .Where(x => x.ParentCompilationId == current)
                                                 .Select(x => x.ChildCompilationId)
                                                 .ToListAsync();

            foreach(ulong child in children) toVisit.Enqueue(child);
        }

        bool exists = await context.SoftwareCompilationBySoftwareCompilation
                                   .AnyAsync(x => x.ParentCompilationId == id && x.ChildCompilationId == childId);

        if(exists) return Problem(detail: "This compilation is already included.", statusCode: StatusCodes.Status400BadRequest);

        await context.SoftwareCompilationBySoftwareCompilation.AddAsync(new SoftwareCompilationBySoftwareCompilation
        {
            ParentCompilationId = id,
            ChildCompilationId  = childId
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:ulong}/compilations/{childId:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveIncludedCompilationAsync(ulong id, ulong childId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareCompilationBySoftwareCompilation entry =
            await context.SoftwareCompilationBySoftwareCompilation
                         .FirstOrDefaultAsync(x => x.ParentCompilationId == id && x.ChildCompilationId == childId);

        if(entry is null) return NotFound();

        context.SoftwareCompilationBySoftwareCompilation.Remove(entry);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
