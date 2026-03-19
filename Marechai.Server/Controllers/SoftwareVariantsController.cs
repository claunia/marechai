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

[Route("/software/variants")]
[ApiController]
public class SoftwareVariantsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareVariantDto>> GetAsync() => context.SoftwareVariants
                                                               .OrderBy(v => v.Software.Name)
                                                               .ThenBy(v => v.Name)
                                                               .Select(v => new SoftwareVariantDto
                                                                {
                                                                    Id         = v.Id,
                                                                    SoftwareId = v.SoftwareId,
                                                                    Software   = v.Software.Name,
                                                                    Name       = v.Name
                                                                })
                                                               .ToListAsync();

    [HttpGet("/software/{softwareId:ulong}/variants")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareVariantDto>> GetBySoftwareAsync(ulong softwareId) => context.SoftwareVariants
       .Where(v => v.SoftwareId == softwareId)
       .OrderBy(v => v.Name)
       .Select(v => new SoftwareVariantDto
        {
            Id         = v.Id,
            SoftwareId = v.SoftwareId,
            Software   = v.Software.Name,
            Name       = v.Name
        })
       .ToListAsync();

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwareVariantDto> GetAsync(ulong id) => context.SoftwareVariants.Where(v => v.Id == id)
                                                                 .Select(v => new SoftwareVariantDto
                                                                  {
                                                                      Id         = v.Id,
                                                                      SoftwareId = v.SoftwareId,
                                                                      Software   = v.Software.Name,
                                                                      Name       = v.Name
                                                                  })
                                                                 .FirstOrDefaultAsync();

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwareVariantDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwareVariant model = await context.SoftwareVariants.FindAsync(id);

        if(model is null) return NotFound();

        model.SoftwareId = dto.SoftwareId;
        model.Name       = dto.Name;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwareVariantDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwareVariant
        {
            SoftwareId = dto.SoftwareId,
            Name       = dto.Name
        };

        await context.SoftwareVariants.AddAsync(model);
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
        SoftwareVariant item = await context.SoftwareVariants.FindAsync(id);

        if(item is null) return NotFound();

        context.SoftwareVariants.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
