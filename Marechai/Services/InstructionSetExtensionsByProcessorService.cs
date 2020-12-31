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
    public class InstructionSetExtensionsByProcessorService
    {
        readonly MarechaiContext _context;

        public InstructionSetExtensionsByProcessorService(MarechaiContext context) => _context = context;

        public async Task<List<InstructionSetExtensionByProcessorViewModel>> GetByProcessor(int processorId) =>
            await _context.InstructionSetExtensionsByProcessor.Where(e => e.ProcessorId == processorId).
                           Select(e => new InstructionSetExtensionByProcessorViewModel
                           {
                               Id          = e.Id,
                               Extension   = e.Extension.Extension,
                               Processor   = e.Processor.Name,
                               ProcessorId = e.ProcessorId,
                               ExtensionId = e.ExtensionId
                           }).OrderBy(e => e.Extension).ToListAsync();

        public async Task DeleteAsync(int id, string userId)
        {
            InstructionSetExtensionsByProcessor item = await _context.InstructionSetExtensionsByProcessor.FindAsync(id);

            if(item is null)
                return;

            _context.InstructionSetExtensionsByProcessor.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<int> CreateAsync(int processorId, int extensionId, string userId)
        {
            var item = new InstructionSetExtensionsByProcessor
            {
                ProcessorId = processorId,
                ExtensionId = extensionId
            };

            await _context.InstructionSetExtensionsByProcessor.AddAsync(item);
            await _context.SaveChangesWithUserAsync(userId);

            return item.Id;
        }
    }
}