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
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services;

public class CompaniesByBookService(MarechaiContext context)
{
    public async Task<List<CompanyByBookDto>> GetByBook(long bookId) => await context.CompaniesByBooks
                                                                                           .Where(p => p.BookId == bookId)
                                                                                           .Select(p => new CompanyByBookDto
                                                                                            {
                                                                                                Id        = p.Id,
                                                                                                Company   = p.Company.Name,
                                                                                                CompanyId = p.CompanyId,
                                                                                                RoleId    = p.RoleId,
                                                                                                Role      = p.Role.Name,
                                                                                                BookId    = p.BookId
                                                                                            })
                                                                                           .OrderBy(p => p.Company)
                                                                                           .ThenBy(p => p.Role)
                                                                                           .ToListAsync();

    public async Task DeleteAsync(long id, string userId)
    {
        CompaniesByBook item = await context.CompaniesByBooks.FindAsync(id);

        if(item is null) return;

        context.CompaniesByBooks.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<long> CreateAsync(int companyId, long bookId, string roleId, string userId)
    {
        var item = new CompaniesByBook
        {
            CompanyId = companyId,
            BookId    = bookId,
            RoleId    = roleId
        };

        await context.CompaniesByBooks.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }
}