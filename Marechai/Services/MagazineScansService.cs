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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services;

public class MagazineScansService(MarechaiContext context)
{
    public async Task<List<Guid>> GetGuidsByMagazineAsync(long bookId) =>
        await context.MagazineScans.Where(p => p.MagazineId == bookId).Select(p => p.Id).ToListAsync();

    public async Task<MagazineScanDto> GetAsync(Guid id) => await context.MagazineScans.Where(p => p.Id == id)
                                                                         .Select(p => new MagazineScanDto
                                                                          {
                                                                              Author     = p.Author,
                                                                              MagazineId = p.Magazine.Id,
                                                                              ColorSpace =
                                                                                  (ushort?)p.ColorSpace,
                                                                              Comments     = p.Comments,
                                                                              CreationDate = p.CreationDate,
                                                                              ExifVersion  = p.ExifVersion,
                                                                              HorizontalResolution =
                                                                                  p.HorizontalResolution,
                                                                              Id = p.Id,
                                                                              ResolutionUnit =
                                                                                  (ushort?)p.ResolutionUnit,
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
                                                                              OriginalExtension = p.OriginalExtension
                                                                          })
                                                                         .FirstOrDefaultAsync();

    public async Task UpdateAsync(MagazineScanDto dto, string userId)
    {
        MagazineScan model = await context.MagazineScans.FindAsync(dto.Id);

        if(model is null) return;

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
    }

    public async Task<Guid> CreateAsync(MagazineScanDto dto, string userId)
    {
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

    public async Task DeleteAsync(Guid id, string userId)
    {
        MagazineScan item = await context.MagazineScans.FindAsync(id);

        if(item is null) return;

        context.MagazineScans.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}