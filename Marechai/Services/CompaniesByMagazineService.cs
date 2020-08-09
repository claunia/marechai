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
    public class CompaniesByMagazineService
    {
        readonly MarechaiContext _context;

        public CompaniesByMagazineService(MarechaiContext context) => _context = context;

        public async Task<List<CompanyByMagazineViewModel>> GetByMagazine(long magazineId) =>
            await _context.CompaniesByMagazines.Where(p => p.MagazineId == magazineId).
                           Select(p => new CompanyByMagazineViewModel
                           {
                               Id         = p.Id,
                               Company    = p.Company.Name,
                               CompanyId  = p.CompanyId,
                               RoleId     = p.RoleId,
                               Role       = p.Role.Name,
                               MagazineId = p.MagazineId
                           }).OrderBy(p => p.Company).ThenBy(p => p.Role).ToListAsync();

        public async Task DeleteAsync(long id, string userId)
        {
            CompaniesByMagazine item = await _context.CompaniesByMagazines.FindAsync(id);

            if(item is null)
                return;

            _context.CompaniesByMagazines.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(int companyId, long magazineId, string roleId, string userId)
        {
            var item = new CompaniesByMagazine
            {
                CompanyId  = companyId,
                MagazineId = magazineId,
                RoleId     = roleId
            };

            await _context.CompaniesByMagazines.AddAsync(item);
            await _context.SaveChangesWithUserAsync(userId);

            return item.Id;
        }
    }
}