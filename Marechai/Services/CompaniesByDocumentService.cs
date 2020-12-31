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
    public class CompaniesByDocumentService
    {
        readonly MarechaiContext _context;

        public CompaniesByDocumentService(MarechaiContext context) => _context = context;

        public async Task<List<CompanyByDocumentViewModel>> GetByDocument(long documentId) =>
            await _context.CompaniesByDocuments.Where(p => p.DocumentId == documentId).
                           Select(p => new CompanyByDocumentViewModel
                           {
                               Id         = p.Id,
                               Company    = p.Company.Name,
                               CompanyId  = p.CompanyId,
                               RoleId     = p.RoleId,
                               Role       = p.Role.Name,
                               DocumentId = p.DocumentId
                           }).OrderBy(p => p.Company).ThenBy(p => p.Role).ToListAsync();

        public async Task DeleteAsync(long id, string userId)
        {
            CompaniesByDocument item = await _context.CompaniesByDocuments.FindAsync(id);

            if(item is null)
                return;

            _context.CompaniesByDocuments.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(int companyId, long documentId, string roleId, string userId)
        {
            var item = new CompaniesByDocument
            {
                CompanyId  = companyId,
                DocumentId = documentId,
                RoleId     = roleId
            };

            await _context.CompaniesByDocuments.AddAsync(item);
            await _context.SaveChangesWithUserAsync(userId);

            return item.Id;
        }
    }
}