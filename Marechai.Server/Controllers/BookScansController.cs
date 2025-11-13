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
using Microsoft.Extensions.Localization;

namespace Marechai.Server.Controllers;

[Route("/book-scans")]
[ApiController]
public class BookScansController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<Guid>> GetGuidsByBookAsync(long bookId) =>
        await context.BookScans.Where(p => p.BookId == bookId).Select(p => p.Id).ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<BookScanDto> GetAsync(Guid id) => await context.BookScans.Where(p => p.Id == id)
                                                                            .Select(p => new BookScanDto
                                                                             {
                                                                                 Author       = p.Author,
                                                                                 BookId       = p.Book.Id,
                                                                                 ColorSpace   = p.ColorSpace,
                                                                                 Comments     = p.Comments,
                                                                                 CreationDate = p.CreationDate,
                                                                                 ExifVersion  = p.ExifVersion,
                                                                                 HorizontalResolution =
                                                                                     p.HorizontalResolution,
                                                                                 Id = p.Id,
                                                                                 ResolutionUnit =
                                                                                     p.ResolutionUnit,
                                                                                 Page = p.Page,
                                                                                 ScannerManufacturer =
                                                                                     p.ScannerManufacturer,
                                                                                 ScannerModel = p.ScannerModel,
                                                                                 SoftwareUsed = p.SoftwareUsed,
                                                                                 Type         = p.Type,
                                                                                 UploadDate   = p.UploadDate,
                                                                                 UserId       = p.UserId,
                                                                                 VerticalResolution =
                                                                                     p.VerticalResolution,
                                                                                 OriginalExtension =
                                                                                     p.OriginalExtension
                                                                             })
                                                                            .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task UpdateAsync(BookScanDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        BookScan model = await context.BookScans.FindAsync(dto.Id);

        if(model is null) return;

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
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Guid> CreateAsync(BookScanDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return null;
        var model = new BookScan
        {
            Author               = dto.Author,
            BookId               = dto.BookId,
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

        await context.BookScans.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task DeleteAsync(Guid id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        BookScan item = await context.BookScans.FindAsync(id);

        if(item is null) return;

        context.BookScans.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}
