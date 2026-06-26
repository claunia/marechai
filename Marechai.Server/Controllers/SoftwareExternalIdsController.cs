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
///     Admin CRUD for <see cref="SoftwareExternalId" />. Public reads live on
///     <see cref="SoftwareController" /> (<c>GET /software/{id}/external-ids</c>); this controller
///     exposes only the admin write paths, mirroring the existing
///     <see cref="SoftwareAlternativeTitlesController" /> shape.
/// </summary>
[Route("/software/external-ids")]
[ApiController]
public class SoftwareExternalIdsController(MarechaiContext context) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SoftwareExternalIdDto>> CreateAsync([FromBody] SoftwareExternalIdDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(string.IsNullOrWhiteSpace(dto.ExternalId))
            return Problem(detail: "External id is required.", statusCode: StatusCodes.Status400BadRequest);

        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == dto.SoftwareId);

        if(!softwareExists)
            return Problem(detail: "Referenced software does not exist.", statusCode: StatusCodes.Status400BadRequest);

        bool siteExists = await context.ExternalSites.AnyAsync(s => s.Id == dto.ExternalSiteId);

        if(!siteExists)
            return Problem(detail: "Referenced external site does not exist.",
                            statusCode: StatusCodes.Status400BadRequest);

        string externalId = dto.ExternalId.Trim();

        bool duplicate = await context.SoftwareExternalIds.AnyAsync(e => e.ExternalSiteId == dto.ExternalSiteId &&
                                                                          e.ExternalId     == externalId);

        if(duplicate)
            return Problem(detail: "This id is already registered for that site.",
                            statusCode: StatusCodes.Status400BadRequest);

        var model = new SoftwareExternalId
        {
            SoftwareId     = dto.SoftwareId,
            ExternalSiteId = dto.ExternalSiteId,
            ExternalId     = externalId
        };

        await context.SoftwareExternalIds.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new SoftwareExternalIdDto
        {
            Id             = model.Id,
            SoftwareId     = model.SoftwareId,
            ExternalSiteId = model.ExternalSiteId,
            ExternalId     = model.ExternalId
        });
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] SoftwareExternalIdDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(string.IsNullOrWhiteSpace(dto.ExternalId))
            return Problem(detail: "External id is required.", statusCode: StatusCodes.Status400BadRequest);

        SoftwareExternalId model = await context.SoftwareExternalIds.FirstOrDefaultAsync(e => e.Id == id);

        if(model is null) return NotFound();

        string externalId = dto.ExternalId.Trim();

        bool duplicate = await context.SoftwareExternalIds.AnyAsync(e => e.ExternalSiteId == dto.ExternalSiteId &&
                                                                          e.ExternalId     == externalId &&
                                                                          e.Id             != id);

        if(duplicate)
            return Problem(detail: "This id is already registered for that site.",
                            statusCode: StatusCodes.Status400BadRequest);

        model.ExternalSiteId = dto.ExternalSiteId;
        model.ExternalId     = externalId;

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

        SoftwareExternalId model = await context.SoftwareExternalIds.FindAsync(id);

        if(model is null) return NotFound();

        context.SoftwareExternalIds.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
