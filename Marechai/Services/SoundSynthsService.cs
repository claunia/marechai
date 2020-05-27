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

        public async Task<SoundSynthViewModel> GetAsync(int id) => await _context.SoundSynths.Where(s => s.Id == id).
                                                                                  Select(s => new SoundSynthViewModel
                                                                                  {
                                                                                      Id          = s.Id, Name = s.Name,
                                                                                      CompanyId   = s.Company.Id,
                                                                                      CompanyName = s.Company.Name,
                                                                                      ModelCode   = s.ModelCode,
                                                                                      Introduced  = s.Introduced,
                                                                                      Voices      = s.Voices,
                                                                                      Frequency   = s.Frequency,
                                                                                      Depth       = s.Depth,
                                                                                      SquareWave  = s.SquareWave,
                                                                                      WhiteNoise  = s.WhiteNoise,
                                                                                      Type        = s.Type
                                                                                  }).FirstOrDefaultAsync();

        public async Task UpdateAsync(SoundSynthViewModel viewModel)
        {
            SoundSynth model = await _context.SoundSynths.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Depth      = viewModel.Depth;
            model.Frequency  = viewModel.Frequency;
            model.Introduced = viewModel.Introduced;
            model.Name       = viewModel.Name;
            model.Type       = viewModel.Type;
            model.Voices     = viewModel.Voices;
            model.CompanyId  = viewModel.CompanyId;
            model.ModelCode  = viewModel.ModelCode;
            model.SquareWave = viewModel.SquareWave;
            model.WhiteNoise = viewModel.WhiteNoise;

            await _context.SaveChangesAsync();
        }

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