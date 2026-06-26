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
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Marechai.Server.Controllers;

[Route("/software/platforms")]
[ApiController]
public class SoftwarePlatformsController(MarechaiContext context, IMemoryCache cache,
                                         IConfiguration  configuration) : ControllerBase
{
    // Software platforms barely change — cache the full list briefly so the
    // /software landing page doesn't re-hit the DB on every load.
    const           string   PLATFORMS_CACHE_KEY              = "software:platforms:list";
    const           string   PLATFORMS_WITH_SOFTWARE_CACHE_KEY = "software:platforms:with-software";
    static readonly TimeSpan _platformsCacheTtl               = TimeSpan.FromMinutes(5);

    static readonly HashSet<string> _allowedLogoExtensions = [".jpg", ".jpeg", ".png", ".webp", ".bmp"];

    static readonly HashSet<string> _allowedLogoContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/bmp"
    ];

    readonly string _assetRootPath = configuration["AssetRootPath"]!;

    [HttpGet]
    [AllowAnonymous]
    [OutputCache(Duration = 300, VaryByQueryKeys = ["includeUnused"])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwarePlatformDto>> GetAsync([FromQuery] bool includeUnused = true)
    {
        string cacheKey = includeUnused ? PLATFORMS_CACHE_KEY : PLATFORMS_WITH_SOFTWARE_CACHE_KEY;

        if(cache.TryGetValue(cacheKey, out List<SoftwarePlatformDto> cached) && cached is not null)
            return cached;

        IQueryable<SoftwarePlatform> query = context.SoftwarePlatforms;
        if(!includeUnused)
            query = query.Where(p => p.SoftwareReleases.Any());

        List<SoftwarePlatformDto> platforms = await query.OrderBy(p => p.Name)
                                                         .Select(p => new SoftwarePlatformDto
                                                          {
                                                              Id            = p.Id,
                                                              Name          = p.Name,
                                                              LogoId        = p.LogoId,
                                                              LogoExtension = p.LogoExtension
                                                          })
                                                         .ToListAsync();

        cache.Set(cacheKey, platforms, _platformsCacheTtl);

        return platforms;
    }

    [HttpGet("{id:ulong}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoftwarePlatformDto> GetAsync(ulong id) => context.SoftwarePlatforms.Where(p => p.Id == id)
                                                                  .Select(p => new SoftwarePlatformDto
                                                                   {
                                                                       Id            = p.Id,
                                                                       Name          = p.Name,
                                                                       LogoId        = p.LogoId,
                                                                       LogoExtension = p.LogoExtension
                                                                   })
                                                                  .FirstOrDefaultAsync();

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] SoftwarePlatformDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwarePlatform model = await context.SoftwarePlatforms.FindAsync(id);

        if(model is null) return NotFound();

        model.Name = dto.Name;
        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(PLATFORMS_CACHE_KEY);
        cache.Remove(PLATFORMS_WITH_SOFTWARE_CACHE_KEY);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] SoftwarePlatformDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoftwarePlatform
        {
            Name = dto.Name
        };

        await context.SoftwarePlatforms.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(PLATFORMS_CACHE_KEY);
        cache.Remove(PLATFORMS_WITH_SOFTWARE_CACHE_KEY);

        return model.Id;
    }

    [HttpDelete("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoftwarePlatform item = await context.SoftwarePlatforms.FindAsync(id);

        if(item is null) return NotFound();

        context.SoftwarePlatforms.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(PLATFORMS_CACHE_KEY);
        cache.Remove(PLATFORMS_WITH_SOFTWARE_CACHE_KEY);

        return Ok();
    }

    [HttpPost("{id:ulong}/logo")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SoftwarePlatformDto>> UploadLogoAsync(ulong id, IFormFile file)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwarePlatform platform = await context.SoftwarePlatforms.FindAsync(id);

        if(platform is null) return NotFound();

        if(file is null || file.Length == 0)
            return Problem(detail: "No file provided.", statusCode: StatusCodes.Status400BadRequest);

        if(file.Length > 5 * 1024 * 1024)
            return Problem(detail: "File exceeds 5 MB limit.", statusCode: StatusCodes.Status400BadRequest);

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        if(!_allowedLogoExtensions.Contains(extension))
            return Problem(detail: "Unsupported file format. Accepted: JPEG, PNG, WebP, BMP.",
                           statusCode: StatusCodes.Status400BadRequest);

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_allowedLogoContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return Problem(detail: "Unsupported content type.", statusCode: StatusCodes.Status400BadRequest);

        // Remove any previous logo's files before writing the new one.
        if(platform.LogoId.HasValue)
            DeletePlatformLogoFiles(platform.LogoId.Value, platform.LogoExtension);

        var guid = Guid.NewGuid();

        Photos.EnsureCreated(_assetRootPath, false, "platform-logos");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "platform-logos", "originals");
        string originalPath = Path.Combine(originalsDir, $"{guid}{extension}");

        using(var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            ms.Position = 0;

            await using var fs = new FileStream(originalPath, FileMode.CreateNew, FileAccess.Write);
            await ms.CopyToAsync(fs);
        }

        // A platform logo is a small icon, not a photo — render compact full/thumb tiers
        // (256 / 64 px) instead of reusing Photos.ConversionWorker's 4K/512 photo sizing.
        foreach((string format, string outExt) in new[]
                {
                    ("jpeg", "jpg"), ("webp", "webp"), ("avif", "avif")
                })
        {
            string fullPath  = Path.Combine(_assetRootPath, "photos", "platform-logos", format, "4k",
                                            $"{guid}.{outExt}");
            string thumbPath = Path.Combine(_assetRootPath, "photos", "platform-logos", "thumbs", format, "4k",
                                            $"{guid}.{outExt}");

            Photos.ConvertUsingImageMagick(originalPath, fullPath,  256, 256);
            Photos.ConvertUsingImageMagick(originalPath, thumbPath, 64,  64);
        }

        platform.LogoId        = guid;
        platform.LogoExtension = extension.TrimStart('.');
        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(PLATFORMS_CACHE_KEY);
        cache.Remove(PLATFORMS_WITH_SOFTWARE_CACHE_KEY);

        return Ok(new SoftwarePlatformDto
        {
            Id            = platform.Id,
            Name          = platform.Name,
            LogoId        = platform.LogoId,
            LogoExtension = platform.LogoExtension
        });
    }

    [HttpDelete("{id:ulong}/logo")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteLogoAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwarePlatform platform = await context.SoftwarePlatforms.FindAsync(id);

        if(platform is null) return NotFound();

        if(!platform.LogoId.HasValue) return Ok();

        DeletePlatformLogoFiles(platform.LogoId.Value, platform.LogoExtension);

        platform.LogoId        = null;
        platform.LogoExtension = null;
        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(PLATFORMS_CACHE_KEY);
        cache.Remove(PLATFORMS_WITH_SOFTWARE_CACHE_KEY);

        return Ok();
    }

    void DeletePlatformLogoFiles(Guid guid, string originalExtension)
    {
        if(!string.IsNullOrEmpty(originalExtension))
        {
            string originalPath = Path.Combine(_assetRootPath, "photos", "platform-logos", "originals",
                                               $"{guid}.{originalExtension}");

            if(System.IO.File.Exists(originalPath)) System.IO.File.Delete(originalPath);
        }

        foreach((string format, string outExt) in new[]
                {
                    ("jpeg", "jpg"), ("webp", "webp"), ("avif", "avif")
                })
        {
            string fullPath  = Path.Combine(_assetRootPath, "photos", "platform-logos", format, "4k",
                                            $"{guid}.{outExt}");
            string thumbPath = Path.Combine(_assetRootPath, "photos", "platform-logos", "thumbs", format, "4k",
                                            $"{guid}.{outExt}");

            if(System.IO.File.Exists(fullPath))  System.IO.File.Delete(fullPath);
            if(System.IO.File.Exists(thumbPath)) System.IO.File.Delete(thumbPath);
        }
    }

    /// <summary>
    /// Merge multiple software platforms into a target platform.
    /// </summary>
    /// <param name="id">The target platform ID to merge into.</param>
    /// <param name="request">The merge request containing source platform IDs.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("{id:ulong}/merge")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> MergeAsync(ulong id, [FromBody] MergePlatformsRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(request?.SourceIds == null || request.SourceIds.Count == 0)
            return BadRequest("At least one source platform must be specified");

        SoftwarePlatform targetPlatform = await context.SoftwarePlatforms.FindAsync(id);

        if(targetPlatform is null) return NotFound("Target platform not found");

        if(request.SourceIds.Contains(id))
            return BadRequest("Target platform cannot be in the source list");

        var sourcePlatformsToDelete = await context.SoftwarePlatforms
                                                   .Where(p => request.SourceIds.Contains(p.Id))
                                                   .ToListAsync();

        if(sourcePlatformsToDelete.Count != request.SourceIds.Count)
            return NotFound("One or more source platforms not found");

        await context.SoftwareReleases
                    .Where(sr => sr.PlatformId.HasValue && request.SourceIds.Contains(sr.PlatformId.Value))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(sr => sr.PlatformId, (ulong?)id));

        await context.SoftwareScreenshots
                    .Where(ss => ss.SoftwarePlatformId.HasValue && request.SourceIds.Contains(ss.SoftwarePlatformId.Value))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(ss => ss.SoftwarePlatformId, (ulong?)id));

        await context.SoftwarePlatformsByMachine
                    .Where(spm => request.SourceIds.Contains(spm.SoftwarePlatformId))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(spm => spm.SoftwarePlatformId, id));

        await context.OldDosOsPlatformMaps
                    .Where(dopm => dopm.SoftwarePlatformId.HasValue &&
                                   request.SourceIds.Contains(dopm.SoftwarePlatformId.Value))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(dopm => dopm.SoftwarePlatformId, (ulong?)id));

        foreach(var platform in sourcePlatformsToDelete)
            context.SoftwarePlatforms.Remove(platform);

        await context.SaveChangesWithUserAsync(userId);

        cache.Remove(PLATFORMS_CACHE_KEY);
        cache.Remove(PLATFORMS_WITH_SOFTWARE_CACHE_KEY);

        return Ok();
    }
}
