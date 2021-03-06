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
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class StorageByMachineService
    {
        readonly MarechaiContext _context;

        public StorageByMachineService(MarechaiContext context) => _context = context;

        public async Task<List<StorageByMachineViewModel>> GetByMachine(int machineId) =>
            await _context.StorageByMachine.Where(s => s.MachineId == machineId).
                           Select(s => new StorageByMachineViewModel
                           {
                               Id        = s.Id,
                               Type      = s.Type,
                               Interface = s.Interface,
                               Capacity  = s.Capacity,
                               MachineId = s.MachineId
                           }).OrderBy(s => s.Type).ThenBy(s => s.Interface).ThenBy(s => s.Capacity).ToListAsync();

        public async Task DeleteAsync(long id, string userId)
        {
            StorageByMachine item = await _context.StorageByMachine.FindAsync(id);

            if(item is null)
                return;

            _context.StorageByMachine.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(int machineId, StorageType type, StorageInterface @interface,
                                            long? capacity, string userId)
        {
            var item = new StorageByMachine
            {
                MachineId = machineId,
                Type      = type,
                Interface = @interface,
                Capacity  = capacity
            };

            await _context.StorageByMachine.AddAsync(item);
            await _context.SaveChangesWithUserAsync(userId);

            return item.Id;
        }
    }
}