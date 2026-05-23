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
using Microsoft.Extensions.Caching.Memory;

namespace Marechai.Server.Controllers;

[Route("/software/platforms")]
[ApiController]
public class SoftwarePlatformsController(MarechaiContext context, IMemoryCache cache) : ControllerBase
{
    // Software platforms barely change — cache the full list briefly so the
    // /software landing page doesn't re-hit the DB on every load.
    const           string   PLATFORMS_CACHE_KEY = "software:platforms:list";
    static readonly TimeSpan _platformsCacheTtl  = TimeSpan.FromMinutes(5);
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwarePlatformDto>> GetAsync()
    {
        if(cache.TryGetValue(PLATFORMS_CACHE_KEY, out List<SoftwarePlatformDto> cached) && cached is not null)
            return cached;

        List<SoftwarePlatformDto> platforms = await context.SoftwarePlatforms.OrderBy(p => p.Name)
                                                           .Select(p => new SoftwarePlatformDto
                                                            {
                                                                Id   = p.Id,
                                                                Name = p.Name
                                                            })
                                                           .ToListAsync();

        cache.Set(PLATFORMS_CACHE_KEY, platforms, _platformsCacheTtl);

        return platforms;
    }

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwarePlatformDto> GetAsync(ulong id) => context.SoftwarePlatforms.Where(p => p.Id == id)
                                                                  .Select(p => new SoftwarePlatformDto
                                                                   {
                                                                       Id   = p.Id,
                                                                       Name = p.Name
                                                                   })
                                                                  .FirstOrDefaultAsync();

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwarePlatformDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwarePlatform model = await context.SoftwarePlatforms.FindAsync(id);

        if(model is null) return NotFound();

        model.Name = dto.Name;
        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(PLATFORMS_CACHE_KEY);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwarePlatformDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwarePlatform
        {
            Name = dto.Name
        };

        await context.SoftwarePlatforms.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(PLATFORMS_CACHE_KEY);

        return model.Id;
    }

    [HttpDelete("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwarePlatform item = await context.SoftwarePlatforms.FindAsync(id);

        if(item is null) return NotFound();

        context.SoftwarePlatforms.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(PLATFORMS_CACHE_KEY);

        return Ok();
    }
}
