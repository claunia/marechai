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