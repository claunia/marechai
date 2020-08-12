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
    public class DocumentsByMachineFamilyService
    {
        readonly MarechaiContext _context;

        public DocumentsByMachineFamilyService(MarechaiContext context) => _context = context;

        public async Task<List<DocumentByMachineFamilyViewModel>> GetByDocument(long bookId) =>
            await _context.DocumentsByMachineFamilies.Where(p => p.DocumentId == bookId).
                           Select(p => new DocumentByMachineFamilyViewModel
                           {
                               Id              = p.Id,
                               DocumentId      = p.DocumentId,
                               MachineFamilyId = p.MachineFamilyId,
                               MachineFamily   = p.MachineFamily.Name
                           }).OrderBy(p => p.MachineFamily).ToListAsync();

        public async Task DeleteAsync(long id, string userId)
        {
            DocumentsByMachineFamily item = await _context.DocumentsByMachineFamilies.FindAsync(id);

            if(item is null)
                return;

            _context.DocumentsByMachineFamilies.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(int machineFamilyId, long bookId, string userId)
        {
            var item = new DocumentsByMachineFamily
            {
                MachineFamilyId = machineFamilyId,
                DocumentId      = bookId
            };

            await _context.DocumentsByMachineFamilies.AddAsync(item);
            await _context.SaveChangesWithUserAsync(userId);

            return item.Id;
        }
    }
}