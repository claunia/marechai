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
///     Admin CRUD for <see cref="SoftwareAlternativeTitle" />. Public reads live on
///     <see cref="SoftwareController" /> (<c>GET /software/{id}/alternative-titles</c>); this
///     controller exposes only the admin write paths, mirroring the existing
///     <see cref="SoftwareGenresController" /> shape.
/// </summary>
[Route("/software/alternative-titles")]
[ApiController]
public class SoftwareAlternativeTitlesController(MarechaiContext context) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SoftwareAlternativeTitleDto>> CreateAsync(
        [FromBody] SoftwareAlternativeTitleDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(string.IsNullOrWhiteSpace(dto.Title))
            return Problem(detail: "Title is required.", statusCode: StatusCodes.Status400BadRequest);

        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == dto.SoftwareId);

        if(!softwareExists)
            return Problem(detail: "Referenced software does not exist.", statusCode: StatusCodes.Status400BadRequest);

        var model = new SoftwareAlternativeTitle
        {
            SoftwareId = dto.SoftwareId,
            Title      = dto.Title.Trim(),
            Comment    = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim()
        };

        await context.SoftwareAlternativeTitles.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new SoftwareAlternativeTitleDto
        {
            Id               = model.Id,
            SoftwareId       = model.SoftwareId,
            Title            = model.Title,
            Comment          = model.Comment,
            CanonicalComment = model.Comment
        });
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] SoftwareAlternativeTitleDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(string.IsNullOrWhiteSpace(dto.Title))
            return Problem(detail: "Title is required.", statusCode: StatusCodes.Status400BadRequest);

        SoftwareAlternativeTitle model =
            await context.SoftwareAlternativeTitles.FirstOrDefaultAsync(t => t.Id == id);

        if(model is null) return NotFound();

        model.Title = dto.Title.Trim();

        // Admin edit submits the canonical English comment (DTO.CanonicalComment when
        // populated; falls back to DTO.Comment for older clients). The translation worker
        // fills in localized rows on its next sweep.
        string canonicalComment = !string.IsNullOrEmpty(dto.CanonicalComment) ? dto.CanonicalComment : dto.Comment;
        model.Comment = string.IsNullOrWhiteSpace(canonicalComment) ? null : canonicalComment.Trim();

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

        SoftwareAlternativeTitle model = await context.SoftwareAlternativeTitles.FindAsync(id);

        if(model is null) return NotFound();

        context.SoftwareAlternativeTitles.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
