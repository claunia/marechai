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

[Route("/processors/videos")]
[ApiController]
public class ProcessorVideosController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/processors/{processorId:int}/videos")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<ProcessorVideoDto>> GetVideosByProcessorAsync(int processorId) => context.ProcessorVideos
       .Where(v => v.ProcessorId == processorId)
       .OrderBy(v => v.Title)
       .Select(v => new ProcessorVideoDto
        {
            Id            = v.Id,
            ProcessorId   = v.ProcessorId,
            ProcessorName = v.Processor.Name,
            Provider      = v.Provider,
            VideoId       = v.VideoId,
            Title         = v.Title
        })
       .ToListAsync();

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProcessorVideoDto>> GetAsync(long id)
    {
        ProcessorVideoDto dto = await context.ProcessorVideos
                                             .Where(v => v.Id == id)
                                             .Select(v => new ProcessorVideoDto
                                              {
                                                  Id            = v.Id,
                                                  ProcessorId   = v.ProcessorId,
                                                  ProcessorName = v.Processor.Name,
                                                  Provider      = v.Provider,
                                                  VideoId       = v.VideoId,
                                                  Title         = v.Title
                                              })
                                             .FirstOrDefaultAsync();

        if(dto is null) return NotFound();

        return dto;
    }

    [HttpPost("/processors/{processorId:int}/videos")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProcessorVideoDto>> CreateAsync(int                                processorId,
                                                                   [FromBody] CreateProcessorVideoRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.VideoId))
            return BadRequest("Provider and Video ID are required.");

        string provider = request.Provider.Trim();
        string videoId  = request.VideoId.Trim();
        string title    = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();

        bool processorExists = await context.Processors.AnyAsync(p => p.Id == processorId);

        if(!processorExists) return NotFound();

        bool exists = await context.ProcessorVideos.AnyAsync(v => v.ProcessorId == processorId &&
                                                                  v.Provider    == provider    &&
                                                                  v.VideoId     == videoId);

        if(exists) return Conflict("This video is already linked to this processor.");

        var model = new ProcessorVideo
        {
            ProcessorId = processorId,
            Provider    = provider,
            VideoId     = videoId,
            Title       = title
        };

        context.ProcessorVideos.Add(model);
        await context.SaveChangesAsync();

        string processorName = await context.Processors.Where(p => p.Id == processorId)
                                            .Select(p => p.Name)
                                            .FirstOrDefaultAsync();

        var dto = new ProcessorVideoDto
        {
            Id            = model.Id,
            ProcessorId   = processorId,
            ProcessorName = processorName,
            Provider      = provider,
            VideoId       = videoId,
            Title         = title
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
    public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateProcessorVideoRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        ProcessorVideo video = await context.ProcessorVideos.FindAsync(id);

        if(video is null) return NotFound();

        video.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();

        await context.SaveChangesAsync();

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

        ProcessorVideo video = await context.ProcessorVideos.FindAsync(id);

        if(video is null) return NotFound();

        context.ProcessorVideos.Remove(video);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
