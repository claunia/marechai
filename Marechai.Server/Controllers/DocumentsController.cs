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
using System.Globalization;
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

[Route("/documents")]
[ApiController]
public class DocumentsController(MarechaiContext context) : ControllerBase
{
    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetDocumentsCountAsync([FromQuery(Name = "filters")] string[] filters = null,
                                            CancellationToken cancellationToken = default) =>
        ApplyFilters(context.Documents.AsNoTracking(), filters).CountAsync(cancellationToken);

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMinimumYearAsync() => await context.Documents
                                                                 .Where(d => d.Published.HasValue &&
                                                                             d.Published.Value.Year > 1000)
                                                                 .MinAsync(d => (int?)d.Published.Value.Year) ?? 0;

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMaximumYearAsync() => await context.Documents
                                                                 .Where(d => d.Published.HasValue &&
                                                                             d.Published.Value.Year > 1000)
                                                                 .MaxAsync(d => (int?)d.Published.Value.Year) ?? 0;

    [HttpGet("companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetCompaniesAsync() => context.CompaniesByDocuments
                                                                .Select(cd => cd.Company)
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
    public Task<List<CompanyDto>> GetCompaniesByLetterAsync(char c) => context.CompaniesByDocuments
       .Select(cd => cd.Company)
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
    public Task<List<DocumentDto>> GetDocumentsByLetterAsync(char c, [FromQuery] int? skip = null,
                                                             [FromQuery] int? take = null,
                                                             CancellationToken cancellationToken = default)
    {
        IQueryable<Document> ordered = context.Documents
                                              .Where(d =>
                                                  (d.SortTitle != null && EF.Functions.Like(d.SortTitle, $"{c}%")) ||
                                                  (d.SortTitle == null && EF.Functions.Like(d.Title, $"{c}%")))
                                              .OrderBy(d => MarechaiContext.NaturalSortKey(d.SortTitle))
                                              .ThenBy(d => MarechaiContext.NaturalSortKey(d.Title))
                                              .ThenBy(d => d.Published);

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(d => new DocumentDto
                       {
                           Id                 = d.Id,
                           Title              = d.Title,
                           NativeTitle        = d.NativeTitle,
                           SortTitle          = d.SortTitle,
                           Published          = d.Published,
                           PublishedPrecision = d.PublishedPrecision,
                           CountryId          = d.CountryId,
                           Country            = d.Country.Name,
                           InternetArchiveUrl = d.InternetArchiveUrl
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("by-letter/{c}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetDocumentsByLetterCountAsync(char c, CancellationToken cancellationToken = default) =>
        context.Documents
               .Where(d =>
                   (d.SortTitle != null && EF.Functions.Like(d.SortTitle, $"{c}%")) ||
                   (d.SortTitle == null && EF.Functions.Like(d.Title, $"{c}%")))
               .CountAsync(cancellationToken);

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DocumentDto>> GetDocumentsByYearAsync(int year, [FromQuery] int? skip = null,
                                                           [FromQuery] int? take = null,
                                                           CancellationToken cancellationToken = default)
    {
        IQueryable<Document> ordered = context.Documents
                                              .Where(d => d.Published != null && d.Published.Value.Year == year)
                                              .OrderBy(d => MarechaiContext.NaturalSortKey(d.SortTitle))
                                              .ThenBy(d => MarechaiContext.NaturalSortKey(d.Title))
                                              .ThenBy(d => d.Published);

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(d => new DocumentDto
                       {
                           Id                 = d.Id,
                           Title              = d.Title,
                           NativeTitle        = d.NativeTitle,
                           SortTitle          = d.SortTitle,
                           Published          = d.Published,
                           PublishedPrecision = d.PublishedPrecision,
                           CountryId          = d.CountryId,
                           Country            = d.Country.Name,
                           InternetArchiveUrl = d.InternetArchiveUrl
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("by-year/{year:int}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetDocumentsByYearCountAsync(int year, CancellationToken cancellationToken = default) =>
        context.Documents.Where(d => d.Published != null && d.Published.Value.Year == year)
               .CountAsync(cancellationToken);

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DocumentDto>> GetAsync([FromQuery] int?    skip           = null,
                                            [FromQuery] int?    take           = null,
                                            [FromQuery] string  sortBy         = null,
                                            [FromQuery] bool    sortDescending = false,
                                            [FromQuery(Name = "filters")] string[] filters = null,
                                            CancellationToken cancellationToken = default)
    {
        IQueryable<Document> query = ApplyFilters(context.Documents.AsNoTracking(), filters);

        // When no user-supplied sort is set, keep the legacy default ordering
        // (SortTitle → Published → Title) so anonymous unfiltered consumers see
        // the same order they get today.
        IOrderedQueryable<Document> ordered = sortBy switch
        {
            "Title" => sortDescending
                           ? query.OrderByDescending(b => MarechaiContext.NaturalSortKey(b.Title))
                           : query.OrderBy(b => MarechaiContext.NaturalSortKey(b.Title)),
            "Published" => sortDescending
                               ? query.OrderByDescending(b => b.Published)
                               : query.OrderBy(b => b.Published),
            "Country" => sortDescending
                             ? query.OrderByDescending(b => MarechaiContext.NaturalSortKey(b.Country.Name))
                             : query.OrderBy(b => MarechaiContext.NaturalSortKey(b.Country.Name)),
            "InternetArchiveUrl" => sortDescending
                                        ? query.OrderByDescending(b => b.InternetArchiveUrl)
                                        : query.OrderBy(b => b.InternetArchiveUrl),
            _ => query.OrderBy(b => MarechaiContext.NaturalSortKey(b.SortTitle))
                      .ThenBy(b => b.Published)
                      .ThenBy(b => MarechaiContext.NaturalSortKey(b.Title))
        };

        IQueryable<Document> paged = ordered;
        if(skip.HasValue) paged = paged.Skip(skip.Value);
        if(take.HasValue) paged = paged.Take(take.Value);

        return paged.Select(b => new DocumentDto
                     {
                         Id                 = b.Id,
                         Title              = b.Title,
                         NativeTitle        = b.NativeTitle,
                         SortTitle          = b.SortTitle,
                         Published          = b.Published,
                         PublishedPrecision = b.PublishedPrecision,
                         CountryId          = b.CountryId,
                         Country            = b.Country.Name,
                         InternetArchiveUrl = b.InternetArchiveUrl
                     })
                    .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Translates MudDataGrid <c>FilterDefinition</c>s wired over the network as
    /// <c>"{Column}||{Operator}||{Value}"</c> triples into LINQ predicates against
    /// the <see cref="Document"/> entity. Recognized columns mirror the
    /// <c>PropertyColumn</c> names emitted by the admin grid: <c>Title</c>,
    /// <c>Published</c>, <c>Country</c>, <c>InternetArchiveUrl</c>. Unknown columns
    /// and operators are silently ignored — filters MudBlazor may emit for
    /// non-existent columns must never 400 the listing call.
    /// </summary>
    static IQueryable<Document> ApplyFilters(IQueryable<Document> query, string[] filters)
    {
        if(filters is null || filters.Length == 0) return query;

        foreach(string raw in filters)
        {
            if(string.IsNullOrWhiteSpace(raw)) continue;

            string[] parts = raw.Split("||", 3, StringSplitOptions.None);
            if(parts.Length < 2) continue;

            string column   = parts[0];
            string op       = parts[1];
            string value    = parts.Length >= 3 ? parts[2] : string.Empty;
            bool   isEmpty  = op == "is empty";
            bool   isNotEmp = op == "is not empty";

            // Skip non-empty-check operators with no value supplied so a stray
            // open-but-unfilled filter UI doesn't accidentally hide every row.
            if(!isEmpty && !isNotEmp && string.IsNullOrEmpty(value)) continue;

            switch(column)
            {
                case "Title":
                    query = op switch
                    {
                        "contains"     => query.Where(d => d.Title.Contains(value)),
                        "not contains" => query.Where(d => !d.Title.Contains(value)),
                        "equals"       => query.Where(d => d.Title == value),
                        "not equals"   => query.Where(d => d.Title != value),
                        "starts with"  => query.Where(d => d.Title.StartsWith(value)),
                        "ends with"    => query.Where(d => d.Title.EndsWith(value)),
                        "is empty"     => query.Where(d => d.Title == null || d.Title == string.Empty),
                        "is not empty" => query.Where(d => d.Title != null && d.Title != string.Empty),
                        _              => query
                    };
                    break;

                case "Country":
                    query = op switch
                    {
                        "contains"     => query.Where(d => d.Country.Name.Contains(value)),
                        "not contains" => query.Where(d => !d.Country.Name.Contains(value)),
                        "equals"       => query.Where(d => d.Country.Name == value),
                        "not equals"   => query.Where(d => d.Country.Name != value),
                        "starts with"  => query.Where(d => d.Country.Name.StartsWith(value)),
                        "ends with"    => query.Where(d => d.Country.Name.EndsWith(value)),
                        "is empty"     => query.Where(d => d.CountryId == null ||
                                                          d.Country.Name == null ||
                                                          d.Country.Name == string.Empty),
                        "is not empty" => query.Where(d => d.CountryId != null &&
                                                          d.Country.Name != null &&
                                                          d.Country.Name != string.Empty),
                        _              => query
                    };
                    break;

                case "InternetArchiveUrl":
                    query = op switch
                    {
                        "contains"     => query.Where(d => d.InternetArchiveUrl != null &&
                                                          d.InternetArchiveUrl.Contains(value)),
                        "not contains" => query.Where(d => d.InternetArchiveUrl == null ||
                                                          !d.InternetArchiveUrl.Contains(value)),
                        "equals"       => query.Where(d => d.InternetArchiveUrl == value),
                        "not equals"   => query.Where(d => d.InternetArchiveUrl != value),
                        "starts with"  => query.Where(d => d.InternetArchiveUrl != null &&
                                                          d.InternetArchiveUrl.StartsWith(value)),
                        "ends with"    => query.Where(d => d.InternetArchiveUrl != null &&
                                                          d.InternetArchiveUrl.EndsWith(value)),
                        "is empty"     => query.Where(d => d.InternetArchiveUrl == null ||
                                                          d.InternetArchiveUrl == string.Empty),
                        "is not empty" => query.Where(d => d.InternetArchiveUrl != null &&
                                                          d.InternetArchiveUrl != string.Empty),
                        _              => query
                    };
                    break;

                case "Published":
                    if(op == "is empty")    { query = query.Where(d => d.Published == null); break; }
                    if(op == "is not empty"){ query = query.Where(d => d.Published != null); break; }

                    if(!DateTime.TryParse(value, CultureInfo.InvariantCulture,
                                          DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                          out DateTime parsed))
                        continue;

                    DateTime day = parsed.Date;

                    query = op switch
                    {
                        "is"              => query.Where(d => d.Published.HasValue && d.Published.Value.Date == day),
                        "is not"          => query.Where(d => d.Published.HasValue && d.Published.Value.Date != day),
                        "is after"        => query.Where(d => d.Published.HasValue && d.Published.Value.Date >  day),
                        "is before"       => query.Where(d => d.Published.HasValue && d.Published.Value.Date <  day),
                        "is on or after"  => query.Where(d => d.Published.HasValue && d.Published.Value.Date >= day),
                        "is on or before" => query.Where(d => d.Published.HasValue && d.Published.Value.Date <= day),
                        _                 => query
                    };
                    break;
            }
        }

        return query;
    }

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
                                                              Country     = b.Country.Name,
                                                              InternetArchiveUrl = b.InternetArchiveUrl
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
        model.InternetArchiveUrl = dto.InternetArchiveUrl;

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
            CountryId   = dto.CountryId,
            InternetArchiveUrl = dto.InternetArchiveUrl
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

        // Capture display data BEFORE the cascade.
        string documentTitle = await context.Documents.AsNoTracking()
                                            .Where(d => d.Id == id)
                                            .Select(d => d.Title)
                                            .FirstOrDefaultAsync();
        string langName = await context.Iso639.AsNoTracking()
                                       .Where(l => l.Id == languageCode)
                                       .Select(l => l.ReferenceName)
                                       .FirstOrDefaultAsync();
        string subkeyLabel = $"({langName ?? languageCode} synopsis)";

        context.DocumentSynopses.Remove(synopsis);

        await context.SaveChangesWithUserAsync(userId);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntitySubkeyAsync(
            context, Marechai.Data.SuggestionEntityType.DocumentSynopsis,
            id, languageCode, documentTitle ?? $"#{id}", subkeyLabel);

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

        string entityName = item.Title;

        context.Documents.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        // Cascade: mark stale every entity-edit suggestion for this document AND every
        // per-language synopsis suggestion. Both share the same Document FK so deleting the
        // row leaves both flavours of pending suggestion targeting a now-missing row.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.Document, id, entityName);
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.DocumentSynopsis, id, entityName);

        return Ok();
    }
}