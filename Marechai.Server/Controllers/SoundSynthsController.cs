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

[Route("/sound-synths")]
[ApiController]
public class SoundSynthsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoundSynthDto>> GetAsync() => context.SoundSynths.OrderBy(s => s.Company.Name)
                                                          .ThenBy(s => s.Name)
                                                          .ThenBy(s => s.ModelCode)
                                                          .Select(s => new SoundSynthDto
                                                           {
                                                               Id          = s.Id,
                                                               Name        = s.Name,
                                                               CompanyId   = s.Company.Id,
                                                               CompanyName = s.Company.Name,
                                                               ModelCode   = s.ModelCode,
                                                               Introduced  = s.Introduced,
                                                               Voices      = s.Voices,
                                                               Frequency   = s.Frequency,
                                                               Depth       = s.Depth,
                                                               SquareWave  = s.SquareWave,
                                                               WhiteNoise  = s.WhiteNoise,
                                                               Type        = s.Type
                                                           })
                                                          .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoundSynthDto>> GetByMachineAsync(int machineId) => context.SoundByMachine
       .Where(s => s.MachineId == machineId)
       .Select(s => s.SoundSynth)
       .OrderBy(s => s.Company.Name)
       .ThenBy(s => s.Name)
       .ThenBy(s => s.ModelCode)
       .Select(s => new SoundSynthDto
        {
            Id          = s.Id,
            Name        = s.Name,
            CompanyId   = s.Company.Id,
            CompanyName = s.Company.Name,
            ModelCode   = s.ModelCode,
            Introduced  = s.Introduced,
            Voices      = s.Voices,
            Frequency   = s.Frequency,
            Depth       = s.Depth,
            SquareWave  = s.SquareWave,
            WhiteNoise  = s.WhiteNoise,
            Type        = s.Type
        })
       .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoundSynthDto> GetAsync(int id) => context.SoundSynths.Where(s => s.Id == id)
                                                          .Select(s => new SoundSynthDto
                                                           {
                                                               Id          = s.Id,
                                                               Name        = s.Name,
                                                               CompanyId   = s.Company.Id,
                                                               CompanyName = s.Company.Name,
                                                               ModelCode   = s.ModelCode,
                                                               Introduced  = s.Introduced,
                                                               Voices      = s.Voices,
                                                               Frequency   = s.Frequency,
                                                               Depth       = s.Depth,
                                                               SquareWave  = s.SquareWave,
                                                               WhiteNoise  = s.WhiteNoise,
                                                               Type        = s.Type
                                                           })
                                                          .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync([FromBody] SoundSynthDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoundSynth model = await context.SoundSynths.FindAsync(dto.Id);

        if(model is null) return NotFound();

        model.Depth      = dto.Depth;
        model.Frequency  = dto.Frequency;
        model.Introduced = dto.Introduced;
        model.Name       = dto.Name;
        model.Type       = dto.Type;
        model.Voices     = dto.Voices;
        model.CompanyId  = dto.CompanyId;
        model.ModelCode  = dto.ModelCode;
        model.SquareWave = dto.SquareWave;
        model.WhiteNoise = dto.WhiteNoise;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] SoundSynthDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoundSynth
        {
            Depth      = dto.Depth,
            Frequency  = dto.Frequency,
            Introduced = dto.Introduced,
            Name       = dto.Name,
            Type       = dto.Type,
            Voices     = dto.Voices,
            CompanyId  = dto.CompanyId,
            ModelCode  = dto.ModelCode,
            SquareWave = dto.SquareWave,
            WhiteNoise = dto.WhiteNoise
        };

        await context.SoundSynths.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoundSynth item = await context.SoundSynths.FindAsync(id);

        if(item is null) return NotFound();

        context.SoundSynths.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}