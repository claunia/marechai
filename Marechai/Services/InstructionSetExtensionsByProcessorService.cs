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
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services;

public class InstructionSetExtensionsByProcessorService(MarechaiContext context)
{
    public async Task<List<InstructionSetExtensionByProcessorDto>> GetByProcessor(int processorId) =>
        await context.InstructionSetExtensionsByProcessor.Where(e => e.ProcessorId == processorId)
                      .Select(e => new InstructionSetExtensionByProcessorDto
                       {
                           Id          = e.Id,
                           Extension   = e.Extension.Extension,
                           Processor   = e.Processor.Name,
                           ProcessorId = e.ProcessorId,
                           ExtensionId = e.ExtensionId
                       })
                      .OrderBy(e => e.Extension)
                      .ToListAsync();

    public async Task DeleteAsync(int id, string userId)
    {
        InstructionSetExtensionsByProcessor item = await context.InstructionSetExtensionsByProcessor.FindAsync(id);

        if(item is null) return;

        context.InstructionSetExtensionsByProcessor.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<int> CreateAsync(int processorId, int extensionId, string userId)
    {
        var item = new InstructionSetExtensionsByProcessor
        {
            ProcessorId = processorId,
            ExtensionId = extensionId
        };

        await context.InstructionSetExtensionsByProcessor.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }
}