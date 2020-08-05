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
    public class GpusByMachineService
    {
        readonly MarechaiContext _context;

        public GpusByMachineService(MarechaiContext context) => _context = context;

        public async Task<List<GpuByMachineViewModel>> GetByMachine(int machineId) =>
            await _context.GpusByMachine.Where(g => g.MachineId == machineId).Select(g => new GpuByMachineViewModel
            {
                Id          = g.Id,
                Name        = g.Gpu.Name,
                CompanyName = g.Gpu.Company.Name,
                GpuId       = g.GpuId,
                MachineId   = g.MachineId
            }).OrderBy(g => g.CompanyName).ThenBy(g => g.Name).ToListAsync();

        public async Task DeleteAsync(long id, string userId)
        {
            GpusByMachine item = await _context.GpusByMachine.FindAsync(id);

            if(item is null)
                return;

            _context.GpusByMachine.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(int gpuId, int machineId, string userId)
        {
            var item = new GpusByMachine
            {
                GpuId     = gpuId,
                MachineId = machineId
            };

            await _context.GpusByMachine.AddAsync(item);
            await _context.SaveChangesWithUserAsync(userId);

            return item.Id;
        }
    }
}