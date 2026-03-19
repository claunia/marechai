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

[Route("/software/os-compatibility")]
[ApiController]
public class SoftwareOSCompatibilityController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/software/versions/{versionId:ulong}/os-compatibility")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareOSCompatibilityDto>> GetByVersionAsync(ulong versionId) => context
       .SoftwareOSCompatibility.Where(c => c.SoftwareVersionId == versionId)
       .Select(c => new SoftwareOSCompatibilityDto
        {
            SoftwareVersionId = c.SoftwareVersionId,
            SoftwareVersion   = c.SoftwareVersion.VersionString,
            OSVersionId       = c.OSVersionId,
            OSVersion         = c.OSVersion.VersionString
        })
       .OrderBy(c => c.OSVersion)
       .ToListAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateAsync([FromBody] SoftwareOSCompatibilityDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwareOSCompatibility
        {
            SoftwareVersionId = dto.SoftwareVersionId,
            OSVersionId       = dto.OSVersionId
        };

        await context.SoftwareOSCompatibility.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{softwareVersionId:ulong}/{osVersionId:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong softwareVersionId, ulong osVersionId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareOSCompatibility item =
            await context.SoftwareOSCompatibility.FindAsync(softwareVersionId, osVersionId);

        if(item is null) return NotFound();

        context.SoftwareOSCompatibility.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
