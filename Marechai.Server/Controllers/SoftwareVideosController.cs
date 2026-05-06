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

[Route("/software/videos")]
[ApiController]
public class SoftwareVideosController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/software/{softwareId:ulong}/videos")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareVideoDto>> GetVideosBySoftwareAsync(ulong softwareId) => context.SoftwareVideos
       .Where(v => v.SoftwareId == softwareId)
       .OrderBy(v => v.Title)
       .Select(v => new SoftwareVideoDto
        {
            Id           = v.Id,
            SoftwareId   = v.SoftwareId,
            SoftwareName = v.Software.Name,
            Provider     = v.Provider,
            VideoId      = v.VideoId,
            Title        = v.Title
        })
       .ToListAsync();

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoftwareVideoDto>> GetAsync(long id)
    {
        SoftwareVideoDto dto = await context.SoftwareVideos
                                            .Where(v => v.Id == id)
                                            .Select(v => new SoftwareVideoDto
                                             {
                                                 Id           = v.Id,
                                                 SoftwareId   = v.SoftwareId,
                                                 SoftwareName = v.Software.Name,
                                                 Provider     = v.Provider,
                                                 VideoId      = v.VideoId,
                                                 Title        = v.Title
                                             })
                                            .FirstOrDefaultAsync();

        if(dto is null) return NotFound();

        return dto;
    }

    [HttpPost("/software/{softwareId:ulong}/videos")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SoftwareVideoDto>> CreateAsync(ulong softwareId, [FromBody] SoftwareVideoDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        bool exists = await context.SoftwareVideos.AnyAsync(v => v.SoftwareId == softwareId &&
                                                                  v.Provider   == dto.Provider &&
                                                                  v.VideoId    == dto.VideoId);

        if(exists) return Conflict("This video is already linked to this software.");

        var model = new SoftwareVideo
        {
            SoftwareId = softwareId,
            Provider   = dto.Provider,
            VideoId    = dto.VideoId,
            Title      = dto.Title
        };

        context.SoftwareVideos.Add(model);
        await context.SaveChangesAsync();

        dto.Id         = model.Id;
        dto.SoftwareId = softwareId;

        return CreatedAtAction(nameof(GetAsync), new { id = model.Id }, dto);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareVideo video = await context.SoftwareVideos.FindAsync(id);

        if(video is null) return NotFound();

        context.SoftwareVideos.Remove(video);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
