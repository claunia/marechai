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
///     Admin junction CRUD for <see cref="GenreBySoftware" /> (composite PK SoftwareId+GenreId).
///     Public reads live on <see cref="SoftwareController" /> (<c>GET /software/{id}/genres</c>,
///     <c>GET /software/genres</c>); this controller exposes only the admin write paths to mirror
///     the existing <see cref="SoftwareCompanyRolesController" /> shape.
/// </summary>
[Route("/software/genres-by-software")]
[ApiController]
public class SoftwareGenresController(MarechaiContext context) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateAsync([FromBody] SoftwareGenreLinkDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        // Idempotent: if the link already exists, return Ok rather than failing on the
        // composite PK uniqueness violation. Admin UI may double-submit on slow networks.
        bool exists = await context.GenresBySoftware.AnyAsync(g => g.SoftwareId == dto.SoftwareId &&
                                                                    g.GenreId    == dto.GenreId);

        if(exists) return Ok();

        var model = new GenreBySoftware
        {
            SoftwareId = dto.SoftwareId,
            GenreId    = dto.GenreId
        };

        await context.GenresBySoftware.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{softwareId:ulong}/{genreId:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong softwareId, int genreId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        GenreBySoftware item =
            await context.GenresBySoftware.FindAsync(softwareId, genreId);

        if(item is null) return NotFound();

        context.GenresBySoftware.Remove(item);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
