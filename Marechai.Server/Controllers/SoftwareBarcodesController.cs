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
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/software/barcodes")]
[ApiController]
public class SoftwareBarcodesController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareBarcodeDto>> GetAsync() => context.SoftwareBarcodes.OrderBy(b => b.Code)
                                                               .Select(b => new SoftwareBarcodeDto
                                                                {
                                                                    Id        = b.Id,
                                                                    ReleaseId = b.ReleaseId,
                                                                    Code      = b.Code,
                                                                    Type      = b.Type
                                                                })
                                                               .ToListAsync();

    [HttpGet("/software/releases/{releaseId:ulong}/barcodes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareBarcodeDto>> GetByReleaseAsync(ulong releaseId) => context.SoftwareBarcodes
       .Where(b => b.ReleaseId == releaseId)
       .OrderBy(b => b.Code)
       .Select(b => new SoftwareBarcodeDto
        {
            Id        = b.Id,
            ReleaseId = b.ReleaseId,
            Code      = b.Code,
            Type      = b.Type
        })
       .ToListAsync();

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwareBarcodeDto> GetAsync(ulong id) => context.SoftwareBarcodes.Where(b => b.Id == id)
                                                                 .Select(b => new SoftwareBarcodeDto
                                                                  {
                                                                      Id        = b.Id,
                                                                      ReleaseId = b.ReleaseId,
                                                                      Code      = b.Code,
                                                                      Type      = b.Type
                                                                  })
                                                                 .FirstOrDefaultAsync();

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwareBarcodeDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwareBarcode model = await context.SoftwareBarcodes.FindAsync(id);

        if(model is null) return NotFound();

        model.ReleaseId = dto.ReleaseId;
        model.Code      = dto.Code;
        model.Type      = dto.Type;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwareBarcodeDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwareBarcode
        {
            ReleaseId = dto.ReleaseId,
            Code      = dto.Code,
            Type      = dto.Type
        };

        await context.SoftwareBarcodes.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwareBarcode item = await context.SoftwareBarcodes.FindAsync(id);

        if(item is null) return NotFound();

        context.SoftwareBarcodes.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
