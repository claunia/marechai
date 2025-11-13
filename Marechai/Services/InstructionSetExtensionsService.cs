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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services;

public class InstructionSetExtensionsService(MarechaiContext context)
{
    public async Task<List<InstructionSetExtension>> GetAsync() => await context.InstructionSetExtensions
                                                                                .OrderBy(e => e.Extension)
                                                                                .Select(e => new InstructionSetExtension
                                                                                 {
                                                                                     Extension = e.Extension,
                                                                                     Id        = e.Id
                                                                                 })
                                                                                .ToListAsync();

    public async Task<InstructionSetExtension> GetAsync(int id) => await context.InstructionSetExtensions
                                                                      .Where(e => e.Id == id)
                                                                      .Select(e => new InstructionSetExtension
                                                                       {
                                                                           Extension = e.Extension,
                                                                           Id        = e.Id
                                                                       })
                                                                      .FirstOrDefaultAsync();

    public async Task UpdateAsync(InstructionSetExtension viewModel, string userId)
    {
        InstructionSetExtension model = await context.InstructionSetExtensions.FindAsync(viewModel.Id);

        if(model is null) return;

        model.Extension = viewModel.Extension;

        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<int> CreateAsync(InstructionSetExtension viewModel, string userId)
    {
        var model = new InstructionSetExtension
        {
            Extension = viewModel.Extension
        };

        await context.InstructionSetExtensions.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    public async Task DeleteAsync(int id, string userId)
    {
        InstructionSetExtension item = await context.InstructionSetExtensions.FindAsync(id);

        if(item is null) return;

        context.InstructionSetExtensions.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }

    public bool VerifyUnique(string extension) =>
        !context.InstructionSetExtensions.Any(i => string.Equals(i.Extension,
                                                                  extension,
                                                                  StringComparison.InvariantCultureIgnoreCase));
}