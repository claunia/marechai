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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services;

public class MachineFamiliesService(MarechaiContext context)
{
    public async Task<List<MachineFamilyDto>> GetAsync() => await context.MachineFamilies
                                                                               .OrderBy(m => m.Company.Name)
                                                                               .ThenBy(m => m.Name)
                                                                               .Select(m => new MachineFamilyDto
                                                                                {
                                                                                    Id      = m.Id,
                                                                                    Company = m.Company.Name,
                                                                                    Name    = m.Name
                                                                                })
                                                                               .OrderBy(m => m.Name)
                                                                               .ToListAsync();

    public async Task<MachineFamilyDto> GetAsync(int id) => await context.MachineFamilies.Where(f => f.Id == id)
                                                                     .Select(m => new MachineFamilyDto
                                                                      {
                                                                          Id        = m.Id,
                                                                          CompanyId = m.CompanyId,
                                                                          Name      = m.Name
                                                                      })
                                                                     .FirstOrDefaultAsync();

    public async Task UpdateAsync(MachineFamilyDto dto, string userId)
    {
        MachineFamily model = await context.MachineFamilies.FindAsync(dto.Id);

        if(model is null) return;

        model.Name      = dto.Name;
        model.CompanyId = dto.CompanyId;

        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<int> CreateAsync(MachineFamilyDto dto, string userId)
    {
        var model = new MachineFamily
        {
            Name      = dto.Name,
            CompanyId = dto.CompanyId
        };

        await context.MachineFamilies.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    public async Task DeleteAsync(int id, string userId)
    {
        MachineFamily item = await context.MachineFamilies.FindAsync(id);

        if(item is null) return;

        context.MachineFamilies.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}