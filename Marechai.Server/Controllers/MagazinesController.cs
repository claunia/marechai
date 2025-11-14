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

[Route("/magazines")]
[ApiController]
public class MagazinesController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineDto>> GetAsync() => context.Magazines.OrderBy(b => b.NativeTitle)
                                                        .ThenBy(b => b.FirstPublication)
                                                        .ThenBy(b => b.Title)
                                                        .Select(b => new MagazineDto
                                                         {
                                                             Id               = b.Id,
                                                             Title            = b.Title,
                                                             NativeTitle      = b.NativeTitle,
                                                             FirstPublication = b.FirstPublication,
                                                             Synopsis         = b.Synopsis,
                                                             Issn             = b.Issn,
                                                             CountryId        = b.CountryId,
                                                             Country          = b.Country.Name
                                                         })
                                                        .ToListAsync();

    [HttpGet("titles")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineDto>> GetTitlesAsync() => context.Magazines.OrderBy(b => b.Title)
                                                              .ThenBy(b => b.FirstPublication)
                                                              .Select(b => new MagazineDto
                                                               {
                                                                   Id    = b.Id,
                                                                   Title = $"{b.Title} ({b.Country.Name}"
                                                               })
                                                              .ToListAsync();

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<MagazineDto> GetAsync(long id) => context.Magazines.Where(b => b.Id == id)
                                                         .Select(b => new MagazineDto
                                                          {
                                                              Id               = b.Id,
                                                              Title            = b.Title,
                                                              NativeTitle      = b.NativeTitle,
                                                              FirstPublication = b.FirstPublication,
                                                              Synopsis         = b.Synopsis,
                                                              Issn             = b.Issn,
                                                              CountryId        = b.CountryId,
                                                              Country          = b.Country.Name
                                                          })
                                                         .FirstOrDefaultAsync();

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] MagazineDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Magazine model = await context.Magazines.FindAsync(id);

        if(model is null) return NotFound();

        model.Title            = dto.Title;
        model.NativeTitle      = dto.NativeTitle;
        model.FirstPublication = dto.FirstPublication;
        model.Synopsis         = dto.Synopsis;
        model.CountryId        = dto.CountryId;
        model.Issn             = dto.Issn;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] MagazineDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Magazine
        {
            Title            = dto.Title,
            NativeTitle      = dto.NativeTitle,
            FirstPublication = dto.FirstPublication,
            Synopsis         = dto.Synopsis,
            CountryId        = dto.CountryId,
            Issn             = dto.Issn
        };

        await context.Magazines.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpGet("{id:int}/synopsis")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetSynopsisTextAsync(int id) =>
        (await context.Magazines.FirstOrDefaultAsync(d => d.Id == id))?.Synopsis;

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
        Magazine item = await context.Magazines.FindAsync(id);

        if(item is null) return NotFound();

        context.Magazines.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}