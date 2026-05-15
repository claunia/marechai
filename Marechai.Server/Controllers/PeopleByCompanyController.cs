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
// Copyright © 2003-2026 Natalia Portillo
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

[Route("/people-by-company")]
[ApiController]
public class PeopleByCompanyController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/companies/{companyId:int}/people")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonByCompanyDto>> GetByCompany(int companyId) =>
        context.PeopleByCompanies.AsNoTracking()
               .Where(p => p.CompanyId == companyId)
               .OrderBy(p => p.Person.DisplayName ?? p.Person.Alias ?? (p.Person.Name + " " + p.Person.Surname))
               .ThenBy(p => p.Position)
               .ThenBy(p => p.Start)
               .Select(p => new PersonByCompanyDto
                {
                    Id          = p.Id,
                    Name        = p.Person.Name,
                    Surname     = p.Person.Surname,
                    Alias       = p.Person.Alias,
                    DisplayName = p.Person.DisplayName,
                    PersonId    = p.PersonId,
                    CompanyId   = p.CompanyId,
                    Position    = p.Position,
                    Start       = p.Start,
                    End         = p.End,
                    Ongoing     = p.Ongoing
                })
               .ToListAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] PersonByCompanyDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var item = new PeopleByCompany
        {
            PersonId  = dto.PersonId,
            CompanyId = dto.CompanyId,
            Position  = dto.Position,
            Start     = dto.Start,
            End       = dto.End,
            Ongoing   = dto.Ongoing
        };

        await context.PeopleByCompanies.AddAsync(item);
        await context.SaveChangesWithUserAsync(userId);

        return item.Id;
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] PersonByCompanyDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        PeopleByCompany item = await context.PeopleByCompanies.FindAsync(id);

        if(item is null) return NotFound();

        item.Position = dto.Position;
        item.Start    = dto.Start;
        item.End      = dto.End;
        item.Ongoing  = dto.Ongoing;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

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

        PeopleByCompany item = await context.PeopleByCompanies.FindAsync(id);

        if(item is null) return NotFound();

        context.PeopleByCompanies.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
