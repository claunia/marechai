using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class StorageByMachineService
    {
        readonly MarechaiContext _context;

        public StorageByMachineService(MarechaiContext context) => _context = context;

        public async Task<List<StorageByMachineViewModel>> GetByMachine(int machineId) =>
            await _context.StorageByMachine.Where(s => s.MachineId == machineId).
                           Select(s => new StorageByMachineViewModel
                           {
                               Id        = s.Id, Type = s.Type, Interface = s.Interface, Capacity = s.Capacity,
                               MachineId = s.MachineId
                           }).OrderBy(s => s.Type).ThenBy(s => s.Interface).ThenBy(s => s.Capacity).ToListAsync();

        public async Task DeleteAsync(long id)
        {
            StorageByMachine item = await _context.StorageByMachine.FindAsync(id);

            if(item is null)
                return;

            _context.StorageByMachine.Remove(item);

            await _context.SaveChangesAsync();
        }

        public async Task<long> CreateAsync(int machineId, StorageType type, StorageInterface @interface,
                                            long? capacity)
        {
            var item = new StorageByMachine
            {
                MachineId = machineId, Type = type, Interface = @interface, Capacity = capacity
            };

            await _context.StorageByMachine.AddAsync(item);
            await _context.SaveChangesAsync();

            return item.Id;
        }
    }
}