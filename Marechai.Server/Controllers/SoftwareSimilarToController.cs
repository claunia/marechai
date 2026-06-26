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

using System;
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
///     Admin CRUD for <see cref="SoftwareSimilarTo" />. Public reads live on
///     <see cref="SoftwareController" /> (<c>GET /software/{id}/similar</c>); this controller
///     exposes only the admin write paths, mirroring the existing
///     <see cref="SoftwareExternalIdsController" /> shape. Each unordered pair is stored once,
///     normalized so the smaller id is <see cref="SoftwareSimilarTo.SoftwareId" />.
/// </summary>
[Route("/software/similar-to")]
[ApiController]
public class SoftwareSimilarToController(MarechaiContext context) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SoftwareSimilarToDto>> CreateAsync([FromBody] SoftwareSimilarToDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(dto.SoftwareId == dto.SimilarSoftwareId)
            return Problem(detail: "Software cannot be similar to itself.",
                            statusCode: StatusCodes.Status400BadRequest);

        ulong softwareId = Math.Min(dto.SoftwareId, dto.SimilarSoftwareId);
        ulong similarSoftwareId = Math.Max(dto.SoftwareId, dto.SimilarSoftwareId);

        bool bothExist = await context.Softwares.CountAsync(s => s.Id == softwareId || s.Id == similarSoftwareId) ==
                          2;

        if(!bothExist)
            return Problem(detail: "Referenced software does not exist.", statusCode: StatusCodes.Status400BadRequest);

        bool duplicate = await context.SoftwareSimilarTo
                                       .AnyAsync(e => e.SoftwareId       == softwareId &&
                                                      e.SimilarSoftwareId == similarSoftwareId);

        if(duplicate)
            return Problem(detail: "This software is already marked as similar.",
                            statusCode: StatusCodes.Status400BadRequest);

        var model = new SoftwareSimilarTo
        {
            SoftwareId       = softwareId,
            SimilarSoftwareId = similarSoftwareId
        };

        await context.SoftwareSimilarTo.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new SoftwareSimilarToDto
        {
            SoftwareId       = model.SoftwareId,
            SimilarSoftwareId = model.SimilarSoftwareId
        });
    }

    [HttpDelete("{softwareId:ulong}/{similarSoftwareId:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong softwareId, ulong similarSoftwareId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        ulong normalizedSoftwareId = Math.Min(softwareId, similarSoftwareId);
        ulong normalizedSimilarSoftwareId = Math.Max(softwareId, similarSoftwareId);

        SoftwareSimilarTo model =
            await context.SoftwareSimilarTo.FirstOrDefaultAsync(e => e.SoftwareId        == normalizedSoftwareId &&
                                                                      e.SimilarSoftwareId == normalizedSimilarSoftwareId);

        if(model is null) return NotFound();

        context.SoftwareSimilarTo.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
