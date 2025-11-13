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

using System;
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
using Microsoft.Extensions.Localization;

namespace Marechai.Server.Controllers;

[Route("/resolutions-by-gpu")]
[ApiController]
public class ResolutionsByGpuController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<ResolutionByGpuDto>> GetByGpu(int resolutionId) => (await context.ResolutionsByGpu
                                                                                                  .Where(r => r.ResolutionId == resolutionId)
                                                                                                  .Select(r => new ResolutionByGpuDto
                                                                                                   {
                                                                                                       Id    = r.Id,
                                                                                                       GpuId = r.GpuId,
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

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task DeleteAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        ResolutionsByGpu item = await context.ResolutionsByGpu.FindAsync(id);

        if(item is null) return;

        context.ResolutionsByGpu.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<long> CreateAsync(int resolutionId, int gpuId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return 0;
        var item = new ResolutionsByGpu
        {
            GpuId        = gpuId,
            ResolutionId = resolutionId
        };

        await context.ResolutionsByGpu.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }
}
