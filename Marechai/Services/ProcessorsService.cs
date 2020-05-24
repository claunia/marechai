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
                L3 = p.L3, InstructionSet = p.InstructionSet.Name,
                InstructionSetExtensions = p.InstructionSetExtensions.Select(e => e.Extension.Extension).ToList()
            }).ToListAsync();

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
                InstructionSetExtensions =
                    p.Processor.InstructionSetExtensions.Select(e => e.Extension.Extension).ToList()
            }).ToListAsync();
    }
}