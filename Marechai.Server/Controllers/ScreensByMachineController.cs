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

[Route("/screens-by-machine")]
[ApiController]
public class ScreensByMachineController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<ScreenByMachineDto>> GetByMachine(int machineId) => context.ScreensByMachine
       .Where(s => s.MachineId == machineId)
       .Select(s => new ScreenByMachineDto
        {
            Id        = s.Id,
            ScreenId  = s.ScreenId,
            MachineId = s.MachineId,
            Screen = new ScreenDto
            {
                Diagonal        = s.Screen.Diagonal,
                EffectiveColors = s.Screen.EffectiveColors,
                Height          = s.Screen.Height,
                Id              = s.Screen.Id,
                NativeResolution = new ResolutionDto
                {
                    Chars     = s.Screen.NativeResolution.Chars,
                    Colors    = s.Screen.NativeResolution.Colors,
                    Grayscale = s.Screen.NativeResolution.Grayscale,
                    Height    = s.Screen.NativeResolution.Height,
                    Id        = s.Screen.NativeResolutionId,
                    Palette   = s.Screen.NativeResolution.Palette,
                    Width     = s.Screen.NativeResolution.Width
                },
                NativeResolutionId = s.Screen.NativeResolutionId,
                Type               = s.Screen.Type,
                Width              = s.Screen.Width
            }
        })
       .ToListAsync();

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task DeleteAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return;
        ScreensByMachine item = await context.ScreensByMachine.FindAsync(id);

        if(item is null) return;

        context.ScreensByMachine.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<long> CreateAsync(int machineId, int screenId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return 0;
        if(context.ScreensByMachine.Any(s => s.MachineId == machineId && s.ScreenId == screenId)) return 0;

        var item = new ScreensByMachine
        {
            ScreenId  = screenId,
            MachineId = machineId
        };

        await context.ScreensByMachine.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }
}