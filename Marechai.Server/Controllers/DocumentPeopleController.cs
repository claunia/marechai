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

[Route("/documents/people")]
[ApiController]
public class DocumentPeopleController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DocumentPersonDto>> GetAsync() => context.DocumentPeople.OrderBy(d => d.DisplayName)
                                                              .ThenBy(d => d.Alias)
                                                              .ThenBy(d => d.Name)
                                                              .ThenBy(d => d.Surname)
                                                              .Select(d => new DocumentPersonDto
                                                               {
                                                                   Id       = d.Id,
                                                                   Name     = d.FullName,
                                                                   Person   = d.Person.FullName,
                                                                   PersonId = d.PersonId
                                                               })
                                                              .ToListAsync();

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<DocumentPersonDto> GetAsync(int id) => context.DocumentPeople.Where(p => p.Id == id)
                                                              .Select(d => new DocumentPersonDto
                                                               {
                                                                   Id          = d.Id,
                                                                   Alias       = d.Alias,
                                                                   Name        = d.Name,
                                                                   Surname     = d.Surname,
                                                                   DisplayName = d.DisplayName,
                                                                   PersonId    = d.PersonId
                                                               })
                                                              .FirstOrDefaultAsync();

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] DocumentPersonDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        DocumentPerson model = await context.DocumentPeople.FindAsync(id);

        if(model is null) return NotFound();

        model.Alias       = dto.Alias;
        model.Name        = dto.Name;
        model.Surname     = dto.Surname;
        model.DisplayName = dto.DisplayName;
        model.PersonId    = dto.PersonId;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] DocumentPersonDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new DocumentPerson
        {
            Alias       = dto.Alias,
            Name        = dto.Name,
            Surname     = dto.Surname,
            DisplayName = dto.DisplayName,
            PersonId    = dto.PersonId
        };

        await context.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        DocumentPerson item = await context.DocumentPeople.FindAsync(id);

        if(item is null) return NotFound();

        context.DocumentPeople.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}