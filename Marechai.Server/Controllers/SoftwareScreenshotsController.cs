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

namespace Marechai.Server.Controllers;

[Route("/software/screenshots")]
[ApiController]
public class SoftwareScreenshotsController(MarechaiContext context, IConfiguration configuration) : ControllerBase
{
    /// <summary>
    ///     Per-uploader cap of in-flight pending screenshot images per Software for the
    ///     collaborative suggestion flow.
    /// </summary>
    const int PendingScreenshotsPerUserPerSoftwareCap = 50;

    static readonly HashSet<string> _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"
    ];

    /// <summary>
    ///     Narrower allow-set for collaborator pending uploads (jpg/png/webp ONLY). The
    ///     existing admin upload endpoint keeps the broader <see cref="_allowedExtensions" />
    ///     set including tiff/bmp.
    /// </summary>
    static readonly HashSet<string> _pendingAllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    static readonly HashSet<string> _pendingAllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    readonly string _assetRootPath = configuration["AssetRootPath"]!;

    [HttpGet("/software/{softwareId}/screenshots")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<Guid>> GetGuidsBySoftwareAsync(ulong softwareId) =>
        context.SoftwareScreenshots
               .Where(s => s.SoftwareId == softwareId)
               .OrderBy(s => s.CreatedOn)
               .ThenBy(s => s.Id)
               .Select(s => s.Id)
               .ToListAsync();

    [HttpGet("/software/{softwareId}/platforms/{platformId}/screenshots")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<Guid>> GetGuidsByPlatformAsync(ulong softwareId, ulong platformId) =>
        context.SoftwareScreenshots
               .Where(s => s.SoftwareId == softwareId && s.SoftwarePlatformId == platformId)
               .OrderBy(s => s.CreatedOn)
               .ThenBy(s => s.Id)
               .Select(s => s.Id)
               .ToListAsync();

    [HttpGet("/software/{softwareId}/versions/{versionId}/screenshots")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<Guid>> GetGuidsByVersionAsync(ulong softwareId, ulong versionId) =>
        context.SoftwareScreenshots
               .Where(s => s.SoftwareId == softwareId && s.SoftwareVersionId == versionId)
               .OrderBy(s => s.CreatedOn)
               .ThenBy(s => s.Id)
               .Select(s => s.Id)
               .ToListAsync();

    [HttpGet("{id:Guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoftwareScreenshotDto>> GetAsync(Guid id)
    {
        SoftwareScreenshotDto dto = await context.SoftwareScreenshots
                                                  .Where(s => s.Id == id)
                                                  .Select(s => new SoftwareScreenshotDto
                                                   {
                                                       Id                 = s.Id,
                                                       SoftwareId         = s.SoftwareId,
                                                       SoftwareName       = s.Software.Name,
                                                       SoftwarePlatformId = s.SoftwarePlatformId,
                                                       PlatformName       = s.Platform != null ? s.Platform.Name : null,
                                                       SoftwareVersionId  = s.SoftwareVersionId,
                                                       VersionString = s.Version != null ? s.Version.VersionString : null,
                                                       Caption            = s.Caption,
                                                       OriginalExtension  = s.OriginalExtension
                                                   })
                                                  .FirstOrDefaultAsync();

        if(dto is null) return NotFound();

        return Ok(dto);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwareScreenshotDto>> GetAllAsync() =>
        context.SoftwareScreenshots
               .Select(s => new SoftwareScreenshotDto
                {
                    Id                 = s.Id,
                    SoftwareId         = s.SoftwareId,
                    SoftwareName       = s.Software.Name,
                    SoftwarePlatformId = s.SoftwarePlatformId,
                    PlatformName       = s.Platform != null ? s.Platform.Name : null,
                    SoftwareVersionId  = s.SoftwareVersionId,
                    VersionString      = s.Version != null ? s.Version.VersionString : null,
                    Caption            = s.Caption,
                    OriginalExtension  = s.OriginalExtension
                })
               .ToListAsync();

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SoftwareScreenshotDto>> UploadAsync(IFormFile            file,
                                                                       [FromForm] ulong     softwareId,
                                                                       [FromForm] ulong?    softwarePlatformId,
                                                                       [FromForm] ulong?    softwareVersionId,
                                                                       [FromForm] string   caption)
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

        // Validate that the referenced software exists
        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == softwareId);

        if(!softwareExists)
            return BadRequest("Referenced software does not exist.");

        if(softwarePlatformId.HasValue)
        {
            bool platformExists = await context.SoftwarePlatforms.AnyAsync(p => p.Id == softwarePlatformId.Value);

            if(!platformExists)
                return BadRequest("Referenced software platform does not exist.");
        }

        if(softwareVersionId.HasValue)
        {
            bool versionExists = await context.SoftwareVersions.AnyAsync(v => v.Id == softwareVersionId.Value &&
                                                                              v.SoftwareId == softwareId);

            if(!versionExists)
                return BadRequest("Referenced software version does not exist or does not belong to this software.");
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var model = new SoftwareScreenshot
        {
            Id                 = Guid.NewGuid(),
            SoftwareId         = softwareId,
            SoftwarePlatformId = softwarePlatformId,
            SoftwareVersionId  = softwareVersionId,
            Caption            = caption,
            OriginalExtension  = extension.TrimStart('.')
        };

        // Save original file to disk
        Photos.EnsureCreated(_assetRootPath, false, "software-screenshots");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "software-screenshots", "originals");
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

            photos.ConversionWorker(_assetRootPath, model.Id, originalPath, sourceFormat, false,
                                    "software-screenshots");
        });

        // Save to database
        await context.SoftwareScreenshots.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new SoftwareScreenshotDto
        {
            Id                 = model.Id,
            SoftwareId         = model.SoftwareId,
            SoftwarePlatformId = model.SoftwarePlatformId,
            SoftwareVersionId  = model.SoftwareVersionId,
            Caption            = model.Caption,
            OriginalExtension  = model.OriginalExtension
        });
    }

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] SoftwareScreenshotDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareScreenshot model = await context.SoftwareScreenshots.FindAsync(id);

        if(model is null) return NotFound();

        model.Caption            = dto.Caption;
        model.SoftwarePlatformId = dto.SoftwarePlatformId;
        model.SoftwareVersionId  = dto.SoftwareVersionId;

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

        SoftwareScreenshot model = await context.SoftwareScreenshots.FindAsync(id);

        if(model is null) return NotFound();

        context.SoftwareScreenshots.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        // Delete all generated files from disk
        string photosRoot = Path.Combine(_assetRootPath, "photos", "software-screenshots");
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

    static void DeleteFilesByPattern(string directory, string pattern)
    {
        if(!System.IO.Directory.Exists(directory)) return;

        foreach(string file in System.IO.Directory.GetFiles(directory, pattern))
            System.IO.File.Delete(file);
    }

    // ─────────────── Collaborative pending-image endpoints ───────────────
    // These endpoints back the SoftwareScreenshotsSuggestionDialog. Each upload writes a
    // file + sidecar to disk under photos/software-screenshots/pending/ and the
    // collaborator references the returned guid when calling POST /suggestions. Promotion
    // to originals/ happens in SoftwareScreenshotSuggestionApplier.ApplyAsync once an admin
    // accepts the per-image accept-key (screenshot.<guid>).

    /// <summary>
    ///     Stage a single pending screenshot image for a brand-new collaborative suggestion.
    ///     The uploader keeps each pending file on the server (sidecar tracks ownership +
    ///     parent <c>softwareId</c>) until they call <c>POST /suggestions</c> referencing the
    ///     returned <c>guid</c>. Per-uploader cap of 50 in-flight pending images per
    ///     Software.
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
                                                                              [FromQuery] ulong softwareId)
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

        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == softwareId);

        if(!softwareExists) return NotFound("Software not found.");

        long parentEntityId = (long)softwareId;

        int currentCount = PendingImageStore.CountByUploaderForParentEntity(_assetRootPath, "software-screenshots",
            userId, (byte)SuggestionEntityType.SoftwareScreenshot, parentEntityId);

        if(currentCount >= PendingScreenshotsPerUserPerSoftwareCap)
            return Conflict($"You already have {currentCount} pending screenshot images for this software. Maximum " +
                            $"is {PendingScreenshotsPerUserPerSoftwareCap} per software. Submit or remove some first.");

        Guid guid;

        await using(Stream stream = file.OpenReadStream())
        {
            // EntityId stays 0 because the suggestion row that will reference these images
            // doesn't exist yet. ParentEntityId carries the softwareId so the per-uploader
            // cap and cleanup-by-parent helpers can scope correctly.
            guid = await PendingImageStore.StoreAsync(_assetRootPath, "software-screenshots", extension,
                (byte)SuggestionEntityType.SoftwareScreenshot, entityId: 0L, userId, file.ContentType, stream,
                parentEntityId: parentEntityId);
        }

        return Ok(new PendingImageUploadDto
        {
            Guid      = guid,
            Extension = extension.TrimStart('.')
        });
    }

    /// <summary>
    ///     Delete a pending screenshot image before it has been submitted as part of a
    ///     suggestion. Only the original uploader (or an admin) may delete.
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
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-screenshots", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.SoftwareScreenshot) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");

        if(!PendingImageStore.CanAccess(meta, userId, isAdmin)) return Forbid();

        PendingImageStore.Delete(_assetRootPath, "software-screenshots", guid);

        return NoContent();
    }

    /// <summary>
    ///     Stream the binary contents of a pending screenshot image. Used by the dialog
    ///     thumbnail preview AND by the admin SuggestionDiffPanel preview. Auth-gated: only
    ///     the uploader and admins can read.
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
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-screenshots", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.SoftwareScreenshot) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");

        if(!PendingImageStore.CanAccess(meta, userId, isAdmin)) return Forbid();

        string path = await PendingImageStore.GetImagePathAsync(_assetRootPath, "software-screenshots", guid);

        if(path is null || !System.IO.File.Exists(path)) return NotFound();

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return File(stream, meta.ContentType ?? "application/octet-stream");
    }
}
