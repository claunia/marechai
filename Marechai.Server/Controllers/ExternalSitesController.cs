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
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

/// <summary>
///     CRUD for <see cref="ExternalSite" />, the free-text catalog of websites a
///     <see cref="Software" /> entry can carry an external id for (e.g. MobyGames, IGDB). The list
///     is public so admin UIs and (eventually) public-page link rendering can read it without
///     authentication; writes are admin-only.
/// </summary>
[Route("/external-sites")]
[ApiController]
public class ExternalSitesController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<ExternalSiteDto>> GetAllAsync() =>
        context.ExternalSites.OrderBy(s => s.Name)
               .Select(s => new ExternalSiteDto
                {
                    Id          = s.Id,
                    Name        = s.Name,
                    UrlTemplate = s.UrlTemplate
                })
               .ToListAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ExternalSiteDto>> CreateAsync([FromBody] ExternalSiteDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(string.IsNullOrWhiteSpace(dto.Name))
            return Problem(detail: "Name is required.", statusCode: StatusCodes.Status400BadRequest);

        string name = dto.Name.Trim();

        bool nameExists = await context.ExternalSites.AnyAsync(s => s.Name == name);

        if(nameExists)
            return Problem(detail: "A site with this name already exists.",
                            statusCode: StatusCodes.Status400BadRequest);

        var model = new ExternalSite
        {
            Name        = name,
            UrlTemplate = string.IsNullOrWhiteSpace(dto.UrlTemplate) ? null : dto.UrlTemplate.Trim()
        };

        await context.ExternalSites.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new ExternalSiteDto
        {
            Id          = model.Id,
            Name        = model.Name,
            UrlTemplate = model.UrlTemplate
        });
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] ExternalSiteDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(string.IsNullOrWhiteSpace(dto.Name))
            return Problem(detail: "Name is required.", statusCode: StatusCodes.Status400BadRequest);

        ExternalSite model = await context.ExternalSites.FirstOrDefaultAsync(s => s.Id == id);

        if(model is null) return NotFound();

        string name = dto.Name.Trim();

        bool nameExists = await context.ExternalSites.AnyAsync(s => s.Name == name && s.Id != id);

        if(nameExists)
            return Problem(detail: "A site with this name already exists.",
                            statusCode: StatusCodes.Status400BadRequest);

        model.Name        = name;
        model.UrlTemplate = string.IsNullOrWhiteSpace(dto.UrlTemplate) ? null : dto.UrlTemplate.Trim();

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        ExternalSite model = await context.ExternalSites.FindAsync(id);

        if(model is null) return NotFound();

        context.ExternalSites.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
