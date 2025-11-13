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

[Route("/screens")]
[ApiController]
public class ScreensController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<ScreenDto>> GetAsync() => (await context.Screens.Select(s => new ScreenDto
                                                                    {
                                                                        Diagonal           = s.Diagonal,
                                                                        EffectiveColors    = s.EffectiveColors,
                                                                        Height             = s.Height,
                                                                        Id                 = s.Id,
                                                                        Type               = s.Type,
                                                                        Width              = s.Width,
                                                                        NativeResolutionId = s.NativeResolutionId,
                                                                        NativeResolution = new ResolutionDto
                                                                        {
                                                                            Chars     = s.NativeResolution.Chars,
                                                                            Colors    = s.NativeResolution.Colors,
                                                                            Grayscale = s.NativeResolution.Grayscale,
                                                                            Height    = s.NativeResolution.Height,
                                                                            Id        = s.NativeResolution.Id,
                                                                            Palette   = s.NativeResolution.Palette,
                                                                            Width     = s.NativeResolution.Width
                                                                        }
                                                                    })
                                                                   .ToListAsync()).OrderBy(s => s.Diagonal)
       .ThenBy(s => s.EffectiveColors)
       .ThenBy(s => s.NativeResolution.ToString())
       .ThenBy(s => s.Type)
       .ThenBy(s => s.Size)
       .ToList();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ScreenDto> GetAsync(int id) => context.Screens.Where(s => s.Id == id)
                                                      .Select(s => new ScreenDto
                                                       {
                                                           Diagonal        = s.Diagonal,
                                                           EffectiveColors = s.EffectiveColors,
                                                           Height          = s.Height,
                                                           Id              = s.Id,
                                                           NativeResolution = new ResolutionDto
                                                           {
                                                               Chars     = s.NativeResolution.Chars,
                                                               Colors    = s.NativeResolution.Colors,
                                                               Grayscale = s.NativeResolution.Grayscale,
                                                               Height    = s.NativeResolution.Height,
                                                               Id        = s.NativeResolution.Id,
                                                               Palette   = s.NativeResolution.Palette,
                                                               Width     = s.NativeResolution.Width
                                                           },
                                                           NativeResolutionId = s.NativeResolutionId,
                                                           Type               = s.Type,
                                                           Width              = s.Width
                                                       })
                                                      .FirstOrDefaultAsync();

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] ScreenDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Screen model = await context.Screens.FindAsync(id);

        if(model is null) return NotFound();

        Resolution nativeResolution = await context.Resolutions.FindAsync(dto.NativeResolutionId);

        if(nativeResolution is null) return NotFound();

        model.Diagonal           = dto.Diagonal;
        model.EffectiveColors    = dto.EffectiveColors;
        model.Height             = dto.Height;
        model.NativeResolutionId = dto.NativeResolutionId;
        model.Type               = dto.Type;
        model.Width              = dto.Width;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] ScreenDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Screen
        {
            Diagonal           = dto.Diagonal,
            EffectiveColors    = dto.EffectiveColors,
            Height             = dto.Height,
            NativeResolutionId = dto.NativeResolutionId,
            Type               = dto.Type,
            Width              = dto.Width
        };

        await context.Screens.AddAsync(model);
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
        Screen item = await context.Screens.FindAsync(id);

        if(item is null) return NotFound();

        context.Screens.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}