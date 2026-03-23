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

using System;
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

[Route("/software/releases")]
[ApiController]
public class SoftwareReleasesController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareReleaseDto>> GetAsync() => context.SoftwareReleases
                                                               .OrderBy(r => r.SoftwareVersion.Software.Name)
                                                               .ThenBy(r => r.SoftwareVersion.VersionString)
                                                               .Select(r => new SoftwareReleaseDto
                                                                {
                                                                    Id                = r.Id,
                                                                    SoftwareVersionId = r.SoftwareVersionId,
                                                                    SoftwareVersion   = r.SoftwareVersion.VersionString,
                                                                    VariantId         = r.VariantId,
                                                                    Variant           = r.Variant.Name,
                                                                    SubvariantId      = r.SubvariantId,
                                                                    Subvariant        = r.Subvariant.Name,
                                                                    PlatformId        = r.PlatformId,
                                                                    Platform          = r.Platform.Name,
                                                                    RegionId          = r.RegionId,
                                                                    Region            = r.Region.Name,
                                                                    PublisherId       = r.PublisherId,
                                                                    Publisher         = r.Publisher.Name,
                                                                    ReleaseDate       = r.ReleaseDate
                                                                })
                                                               .ToListAsync();

    [HttpGet("/software/versions/{versionId:ulong}/releases")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareReleaseDto>> GetByVersionAsync(ulong versionId) => context.SoftwareReleases
       .Where(r => r.SoftwareVersionId == versionId)
       .OrderBy(r => r.ReleaseDate)
       .Select(r => new SoftwareReleaseDto
        {
            Id                = r.Id,
            SoftwareVersionId = r.SoftwareVersionId,
            SoftwareVersion   = r.SoftwareVersion.VersionString,
            VariantId         = r.VariantId,
            Variant           = r.Variant.Name,
            SubvariantId      = r.SubvariantId,
            Subvariant        = r.Subvariant.Name,
            PlatformId        = r.PlatformId,
            Platform          = r.Platform.Name,
            RegionId          = r.RegionId,
            Region            = r.Region.Name,
            PublisherId       = r.PublisherId,
            Publisher         = r.Publisher.Name,
            ReleaseDate       = r.ReleaseDate
        })
       .ToListAsync();

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwareReleaseDto> GetAsync(ulong id) => context.SoftwareReleases.Where(r => r.Id == id)
                                                                 .Select(r => new SoftwareReleaseDto
                                                                  {
                                                                      Id                = r.Id,
                                                                      SoftwareVersionId = r.SoftwareVersionId,
                                                                      SoftwareVersion   = r.SoftwareVersion.VersionString,
                                                                      VariantId         = r.VariantId,
                                                                      Variant           = r.Variant.Name,
                                                                      SubvariantId      = r.SubvariantId,
                                                                      Subvariant        = r.Subvariant.Name,
                                                                      PlatformId        = r.PlatformId,
                                                                      Platform          = r.Platform.Name,
                                                                      RegionId          = r.RegionId,
                                                                      Region            = r.Region.Name,
                                                                      PublisherId       = r.PublisherId,
                                                                      Publisher         = r.Publisher.Name,
                                                                      ReleaseDate       = r.ReleaseDate
                                                                  })
                                                                 .FirstOrDefaultAsync();

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwareReleaseDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwareRelease model = await context.SoftwareReleases.FindAsync(id);

        if(model is null) return NotFound();

        model.SoftwareVersionId = dto.SoftwareVersionId;
        model.VariantId         = dto.VariantId;
        model.SubvariantId      = dto.SubvariantId;
        model.PlatformId        = dto.PlatformId;
        model.RegionId          = dto.RegionId;
        model.PublisherId       = dto.PublisherId;
        model.ReleaseDate       = dto.ReleaseDate;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwareReleaseDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwareRelease
        {
            SoftwareVersionId = dto.SoftwareVersionId,
            VariantId         = dto.VariantId,
            SubvariantId      = dto.SubvariantId,
            PlatformId        = dto.PlatformId,
            RegionId          = dto.RegionId,
            PublisherId       = dto.PublisherId,
            ReleaseDate       = dto.ReleaseDate
        };

        await context.SoftwareReleases.AddAsync(model);
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
        SoftwareRelease item = await context.SoftwareReleases.FindAsync(id);

        if(item is null) return NotFound();

        context.SoftwareReleases.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}
