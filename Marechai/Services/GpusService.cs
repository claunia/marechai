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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class GpusService
    {
        readonly MarechaiContext _context;

        public GpusService(MarechaiContext context) => _context = context;

        public async Task<List<GpuViewModel>> GetAsync() =>
            await _context.Gpus.OrderBy(g => g.Company.Name).ThenBy(g => g.Name).ThenBy(g => g.Introduced).
                           Select(g => new GpuViewModel
                           {
                               Id         = g.Id,
                               Company    = g.Company.Name,
                               Introduced = g.Introduced,
                               ModelCode  = g.ModelCode,
                               Name       = g.Name
                           }).ToListAsync();

        public async Task<List<GpuViewModel>> GetByMachineAsync(int machineId) =>
            await _context.GpusByMachine.Where(g => g.MachineId == machineId).Select(g => g.Gpu).
                           OrderBy(g => g.Company.Name).ThenBy(g => g.Name).Select(g => new GpuViewModel
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
                           }).ToListAsync();

        public async Task<GpuViewModel> GetAsync(int id) =>
            await _context.Gpus.Where(g => g.Id == id).Select(g => new GpuViewModel
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
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(GpuViewModel viewModel, string userId)
        {
            Gpu model = await _context.Gpus.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Name        = viewModel.Name;
            model.CompanyId   = viewModel.CompanyId;
            model.ModelCode   = viewModel.ModelCode;
            model.Introduced  = viewModel.Introduced;
            model.Package     = viewModel.Package;
            model.Process     = viewModel.Process;
            model.ProcessNm   = viewModel.ProcessNm;
            model.DieSize     = viewModel.DieSize;
            model.Transistors = viewModel.Transistors;

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<int> CreateAsync(GpuViewModel viewModel, string userId)
        {
            var model = new Gpu
            {
                Name        = viewModel.Name,
                CompanyId   = viewModel.CompanyId,
                ModelCode   = viewModel.ModelCode,
                Introduced  = viewModel.Introduced,
                Package     = viewModel.Package,
                Process     = viewModel.Process,
                ProcessNm   = viewModel.ProcessNm,
                DieSize     = viewModel.DieSize,
                Transistors = viewModel.Transistors
            };

            await _context.Gpus.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(int id, string userId)
        {
            Gpu item = await _context.Gpus.FindAsync(id);

            if(item is null)
                return;

            _context.Gpus.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}