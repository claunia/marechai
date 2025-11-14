/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
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
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services;

public class CurrencyInflationService(MarechaiContext context)
{
    public async Task<List<CurrencyInflationDto>> GetAsync() => await context.CurrenciesInflation
                                                                                   .OrderBy(i => i.Currency.Name)
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

    public async Task<CurrencyInflationDto> GetAsync(int id) => await context.CurrenciesInflation
                                                                         .Where(b => b.Id == id)
                                                                         .Select(i => new CurrencyInflationDto
                                                                          {
                                                                              Id           = i.Id,
                                                                              CurrencyCode = i.Currency.Code,
                                                                              CurrencyName = i.Currency.Name,
                                                                              Year         = i.Year,
                                                                              Inflation    = i.Inflation
                                                                          })
                                                                         .FirstOrDefaultAsync();

    public async Task UpdateAsync(CurrencyInflationDto dto, string userId)
    {
        CurrencyInflation model = await context.CurrenciesInflation.FindAsync(dto.Id);

        if(model is null) return;

        model.CurrencyCode = dto.CurrencyCode;
        model.Year         = dto.Year;
        model.Inflation    = dto.Inflation;
        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<int> CreateAsync(CurrencyInflationDto dto, string userId)
    {
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

    public async Task DeleteAsync(int id, string userId)
    {
        CurrencyInflation item = await context.CurrenciesInflation.FindAsync(id);

        if(item is null) return;

        context.CurrenciesInflation.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}