/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

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

        public async Task<int> CreateAsync(SoundSynthViewModel viewModel)
        {
            var model = new SoundSynth
            {
                Depth      = viewModel.Depth, Frequency     = viewModel.Frequency, Introduced = viewModel.Introduced,
                Name       = viewModel.Name, Type           = viewModel.Type, Voices          = viewModel.Voices,
                CompanyId  = viewModel.CompanyId, ModelCode = viewModel.ModelCode, SquareWave = viewModel.SquareWave,
                WhiteNoise = viewModel.WhiteNoise
            };

            await _context.SoundSynths.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.Id;
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