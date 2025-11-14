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

public class GpusService(MarechaiContext context)
{
    public async Task<List<GpuDto>> GetAsync() => await context.Gpus.OrderBy(g => g.Company.Name)
                                                                     .ThenBy(g => g.Name)
                                                                     .ThenBy(g => g.Introduced)
                                                                     .Select(g => new GpuDto
                                                                      {
                                                                          Id         = g.Id,
                                                                          Company    = g.Company.Name,
                                                                          Introduced = g.Introduced,
                                                                          ModelCode  = g.ModelCode,
                                                                          Name       = g.Name
                                                                      })
                                                                     .ToListAsync();

    public async Task<List<GpuDto>> GetByMachineAsync(int machineId) => await context.GpusByMachine
                                                                                 .Where(g => g.MachineId == machineId)
                                                                                 .Select(g => g.Gpu)
                                                                                 .OrderBy(g => g.Company.Name)
                                                                                 .ThenBy(g => g.Name)
                                                                                 .Select(g => new GpuDto
                                                                                  {
                                                                                      Id          = g.Id,
                                                                                      Name        = g.Name,
                                                                                      Company     = g.Company.Name,
                                                                                      CompanyId   = g.Company.Id,
                                                                                      ModelCode   = g.ModelCode,
                                                                                      Introduced  = g.Introduced,
                                                                                      Package     = g.Package,
                                                                                      Process     = g.Process,
                                                                                      ProcessNm   = g.ProcessNm,
                                                                                      DieSize     = g.DieSize,
                                                                                      Transistors = g.Transistors
                                                                                  })
                                                                                 .ToListAsync();

    public async Task<GpuDto> GetAsync(int id) => await context.Gpus.Where(g => g.Id == id)
                                                                      .Select(g => new GpuDto
                                                                       {
                                                                           Id          = g.Id,
                                                                           Name        = g.Name,
                                                                           CompanyId   = g.Company.Id,
                                                                           ModelCode   = g.ModelCode,
                                                                           Introduced  = g.Introduced,
                                                                           Package     = g.Package,
                                                                           Process     = g.Process,
                                                                           ProcessNm   = g.ProcessNm,
                                                                           DieSize     = g.DieSize,
                                                                           Transistors = g.Transistors
                                                                       })
                                                                      .FirstOrDefaultAsync();

    public async Task UpdateAsync(GpuDto dto, string userId)
    {
        Gpu model = await context.Gpus.FindAsync(dto.Id);

        if(model is null) return;

        model.Name        = dto.Name;
        model.CompanyId   = dto.CompanyId;
        model.ModelCode   = dto.ModelCode;
        model.Introduced  = dto.Introduced;
        model.Package     = dto.Package;
        model.Process     = dto.Process;
        model.ProcessNm   = dto.ProcessNm;
        model.DieSize     = dto.DieSize;
        model.Transistors = dto.Transistors;

        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<int> CreateAsync(GpuDto dto, string userId)
    {
        var model = new Gpu
        {
            Name        = dto.Name,
            CompanyId   = dto.CompanyId,
            ModelCode   = dto.ModelCode,
            Introduced  = dto.Introduced,
            Package     = dto.Package,
            Process     = dto.Process,
            ProcessNm   = dto.ProcessNm,
            DieSize     = dto.DieSize,
            Transistors = dto.Transistors
        };

        await context.Gpus.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    public async Task DeleteAsync(int id, string userId)
    {
        Gpu item = await context.Gpus.FindAsync(id);

        if(item is null) return;

        context.Gpus.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}