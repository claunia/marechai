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

namespace Marechai.Server.Controllers;

[Route("/software/covers")]
[ApiController]
public class SoftwareCoversController(MarechaiContext context, IConfiguration configuration) : ControllerBase
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
            return BadRequest("No file provided.");

        if(file.Length > 50 * 1024 * 1024)
            return BadRequest("File exceeds 50 MB limit.");

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        if(!_allowedExtensions.Contains(extension))
            return BadRequest("Unsupported file format. Accepted: JPEG, PNG, WebP, TIFF, BMP.");

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Unsupported content type.");

        bool releaseExists = await context.SoftwareReleases.AnyAsync(r => r.Id == (ulong)releaseId);

        if(!releaseExists)
            return BadRequest("Referenced software release does not exist.");

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
                return BadRequest("Referenced software release does not exist.");

            ulong? currentSoftwareId = model.Release.SoftwareId ?? model.Release.SoftwareVersion?.SoftwareId;
            ulong? newSoftwareId     = newRelease.SoftwareId    ?? newRelease.SoftwareVersion?.SoftwareId;

            if(currentSoftwareId != newSoftwareId)
                return BadRequest("Release does not belong to the same software.");

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

        string[] formats     = ["jpeg", "webp", "avif", "jxl"];
        string[] resolutions = ["4k"];

        foreach(string format in formats)
        {
            string ext = format switch
            {
                "jpeg" => ".jpg",
                "webp" => ".webp",
                "avif" => ".avif",
                "jxl"  => ".jxl",
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

        if(file is null || file.Length == 0) return BadRequest("No file provided.");

        if(file.Length > 50 * 1024 * 1024) return BadRequest("File exceeds 50 MB limit.");

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        if(!_pendingAllowedExtensions.Contains(extension))
            return BadRequest("Unsupported file format. Accepted: JPEG, PNG, WebP.");

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_pendingAllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Unsupported content type.");

        bool releaseExists = await context.SoftwareReleases.AnyAsync(r => r.Id == releaseId);

        if(!releaseExists) return NotFound("Software release not found.");

        long parentEntityId = (long)releaseId;

        int currentCount = PendingImageStore.CountByUploaderForParentEntity(_assetRootPath, "software-covers",
            userId, (byte)SuggestionEntityType.SoftwareCover, parentEntityId);

        if(currentCount >= PendingPhotosPerUserPerReleaseCap)
            return Conflict($"You already have {currentCount} pending cover images for this release. Maximum is " +
                            $"{PendingPhotosPerUserPerReleaseCap} per release. Submit or remove some first.");

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
}
