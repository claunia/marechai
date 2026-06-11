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

[Route("/sound-synths/videos")]
[ApiController]
public class SoundSynthVideosController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/sound-synths/{soundSynthId:int}/videos")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoundSynthVideoDto>> GetVideosBySoundSynthAsync(int soundSynthId) => context.SoundSynthVideos
       .Where(v => v.SoundSynthId == soundSynthId)
       .OrderBy(v => v.Title)
       .Select(v => new SoundSynthVideoDto
        {
            Id             = v.Id,
            SoundSynthId   = v.SoundSynthId,
            SoundSynthName = v.SoundSynth.Name,
            Provider       = v.Provider,
            VideoId        = v.VideoId,
            Title          = v.Title
        })
       .ToListAsync();

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoundSynthVideoDto>> GetAsync(long id)
    {
        SoundSynthVideoDto dto = await context.SoundSynthVideos
                                              .Where(v => v.Id == id)
                                              .Select(v => new SoundSynthVideoDto
                                               {
                                                   Id             = v.Id,
                                                   SoundSynthId   = v.SoundSynthId,
                                                   SoundSynthName = v.SoundSynth.Name,
                                                   Provider       = v.Provider,
                                                   VideoId        = v.VideoId,
                                                   Title          = v.Title
                                               })
                                              .FirstOrDefaultAsync();

        if(dto is null) return NotFound();

        return dto;
    }

    [HttpPost("/sound-synths/{soundSynthId:int}/videos")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SoundSynthVideoDto>> CreateAsync(int                                   soundSynthId,
                                                                    [FromBody] CreateSoundSynthVideoRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.VideoId))
            return Problem(detail: "Provider and Video ID are required.", statusCode: StatusCodes.Status400BadRequest);

        string provider = request.Provider.Trim();
        string videoId  = request.VideoId.Trim();
        string title    = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();

        bool soundSynthExists = await context.SoundSynths.AnyAsync(s => s.Id == soundSynthId);

        if(!soundSynthExists) return NotFound();

        bool exists = await context.SoundSynthVideos.AnyAsync(v => v.SoundSynthId == soundSynthId &&
                                                                   v.Provider     == provider     &&
                                                                   v.VideoId      == videoId);

        if(exists) return Problem(detail: "This video is already linked to this sound synth.", statusCode: StatusCodes.Status409Conflict);

        var model = new SoundSynthVideo
        {
            SoundSynthId = soundSynthId,
            Provider     = provider,
            VideoId      = videoId,
            Title        = title
        };

        context.SoundSynthVideos.Add(model);
        await context.SaveChangesWithUserAsync(userId);

        string soundSynthName = await context.SoundSynths.Where(s => s.Id == soundSynthId)
                                             .Select(s => s.Name)
                                             .FirstOrDefaultAsync();

        var dto = new SoundSynthVideoDto
        {
            Id             = model.Id,
            SoundSynthId   = soundSynthId,
            SoundSynthName = soundSynthName,
            Provider       = provider,
            VideoId        = videoId,
            Title          = title
        };

        return CreatedAtAction(nameof(GetAsync), new { id = model.Id }, dto);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateSoundSynthVideoRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoundSynthVideo video = await context.SoundSynthVideos.FindAsync(id);

        if(video is null) return NotFound();

        video.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();

        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoundSynthVideo video = await context.SoundSynthVideos.FindAsync(id);

        if(video is null) return NotFound();

        context.SoundSynthVideos.Remove(video);
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }
}
