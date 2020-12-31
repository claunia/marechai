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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class CompaniesBySoftwareFamilyService
    {
        readonly MarechaiContext _context;

        public CompaniesBySoftwareFamilyService(MarechaiContext context) => _context = context;

        public async Task<List<CompanyBySoftwareFamilyViewModel>> GetBySoftwareFamily(ulong softwareFamilyId) =>
            await _context.CompaniesBySoftwareFamilies.Where(p => p.SoftwareFamilyId == softwareFamilyId).
                           Select(p => new CompanyBySoftwareFamilyViewModel
                           {
                               Id               = p.Id,
                               Company          = p.Company.Name,
                               CompanyId        = p.CompanyId,
                               RoleId           = p.RoleId,
                               Role             = p.Role.Name,
                               SoftwareFamilyId = p.SoftwareFamilyId
                           }).OrderBy(p => p.Company).ThenBy(p => p.Role).ToListAsync();

        public async Task DeleteAsync(ulong id, string userId)
        {
            CompaniesBySoftwareFamily item = await _context.CompaniesBySoftwareFamilies.FindAsync(id);

            if(item is null)
                return;

            _context.CompaniesBySoftwareFamilies.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<ulong> CreateAsync(int companyId, ulong softwareFamilyId, string roleId, string userId)
        {
            var item = new CompaniesBySoftwareFamily
            {
                CompanyId        = companyId,
                SoftwareFamilyId = softwareFamilyId,
                RoleId           = roleId
            };

            await _context.CompaniesBySoftwareFamilies.AddAsync(item);
            await _context.SaveChangesWithUserAsync(userId);

            return item.Id;
        }
    }
}