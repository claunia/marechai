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

[Route("/currency-pegging")]
[ApiController]
public class CurrencyPeggingController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<CurrencyPeggingDto>> GetAsync() => await context.CurrenciesPegging
                                                                                 .OrderBy(i => i.Source.Name)
                                                                                 .ThenBy(i => i.Destination.Name)
                                                                                 .ThenBy(i => i.Start)
                                                                                 .ThenBy(i => i.End)
                                                                                 .Select(i => new CurrencyPeggingDto
                                                                                  {
                                                                                      Id              = i.Id,
                                                                                      SourceCode      = i.Source.Code,
                                                                                      SourceName      = i.Source.Name,
                                                                                      DestinationCode = i.Source.Code,
                                                                                      DestinationName = i.Source.Name,
                                                                                      Ratio           = i.Ratio,
                                                                                      Start           = i.Start,
                                                                                      End             = i.End
                                                                                  })
                                                                                 .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<CurrencyPeggingDto> GetAsync(int id) => await context.CurrenciesPegging
                                                                       .Where(b => b.Id == id)
                                                                       .Select(i => new CurrencyPeggingDto
                                                                        {
                                                                            Id              = i.Id,
                                                                            SourceCode      = i.Source.Code,
                                                                            SourceName      = i.Source.Name,
                                                                            DestinationCode = i.Destination.Code,
                                                                            DestinationName = i.Destination.Name,
                                                                            Ratio           = i.Ratio,
                                                                            Start           = i.Start,
                                                                            End             = i.End
                                                                        })
                                                                       .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task UpdateAsync(CurrencyPeggingDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        CurrencyPegging model = await context.CurrenciesPegging.FindAsync(dto.Id);

        if(model is null) return;

        model.SourceCode      = dto.SourceCode;
        model.DestinationCode = dto.DestinationCode;
        model.Ratio           = dto.Ratio;
        model.Start           = dto.Start;
        model.End             = dto.End;
        await context.SaveChangesWithUserAsync(userId);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> CreateAsync(CurrencyPeggingDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return 0;
        var model = new CurrencyPegging
        {
            SourceCode      = dto.SourceCode,
            DestinationCode = dto.DestinationCode,
            Ratio           = dto.Ratio,
            Start           = dto.Start,
            End             = dto.End
        };

        await context.CurrenciesPegging.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        CurrencyPegging item = await context.CurrenciesPegging.FindAsync(id);

        if(item is null) return;

        context.CurrenciesPegging.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}
