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
using Marechai.Database;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services;

public class ComputersService(MarechaiContext context)
{
    public async Task<int> GetComputersCountAsync() =>
        await context.Machines.CountAsync(c => c.Type == MachineType.Computer);

    public Task<int> GetMinimumYearAsync() => context.Machines
                                                      .Where(t => t.Type == MachineType.Computer &&
                                                                  t.Introduced.HasValue          &&
                                                                  t.Introduced.Value.Year > 1000)
                                                      .MinAsync(t => t.Introduced.Value.Year);

    public Task<int> GetMaximumYearAsync() => context.Machines
                                                      .Where(t => t.Type == MachineType.Computer &&
                                                                  t.Introduced.HasValue          &&
                                                                  t.Introduced.Value.Year > 1000)
                                                      .MaxAsync(t => t.Introduced.Value.Year);

    public async Task<List<MachineViewModel>> GetComputersByLetterAsync(char c) => await context.Machines
       .Include(m => m.Company)
       .Where(m => m.Type == MachineType.Computer && EF.Functions.Like(m.Name, $"{c}%"))
       .OrderBy(m => m.Company.Name)
       .ThenBy(m => m.Name)
       .Select(m => new MachineViewModel
        {
            Id      = m.Id,
            Name    = m.Name,
            Company = m.Company.Name
        })
       .ToListAsync();

    public async Task<List<MachineViewModel>> GetComputersByYearAsync(int year) => await context.Machines
       .Include(m => m.Company)
       .Where(m => m.Type == MachineType.Computer && m.Introduced != null && m.Introduced.Value.Year == year)
       .OrderBy(m => m.Company.Name)
       .ThenBy(m => m.Name)
       .Select(m => new MachineViewModel
        {
            Id      = m.Id,
            Name    = m.Name,
            Company = m.Company.Name
        })
       .ToListAsync();

    public async Task<List<MachineViewModel>> GetComputersAsync() => await context.Machines.Include(m => m.Company)
                                                                        .Where(m => m.Type == MachineType.Computer)
                                                                        .OrderBy(m => m.Company.Name)
                                                                        .ThenBy(m => m.Name)
                                                                        .Select(m => new MachineViewModel
                                                                         {
                                                                             Id      = m.Id,
                                                                             Name    = m.Name,
                                                                             Company = m.Company.Name
                                                                         })
                                                                        .ToListAsync();
}