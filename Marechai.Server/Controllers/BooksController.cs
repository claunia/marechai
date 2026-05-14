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
using System.Threading;
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

[Route("/books")]
[ApiController]
public class BooksController(
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

    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetBooksCountAsync() => context.Books.CountAsync();

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMinimumYearAsync() => await context.Books
                                                                 .Where(b => b.Published.HasValue &&
                                                                             b.Published.Value.Year > 1000)
                                                                 .MinAsync(b => (int?)b.Published.Value.Year) ?? 0;

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMaximumYearAsync() => await context.Books
                                                                 .Where(b => b.Published.HasValue &&
                                                                             b.Published.Value.Year > 1000)
                                                                 .MaxAsync(b => (int?)b.Published.Value.Year) ?? 0;

    [HttpGet("companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetCompaniesAsync() => context.CompaniesByBooks
                                                                .Select(cb => cb.Company)
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
    public Task<List<CompanyDto>> GetCompaniesByLetterAsync(char c) => context.CompaniesByBooks
       .Select(cb => cb.Company)
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
    public Task<List<BookDto>> GetBooksByLetterAsync(char c, [FromQuery] int? skip = null,
                                                     [FromQuery] int? take = null,
                                                     CancellationToken cancellationToken = default)
    {
        IQueryable<Book> ordered = context.Books
                                          .Where(b =>
                                              (b.SortTitle != null &&
                                               EF.Functions.Like(b.SortTitle, $"{c}%")) ||
                                              (b.SortTitle == null &&
                                               EF.Functions.Like(b.Title, $"{c}%")))
                                          .OrderBy(b => MarechaiContext.NaturalSortKey(b.SortTitle))
                                          .ThenBy(b => MarechaiContext.NaturalSortKey(b.Title))
                                          .ThenBy(b => b.Published);

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(b => new BookDto
                       {
                           Id                     = b.Id,
                           Title                  = b.Title,
                           NativeTitle            = b.NativeTitle,
                           SortTitle              = b.SortTitle,
                           Published              = b.Published,
                           PublishedPrecision     = b.PublishedPrecision,
                           Isbn                   = b.Isbn,
                           CountryId              = b.CountryId,
                           Pages                  = b.Pages,
                           Country                = b.Country.Name,
                           CoverGuid              = b.CoverGuid,
                           OriginalCoverExtension = b.OriginalCoverExtension,
                           InternetArchiveUrl     = b.InternetArchiveUrl
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("by-letter/{c}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetBooksByLetterCountAsync(char c, CancellationToken cancellationToken = default) =>
        context.Books
               .Where(b =>
                   (b.SortTitle != null && EF.Functions.Like(b.SortTitle, $"{c}%")) ||
                   (b.SortTitle == null && EF.Functions.Like(b.Title, $"{c}%")))
               .CountAsync(cancellationToken);

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<BookDto>> GetBooksByYearAsync(int year, [FromQuery] int? skip = null,
                                                   [FromQuery] int? take = null,
                                                   CancellationToken cancellationToken = default)
    {
        IQueryable<Book> ordered = context.Books
                                          .Where(b => b.Published != null && b.Published.Value.Year == year)
                                          .OrderBy(b => MarechaiContext.NaturalSortKey(b.SortTitle))
                                          .ThenBy(b => MarechaiContext.NaturalSortKey(b.Title))
                                          .ThenBy(b => b.Published);

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(b => new BookDto
                       {
                           Id                     = b.Id,
                           Title                  = b.Title,
                           NativeTitle            = b.NativeTitle,
                           SortTitle              = b.SortTitle,
                           Published              = b.Published,
                           PublishedPrecision     = b.PublishedPrecision,
                           Isbn                   = b.Isbn,
                           CountryId              = b.CountryId,
                           Pages                  = b.Pages,
                           Country                = b.Country.Name,
                           CoverGuid              = b.CoverGuid,
                           OriginalCoverExtension = b.OriginalCoverExtension,
                           InternetArchiveUrl     = b.InternetArchiveUrl
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("by-year/{year:int}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetBooksByYearCountAsync(int year, CancellationToken cancellationToken = default) =>
        context.Books
               .Where(b => b.Published != null && b.Published.Value.Year == year)
               .CountAsync(cancellationToken);

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<BookDto>> GetAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                        CancellationToken cancellationToken = default)
    {
        IQueryable<Book> ordered = context.Books
                                          .OrderBy(b => MarechaiContext.NaturalSortKey(b.SortTitle))
                                          .ThenBy(b => b.Published)
                                          .ThenBy(b => MarechaiContext.NaturalSortKey(b.Title));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(b => new BookDto
                       {
                           Id                     = b.Id,
                           Title                  = b.Title,
                           NativeTitle            = b.NativeTitle,
                           SortTitle              = b.SortTitle,
                           Published              = b.Published,
                           PublishedPrecision     = b.PublishedPrecision,
                           Isbn                   = b.Isbn,
                           CountryId              = b.CountryId,
                           Pages                  = b.Pages,
                           Edition                = b.Edition,
                           PreviousId             = b.PreviousId,
                           SourceId               = b.SourceId,
                           Country                = b.Country.Name,
                           CoverGuid              = b.CoverGuid,
                           OriginalCoverExtension = b.OriginalCoverExtension,
                           InternetArchiveUrl     = b.InternetArchiveUrl
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<BookDto> GetAsync(long id) => context.Books.Where(b => b.Id == id)
                                                     .Select(b => new BookDto
                                                      {
                                                          Id                     = b.Id,
                                                          Title                  = b.Title,
                                                          NativeTitle            = b.NativeTitle,
                                                          SortTitle              = b.SortTitle,
                                                          Published              = b.Published,
                                                          PublishedPrecision     = b.PublishedPrecision,
                                                          Isbn                   = b.Isbn,
                                                          CountryId              = b.CountryId,
                                                          Pages                  = b.Pages,
                                                          Edition                = b.Edition,
                                                          PreviousId             = b.PreviousId,
                                                          SourceId               = b.SourceId,
                                                          Country                = b.Country.Name,
                                                          CoverGuid              = b.CoverGuid,
                                                          OriginalCoverExtension = b.OriginalCoverExtension,
                                                          InternetArchiveUrl     = b.InternetArchiveUrl
                                                      })
                                                     .FirstOrDefaultAsync();

    /// <summary>
    /// Consolidated payload for the public /book/{Id} view page. Replaces 6–8 sequential
    /// HTTP round-trips (head + previous + source + synopsis + people + companies +
    /// machines + machine-families) with a single response. The head + previous/source
    /// titles are pulled in one projected query; the synopsis fallback to English is
    /// collapsed into a single ordered query; the four child collections are fetched in
    /// parallel using independent DbContext instances from <c>IDbContextFactory</c>
    /// (DbContext is not thread-safe; sharing the request-scoped context across parallel
    /// branches throws InvalidOperationException).
    /// </summary>
    [HttpGet("{id:long}/full")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookFullDto>> GetFullAsync(long id, [FromQuery] string lang = "eng")
    {
        // The five child collections plus the synopsis only depend on `id` (from the
        // URL) and not on any value projected by the head query, so we fire all SIX
        // queries — head + synopsis + people + companies + machines + machine-families
        // — in parallel using independent DbContext instances from the factory
        // (DbContext is not thread-safe). Compared to running head first and then the
        // children in parallel, this saves one full ~180 ms RTT in the common case.
        await using var headCtx       = await dbFactory.CreateDbContextAsync();
        await using var synopsisCtx   = await dbFactory.CreateDbContextAsync();
        await using var peopleCtx     = await dbFactory.CreateDbContextAsync();
        await using var companiesCtx  = await dbFactory.CreateDbContextAsync();
        await using var machinesCtx   = await dbFactory.CreateDbContextAsync();
        await using var familiesCtx   = await dbFactory.CreateDbContextAsync();

        // Head + previous/source titles in a single projected join. Returns null
        // when the book does not exist; we surface that as 404 below.
        var headTask = headCtx.Books.AsNoTracking()
                              .Where(b => b.Id == id)
                              .Select(b => new
                               {
                                   b.Id,
                                   b.Title,
                                   b.NativeTitle,
                                   b.SortTitle,
                                   b.Published,
                                   b.PublishedPrecision,
                                   b.Isbn,
                                   b.CountryId,
                                   b.Pages,
                                   b.Edition,
                                   b.PreviousId,
                                   b.SourceId,
                                   CountryName            = b.Country.Name,
                                   b.CoverGuid,
                                   b.OriginalCoverExtension,
                                   b.InternetArchiveUrl,
                                   PreviousTitle = b.PreviousId.HasValue ? b.Previous.Title : null,
                                   SourceTitle   = b.SourceId.HasValue ? b.Source.Title : null
                               })
                              .FirstOrDefaultAsync();

        // Synopsis: collapse the original two-step lookup (try requested lang,
        // then English fallback) into a single ordered query. Synopses that
        // match the requested language sort first (key 0); English fallback
        // is key 1. FirstOrDefaultAsync returns the preferred row.
        Task<DocumentSynopsisDto> synopsisTask = synopsisCtx.BookSynopses.AsNoTracking()
            .Where(s => s.BookId == id && (s.LanguageCode == lang || s.LanguageCode == "eng"))
            .OrderBy(s => s.LanguageCode == lang ? 0 : 1)
            .Select(s => new DocumentSynopsisDto
             {
                 Id           = s.Id,
                 Text         = s.Text,
                 LanguageCode = s.LanguageCode,
                 Language     = s.Language.ReferenceName
             })
            .FirstOrDefaultAsync();

        // Mirrors PeopleByBookController.GetByBook — projection identical, sort
        // happens client-side because PersonByBookDto.FullName is a computed
        // string property and EF cannot translate it to SQL.
        Task<List<PersonByBookDto>> peopleTask = peopleCtx.PeopleByBooks.AsNoTracking()
            .Where(p => p.BookId == id)
            .Select(p => new PersonByBookDto
             {
                 Id          = p.Id,
                 Name        = p.Person.Name,
                 Surname     = p.Person.Surname,
                 Alias       = p.Person.Alias,
                 DisplayName = p.Person.DisplayName,
                 PersonId    = p.PersonId,
                 RoleId      = p.RoleId,
                 Role        = p.Role.Name,
                 BookId      = p.BookId
             })
            .ToListAsync();

        // Mirrors CompaniesByBookController.GetByBook.
        Task<List<CompanyByBookDto>> companiesTask = companiesCtx.CompaniesByBooks.AsNoTracking()
            .Where(p => p.BookId == id)
            .Select(p => new CompanyByBookDto
             {
                 Id        = p.Id,
                 Company   = p.Company.Name,
                 CompanyId = p.CompanyId,
                 RoleId    = p.RoleId,
                 Role      = p.Role.Name,
                 BookId    = p.BookId
             })
            .OrderBy(p => p.Company)
            .ThenBy(p => p.Role)
            .ToListAsync();

        // Mirrors BooksByMachineController.GetByBook.
        Task<List<BookByMachineDto>> machinesTask = machinesCtx.BooksByMachines.AsNoTracking()
            .Where(p => p.BookId == id)
            .Select(p => new BookByMachineDto
             {
                 Id        = p.Id,
                 BookId    = p.BookId,
                 MachineId = p.MachineId,
                 Machine   = p.Machine.Name
             })
            .OrderBy(p => p.Machine)
            .ToListAsync();

        // Mirrors BooksByMachineFamilyController.GetByBook.
        Task<List<BookByMachineFamilyDto>> familiesTask = familiesCtx.BooksByMachineFamilies.AsNoTracking()
            .Where(p => p.BookId == id)
            .Select(p => new BookByMachineFamilyDto
             {
                 Id              = p.Id,
                 BookId          = p.BookId,
                 MachineFamilyId = p.MachineFamilyId,
                 MachineFamily   = p.MachineFamily.Name
             })
            .OrderBy(p => p.MachineFamily)
            .ToListAsync();

        await Task.WhenAll(headTask, synopsisTask, peopleTask, companiesTask, machinesTask, familiesTask);

        var head = headTask.Result;

        if(head is null) return NotFound();

        var book = new BookDto
        {
            Id                     = head.Id,
            Title                  = head.Title,
            NativeTitle            = head.NativeTitle,
            SortTitle              = head.SortTitle,
            Published              = head.Published,
            PublishedPrecision     = head.PublishedPrecision,
            Isbn                   = head.Isbn,
            CountryId              = head.CountryId,
            Pages                  = head.Pages,
            Edition                = head.Edition,
            PreviousId             = head.PreviousId,
            SourceId               = head.SourceId,
            Country                = head.CountryName,
            CoverGuid              = head.CoverGuid,
            OriginalCoverExtension = head.OriginalCoverExtension,
            InternetArchiveUrl     = head.InternetArchiveUrl
        };

        List<PersonByBookDto> people = peopleTask.Result.OrderBy(p => p.FullName).ThenBy(p => p.Role).ToList();

        return new BookFullDto
        {
            Book              = book,
            PreviousBookTitle = head.PreviousTitle,
            SourceBookTitle   = head.SourceTitle,
            Synopsis          = synopsisTask.Result,
            People            = people,
            Companies         = companiesTask.Result,
            Machines          = machinesTask.Result,
            MachineFamilies   = familiesTask.Result
        };
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] BookDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        Book model = await context.Books.FindAsync(id);

        if(model is null) return NotFound();

        model.Title       = dto.Title;
        model.NativeTitle = dto.NativeTitle;
        model.SortTitle   = dto.SortTitle;
        model.Published   = dto.Published;
        model.PublishedPrecision = dto.PublishedPrecision;
        model.CountryId   = dto.CountryId;
        model.Isbn        = dto.Isbn;
        model.Pages       = dto.Pages;
        model.Edition     = dto.Edition;
        model.PreviousId  = dto.PreviousId;
        model.SourceId    = dto.SourceId;
        model.InternetArchiveUrl = dto.InternetArchiveUrl;

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedBookInDb,
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
    public async Task<ActionResult<long>> CreateAsync([FromBody] BookDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Book
        {
            Title       = dto.Title,
            NativeTitle = dto.NativeTitle,
            SortTitle   = dto.SortTitle,
            Published   = dto.Published,
            PublishedPrecision = dto.PublishedPrecision,
            CountryId   = dto.CountryId,
            Isbn        = dto.Isbn,
            Pages       = dto.Pages,
            Edition     = dto.Edition,
            PreviousId  = dto.PreviousId,
            SourceId    = dto.SourceId,
            InternetArchiveUrl = dto.InternetArchiveUrl
        };

        await context.Books.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.NewBookInDb,
            Name    = dto.Title
        });

        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpGet("{id:long}/synopses")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DocumentSynopsisDto>> GetSynopsesAsync(long id) => context.BookSynopses
       .Where(s => s.BookId == id)
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
        DocumentSynopsisDto synopsis = await context.BookSynopses
                                                    .Where(s => s.BookId == id && s.LanguageCode == lang)
                                                    .Select(s => new DocumentSynopsisDto
                                                     {
                                                         Id           = s.Id,
                                                         Text         = s.Text,
                                                         LanguageCode = s.LanguageCode,
                                                         Language     = s.Language.ReferenceName
                                                     })
                                                    .FirstOrDefaultAsync();

        if(synopsis is null && lang != "eng")
            synopsis = await context.BookSynopses
                                    .Where(s => s.BookId == id && s.LanguageCode == "eng")
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

        BookSynopsis current = await context.BookSynopses
                                            .FirstOrDefaultAsync(s => s.BookId       == id &&
                                                                      s.LanguageCode == synopsis.LanguageCode);

        if(current is null)
        {
            current = new BookSynopsis
            {
                BookId       = id,
                LanguageCode = synopsis.LanguageCode,
                Text         = synopsis.Text
            };

            await context.BookSynopses.AddAsync(current);
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

        BookSynopsis synopsis = await context.BookSynopses
                                             .FirstOrDefaultAsync(s => s.BookId       == id &&
                                                                       s.LanguageCode == languageCode);

        if(synopsis is null) return NotFound();

        // Capture display data BEFORE the cascade, while the book + language rows are still
        // available for the system message body.
        string bookTitle = await context.Books.AsNoTracking()
                                        .Where(b => b.Id == id)
                                        .Select(b => b.Title)
                                        .FirstOrDefaultAsync();
        string langName  = await context.Iso639.AsNoTracking()
                                        .Where(l => l.Id == languageCode)
                                        .Select(l => l.ReferenceName)
                                        .FirstOrDefaultAsync();
        string subkeyLabel = $"({langName ?? languageCode} synopsis)";

        context.BookSynopses.Remove(synopsis);

        await context.SaveChangesWithUserAsync(userId);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntitySubkeyAsync(
            context, Marechai.Data.SuggestionEntityType.BookSynopsis,
            id, languageCode, bookTitle ?? $"#{id}", subkeyLabel);

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

        Book item = await context.Books.FindAsync(id);

        if(item is null) return NotFound();

        string entityName = item.Title;

        context.Books.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        // Cascade: mark stale every entity-level Book suggestion for this book.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.Book, id, entityName);

        // Cascade: mark stale every per-language synopsis suggestion for this book.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.BookSynopsis, id, entityName);

        return Ok();
    }

    [HttpPost("{id:long}/cover/upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BookDto>> UploadCoverAsync(long id, IFormFile file)
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

        Book book = await context.Books.FindAsync(id);

        if(book is null) return NotFound();

        // If cover already exists, delete old files
        if(book.CoverGuid.HasValue)
            DeleteCoverFiles(book.CoverGuid.Value);

        Guid coverGuid = Guid.NewGuid();

        // Save original file to disk
        Photos.EnsureCreated(_assetRootPath, false, "book-covers");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "book-covers", "originals");
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
            photos.ConversionWorker(_assetRootPath, coverGuid, originalPath, sourceFormat, false, "book-covers");
        });

        // Update book record
        book.CoverGuid              = coverGuid;
        book.OriginalCoverExtension = extension.TrimStart('.');
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new BookDto
        {
            Id                     = book.Id,
            Title                  = book.Title,
            NativeTitle            = book.NativeTitle,
            Published              = book.Published,
            PublishedPrecision     = book.PublishedPrecision,
            Isbn                   = book.Isbn,
            CountryId              = book.CountryId,
            Pages                  = book.Pages,
            Edition                = book.Edition,
            PreviousId             = book.PreviousId,
            SourceId               = book.SourceId,
            CoverGuid              = book.CoverGuid,
            OriginalCoverExtension = book.OriginalCoverExtension
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

        Book book = await context.Books.FindAsync(id);

        if(book is null) return NotFound();

        if(!book.CoverGuid.HasValue) return NoContent();

        DeleteCoverFiles(book.CoverGuid.Value);

        book.CoverGuid              = null;
        book.OriginalCoverExtension = null;
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    // ───────────────────────────── Pending cover (collaborator upload) ─────────────────────────────

    /// <summary>
    ///     Allowed file extensions for collaborator-uploaded pending covers. Stricter than
    ///     the admin upload (which also accepts TIFF/BMP) — pending covers go through admin
    ///     review and we want to constrain the surface to web-friendly formats only.
    /// </summary>
    static readonly HashSet<string> _pendingAllowedExtensions =
        Marechai.Server.Helpers.PendingImageStore.AllowedExtensions;

    static readonly HashSet<string> _pendingAllowedContentTypes =
        Marechai.Server.Helpers.PendingImageStore.AllowedContentTypes;

    /// <summary>
    ///     Upload a pending cover for a book that the caller is suggesting an edit on.
    ///     Accepts JPG/PNG/WebP up to 50 MB; the file is stored unchanged in
    ///     <c>book-covers/pending/&lt;guid&gt;.&lt;ext&gt;</c> with a sidecar JSON file recording
    ///     the uploader. Auto-deletes any prior pending covers from the same uploader for
    ///     the same book so a user always has at most one pending cover per book in flight.
    ///     The returned <c>{guid, extension}</c> must be embedded in the
    ///     <c>cover_pending_guid</c> + <c>cover_pending_extension</c> fields of the
    ///     subsequent suggestion submission.
    /// </summary>
    [HttpPost("{id:long}/cover/pending")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(PendingImageUploadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PendingImageUploadDto>> UploadPendingCoverAsync(long id, IFormFile file)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        if(file is null || file.Length == 0)
            return BadRequest("No file provided.");

        if(file.Length > 50 * 1024 * 1024)
            return BadRequest("File exceeds 50 MB limit.");

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        if(!_pendingAllowedExtensions.Contains(extension))
            return BadRequest("Unsupported file format. Accepted: JPEG, PNG, WebP.");

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_pendingAllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Unsupported content type.");

        // Verify the targeted book exists; we don't want stray uploads for nonexistent ids.
        bool bookExists = await context.Books.AsNoTracking().AnyAsync(b => b.Id == id);
        if(!bookExists) return NotFound();

        // Cleanup: each user gets at most ONE pending cover per book. Replace any prior
        // upload before storing the new one.
        Marechai.Server.Helpers.PendingImageStore.DeleteByUploaderForEntity(
            _assetRootPath, "book-covers", userId, (byte)Marechai.Data.SuggestionEntityType.Book, id);

        await using var stream = file.OpenReadStream();
        Guid guid = await Marechai.Server.Helpers.PendingImageStore.StoreAsync(
            _assetRootPath, "book-covers", extension,
            (byte)Marechai.Data.SuggestionEntityType.Book, id, userId,
            file.ContentType, stream);

        return Ok(new PendingImageUploadDto { Guid = guid, Extension = extension.TrimStart('.') });
    }

    /// <summary>
    ///     Upload a pending cover for a brand-new book that doesn't exist yet — paired with
    ///     the addition-mode <c>POST /suggestions</c> flow (entityType=Book, entityId=null).
    ///     Identical to <see cref="UploadPendingCoverAsync" /> except it skips the
    ///     book-existence check and stores the file with <c>EntityId=0</c> in the sidecar
    ///     (since the actual book id only comes into existence when the suggestion is
    ///     accepted). The "one pending cover per (uploader, entity)" cleanup keys on
    ///     <c>(uploader, EntityType=Book, EntityId=0)</c>, so the user always has at most
    ///     one in-flight new-book cover at a time.
    /// </summary>
    [HttpPost("cover/pending/new")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(PendingImageUploadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PendingImageUploadDto>> UploadPendingNewBookCoverAsync(IFormFile file)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        if(file is null || file.Length == 0)
            return BadRequest("No file provided.");

        if(file.Length > 50 * 1024 * 1024)
            return BadRequest("File exceeds 50 MB limit.");

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        if(!_pendingAllowedExtensions.Contains(extension))
            return BadRequest("Unsupported file format. Accepted: JPEG, PNG, WebP.");

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_pendingAllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Unsupported content type.");

        // Cleanup: each user gets at most ONE pending NEW-book cover at a time. Replace any
        // prior upload (keyed on EntityId=0) before storing the new one.
        Marechai.Server.Helpers.PendingImageStore.DeleteByUploaderForEntity(
            _assetRootPath, "book-covers", userId, (byte)Marechai.Data.SuggestionEntityType.Book, 0L);

        await using var stream = file.OpenReadStream();
        Guid guid = await Marechai.Server.Helpers.PendingImageStore.StoreAsync(
            _assetRootPath, "book-covers", extension,
            (byte)Marechai.Data.SuggestionEntityType.Book, 0L, userId,
            file.ContentType, stream);

        return Ok(new PendingImageUploadDto { Guid = guid, Extension = extension.TrimStart('.') });
    }

    /// <summary>
    ///     Serve a pending cover image. Authorization: the uploader OR any admin/uberadmin
    ///     can view (so the dialog preview works for the contributor and the review queue
    ///     works for the moderator).
    /// </summary>
    [HttpGet("cover/pending/{guid:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPendingCoverAsync(Guid guid)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        var meta = await Marechai.Server.Helpers.PendingImageStore.GetMetadataAsync(
            _assetRootPath, "book-covers", guid);
        if(meta is null) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");
        if(!Marechai.Server.Helpers.PendingImageStore.CanAccess(meta, userId, isAdmin))
            return Forbid();

        string filePath = await Marechai.Server.Helpers.PendingImageStore.GetImagePathAsync(
            _assetRootPath, "book-covers", guid);
        if(filePath is null) return NotFound();

        string contentType = !string.IsNullOrEmpty(meta.ContentType) ? meta.ContentType : "application/octet-stream";
        return PhysicalFile(filePath, contentType);
    }

    /// <summary>
    ///     Explicitly delete a pending cover (uploader OR admin). Useful for the
    ///     "remove cover before submit" UX in the dialog and for admin-side cleanup of
    ///     orphaned pending uploads.
    /// </summary>
    [HttpDelete("cover/pending/{guid:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePendingCoverAsync(Guid guid)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        var meta = await Marechai.Server.Helpers.PendingImageStore.GetMetadataAsync(
            _assetRootPath, "book-covers", guid);
        if(meta is null) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");
        if(!Marechai.Server.Helpers.PendingImageStore.CanAccess(meta, userId, isAdmin))
            return Forbid();

        Marechai.Server.Helpers.PendingImageStore.Delete(_assetRootPath, "book-covers", guid);
        return NoContent();
    }

    void DeleteCoverFiles(Guid coverGuid)
    {
        string photosRoot = Path.Combine(_assetRootPath, "photos", "book-covers");
        string guidStr    = coverGuid.ToString();

        // Delete original
        DeleteFilesByPattern(Path.Combine(photosRoot, "originals"), $"{guidStr}.*");

        // Delete all format/resolution variants (full + thumbnails)
        string[] formats     = ["jpeg", "webp", "avif"];
        string[] resolutions = ["4k"];

        foreach(string format in formats)
        {
            string ext = format switch
            {
                "jpeg" => ".jpg",
                "webp" => ".webp",
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