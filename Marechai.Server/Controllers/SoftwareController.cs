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

[Route("/software")]
[ApiController]
public class SoftwareController(MarechaiContext context) : ControllerBase
{
    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetSoftwareCountAsync() => context.Softwares.CountAsync();

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
       .OrderBy(s => s.Name)
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            IsOperatingSystem = s.IsOperatingSystem,
            IsGame            = s.IsGame
        })
       .ToListAsync();

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareByYearAsync(int year) => context.Softwares
       .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.ReleaseDate != null &&
                                                           r.ReleaseDate.Value.Year == year)))
       .OrderBy(s => s.Name)
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            IsOperatingSystem = s.IsOperatingSystem,
            IsGame            = s.IsGame
        })
       .ToListAsync();

    [HttpGet("by-platform/{platformId:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareByPlatformAsync(ulong platformId) => context.Softwares
       .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.PlatformId == platformId)))
       .OrderBy(s => s.Name)
       .Select(s => new SoftwareDto
        {
            Id                = s.Id,
            Name              = s.Name,
            FamilyId          = s.FamilyId,
            Family            = s.Family.Name,
            IsOperatingSystem = s.IsOperatingSystem,
            IsGame            = s.IsGame
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
    public Task<List<SoftwareDto>> GetAsync() => context.Softwares.OrderBy(s => s.Name)
                                                        .Select(s => new SoftwareDto
                                                         {
                                                             Id                = s.Id,
                                                             Name              = s.Name,
                                                             FamilyId          = s.FamilyId,
                                                             Family            = s.Family.Name,
                                                             IsOperatingSystem = s.IsOperatingSystem,
                                                             IsGame            = s.IsGame
                                                         })
                                                        .ToListAsync();

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
}
