using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class ProcessorsByMachineService
    {
        readonly MarechaiContext _context;

        public ProcessorsByMachineService(MarechaiContext context) => _context = context;

        public async Task<List<ProcessorByMachineViewModel>> GetByMachine(int machineId) =>
            await _context.ProcessorsByMachine.Where(p => p.MachineId == machineId).
                           Select(p => new ProcessorByMachineViewModel
                           {
                               Id          = p.Id, Name = p.Processor.Name, CompanyName = p.Processor.Company.Name,
                               ProcessorId = p.ProcessorId, MachineId = p.MachineId, Speed = p.Speed
                           }).OrderBy(p => p.CompanyName).ThenBy(p => p.Name).ToListAsync();

        public async Task DeleteAsync(long id)
        {
            ProcessorsByMachine item = await _context.ProcessorsByMachine.FindAsync(id);

            if(item is null)
                return;

            _context.ProcessorsByMachine.Remove(item);

            await _context.SaveChangesAsync();
        }

        public async Task<long> CreateAsync(int processorId, int machineId, float? speed)
        {
            var item = new ProcessorsByMachine
            {
                ProcessorId = processorId, MachineId = machineId, Speed = speed
            };

            await _context.ProcessorsByMachine.AddAsync(item);
            await _context.SaveChangesAsync();

            return item.Id;
        }
    }
}