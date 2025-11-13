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

[Route("/currency-inflation")]
[ApiController]
public class CurrencyInflationController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CurrencyInflationDto>> GetAsync() => context.CurrenciesInflation.OrderBy(i => i.Currency.Name)
                                                                 .ThenBy(i => i.Year)
                                                                 .Select(i => new CurrencyInflationDto
                                                                  {
                                                                      Id           = i.Id,
                                                                      CurrencyCode = i.Currency.Code,
                                                                      CurrencyName = i.Currency.Name,
                                                                      Year         = i.Year,
                                                                      Inflation    = i.Inflation
                                                                  })
                                                                 .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<CurrencyInflationDto> GetAsync(int id) => context.CurrenciesInflation.Where(b => b.Id == id)
                                                                 .Select(i => new CurrencyInflationDto
                                                                  {
                                                                      Id           = i.Id,
                                                                      CurrencyCode = i.Currency.Code,
                                                                      CurrencyName = i.Currency.Name,
                                                                      Year         = i.Year,
                                                                      Inflation    = i.Inflation
                                                                  })
                                                                 .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task UpdateAsync(CurrencyInflationDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return;
        CurrencyInflation model = await context.CurrenciesInflation.FindAsync(dto.Id);

        if(model is null) return;

        model.CurrencyCode = dto.CurrencyCode;
        model.Year         = dto.Year;
        model.Inflation    = dto.Inflation;
        await context.SaveChangesWithUserAsync(userId);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> CreateAsync(CurrencyInflationDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return 0;

        var model = new CurrencyInflation
        {
            CurrencyCode = dto.CurrencyCode,
            Year         = dto.Year,
            Inflation    = dto.Inflation
        };

        await context.CurrenciesInflation.AddAsync(model);
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
        CurrencyInflation item = await context.CurrenciesInflation.FindAsync(id);

        if(item is null) return;

        context.CurrenciesInflation.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}