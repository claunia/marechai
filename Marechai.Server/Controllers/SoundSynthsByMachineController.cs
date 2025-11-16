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

[Route("/sound-synths-by-machine")]
[ApiController]
public class SoundSynthsByMachineController(MarechaiContext context) : ControllerBase
{
    [HttpGet("by-machine/{machineId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoundSynthByMachineDto>> GetByMachine(int machineId) => context.SoundByMachine
       .Where(g => g.MachineId == machineId)
       .Select(g => new SoundSynthByMachineDto
        {
            Id           = g.Id,
            Name         = g.SoundSynth.Name,
            CompanyName  = g.SoundSynth.Company.Name,
            SoundSynthId = g.SoundSynthId,
            MachineId    = g.MachineId
        })
       .OrderBy(g => g.CompanyName)
       .ThenBy(g => g.Name)
       .ToListAsync();

    [HttpGet("by-sound-synth/{soundSynthId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoundSynthByMachineDto>> GetBySoundSynth(int soundSynthId) => context.SoundByMachine
       .Where(g => g.SoundSynthId == soundSynthId)
       .Select(g => new SoundSynthByMachineDto
        {
            Id           = g.Id,
            Name         = g.Machine.Name,
            CompanyName  = g.Machine.Company.Name,
            SoundSynthId = g.SoundSynthId,
            MachineId    = g.MachineId
        })
       .OrderBy(g => g.CompanyName)
       .ThenBy(g => g.Name)
       .ToListAsync();

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
        SoundByMachine item = await context.SoundByMachine.FindAsync(id);

        if(item is null) return NotFound();

        context.SoundByMachine.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] SoundSynthByMachineDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var item = new SoundByMachine
        {
            SoundSynthId = dto.SoundSynthId,
            MachineId    = dto.MachineId
        };

        await context.SoundByMachine.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }
}