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
    public class SoftwareVersionsService
    {
        readonly MarechaiContext _context;

        public SoftwareVersionsService(MarechaiContext context) => _context = context;

        public async Task<List<SoftwareVersionViewModel>> GetAsync() => await _context.
                                                                              SoftwareVersions.
                                                                              OrderBy(b => b.Family.Name).
                                                                              ThenBy(b => b.Version).
                                                                              ThenBy(b => b.Introduced).
                                                                              Select(b => new SoftwareVersionViewModel
                                                                              {
                                                                                  Id         = b.Id,
                                                                                  Family     = b.Family.Name,
                                                                                  Name       = b.Name,
                                                                                  Codename   = b.Codename,
                                                                                  Version    = b.Version,
                                                                                  Introduced = b.Introduced,
                                                                                  Previous   = b.Previous.Name,
                                                                                  License    = b.License.Name,
                                                                                  FamilyId   = b.FamilyId,
                                                                                  LicenseId  = b.LicenseId,
                                                                                  PreviousId = b.PreviousId
                                                                              }).ToListAsync();

        public async Task<SoftwareVersionViewModel> GetAsync(ulong id) =>
            await _context.SoftwareVersions.Where(b => b.Id == id).Select(b => new SoftwareVersionViewModel
            {
                Id         = b.Id,
                Family     = b.Family.Name,
                Name       = b.Name,
                Codename   = b.Codename,
                Version    = b.Version,
                Introduced = b.Introduced,
                Previous   = b.Previous.Name,
                License    = b.License.Name,
                FamilyId   = b.FamilyId,
                LicenseId  = b.LicenseId,
                PreviousId = b.PreviousId
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(SoftwareVersionViewModel viewModel, string userId)
        {
            SoftwareVersion model = await _context.SoftwareVersions.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Name       = viewModel.Name;
            model.Codename   = viewModel.Codename;
            model.Version    = viewModel.Version;
            model.Introduced = viewModel.Introduced;
            model.FamilyId   = viewModel.FamilyId;
            model.LicenseId  = viewModel.LicenseId;
            model.PreviousId = viewModel.PreviousId;
            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<ulong> CreateAsync(SoftwareVersionViewModel viewModel, string userId)
        {
            var model = new SoftwareVersion
            {
                Name       = viewModel.Name,
                Codename   = viewModel.Codename,
                Version    = viewModel.Version,
                Introduced = viewModel.Introduced,
                FamilyId   = viewModel.FamilyId,
                LicenseId  = viewModel.LicenseId,
                PreviousId = viewModel.PreviousId
            };

            await _context.SoftwareVersions.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(ulong id, string userId)
        {
            SoftwareVersion item = await _context.SoftwareVersions.FindAsync(id);

            if(item is null)
                return;

            _context.SoftwareVersions.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}