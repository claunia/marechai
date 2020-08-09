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
    public class MagazinesByMachineFamilyService
    {
        readonly MarechaiContext _context;

        public MagazinesByMachineFamilyService(MarechaiContext context) => _context = context;

        public async Task<List<MagazineByMachineFamilyViewModel>> GetByMagazine(long bookId) =>
            await _context.MagazinesByMachinesFamilies.Where(p => p.MagazineId == bookId).
                           Select(p => new MagazineByMachineFamilyViewModel
                           {
                               Id              = p.Id,
                               MagazineId      = p.MagazineId,
                               MachineFamilyId = p.MachineFamilyId,
                               MachineFamily   = p.MachineFamily.Name
                           }).OrderBy(p => p.MachineFamily).ThenBy(p => p.Magazine).ToListAsync();

        public async Task DeleteAsync(long id, string userId)
        {
            MagazinesByMachineFamily item = await _context.MagazinesByMachinesFamilies.FindAsync(id);

            if(item is null)
                return;

            _context.MagazinesByMachinesFamilies.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(int machineFamilyId, long bookId, string userId)
        {
            var item = new MagazinesByMachineFamily
            {
                MachineFamilyId = machineFamilyId,
                MagazineId      = bookId
            };

            await _context.MagazinesByMachinesFamilies.AddAsync(item);
            await _context.SaveChangesWithUserAsync(userId);

            return item.Id;
        }
    }
}