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
    public class SoftwareFamiliesService
    {
        readonly MarechaiContext _context;

        public SoftwareFamiliesService(MarechaiContext context) => _context = context;

        public async Task<List<SoftwareFamilyViewModel>> GetAsync() => await _context.
                                                                             SoftwareFamilies.OrderBy(b => b.Name).
                                                                             Select(b => new SoftwareFamilyViewModel
                                                                             {
                                                                                 Id         = b.Id,
                                                                                 Name       = b.Name,
                                                                                 Parent     = b.Parent.Name,
                                                                                 Introduced = b.Introduced,
                                                                                 ParentId   = b.ParentId
                                                                             }).ToListAsync();

        public async Task<SoftwareFamilyViewModel> GetAsync(ulong id) =>
            await _context.SoftwareFamilies.Where(b => b.Id == id).Select(b => new SoftwareFamilyViewModel
            {
                Id         = b.Id,
                Name       = b.Name,
                Parent     = b.Parent.Name,
                Introduced = b.Introduced,
                ParentId   = b.ParentId
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(SoftwareFamilyViewModel viewModel, string userId)
        {
            SoftwareFamily model = await _context.SoftwareFamilies.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Name       = viewModel.Name;
            model.ParentId   = viewModel.ParentId;
            model.Introduced = viewModel.Introduced;
            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<ulong> CreateAsync(SoftwareFamilyViewModel viewModel, string userId)
        {
            var model = new SoftwareFamily
            {
                Name       = viewModel.Name,
                ParentId   = viewModel.ParentId,
                Introduced = viewModel.Introduced
            };

            await _context.SoftwareFamilies.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(ulong id, string userId)
        {
            SoftwareFamily item = await _context.SoftwareFamilies.FindAsync(id);

            if(item is null)
                return;

            _context.SoftwareFamilies.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}