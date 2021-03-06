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
    public class ResolutionsByGpuService
    {
        readonly MarechaiContext _context;

        public ResolutionsByGpuService(MarechaiContext context) => _context = context;

        public async Task<List<ResolutionByGpuViewModel>> GetByGpu(int resolutionId) =>
            (await _context.ResolutionsByGpu.Where(r => r.ResolutionId == resolutionId).
                            Select(r => new ResolutionByGpuViewModel
                            {
                                Id    = r.Id,
                                GpuId = r.GpuId,
                                Resolution = new ResolutionViewModel
                                {
                                    Id        = r.Resolution.Id,
                                    Width     = r.Resolution.Width,
                                    Height    = r.Resolution.Height,
                                    Colors    = r.Resolution.Colors,
                                    Palette   = r.Resolution.Palette,
                                    Chars     = r.Resolution.Chars,
                                    Grayscale = r.Resolution.Grayscale
                                },
                                ResolutionId = r.ResolutionId
                            }).ToListAsync()).OrderBy(r => r.Resolution.Width).ThenBy(r => r.Resolution.Height).
                                              ThenBy(r => r.Resolution.Chars).ThenBy(r => r.Resolution.Grayscale).
                                              ThenBy(r => r.Resolution.Colors).ThenBy(r => r.Resolution.Palette).
                                              ToList();

        public async Task DeleteAsync(long id, string userId)
        {
            ResolutionsByGpu item = await _context.ResolutionsByGpu.FindAsync(id);

            if(item is null)
                return;

            _context.ResolutionsByGpu.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(int resolutionId, int gpuId, string userId)
        {
            var item = new ResolutionsByGpu
            {
                GpuId        = gpuId,
                ResolutionId = resolutionId
            };

            await _context.ResolutionsByGpu.AddAsync(item);
            await _context.SaveChangesWithUserAsync(userId);

            return item.Id;
        }
    }
}