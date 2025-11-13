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
// Copyright © 2003-2025 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Marechai.Server.Controllers;

[Route("/company-logos")]
[ApiController]
public class CompanyLogosController(MarechaiContext context, IWebHostEnvironment host) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<CompanyLogo>> GetByCompany(int companyId) =>
        await context.CompanyLogos.Where(l => l.CompanyId == companyId).OrderBy(l => l.Year).ToListAsync();

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        CompanyLogo logo = await context.CompanyLogos.Where(l => l.Id == id).FirstOrDefaultAsync();

        if(logo is null) return;

        context.CompanyLogos.Remove(logo);
        await context.SaveChangesWithUserAsync(userId);

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos", logo.Guid + ".svg")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos", logo.Guid + ".svg"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/webp/1x", logo.Guid + ".webp")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/webp/1x", logo.Guid + ".webp"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/webp/2x", logo.Guid + ".webp")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/webp/2x", logo.Guid + ".webp"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/webp/3x", logo.Guid + ".webp")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/webp/3x", logo.Guid + ".webp"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/png/1x", logo.Guid + ".png")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/png/1x", logo.Guid + ".png"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/png/2x", logo.Guid + ".png")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/png/2x", logo.Guid + ".png"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/png/3x", logo.Guid + ".png")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/png/3x", logo.Guid + ".png"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/1x", logo.Guid + ".webp")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/1x", logo.Guid + ".webp"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/2x", logo.Guid + ".webp")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/2x", logo.Guid + ".webp"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/3x", logo.Guid + ".webp")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/webp/3x", logo.Guid + ".webp"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/png/1x", logo.Guid + ".png")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/png/1x", logo.Guid + ".png"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/png/2x", logo.Guid + ".png")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/png/2x", logo.Guid + ".png"));

        if(File.Exists(Path.Combine(_webRootPath, "assets/logos/thumbs/png/3x", logo.Guid + ".png")))
            File.Delete(Path.Combine(_webRootPath, "assets/logos/thumbs/png/3x", logo.Guid + ".png"));
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task ChangeYearAsync(int id, int? year)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        CompanyLogo logo = await context.CompanyLogos.Where(l => l.Id == id).FirstOrDefaultAsync();

        if(logo is null) return;

        logo.Year = year;
        await context.SaveChangesWithUserAsync(userId);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> CreateAsync(int companyId, Guid guid, int? year)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return 0;
        var logo = new CompanyLogo
        {
            Guid      = guid,
            Year      = year,
            CompanyId = companyId
        };

        await context.CompanyLogos.AddAsync(logo);
        await context.SaveChangesWithUserAsync(userId);

        return logo.Id;
    }
}
