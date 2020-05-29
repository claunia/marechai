using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class GpuByMachineService
    {
        readonly MarechaiContext _context;

        public GpuByMachineService(MarechaiContext context) => _context = context;

        public async Task<List<GpuByMachineViewModel>> GetByMachine(int machineId) =>
            await _context.GpusByMachine.Where(g => g.MachineId == machineId).Select(g => new GpuByMachineViewModel
            {
                Id        = g.Id, Name = g.Gpu.Name, CompanyName = g.Gpu.Company.Name, GpuId = g.GpuId,
                MachineId = g.MachineId
            }).OrderBy(g => g.CompanyName).ThenBy(g => g.Name).ToListAsync();

        public async Task DeleteAsync(long id)
        {
            GpusByMachine item = await _context.GpusByMachine.FindAsync(id);

            if(item is null)
                return;

            _context.GpusByMachine.Remove(item);

            await _context.SaveChangesAsync();
        }

        public async Task<long> CreateAsync(int gpuId, int machineId)
        {
            var item = new GpusByMachine
            {
                GpuId = gpuId, MachineId = machineId
            };

            await _context.GpusByMachine.AddAsync(item);
            await _context.SaveChangesAsync();

            return item.Id;
        }
    }
}