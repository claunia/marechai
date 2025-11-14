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
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services;

public class InstructionSetsService(MarechaiContext context)
{
    public async Task<List<InstructionSet>> GetAsync() => await context.InstructionSets.OrderBy(e => e.Name)
                                                                       .Select(e => new InstructionSet
                                                                        {
                                                                            Name = e.Name,
                                                                            Id   = e.Id
                                                                        })
                                                                       .ToListAsync();

    public async Task<InstructionSet> GetAsync(int id) => await context.InstructionSets.Where(e => e.Id == id)
                                                                        .Select(e => new InstructionSet
                                                                         {
                                                                             Name = e.Name,
                                                                             Id   = e.Id
                                                                         })
                                                                        .FirstOrDefaultAsync();

    public async Task UpdateAsync(InstructionSet viewModel, string userId)
    {
        InstructionSet model = await context.InstructionSets.FindAsync(viewModel.Id);

        if(model is null) return;

        model.Name = viewModel.Name;

        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<int> CreateAsync(InstructionSet viewModel, string userId)
    {
        var model = new InstructionSet
        {
            Name = viewModel.Name
        };

        await context.InstructionSets.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    public async Task DeleteAsync(int id, string userId)
    {
        InstructionSet item = await context.InstructionSets.FindAsync(id);

        if(item is null) return;

        context.InstructionSets.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }

    public bool VerifyUnique(string name) =>
        !context.InstructionSets.Any(i => string.Equals(i.Name, name, StringComparison.InvariantCultureIgnoreCase));
}