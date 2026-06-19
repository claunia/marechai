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

[Route("/machines/promo-art")]
[ApiController]
public class MachinePromoArtController(MarechaiContext context, IConfiguration configuration) : ControllerBase
{
    static readonly HashSet<string> _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"
    ];

    static readonly HashSet<string> _pendingAllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    static readonly HashSet<string> _pendingAllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    const int PendingPhotosPerUserPerMachineCap = 30;

    readonly string _assetRootPath = configuration["AssetRootPath"]!;

    [HttpGet("/machines/{machineId:int}/promo-art")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<MachinePromoArtDto>> GetByMachineAsync(int machineId, [FromQuery] string lang = null)
    {
        string langCode = LanguageResolver.Resolve(HttpContext, lang);

        if(string.Equals(langCode, "eng", StringComparison.Ordinal))
            return context.MachinePromoArt
                          .Where(p => p.MachineId == machineId)
                          .OrderBy(p => p.Group.Name)
                          .ThenBy(p => p.CreatedOn)
                          .ThenBy(p => p.Id)
                          .Select(p => new MachinePromoArtDto
                           {
                               Id = p.Id,
                               MachineId = p.MachineId,
                               GroupId = p.GroupId,
                               GroupName = p.Group.Name,
                               Caption = p.Caption,
                               OriginalExtension = p.OriginalExtension
                           })
                          .ToListAsync();

        return context.MachinePromoArt
                      .Where(p => p.MachineId == machineId)
                      .OrderBy(p => p.Group.Name)
                      .ThenBy(p => p.CreatedOn)
                      .ThenBy(p => p.Id)
                      .Select(p => new MachinePromoArtDto
                       {
                           Id = p.Id,
                           MachineId = p.MachineId,
                           GroupId = p.GroupId,
                           GroupName = context.SoftwarePromoArtGroupTranslations
                                              .Where(t => t.GroupId == p.GroupId && t.LanguageCode == langCode)
                                              .Select(t => t.Name)
                                              .FirstOrDefault() ?? p.Group.Name,
                           Caption = p.Caption,
                           OriginalExtension = p.OriginalExtension
                       })
                      .ToListAsync();
    }

    [HttpGet("{id:Guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MachinePromoArtDto>> GetAsync(Guid id, [FromQuery] string lang = null)
    {
        string langCode = LanguageResolver.Resolve(HttpContext, lang);
        bool isEnglish = string.Equals(langCode, "eng", StringComparison.Ordinal);

        var promo = await context.MachinePromoArt
                                 .Where(p => p.Id == id)
                                 .Select(p => new MachinePromoArtDto
                                  {
                                      Id = p.Id,
                                      MachineId = p.MachineId,
                                      GroupId = p.GroupId,
                                      GroupName = isEnglish
                                                      ? p.Group.Name
                                                      : context.SoftwarePromoArtGroupTranslations
                                                               .Where(t => t.GroupId == p.GroupId &&
                                                                           t.LanguageCode == langCode)
                                                               .Select(t => t.Name)
                                                               .FirstOrDefault() ?? p.Group.Name,
                                      Caption = p.Caption,
                                      OriginalExtension = p.OriginalExtension
                                  })
                                 .FirstOrDefaultAsync();

        if(promo is null) return NotFound();
        return promo;
    }

    [HttpGet("groups")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<SoftwarePromoArtGroupDto>> GetGroupsAsync([FromQuery] string lang = null)
    {
        string langCode = LanguageResolver.Resolve(HttpContext, lang);
        bool isEnglish = string.Equals(langCode, "eng", StringComparison.Ordinal);

        List<SoftwarePromoArtGroupDto> groups = isEnglish
                                                    ? await context.SoftwarePromoArtGroups
                                                                   .Select(g => new SoftwarePromoArtGroupDto
                                                                    {
                                                                        Id = g.Id,
                                                                        Name = g.Name,
                                                                        CanonicalName = g.Name
                                                                    })
                                                                   .ToListAsync()
                                                    : await context.SoftwarePromoArtGroups
                                                                   .Select(g => new SoftwarePromoArtGroupDto
                                                                    {
                                                                        Id = g.Id,
                                                                        Name = context.SoftwarePromoArtGroupTranslations
                                                                                      .Where(t => t.GroupId == g.Id &&
                                                                                                  t.LanguageCode == langCode)
                                                                                      .Select(t => t.Name)
                                                                                      .FirstOrDefault() ?? g.Name,
                                                                        CanonicalName = g.Name
                                                                    })
                                                                   .ToListAsync();

        groups.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
        return groups;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MachinePromoArtDto>> UploadAsync(IFormFile file,
                                                                    [FromForm] int machineId,
                                                                    [FromForm] string groupName,
                                                                    [FromForm] string caption)
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

        string trimmedGroup = groupName?.Trim();
        if(string.IsNullOrWhiteSpace(trimmedGroup))
            return Problem(detail: "Group name is required.", statusCode: StatusCodes.Status400BadRequest);
        if(trimmedGroup.Length > 256)
            return Problem(detail: "Group name exceeds 256 characters.", statusCode: StatusCodes.Status400BadRequest);

        bool machineExists = await context.Machines.AnyAsync(m => m.Id == machineId);
        if(!machineExists)
            return Problem(detail: "Referenced machine does not exist.", statusCode: StatusCodes.Status400BadRequest);

        SoftwarePromoArtGroup group =
            await context.SoftwarePromoArtGroups.FirstOrDefaultAsync(g => g.Name == trimmedGroup);

        if(group is null)
        {
            group = new SoftwarePromoArtGroup { Name = trimmedGroup };
            await context.SoftwarePromoArtGroups.AddAsync(group);
            await context.SaveChangesWithUserAsync(userId);
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var model = new MachinePromoArt
        {
            Id = Guid.NewGuid(),
            MachineId = machineId,
            GroupId = group.Id,
            Caption = string.IsNullOrWhiteSpace(caption) ? null : caption,
            OriginalExtension = extension.TrimStart('.')
        };

        Photos.EnsureCreated(_assetRootPath, false, "machine-promo-art");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "machine-promo-art", "originals");
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
            photos.ConversionWorker(_assetRootPath, model.Id, originalPath, sourceFormat, false, "machine-promo-art");
        });

        await context.MachinePromoArt.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new MachinePromoArtDto
        {
            Id = model.Id,
            MachineId = model.MachineId,
            GroupId = model.GroupId,
            GroupName = group.Name,
            Caption = model.Caption,
            OriginalExtension = model.OriginalExtension
        });
    }

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] UpdateMachinePromoArtRequest dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        MachinePromoArt model = await context.MachinePromoArt.FirstOrDefaultAsync(p => p.Id == id);
        if(model is null) return NotFound();

        int oldGroupId = model.GroupId;

        if(dto.GroupName is not null)
        {
            string trimmedGroup = dto.GroupName.Trim();
            if(string.IsNullOrWhiteSpace(trimmedGroup))
                return Problem(detail: "Group name cannot be empty.", statusCode: StatusCodes.Status400BadRequest);
            if(trimmedGroup.Length > 256)
                return Problem(detail: "Group name exceeds 256 characters.", statusCode: StatusCodes.Status400BadRequest);

            SoftwarePromoArtGroup group =
                await context.SoftwarePromoArtGroups.FirstOrDefaultAsync(g => g.Name == trimmedGroup);

            if(group is null)
            {
                group = new SoftwarePromoArtGroup { Name = trimmedGroup };
                await context.SoftwarePromoArtGroups.AddAsync(group);
                await context.SaveChangesWithUserAsync(userId);
            }

            model.GroupId = group.Id;
        }

        model.Caption = string.IsNullOrWhiteSpace(dto.Caption) ? null : dto.Caption;
        await context.SaveChangesWithUserAsync(userId);

        if(model.GroupId != oldGroupId)
            await DeleteOrphanGroupIfUnusedAsync(oldGroupId, userId);

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

        MachinePromoArt model = await context.MachinePromoArt.FirstOrDefaultAsync(p => p.Id == id);
        if(model is null) return NotFound();

        int oldGroupId = model.GroupId;

        context.MachinePromoArt.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        await DeleteOrphanGroupIfUnusedAsync(oldGroupId, userId);

        string photosRoot = Path.Combine(_assetRootPath, "photos", "machine-promo-art");
        string guidStr = id.ToString();

        DeleteFilesByPattern(Path.Combine(photosRoot, "originals"), $"{guidStr}.*");

        string[] formats = ["jpeg", "webp", "avif"];
        string[] resolutions = ["4k"];

        foreach(string format in formats)
        {
            string ext = format switch
            {
                "jpeg" => ".jpg",
                "webp" => ".webp",
                "avif" => ".avif",
                _ => $".{format}"
            };

            foreach(string res in resolutions)
            {
                string fullPath = Path.Combine(photosRoot, format, res, $"{guidStr}{ext}");
                string thumbPath = Path.Combine(photosRoot, "thumbs", format, res, $"{guidStr}{ext}");

                if(System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);

                if(System.IO.File.Exists(thumbPath))
                    System.IO.File.Delete(thumbPath);
            }
        }

        return Ok();
    }

    [HttpPost("pending")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PendingImageUploadDto>> UploadPendingAsync(IFormFile file,
                                                                              [FromQuery] int machineId)
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

        bool machineExists = await context.Machines.AnyAsync(m => m.Id == machineId);
        if(!machineExists) return Problem(detail: "Machine not found.", statusCode: StatusCodes.Status404NotFound);

        int currentCount = PendingImageStore.CountByUploaderForParentEntity(_assetRootPath, "machine-promo-art",
            userId, (byte)SuggestionEntityType.MachinePromoArt, machineId);

        if(currentCount >= PendingPhotosPerUserPerMachineCap)
            return Problem(detail: $"You already have {currentCount} pending promo art images for this machine. Maximum is " +
                                   $"{PendingPhotosPerUserPerMachineCap} per machine. Submit or remove some first.",
                           statusCode: StatusCodes.Status409Conflict);

        Guid guid;
        await using(Stream stream = file.OpenReadStream())
        {
            guid = await PendingImageStore.StoreAsync(_assetRootPath, "machine-promo-art", extension,
                (byte)SuggestionEntityType.MachinePromoArt, entityId: 0L, userId, file.ContentType, stream,
                parentEntityId: machineId);
        }

        return Ok(new PendingImageUploadDto
        {
            Guid = guid,
            Extension = extension.TrimStart('.')
        });
    }

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
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "machine-promo-art", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.MachinePromoArt) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");
        if(!PendingImageStore.CanAccess(meta, userId, isAdmin)) return Forbid();

        PendingImageStore.Delete(_assetRootPath, "machine-promo-art", guid);
        return NoContent();
    }

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
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "machine-promo-art", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.MachinePromoArt) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");
        if(!PendingImageStore.CanAccess(meta, userId, isAdmin)) return Forbid();

        string path = await PendingImageStore.GetImagePathAsync(_assetRootPath, "machine-promo-art", guid);
        if(path is null || !System.IO.File.Exists(path)) return NotFound();

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, meta.ContentType ?? "application/octet-stream");
    }

    async Task DeleteOrphanGroupIfUnusedAsync(int groupId, string userId)
    {
        bool usedBySoftware = await context.SoftwarePromoArt.AnyAsync(p => p.GroupId == groupId);
        bool usedByMachine = await context.MachinePromoArt.AnyAsync(p => p.GroupId == groupId);

        if(usedBySoftware || usedByMachine) return;

        SoftwarePromoArtGroup oldGroup =
            await context.SoftwarePromoArtGroups.FirstOrDefaultAsync(g => g.Id == groupId);

        if(oldGroup is not null)
        {
            context.SoftwarePromoArtGroups.Remove(oldGroup);
            await context.SaveChangesWithUserAsync(userId);
        }
    }

    static void DeleteFilesByPattern(string directory, string pattern)
    {
        if(!System.IO.Directory.Exists(directory)) return;

        foreach(string file in System.IO.Directory.GetFiles(directory, pattern))
            System.IO.File.Delete(file);
    }
}
