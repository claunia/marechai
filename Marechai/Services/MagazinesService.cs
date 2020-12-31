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
    public class MagazinesService
    {
        readonly MarechaiContext _context;

        public MagazinesService(MarechaiContext context) => _context = context;

        public async Task<List<MagazineViewModel>> GetAsync() => await _context.
                                                                       Magazines.OrderBy(b => b.NativeTitle).
                                                                       ThenBy(b => b.FirstPublication).
                                                                       ThenBy(b => b.Title).
                                                                       Select(b => new MagazineViewModel
                                                                       {
                                                                           Id               = b.Id,
                                                                           Title            = b.Title,
                                                                           NativeTitle      = b.NativeTitle,
                                                                           FirstPublication = b.FirstPublication,
                                                                           Synopsis         = b.Synopsis,
                                                                           Issn             = b.Issn,
                                                                           CountryId        = b.CountryId,
                                                                           Country          = b.Country.Name
                                                                       }).ToListAsync();

        public async Task<List<MagazineViewModel>> GetTitlesAsync() => await _context.
                                                                             Magazines.OrderBy(b => b.Title).
                                                                             ThenBy(b => b.FirstPublication).
                                                                             Select(b => new MagazineViewModel
                                                                             {
                                                                                 Id    = b.Id,
                                                                                 Title = $"{b.Title} ({b.Country.Name}"
                                                                             }).ToListAsync();

        public async Task<MagazineViewModel> GetAsync(long id) => await _context.Magazines.Where(b => b.Id == id).
                                                                      Select(b => new MagazineViewModel
                                                                      {
                                                                          Id               = b.Id,
                                                                          Title            = b.Title,
                                                                          NativeTitle      = b.NativeTitle,
                                                                          FirstPublication = b.FirstPublication,
                                                                          Synopsis         = b.Synopsis,
                                                                          Issn             = b.Issn,
                                                                          CountryId        = b.CountryId,
                                                                          Country          = b.Country.Name
                                                                      }).FirstOrDefaultAsync();

        public async Task UpdateAsync(MagazineViewModel viewModel, string userId)
        {
            Magazine model = await _context.Magazines.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Title            = viewModel.Title;
            model.NativeTitle      = viewModel.NativeTitle;
            model.FirstPublication = viewModel.FirstPublication;
            model.Synopsis         = viewModel.Synopsis;
            model.CountryId        = viewModel.CountryId;
            model.Issn             = viewModel.Issn;
            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(MagazineViewModel viewModel, string userId)
        {
            var model = new Magazine
            {
                Title            = viewModel.Title,
                NativeTitle      = viewModel.NativeTitle,
                FirstPublication = viewModel.FirstPublication,
                Synopsis         = viewModel.Synopsis,
                CountryId        = viewModel.CountryId,
                Issn             = viewModel.Issn
            };

            await _context.Magazines.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task<string> GetSynopsisTextAsync(int id) =>
            (await _context.Magazines.FirstOrDefaultAsync(d => d.Id == id))?.Synopsis;

        public async Task DeleteAsync(long id, string userId)
        {
            Magazine item = await _context.Magazines.FindAsync(id);

            if(item is null)
                return;

            _context.Magazines.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}