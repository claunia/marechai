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

[Route("/software/covers")]
[ApiController]
public class SoftwareCoversController(MarechaiContext context, IConfiguration configuration,
                                      BatchUploadJobStore batchJobs) : ControllerBase
{
    static readonly HashSet<string> _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"
    ];

    readonly string _assetRootPath = configuration["AssetRootPath"]!;

    [HttpGet("/software/releases/{releaseId}/covers")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<Guid>> GetGuidsByReleaseAsync(ulong releaseId) =>
        context.SoftwareCovers
               .Where(c => c.SoftwareReleaseId == releaseId)
               .OrderBy(c => c.CreatedOn)
               .ThenBy(c => c.Id)
               .Select(c => c.Id)
               .ToListAsync();

    [HttpGet("/software/{softwareId}/covers")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareCoverDto>> GetBySoftwareAsync(ulong softwareId, [FromQuery] string lang = null)
    {
        string langCode  = LanguageResolver.Resolve(HttpContext, lang);
        bool   isEnglish = string.Equals(langCode, "eng", StringComparison.Ordinal);

        IQueryable<SoftwareCover> source = context.SoftwareCovers
                                                  .Where(c => c.Release.SoftwareId == softwareId)
                                                  .OrderBy(c => c.Type)
                                                  .ThenBy(c => c.Release.PlatformId);

        // English fast-path: skip the correlated translation sub-query entirely. Caption and
        // CanonicalCaption are identical in this case.
        if(isEnglish)
            return source.Select(c => new SoftwareCoverDto
                          {
                              Id                = c.Id,
                              SoftwareReleaseId = c.SoftwareReleaseId,
                              ReleaseTitle      = c.Release.Title,
                              Type              = (int)c.Type,
                              TypeName          = c.Type.ToString(),
                              Caption           = c.Caption,
                              CanonicalCaption  = c.Caption,
                              OriginalExtension = c.OriginalExtension,
                              PlatformName      = c.Release.Platform != null ? c.Release.Platform.Name : null,
                              RegionNames = c.Release.Regions != null
                                                ? string.Join(", ", c.Release.Regions.Select(r => r.UnM49.Name))
                                                : null
                          })
                         .ToListAsync();

        return source.Select(c => new SoftwareCoverDto
                      {
                          Id                = c.Id,
                          SoftwareReleaseId = c.SoftwareReleaseId,
                          ReleaseTitle      = c.Release.Title,
                          Type              = (int)c.Type,
                          TypeName          = c.Type.ToString(),
                          Caption = c.Caption == null
                                        ? null
                                        : (context.SoftwareCoverCaptionTranslations
                                                  .Where(t => t.CaptionText  == c.Caption &&
                                                              t.LanguageCode == langCode)
                                                  .Select(t => t.Translation)
                                                  .FirstOrDefault() ?? c.Caption),
                          CanonicalCaption  = c.Caption,
                          OriginalExtension = c.OriginalExtension,
                          PlatformName      = c.Release.Platform != null ? c.Release.Platform.Name : null,
                          RegionNames = c.Release.Regions != null
                                            ? string.Join(", ", c.Release.Regions.Select(r => r.UnM49.Name))
                                            : null
                      })
                     .ToListAsync();
    }

    [HttpGet("{id:Guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoftwareCoverDto>> GetAsync(Guid id, [FromQuery] string lang = null)
    {
        string langCode  = LanguageResolver.Resolve(HttpContext, lang);
        bool   isEnglish = string.Equals(langCode, "eng", StringComparison.Ordinal);

        SoftwareCoverDto dto = await context.SoftwareCovers
                                            .Where(c => c.Id == id)
                                            .Select(c => new SoftwareCoverDto
                                             {
                                                 Id                = c.Id,
                                                 SoftwareReleaseId = c.SoftwareReleaseId,
                                                 ReleaseTitle      = c.Release.Title,
                                                 Type              = (int)c.Type,
                                                 TypeName          = c.Type.ToString(),
                                                 Caption = isEnglish || c.Caption == null
                                                               ? c.Caption
                                                               : (context.SoftwareCoverCaptionTranslations
                                                                         .Where(t => t.CaptionText  == c.Caption &&
                                                                                     t.LanguageCode == langCode)
                                                                         .Select(t => t.Translation)
                                                                         .FirstOrDefault() ?? c.Caption),
                                                 CanonicalCaption  = c.Caption,
                                                 OriginalExtension = c.OriginalExtension,
                                                 PlatformName = c.Release.Platform != null
                                                                    ? c.Release.Platform.Name
                                                                    : null,
                                                 RegionNames = c.Release.Regions != null
                                                                   ? string.Join(", ",
                                                                       c.Release.Regions.Select(r => r.UnM49.Name))
                                                                   : null
                                             })
                                            .FirstOrDefaultAsync();

        if(dto is null) return NotFound();

        return Ok(dto);
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SoftwareCoverDto>> UploadAsync(IFormFile                    file,
                                                                   [FromForm] ulong             releaseId,
                                                                   [FromForm] SoftwareCoverType type,
                                                                   [FromForm] string           caption)
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

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return Problem(detail: "Unsupported content type.", statusCode: StatusCodes.Status400BadRequest);

        bool releaseExists = await context.SoftwareReleases.AnyAsync(r => r.Id == (ulong)releaseId);

        if(!releaseExists)
            return Problem(detail: "Referenced software release does not exist.", statusCode: StatusCodes.Status400BadRequest);

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var model = new SoftwareCover
        {
            Id                = Guid.NewGuid(),
            SoftwareReleaseId = releaseId,
            Type              = type,
            Caption           = caption,
            OriginalExtension = extension.TrimStart('.')
        };

        Photos.EnsureCreated(_assetRootPath, false, "software-covers");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "software-covers", "originals");
        string originalPath = Path.Combine(originalsDir, $"{model.Id}{extension}");

        ms.Position = 0;

        await using(var fs = new FileStream(originalPath, FileMode.CreateNew, FileAccess.Write))
        {
            await ms.CopyToAsync(fs);
        }

        string sourceFormat = extension.TrimStart('.');

        _ = Task.Run(() =>
        {
            var photos = new Photos();

            photos.ConversionWorker(_assetRootPath, model.Id, originalPath, sourceFormat, false, "software-covers");
        });

        await context.SoftwareCovers.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new SoftwareCoverDto
        {
            Id                = model.Id,
            SoftwareReleaseId = model.SoftwareReleaseId,
            Type              = (int)model.Type,
            TypeName          = model.Type.ToString(),
            Caption           = model.Caption,
            CanonicalCaption  = model.Caption,
            OriginalExtension = model.OriginalExtension
        });
    }

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] SoftwareCoverDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareCover model = await context.SoftwareCovers
                                           .Include(c => c.Release)
                                           .ThenInclude(r => r.SoftwareVersion)
                                           .FirstOrDefaultAsync(c => c.Id == id);

        if(model is null) return NotFound();

        // Allow release reassignment only within the same software
        if(dto.SoftwareReleaseId != 0 && (ulong)dto.SoftwareReleaseId != model.SoftwareReleaseId)
        {
            SoftwareRelease newRelease = await context.SoftwareReleases
                                                      .Include(r => r.SoftwareVersion)
                                                      .FirstOrDefaultAsync(r => r.Id == (ulong)dto.SoftwareReleaseId);

            if(newRelease is null)
                return Problem(detail: "Referenced software release does not exist.", statusCode: StatusCodes.Status400BadRequest);

            ulong? currentSoftwareId = model.Release.SoftwareId ?? model.Release.SoftwareVersion?.SoftwareId;
            ulong? newSoftwareId     = newRelease.SoftwareId    ?? newRelease.SoftwareVersion?.SoftwareId;

            if(currentSoftwareId != newSoftwareId)
                return Problem(detail: "Release does not belong to the same software.", statusCode: StatusCodes.Status400BadRequest);

            model.SoftwareReleaseId = (ulong)dto.SoftwareReleaseId;
        }

        // Admin / suggestion edit submits the canonical English caption (DTO.CanonicalCaption
        // when populated; falls back to DTO.Caption for older clients that haven't migrated).
        // The translation worker fills in localized rows on its next sweep.
        model.Caption = !string.IsNullOrEmpty(dto.CanonicalCaption) ? dto.CanonicalCaption : dto.Caption;
        model.Type    = (SoftwareCoverType)dto.Type;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:Guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareCover model = await context.SoftwareCovers.FindAsync(id);

        if(model is null) return NotFound();

        context.SoftwareCovers.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        string photosRoot = Path.Combine(_assetRootPath, "photos", "software-covers");
        string guidStr    = id.ToString();

        DeleteFilesByPattern(Path.Combine(photosRoot, "originals"), $"{guidStr}.*");

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

    static void DeleteFilesByPattern(string directory, string pattern)
    {
        if(!System.IO.Directory.Exists(directory)) return;

        foreach(string file in System.IO.Directory.GetFiles(directory, pattern))
            System.IO.File.Delete(file);
    }

    /// <summary>
    ///     Maximum in-flight pending cover images a single collaborator may stage for a
    ///     given <see cref="SoftwareRelease" /> before they submit (or cancel) the suggestion.
    ///     Mirrors the per-batch cap enforced by the dialog.
    /// </summary>
    const int PendingPhotosPerUserPerReleaseCap = 30;

    /// <summary>
    ///     Allowed extensions for collaborator-uploaded cover images (narrower than the admin
    ///     upload set; must match server-side JS validation).
    /// </summary>
    static readonly HashSet<string> _pendingAllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    /// <summary>
    ///     Allowed content types for collaborator-uploaded cover images (narrower than the
    ///     admin upload set; must match server-side JS validation).
    /// </summary>
    static readonly HashSet<string> _pendingAllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    /// <summary>
    ///     Stage a single pending cover image for a brand-new collaborative suggestion. The
    ///     uploader keeps each pending file on the server (sidecar tracks ownership + parent
    ///     <c>releaseId</c>) until they call <c>POST /suggestions</c> referencing the returned
    ///     <c>guid</c>. Per-uploader cap of 30 in-flight pending images per release.
    /// </summary>
    [HttpPost("pending")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PendingImageUploadDto>> UploadPendingAsync(IFormFile         file,
                                                                              [FromQuery] ulong releaseId)
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

        bool releaseExists = await context.SoftwareReleases.AnyAsync(r => r.Id == releaseId);

        if(!releaseExists) return Problem(detail: "Software release not found.", statusCode: StatusCodes.Status404NotFound);

        long parentEntityId = (long)releaseId;

        int currentCount = PendingImageStore.CountByUploaderForParentEntity(_assetRootPath, "software-covers",
            userId, (byte)SuggestionEntityType.SoftwareCover, parentEntityId);

        if(currentCount >= PendingPhotosPerUserPerReleaseCap)
            return Problem(detail: $"You already have {currentCount} pending cover images for this release. Maximum is " +
                                   $"{PendingPhotosPerUserPerReleaseCap} per release. Submit or remove some first.",
                           statusCode: StatusCodes.Status409Conflict);

        Guid guid;

        await using(Stream stream = file.OpenReadStream())
        {
            // EntityId stays 0 because the suggestion row that will reference these images
            // doesn't exist yet. ParentEntityId carries the releaseId so the per-uploader
            // cap and cleanup-by-parent helpers can scope correctly.
            guid = await PendingImageStore.StoreAsync(_assetRootPath, "software-covers", extension,
                (byte)SuggestionEntityType.SoftwareCover, entityId: 0L, userId, file.ContentType, stream,
                parentEntityId: parentEntityId);
        }

        return Ok(new PendingImageUploadDto
        {
            Guid      = guid,
            Extension = extension.TrimStart('.')
        });
    }

    /// <summary>
    ///     Delete a pending cover image before it has been submitted as part of a suggestion.
    ///     Only the original uploader (or an admin) may delete.
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
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-covers", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.SoftwareCover) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");

        if(!PendingImageStore.CanAccess(meta, userId, isAdmin)) return Forbid();

        PendingImageStore.Delete(_assetRootPath, "software-covers", guid);

        return NoContent();
    }

    /// <summary>
    ///     Stream the binary contents of a pending cover image. Used by the dialog thumbnail
    ///     preview AND by the admin SuggestionDiffPanel preview. Auth-gated: only the
    ///     uploader and admins can read.
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
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-covers", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.SoftwareCover) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");

        if(!PendingImageStore.CanAccess(meta, userId, isAdmin)) return Forbid();

        string path = await PendingImageStore.GetImagePathAsync(_assetRootPath, "software-covers", guid);

        if(path is null || !System.IO.File.Exists(path)) return NotFound();

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return File(stream, meta.ContentType ?? "application/octet-stream");
    }

    // ────────────────────────────── Admin batch upload ──────────────────────────────

    /// <summary>
    ///     Allowed extensions accepted by the admin batch-upload staging endpoint. Wider
    ///     than both the legacy <c>/upload</c> set (which also rejects AVIF) and the
    ///     collaborator-suggestion set (which rejects everything beyond JPEG/PNG/WebP).
    /// </summary>
    static readonly HashSet<string> _adminBatchAllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".avif", ".bmp", ".tif", ".tiff"
    };

    /// <summary>
    ///     ImageMagick canonical format names (as returned by <c>identify -format %m</c>)
    ///     that we trust for admin batch uploads. The set is checked AFTER the file is
    ///     written to disk so an attacker can't bypass it by lying about the extension or
    ///     MIME type.
    /// </summary>
    static readonly HashSet<string> _adminBatchAllowedMagickFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        "JPEG", "PNG", "WEBP", "AVIF", "BMP", "TIFF"
    };

    /// <summary>
    ///     Stage a single image as part of an admin batch upload. The server writes the
    ///     file to the pending folder, content-sniffs it with ImageMagick (rejecting on
    ///     mismatch regardless of extension/MIME), generates a 256x256 thumbnail returned
    ///     inline as a JPEG data URL, and stores both file + sidecar for later commit.
    /// </summary>
    [HttpPost("admin/pending")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<AdminPendingCoverUploadDto>> UploadAdminBatchPendingAsync(IFormFile         file,
                                                                                              [FromQuery] ulong releaseId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        if(file is null || file.Length == 0) return Problem(detail: "No file provided.", statusCode: StatusCodes.Status400BadRequest);
        if(file.Length > 50 * 1024 * 1024) return Problem(detail: "File exceeds 50 MB limit.", statusCode: StatusCodes.Status400BadRequest);

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        if(!_adminBatchAllowedExtensions.Contains(extension))
            return Problem(detail: "Unsupported file format. Accepted: JPEG, PNG, WebP, AVIF, BMP, TIFF.", statusCode: StatusCodes.Status400BadRequest);

        bool releaseExists = await context.SoftwareReleases.AnyAsync(r => r.Id == releaseId);
        if(!releaseExists) return Problem(detail: "Software release not found.", statusCode: StatusCodes.Status404NotFound);

        // Persist FIRST (with a temporary placeholder for dimensions), then content-sniff
        // and either re-write the sidecar with real dimensions or reject + delete the file.
        Guid guid;
        await using(Stream stream = file.OpenReadStream())
        {
            guid = await PendingImageStore.StoreAsync(_assetRootPath, "software-covers", extension,
                (byte)SuggestionEntityType.SoftwareCover, entityId: 0L, userId, file.ContentType, stream,
                parentEntityId: (long)releaseId, _adminBatchAllowedExtensions, isAdminStaging: true,
                width: null, height: null);
        }

        string imagePath = await PendingImageStore.GetImagePathAsync(_assetRootPath, "software-covers", guid);
        if(imagePath is null)
        {
            PendingImageStore.Delete(_assetRootPath, "software-covers", guid);
            return Problem(detail: "Failed to persist uploaded file.", statusCode: StatusCodes.Status400BadRequest);
        }

        (string magickFormat, int width, int height) = Photos.Identify(imagePath);
        if(magickFormat is null || !_adminBatchAllowedMagickFormats.Contains(magickFormat))
        {
            PendingImageStore.Delete(_assetRootPath, "software-covers", guid);
            return Problem(detail: "File content does not match a supported image format.",
                           statusCode: StatusCodes.Status415UnsupportedMediaType);
        }

        byte[] thumbBytes = Photos.GenerateThumbnailJpeg(imagePath);
        if(thumbBytes is null)
        {
            PendingImageStore.Delete(_assetRootPath, "software-covers", guid);
            return Problem(detail: "Failed to generate thumbnail for the uploaded image.", statusCode: StatusCodes.Status400BadRequest);
        }

        await PendingImageStore.StoreThumbnailAsync(_assetRootPath, "software-covers", guid, thumbBytes);

        // Re-write the sidecar so dimensions are captured for future consumers.
        PendingImageStore.PendingMetadata meta =
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-covers", guid);
        if(meta is not null)
        {
            meta.Width  = width;
            meta.Height = height;
            string sidecar = Path.Combine(_assetRootPath, "photos", "software-covers", "pending",
                                          guid.ToString() + ".json");
            await System.IO.File.WriteAllTextAsync(sidecar, System.Text.Json.JsonSerializer.Serialize(meta));
        }

        string dataUrl = "data:image/jpeg;base64," + Convert.ToBase64String(thumbBytes);

        return Ok(new AdminPendingCoverUploadDto
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
    ///     Delete a single admin-staged pending image before commit. Owner-only (an admin
    ///     cannot delete another admin's in-flight staging entries).
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
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-covers", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.SoftwareCover) return NotFound();
        if(!meta.IsAdminStaging) return NotFound();
        if(!string.Equals(meta.UploadedById, userId, StringComparison.Ordinal)) return Forbid();

        PendingImageStore.Delete(_assetRootPath, "software-covers", guid);
        return NoContent();
    }

    /// <summary>
    ///     Commit a batch of admin-staged pending images into permanent <c>SoftwareCover</c>
    ///     rows. The endpoint returns 202 with a <c>jobId</c> immediately and processes the
    ///     batch on a background task; the client polls
    ///     <c>GET /software/covers/admin/batch/{jobId}/status</c> for progress and per-item
    ///     results. Each image runs through the standard 8-variant conversion (JPEG/WebP/
    ///     AVIF × full+thumb) sequentially so the progress bar advances one image at a
    ///     time.
    /// </summary>
    [HttpPost("admin/batch/commit")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(typeof(AdminBatchJobStatusDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminBatchJobStatusDto>> CommitAdminBatchAsync(
        [FromBody] AdminBatchCommitRequestDto request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        if(request is null || request.Items is null || request.Items.Count == 0)
            return Problem(detail: "No items provided.", statusCode: StatusCodes.Status400BadRequest);

        bool releaseExists = await context.SoftwareReleases.AnyAsync(r => r.Id == request.SoftwareReleaseId);
        if(!releaseExists) return Problem(detail: "Software release not found.", statusCode: StatusCodes.Status404NotFound);

        // Validate every pending id belongs to the caller and matches release/admin scope.
        foreach(AdminBatchCommitItemDto item in request.Items)
        {
            PendingImageStore.PendingMetadata meta =
                await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-covers", item.PendingId);

            if(meta is null) return Problem(detail: $"Pending image {item.PendingId} not found.", statusCode: StatusCodes.Status404NotFound);
            if(meta.EntityType != (byte)SuggestionEntityType.SoftwareCover)
                return Problem(detail: $"Pending image {item.PendingId} is not a software cover.", statusCode: StatusCodes.Status400BadRequest);
            if(!meta.IsAdminStaging)
                return Problem(detail: $"Pending image {item.PendingId} is not an admin-staged image.", statusCode: StatusCodes.Status400BadRequest);
            if(!string.Equals(meta.UploadedById, userId, StringComparison.Ordinal))
                return Forbid();
            if(meta.ParentEntityId != (long)request.SoftwareReleaseId)
                return Problem(detail: $"Pending image {item.PendingId} was staged for a different release.", statusCode: StatusCodes.Status400BadRequest);
            if(!Enum.IsDefined(typeof(SoftwareCoverType), (byte)item.Type))
                return Problem(detail: $"Invalid cover type {item.Type} for pending image {item.PendingId}.", statusCode: StatusCodes.Status400BadRequest);
        }

        Guid jobId = batchJobs.StartJob(userId, (long)request.SoftwareReleaseId, request.Items.Count);

        // Fire the worker. The worker uses its own DI scope (created from IServiceScopeFactory
        // injected via the controller's RequestServices) so DbContext lifetimes are correct.
        var scopeFactory =
            HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        string assetRootPath = _assetRootPath;
        List<AdminBatchCommitItemDto> items = request.Items;
        ulong releaseId      = request.SoftwareReleaseId;

        _ = Task.Run(() => RunBatchJobAsync(jobId, userId, releaseId, items, assetRootPath, scopeFactory));

        AdminBatchJobStatusDto snapshot = SnapshotJob(batchJobs.GetForOwner(jobId, userId)!);
        return Accepted(snapshot);
    }

    /// <summary>
    ///     Poll the status of an in-flight admin batch-commit job. Owner-only.
    /// </summary>
    [HttpGet("admin/batch/{jobId:guid}/status")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AdminBatchJobStatusDto> GetAdminBatchStatus(Guid jobId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        BatchUploadJobStore.BatchJob job = batchJobs.GetForOwner(jobId, userId);
        if(job is null) return NotFound();

        return Ok(SnapshotJob(job));
    }

    static AdminBatchJobStatusDto SnapshotJob(BatchUploadJobStore.BatchJob job)
    {
        lock(job.Lock)
        {
            return new AdminBatchJobStatusDto
            {
                JobId            = job.JobId,
                State            = (int)job.State,
                Total            = job.Total,
                Processed        = job.Processed,
                CurrentPendingId = job.CurrentPendingId,
                Results          = job.Results.Select(r => new AdminBatchJobItemResultDto
                                       {
                                           PendingId = r.PendingId,
                                           Succeeded = r.Succeeded,
                                           CoverId   = r.AssignedId,
                                           Error     = r.Error
                                       })
                                       .ToList()
            };
        }
    }

    /// <summary>
    ///     Sequential worker that promotes each pending image into a SoftwareCover row,
    ///     runs the 8-variant conversion synchronously per image (so the progress bar
    ///     advances one-at-a-time), and records per-item success/failure on the job.
    /// </summary>
    async Task RunBatchJobAsync(Guid jobId, string userId, ulong releaseId,
                                List<AdminBatchCommitItemDto> items, string assetRootPath,
                                Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory)
    {
        BatchUploadJobStore.BatchJob job = batchJobs.GetForOwner(jobId, userId);
        if(job is null) return;

        lock(job.Lock) { job.State = BatchJobState.Processing; }

        Photos.EnsureCreated(assetRootPath, false, "software-covers");

        for(int i = 0; i < items.Count; i++)
        {
            AdminBatchCommitItemDto item = items[i];

            lock(job.Lock)
            {
                job.CurrentPendingId = item.PendingId;
                job.Processed        = i;
                job.LastTouchedOn    = DateTime.UtcNow;
            }

            try
            {
                (string movedPath, string ext) = await PendingImageStore.PromoteToOriginalsAsync(assetRootPath,
                                                     "software-covers", item.PendingId);

                if(movedPath is null)
                {
                    AppendResult(job, item.PendingId, false, null, "Pending file missing at commit time.");
                    continue;
                }

                // Rename the moved file to use a fresh cover guid so its filename matches the DB row id.
                Guid   coverId       = Guid.NewGuid();
                string originalsDir  = Path.Combine(assetRootPath, "photos", "software-covers", "originals");
                string finalPath     = Path.Combine(originalsDir, coverId.ToString() + "." + ext);
                System.IO.File.Move(movedPath, finalPath, overwrite: true);

                using(Microsoft.Extensions.DependencyInjection.IServiceScope scope = scopeFactory.CreateScope())
                {
                    var ctx = scope.ServiceProvider
                                   .GetRequiredService<MarechaiContext>();
                    var model = new SoftwareCover
                    {
                        Id                = coverId,
                        SoftwareReleaseId = releaseId,
                        Type              = (SoftwareCoverType)(byte)item.Type,
                        Caption           = string.IsNullOrWhiteSpace(item.Caption) ? null : item.Caption.Trim(),
                        OriginalExtension = ext
                    };

                    await ctx.SoftwareCovers.AddAsync(model);
                    await ctx.SaveChangesWithUserAsync(userId);
                }

                // Run the 8-variant conversion synchronously so the progress bar advances
                // only after every variant for this image has landed.
                var photos = new Photos();
                photos.ConversionWorker(assetRootPath, coverId, finalPath, ext, false, "software-covers");

                AppendResult(job, item.PendingId, true, coverId, null);
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

    static void AppendResult(BatchUploadJobStore.BatchJob job, Guid pendingId, bool ok, Guid? coverId, string error)
    {
        lock(job.Lock)
        {
            job.Results.Add(new BatchJobItemResult
            {
                PendingId  = pendingId,
                Succeeded  = ok,
                AssignedId = coverId,
                Error      = error
            });
            job.LastTouchedOn = DateTime.UtcNow;
        }
    }
}
