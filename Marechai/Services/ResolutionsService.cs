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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class ResolutionsService
    {
        readonly MarechaiContext _context;

        public ResolutionsService(MarechaiContext context) => _context = context;

        public async Task<List<ResolutionViewModel>> GetAsync() =>
            await _context.Resolutions.Select(r => new ResolutionViewModel
                           {
                               Id        = r.Id,
                               Width     = r.Width,
                               Height    = r.Height,
                               Colors    = r.Colors,
                               Palette   = r.Palette,
                               Chars     = r.Chars,
                               Grayscale = r.Grayscale
                           }).OrderBy(r => r.Width).ThenBy(r => r.Height).ThenBy(r => r.Chars).ThenBy(r => r.Grayscale).
                           ThenBy(r => r.Colors).ThenBy(r => r.Palette).ToListAsync();

        public async Task<ResolutionViewModel> GetAsync(int id) =>
            await _context.Resolutions.Where(r => r.Id == id).Select(r => new ResolutionViewModel
            {
                Id        = r.Id,
                Width     = r.Width,
                Height    = r.Height,
                Colors    = r.Colors,
                Palette   = r.Palette,
                Chars     = r.Chars,
                Grayscale = r.Grayscale
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(ResolutionViewModel viewModel, string userId)
        {
            Resolution model = await _context.Resolutions.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Chars     = viewModel.Chars;
            model.Colors    = viewModel.Colors;
            model.Grayscale = viewModel.Grayscale;
            model.Height    = viewModel.Height;
            model.Palette   = viewModel.Palette;
            model.Width     = viewModel.Width;

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<int> CreateAsync(ResolutionViewModel viewModel, string userId)
        {
            var model = new Resolution
            {
                Chars     = viewModel.Chars,
                Colors    = viewModel.Colors,
                Grayscale = viewModel.Grayscale,
                Height    = viewModel.Height,
                Palette   = viewModel.Palette,
                Width     = viewModel.Width
            };

            await _context.Resolutions.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(int id, string userId)
        {
            Resolution item = await _context.Resolutions.FindAsync(id);

            if(item is null)
                return;

            _context.Resolutions.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}