using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class SoundSynthsService
    {
        readonly MarechaiContext _context;

        public SoundSynthsService(MarechaiContext context) => _context = context;

        public async Task<List<SoundSynthViewModel>> GetAsync() =>
            await _context.SoundSynths.OrderBy(s => s.Company.Name).ThenBy(s => s.Name).ThenBy(s => s.ModelCode).
                           Select(s => new SoundSynthViewModel
                           {
                               Id         = s.Id, Name = s.Name, CompanyId = s.Company.Id, CompanyName = s.Company.Name,
                               ModelCode  = s.ModelCode, Introduced = s.Introduced, Voices = s.Voices,
                               Frequency  = s.Frequency, Depth = s.Depth, SquareWave = s.SquareWave,
                               WhiteNoise = s.WhiteNoise, Type = s.Type
                           }).ToListAsync();

        public async Task<List<SoundSynthViewModel>> GetByMachineAsync(int machineId) =>
            await _context.SoundByMachine.Where(s => s.MachineId == machineId).Select(s => s.SoundSynth).
                           OrderBy(s => s.Company.Name).ThenBy(s => s.Name).ThenBy(s => s.ModelCode).
                           Select(s => new SoundSynthViewModel
                           {
                               Id         = s.Id, Name = s.Name, CompanyId = s.Company.Id, CompanyName = s.Company.Name,
                               ModelCode  = s.ModelCode, Introduced = s.Introduced, Voices = s.Voices,
                               Frequency  = s.Frequency, Depth = s.Depth, SquareWave = s.SquareWave,
                               WhiteNoise = s.WhiteNoise, Type = s.Type
                           }).ToListAsync();

        public async Task<SoundSynth> GetAsync(int id) => await _context.SoundSynths.FindAsync(id);

        public async Task DeleteAsync(int id)
        {
            SoundSynth item = await _context.SoundSynths.FindAsync(id);

            if(item is null)
                return;

            _context.SoundSynths.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}