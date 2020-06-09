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
    public class ProcessorsService
    {
        readonly MarechaiContext _context;

        public ProcessorsService(MarechaiContext context) => _context = context;

        public async Task<List<ProcessorViewModel>> GetAsync() =>
            await _context.Processors.Select(p => new ProcessorViewModel
            {
                Name = p.Name, CompanyName = p.Company.Name, CompanyId = p.Company.Id, ModelCode = p.ModelCode,
                Introduced = p.Introduced, Speed = p.Speed, Package = p.Package, Gprs = p.Gprs,
                GprSize = p.GprSize, Fprs = p.Fprs, FprSize = p.FprSize, Cores = p.Cores,
                ThreadsPerCore = p.ThreadsPerCore, Process = p.Process, ProcessNm = p.ProcessNm, DieSize = p.DieSize,
                Transistors = p.Transistors, DataBus = p.DataBus, AddrBus = p.AddrBus, SimdRegisters = p.SimdRegisters,
                SimdSize = p.SimdSize, L1Instruction = p.L1Instruction, L1Data = p.L1Data, L2 = p.L2,
                L3 = p.L3, InstructionSet = p.InstructionSet.Name, Id = p.Id,
                InstructionSetExtensions = p.InstructionSetExtensions.Select(e => e.Extension.Extension).ToList()
            }).OrderBy(p => p.CompanyName).ThenBy(p => p.Name).ToListAsync();

        public async Task<List<ProcessorViewModel>> GetByMachineAsync(int machineId) =>
            await _context.ProcessorsByMachine.Where(p => p.MachineId == machineId).Select(p => new ProcessorViewModel
            {
                Name = p.Processor.Name, CompanyName = p.Processor.Company.Name, CompanyId = p.Processor.Company.Id,
                ModelCode = p.Processor.ModelCode, Introduced = p.Processor.Introduced, Speed = p.Speed,
                Package = p.Processor.Package, Gprs = p.Processor.Gprs, GprSize = p.Processor.GprSize,
                Fprs = p.Processor.Fprs, FprSize = p.Processor.FprSize, Cores = p.Processor.Cores,
                ThreadsPerCore = p.Processor.ThreadsPerCore, Process = p.Processor.Process,
                ProcessNm = p.Processor.ProcessNm, DieSize = p.Processor.DieSize, Transistors = p.Processor.Transistors,
                DataBus = p.Processor.DataBus, AddrBus = p.Processor.AddrBus, SimdRegisters = p.Processor.SimdRegisters,
                SimdSize = p.Processor.SimdSize, L1Instruction = p.Processor.L1Instruction, L1Data = p.Processor.L1Data,
                L2 = p.Processor.L2, L3 = p.Processor.L3, InstructionSet = p.Processor.InstructionSet.Name,
                Id = p.Processor.Id,
                InstructionSetExtensions =
                    p.Processor.InstructionSetExtensions.Select(e => e.Extension.Extension).ToList()
            }).OrderBy(p => p.CompanyName).ThenBy(p => p.Name).ToListAsync();

        public async Task<ProcessorViewModel> GetAsync(int id) =>
            await _context.Processors.Where(p => p.Id == id).Select(p => new ProcessorViewModel
            {
                Name = p.Name, CompanyName = p.Company.Name, CompanyId = p.Company.Id, ModelCode = p.ModelCode,
                Introduced = p.Introduced, Speed = p.Speed, Package = p.Package, Gprs = p.Gprs,
                GprSize = p.GprSize, Fprs = p.Fprs, FprSize = p.FprSize, Cores = p.Cores,
                ThreadsPerCore = p.ThreadsPerCore, Process = p.Process, ProcessNm = p.ProcessNm, DieSize = p.DieSize,
                Transistors = p.Transistors, DataBus = p.DataBus, AddrBus = p.AddrBus, SimdRegisters = p.SimdRegisters,
                SimdSize = p.SimdSize, L1Instruction = p.L1Instruction, L1Data = p.L1Data, L2 = p.L2,
                L3 = p.L3, InstructionSet = p.InstructionSet.Name, InstructionSetId = p.InstructionSetId
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(ProcessorViewModel viewModel)
        {
            Processor model = await _context.Processors.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.AddrBus          = viewModel.AddrBus;
            model.CompanyId        = viewModel.CompanyId;
            model.Cores            = viewModel.Cores;
            model.DataBus          = viewModel.DataBus;
            model.DieSize          = viewModel.DieSize;
            model.Fprs             = viewModel.Fprs;
            model.FprSize          = viewModel.FprSize;
            model.Gprs             = viewModel.Gprs;
            model.GprSize          = viewModel.GprSize;
            model.InstructionSetId = viewModel.InstructionSetId;
            model.Introduced       = viewModel.Introduced;
            model.L1Data           = viewModel.L1Data;
            model.L1Instruction    = viewModel.L1Instruction;
            model.L2               = viewModel.L2;
            model.L3               = viewModel.L3;
            model.ModelCode        = viewModel.ModelCode;
            model.Name             = viewModel.Name;
            model.Package          = viewModel.Package;
            model.Process          = viewModel.Process;
            model.ProcessNm        = viewModel.ProcessNm;
            model.SimdRegisters    = viewModel.SimdRegisters;
            model.SimdSize         = viewModel.SimdSize;
            model.Speed            = viewModel.Speed;
            model.ThreadsPerCore   = viewModel.ThreadsPerCore;
            model.Transistors      = viewModel.Transistors;

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(ProcessorViewModel viewModel)
        {
            var model = new Processor
            {
                AddrBus = viewModel.AddrBus, CompanyId = viewModel.CompanyId, Cores = viewModel.Cores,
                DataBus = viewModel.DataBus, DieSize = viewModel.DieSize, Fprs = viewModel.Fprs,
                FprSize = viewModel.FprSize, Gprs = viewModel.Gprs, GprSize = viewModel.GprSize,
                InstructionSetId = viewModel.InstructionSetId, Introduced = viewModel.Introduced,
                L1Data = viewModel.L1Data, L1Instruction = viewModel.L1Instruction, L2 = viewModel.L2,
                L3 = viewModel.L3, ModelCode = viewModel.ModelCode, Name = viewModel.Name, Package = viewModel.Package,
                Process = viewModel.Process, ProcessNm = viewModel.ProcessNm, SimdRegisters = viewModel.SimdRegisters,
                SimdSize = viewModel.SimdSize, Speed = viewModel.Speed, ThreadsPerCore = viewModel.ThreadsPerCore,
                Transistors = viewModel.Transistors
            };

            await _context.Processors.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.Id;
        }

        public async Task DeleteAsync(int id)
        {
            Processor item = await _context.Processors.FindAsync(id);

            if(item is null)
                return;

            _context.Processors.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}