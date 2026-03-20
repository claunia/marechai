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
using System.Text;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Svg.Skia;

namespace Marechai.Server.Controllers;

[Route("/companies/logos")]
[ApiController]
public class CompanyLogosController(MarechaiContext context, IConfiguration configuration) : ControllerBase
{
    private readonly string _assetRootPath = configuration["AssetRootPath"]!;

    [HttpGet("/companies/{companyId:int}/logos")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyLogoDto>> GetByCompany(int companyId)
    {
        return context.CompanyLogos
        .Where(l => l.CompanyId == companyId)
        .OrderBy(l => l.Year)
        .Select(l => new CompanyLogoDto
        {
            Id = l.Id,
            CompanyId = l.CompanyId,
            Year = l.Year,
            Guid = l.Guid
        })
        .ToListAsync();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.Sid);

        if (userId is null) return Unauthorized();
        var logo = await context.CompanyLogos.Where(l => l.Id == id).FirstOrDefaultAsync();

        if (logo is null) return NotFound();

        context.CompanyLogos.Remove(logo);
        await context.SaveChangesWithUserAsync(userId);

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos", logo.Guid + ".svg")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos", logo.Guid + ".svg"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/webp/1x", logo.Guid + ".webp")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/webp/1x", logo.Guid + ".webp"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/webp/2x", logo.Guid + ".webp")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/webp/2x", logo.Guid + ".webp"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/webp/3x", logo.Guid + ".webp")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/webp/3x", logo.Guid + ".webp"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/png/1x", logo.Guid + ".png")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/png/1x", logo.Guid + ".png"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/png/2x", logo.Guid + ".png")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/png/2x", logo.Guid + ".png"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/png/3x", logo.Guid + ".png")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/png/3x", logo.Guid + ".png"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/thumbs/webp/1x", logo.Guid + ".webp")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/thumbs/webp/1x", logo.Guid + ".webp"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/thumbs/webp/2x", logo.Guid + ".webp")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/thumbs/webp/2x", logo.Guid + ".webp"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/thumbs/webp/3x", logo.Guid + ".webp")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/thumbs/webp/3x", logo.Guid + ".webp"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/thumbs/png/1x", logo.Guid + ".png")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/thumbs/png/1x", logo.Guid + ".png"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/thumbs/png/2x", logo.Guid + ".png")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/thumbs/png/2x", logo.Guid + ".png"));

        if (System.IO.File.Exists(Path.Combine(_assetRootPath, "logos/thumbs/png/3x", logo.Guid + ".png")))
            System.IO.File.Delete(Path.Combine(_assetRootPath, "logos/thumbs/png/3x", logo.Guid + ".png"));

        return Ok();
    }

    [HttpPut("change-year/{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangeYearAsync(int id, [FromBody] int? year)
    {
        var userId = User.FindFirstValue(ClaimTypes.Sid);

        if (userId is null) return Unauthorized();

        var logo = await context.CompanyLogos.Where(l => l.Id == id).FirstOrDefaultAsync();

        if (logo is null) return NotFound();

        logo.Year = year;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> CreateAsync([FromBody] CompanyLogoDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.Sid);

        if (userId is null) return Unauthorized();

        var logo = new CompanyLogo
        {
            Guid = dto.Guid,
            Year = dto.Year,
            CompanyId = dto.CompanyId
        };

        await context.CompanyLogos.AddAsync(logo);
        await context.SaveChangesWithUserAsync(userId);

        return logo.Id;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CompanyLogoDto>> UploadAsync(IFormFile file, [FromForm] int companyId,
                                                                [FromForm] int? year)
    {
        var userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(file is null || file.Length == 0)
            return BadRequest("No file provided.");

        if(file.Length > 5 * 1024 * 1024)
            return BadRequest("File exceeds 5 MB limit.");

        // Read the file into memory for validation
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        // Validate SVG header
        var headerBuffer = new byte[6];
        int headerRead = await ms.ReadAsync(headerBuffer);

        if(headerRead < 5)
            return BadRequest("File is too small to be a valid SVG.");

        string header = Encoding.UTF8.GetString(headerBuffer, 0, headerRead);

        if(!header.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) &&
           !header.StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
            return BadRequest("File does not appear to be a valid SVG.");

        // Validate SVG footer
        ms.Seek(-7, SeekOrigin.End);
        var footerBuffer = new byte[7];
        int footerRead = await ms.ReadAsync(footerBuffer);
        string footer = Encoding.UTF8.GetString(footerBuffer, 0, footerRead).TrimEnd();

        if(!footer.EndsWith("</svg>", StringComparison.OrdinalIgnoreCase))
            return BadRequest("File does not appear to be a valid SVG.");

        // Validate by loading with SkiaSharp
        ms.Position = 0;

        try
        {
            using var svg = new SKSvg();
            svg.Load(ms);

            if(svg.Picture is null)
                return BadRequest("SVG could not be parsed.");
        }
        catch(Exception)
        {
            return BadRequest("SVG could not be parsed.");
        }

        // Generate GUID and render all variants
        var guid = Guid.NewGuid();
        ms.Position = 0;

        try
        {
            // RenderCompanyLogo expects a root path and prepends "assets/logos/" internally,
            // so pass the parent of assetRootPath (which IS the assets directory)
            SvgRender.RenderCompanyLogo(guid, ms, Path.GetDirectoryName(_assetRootPath)!);
        }
        catch(Exception)
        {
            return BadRequest("SVG rendering failed.");
        }

        // Save original SVG to disk
        string svgDir = Path.Combine(_assetRootPath, "logos");

        if(!Directory.Exists(svgDir))
            Directory.CreateDirectory(svgDir);

        string svgPath = Path.Combine(svgDir, $"{guid}.svg");

        ms.Position = 0;
        await using var svgFs = new FileStream(svgPath, FileMode.CreateNew, FileAccess.Write);
        await ms.CopyToAsync(svgFs);

        // Create database record
        var logo = new CompanyLogo
        {
            Guid      = guid,
            Year      = year,
            CompanyId = companyId
        };

        await context.CompanyLogos.AddAsync(logo);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new CompanyLogoDto
        {
            Id        = logo.Id,
            CompanyId = logo.CompanyId,
            Year      = logo.Year,
            Guid      = logo.Guid
        });
    }
}