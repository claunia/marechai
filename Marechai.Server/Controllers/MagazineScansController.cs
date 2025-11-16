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
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/magazines/scans")]
[ApiController]
public class MagazineScansController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/magazines/{magazineId:long}/scans")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<Guid>> GetGuidsByMagazineAsync(long magazineId) => context.MagazineScans
       .Where(p => p.MagazineId == magazineId)
       .Select(p => p.Id)
       .ToListAsync();

    [HttpGet("{id:Guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<MagazineScanDto> GetAsync(Guid id) => context.MagazineScans.Where(p => p.Id == id)
                                                             .Select(p => new MagazineScanDto
                                                              {
                                                                  Author               = p.Author,
                                                                  MagazineId           = p.Magazine.Id,
                                                                  ColorSpace           = (ushort?)p.ColorSpace,
                                                                  Comments             = p.Comments,
                                                                  CreationDate         = p.CreationDate,
                                                                  ExifVersion          = p.ExifVersion,
                                                                  HorizontalResolution = p.HorizontalResolution,
                                                                  Id                   = p.Id,
                                                                  ResolutionUnit       = (ushort?)p.ResolutionUnit,
                                                                  Page                 = p.Page,
                                                                  ScannerManufacturer  = p.ScannerManufacturer,
                                                                  ScannerModel         = p.ScannerModel,
                                                                  SoftwareUsed         = p.SoftwareUsed,
                                                                  Type                 = p.Type,
                                                                  UploadDate           = p.UploadDate,
                                                                  UserId               = p.UserId,
                                                                  VerticalResolution   = p.VerticalResolution,
                                                                  OriginalExtension    = p.OriginalExtension
                                                              })
                                                             .FirstOrDefaultAsync();

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] MagazineScanDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        MagazineScan model = await context.MagazineScans.FindAsync(id);

        if(model is null) return NotFound();

        model.Author               = dto.Author;
        model.ColorSpace           = dto.ColorSpace.HasValue ? (ColorSpace)dto.ColorSpace.Value : null;
        model.Comments             = dto.Comments;
        model.CreationDate         = dto.CreationDate;
        model.ExifVersion          = dto.ExifVersion;
        model.HorizontalResolution = dto.HorizontalResolution;
        model.ResolutionUnit       = dto.ResolutionUnit.HasValue ? (ResolutionUnit)dto.ResolutionUnit.Value : null;
        model.Page                 = dto.Page;
        model.ScannerManufacturer  = dto.ScannerManufacturer;
        model.ScannerModel         = dto.ScannerModel;
        model.Type                 = dto.Type;
        model.SoftwareUsed         = dto.SoftwareUsed;
        model.VerticalResolution   = dto.VerticalResolution;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> CreateAsync([FromBody] MagazineScanDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new MagazineScan
        {
            Author               = dto.Author,
            MagazineId           = dto.MagazineId,
            ColorSpace           = dto.ColorSpace.HasValue ? (ColorSpace)dto.ColorSpace.Value : null,
            Comments             = dto.Comments,
            CreationDate         = dto.CreationDate,
            ExifVersion          = dto.ExifVersion,
            HorizontalResolution = dto.HorizontalResolution,
            Id                   = dto.Id,
            ResolutionUnit       = dto.ResolutionUnit.HasValue ? (ResolutionUnit)dto.ResolutionUnit.Value : null,
            Page                 = dto.Page,
            ScannerManufacturer  = dto.ScannerManufacturer,
            ScannerModel         = dto.ScannerModel,
            Type                 = dto.Type,
            SoftwareUsed         = dto.SoftwareUsed,
            UploadDate           = dto.UploadDate,
            UserId               = dto.UserId,
            VerticalResolution   = dto.VerticalResolution,
            OriginalExtension    = dto.OriginalExtension
        };

        await context.MagazineScans.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete("{id:Guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        MagazineScan item = await context.MagazineScans.FindAsync(id);

        if(item is null) return NotFound();

        context.MagazineScans.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}