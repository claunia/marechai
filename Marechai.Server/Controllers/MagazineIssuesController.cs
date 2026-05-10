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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marechai.Server.Controllers;

[Route("/magazines/issues")]
[ApiController]
public class MagazineIssuesController(
    MarechaiContext                    context,
    IConfiguration                     configuration,
    IDbContextFactory<MarechaiContext> dbFactory) : ControllerBase
{
    static readonly HashSet<string> _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"
    ];

    readonly string _assetRootPath = configuration["AssetRootPath"]!;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineIssueDto>> GetAsync() => context.MagazineIssues.OrderBy(b => b.Magazine.Title)
                                                             .ThenBy(b => b.Published)
                                                             .ThenBy(b => b.Caption)
                                                             .Select(b => new MagazineIssueDto
                                                              {
                                                                  Id                     = b.Id,
                                                                  MagazineId             = b.MagazineId,
                                                                  MagazineTitle          = b.Magazine.Title,
                                                                  Caption                = b.Caption,
                                                                  NativeCaption          = b.NativeCaption,
                                                                  Published              = b.Published,
                                                                  PublishedPrecision     = b.PublishedPrecision,
                                                                  ProductCode            = b.ProductCode,
                                                                  Pages                  = b.Pages,
                                                                  IssueNumber            = b.IssueNumber,
                                                                  InternetArchiveUrl     = b.InternetArchiveUrl,
                                                                  CoverGuid              = b.CoverGuid,
                                                                  OriginalCoverExtension = b.OriginalCoverExtension
                                                              })
                                                             .ToListAsync();

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<MagazineIssueDto> GetAsync(long id) => context.MagazineIssues.Where(b => b.Id == id)
                                                              .Select(b => new MagazineIssueDto
                                                               {
                                                                   Id                     = b.Id,
                                                                   MagazineId             = b.MagazineId,
                                                                   MagazineTitle          = b.Magazine.Title,
                                                                   Caption                = b.Caption,
                                                                   NativeCaption          = b.NativeCaption,
                                                                   Published              = b.Published,
                                                                   PublishedPrecision     = b.PublishedPrecision,
                                                                   ProductCode            = b.ProductCode,
                                                                   Pages                  = b.Pages,
                                                                   IssueNumber            = b.IssueNumber,
                                                                   InternetArchiveUrl     = b.InternetArchiveUrl,
                                                                   CoverGuid              = b.CoverGuid,
                                                                   OriginalCoverExtension = b.OriginalCoverExtension
                                                               })
                                                              .FirstOrDefaultAsync();

    /// <summary>
    /// Consolidated payload for the public /magazine/issue/{Id} view page. Returns the issue
    /// head plus the parent magazine title and the three issue-level junction collections
    /// (machines, machine families, software) in a single response. Each query runs on an
    /// independent <see cref="MarechaiContext"/> from the factory because <c>DbContext</c>
    /// is not thread-safe; sharing the request-scoped context across parallel branches
    /// throws <see cref="InvalidOperationException"/>.
    /// </summary>
    [HttpGet("{id:long}/full")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MagazineIssueFullDto>> GetFullAsync(long id)
    {
        await using var headCtx     = await dbFactory.CreateDbContextAsync();
        await using var machinesCtx = await dbFactory.CreateDbContextAsync();
        await using var familiesCtx = await dbFactory.CreateDbContextAsync();
        await using var softwareCtx = await dbFactory.CreateDbContextAsync();

        var headTask = headCtx.MagazineIssues.AsNoTracking()
                              .Where(b => b.Id == id)
                              .Select(b => new
                               {
                                   b.Id,
                                   b.MagazineId,
                                   MagazineTitle          = b.Magazine.Title,
                                   b.Caption,
                                   b.NativeCaption,
                                   b.Published,
                                   b.PublishedPrecision,
                                   b.ProductCode,
                                   b.Pages,
                                   b.IssueNumber,
                                   b.InternetArchiveUrl,
                                   b.CoverGuid,
                                   b.OriginalCoverExtension
                               })
                              .FirstOrDefaultAsync();

        // Mirrors MagazinesByMachineController.GetByMagazine.
        Task<List<MagazineByMachineDto>> machinesTask = machinesCtx.MagazinesByMachines.AsNoTracking()
            .Where(p => p.MagazineId == id)
            .Select(p => new MagazineByMachineDto
             {
                 Id         = p.Id,
                 MagazineId = p.MagazineId,
                 MachineId  = p.MachineId,
                 Machine    = p.Machine.Name
             })
            .OrderBy(p => p.Machine)
            .ToListAsync();

        // Mirrors MagazinesByMachineFamilyController.GetByMagazine.
        Task<List<MagazineByMachineFamilyDto>> familiesTask = familiesCtx.MagazinesByMachinesFamilies.AsNoTracking()
            .Where(p => p.MagazineId == id)
            .Select(p => new MagazineByMachineFamilyDto
             {
                 Id              = p.Id,
                 MagazineId      = p.MagazineId,
                 MachineFamilyId = p.MachineFamilyId,
                 MachineFamily   = p.MachineFamily.Name
             })
            .OrderBy(p => p.MachineFamily)
            .ToListAsync();

        // Mirrors MagazinesBySoftwareController.GetByMagazine.
        Task<List<MagazineBySoftwareDto>> softwareTask = softwareCtx.MagazinesBySoftware.AsNoTracking()
            .Where(p => p.MagazineId == id)
            .Select(p => new MagazineBySoftwareDto
             {
                 Id         = p.Id,
                 MagazineId = p.MagazineId,
                 SoftwareId = p.SoftwareId,
                 Software   = p.Software.Name
             })
            .OrderBy(p => p.Software)
            .ToListAsync();

        await Task.WhenAll(headTask, machinesTask, familiesTask, softwareTask);

        var head = headTask.Result;

        if(head is null) return NotFound();

        var issue = new MagazineIssueDto
        {
            Id                     = head.Id,
            MagazineId             = head.MagazineId,
            MagazineTitle          = head.MagazineTitle,
            Caption                = head.Caption,
            NativeCaption          = head.NativeCaption,
            Published              = head.Published,
            PublishedPrecision     = head.PublishedPrecision,
            ProductCode            = head.ProductCode,
            Pages                  = head.Pages,
            IssueNumber            = head.IssueNumber,
            InternetArchiveUrl     = head.InternetArchiveUrl,
            CoverGuid              = head.CoverGuid,
            OriginalCoverExtension = head.OriginalCoverExtension
        };

        return new MagazineIssueFullDto
        {
            Issue           = issue,
            MagazineTitle   = head.MagazineTitle,
            Machines        = machinesTask.Result,
            MachineFamilies = familiesTask.Result,
            Software        = softwareTask.Result
        };
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] MagazineIssueDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        MagazineIssue model = await context.MagazineIssues.FindAsync(id);

        if(model is null) return NotFound();

        model.MagazineId         = dto.MagazineId;
        model.Caption            = dto.Caption;
        model.NativeCaption      = dto.NativeCaption;
        model.Published          = dto.Published;
        model.PublishedPrecision = dto.PublishedPrecision;
        model.ProductCode        = dto.ProductCode;
        model.Pages              = dto.Pages;
        model.IssueNumber        = dto.IssueNumber;
        model.InternetArchiveUrl = dto.InternetArchiveUrl;

        string newsName = await BuildMagazineIssueNewsNameAsync(model);

        await context.News.AddAsync(new News
        {
            AddedId = model.MagazineId,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedMagazineIssueInDb,
            Name    = newsName
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] MagazineIssueDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new MagazineIssue
        {
            MagazineId         = dto.MagazineId,
            Caption            = dto.Caption,
            NativeCaption      = dto.NativeCaption,
            Published          = dto.Published,
            PublishedPrecision = dto.PublishedPrecision,
            ProductCode        = dto.ProductCode,
            Pages              = dto.Pages,
            IssueNumber        = dto.IssueNumber,
            InternetArchiveUrl = dto.InternetArchiveUrl
        };

        await context.MagazineIssues.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        string newsName = await BuildMagazineIssueNewsNameAsync(model);

        await context.News.AddAsync(new News
        {
            AddedId = model.MagazineId,
            Date    = DateTime.UtcNow,
            Type    = NewsType.NewMagazineIssueInDb,
            Name    = newsName
        });

        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        MagazineIssue item = await context.MagazineIssues.FindAsync(id);

        if(item is null) return NotFound();

        context.MagazineIssues.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    async Task<string> BuildMagazineIssueNewsNameAsync(MagazineIssue model)
    {
        string magazineTitle = await context.Magazines.Where(m => m.Id == model.MagazineId)
                                            .Select(m => m.Title)
                                            .FirstOrDefaultAsync() ?? string.Empty;

        if(model.IssueNumber.HasValue) return $"{magazineTitle} #{model.IssueNumber.Value}";

        if(model.Published.HasValue)
        {
            string dateStr = model.PublishedPrecision switch
            {
                DatePrecision.YearOnly  => model.Published.Value.ToString("yyyy"),
                DatePrecision.MonthYear => model.Published.Value.ToString("MMMM yyyy"),
                _                       => model.Published.Value.ToString("d")
            };

            return $"{magazineTitle} ({dateStr})";
        }

        return string.IsNullOrWhiteSpace(model.Caption) ? magazineTitle : $"{magazineTitle}: {model.Caption}";
    }

    [HttpPost("{id:long}/cover/upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MagazineIssueDto>> UploadCoverAsync(long id, IFormFile file)
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

        MagazineIssue issue = await context.MagazineIssues.FindAsync(id);

        if(issue is null) return NotFound();

        // If cover already exists, delete old files
        if(issue.CoverGuid.HasValue)
            DeleteCoverFiles(issue.CoverGuid.Value);

        Guid coverGuid = Guid.NewGuid();

        // Save original file to disk
        Photos.EnsureCreated(_assetRootPath, false, "magazine-issue-covers");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "magazine-issue-covers", "originals");
        string originalPath = Path.Combine(originalsDir, $"{coverGuid}{extension}");

        await using(var fs = new FileStream(originalPath, FileMode.CreateNew, FileAccess.Write))
        {
            await file.CopyToAsync(fs);
        }

        // Fire conversion worker (generates all format/resolution variants)
        string sourceFormat = extension.TrimStart('.');

        _ = Task.Run(() =>
        {
            var photos = new Photos();
            photos.ConversionWorker(_assetRootPath, coverGuid, originalPath, sourceFormat, false,
                                    "magazine-issue-covers");
        });

        // Update issue record
        issue.CoverGuid              = coverGuid;
        issue.OriginalCoverExtension = extension.TrimStart('.');
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new MagazineIssueDto
        {
            Id                     = issue.Id,
            MagazineId             = issue.MagazineId,
            Caption                = issue.Caption,
            NativeCaption          = issue.NativeCaption,
            Published              = issue.Published,
            PublishedPrecision     = issue.PublishedPrecision,
            ProductCode            = issue.ProductCode,
            Pages                  = issue.Pages,
            IssueNumber            = issue.IssueNumber,
            InternetArchiveUrl     = issue.InternetArchiveUrl,
            CoverGuid              = issue.CoverGuid,
            OriginalCoverExtension = issue.OriginalCoverExtension
        });
    }

    [HttpDelete("{id:long}/cover")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteCoverAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        MagazineIssue issue = await context.MagazineIssues.FindAsync(id);

        if(issue is null) return NotFound();

        if(!issue.CoverGuid.HasValue) return NoContent();

        DeleteCoverFiles(issue.CoverGuid.Value);

        issue.CoverGuid              = null;
        issue.OriginalCoverExtension = null;
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    void DeleteCoverFiles(Guid coverGuid)
    {
        string photosRoot = Path.Combine(_assetRootPath, "photos", "magazine-issue-covers");
        string guidStr    = coverGuid.ToString();

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
    }

    static void DeleteFilesByPattern(string directory, string pattern)
    {
        if(!System.IO.Directory.Exists(directory)) return;

        foreach(string file in System.IO.Directory.GetFiles(directory, pattern))
            System.IO.File.Delete(file);
    }
}
