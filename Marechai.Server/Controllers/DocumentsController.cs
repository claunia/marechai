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

[Route("/documents")]
[ApiController]
public class DocumentsController(MarechaiContext context) : ControllerBase
{
    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetDocumentsCountAsync() => context.Documents.CountAsync();

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetMinimumYearAsync() => context.Documents
                                                     .Where(d => d.Published.HasValue &&
                                                                 d.Published.Value.Year > 1000)
                                                     .MinAsync(d => d.Published.Value.Year);

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetMaximumYearAsync() => context.Documents
                                                     .Where(d => d.Published.HasValue &&
                                                                 d.Published.Value.Year > 1000)
                                                     .MaxAsync(d => d.Published.Value.Year);

    [HttpGet("by-letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DocumentDto>> GetDocumentsByLetterAsync(char c) => context.Documents
       .Where(d =>
            (d.SortTitle != null && EF.Functions.Like(d.SortTitle, $"{c}%")) ||
            (d.SortTitle == null && EF.Functions.Like(d.Title, $"{c}%")))
       .OrderBy(d => d.SortTitle)
       .ThenBy(d => d.Title)
       .ThenBy(d => d.Published)
       .Select(d => new DocumentDto
        {
            Id          = d.Id,
            Title       = d.Title,
            NativeTitle = d.NativeTitle,
            SortTitle   = d.SortTitle,
            Published   = d.Published,
            PublishedPrecision = d.PublishedPrecision,
            CountryId   = d.CountryId,
            Country     = d.Country.Name
        })
       .ToListAsync();

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DocumentDto>> GetDocumentsByYearAsync(int year) => context.Documents
       .Where(d => d.Published != null && d.Published.Value.Year == year)
       .OrderBy(d => d.SortTitle)
       .ThenBy(d => d.Title)
       .ThenBy(d => d.Published)
       .Select(d => new DocumentDto
        {
            Id          = d.Id,
            Title       = d.Title,
            NativeTitle = d.NativeTitle,
            SortTitle   = d.SortTitle,
            Published   = d.Published,
            PublishedPrecision = d.PublishedPrecision,
            CountryId   = d.CountryId,
            Country     = d.Country.Name
        })
       .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DocumentDto>> GetAsync() => context.Documents.OrderBy(b => b.SortTitle)
                                                        .ThenBy(b => b.Published)
                                                        .ThenBy(b => b.Title)
                                                        .Select(b => new DocumentDto
                                                         {
                                                             Id          = b.Id,
                                                             Title       = b.Title,
                                                             NativeTitle = b.NativeTitle,
                                                             SortTitle   = b.SortTitle,
                                                             Published   = b.Published,
                                                             PublishedPrecision = b.PublishedPrecision,
                                                             CountryId   = b.CountryId,
                                                             Country     = b.Country.Name
                                                         })
                                                        .ToListAsync();

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<DocumentDto> GetAsync(long id) => context.Documents.Where(b => b.Id == id)
                                                         .Select(b => new DocumentDto
                                                          {
                                                              Id          = b.Id,
                                                              Title       = b.Title,
                                                              NativeTitle = b.NativeTitle,
                                                              SortTitle   = b.SortTitle,
                                                              Published   = b.Published,
                                                              PublishedPrecision = b.PublishedPrecision,
                                                              CountryId   = b.CountryId,
                                                              Country     = b.Country.Name
                                                          })
                                                         .FirstOrDefaultAsync();

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] DocumentDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Document model = await context.Documents.FindAsync(id);

        if(model is null) return NotFound();

        model.Title       = dto.Title;
        model.NativeTitle = dto.NativeTitle;
        model.SortTitle   = dto.SortTitle;
        model.Published   = dto.Published;
        model.PublishedPrecision = dto.PublishedPrecision;
        model.CountryId   = dto.CountryId;

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedDocumentInDb,
            Name    = dto.Title
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] DocumentDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Document
        {
            Title       = dto.Title,
            NativeTitle = dto.NativeTitle,
            SortTitle   = dto.SortTitle,
            Published   = dto.Published,
            PublishedPrecision = dto.PublishedPrecision,
            CountryId   = dto.CountryId
        };

        await context.Documents.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.NewDocumentInDb,
            Name    = dto.Title
        });

        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpGet("{id:long}/synopses")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DocumentSynopsisDto>> GetSynopsesAsync(long id) => context.DocumentSynopses
       .Where(s => s.DocumentId == id)
       .Select(s => new DocumentSynopsisDto
        {
            Id           = s.Id,
            Text         = s.Text,
            LanguageCode = s.LanguageCode,
            Language     = s.Language.ReferenceName
        })
       .ToListAsync();

    [HttpGet("{id:long}/synopsis")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<DocumentSynopsisDto> GetSynopsisAsync(long id, [FromQuery] string lang = "eng")
    {
        DocumentSynopsisDto synopsis = await context.DocumentSynopses
                                                    .Where(s => s.DocumentId == id && s.LanguageCode == lang)
                                                    .Select(s => new DocumentSynopsisDto
                                                     {
                                                         Id           = s.Id,
                                                         Text         = s.Text,
                                                         LanguageCode = s.LanguageCode,
                                                         Language     = s.Language.ReferenceName
                                                     })
                                                    .FirstOrDefaultAsync();

        if(synopsis is null && lang != "eng")
            synopsis = await context.DocumentSynopses
                                    .Where(s => s.DocumentId == id && s.LanguageCode == "eng")
                                    .Select(s => new DocumentSynopsisDto
                                     {
                                         Id           = s.Id,
                                         Text         = s.Text,
                                         LanguageCode = s.LanguageCode,
                                         Language     = s.Language.ReferenceName
                                     })
                                    .FirstOrDefaultAsync();

        return synopsis;
    }

    [HttpPost("{id:long}/synopsis")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateOrUpdateSynopsisAsync(
        long id, [FromBody] DocumentSynopsisDto synopsis)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        DocumentSynopsis current = await context.DocumentSynopses
                                                .FirstOrDefaultAsync(s => s.DocumentId   == id &&
                                                                          s.LanguageCode == synopsis.LanguageCode);

        if(current is null)
        {
            current = new DocumentSynopsis
            {
                DocumentId   = id,
                LanguageCode = synopsis.LanguageCode,
                Text         = synopsis.Text
            };

            await context.DocumentSynopses.AddAsync(current);
        }
        else
        {
            current.Text = synopsis.Text;
        }

        await context.SaveChangesWithUserAsync(userId);

        return current.Id;
    }

    [HttpDelete("{id:long}/synopsis/{languageCode}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteSynopsisAsync(long id, string languageCode)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        DocumentSynopsis synopsis = await context.DocumentSynopses
                                                 .FirstOrDefaultAsync(s => s.DocumentId   == id &&
                                                                           s.LanguageCode == languageCode);

        if(synopsis is null) return NotFound();

        context.DocumentSynopses.Remove(synopsis);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
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
        Document item = await context.Documents.FindAsync(id);

        if(item is null) return NotFound();

        context.Documents.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}