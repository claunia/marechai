using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class SoundSynthsByMachineService
    {
        readonly MarechaiContext _context;

        public SoundSynthsByMachineService(MarechaiContext context) => _context = context;

        public async Task<List<SoundSynthByMachineViewModel>> GetByMachine(int machineId) =>
            await _context.SoundByMachine.Where(g => g.MachineId == machineId).
                           Select(g => new SoundSynthByMachineViewModel
                           {
                               Id           = g.Id, Name = g.SoundSynth.Name, CompanyName = g.SoundSynth.Company.Name,
                               SoundSynthId = g.SoundSynthId, MachineId = g.MachineId
                           }).OrderBy(g => g.CompanyName).ThenBy(g => g.Name).ToListAsync();

        public async Task DeleteAsync(long id)
        {
            SoundByMachine item = await _context.SoundByMachine.FindAsync(id);

            if(item is null)
                return;

            _context.SoundByMachine.Remove(item);

            await _context.SaveChangesAsync();
        }

        public async Task<long> CreateAsync(int soundSynthId, int machineId)
        {
            var item = new SoundByMachine
            {
                SoundSynthId = soundSynthId, MachineId = machineId
            };

            await _context.SoundByMachine.AddAsync(item);
            await _context.SaveChangesAsync();

            return item.Id;
        }
    }
}