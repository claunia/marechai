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
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class LicensesService
    {
        readonly MarechaiContext _context;

        public LicensesService(MarechaiContext context) => _context = context;

        public async Task<List<License>> GetAsync() =>
            await _context.Licenses.OrderBy(l => l.Name).Select(l => new License
            {
                FsfApproved = l.FsfApproved,
                Id          = l.Id,
                Link        = l.Link,
                Name        = l.Name,
                OsiApproved = l.OsiApproved,
                SPDX        = l.SPDX
            }).ToListAsync();

        public async Task<License> GetAsync(int id) =>
            await _context.Licenses.Where(l => l.Id == id).Select(l => new License
            {
                FsfApproved = l.FsfApproved,
                Id          = l.Id,
                Link        = l.Link,
                Name        = l.Name,
                OsiApproved = l.OsiApproved,
                SPDX        = l.SPDX,
                Text        = l.Text
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(License viewModel, string userId)
        {
            License model = await _context.Licenses.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.FsfApproved = viewModel.FsfApproved;
            model.Link        = viewModel.Link;
            model.Name        = viewModel.Name;
            model.OsiApproved = viewModel.OsiApproved;
            model.SPDX        = viewModel.SPDX;
            model.Text        = viewModel.Text;

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<int> CreateAsync(License viewModel, string userId)
        {
            var model = new License
            {
                FsfApproved = viewModel.FsfApproved,
                Link        = viewModel.Link,
                Name        = viewModel.Name,
                OsiApproved = viewModel.OsiApproved,
                SPDX        = viewModel.SPDX,
                Text        = viewModel.Text
            };

            await _context.Licenses.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(int id, string userId)
        {
            License item = await _context.Licenses.FindAsync(id);

            if(item is null)
                return;

            _context.Licenses.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}