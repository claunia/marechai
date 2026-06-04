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
using Marechai.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marechai.Server.Controllers;

[Route("/processors/photos")]
[ApiController]
public class ProcessorPhotosController(MarechaiContext context, IConfiguration configuration,
                                       BatchUploadJobStore batchJobs) : ControllerBase
{
    static readonly HashSet<string> _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"
    ];

    readonly string _assetRootPath = configuration["AssetRootPath"]!;
    [HttpGet("/processors/{processorId:int}/photos")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<Guid>> GetGuidsByProcessorAsync(int processorId) => context.ProcessorPhotos
                                                                    .Where(p => p.ProcessorId == processorId)
                                                                    .OrderBy(p => p.CreatedOn)
                                                                    .ThenBy(p => p.Id)
                                                                    .Select(p => p.Id)
                                                                    .ToListAsync();

    [HttpGet("{id:Guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ProcessorPhotoDto> GetAsync(Guid id) => context.ProcessorPhotos.Where(p => p.Id == id)
                                                         .Select(p => new ProcessorPhotoDto
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
                                                              ProcessorCompanyName = p.Processor.Company.Name,
                                                              ProcessorId          = p.ProcessorId,
                                                              ProcessorName        = p.Processor.Name,
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
    public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] ProcessorPhotoDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        ProcessorPhoto model = await context.ProcessorPhotos.FindAsync(id);

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
    public async Task<ActionResult<Guid>> CreateAsync([FromBody] ProcessorPhotoDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new ProcessorPhoto
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
            ProcessorId           = dto.ProcessorId,
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

        await context.ProcessorPhotos.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProcessorPhotoDto>> UploadAsync(IFormFile            file,
                                                                   [FromForm] int       processorId,
                                                                   [FromForm] int       licenseId,
                                                                   [FromForm] string   source)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(file is null || file.Length == 0)
            return Problem(detail: "No file provided.", statusCode: StatusCodes.Status400BadRequest);

        if(file.Length > 50 * 1024 * 1024)
            return Problem(detail: "File exceeds 50 MB limit.", statusCode: StatusCodes.Status400BadRequest);

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        if(!_allowedExtensions.Contains(extension))
            return Problem(detail: "Unsupported file format. Accepted: JPEG, PNG, WebP, TIFF, BMP.", statusCode: StatusCodes.Status400BadRequest);

        if(!string.IsNullOrEmpty(file.ContentType) && !_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return Problem(detail: "Unsupported content type.", statusCode: StatusCodes.Status400BadRequest);

        // Read the file into memory
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        // Extract EXIF metadata
        var model = new ProcessorPhoto
        {
            Id                = Guid.NewGuid(),
            ProcessorId       = processorId,
            LicenseId         = licenseId,
            Source            = source,
            UserId            = userId,
            UploadDate        = DateTime.UtcNow,
            OriginalExtension = extension.TrimStart('.')
        };

        PhotoExifExtractor.ExtractInto(model, ms);

        // Save original file to disk
        Photos.EnsureCreated(_assetRootPath, false, "processors");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "processors", "originals");
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
            photos.ConversionWorker(_assetRootPath, model.Id, originalPath, sourceFormat, false, "processors");
        });

        // Save to database
        await context.ProcessorPhotos.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new ProcessorPhotoDto
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
            ProcessorId           = model.ProcessorId,
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

        ProcessorPhoto model = await context.ProcessorPhotos.FindAsync(id);

        if(model is null) return NotFound();

        context.ProcessorPhotos.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        // Delete all generated files from disk
        string photosRoot = Path.Combine(_assetRootPath, "photos", "processors");
        string guidStr    = id.ToString();

        // Delete original
        DeleteFilesByPattern(Path.Combine(photosRoot, "originals"), $"{guidStr}.*");

        // Delete all format/resolution variants (full + thumbnails)
        string[] formats     = ["jpeg", "webp", "avif"];
        string[] resolutions = ["4k"];

        foreach(string format in formats)
        {
            string ext = format switch
            {
                "jpeg" => ".jpg",
                "webp" => ".webp",
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

    /// <summary>
    ///     Maximum in-flight pending photos a single collaborator may stage for a given
    ///     processor before they submit (or cancel) the suggestion. Mirrors the per-batch
    ///     cap enforced by the dialog.
    /// </summary>
    const int PendingPhotosPerUserPerProcessorCap = 15;

    /// <summary>Allowed extensions for collaborator-uploaded processor photos (must match server JS validation).</summary>
    static readonly HashSet<string> _pendingAllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    /// <summary>Allowed content types for collaborator-uploaded processor photos (must match server JS validation).</summary>
    static readonly HashSet<string> _pendingAllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    /// <summary>
    ///     Stage a single pending processor photo for a brand-new collaborative suggestion.
    ///     The uploader keeps each pending file on the server (sidecar tracks ownership +
    ///     parent <c>processorId</c>) until they call <c>POST /suggestions</c> referencing
    ///     the returned <c>guid</c>. Per-uploader cap of 15 in-flight pending photos per
    ///     processor.
    /// </summary>
    [HttpPost("pending")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PendingImageUploadDto>> UploadPendingAsync(IFormFile file,
        [FromQuery] int processorId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        if(file is null || file.Length == 0) return Problem(detail: "No file provided.", statusCode: StatusCodes.Status400BadRequest);

        if(file.Length > 50 * 1024 * 1024) return Problem(detail: "File exceeds 50 MB limit.", statusCode: StatusCodes.Status400BadRequest);

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        if(!_pendingAllowedExtensions.Contains(extension))
            return Problem(detail: "Unsupported file format. Accepted: JPEG, PNG, WebP.", statusCode: StatusCodes.Status400BadRequest);

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_pendingAllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return Problem(detail: "Unsupported content type.", statusCode: StatusCodes.Status400BadRequest);

        bool processorExists = await context.Processors.AnyAsync(p => p.Id == processorId);

        if(!processorExists) return Problem(detail: "Processor not found.", statusCode: StatusCodes.Status404NotFound);

        int currentCount = PendingImageStore.CountByUploaderForParentEntity(_assetRootPath, "processors", userId,
            (byte)SuggestionEntityType.ProcessorPhoto, processorId);

        if(currentCount >= PendingPhotosPerUserPerProcessorCap)
            return Problem(detail: $"You already have {currentCount} pending photos for this processor. Maximum is " +
                                   $"{PendingPhotosPerUserPerProcessorCap} per processor. Submit or remove some first.",
                           statusCode: StatusCodes.Status409Conflict);

        Guid guid;

        await using(Stream stream = file.OpenReadStream())
        {
            // EntityId stays 0 because the suggestion row that will reference these photos
            // doesn't exist yet. ParentEntityId carries the processorId so the per-uploader
            // cap and cleanup-by-parent helpers can scope correctly.
            guid = await PendingImageStore.StoreAsync(_assetRootPath, "processors", extension,
                (byte)SuggestionEntityType.ProcessorPhoto, entityId: 0L, userId, file.ContentType, stream,
                parentEntityId: processorId);
        }

        return Ok(new PendingImageUploadDto
        {
            Guid      = guid,
            Extension = extension.TrimStart('.')
        });
    }

    /// <summary>
    ///     Delete a pending photo before it has been submitted as part of a suggestion. Only
    ///     the original uploader (or an admin) may delete.
    /// </summary>
    [HttpDelete("pending/{guid:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePendingAsync(Guid guid)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        PendingImageStore.PendingMetadata meta =
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "processors", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.ProcessorPhoto) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");

        if(!PendingImageStore.CanAccess(meta, userId, isAdmin)) return Forbid();

        PendingImageStore.Delete(_assetRootPath, "processors", guid);

        return NoContent();
    }

    /// <summary>
    ///     Stream the binary contents of a pending processor photo. Used by the dialog
    ///     thumbnail preview AND by the admin SuggestionDiffPanel preview. Auth-gated:
    ///     only the uploader and admins can read.
    /// </summary>
    [HttpGet("pending/{guid:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPendingAsync(Guid guid)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        PendingImageStore.PendingMetadata meta =
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "processors", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.ProcessorPhoto) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");

        if(!PendingImageStore.CanAccess(meta, userId, isAdmin)) return Forbid();

        string path = await PendingImageStore.GetImagePathAsync(_assetRootPath, "processors", guid);

        if(path is null || !System.IO.File.Exists(path)) return NotFound();

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return File(stream, meta.ContentType ?? "application/octet-stream");
    }

    static void DeleteFilesByPattern(string directory, string pattern)
    {
        if(!System.IO.Directory.Exists(directory)) return;

        foreach(string file in System.IO.Directory.GetFiles(directory, pattern))
            System.IO.File.Delete(file);
    }

    // ───────────────────────────── Admin batch upload ──────────────────────────────

    /// <summary>
    ///     Allowed extensions accepted by the admin batch-upload staging endpoint. Wider
    ///     than the collaborator-suggestion path because admins may upload formats the
    ///     conversion pipeline supports end-to-end.
    /// </summary>
    static readonly HashSet<string> _adminBatchAllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".avif", ".bmp", ".tif", ".tiff"
    };

    /// <summary>
    ///     ImageMagick canonical format names (as returned by <c>identify -format %m</c>)
    ///     trusted for admin batch uploads. Checked AFTER the file is written to disk so
    ///     an attacker can't bypass it by lying about the extension or MIME type.
    /// </summary>
    static readonly HashSet<string> _adminBatchAllowedMagickFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        "JPEG", "PNG", "WEBP", "AVIF", "BMP", "TIFF"
    };

    /// <summary>
    ///     Stage a single image as part of an admin processor-photo batch upload. The server
    ///     writes the file to the pending folder, content-sniffs it with ImageMagick
    ///     (rejecting on mismatch regardless of extension/MIME), generates a 256x256
    ///     thumbnail returned inline as a JPEG data URL, and stores both file + sidecar
    ///     for later commit.
    /// </summary>
    [HttpPost("admin/pending")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<AdminPendingProcessorPhotoUploadDto>> UploadAdminBatchPendingAsync(IFormFile file,
        [FromQuery] int processorId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        if(file is null || file.Length == 0) return Problem(detail: "No file provided.", statusCode: StatusCodes.Status400BadRequest);
        if(file.Length > 50 * 1024 * 1024) return Problem(detail: "File exceeds 50 MB limit.", statusCode: StatusCodes.Status400BadRequest);

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        if(!_adminBatchAllowedExtensions.Contains(extension))
            return Problem(detail: "Unsupported file format. Accepted: JPEG, PNG, WebP, AVIF, BMP, TIFF.", statusCode: StatusCodes.Status400BadRequest);

        bool processorExists = await context.Processors.AnyAsync(p => p.Id == processorId);
        if(!processorExists) return Problem(detail: "Processor not found.", statusCode: StatusCodes.Status404NotFound);

        // Persist FIRST (with a temporary placeholder for dimensions), then content-sniff
        // and either re-write the sidecar with real dimensions or reject + delete the file.
        Guid guid;
        await using(Stream stream = file.OpenReadStream())
        {
            guid = await PendingImageStore.StoreAsync(_assetRootPath, "processors", extension,
                (byte)SuggestionEntityType.ProcessorPhoto, entityId: 0L, userId, file.ContentType, stream,
                parentEntityId: processorId, _adminBatchAllowedExtensions, isAdminStaging: true,
                width: null, height: null);
        }

        string imagePath = await PendingImageStore.GetImagePathAsync(_assetRootPath, "processors", guid);
        if(imagePath is null)
        {
            PendingImageStore.Delete(_assetRootPath, "processors", guid);
            return Problem(detail: "Failed to persist uploaded file.", statusCode: StatusCodes.Status400BadRequest);
        }

        (string magickFormat, int width, int height) = Photos.Identify(imagePath);
        if(magickFormat is null || !_adminBatchAllowedMagickFormats.Contains(magickFormat))
        {
            PendingImageStore.Delete(_assetRootPath, "processors", guid);
            return Problem(detail: "File content does not match a supported image format.",
                           statusCode: StatusCodes.Status415UnsupportedMediaType);
        }

        byte[] thumbBytes = Photos.GenerateThumbnailJpeg(imagePath);
        if(thumbBytes is null)
        {
            PendingImageStore.Delete(_assetRootPath, "processors", guid);
            return Problem(detail: "Failed to generate thumbnail for the uploaded image.", statusCode: StatusCodes.Status400BadRequest);
        }

        await PendingImageStore.StoreThumbnailAsync(_assetRootPath, "processors", guid, thumbBytes);

        // Re-write the sidecar so dimensions are captured for future consumers.
        PendingImageStore.PendingMetadata meta =
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "processors", guid);
        if(meta is not null)
        {
            meta.Width  = width;
            meta.Height = height;
            string sidecar = Path.Combine(_assetRootPath, "photos", "processors", "pending",
                                          guid.ToString() + ".json");
            await System.IO.File.WriteAllTextAsync(sidecar, System.Text.Json.JsonSerializer.Serialize(meta));
        }

        string dataUrl = "data:image/jpeg;base64," + Convert.ToBase64String(thumbBytes);

        return Ok(new AdminPendingProcessorPhotoUploadDto
        {
            Id              = guid,
            Extension       = extension.TrimStart('.'),
            ThumbnailBase64 = dataUrl,
            SizeBytes       = file.Length,
            Width           = width,
            Height          = height
        });
    }

    /// <summary>
    ///     Delete a single admin-staged pending processor-photo image before commit.
    ///     Owner-only (an admin cannot delete another admin's in-flight staging entries).
    /// </summary>
    [HttpDelete("admin/pending/{guid:guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAdminBatchPendingAsync(Guid guid)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        PendingImageStore.PendingMetadata meta =
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "processors", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.ProcessorPhoto) return NotFound();
        if(!meta.IsAdminStaging) return NotFound();
        if(!string.Equals(meta.UploadedById, userId, StringComparison.Ordinal)) return Forbid();

        PendingImageStore.Delete(_assetRootPath, "processors", guid);
        return NoContent();
    }

    /// <summary>
    ///     Commit a batch of admin-staged pending images into permanent <c>ProcessorPhoto</c>
    ///     rows. The endpoint returns 202 with a <c>jobId</c> immediately and processes the
    ///     batch on a background task; the client polls
    ///     <c>GET /processors/photos/admin/batch/{jobId}/status</c> for progress and per-item
    ///     results. Each image runs through the standard 8-variant conversion sequentially
    ///     so the progress bar advances one image at a time.
    /// </summary>
    [HttpPost("admin/batch/commit")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(typeof(AdminProcessorPhotoBatchJobStatusDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProcessorPhotoBatchJobStatusDto>> CommitAdminBatchAsync(
        [FromBody] AdminProcessorPhotoBatchCommitRequestDto request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        if(request is null || request.Items is null || request.Items.Count == 0)
            return Problem(detail: "No items provided.", statusCode: StatusCodes.Status400BadRequest);

        bool processorExists = await context.Processors.AnyAsync(p => p.Id == request.ProcessorId);
        if(!processorExists) return Problem(detail: "Processor not found.", statusCode: StatusCodes.Status404NotFound);

        bool licenseExists = await context.Licenses.AnyAsync(l => l.Id == request.LicenseId);
        if(!licenseExists) return Problem(detail: "License not found.", statusCode: StatusCodes.Status404NotFound);

        // Validate every pending id belongs to the caller and matches processor/admin scope.
        foreach(AdminProcessorPhotoBatchCommitItemDto item in request.Items)
        {
            PendingImageStore.PendingMetadata meta =
                await PendingImageStore.GetMetadataAsync(_assetRootPath, "processors", item.PendingId);

            if(meta is null) return Problem(detail: $"Pending image {item.PendingId} not found.", statusCode: StatusCodes.Status404NotFound);
            if(meta.EntityType != (byte)SuggestionEntityType.ProcessorPhoto)
                return Problem(detail: $"Pending image {item.PendingId} is not a processor photo.", statusCode: StatusCodes.Status400BadRequest);
            if(!meta.IsAdminStaging)
                return Problem(detail: $"Pending image {item.PendingId} is not an admin-staged image.", statusCode: StatusCodes.Status400BadRequest);
            if(!string.Equals(meta.UploadedById, userId, StringComparison.Ordinal))
                return Forbid();
            if(meta.ParentEntityId != request.ProcessorId)
                return Problem(detail: $"Pending image {item.PendingId} was staged for a different processor.", statusCode: StatusCodes.Status400BadRequest);
        }

        Guid jobId = batchJobs.StartJob(userId, request.ProcessorId, request.Items.Count);

        var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
        string assetRootPath = _assetRootPath;
        List<AdminProcessorPhotoBatchCommitItemDto> items = request.Items;
        int processorId = request.ProcessorId;
        int licenseId = request.LicenseId;

        _ = Task.Run(() => RunBatchJobAsync(jobId, userId, processorId, licenseId, items, assetRootPath, scopeFactory));

        AdminProcessorPhotoBatchJobStatusDto snapshot = SnapshotJob(batchJobs.GetForOwner(jobId, userId)!);
        return Accepted(snapshot);
    }

    /// <summary>
    ///     Poll the status of an in-flight admin processor-photo batch-commit job. Owner-only.
    /// </summary>
    [HttpGet("admin/batch/{jobId:guid}/status")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AdminProcessorPhotoBatchJobStatusDto> GetAdminBatchStatus(Guid jobId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        BatchUploadJobStore.BatchJob job = batchJobs.GetForOwner(jobId, userId);
        if(job is null) return NotFound();

        return Ok(SnapshotJob(job));
    }

    static AdminProcessorPhotoBatchJobStatusDto SnapshotJob(BatchUploadJobStore.BatchJob job)
    {
        lock(job.Lock)
        {
            return new AdminProcessorPhotoBatchJobStatusDto
            {
                JobId            = job.JobId,
                State            = (int)job.State,
                Total            = job.Total,
                Processed        = job.Processed,
                CurrentPendingId = job.CurrentPendingId,
                Results          = job.Results.Select(r => new AdminProcessorPhotoBatchJobItemResultDto
                                       {
                                           PendingId = r.PendingId,
                                           Succeeded = r.Succeeded,
                                           PhotoId   = r.AssignedId,
                                           Error     = r.Error
                                       })
                                       .ToList()
            };
        }
    }

    /// <summary>
    ///     Sequential worker that promotes each pending image into a ProcessorPhoto row,
    ///     extracts EXIF metadata, runs the 8-variant conversion synchronously per image
    ///     (so the progress bar advances one-at-a-time), and records per-item
    ///     success/failure on the job.
    /// </summary>
    async Task RunBatchJobAsync(Guid jobId, string userId, int processorId, int licenseId,
                                List<AdminProcessorPhotoBatchCommitItemDto> items, string assetRootPath,
                                IServiceScopeFactory scopeFactory)
    {
        BatchUploadJobStore.BatchJob job = batchJobs.GetForOwner(jobId, userId);
        if(job is null) return;

        lock(job.Lock) { job.State = BatchJobState.Processing; }

        Photos.EnsureCreated(assetRootPath, false, "processors");

        for(int i = 0; i < items.Count; i++)
        {
            AdminProcessorPhotoBatchCommitItemDto item = items[i];

            lock(job.Lock)
            {
                job.CurrentPendingId = item.PendingId;
                job.Processed        = i;
                job.LastTouchedOn    = DateTime.UtcNow;
            }

            try
            {
                (string movedPath, string ext) = await PendingImageStore.PromoteToOriginalsAsync(assetRootPath,
                                                     "processors", item.PendingId);

                if(movedPath is null)
                {
                    AppendResult(job, item.PendingId, false, null, "Pending file missing at commit time.");
                    continue;
                }

                // Rename the moved file to use a fresh photo guid so its filename matches the DB row id.
                Guid   photoId      = Guid.NewGuid();
                string originalsDir = Path.Combine(assetRootPath, "photos", "processors", "originals");
                string finalPath    = Path.Combine(originalsDir, photoId.ToString() + "." + ext);
                System.IO.File.Move(movedPath, finalPath, overwrite: true);

                var model = new ProcessorPhoto
                {
                    Id                = photoId,
                    ProcessorId       = processorId,
                    LicenseId         = licenseId,
                    Source            = string.IsNullOrWhiteSpace(item.Source) ? null : item.Source.Trim(),
                    UserId            = userId,
                    UploadDate        = DateTime.UtcNow,
                    OriginalExtension = ext
                };

                // Extract EXIF metadata from the promoted file (same pipeline as single-upload).
                await using(var fs = new FileStream(finalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    PhotoExifExtractor.ExtractInto(model, fs);
                }

                using(IServiceScope scope = scopeFactory.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<MarechaiContext>();
                    await ctx.ProcessorPhotos.AddAsync(model);
                    await ctx.SaveChangesWithUserAsync(userId);
                }

                // Run the 8-variant conversion synchronously so the progress bar advances
                // only after every variant for this image has landed.
                var photos = new Photos();
                photos.ConversionWorker(assetRootPath, photoId, finalPath, ext, false, "processors");

                AppendResult(job, item.PendingId, true, photoId, null);
            }
            catch(Exception ex)
            {
                AppendResult(job, item.PendingId, false, null, ex.Message);
            }
        }

        lock(job.Lock)
        {
            job.Processed        = items.Count;
            job.CurrentPendingId = null;
            job.State            = BatchJobState.Completed;
            job.LastTouchedOn    = DateTime.UtcNow;
        }
    }

    static void AppendResult(BatchUploadJobStore.BatchJob job, Guid pendingId, bool ok, Guid? photoId, string error)
    {
        lock(job.Lock)
        {
            job.Results.Add(new BatchJobItemResult
            {
                PendingId  = pendingId,
                Succeeded  = ok,
                AssignedId = photoId,
                Error      = error
            });
            job.LastTouchedOn = DateTime.UtcNow;
        }
    }
}
