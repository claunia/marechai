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
    public class ScreensService
    {
        readonly MarechaiContext _context;

        public ScreensService(MarechaiContext context) => _context = context;

        public async Task<List<ScreenViewModel>> GetAsync() => (await _context.Screens.Select(s => new ScreenViewModel
                                                                   {
                                                                       Diagonal = s.Diagonal,
                                                                       EffectiveColors = s.EffectiveColors,
                                                                       Height = s.Height, Id = s.Id, Type = s.Type,
                                                                       Width = s.Width,
                                                                       NativeResolutionId = s.NativeResolutionId,
                                                                       NativeResolution = new ResolutionViewModel
                                                                       {
                                                                           Chars     = s.NativeResolution.Chars,
                                                                           Colors    = s.NativeResolution.Colors,
                                                                           Grayscale = s.NativeResolution.Grayscale,
                                                                           Height    = s.NativeResolution.Height,
                                                                           Id        = s.NativeResolution.Id,
                                                                           Palette   = s.NativeResolution.Palette,
                                                                           Width     = s.NativeResolution.Width
                                                                       }
                                                                   }).ToListAsync()).OrderBy(s => s.Diagonal).
                                                                                     ThenBy(s => s.EffectiveColors).
                                                                                     ThenBy(s => s.NativeResolution.
                                                                                                   ToString()).
                                                                                     ThenBy(s => s.Type).
                                                                                     ThenBy(s => s.Size).ToList();

        public async Task<ScreenViewModel> GetAsync(int id) =>
            await _context.Screens.Where(s => s.Id == id).Select(s => new ScreenViewModel
            {
                Diagonal = s.Diagonal, EffectiveColors = s.EffectiveColors, Height = s.Height, Id = s.Id,
                NativeResolution = new ResolutionViewModel
                {
                    Chars = s.NativeResolution.Chars, Colors = s.NativeResolution.Colors,
                    Grayscale = s.NativeResolution.Grayscale, Height = s.NativeResolution.Height,
                    Id = s.NativeResolution.Id, Palette = s.NativeResolution.Palette, Width = s.NativeResolution.Width
                },
                NativeResolutionId = s.NativeResolutionId, Type = s.Type, Width = s.Width
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(ScreenViewModel viewModel)
        {
            Screen model = await _context.Screens.FindAsync(viewModel.Id);

            if(model is null)
                return;

            Resolution nativeResolution = await _context.Resolutions.FindAsync(viewModel.NativeResolutionId);

            if(nativeResolution is null)
                return;

            model.Diagonal           = viewModel.Diagonal;
            model.EffectiveColors    = viewModel.EffectiveColors;
            model.Height             = viewModel.Height;
            model.NativeResolutionId = viewModel.NativeResolutionId;
            model.Type               = viewModel.Type;
            model.Width              = viewModel.Width;

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(ScreenViewModel viewModel)
        {
            var model = new Screen
            {
                Diagonal = viewModel.Diagonal, EffectiveColors = viewModel.EffectiveColors, Height = viewModel.Height,
                NativeResolutionId = viewModel.NativeResolutionId, Type = viewModel.Type, Width = viewModel.Width
            };

            await _context.Screens.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.Id;
        }

        public async Task DeleteAsync(int id)
        {
            Screen item = await _context.Screens.FindAsync(id);

            if(item is null)
                return;

            _context.Screens.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}