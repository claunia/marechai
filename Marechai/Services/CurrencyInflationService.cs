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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class CurrencyInflationService
    {
        readonly MarechaiContext _context;

        public CurrencyInflationService(MarechaiContext context) => _context = context;

        public async Task<List<CurrencyInflationViewModel>> GetAsync() => await _context.
                                                                              CurrenciesInflation.
                                                                              OrderBy(i => i.Currency.Name).
                                                                              ThenBy(i => i.Year).
                                                                              Select(i => new CurrencyInflationViewModel
                                                                              {
                                                                                  Id           = i.Id,
                                                                                  CurrencyCode = i.Currency.Code,
                                                                                  CurrencyName = i.Currency.Name,
                                                                                  Year         = i.Year,
                                                                                  Inflation    = i.Inflation
                                                                              }).ToListAsync();

        public async Task<CurrencyInflationViewModel> GetAsync(int id) =>
            await _context.CurrenciesInflation.Where(b => b.Id == id).Select(i => new CurrencyInflationViewModel
            {
                Id           = i.Id,
                CurrencyCode = i.Currency.Code,
                CurrencyName = i.Currency.Name,
                Year         = i.Year,
                Inflation    = i.Inflation
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(CurrencyInflationViewModel viewModel, string userId)
        {
            CurrencyInflation model = await _context.CurrenciesInflation.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.CurrencyCode = viewModel.CurrencyCode;
            model.Year         = viewModel.Year;
            model.Inflation    = viewModel.Inflation;
            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<int> CreateAsync(CurrencyInflationViewModel viewModel, string userId)
        {
            var model = new CurrencyInflation
            {
                CurrencyCode = viewModel.CurrencyCode,
                Year         = viewModel.Year,
                Inflation    = viewModel.Inflation
            };

            await _context.CurrenciesInflation.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(int id, string userId)
        {
            CurrencyInflation item = await _context.CurrenciesInflation.FindAsync(id);

            if(item is null)
                return;

            _context.CurrenciesInflation.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}