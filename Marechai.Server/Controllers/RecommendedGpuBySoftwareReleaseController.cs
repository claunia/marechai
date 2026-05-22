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

[Route("/software/releases/recommended-gpus")]
[ApiController]
public class RecommendedGpuBySoftwareReleaseController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/software/releases/{releaseId:ulong}/recommended-gpus")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<GpuBySoftwareReleaseDto>> GetByReleaseAsync(ulong releaseId) =>
        context.RecommendedGpuBySoftwareRelease
               .AsNoTracking()
               .Where(p => p.ReleaseId == releaseId)
               .Select(p => new GpuBySoftwareReleaseDto
                {
                    ReleaseId = p.ReleaseId,
                    GpuId     = p.GpuId,
                    Gpu       = p.Gpu.Name
                })
               .OrderBy(p => p.Gpu)
               .ToListAsync();

    [HttpDelete("{releaseId:ulong}/{gpuId:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong releaseId, int gpuId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        RecommendedGpuBySoftwareRelease item =
            await context.RecommendedGpuBySoftwareRelease.FindAsync(releaseId, gpuId);

        if(item is null) return NotFound();

        context.RecommendedGpuBySoftwareRelease.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateAsync([FromBody] GpuBySoftwareReleaseDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var item = new RecommendedGpuBySoftwareRelease
        {
            ReleaseId = dto.ReleaseId,
            GpuId     = dto.GpuId
        };

        await context.RecommendedGpuBySoftwareRelease.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
