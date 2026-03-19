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

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/software/versions")]
[ApiController]
public class SoftwareVersionsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareVersionDto>> GetAsync() => context.SoftwareVersions
                                                               .OrderBy(v => v.Software.Name)
                                                               .ThenBy(v => v.VersionString)
                                                               .Select(v => new SoftwareVersionDto
                                                                {
                                                                    Id              = v.Id,
                                                                    SoftwareId      = v.SoftwareId,
                                                                    Software        = v.Software.Name,
                                                                    Codename        = v.Codename,
                                                                    VersionString   = v.VersionString,
                                                                    PublicVersion   = v.PublicVersion,
                                                                    ParentVersionId = v.ParentVersionId,
                                                                    ParentVersion   = v.ParentVersion.VersionString,
                                                                    LicenseId       = v.LicenseId,
                                                                    License         = v.License.Name
                                                                })
                                                               .ToListAsync();

    [HttpGet("/software/{softwareId:ulong}/versions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareVersionDto>> GetBySoftwareAsync(ulong softwareId) => context.SoftwareVersions
       .Where(v => v.SoftwareId == softwareId)
       .OrderBy(v => v.VersionString)
       .Select(v => new SoftwareVersionDto
        {
            Id              = v.Id,
            SoftwareId      = v.SoftwareId,
            Software        = v.Software.Name,
            Codename        = v.Codename,
            VersionString   = v.VersionString,
            PublicVersion   = v.PublicVersion,
            ParentVersionId = v.ParentVersionId,
            ParentVersion   = v.ParentVersion.VersionString,
            LicenseId       = v.LicenseId,
            License         = v.License.Name
        })
       .ToListAsync();

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwareVersionDto> GetAsync(ulong id) => context.SoftwareVersions.Where(v => v.Id == id)
                                                                 .Select(v => new SoftwareVersionDto
                                                                  {
                                                                      Id              = v.Id,
                                                                      SoftwareId      = v.SoftwareId,
                                                                      Software        = v.Software.Name,
                                                                      Codename        = v.Codename,
                                                                      VersionString   = v.VersionString,
                                                                      PublicVersion   = v.PublicVersion,
                                                                      ParentVersionId = v.ParentVersionId,
                                                                      ParentVersion   = v.ParentVersion.VersionString,
                                                                      LicenseId       = v.LicenseId,
                                                                      License         = v.License.Name
                                                                  })
                                                                 .FirstOrDefaultAsync();

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwareVersionDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwareVersion model = await context.SoftwareVersions.FindAsync(id);

        if(model is null) return NotFound();

        model.SoftwareId      = dto.SoftwareId;
        model.Codename        = dto.Codename;
        model.VersionString   = dto.VersionString;
        model.PublicVersion   = dto.PublicVersion;
        model.ParentVersionId = dto.ParentVersionId;
        model.LicenseId       = dto.LicenseId;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwareVersionDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwareVersion
        {
            SoftwareId      = dto.SoftwareId,
            Codename        = dto.Codename,
            VersionString   = dto.VersionString,
            PublicVersion   = dto.PublicVersion,
            ParentVersionId = dto.ParentVersionId,
            LicenseId       = dto.LicenseId
        };

        await context.SoftwareVersions.AddAsync(model);
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
        SoftwareVersion item = await context.SoftwareVersions.FindAsync(id);

        if(item is null) return NotFound();

        context.SoftwareVersions.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
