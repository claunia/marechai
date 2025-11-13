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
// Copyright © 2003-2025 Natalia Portillo
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

[Route("/software-versions")]
[ApiController]
public class SoftwareVersionsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareVersionDto>> GetAsync() => context.SoftwareVersions.OrderBy(b => b.Family.Name)
                                                               .ThenBy(b => b.Version)
                                                               .ThenBy(b => b.Introduced)
                                                               .Select(b => new SoftwareVersionDto
                                                                {
                                                                    Id         = b.Id,
                                                                    Family     = b.Family.Name,
                                                                    Name       = b.Name,
                                                                    Codename   = b.Codename,
                                                                    Version    = b.Version,
                                                                    Introduced = b.Introduced,
                                                                    Previous   = b.Previous.Name,
                                                                    License    = b.License.Name,
                                                                    FamilyId   = b.FamilyId,
                                                                    LicenseId  = b.LicenseId,
                                                                    PreviousId = b.PreviousId
                                                                })
                                                               .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwareVersionDto> GetAsync(ulong id) => context.SoftwareVersions.Where(b => b.Id == id)
                                                                 .Select(b => new SoftwareVersionDto
                                                                  {
                                                                      Id         = b.Id,
                                                                      Family     = b.Family.Name,
                                                                      Name       = b.Name,
                                                                      Codename   = b.Codename,
                                                                      Version    = b.Version,
                                                                      Introduced = b.Introduced,
                                                                      Previous   = b.Previous.Name,
                                                                      License    = b.License.Name,
                                                                      FamilyId   = b.FamilyId,
                                                                      LicenseId  = b.LicenseId,
                                                                      PreviousId = b.PreviousId
                                                                  })
                                                                 .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateAsync(SoftwareVersionDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwareVersion model = await context.SoftwareVersions.FindAsync(dto.Id);

        if(model is null) return NotFound();

        model.Name       = dto.Name;
        model.Codename   = dto.Codename;
        model.Version    = dto.Version;
        model.Introduced = dto.Introduced;
        model.FamilyId   = dto.FamilyId;
        model.LicenseId  = dto.LicenseId;
        model.PreviousId = dto.PreviousId;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ulong>> CreateAsync(SoftwareVersionDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwareVersion
        {
            Name       = dto.Name,
            Codename   = dto.Codename,
            Version    = dto.Version,
            Introduced = dto.Introduced,
            FamilyId   = dto.FamilyId,
            LicenseId  = dto.LicenseId,
            PreviousId = dto.PreviousId
        };

        await context.SoftwareVersions.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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