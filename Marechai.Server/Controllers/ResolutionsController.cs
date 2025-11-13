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

[Route("/resolutions")]
[ApiController]
public class ResolutionsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<ResolutionDto>> GetAsync() => context.Resolutions.Select(r => new ResolutionDto
                                                           {
                                                               Id        = r.Id,
                                                               Width     = r.Width,
                                                               Height    = r.Height,
                                                               Colors    = r.Colors,
                                                               Palette   = r.Palette,
                                                               Chars     = r.Chars,
                                                               Grayscale = r.Grayscale
                                                           })
                                                          .OrderBy(r => r.Width)
                                                          .ThenBy(r => r.Height)
                                                          .ThenBy(r => r.Chars)
                                                          .ThenBy(r => r.Grayscale)
                                                          .ThenBy(r => r.Colors)
                                                          .ThenBy(r => r.Palette)
                                                          .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ResolutionDto> GetAsync(int id) => context.Resolutions.Where(r => r.Id == id)
                                                          .Select(r => new ResolutionDto
                                                           {
                                                               Id        = r.Id,
                                                               Width     = r.Width,
                                                               Height    = r.Height,
                                                               Colors    = r.Colors,
                                                               Palette   = r.Palette,
                                                               Chars     = r.Chars,
                                                               Grayscale = r.Grayscale
                                                           })
                                                          .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync([FromBody] ResolutionDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Resolution model = await context.Resolutions.FindAsync(dto.Id);

        if(model is null) return NotFound();

        model.Chars     = dto.Chars;
        model.Colors    = dto.Colors;
        model.Grayscale = dto.Grayscale;
        model.Height    = dto.Height;
        model.Palette   = dto.Palette;
        model.Width     = dto.Width;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] ResolutionDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Resolution
        {
            Chars     = dto.Chars,
            Colors    = dto.Colors,
            Grayscale = dto.Grayscale,
            Height    = dto.Height,
            Palette   = dto.Palette,
            Width     = dto.Width
        };

        await context.Resolutions.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Resolution item = await context.Resolutions.FindAsync(id);

        if(item is null) return NotFound();

        context.Resolutions.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}