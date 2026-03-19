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

[Route("/software/subvariants")]
[ApiController]
public class SoftwareSubvariantsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareSubvariantDto>> GetAsync() => context.SoftwareSubvariants
                                                                  .OrderBy(s => s.Variant.Name)
                                                                  .ThenBy(s => s.Name)
                                                                  .Select(s => new SoftwareSubvariantDto
                                                                   {
                                                                       Id        = s.Id,
                                                                       VariantId = s.VariantId,
                                                                       Variant   = s.Variant.Name,
                                                                       Name      = s.Name
                                                                   })
                                                                  .ToListAsync();

    [HttpGet("/software/variants/{variantId:ulong}/subvariants")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareSubvariantDto>> GetByVariantAsync(ulong variantId) => context.SoftwareSubvariants
       .Where(s => s.VariantId == variantId)
       .OrderBy(s => s.Name)
       .Select(s => new SoftwareSubvariantDto
        {
            Id        = s.Id,
            VariantId = s.VariantId,
            Variant   = s.Variant.Name,
            Name      = s.Name
        })
       .ToListAsync();

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwareSubvariantDto> GetAsync(ulong id) => context.SoftwareSubvariants.Where(s => s.Id == id)
                                                                    .Select(s => new SoftwareSubvariantDto
                                                                     {
                                                                         Id        = s.Id,
                                                                         VariantId = s.VariantId,
                                                                         Variant   = s.Variant.Name,
                                                                         Name      = s.Name
                                                                     })
                                                                    .FirstOrDefaultAsync();

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwareSubvariantDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwareSubvariant model = await context.SoftwareSubvariants.FindAsync(id);

        if(model is null) return NotFound();

        model.VariantId = dto.VariantId;
        model.Name      = dto.Name;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwareSubvariantDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwareSubvariant
        {
            VariantId = dto.VariantId,
            Name      = dto.Name
        };

        await context.SoftwareSubvariants.AddAsync(model);
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
        SoftwareSubvariant item = await context.SoftwareSubvariants.FindAsync(id);

        if(item is null) return NotFound();

        context.SoftwareSubvariants.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
