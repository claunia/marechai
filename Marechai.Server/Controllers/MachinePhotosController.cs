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

[Route("/machine-photos")]
[ApiController]
public class MachinePhotosController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<Guid>> GetGuidsByMachineAsync(int machineId) =>
        await context.MachinePhotos.Where(p => p.MachineId == machineId).Select(p => p.Id).ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<MachinePhotoDto> GetAsync(Guid id) => context.MachinePhotos.Where(p => p.Id == id)
                                                             .Select(p => new MachinePhotoDto
                                                              {
                                                                  Aperture              = p.Aperture,
                                                                  Author                = p.Author,
                                                                  CameraManufacturer    = p.CameraManufacturer,
                                                                  CameraModel           = p.CameraModel,
                                                                  ColorSpace            = p.ColorSpace,
                                                                  Comments              = p.Comments,
                                                                  Contrast              = p.Contrast,
                                                                  CreationDate          = p.CreationDate,
                                                                  DigitalZoomRatio      = p.DigitalZoomRatio,
                                                                  ExifVersion           = p.ExifVersion,
                                                                  ExposureTime          = p.ExposureTime,
                                                                  ExposureMethod        = p.ExposureMethod,
                                                                  ExposureProgram       = p.ExposureProgram,
                                                                  Flash                 = p.Flash,
                                                                  Focal                 = p.Focal,
                                                                  FocalLength           = p.FocalLength,
                                                                  FocalLengthEquivalent = p.FocalLengthEquivalent,
                                                                  HorizontalResolution  = p.HorizontalResolution,
                                                                  Id                    = p.Id,
                                                                  IsoRating             = p.IsoRating,
                                                                  Lens                  = p.Lens,
                                                                  LicenseId             = p.LicenseId,
                                                                  LicenseName           = p.License.Name,
                                                                  LightSource           = p.LightSource,
                                                                  MachineCompanyName    = p.Machine.Company.Name,
                                                                  MachineId             = p.MachineId,
                                                                  MachineName           = p.Machine.Name,
                                                                  MeteringMode          = p.MeteringMode,
                                                                  ResolutionUnit        = p.ResolutionUnit,
                                                                  Orientation           = p.Orientation,
                                                                  Saturation            = p.Saturation,
                                                                  SceneCaptureType      = p.SceneCaptureType,
                                                                  SensingMethod         = p.SensingMethod,
                                                                  Sharpness             = p.Sharpness,
                                                                  SoftwareUsed          = p.SoftwareUsed,
                                                                  Source                = p.Source,
                                                                  SubjectDistanceRange  = p.SubjectDistanceRange,
                                                                  UploadDate            = p.UploadDate,
                                                                  UserId                = p.UserId,
                                                                  VerticalResolution    = p.VerticalResolution,
                                                                  WhiteBalance          = p.WhiteBalance,
                                                                  OriginalExtension     = p.OriginalExtension
                                                              })
                                                             .FirstOrDefaultAsync();

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] MachinePhotoDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        MachinePhoto model = await context.MachinePhotos.FindAsync(id);

        if(model is null) return NotFound();

        model.Aperture              = dto.Aperture;
        model.Author                = dto.Author;
        model.CameraManufacturer    = dto.CameraManufacturer;
        model.CameraModel           = dto.CameraModel;
        model.ColorSpace            = dto.ColorSpace;
        model.Comments              = dto.Comments;
        model.Contrast              = dto.Contrast;
        model.CreationDate          = dto.CreationDate;
        model.DigitalZoomRatio      = dto.DigitalZoomRatio;
        model.ExifVersion           = dto.ExifVersion;
        model.ExposureTime          = dto.ExposureTime;
        model.ExposureMethod        = dto.ExposureMethod;
        model.ExposureProgram       = dto.ExposureProgram;
        model.Flash                 = dto.Flash;
        model.Focal                 = dto.Focal;
        model.FocalLength           = dto.FocalLength;
        model.FocalLengthEquivalent = dto.FocalLengthEquivalent;
        model.HorizontalResolution  = dto.HorizontalResolution;
        model.IsoRating             = dto.IsoRating;
        model.Lens                  = dto.Lens;
        model.LicenseId             = dto.LicenseId;
        model.LightSource           = dto.LightSource;
        model.MeteringMode          = dto.MeteringMode;
        model.ResolutionUnit        = dto.ResolutionUnit;
        model.Orientation           = dto.Orientation;
        model.Saturation            = dto.Saturation;
        model.SceneCaptureType      = dto.SceneCaptureType;
        model.SensingMethod         = dto.SensingMethod;
        model.Sharpness             = dto.Sharpness;
        model.SoftwareUsed          = dto.SoftwareUsed;
        model.Source                = dto.Source;
        model.SubjectDistanceRange  = dto.SubjectDistanceRange;
        model.VerticalResolution    = dto.VerticalResolution;
        model.WhiteBalance          = dto.WhiteBalance;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> CreateAsync([FromBody] MachinePhotoDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new MachinePhoto
        {
            Aperture              = dto.Aperture,
            Author                = dto.Author,
            CameraManufacturer    = dto.CameraManufacturer,
            CameraModel           = dto.CameraModel,
            ColorSpace            = dto.ColorSpace,
            Comments              = dto.Comments,
            Contrast              = dto.Contrast,
            CreationDate          = dto.CreationDate,
            DigitalZoomRatio      = dto.DigitalZoomRatio,
            ExifVersion           = dto.ExifVersion,
            ExposureTime          = dto.ExposureTime,
            ExposureMethod        = dto.ExposureMethod,
            ExposureProgram       = dto.ExposureProgram,
            Flash                 = dto.Flash,
            Focal                 = dto.Focal,
            FocalLength           = dto.FocalLength,
            FocalLengthEquivalent = dto.FocalLengthEquivalent,
            HorizontalResolution  = dto.HorizontalResolution,
            Id                    = dto.Id,
            IsoRating             = dto.IsoRating,
            Lens                  = dto.Lens,
            LicenseId             = dto.LicenseId,
            LightSource           = dto.LightSource,
            MachineId             = dto.MachineId,
            MeteringMode          = dto.MeteringMode,
            ResolutionUnit        = dto.ResolutionUnit,
            Orientation           = dto.Orientation,
            Saturation            = dto.Saturation,
            SceneCaptureType      = dto.SceneCaptureType,
            SensingMethod         = dto.SensingMethod,
            Sharpness             = dto.Sharpness,
            SoftwareUsed          = dto.SoftwareUsed,
            Source                = dto.Source,
            SubjectDistanceRange  = dto.SubjectDistanceRange,
            UploadDate            = dto.UploadDate,
            UserId                = dto.UserId,
            VerticalResolution    = dto.VerticalResolution,
            WhiteBalance          = dto.WhiteBalance,
            OriginalExtension     = dto.OriginalExtension
        };

        await context.MachinePhotos.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }
}