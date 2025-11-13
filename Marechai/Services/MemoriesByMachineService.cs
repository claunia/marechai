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

public class MemoriesByMachineService(MarechaiContext context)
{
    public async Task<List<MemoryByMachineViewModel>> GetByMachine(int machineId) => await context.MemoryByMachine
                                                                                                  .Where(m => m.MachineId == machineId)
                                                                                                  .Select(m => new MemoryByMachineViewModel
                                                                                                   {
                                                                                                       Id        = m.Id,
                                                                                                       Type      = m.Type,
                                                                                                       Usage     = m.Usage,
                                                                                                       Size      = m.Size,
                                                                                                       Speed     = m.Speed,
                                                                                                       MachineId = m.MachineId
                                                                                                   })
                                                                                                  .OrderBy(m => m.Type)
                                                                                                  .ThenBy(m => m.Usage)
                                                                                                  .ThenBy(m => m.Size)
                                                                                                  .ThenBy(m => m.Speed)
                                                                                                  .ToListAsync();

    public async Task DeleteAsync(long id, string userId)
    {
        MemoryByMachine item = await context.MemoryByMachine.FindAsync(id);

        if(item is null) return;

        context.MemoryByMachine.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<long> CreateAsync(int    machineId, MemoryType type, MemoryUsage usage, long? size, double? speed,
                                        string userId)
    {
        var item = new MemoryByMachine
        {
            MachineId = machineId,
            Type      = type,
            Usage     = usage,
            Size      = size,
            Speed     = speed
        };

        await context.MemoryByMachine.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }
}