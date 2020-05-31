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
        readonly GpusService                       _gpusService;
        readonly IStringLocalizer<MachinesService> _l;
        readonly ProcessorsService                 _processorsService;
        readonly SoundSynthsService                _soundSynthsService;

        public MachinesService(MarechaiContext context, IStringLocalizer<MachinesService> localizer,
                               GpusService gpusService, ProcessorsService processorsService,
                               SoundSynthsService soundSynthsService)
        {
            _context            = context;
            _l                  = localizer;
            _gpusService        = gpusService;
            _processorsService  = processorsService;
            _soundSynthsService = soundSynthsService;
        }

        public async Task<List<MachineViewModel>> GetAsync() =>
            await _context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).ThenBy(m => m.Family.Name).
                           Select(m => new MachineViewModel
                           {
                               Id         = m.Id, Company      = m.Company.Name, Name = m.Name, Model = m.Model,
                               Introduced = m.Introduced, Type = m.Type, Family       = m.Family.Name
                           }).ToListAsync();

        public async Task<MachineViewModel> GetAsync(int id) => await _context.Machines.Where(m => m.Id == id).
                                                                               Select(m => new MachineViewModel
                                                                               {
                                                                                   Id = m.Id, Company = m.Company.Name,
                                                                                   CompanyId = m.CompanyId,
                                                                                   Name = m.Name, Model = m.Model,
                                                                                   Introduced = m.Introduced,
                                                                                   Type = m.Type, FamilyId = m.FamilyId
                                                                               }).FirstOrDefaultAsync();

        public async Task UpdateAsync(MachineViewModel viewModel)
        {
            Machine model = await _context.Machines.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.CompanyId  = viewModel.CompanyId;
            model.Name       = viewModel.Name;
            model.Model      = viewModel.Model;
            model.Introduced = viewModel.Introduced;
            model.Type       = viewModel.Type;
            model.FamilyId   = viewModel.FamilyId;

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(MachineViewModel viewModel)
        {
            var model = new Machine
            {
                CompanyId  = viewModel.CompanyId, Name  = viewModel.Name, Model    = viewModel.Model,
                Introduced = viewModel.Introduced, Type = viewModel.Type, FamilyId = viewModel.FamilyId
            };

            await _context.Machines.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.Id;
        }

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
                model.Company = company.Name;

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

            model.Gpus = await _gpusService.GetByMachineAsync(machine.Id);

            model.Memory = await _context.MemoryByMachine.Where(m => m.MachineId == machine.Id).
                                          Select(m => new MemoryViewModel
                                          {
                                              Type = m.Type, Usage = m.Usage, Size = m.Size, Speed = m.Speed
                                          }).ToListAsync();

            model.Processors = await _processorsService.GetByMachineAsync(machine.Id);

            model.SoundSynthesizers = await _soundSynthsService.GetByMachineAsync(machine.Id);

            model.Storage = await _context.StorageByMachine.Where(s => s.MachineId == machine.Id).
                                           Select(s => new StorageViewModel
                                           {
                                               Type = s.Type, Interface = s.Interface, Capacity = s.Capacity
                                           }).ToListAsync();

            return model;
        }

        public async Task DeleteAsync(int id)
        {
            Machine item = await _context.Machines.FindAsync(id);

            if(item is null)
                return;

            _context.Machines.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}