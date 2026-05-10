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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/magazines/issues")]
[ApiController]
public class MagazineIssuesController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineIssueDto>> GetAsync() => context.MagazineIssues.OrderBy(b => b.Magazine.Title)
                                                             .ThenBy(b => b.Published)
                                                             .ThenBy(b => b.Caption)
                                                             .Select(b => new MagazineIssueDto
                                                              {
                                                                  Id                 = b.Id,
                                                                  MagazineId         = b.MagazineId,
                                                                  MagazineTitle      = b.Magazine.Title,
                                                                  Caption            = b.Caption,
                                                                  NativeCaption      = b.NativeCaption,
                                                                  Published          = b.Published,
                                                                  PublishedPrecision = b.PublishedPrecision,
                                                                  ProductCode        = b.ProductCode,
                                                                  Pages              = b.Pages,
                                                                  IssueNumber        = b.IssueNumber,
                                                                  InternetArchiveUrl = b.InternetArchiveUrl
                                                              })
                                                             .ToListAsync();

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<MagazineIssueDto> GetAsync(long id) => context.MagazineIssues.Where(b => b.Id == id)
                                                              .Select(b => new MagazineIssueDto
                                                               {
                                                                   Id                 = b.Id,
                                                                   MagazineId         = b.MagazineId,
                                                                   MagazineTitle      = b.Magazine.Title,
                                                                   Caption            = b.Caption,
                                                                   NativeCaption      = b.NativeCaption,
                                                                   Published          = b.Published,
                                                                   PublishedPrecision = b.PublishedPrecision,
                                                                   ProductCode        = b.ProductCode,
                                                                   Pages              = b.Pages,
                                                                   IssueNumber        = b.IssueNumber,
                                                                   InternetArchiveUrl = b.InternetArchiveUrl
                                                               })
                                                              .FirstOrDefaultAsync();

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
}
