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

[Route("/document-scans")]
[ApiController]
public class DocumentScansController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<Guid>> GetGuidsByDocumentAsync(long bookId) =>
        await context.DocumentScans.Where(p => p.DocumentId == bookId).Select(p => p.Id).ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<DocumentScanDto> GetAsync(Guid id) => context.DocumentScans.Where(p => p.Id == id)
                                                             .Select(p => new DocumentScanDto
                                                              {
                                                                  Author               = p.Author,
                                                                  DocumentId           = p.Document.Id,
                                                                  ColorSpace           = p.ColorSpace,
                                                                  Comments             = p.Comments,
                                                                  CreationDate         = p.CreationDate,
                                                                  ExifVersion          = p.ExifVersion,
                                                                  HorizontalResolution = p.HorizontalResolution,
                                                                  Id                   = p.Id,
                                                                  ResolutionUnit       = p.ResolutionUnit,
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

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync([FromBody] DocumentScanDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        DocumentScan model = await context.DocumentScans.FindAsync(dto.Id);

        if(model is null) return NotFound();

        model.Author               = dto.Author;
        model.ColorSpace           = dto.ColorSpace;
        model.Comments             = dto.Comments;
        model.CreationDate         = dto.CreationDate;
        model.ExifVersion          = dto.ExifVersion;
        model.HorizontalResolution = dto.HorizontalResolution;
        model.ResolutionUnit       = dto.ResolutionUnit;
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
    public async Task<ActionResult<Guid>> CreateAsync([FromBody] DocumentScanDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new DocumentScan
        {
            Author               = dto.Author,
            DocumentId           = dto.DocumentId,
            ColorSpace           = dto.ColorSpace,
            Comments             = dto.Comments,
            CreationDate         = dto.CreationDate,
            ExifVersion          = dto.ExifVersion,
            HorizontalResolution = dto.HorizontalResolution,
            Id                   = dto.Id,
            ResolutionUnit       = dto.ResolutionUnit,
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

        await context.DocumentScans.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        DocumentScan item = await context.DocumentScans.FindAsync(id);

        if(item is null) return NotFound();

        context.DocumentScans.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}