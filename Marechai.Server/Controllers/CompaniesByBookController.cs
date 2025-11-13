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

[Route("/books/companies")]
[ApiController]
public class CompaniesByBookController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/books/{bookId:long}/companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyByBookDto>> GetByBook(long bookId) => context.CompaniesByBooks
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
        CompaniesByBook item = await context.CompaniesByBooks.FindAsync(id);

        if(item is null) return NotFound();

        context.CompaniesByBooks.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] CompanyByBookDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var item = new CompaniesByBook
        {
            CompanyId = dto.CompanyId,
            BookId    = dto.BookId,
            RoleId    = dto.RoleId
        };

        await context.CompaniesByBooks.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }
}