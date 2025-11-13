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

[Route("/people")]
[ApiController]
public class PeopleController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonDto>> GetAsync() => context.People.OrderBy(p => p.DisplayName)
                                                      .ThenBy(p => p.Alias)
                                                      .ThenBy(p => p.Name)
                                                      .ThenBy(p => p.Surname)
                                                      .Select(p => new PersonDto
                                                       {
                                                           Id             = p.Id,
                                                           Name           = p.Name,
                                                           Surname        = p.Surname,
                                                           CountryOfBirth = p.CountryOfBirth.Name,
                                                           BirthDate      = p.BirthDate,
                                                           DeathDate      = p.DeathDate,
                                                           Webpage        = p.Webpage,
                                                           Twitter        = p.Twitter,
                                                           Facebook       = p.Facebook,
                                                           Photo          = p.Photo,
                                                           Alias          = p.Alias,
                                                           DisplayName    = p.DisplayName
                                                       })
                                                      .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<PersonDto> GetAsync(int id) => context.People.Where(p => p.Id == id)
                                                      .Select(p => new PersonDto
                                                       {
                                                           Id               = p.Id,
                                                           Name             = p.Name,
                                                           Surname          = p.Surname,
                                                           CountryOfBirthId = p.CountryOfBirthId,
                                                           BirthDate        = p.BirthDate,
                                                           DeathDate        = p.DeathDate,
                                                           Webpage          = p.Webpage,
                                                           Twitter          = p.Twitter,
                                                           Facebook         = p.Facebook,
                                                           Photo            = p.Photo,
                                                           Alias            = p.Alias,
                                                           DisplayName      = p.DisplayName
                                                       })
                                                      .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateAsync(PersonDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Person model = await context.People.FindAsync(dto.Id);

        if(model is null) return NotFound();

        model.Name             = dto.Name;
        model.Surname          = dto.Surname;
        model.CountryOfBirthId = dto.CountryOfBirthId;
        model.BirthDate        = dto.BirthDate;
        model.DeathDate        = dto.DeathDate;
        model.Webpage          = dto.Webpage;
        model.Twitter          = dto.Twitter;
        model.Facebook         = dto.Facebook;
        model.Photo            = dto.Photo;
        model.Alias            = dto.Alias;
        model.DisplayName      = dto.DisplayName;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> CreateAsync(PersonDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Person
        {
            Name             = dto.Name,
            Surname          = dto.Surname,
            CountryOfBirthId = dto.CountryOfBirthId,
            BirthDate        = dto.BirthDate,
            DeathDate        = dto.DeathDate,
            Webpage          = dto.Webpage,
            Twitter          = dto.Twitter,
            Facebook         = dto.Facebook,
            Photo            = dto.Photo,
            Alias            = dto.Alias,
            DisplayName      = dto.DisplayName
        };

        await context.People.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Person item = await context.People.FindAsync(id);

        if(item is null) return NotFound();

        context.People.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}