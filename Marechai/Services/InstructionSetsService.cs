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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class InstructionSetsService
    {
        readonly MarechaiContext _context;

        public InstructionSetsService(MarechaiContext context) => _context = context;

        public async Task<List<InstructionSet>> GetAsync() =>
            await _context.InstructionSets.OrderBy(e => e.Name).Select(e => new InstructionSet
            {
                Name = e.Name, Id = e.Id
            }).ToListAsync();

        public async Task<InstructionSet> GetAsync(int id) =>
            await _context.InstructionSets.Where(e => e.Id == id).Select(e => new InstructionSet
            {
                Name = e.Name, Id = e.Id
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(InstructionSet viewModel, string userId)
        {
            InstructionSet model = await _context.InstructionSets.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Name = viewModel.Name;

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<int> CreateAsync(InstructionSet viewModel, string userId)
        {
            var model = new InstructionSet
            {
                Name = viewModel.Name
            };

            await _context.InstructionSets.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(int id, string userId)
        {
            InstructionSet item = await _context.InstructionSets.FindAsync(id);

            if(item is null)
                return;

            _context.InstructionSets.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public bool VerifyUnique(string name) =>
            !_context.InstructionSets.Any(i => string.Equals(i.Name, name,
                                                             StringComparison.InvariantCultureIgnoreCase));
    }
}