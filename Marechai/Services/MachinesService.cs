using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Marechai.Services
{
    public class MachinesService
    {
        readonly MarechaiContext                   _context;
        readonly IStringLocalizer<MachinesService> _l;

        public MachinesService(MarechaiContext context, IStringLocalizer<MachinesService> localizer)
        {
            _context = context;
            _l       = localizer;
        }

        public async Task<List<MachineViewModel>> GetAsync() =>
            await _context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).ThenBy(m => m.Family.Name).
                           Select(m => new MachineViewModel
                           {
                               Id         = m.Id, Company      = m.Company.Name, Name = m.Name, Model = m.Model,
                               Introduced = m.Introduced, Type = m.Type, Family       = m.Family.Name
                           }).ToListAsync();

        public async Task<MachineViewModel> GetMachine(int id)
        {
            Machine machine = await _context.Machines.FindAsync(id);

            if(machine is null)
                return null;

            var model = new MachineViewModel
            {
                Introduced = machine.Introduced, Name = machine.Name, CompanyId = machine.CompanyId,
                Model      = machine.Model
            };

            Company company = await _context.Companies.FindAsync(model.CompanyId);

            if(company != null)
            {
                model.CompanyName = company.Name;

                IQueryable<CompanyLogo> logos = _context.CompanyLogos.Where(l => l.CompanyId == company.Id);

                if(model.Introduced.HasValue)
                    model.CompanyLogo = (await logos.FirstOrDefaultAsync(l => l.Year >= model.Introduced.Value.Year))?.
                        Guid;

                if(model.CompanyLogo is null &&
                   logos.Any())
                    model.CompanyLogo = (await logos.FirstAsync())?.Guid;
            }

            MachineFamily family = await _context.MachineFamilies.FindAsync(machine.FamilyId);

            if(family != null)
            {
                model.FamilyName = family.Name;
                model.FamilyId   = family.Id;
            }

            model.Gpus = await _context.GpusByMachine.Where(g => g.MachineId == machine.Id).Select(g => g.Gpu).
                                        Select(g => new GpuViewModel
                                        {
                                            Id         = g.Id, Name = g.Name, Company = g.Company.Name,
                                            CompanyId  = g.Company.Id, ModelCode = g.ModelCode,
                                            Introduced = g.Introduced, Package = g.Package, Process = g.Process,
                                            ProcessNm  = g.ProcessNm, DieSize = g.DieSize, Transistors = g.Transistors
                                        }).ToListAsync();

            model.Memory = await _context.MemoryByMachine.Where(m => m.MachineId == machine.Id).
                                          Select(m => new MemoryViewModel
                                          {
                                              Type = m.Type, Usage = m.Usage, Size = m.Size, Speed = m.Speed
                                          }).ToListAsync();

            model.Processors = await _context.ProcessorsByMachine.Where(p => p.MachineId == machine.Id).
                                              Select(p => new ProcessorViewModel
                                              {
                                                  Name = p.Processor.Name, CompanyName = p.Processor.Company.Name,
                                                  CompanyId = p.Processor.Company.Id, ModelCode = p.Processor.ModelCode,
                                                  Introduced = p.Processor.Introduced, Speed = p.Speed,
                                                  Package = p.Processor.Package, Gprs = p.Processor.Gprs,
                                                  GprSize = p.Processor.GprSize, Fprs = p.Processor.Fprs,
                                                  FprSize = p.Processor.FprSize, Cores = p.Processor.Cores,
                                                  ThreadsPerCore = p.Processor.ThreadsPerCore,
                                                  Process = p.Processor.Process, ProcessNm = p.Processor.ProcessNm,
                                                  DieSize = p.Processor.DieSize, Transistors = p.Processor.Transistors,
                                                  DataBus = p.Processor.DataBus, AddrBus = p.Processor.AddrBus,
                                                  SimdRegisters = p.Processor.SimdRegisters,
                                                  SimdSize = p.Processor.SimdSize,
                                                  L1Instruction = p.Processor.L1Instruction,
                                                  L1Data = p.Processor.L1Data, L2 = p.Processor.L2, L3 = p.Processor.L3,
                                                  InstructionSet = p.Processor.InstructionSet.Name,
                                                  InstructionSetExtensions =
                                                      p.Processor.InstructionSetExtensions.
                                                        Select(e => e.Extension.Extension).ToList()
                                              }).ToListAsync();

            model.SoundSynthesizers =
                await _context.SoundByMachine.Where(s => s.MachineId == machine.Id).Select(s => s.SoundSynth).
                               Select(s => new SoundSynthViewModel
                               {
                                   Id = s.Id, Name = s.Name, CompanyId = s.Company.Id, CompanyName = s.Company.Name,
                                   ModelCode = s.ModelCode, Introduced = s.Introduced, Voices = s.Voices,
                                   Frequency = s.Frequency, Depth = s.Depth, SquareWave = s.SquareWave,
                                   WhiteNoise = s.WhiteNoise, Type = s.Type
                               }).ToListAsync();

            model.Storage = await _context.StorageByMachine.Where(s => s.MachineId == machine.Id).
                                           Select(s => new StorageViewModel
                                           {
                                               Type = s.Type, Interface = s.Interface, Capacity = s.Capacity
                                           }).ToListAsync();

            return model;
        }
    }
}