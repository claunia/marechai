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
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Helpers;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Exif.Makernotes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marechai.Server.Controllers;

[Route("/gpus/photos")]
[ApiController]
public class GpuPhotosController(MarechaiContext context, IConfiguration configuration) : ControllerBase
{
    static readonly HashSet<string> _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"
    ];

    readonly string _assetRootPath = configuration["AssetRootPath"]!;
    [HttpGet("/gpus/{gpuId:int}/photos")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<Guid>> GetGuidsByGpuAsync(int gpuId) => context.GpuPhotos.AsNoTracking()
                                                                    .Where(p => p.GpuId == gpuId)
                                                                    .OrderBy(p => p.CreatedOn)
                                                                    .ThenBy(p => p.Id)
                                                                    .Select(p => p.Id)
                                                                    .ToListAsync();

    [HttpGet("{id:Guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<GpuPhotoDto> GetAsync(Guid id) => context.GpuPhotos.Where(p => p.Id == id)
                                                         .Select(p => new GpuPhotoDto
                                                          {
                                                              Aperture              = p.Aperture,
                                                              Author                = p.Author,
                                                              CameraManufacturer    = p.CameraManufacturer,
                                                              CameraModel           = p.CameraModel,
                                                              ColorSpace            = (ushort?)p.ColorSpace,
                                                              Comments              = p.Comments,
                                                              Contrast              = (ushort?)p.Contrast,
                                                              CreationDate          = p.CreationDate,
                                                              DigitalZoomRatio      = p.DigitalZoomRatio,
                                                              ExifVersion           = p.ExifVersion,
                                                              ExposureTime          = p.ExposureTime,
                                                              ExposureMethod        = (ushort?)p.ExposureMethod,
                                                              ExposureProgram       = (ushort?)p.ExposureProgram,
                                                              Flash                 = (ushort?)p.Flash,
                                                              Focal                 = p.Focal,
                                                              FocalLength           = p.FocalLength,
                                                              FocalLengthEquivalent = p.FocalLengthEquivalent,
                                                              HorizontalResolution  = p.HorizontalResolution,
                                                              Id                    = p.Id,
                                                              IsoRating             = p.IsoRating,
                                                              Lens                  = p.Lens,
                                                              LicenseId             = p.LicenseId,
                                                              LicenseName           = p.License.Name,
                                                              LightSource = p.LightSource.HasValue
                                                                  ? (ushort?)p.LightSource
                                                                  : null,
                                                              GpuCompanyName       = p.Gpu.Company.Name,
                                                              GpuId                = p.GpuId,
                                                              GpuName              = p.Gpu.Name,
                                                              MeteringMode         = (ushort?)p.MeteringMode,
                                                              ResolutionUnit       = (ushort?)p.ResolutionUnit,
                                                              Orientation          = (ushort?)p.Orientation,
                                                              Saturation           = (ushort?)p.Saturation,
                                                              SceneCaptureType     = (ushort?)p.SceneCaptureType,
                                                              SensingMethod        = (ushort?)p.SensingMethod,
                                                              Sharpness            = (ushort?)p.Sharpness,
                                                              SoftwareUsed         = p.SoftwareUsed,
                                                              Source               = p.Source,
                                                              SubjectDistanceRange = (byte?)p.SubjectDistanceRange,
                                                              UploadDate           = p.UploadDate,
                                                              UserId               = p.UserId,
                                                              VerticalResolution   = p.VerticalResolution,
                                                              WhiteBalance         = (ushort?)p.WhiteBalance,
                                                              OriginalExtension    = p.OriginalExtension
                                                          })
                                                         .FirstOrDefaultAsync();

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] GpuPhotoDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        GpuPhoto model = await context.GpuPhotos.FindAsync(id);

        if(model is null) return NotFound();

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
        model.LightSource = dto.LightSource.HasValue ? (LightSource?)dto.LightSource : null;
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

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> CreateAsync([FromBody] GpuPhotoDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new GpuPhoto
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
            LightSource           = dto.LightSource.HasValue ? (LightSource?)dto.LightSource : null,
            GpuId                 = dto.GpuId,
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

        await context.GpuPhotos.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GpuPhotoDto>> UploadAsync(IFormFile            file,
                                                             [FromForm] int       gpuId,
                                                             [FromForm] int       licenseId,
                                                             [FromForm] string   source)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(file is null || file.Length == 0)
            return BadRequest("No file provided.");

        if(file.Length > 50 * 1024 * 1024)
            return BadRequest("File exceeds 50 MB limit.");

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        if(!_allowedExtensions.Contains(extension))
            return BadRequest("Unsupported file format. Accepted: JPEG, PNG, WebP, TIFF, BMP.");

        if(!string.IsNullOrEmpty(file.ContentType) && !_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Unsupported content type.");

        // Read the file into memory
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        // Extract EXIF metadata
        var model = new GpuPhoto
        {
            Id                = Guid.NewGuid(),
            GpuId             = gpuId,
            LicenseId         = licenseId,
            Source            = source,
            UserId            = userId,
            UploadDate        = DateTime.UtcNow,
            OriginalExtension = extension.TrimStart('.')
        };

        try
        {
            IReadOnlyList<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(ms);

            var exifIfd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            var exifSub  = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            if(exifIfd0 is not null)
            {
                if(exifIfd0.TryGetUInt16(ExifDirectoryBase.TagOrientation, out ushort orientation))
                    model.Orientation = (Orientation)orientation;

                model.CameraManufacturer = exifIfd0.GetDescription(ExifDirectoryBase.TagMake);
                model.CameraModel        = exifIfd0.GetDescription(ExifDirectoryBase.TagModel);
                model.SoftwareUsed       = exifIfd0.GetDescription(ExifDirectoryBase.TagSoftware);
                model.Author             = exifIfd0.GetDescription(ExifDirectoryBase.TagArtist);

                if(exifIfd0.TryGetDouble(ExifDirectoryBase.TagXResolution, out double xRes))
                    model.HorizontalResolution = xRes;

                if(exifIfd0.TryGetDouble(ExifDirectoryBase.TagYResolution, out double yRes))
                    model.VerticalResolution = yRes;

                if(exifIfd0.TryGetUInt16(ExifDirectoryBase.TagResolutionUnit, out ushort resUnit))
                    model.ResolutionUnit = (ResolutionUnit)resUnit;
            }

            if(exifSub is not null)
            {
                if(exifSub.TryGetDouble(ExifDirectoryBase.TagFNumber, out double fNumber))
                    model.Focal = fNumber;

                if(exifSub.TryGetDouble(ExifDirectoryBase.TagAperture, out double aperture))
                    model.Aperture = aperture;

                if(exifSub.TryGetDouble(ExifDirectoryBase.TagExposureTime, out double exposureTime))
                    model.ExposureTime = exposureTime;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagExposureProgram, out ushort exposureProgram))
                    model.ExposureProgram = (ExposureProgram)exposureProgram;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagIsoEquivalent, out ushort isoRating))
                    model.IsoRating = isoRating;

                model.ExifVersion = exifSub.GetDescription(ExifDirectoryBase.TagExifVersion);

                if(exifSub.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime creationDate))
                    model.CreationDate = creationDate;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagMeteringMode, out ushort meteringMode))
                    model.MeteringMode = (MeteringMode)meteringMode;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagFlash, out ushort flash))
                    model.Flash = (Flash)flash;

                if(exifSub.TryGetDouble(ExifDirectoryBase.TagFocalLength, out double focalLength))
                    model.FocalLength = focalLength;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagColorSpace, out ushort colorSpace))
                    model.ColorSpace = (ColorSpace)colorSpace;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagExposureMode, out ushort exposureMode))
                    model.ExposureMethod = (ExposureMode)exposureMode;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagWhiteBalance, out ushort whiteBalance))
                    model.WhiteBalance = (WhiteBalance)whiteBalance;

                if(exifSub.TryGetDouble(ExifDirectoryBase.TagDigitalZoomRatio, out double digitalZoom))
                    model.DigitalZoomRatio = digitalZoom;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.Tag35MMFilmEquivFocalLength, out ushort focalLengthEquiv))
                    model.FocalLengthEquivalent = focalLengthEquiv;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagSceneCaptureType, out ushort sceneCaptureType))
                    model.SceneCaptureType = (SceneCaptureType)sceneCaptureType;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagContrast, out ushort contrast))
                    model.Contrast = (Contrast)contrast;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagSaturation, out ushort saturation))
                    model.Saturation = (Saturation)saturation;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagSharpness, out ushort sharpness))
                    model.Sharpness = (Sharpness)sharpness;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagSubjectDistanceRange, out ushort subjectDistRange))
                    model.SubjectDistanceRange = (SubjectDistanceRange)subjectDistRange;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagSensingMethod, out ushort sensingMethod))
                    model.SensingMethod = (SensingMethod)sensingMethod;

                if(exifSub.TryGetUInt16(0x9208, out ushort lightSource))
                    model.LightSource = (LightSource)lightSource;

                model.Lens = exifSub.GetDescription(ExifDirectoryBase.TagLensModel);

                model.Comments = exifSub.GetDescription(ExifDirectoryBase.TagUserComment);
            }
        }
        catch
        {
            // EXIF extraction failed — continue without metadata
        }

        // Save original file to disk
        Photos.EnsureCreated(_assetRootPath, false, "gpus");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "gpus", "originals");
        string originalPath = Path.Combine(originalsDir, $"{model.Id}{extension}");

        ms.Position = 0;
        await using(var fs = new FileStream(originalPath, FileMode.CreateNew, FileAccess.Write))
        {
            await ms.CopyToAsync(fs);
        }

        // Fire conversion worker (generates all format/resolution variants)
        string sourceFormat = extension.TrimStart('.');

        _ = Task.Run(() =>
        {
            var photos = new Photos();
            photos.ConversionWorker(_assetRootPath, model.Id, originalPath, sourceFormat, false, "gpus");
        });

        // Save to database
        await context.GpuPhotos.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new GpuPhotoDto
        {
            Id                    = model.Id,
            Aperture              = model.Aperture,
            Author                = model.Author,
            CameraManufacturer    = model.CameraManufacturer,
            CameraModel           = model.CameraModel,
            ColorSpace            = (ushort?)model.ColorSpace,
            Comments              = model.Comments,
            Contrast              = (ushort?)model.Contrast,
            CreationDate          = model.CreationDate,
            DigitalZoomRatio      = model.DigitalZoomRatio,
            ExifVersion           = model.ExifVersion,
            ExposureTime          = model.ExposureTime,
            ExposureMethod        = (ushort?)model.ExposureMethod,
            ExposureProgram       = (ushort?)model.ExposureProgram,
            Flash                 = (ushort?)model.Flash,
            Focal                 = model.Focal,
            FocalLength           = model.FocalLength,
            FocalLengthEquivalent = model.FocalLengthEquivalent,
            HorizontalResolution  = model.HorizontalResolution,
            IsoRating             = model.IsoRating,
            Lens                  = model.Lens,
            LicenseId             = model.LicenseId,
            LightSource           = (ushort?)model.LightSource,
            GpuId                 = model.GpuId,
            MeteringMode          = (ushort?)model.MeteringMode,
            ResolutionUnit        = (ushort?)model.ResolutionUnit,
            Orientation           = (ushort?)model.Orientation,
            Saturation            = (ushort?)model.Saturation,
            SceneCaptureType      = (ushort?)model.SceneCaptureType,
            SensingMethod         = (ushort?)model.SensingMethod,
            Sharpness             = (ushort?)model.Sharpness,
            SoftwareUsed          = model.SoftwareUsed,
            Source                = model.Source,
            SubjectDistanceRange  = (byte?)model.SubjectDistanceRange,
            UploadDate            = model.UploadDate,
            UserId                = model.UserId,
            VerticalResolution    = model.VerticalResolution,
            WhiteBalance          = (ushort?)model.WhiteBalance,
            OriginalExtension     = model.OriginalExtension
        });
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

        GpuPhoto model = await context.GpuPhotos.FindAsync(id);

        if(model is null) return NotFound();

        context.GpuPhotos.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        // Delete all generated files from disk
        string photosRoot = Path.Combine(_assetRootPath, "photos", "gpus");
        string guidStr    = id.ToString();

        // Delete original
        DeleteFilesByPattern(Path.Combine(photosRoot, "originals"), $"{guidStr}.*");

        // Delete all format/resolution variants (full + thumbnails)
        string[] formats     = ["jpeg", "webp", "heif", "avif"];
        string[] resolutions = ["hd", "1440p", "4k"];

        foreach(string format in formats)
        {
            string ext = format switch
            {
                "jpeg" => ".jpg",
                "webp" => ".webp",
                "heif" => ".heic",
                "avif" => ".avif",
                _      => $".{format}"
            };

            foreach(string res in resolutions)
            {
                string fullPath  = Path.Combine(photosRoot, format, res, $"{guidStr}{ext}");
                string thumbPath = Path.Combine(photosRoot, "thumbs", format, res, $"{guidStr}{ext}");

                if(System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);

                if(System.IO.File.Exists(thumbPath))
                    System.IO.File.Delete(thumbPath);
            }
        }

        return Ok();
    }

    static void DeleteFilesByPattern(string directory, string pattern)
    {
        if(!System.IO.Directory.Exists(directory)) return;

        foreach(string file in System.IO.Directory.GetFiles(directory, pattern))
            System.IO.File.Delete(file);
    }
}
