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
    public class ConsolesService
    {
        readonly MarechaiContext _context;

        public ConsolesService(MarechaiContext context) => _context = context;

        public async Task<int> GetConsolesCountAsync() =>
            await _context.Machines.CountAsync(c => c.Type == MachineType.Console);

        public Task<int> GetMinimumYearAsync() => _context.Machines.
                                                           Where(t => t.Type == MachineType.Console &&
                                                                      t.Introduced.HasValue         &&
                                                                      t.Introduced.Value.Year > 1000).
                                                           MinAsync(t => t.Introduced.Value.Year);

        public Task<int> GetMaximumYearAsync() => _context.Machines.
                                                           Where(t => t.Type == MachineType.Console &&
                                                                      t.Introduced.HasValue         &&
                                                                      t.Introduced.Value.Year > 1000).
                                                           MaxAsync(t => t.Introduced.Value.Year);

        public async Task<List<MachineViewModel>> GetConsolesByLetterAsync(char c) =>
            await _context.Machines.Include(m => m.Company).
                           Where(m => m.Type == MachineType.Console && EF.Functions.Like(m.Name, $"{c}%")).
                           OrderBy(m => m.Company.Name).ThenBy(m => m.Name).Select(m => new MachineViewModel
                           {
                               Id      = m.Id,
                               Name    = m.Name,
                               Company = m.Company.Name
                           }).ToListAsync();

        public async Task<List<MachineViewModel>> GetConsolesByYearAsync(int year) =>
            await _context.Machines.Include(m => m.Company).
                           Where(m => m.Type                  == MachineType.Console && m.Introduced != null &&
                                      m.Introduced.Value.Year == year).OrderBy(m => m.Company.Name).ThenBy(m => m.Name).
                           Select(m => new MachineViewModel
                           {
                               Id      = m.Id,
                               Name    = m.Name,
                               Company = m.Company.Name
                           }).ToListAsync();

        public async Task<List<MachineViewModel>> GetConsolesAsync() =>
            await _context.Machines.Include(m => m.Company).Where(m => m.Type == MachineType.Console).
                           OrderBy(m => m.Company.Name).ThenBy(m => m.Name).Select(m => new MachineViewModel
                           {
                               Id      = m.Id,
                               Name    = m.Name,
                               Company = m.Company.Name
                           }).ToListAsync();
    }
}