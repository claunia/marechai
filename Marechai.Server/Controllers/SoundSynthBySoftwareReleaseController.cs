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

[Route("/software/releases/sound-synths")]
[ApiController]
public class SoundSynthBySoftwareReleaseController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/software/releases/{releaseId:ulong}/sound-synths")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoundSynthBySoftwareReleaseDto>> GetByReleaseAsync(ulong releaseId) =>
        context.SoundSynthBySoftwareRelease
               .AsNoTracking()
               .Where(p => p.ReleaseId == releaseId)
               .Select(p => new SoundSynthBySoftwareReleaseDto
                {
                    ReleaseId    = p.ReleaseId,
                    SoundSynthId = p.SoundSynthId,
                    SoundSynth   = p.SoundSynth.Name
                })
               .OrderBy(p => p.SoundSynth)
               .ToListAsync();

    [HttpDelete("{releaseId:ulong}/{soundSynthId:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong releaseId, int soundSynthId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoundSynthBySoftwareRelease item =
            await context.SoundSynthBySoftwareRelease.FindAsync(releaseId, soundSynthId);

        if(item is null) return NotFound();

        context.SoundSynthBySoftwareRelease.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateAsync([FromBody] SoundSynthBySoftwareReleaseDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var item = new SoundSynthBySoftwareRelease
        {
            ReleaseId    = dto.ReleaseId,
            SoundSynthId = dto.SoundSynthId
        };

        await context.SoundSynthBySoftwareRelease.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
