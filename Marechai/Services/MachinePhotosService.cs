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

public class MachinePhotosService(MarechaiContext context)
{
    public async Task<List<Guid>> GetGuidsByMachineAsync(int machineId) =>
        await context.MachinePhotos.Where(p => p.MachineId == machineId).Select(p => p.Id).ToListAsync();

    // TODO: Get only the needed parts of ApplicationUser
    public async Task<MachinePhotoDto> GetAsync(Guid id) => await context.MachinePhotos.Where(p => p.Id == id)
                                                                         .Select(p => new MachinePhotoDto
                                                                          {
                                                                              Aperture = p.Aperture,
                                                                              Author   = p.Author,
                                                                              CameraManufacturer =
                                                                                  p.CameraManufacturer,
                                                                              CameraModel = p.CameraModel,
                                                                              ColorSpace =
                                                                                  (ushort?)p.ColorSpace,
                                                                              Comments = p.Comments,
                                                                              Contrast =
                                                                                  (ushort?)p.Contrast,
                                                                              CreationDate = p.CreationDate,
                                                                              DigitalZoomRatio =
                                                                                  p.DigitalZoomRatio,
                                                                              ExifVersion  = p.ExifVersion,
                                                                              ExposureTime = p.ExposureTime,
                                                                              ExposureMethod =
                                                                                  (ushort?)p.ExposureMethod,
                                                                              ExposureProgram =
                                                                                  (ushort?)p.ExposureProgram,
                                                                              Flash       = (ushort?)p.Flash,
                                                                              Focal       = p.Focal,
                                                                              FocalLength = p.FocalLength,
                                                                              FocalLengthEquivalent =
                                                                                  p.FocalLengthEquivalent,
                                                                              HorizontalResolution =
                                                                                  p.HorizontalResolution,
                                                                              Id          = p.Id,
                                                                              IsoRating   = p.IsoRating,
                                                                              Lens        = p.Lens,
                                                                              LicenseId   = p.LicenseId,
                                                                              LicenseName = p.License.Name,
                                                                              LightSource =
                                                                                  (ushort?)p.LightSource,
                                                                              MachineCompanyName =
                                                                                  p.Machine.Company.Name,
                                                                              MachineId   = p.MachineId,
                                                                              MachineName = p.Machine.Name,
                                                                              MeteringMode =
                                                                                  (ushort?)p.MeteringMode,
                                                                              ResolutionUnit =
                                                                                  (ushort?)p.ResolutionUnit,
                                                                              Orientation =
                                                                                  (ushort?)p.Orientation,
                                                                              Saturation =
                                                                                  (ushort?)p.Saturation,
                                                                              SceneCaptureType =
                                                                                  (ushort?)p.SceneCaptureType,
                                                                              SensingMethod =
                                                                                  (ushort?)p.SensingMethod,
                                                                              Sharpness =
                                                                                  (ushort?)p.Sharpness,
                                                                              SoftwareUsed = p.SoftwareUsed,
                                                                              Source       = p.Source,
                                                                              SubjectDistanceRange =
                                                                                  (byte?)p.SubjectDistanceRange,
                                                                              UploadDate = p.UploadDate,
                                                                              UserId     = p.UserId,
                                                                              VerticalResolution =
                                                                                  p.VerticalResolution,
                                                                              WhiteBalance =
                                                                                  (ushort?)p.WhiteBalance,
                                                                              OriginalExtension =
                                                                                  p.OriginalExtension
                                                                          })
                                                                         .FirstOrDefaultAsync();

    public async Task UpdateAsync(MachinePhotoDto dto, string userId)
    {
        MachinePhoto model = await context.MachinePhotos.FindAsync(dto.Id);

        if(model is null) return;

        model.Aperture = dto.Aperture;
        model.Author = dto.Author;
        model.CameraManufacturer = dto.CameraManufacturer;
        model.CameraModel = dto.CameraModel;
        model.ColorSpace = dto.ColorSpace.HasValue ? (ColorSpace)dto.ColorSpace.Value : null;
        model.Comments = dto.Comments;
        model.Contrast = dto.Contrast.HasValue ? (Contrast)dto.Contrast.Value : null;
        model.CreationDate = dto.CreationDate;
        model.DigitalZoomRatio = dto.DigitalZoomRatio;
        model.ExifVersion = dto.ExifVersion;
        model.ExposureTime = dto.ExposureTime;
        model.ExposureMethod = dto.ExposureMethod.HasValue ? (ExposureMode)dto.ExposureMethod.Value : null;
        model.ExposureProgram = dto.ExposureProgram.HasValue ? (ExposureProgram)dto.ExposureProgram.Value : null;
        model.Flash = dto.Flash.HasValue ? (Flash)dto.Flash.Value : null;
        model.Focal = dto.Focal;
        model.FocalLength = dto.FocalLength;
        model.FocalLengthEquivalent = dto.FocalLengthEquivalent;
        model.HorizontalResolution = dto.HorizontalResolution;
        model.IsoRating = dto.IsoRating;
        model.Lens = dto.Lens;
        model.LicenseId = dto.LicenseId;
        model.LightSource = dto.LightSource.HasValue ? (LightSource)dto.LightSource.Value : null;
        model.MeteringMode = dto.MeteringMode.HasValue ? (MeteringMode)dto.MeteringMode.Value : null;
        model.ResolutionUnit = dto.ResolutionUnit.HasValue ? (ResolutionUnit)dto.ResolutionUnit.Value : null;
        model.Orientation = dto.Orientation.HasValue ? (Orientation)dto.Orientation.Value : null;
        model.Saturation = dto.Saturation.HasValue ? (Saturation)dto.Saturation.Value : null;
        model.SceneCaptureType = dto.SceneCaptureType.HasValue ? (SceneCaptureType)dto.SceneCaptureType.Value : null;
        model.SensingMethod = dto.SensingMethod.HasValue ? (SensingMethod)dto.SensingMethod.Value : null;
        model.Sharpness = dto.Sharpness.HasValue ? (Sharpness)dto.Sharpness.Value : null;
        model.SoftwareUsed = dto.SoftwareUsed;
        model.Source = dto.Source;

        model.SubjectDistanceRange = dto.SubjectDistanceRange.HasValue
                                         ? (SubjectDistanceRange)dto.SubjectDistanceRange.Value
                                         : null;

        model.VerticalResolution = dto.VerticalResolution;
        model.WhiteBalance       = dto.WhiteBalance.HasValue ? (WhiteBalance)dto.WhiteBalance.Value : null;

        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<Guid> CreateAsync(MachinePhotoDto dto, string userId)
    {
        var model = new MachinePhoto
        {
            Aperture              = dto.Aperture,
            Author                = dto.Author,
            CameraManufacturer    = dto.CameraManufacturer,
            CameraModel           = dto.CameraModel,
            ColorSpace            = dto.ColorSpace.HasValue ? (ColorSpace)dto.ColorSpace.Value : null,
            Comments              = dto.Comments,
            Contrast              = dto.Contrast.HasValue ? (Contrast)dto.Contrast.Value : null,
            CreationDate          = dto.CreationDate,
            DigitalZoomRatio      = dto.DigitalZoomRatio,
            ExifVersion           = dto.ExifVersion,
            ExposureTime          = dto.ExposureTime,
            ExposureMethod        = dto.ExposureMethod.HasValue ? (ExposureMode)dto.ExposureMethod.Value : null,
            ExposureProgram       = dto.ExposureProgram.HasValue ? (ExposureProgram)dto.ExposureProgram.Value : null,
            Flash                 = dto.Flash.HasValue ? (Flash)dto.Flash.Value : null,
            Focal                 = dto.Focal,
            FocalLength           = dto.FocalLength,
            FocalLengthEquivalent = dto.FocalLengthEquivalent,
            HorizontalResolution  = dto.HorizontalResolution,
            Id                    = dto.Id,
            IsoRating             = dto.IsoRating,
            Lens                  = dto.Lens,
            LicenseId             = dto.LicenseId,
            LightSource           = dto.LightSource.HasValue ? (LightSource)dto.LightSource.Value : null,
            MachineId             = dto.MachineId,
            MeteringMode          = dto.MeteringMode.HasValue ? (MeteringMode)dto.MeteringMode.Value : null,
            ResolutionUnit        = dto.ResolutionUnit.HasValue ? (ResolutionUnit)dto.ResolutionUnit.Value : null,
            Orientation           = dto.Orientation.HasValue ? (Orientation)dto.Orientation.Value : null,
            Saturation            = dto.Saturation.HasValue ? (Saturation)dto.Saturation.Value : null,
            SceneCaptureType      = dto.SceneCaptureType.HasValue ? (SceneCaptureType)dto.SceneCaptureType.Value : null,
            SensingMethod         = dto.SensingMethod.HasValue ? (SensingMethod)dto.SensingMethod.Value : null,
            Sharpness             = dto.Sharpness.HasValue ? (Sharpness)dto.Sharpness.Value : null,
            SoftwareUsed          = dto.SoftwareUsed,
            Source                = dto.Source,
            SubjectDistanceRange =
                dto.SubjectDistanceRange.HasValue ? (SubjectDistanceRange)dto.SubjectDistanceRange.Value : null,
            UploadDate         = dto.UploadDate,
            UserId             = dto.UserId,
            VerticalResolution = dto.VerticalResolution,
            WhiteBalance       = dto.WhiteBalance.HasValue ? (WhiteBalance)dto.WhiteBalance.Value : null,
            OriginalExtension  = dto.OriginalExtension
        };

        await context.MachinePhotos.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }
}