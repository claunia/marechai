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

[Route("/machine-families")]
[ApiController]
public class MachineFamiliesController(MarechaiContext context, IMemoryCache cache) : ControllerBase
{
    // Cache key + TTL for the all-families list. Used by every machine-family
    // dropdown across admin + public pages. Slow-changing reference data, so a
    // 5-minute TTL is safe; we invalidate on Create/Update/Delete below.
    const           string   MACHINE_FAMILIES_ALL_KEY = "machine-families:all";
    static readonly TimeSpan _catalogCacheTtl         = TimeSpan.FromMinutes(5);

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<MachineFamilyDto>> GetAsync()
    {
        if(cache.TryGetValue(MACHINE_FAMILIES_ALL_KEY, out List<MachineFamilyDto> cached) && cached is not null)
            return cached;

        List<MachineFamilyDto> families = await context.MachineFamilies.AsNoTracking()
                                                       .Select(m => new MachineFamilyDto
                                                        {
                                                            Id      = m.Id,
                                                            Company = m.Company.Name,
                                                            Name    = m.Name
                                                        })
                                                       .OrderBy(m => m.Name)
                                                       .ToListAsync();

        cache.Set(MACHINE_FAMILIES_ALL_KEY, families, _catalogCacheTtl);

        return families;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<MachineFamilyDto> GetAsync(int id) => context.MachineFamilies.AsNoTracking()
                                                             .Where(f => f.Id == id)
                                                             .Select(m => new MachineFamilyDto
                                                              {
                                                                  Id        = m.Id,
                                                                  CompanyId = m.CompanyId,
                                                                  Company   = m.Company.Name,
                                                                  Name      = m.Name
                                                              })
                                                             .FirstOrDefaultAsync();

    [HttpGet("{id:int}/machines")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetMachinesAsync(int id) => context.Machines.AsNoTracking()
       .Where(m => m.FamilyId == id)
       .OrderBy(m => m.Name)
       .Select(m => new MachineDto
        {
            Id   = m.Id,
            Name = m.Name,
            Type = m.Type
        })
       .ToListAsync();

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] MachineFamilyDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        MachineFamily model = await context.MachineFamilies.FindAsync(id);

        if(model is null) return NotFound();

        model.Name      = dto.Name;
        model.CompanyId = dto.CompanyId;

        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(MACHINE_FAMILIES_ALL_KEY);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] MachineFamilyDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new MachineFamily
        {
            Name      = dto.Name,
            CompanyId = dto.CompanyId
        };

        await context.MachineFamilies.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(MACHINE_FAMILIES_ALL_KEY);

        return model.Id;
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        MachineFamily item = await context.MachineFamilies.FindAsync(id);

        if(item is null) return NotFound();

        context.MachineFamilies.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(MACHINE_FAMILIES_ALL_KEY);

        return Ok();
    }
}