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
    public class CurrencyPeggingService
    {
        readonly MarechaiContext _context;

        public CurrencyPeggingService(MarechaiContext context) => _context = context;

        public async Task<List<CurrencyPeggingViewModel>> GetAsync() => await _context.
                                                                              CurrenciesPegging.
                                                                              OrderBy(i => i.Source.Name).
                                                                              ThenBy(i => i.Destination.Name).
                                                                              ThenBy(i => i.Start).ThenBy(i => i.End).
                                                                              Select(i => new CurrencyPeggingViewModel
                                                                              {
                                                                                  Id              = i.Id,
                                                                                  SourceCode      = i.Source.Code,
                                                                                  SourceName      = i.Source.Name,
                                                                                  DestinationCode = i.Source.Code,
                                                                                  DestinationName = i.Source.Name,
                                                                                  Ratio           = i.Ratio,
                                                                                  Start           = i.Start,
                                                                                  End             = i.End
                                                                              }).ToListAsync();

        public async Task<CurrencyPeggingViewModel> GetAsync(int id) =>
            await _context.CurrenciesPegging.Where(b => b.Id == id).Select(i => new CurrencyPeggingViewModel
            {
                Id              = i.Id,
                SourceCode      = i.Source.Code,
                SourceName      = i.Source.Name,
                DestinationCode = i.Destination.Code,
                DestinationName = i.Destination.Name,
                Ratio           = i.Ratio,
                Start           = i.Start,
                End             = i.End
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(CurrencyPeggingViewModel viewModel, string userId)
        {
            CurrencyPegging model = await _context.CurrenciesPegging.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.SourceCode      = viewModel.SourceCode;
            model.DestinationCode = viewModel.DestinationCode;
            model.Ratio           = viewModel.Ratio;
            model.Start           = viewModel.Start;
            model.End             = viewModel.End;
            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<int> CreateAsync(CurrencyPeggingViewModel viewModel, string userId)
        {
            var model = new CurrencyPegging
            {
                SourceCode      = viewModel.SourceCode,
                DestinationCode = viewModel.DestinationCode,
                Ratio           = viewModel.Ratio,
                Start           = viewModel.Start,
                End             = viewModel.End
            };

            await _context.CurrenciesPegging.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(int id, string userId)
        {
            CurrencyPegging item = await _context.CurrenciesPegging.FindAsync(id);

            if(item is null)
                return;

            _context.CurrenciesPegging.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}