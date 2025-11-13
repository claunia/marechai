/*******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ---------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] -----------------------------------------------------------
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
// ---------------------------------------------------------------------------
// Copyright © 2003-2025 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/people-by-book")]
[ApiController]
public class PeopleByBookController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/books/{bookId:int}/people")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<PersonByBookDto>> GetByBook(long bookId) => (await context.PeopleByBooks
                                                                           .Where(p => p.BookId == bookId)
                                                                           .Select(p => new PersonByBookDto
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
                                                                            })
                                                                           .ToListAsync()).OrderBy(p => p.FullName)
       .ThenBy(p => p.Role)
       .ToList();

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        PeopleByBook item = await context.PeopleByBooks.FindAsync(id);

        if(item is null) return NotFound();

        context.PeopleByBooks.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] PersonByBookDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var item = new PeopleByBook
        {
            PersonId = dto.PersonId,
            BookId   = dto.BookId,
            RoleId   = dto.RoleId
        };

        await context.PeopleByBooks.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }
}