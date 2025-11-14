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

[Route("/currencies/pegging")]
[ApiController]
public class CurrencyPeggingController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CurrencyPeggingDto>> GetAsync() => context.CurrenciesPegging.OrderBy(i => i.Source.Name)
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

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<CurrencyPeggingDto> GetAsync(int id) => context.CurrenciesPegging.Where(b => b.Id == id)
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

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] CurrencyPeggingDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        CurrencyPegging model = await context.CurrenciesPegging.FindAsync(id);

        if(model is null) return NotFound();

        model.SourceCode      = dto.SourceCode;
        model.DestinationCode = dto.DestinationCode;
        model.Ratio           = dto.Ratio;
        model.Start           = dto.Start;
        model.End             = dto.End;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> CreateAsync([FromBody] CurrencyPeggingDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

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

        CurrencyPegging item = await context.CurrenciesPegging.FindAsync(id);

        if(item is null) return NotFound();

        context.CurrenciesPegging.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}