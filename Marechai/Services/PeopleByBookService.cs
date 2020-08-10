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
    public class PeopleByBookService
    {
        readonly MarechaiContext _context;

        public PeopleByBookService(MarechaiContext context) => _context = context;

        public async Task<List<PersonByBookViewModel>> GetByBook(long bookId) =>
            (await _context.PeopleByBooks.Where(p => p.BookId == bookId).Select(p => new PersonByBookViewModel
                {
                    Id          = p.Id,
                    Name        = p.Person.Name,
                    Surname     = p.Person.Surname,
                    Alias       = p.Person.Alias,
                    DisplayName = p.Person.DisplayName,
                    PersonId    = p.PersonId,
                    RoleId      = p.RoleId,
                    Role        = p.Role.Name,
                    BookId      = p.BookId
                }).ToListAsync()).OrderBy(p => p.FullName).ThenBy(p => p.Role).ToList();

        public async Task DeleteAsync(long id, string userId)
        {
            PeopleByBook item = await _context.PeopleByBooks.FindAsync(id);

            if(item is null)
                return;

            _context.PeopleByBooks.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(int personId, long bookId, string roleId, string userId)
        {
            var item = new PeopleByBook
            {
                PersonId = personId,
                BookId   = bookId,
                RoleId   = roleId
            };

            await _context.PeopleByBooks.AddAsync(item);
            await _context.SaveChangesWithUserAsync(userId);

            return item.Id;
        }
    }
}