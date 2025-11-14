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
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services;

public class PeopleByMagazineService(MarechaiContext context)
{
    public async Task<List<PersonByMagazineDto>> GetByMagazine(long magazineId) => (await context
                                                                                               .PeopleByMagazines.Where(p => p.MagazineId == magazineId)
                                                                                               .Select(p => new PersonByMagazineDto
                                                                                                {
                                                                                                    Id          = p.Id,
                                                                                                    Name        = p.Person.Name,
                                                                                                    Surname     = p.Person.Surname,
                                                                                                    Alias       = p.Person.Alias,
                                                                                                    DisplayName = p.Person.DisplayName,
                                                                                                    PersonId    = p.PersonId,
                                                                                                    RoleId      = p.RoleId,
                                                                                                    Role        = p.Role.Name,
                                                                                                    MagazineId  = p.MagazineId
                                                                                                })
                                                                                               .ToListAsync()).OrderBy(p => p.FullName)
                                                                                                              .ThenBy(p => p.Role)
                                                                                                              .ToList();

    public async Task DeleteAsync(long id, string userId)
    {
        PeopleByMagazine item = await context.PeopleByMagazines.FindAsync(id);

        if(item is null) return;

        context.PeopleByMagazines.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<long> CreateAsync(int personId, long magazineId, string roleId, string userId)
    {
        var item = new PeopleByMagazine
        {
            PersonId   = personId,
            MagazineId = magazineId,
            RoleId     = roleId
        };

        await context.PeopleByMagazines.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }
}