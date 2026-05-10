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
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marechai.Server.Controllers;

[Route("/software/promo-art")]
[ApiController]
public class SoftwarePromoArtController(MarechaiContext context, IConfiguration configuration) : ControllerBase
{
    static readonly HashSet<string> _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"
    ];

    readonly string _assetRootPath = configuration["AssetRootPath"]!;

    [HttpGet("/software/{softwareId:ulong}/promo-art")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwarePromoArtDto>> GetBySoftwareAsync(ulong softwareId) =>
        context.SoftwarePromoArt
               .Where(p => p.SoftwareId == softwareId)
               .OrderBy(p => p.Group.Name)
               .ThenBy(p => p.CreatedOn)
               .ThenBy(p => p.Id)
               .Select(p => new SoftwarePromoArtDto
                {
                    Id                = p.Id,
                    SoftwareId        = p.SoftwareId,
                    GroupId           = p.GroupId,
                    GroupName         = p.Group.Name,
                    Caption           = p.Caption,
                    OriginalExtension = p.OriginalExtension
                })
               .ToListAsync();

    [HttpGet("{id:Guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoftwarePromoArtDto>> GetAsync(Guid id)
    {
        var promo = await context.SoftwarePromoArt
                                 .Where(p => p.Id == id)
                                 .Select(p => new SoftwarePromoArtDto
                                  {
                                      Id                = p.Id,
                                      SoftwareId        = p.SoftwareId,
                                      GroupId           = p.GroupId,
                                      GroupName         = p.Group.Name,
                                      Caption           = p.Caption,
                                      OriginalExtension = p.OriginalExtension
                                  })
                                 .FirstOrDefaultAsync();

        if(promo is null) return NotFound();

        return promo;
    }

    [HttpGet("groups")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<SoftwarePromoArtGroupDto>> GetGroupsAsync() =>
        context.SoftwarePromoArtGroups
               .OrderBy(g => g.Name)
               .Select(g => new SoftwarePromoArtGroupDto
                {
                    Id   = g.Id,
                    Name = g.Name
                })
               .ToListAsync();

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SoftwarePromoArtDto>> UploadAsync(IFormFile         file,
                                                                     [FromForm] ulong  softwareId,
                                                                     [FromForm] string groupName,
                                                                     [FromForm] string caption)
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

        string trimmedGroup = groupName?.Trim();

        if(string.IsNullOrWhiteSpace(trimmedGroup))
            return BadRequest("Group name is required.");

        if(trimmedGroup.Length > 256)
            return BadRequest("Group name exceeds 256 characters.");

        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == softwareId);

        if(!softwareExists)
            return BadRequest("Referenced software does not exist.");

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

        var model = new SoftwarePromoArt
        {
            Id                = Guid.NewGuid(),
            SoftwareId        = softwareId,
            GroupId           = group.Id,
            Caption           = string.IsNullOrWhiteSpace(caption) ? null : caption,
            OriginalExtension = extension.TrimStart('.')
        };

        Photos.EnsureCreated(_assetRootPath, false, "software-promo-art");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "software-promo-art", "originals");
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

            photos.ConversionWorker(_assetRootPath, model.Id, originalPath, sourceFormat, false,
                                    "software-promo-art");
        });

        await context.SoftwarePromoArt.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new SoftwarePromoArtDto
        {
            Id                = model.Id,
            SoftwareId        = model.SoftwareId,
            GroupId           = model.GroupId,
            GroupName         = group.Name,
            Caption           = model.Caption,
            OriginalExtension = model.OriginalExtension
        });
    }

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] UpdateSoftwarePromoArtRequest dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwarePromoArt model = await context.SoftwarePromoArt.FirstOrDefaultAsync(p => p.Id == id);

        if(model is null) return NotFound();

        int oldGroupId = model.GroupId;

        if(dto.GroupName is not null)
        {
            string trimmedGroup = dto.GroupName.Trim();

            if(string.IsNullOrWhiteSpace(trimmedGroup))
                return BadRequest("Group name cannot be empty.");

            if(trimmedGroup.Length > 256)
                return BadRequest("Group name exceeds 256 characters.");

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

        // Auto-purge orphan group if reassignment left it empty.
        if(model.GroupId != oldGroupId)
        {
            bool oldGroupHasItems = await context.SoftwarePromoArt.AnyAsync(p => p.GroupId == oldGroupId);

            if(!oldGroupHasItems)
            {
                SoftwarePromoArtGroup oldGroup =
                    await context.SoftwarePromoArtGroups.FirstOrDefaultAsync(g => g.Id == oldGroupId);

                if(oldGroup is not null)
                {
                    context.SoftwarePromoArtGroups.Remove(oldGroup);
                    await context.SaveChangesWithUserAsync(userId);
                }
            }
        }

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

        SoftwarePromoArt model = await context.SoftwarePromoArt.FirstOrDefaultAsync(p => p.Id == id);

        if(model is null) return NotFound();

        int oldGroupId = model.GroupId;

        context.SoftwarePromoArt.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        // Auto-purge orphan group if it became empty.
        bool oldGroupHasItems = await context.SoftwarePromoArt.AnyAsync(p => p.GroupId == oldGroupId);

        if(!oldGroupHasItems)
        {
            SoftwarePromoArtGroup oldGroup =
                await context.SoftwarePromoArtGroups.FirstOrDefaultAsync(g => g.Id == oldGroupId);

            if(oldGroup is not null)
            {
                context.SoftwarePromoArtGroups.Remove(oldGroup);
                await context.SaveChangesWithUserAsync(userId);
            }
        }

        string photosRoot = Path.Combine(_assetRootPath, "photos", "software-promo-art");
        string guidStr    = id.ToString();

        DeleteFilesByPattern(Path.Combine(photosRoot, "originals"), $"{guidStr}.*");

        string[] formats     = ["jpeg", "webp", "avif", "jxl"];
        string[] resolutions = ["hd", "1440p", "4k"];

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
}
