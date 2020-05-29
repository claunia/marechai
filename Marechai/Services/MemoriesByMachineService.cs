using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class MemoriesByMachineService
    {
        readonly MarechaiContext _context;

        public MemoriesByMachineService(MarechaiContext context) => _context = context;

        public async Task<List<MemoryByMachineViewModel>> GetByMachine(int machineId) =>
            await _context.MemoryByMachine.Where(m => m.MachineId == machineId).Select(m => new MemoryByMachineViewModel
            {
                Id    = m.Id, Type         = m.Type, Usage = m.Usage, Size = m.Size,
                Speed = m.Speed, MachineId = m.MachineId
            }).OrderBy(m => m.Type).ThenBy(m => m.Usage).ThenBy(m => m.Size).ThenBy(m => m.Speed).ToListAsync();

        public async Task DeleteAsync(long id)
        {
            MemoryByMachine item = await _context.MemoryByMachine.FindAsync(id);

            if(item is null)
                return;

            _context.MemoryByMachine.Remove(item);

            await _context.SaveChangesAsync();
        }

        public async Task<long> CreateAsync(int machineId, MemoryType type, MemoryUsage usage, long? size,
                                            double? speed)
        {
            var item = new MemoryByMachine
            {
                MachineId = machineId, Type = type, Usage = usage, Size = size,
                Speed     = speed
            };

            await _context.MemoryByMachine.AddAsync(item);
            await _context.SaveChangesAsync();

            return item.Id;
        }
    }
}