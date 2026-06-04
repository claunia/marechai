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

[Route("/gpus/videos")]
[ApiController]
public class GpuVideosController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/gpus/{gpuId:int}/videos")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<GpuVideoDto>> GetVideosByGpuAsync(int gpuId) => context.GpuVideos.AsNoTracking()
       .Where(v => v.GpuId == gpuId)
       .OrderBy(v => v.Title)
       .Select(v => new GpuVideoDto
        {
            Id       = v.Id,
            GpuId    = v.GpuId,
            GpuName  = v.Gpu.Name,
            Provider = v.Provider,
            VideoId  = v.VideoId,
            Title    = v.Title
        })
       .ToListAsync();

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GpuVideoDto>> GetAsync(long id)
    {
        GpuVideoDto dto = await context.GpuVideos
                                       .Where(v => v.Id == id)
                                       .Select(v => new GpuVideoDto
                                        {
                                            Id       = v.Id,
                                            GpuId    = v.GpuId,
                                            GpuName  = v.Gpu.Name,
                                            Provider = v.Provider,
                                            VideoId  = v.VideoId,
                                            Title    = v.Title
                                        })
                                       .FirstOrDefaultAsync();

        if(dto is null) return NotFound();

        return dto;
    }

    [HttpPost("/gpus/{gpuId:int}/videos")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GpuVideoDto>> CreateAsync(int gpuId, [FromBody] CreateGpuVideoRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.VideoId))
            return Problem(detail: "Provider and Video ID are required.", statusCode: StatusCodes.Status400BadRequest);

        string provider = request.Provider.Trim();
        string videoId  = request.VideoId.Trim();
        string title    = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();

        bool gpuExists = await context.Gpus.AnyAsync(g => g.Id == gpuId);

        if(!gpuExists) return NotFound();

        bool exists = await context.GpuVideos.AnyAsync(v => v.GpuId    == gpuId    &&
                                                            v.Provider == provider &&
                                                            v.VideoId  == videoId);

        if(exists) return Problem(detail: "This video is already linked to this GPU.", statusCode: StatusCodes.Status409Conflict);

        var model = new GpuVideo
        {
            GpuId    = gpuId,
            Provider = provider,
            VideoId  = videoId,
            Title    = title
        };

        context.GpuVideos.Add(model);
        await context.SaveChangesAsync();

        string gpuName = await context.Gpus.Where(g => g.Id == gpuId)
                                      .Select(g => g.Name)
                                      .FirstOrDefaultAsync();

        var dto = new GpuVideoDto
        {
            Id       = model.Id,
            GpuId    = gpuId,
            GpuName  = gpuName,
            Provider = provider,
            VideoId  = videoId,
            Title    = title
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
    public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateGpuVideoRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        GpuVideo video = await context.GpuVideos.FindAsync(id);

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

        GpuVideo video = await context.GpuVideos.FindAsync(id);

        if(video is null) return NotFound();

        context.GpuVideos.Remove(video);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
