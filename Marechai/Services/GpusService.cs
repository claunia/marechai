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
                               Id = g.Id, Company = g.Company.Name, Introduced = g.Introduced, ModelCode = g.ModelCode,
                               Name = g.Name
                           }).ToListAsync();

        public async Task<List<GpuViewModel>> GetByMachineAsync(int machineId) =>
            await _context.GpusByMachine.Where(g => g.MachineId == machineId).Select(g => g.Gpu).
                           OrderBy(g => g.Company.Name).ThenBy(g => g.Name).Select(g => new GpuViewModel
                           {
                               Id          = g.Id, Name = g.Name, Company = g.Company.Name, CompanyId = g.Company.Id,
                               ModelCode   = g.ModelCode, Introduced = g.Introduced, Package = g.Package,
                               Process     = g.Process, ProcessNm = g.ProcessNm, DieSize = g.DieSize,
                               Transistors = g.Transistors
                           }).ToListAsync();

        public async Task<GpuViewModel> GetAsync(int id) =>
            await _context.Gpus.Where(g => g.Id == id).Select(g => new GpuViewModel
            {
                Id         = g.Id, Name             = g.Name, CompanyId  = g.Company.Id, ModelCode = g.ModelCode,
                Introduced = g.Introduced, Package  = g.Package, Process = g.Process, ProcessNm    = g.ProcessNm,
                DieSize    = g.DieSize, Transistors = g.Transistors
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(GpuViewModel viewModel)
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

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Gpu item = await _context.Gpus.FindAsync(id);

            if(item is null)
                return;

            _context.Gpus.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}