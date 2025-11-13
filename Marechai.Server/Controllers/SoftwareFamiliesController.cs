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

[Route("/software/families")]
[ApiController]
public class SoftwareFamiliesController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareFamilyDto>> GetAsync() => context.SoftwareFamilies.OrderBy(b => b.Name)
                                                              .Select(b => new SoftwareFamilyDto
                                                               {
                                                                   Id         = b.Id,
                                                                   Name       = b.Name,
                                                                   Parent     = b.Parent.Name,
                                                                   Introduced = b.Introduced,
                                                                   ParentId   = b.ParentId
                                                               })
                                                              .ToListAsync();

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwareFamilyDto> GetAsync(ulong id) => context.SoftwareFamilies.Where(b => b.Id == id)
                                                                .Select(b => new SoftwareFamilyDto
                                                                 {
                                                                     Id         = b.Id,
                                                                     Name       = b.Name,
                                                                     Parent     = b.Parent.Name,
                                                                     Introduced = b.Introduced,
                                                                     ParentId   = b.ParentId
                                                                 })
                                                                .FirstOrDefaultAsync();

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwareFamilyDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwareFamily model = await context.SoftwareFamilies.FindAsync(id);

        if(model is null) return NotFound();

        model.Name       = dto.Name;
        model.ParentId   = dto.ParentId;
        model.Introduced = dto.Introduced;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwareFamilyDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwareFamily
        {
            Name       = dto.Name,
            ParentId   = dto.ParentId,
            Introduced = dto.Introduced
        };

        await context.SoftwareFamilies.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwareFamily item = await context.SoftwareFamilies.FindAsync(id);

        if(item is null) return NotFound();

        context.SoftwareFamilies.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}