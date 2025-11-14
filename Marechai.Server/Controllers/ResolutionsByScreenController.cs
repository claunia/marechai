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

[Route("/resolutions-by-screen")]
[ApiController]
public class ResolutionsByScreenController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/screens/{screenId:int}/resolutions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<ResolutionByScreenDto>> GetByScreen(int screenId) => (await context.ResolutionsByScreen
                   .Where(r => r.ScreenId == screenId)
                   .Select(r => new ResolutionByScreenDto
                    {
                        Id       = r.Id,
                        ScreenId = r.ScreenId,
                        Resolution = new ResolutionDto
                        {
                            Id        = r.Resolution.Id,
                            Width     = r.Resolution.Width,
                            Height    = r.Resolution.Height,
                            Colors    = r.Resolution.Colors,
                            Palette   = r.Resolution.Palette,
                            Chars     = r.Resolution.Chars,
                            Grayscale = r.Resolution.Grayscale
                        },
                        ResolutionId = r.ResolutionId
                    })
                   .ToListAsync()).OrderBy(r => r.Resolution.Width)
                                  .ThenBy(r => r.Resolution.Height)
                                  .ThenBy(r => r.Resolution.Chars)
                                  .ThenBy(r => r.Resolution.Grayscale)
                                  .ThenBy(r => r.Resolution.Colors)
                                  .ThenBy(r => r.Resolution.Palette)
                                  .ToList();

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        ResolutionsByScreen item = await context.ResolutionsByScreen.FindAsync(id);

        if(item is null) return NotFound();

        context.ResolutionsByScreen.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] ResolutionByScreenDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var item = new ResolutionsByScreen
        {
            ScreenId     = dto.ScreenId,
            ResolutionId = dto.ResolutionId
        };

        await context.ResolutionsByScreen.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }
}