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
    public class DocumentRolesService
    {
        readonly MarechaiContext _context;

        public DocumentRolesService(MarechaiContext context) => _context = context;

        public async Task<List<DocumentRoleViewModel>> GetAsync() => await _context.DocumentRoles.
                                                                         OrderBy(c => c.Name).
                                                                         Select(c => new DocumentRoleViewModel
                                                                         {
                                                                             Id      = c.Id,
                                                                             Name    = c.Name,
                                                                             Enabled = c.Enabled
                                                                         }).ToListAsync();

        public async Task<List<DocumentRoleViewModel>> GetEnabledAsync() =>
            await _context.DocumentRoles.Where(c => c.Enabled).OrderBy(c => c.Name).
                           Select(c => new DocumentRoleViewModel
                           {
                               Id      = c.Id,
                               Name    = c.Name,
                               Enabled = c.Enabled
                           }).ToListAsync();

        public async Task<DocumentRoleViewModel> GetAsync(string id) =>
            await _context.DocumentRoles.Where(c => c.Id == id).Select(c => new DocumentRoleViewModel
            {
                Id      = c.Id,
                Name    = c.Name,
                Enabled = c.Enabled
            }).FirstOrDefaultAsync();
    }
}