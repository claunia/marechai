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
using System.Threading;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/magazines")]
[ApiController]
public class MagazinesController(MarechaiContext context) : ControllerBase
{
    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetMagazinesCountAsync() => context.Magazines.CountAsync();

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMinimumYearAsync() => await context.Magazines
                                                                 .Where(m => m.FirstPublication.HasValue &&
                                                                             m.FirstPublication.Value.Year > 1000)
                                                                 .MinAsync(m => (int?)m.FirstPublication.Value.Year) ??
                                                    0;

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMaximumYearAsync() => await context.Magazines
                                                                 .Where(m => m.FirstPublication.HasValue &&
                                                                             m.FirstPublication.Value.Year > 1000)
                                                                 .MaxAsync(m => (int?)m.FirstPublication.Value.Year) ??
                                                    0;

    [HttpGet("companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetCompaniesAsync() => context.CompaniesByMagazines
                                                                .Select(cm => cm.Company)
                                                                .Distinct()
                                                                .Include(c => c.Logos)
                                                                .OrderBy(c => MarechaiContext.NaturalSortKey(c.Name))
                                                                .Select(c => new CompanyDto
                                                                 {
                                                                     Id = c.Id,
                                                                     LastLogo =
                                                                         c.Logos.OrderByDescending(l => l.Year)
                                                                          .FirstOrDefault()
                                                                          .Guid,
                                                                     Name = c.Name
                                                                 })
                                                                .ToListAsync();

    [HttpGet("companies/letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetCompaniesByLetterAsync(char c) => context.CompaniesByMagazines
       .Select(cm => cm.Company)
       .Distinct()
       .Include(c => c.Logos)
       .Where(co => EF.Functions.Like(co.Name, $"{c}%"))
       .OrderBy(co => MarechaiContext.NaturalSortKey(co.Name))
       .Select(co => new CompanyDto
        {
            Id       = co.Id,
            LastLogo = co.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
            Name     = co.Name
        })
       .ToListAsync();

    [HttpGet("by-letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineDto>> GetMagazinesByLetterAsync(char c, [FromQuery] int? skip = null,
                                                             [FromQuery] int? take = null,
                                                             CancellationToken cancellationToken = default)
    {
        IQueryable<Magazine> ordered = context.Magazines
                                              .Where(m =>
                                                  (m.SortTitle != null && EF.Functions.Like(m.SortTitle, $"{c}%")) ||
                                                  (m.SortTitle == null && EF.Functions.Like(m.Title, $"{c}%")))
                                              .OrderBy(m => MarechaiContext.NaturalSortKey(m.SortTitle))
                                              .ThenBy(m => MarechaiContext.NaturalSortKey(m.Title))
                                              .ThenBy(m => m.FirstPublication);

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(m => new MagazineDto
                       {
                           Id                        = m.Id,
                           Title                     = m.Title,
                           NativeTitle               = m.NativeTitle,
                           SortTitle                 = m.SortTitle,
                           Published                 = m.Published,
                           PublishedPrecision        = m.PublishedPrecision,
                           FirstPublication          = m.FirstPublication,
                           FirstPublicationPrecision = m.FirstPublicationPrecision,
                           Issn                      = m.Issn,
                           CountryId                 = m.CountryId,
                           Country                   = m.Country.Name
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("by-letter/{c}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetMagazinesByLetterCountAsync(char c, CancellationToken cancellationToken = default) =>
        context.Magazines
               .Where(m =>
                   (m.SortTitle != null && EF.Functions.Like(m.SortTitle, $"{c}%")) ||
                   (m.SortTitle == null && EF.Functions.Like(m.Title, $"{c}%")))
               .CountAsync(cancellationToken);

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineDto>> GetMagazinesByYearAsync(int year, [FromQuery] int? skip = null,
                                                           [FromQuery] int? take = null,
                                                           CancellationToken cancellationToken = default)
    {
        IQueryable<Magazine> ordered = context.Magazines
                                              .Where(m => m.FirstPublication != null &&
                                                          m.FirstPublication.Value.Year == year)
                                              .OrderBy(m => MarechaiContext.NaturalSortKey(m.SortTitle))
                                              .ThenBy(m => MarechaiContext.NaturalSortKey(m.Title))
                                              .ThenBy(m => m.FirstPublication);

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(m => new MagazineDto
                       {
                           Id                        = m.Id,
                           Title                     = m.Title,
                           NativeTitle               = m.NativeTitle,
                           SortTitle                 = m.SortTitle,
                           Published                 = m.Published,
                           PublishedPrecision        = m.PublishedPrecision,
                           FirstPublication          = m.FirstPublication,
                           FirstPublicationPrecision = m.FirstPublicationPrecision,
                           Issn                      = m.Issn,
                           CountryId                 = m.CountryId,
                           Country                   = m.Country.Name
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("by-year/{year:int}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetMagazinesByYearCountAsync(int year, CancellationToken cancellationToken = default) =>
        context.Magazines
               .Where(m => m.FirstPublication != null && m.FirstPublication.Value.Year == year)
               .CountAsync(cancellationToken);

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineDto>> GetAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                            CancellationToken cancellationToken = default)
    {
        IQueryable<Magazine> ordered = context.Magazines
                                              .OrderBy(b => MarechaiContext.NaturalSortKey(b.SortTitle))
                                              .ThenBy(b => b.FirstPublication)
                                              .ThenBy(b => MarechaiContext.NaturalSortKey(b.Title));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(b => new MagazineDto
                       {
                           Id                        = b.Id,
                           Title                     = b.Title,
                           NativeTitle               = b.NativeTitle,
                           SortTitle                 = b.SortTitle,
                           Published                 = b.Published,
                           PublishedPrecision        = b.PublishedPrecision,
                           FirstPublication          = b.FirstPublication,
                           FirstPublicationPrecision = b.FirstPublicationPrecision,
                           Issn                      = b.Issn,
                           CountryId                 = b.CountryId,
                           Country                   = b.Country.Name
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("titles")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineDto>> GetTitlesAsync() => context.Magazines.OrderBy(b => MarechaiContext.NaturalSortKey(b.Title))
                                                              .ThenBy(b => b.FirstPublication)
                                                              .Select(b => new MagazineDto
                                                               {
                                                                   Id    = b.Id,
                                                                   Title = $"{b.Title} ({b.Country.Name}"
                                                               })
                                                              .ToListAsync();

    [HttpGet("{magazineId:long}/issues")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineIssueDto>> GetIssuesByMagazineAsync(long magazineId) =>
        context.MagazineIssues.Where(i => i.MagazineId == magazineId)
               .OrderBy(i => i.Published)
               .ThenBy(i => i.IssueNumber)
               .ThenBy(i => i.Caption)
               .Select(i => new MagazineIssueDto
                {
                    Id                 = i.Id,
                    MagazineId         = i.MagazineId,
                    MagazineTitle      = i.Magazine.Title,
                    Caption            = i.Caption,
                    NativeCaption      = i.NativeCaption,
                    Published          = i.Published,
                    PublishedPrecision = i.PublishedPrecision,
                    ProductCode        = i.ProductCode,
                    Pages              = i.Pages,
                    IssueNumber        = i.IssueNumber
                })
               .ToListAsync();

    /// <summary>
    /// Distinct list of publication years across all issues for the magazine. Years are
    /// returned in descending order (newest first); a single trailing <c>null</c> entry
    /// represents the "Others" bucket (issues with no <see cref="MagazineIssue.Published"/>
    /// value). Used by the public magazine view to render year navigation pills without
    /// materializing every issue row.
    /// </summary>
    [HttpGet("{id:long}/issue-years")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<int?>>> GetIssueYearsAsync(long id)
    {
        List<int?> raw = await context.MagazineIssues.AsNoTracking()
                                      .Where(i => i.MagazineId == id)
                                      .Select(i => i.Published.HasValue ? (int?)i.Published.Value.Year : null)
                                      .Distinct()
                                      .ToListAsync();

        // Sort client-side: years descending (newest first), nulls last for the "Others" pill.
        return raw.OrderByDescending(y => y.HasValue).ThenByDescending(y => y).ToList();
    }

    /// <summary>
    /// Issues for the magazine published in the given year, ordered chronologically then
    /// by issue number. Returns the lean issue DTO without <see cref="MagazineIssueDto.MagazineTitle"/>
    /// (already known by the caller — saves one join).
    /// </summary>
    [HttpGet("{id:long}/issues/by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineIssueDto>> GetIssuesByYearAsync(long id, int year) =>
        context.MagazineIssues.AsNoTracking()
               .Where(i => i.MagazineId == id && i.Published.HasValue && i.Published.Value.Year == year)
               .OrderBy(i => i.Published)
               .ThenBy(i => i.IssueNumber)
               .ThenBy(i => i.Caption)
               .Select(i => new MagazineIssueDto
                {
                    Id                     = i.Id,
                    MagazineId             = i.MagazineId,
                    Caption                = i.Caption,
                    NativeCaption          = i.NativeCaption,
                    Published              = i.Published,
                    PublishedPrecision     = i.PublishedPrecision,
                    ProductCode            = i.ProductCode,
                    Pages                  = i.Pages,
                    IssueNumber            = i.IssueNumber,
                    InternetArchiveUrl     = i.InternetArchiveUrl,
                    CoverGuid              = i.CoverGuid,
                    OriginalCoverExtension = i.OriginalCoverExtension
                })
               .ToListAsync();

    /// <summary>
    /// Issues for the magazine that lack a <see cref="MagazineIssue.Published"/> date —
    /// i.e. the "Others" bucket. Ordered by issue number then caption.
    /// </summary>
    [HttpGet("{id:long}/issues/no-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineIssueDto>> GetIssuesNoYearAsync(long id) =>
        context.MagazineIssues.AsNoTracking()
               .Where(i => i.MagazineId == id && !i.Published.HasValue)
               .OrderBy(i => i.IssueNumber)
               .ThenBy(i => i.Caption)
               .Select(i => new MagazineIssueDto
                {
                    Id                     = i.Id,
                    MagazineId             = i.MagazineId,
                    Caption                = i.Caption,
                    NativeCaption          = i.NativeCaption,
                    Published              = i.Published,
                    PublishedPrecision     = i.PublishedPrecision,
                    ProductCode            = i.ProductCode,
                    Pages                  = i.Pages,
                    IssueNumber            = i.IssueNumber,
                    InternetArchiveUrl     = i.InternetArchiveUrl,
                    CoverGuid              = i.CoverGuid,
                    OriginalCoverExtension = i.OriginalCoverExtension
                })
               .ToListAsync();

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<MagazineDto> GetAsync(long id) => context.Magazines.Where(b => b.Id == id)
                                                         .Select(b => new MagazineDto
                                                          {
                                                              Id               = b.Id,
                                                              Title            = b.Title,
                                                              NativeTitle      = b.NativeTitle,
                                                              SortTitle        = b.SortTitle,
                                                              Published                 = b.Published,
                                                              PublishedPrecision        = b.PublishedPrecision,
                                                              FirstPublication = b.FirstPublication,
                                                              FirstPublicationPrecision = b.FirstPublicationPrecision,
                                                              Issn             = b.Issn,
                                                              CountryId        = b.CountryId,
                                                              Country          = b.Country.Name
                                                          })
                                                         .FirstOrDefaultAsync();

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] MagazineDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Magazine model = await context.Magazines.FindAsync(id);

        if(model is null) return NotFound();

        model.Title            = dto.Title;
        model.NativeTitle      = dto.NativeTitle;
        model.SortTitle        = dto.SortTitle;
        model.Published                 = dto.Published;
        model.PublishedPrecision        = dto.PublishedPrecision;
        model.FirstPublication = dto.FirstPublication;
        model.FirstPublicationPrecision = dto.FirstPublicationPrecision;
        model.CountryId        = dto.CountryId;
        model.Issn             = dto.Issn;

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedMagazineInDb,
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
    public async Task<ActionResult<long>> CreateAsync([FromBody] MagazineDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Magazine
        {
            Title            = dto.Title,
            NativeTitle      = dto.NativeTitle,
            SortTitle        = dto.SortTitle,
            Published                 = dto.Published,
            PublishedPrecision        = dto.PublishedPrecision,
            FirstPublication = dto.FirstPublication,
            FirstPublicationPrecision = dto.FirstPublicationPrecision,
            CountryId        = dto.CountryId,
            Issn             = dto.Issn
        };

        await context.Magazines.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.NewMagazineInDb,
            Name    = dto.Title
        });

        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpGet("{id:long}/synopses")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DocumentSynopsisDto>> GetSynopsesAsync(long id) => context.MagazineSynopses
       .Where(s => s.MagazineId == id)
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
        DocumentSynopsisDto synopsis = await context.MagazineSynopses
                                                    .Where(s => s.MagazineId == id && s.LanguageCode == lang)
                                                    .Select(s => new DocumentSynopsisDto
                                                     {
                                                         Id           = s.Id,
                                                         Text         = s.Text,
                                                         LanguageCode = s.LanguageCode,
                                                         Language     = s.Language.ReferenceName
                                                     })
                                                    .FirstOrDefaultAsync();

        if(synopsis is null && lang != "eng")
            synopsis = await context.MagazineSynopses
                                    .Where(s => s.MagazineId == id && s.LanguageCode == "eng")
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

        MagazineSynopsis current = await context.MagazineSynopses
                                                .FirstOrDefaultAsync(s => s.MagazineId   == id &&
                                                                          s.LanguageCode == synopsis.LanguageCode);

        if(current is null)
        {
            current = new MagazineSynopsis
            {
                MagazineId   = id,
                LanguageCode = synopsis.LanguageCode,
                Text         = synopsis.Text
            };

            await context.MagazineSynopses.AddAsync(current);
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

        MagazineSynopsis synopsis = await context.MagazineSynopses
                                                 .FirstOrDefaultAsync(s => s.MagazineId   == id &&
                                                                           s.LanguageCode == languageCode);

        if(synopsis is null) return NotFound();

        // Capture display data BEFORE the cascade.
        string magazineTitle = await context.Magazines.AsNoTracking()
                                            .Where(m => m.Id == id)
                                            .Select(m => m.Title)
                                            .FirstOrDefaultAsync();
        string langName = await context.Iso639.AsNoTracking()
                                       .Where(l => l.Id == languageCode)
                                       .Select(l => l.ReferenceName)
                                       .FirstOrDefaultAsync();
        string subkeyLabel = $"({langName ?? languageCode} synopsis)";

        context.MagazineSynopses.Remove(synopsis);

        await context.SaveChangesWithUserAsync(userId);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntitySubkeyAsync(
            context, Marechai.Data.SuggestionEntityType.MagazineSynopsis,
            id, languageCode, magazineTitle ?? $"#{id}", subkeyLabel);

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
        Magazine item = await context.Magazines.FindAsync(id);

        if(item is null) return NotFound();

        string entityName = item.Title;

        context.Magazines.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        // Cascade: mark stale every entity-edit suggestion for this magazine AND every
        // per-language synopsis suggestion.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.Magazine, id, entityName);
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.MagazineSynopsis, id, entityName);

        return Ok();
    }
}