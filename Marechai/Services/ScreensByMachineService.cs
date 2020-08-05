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
    public class ScreensByMachineService
    {
        readonly MarechaiContext _context;

        public ScreensByMachineService(MarechaiContext context) => _context = context;

        public async Task<List<ScreenByMachineViewModel>> GetByMachine(int machineId) =>
            await _context.ScreensByMachine.Where(s => s.MachineId == machineId).
                           Select(s => new ScreenByMachineViewModel
                           {
                               Id        = s.Id,
                               ScreenId  = s.ScreenId,
                               MachineId = s.MachineId,
                               Screen = new ScreenViewModel
                               {
                                   Diagonal        = s.Screen.Diagonal,
                                   EffectiveColors = s.Screen.EffectiveColors,
                                   Height          = s.Screen.Height,
                                   Id              = s.Screen.Id,
                                   NativeResolution = new ResolutionViewModel
                                   {
                                       Chars     = s.Screen.NativeResolution.Chars,
                                       Colors    = s.Screen.NativeResolution.Colors,
                                       Grayscale = s.Screen.NativeResolution.Grayscale,
                                       Height    = s.Screen.NativeResolution.Height,
                                       Id        = s.Screen.NativeResolutionId,
                                       Palette   = s.Screen.NativeResolution.Palette,
                                       Width     = s.Screen.NativeResolution.Width
                                   },
                                   NativeResolutionId = s.Screen.NativeResolutionId,
                                   Type               = s.Screen.Type,
                                   Width              = s.Screen.Width
                               }
                           }).ToListAsync();

        public async Task DeleteAsync(long id, string userId)
        {
            ScreensByMachine item = await _context.ScreensByMachine.FindAsync(id);

            if(item is null)
                return;

            _context.ScreensByMachine.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(int machineId, int screenId, string userId)
        {
            if(_context.ScreensByMachine.Any(s => s.MachineId == machineId && s.ScreenId == screenId))
                return 0;

            var item = new ScreensByMachine
            {
                ScreenId  = screenId,
                MachineId = machineId
            };

            await _context.ScreensByMachine.AddAsync(item);
            await _context.SaveChangesWithUserAsync(userId);

            return item.Id;
        }
    }
}