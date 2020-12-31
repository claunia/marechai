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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class MachinePhotosService
    {
        readonly MarechaiContext _context;

        public MachinePhotosService(MarechaiContext context) => _context = context;

        public async Task<List<Guid>> GetGuidsByMachineAsync(int machineId) =>
            await _context.MachinePhotos.Where(p => p.MachineId == machineId).Select(p => p.Id).ToListAsync();

        // TODO: Get only the needed parts of ApplicationUser
        public async Task<MachinePhotoViewModel> GetAsync(Guid id) =>
            await _context.MachinePhotos.Where(p => p.Id == id).Select(p => new MachinePhotoViewModel
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
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(MachinePhotoViewModel viewModel, string userId)
        {
            MachinePhoto model = await _context.MachinePhotos.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Aperture              = viewModel.Aperture;
            model.Author                = viewModel.Author;
            model.CameraManufacturer    = viewModel.CameraManufacturer;
            model.CameraModel           = viewModel.CameraModel;
            model.ColorSpace            = viewModel.ColorSpace;
            model.Comments              = viewModel.Comments;
            model.Contrast              = viewModel.Contrast;
            model.CreationDate          = viewModel.CreationDate;
            model.DigitalZoomRatio      = viewModel.DigitalZoomRatio;
            model.ExifVersion           = viewModel.ExifVersion;
            model.ExposureTime          = viewModel.ExposureTime;
            model.ExposureMethod        = viewModel.ExposureMethod;
            model.ExposureProgram       = viewModel.ExposureProgram;
            model.Flash                 = viewModel.Flash;
            model.Focal                 = viewModel.Focal;
            model.FocalLength           = viewModel.FocalLength;
            model.FocalLengthEquivalent = viewModel.FocalLengthEquivalent;
            model.HorizontalResolution  = viewModel.HorizontalResolution;
            model.IsoRating             = viewModel.IsoRating;
            model.Lens                  = viewModel.Lens;
            model.LicenseId             = viewModel.LicenseId;
            model.LightSource           = viewModel.LightSource;
            model.MeteringMode          = viewModel.MeteringMode;
            model.ResolutionUnit        = viewModel.ResolutionUnit;
            model.Orientation           = viewModel.Orientation;
            model.Saturation            = viewModel.Saturation;
            model.SceneCaptureType      = viewModel.SceneCaptureType;
            model.SensingMethod         = viewModel.SensingMethod;
            model.Sharpness             = viewModel.Sharpness;
            model.SoftwareUsed          = viewModel.SoftwareUsed;
            model.Source                = viewModel.Source;
            model.SubjectDistanceRange  = viewModel.SubjectDistanceRange;
            model.VerticalResolution    = viewModel.VerticalResolution;
            model.WhiteBalance          = viewModel.WhiteBalance;

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<Guid> CreateAsync(MachinePhotoViewModel viewModel, string userId)
        {
            var model = new MachinePhoto
            {
                Aperture              = viewModel.Aperture,
                Author                = viewModel.Author,
                CameraManufacturer    = viewModel.CameraManufacturer,
                CameraModel           = viewModel.CameraModel,
                ColorSpace            = viewModel.ColorSpace,
                Comments              = viewModel.Comments,
                Contrast              = viewModel.Contrast,
                CreationDate          = viewModel.CreationDate,
                DigitalZoomRatio      = viewModel.DigitalZoomRatio,
                ExifVersion           = viewModel.ExifVersion,
                ExposureTime          = viewModel.ExposureTime,
                ExposureMethod        = viewModel.ExposureMethod,
                ExposureProgram       = viewModel.ExposureProgram,
                Flash                 = viewModel.Flash,
                Focal                 = viewModel.Focal,
                FocalLength           = viewModel.FocalLength,
                FocalLengthEquivalent = viewModel.FocalLengthEquivalent,
                HorizontalResolution  = viewModel.HorizontalResolution,
                Id                    = viewModel.Id,
                IsoRating             = viewModel.IsoRating,
                Lens                  = viewModel.Lens,
                LicenseId             = viewModel.LicenseId,
                LightSource           = viewModel.LightSource,
                MachineId             = viewModel.MachineId,
                MeteringMode          = viewModel.MeteringMode,
                ResolutionUnit        = viewModel.ResolutionUnit,
                Orientation           = viewModel.Orientation,
                Saturation            = viewModel.Saturation,
                SceneCaptureType      = viewModel.SceneCaptureType,
                SensingMethod         = viewModel.SensingMethod,
                Sharpness             = viewModel.Sharpness,
                SoftwareUsed          = viewModel.SoftwareUsed,
                Source                = viewModel.Source,
                SubjectDistanceRange  = viewModel.SubjectDistanceRange,
                UploadDate            = viewModel.UploadDate,
                UserId                = viewModel.UserId,
                VerticalResolution    = viewModel.VerticalResolution,
                WhiteBalance          = viewModel.WhiteBalance,
                OriginalExtension     = viewModel.OriginalExtension
            };

            await _context.MachinePhotos.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }
    }
}