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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Markdig;

namespace Marechai.Server.Controllers;

[Route("/people")]
[ApiController]
public class PeopleController(
    MarechaiContext                    context,
    IConfiguration                     configuration,
    IDbContextFactory<MarechaiContext> dbFactory) : ControllerBase
{
    static readonly HashSet<string> _pendingAllowedExtensions =
        Marechai.Server.Helpers.PendingImageStore.AllowedExtensions;

    static readonly HashSet<string> _pendingAllowedContentTypes =
        Marechai.Server.Helpers.PendingImageStore.AllowedContentTypes;

    readonly string _assetRootPath = configuration["AssetRootPath"]!;

    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetPeopleCountAsync() => context.People.CountAsync();

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMinimumYearAsync() => await context.People
                                                                 .Where(p => p.BirthDate > DateTime.MinValue &&
                                                                             p.BirthDate.Year > 1000)
                                                                 .MinAsync(p => (int?)p.BirthDate.Year) ?? 0;

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMaximumYearAsync() => await context.People
                                                                 .Where(p => p.BirthDate > DateTime.MinValue &&
                                                                             p.BirthDate.Year > 1000)
                                                                 .MaxAsync(p => (int?)p.BirthDate.Year) ?? 0;

    [HttpGet("by-letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonDto>> GetPeopleByLetterAsync(char c, [FromQuery] int? skip = null,
                                                        [FromQuery] int? take = null,
                                                        CancellationToken cancellationToken = default)
    {
        IQueryable<Person> ordered = context.People
            .Where(p =>
                (p.DisplayName != null &&
                 EF.Functions.Like(p.DisplayName, $"{c}%")) ||
                (p.DisplayName == null && p.Alias != null &&
                 EF.Functions.Like(p.Alias, $"{c}%")) ||
                (p.DisplayName == null && p.Alias == null &&
                 EF.Functions.Like(p.Surname, $"{c}%")))
            .OrderBy(p => MarechaiContext.NaturalSortKey(p.DisplayName))
            .ThenBy(p => MarechaiContext.NaturalSortKey(p.Alias))
            .ThenBy(p => MarechaiContext.NaturalSortKey(p.Name))
            .ThenBy(p => MarechaiContext.NaturalSortKey(p.Surname));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(p => new PersonDto
                       {
                           Id                 = p.Id,
                           Name               = p.Name,
                           Surname            = p.Surname,
                           CountryOfBirth     = p.CountryOfBirth.Name,
                           CountryOfBirthId   = p.CountryOfBirthId,
                           BirthDate          = p.BirthDate,
                           BirthDatePrecision = p.BirthDatePrecision,
                           DeathDate          = p.DeathDate,
                           DeathDatePrecision = p.DeathDatePrecision,
                           Photo              = p.Photo,
                           Alias              = p.Alias,
                           DisplayName        = p.DisplayName
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("by-letter/{c}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetPeopleByLetterCountAsync(char c, CancellationToken cancellationToken = default) =>
        context.People
               .Where(p =>
                   (p.DisplayName != null &&
                    EF.Functions.Like(p.DisplayName, $"{c}%")) ||
                   (p.DisplayName == null && p.Alias != null &&
                    EF.Functions.Like(p.Alias, $"{c}%")) ||
                   (p.DisplayName == null && p.Alias == null &&
                    EF.Functions.Like(p.Surname, $"{c}%")))
               .CountAsync(cancellationToken);

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonDto>> GetPeopleByYearAsync(int year, [FromQuery] int? skip = null,
                                                      [FromQuery] int? take = null,
                                                      CancellationToken cancellationToken = default)
    {
        IQueryable<Person> ordered = context.People
            .Where(p => p.BirthDate > DateTime.MinValue && p.BirthDate.Year == year)
            .OrderBy(p => MarechaiContext.NaturalSortKey(p.DisplayName))
            .ThenBy(p => MarechaiContext.NaturalSortKey(p.Alias))
            .ThenBy(p => MarechaiContext.NaturalSortKey(p.Name))
            .ThenBy(p => MarechaiContext.NaturalSortKey(p.Surname));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(p => new PersonDto
                       {
                           Id                 = p.Id,
                           Name               = p.Name,
                           Surname            = p.Surname,
                           CountryOfBirth     = p.CountryOfBirth.Name,
                           CountryOfBirthId   = p.CountryOfBirthId,
                           BirthDate          = p.BirthDate,
                           BirthDatePrecision = p.BirthDatePrecision,
                           DeathDate          = p.DeathDate,
                           DeathDatePrecision = p.DeathDatePrecision,
                           Photo              = p.Photo,
                           Alias              = p.Alias,
                           DisplayName        = p.DisplayName
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("by-year/{year:int}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetPeopleByYearCountAsync(int year, CancellationToken cancellationToken = default) =>
        context.People
               .Where(p => p.BirthDate > DateTime.MinValue && p.BirthDate.Year == year)
               .CountAsync(cancellationToken);

    [HttpGet("{personId:int}/books")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonByBookDto>> GetBooksByPersonAsync(int personId) =>
        context.PeopleByBooks.AsNoTracking()
               .Where(p => p.PersonId == personId)
               .OrderBy(p => p.Role.Name)
               .Select(p => new PersonByBookDto
                {
                    Id          = p.Id,
                    PersonId    = p.PersonId,
                    BookId      = p.BookId,
                    RoleId      = p.RoleId,
                    Role        = p.Role.Name,
                    BookTitle   = p.Book.Title,
                    Name        = p.Person.Name,
                    Surname     = p.Person.Surname,
                    Alias       = p.Person.Alias,
                    DisplayName = p.Person.DisplayName
                })
               .ToListAsync();

    [HttpGet("{personId:int}/documents")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonByDocumentDto>> GetDocumentsByPersonAsync(int personId) =>
        context.PeopleByDocuments.AsNoTracking()
               .Where(p => p.PersonId == personId)
               .OrderBy(p => p.Role.Name)
               .Select(p => new PersonByDocumentDto
                {
                    Id            = p.Id,
                    PersonId      = p.PersonId,
                    DocumentId    = p.DocumentId,
                    RoleId        = p.RoleId,
                    Role          = p.Role.Name,
                    DocumentTitle = p.Document.Title,
                    Name          = p.Person.Name,
                    Surname       = p.Person.Surname,
                    Alias         = p.Person.Alias,
                    DisplayName   = p.Person.DisplayName
                })
               .ToListAsync();

    [HttpGet("{personId:int}/magazines")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonByMagazineDto>> GetMagazinesByPersonAsync(int personId) =>
        context.PeopleByMagazines.AsNoTracking()
               .Where(p => p.PersonId == personId)
               .OrderBy(p => p.Role.Name)
               .Select(p => new PersonByMagazineDto
                {
                    Id            = p.Id,
                    PersonId      = p.PersonId,
                    MagazineId    = p.MagazineId,
                    RoleId        = p.RoleId,
                    Role          = p.Role.Name,
                    MagazineTitle = p.Magazine.Magazine.Title,
                    Name          = p.Person.Name,
                    Surname       = p.Person.Surname,
                    Alias         = p.Person.Alias,
                    DisplayName   = p.Person.DisplayName
                })
               .ToListAsync();

    [HttpGet("{personId:int}/software")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonBySoftwareDto>> GetSoftwareByPersonAsync(int personId) =>
        context.PeopleBySoftware.AsNoTracking()
               .Where(p => p.PersonId == personId)
               .OrderBy(p => p.Software.Name)
               .ThenBy(p => p.DocumentRole != null ? p.DocumentRole.Name : p.Role)
               .Select(p => new PersonBySoftwareDto
                {
                    Id           = p.Id,
                    PersonId     = p.PersonId,
                    SoftwareId   = p.SoftwareId,
                    Role         = p.DocumentRole != null ? p.DocumentRole.Name : p.Role,
                    SoftwareName = p.Software.Name,
                    Name         = p.Person.Name,
                    Surname      = p.Person.Surname,
                    Alias        = p.Person.Alias,
                    DisplayName  = p.Person.DisplayName
                })
               .ToListAsync();

    [HttpGet("{personId:int}/companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonByCompanyDto>> GetCompaniesByPersonAsync(int personId) =>
        context.PeopleByCompanies.AsNoTracking()
               .Where(p => p.PersonId == personId)
               .OrderBy(p => p.Company.Name)
               .ThenBy(p => p.Position)
               .ThenBy(p => p.Start)
               .Select(p => new PersonByCompanyDto
                {
                    Id          = p.Id,
                    PersonId    = p.PersonId,
                    CompanyId   = p.CompanyId,
                    CompanyName = p.Company.Name,
                    Position    = p.Position,
                    Start       = p.Start,
                    End         = p.End,
                    Ongoing     = p.Ongoing,
                    Name        = p.Person.Name,
                    Surname     = p.Person.Surname,
                    Alias       = p.Person.Alias,
                    DisplayName = p.Person.DisplayName
                })
               .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonDto>> GetAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                          [FromQuery] string search = null,
                                          CancellationToken cancellationToken = default)
    {
        IQueryable<Person> baseQ = context.People;

        if(!string.IsNullOrWhiteSpace(search))
            baseQ = baseQ.Where(p => (p.DisplayName != null && p.DisplayName.Contains(search))   ||
                                     (p.Alias       != null && p.Alias.Contains(search))         ||
                                     (p.Name        != null && p.Name.Contains(search))          ||
                                     (p.Surname     != null && p.Surname.Contains(search)));

        IQueryable<Person> ordered = baseQ
                                            .OrderBy(p => MarechaiContext.NaturalSortKey(p.DisplayName))
                                            .ThenBy(p => MarechaiContext.NaturalSortKey(p.Alias))
                                            .ThenBy(p => MarechaiContext.NaturalSortKey(p.Name))
                                            .ThenBy(p => MarechaiContext.NaturalSortKey(p.Surname));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(p => new PersonDto
                       {
                           Id                 = p.Id,
                           Name               = p.Name,
                           Surname            = p.Surname,
                           CountryOfBirth     = p.CountryOfBirth.Name,
                           BirthDate          = p.BirthDate,
                           BirthDatePrecision = p.BirthDatePrecision,
                           DeathDate          = p.DeathDate,
                           DeathDatePrecision = p.DeathDatePrecision,
                           Webpage            = p.Webpage,
                           Twitter            = p.Twitter,
                           Facebook           = p.Facebook,
                           Photo              = p.Photo,
                           Alias              = p.Alias,
                           DisplayName        = p.DisplayName
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<PersonDto> GetAsync(int id) => context.People.Where(p => p.Id == id)
                                                      .Select(p => new PersonDto
                                                       {
                                                           Id               = p.Id,
                                                           Name             = p.Name,
                                                           Surname          = p.Surname,
                                                           CountryOfBirthId = p.CountryOfBirthId,
                                                           BirthDate        = p.BirthDate,
                                                           BirthDatePrecision = p.BirthDatePrecision,
                                                           DeathDate        = p.DeathDate,
                                                           DeathDatePrecision = p.DeathDatePrecision,
                                                           Webpage          = p.Webpage,
                                                           Twitter          = p.Twitter,
                                                           Facebook         = p.Facebook,
                                                           Photo            = p.Photo,
                                                           Alias            = p.Alias,
                                                           DisplayName      = p.DisplayName
                                                       })
                                                      .FirstOrDefaultAsync();

    /// <summary>
    /// Consolidated /people/{id}/full endpoint for the public view page. Returns
    /// the person head plus the language-aware biography (with English fallback
    /// collapsed into a single ordered query) plus all five child collections in
    /// one HTTP response, replacing the 7 sequential round-trips the page used
    /// to make.
    ///
    /// Each query runs on its own DbContext from the factory so they can fan out
    /// in parallel via Task.WhenAll. None of the children depend on values
    /// projected by the head, so we use the /book head-with-children pattern
    /// (all 7 queries fire in parallel) rather than head-first-then-children;
    /// this saves one full ~180 ms RTT in the common case.
    /// </summary>
    [HttpGet("{id:int}/full")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonFullDto>> GetFullAsync(int id, [FromQuery] string lang = "eng")
    {
        await using var headCtx        = await dbFactory.CreateDbContextAsync();
        await using var descriptionCtx = await dbFactory.CreateDbContextAsync();
        await using var companiesCtx   = await dbFactory.CreateDbContextAsync();
        await using var booksCtx       = await dbFactory.CreateDbContextAsync();
        await using var documentsCtx   = await dbFactory.CreateDbContextAsync();
        await using var magazinesCtx   = await dbFactory.CreateDbContextAsync();
        await using var softwareCtx    = await dbFactory.CreateDbContextAsync();

        // Head — same projection as GetAsync(int id) above. AsNoTracking because
        // this is read-only.
        Task<PersonDto> headTask = headCtx.People.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PersonDto
             {
                 Id                 = p.Id,
                 Name               = p.Name,
                 Surname            = p.Surname,
                 CountryOfBirthId   = p.CountryOfBirthId,
                 BirthDate          = p.BirthDate,
                 BirthDatePrecision = p.BirthDatePrecision,
                 DeathDate          = p.DeathDate,
                 DeathDatePrecision = p.DeathDatePrecision,
                 Webpage            = p.Webpage,
                 Twitter            = p.Twitter,
                 Facebook           = p.Facebook,
                 Photo              = p.Photo,
                 Alias              = p.Alias,
                 DisplayName        = p.DisplayName
             })
            .FirstOrDefaultAsync();

        // Description: collapse the original two-step lookup (try requested
        // lang, then English fallback) into a single ordered query. Matches
        // for the requested language sort first (key 0); English fallback is
        // key 1; FirstOrDefaultAsync returns the preferred row.
        var descriptionTask = descriptionCtx.PersonDescriptions.AsNoTracking()
            .Where(d => d.PersonId == id && (d.LanguageCode == lang || d.LanguageCode == "eng"))
            .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
            .Select(d => new { d.Html, d.Text, d.LanguageCode })
            .FirstOrDefaultAsync();

        // Mirrors GetCompaniesByPersonAsync. Sort runs in SQL using the underlying
        // entity columns (Company.Name, Position, Start) so it can be translated
        // to a server-side ORDER BY.
        Task<List<PersonByCompanyDto>> companiesTask = companiesCtx.PeopleByCompanies.AsNoTracking()
            .Where(p => p.PersonId == id)
            .OrderBy(p => p.Company.Name)
            .ThenBy(p => p.Position)
            .ThenBy(p => p.Start)
            .Select(p => new PersonByCompanyDto
             {
                 Id          = p.Id,
                 PersonId    = p.PersonId,
                 CompanyId   = p.CompanyId,
                 CompanyName = p.Company.Name,
                 Position    = p.Position,
                 Start       = p.Start,
                 End         = p.End,
                 Ongoing     = p.Ongoing,
                 Name        = p.Person.Name,
                 Surname     = p.Person.Surname,
                 Alias       = p.Person.Alias,
                 DisplayName = p.Person.DisplayName
             })
            .ToListAsync();

        // Mirrors GetBooksByPersonAsync.
        Task<List<PersonByBookDto>> booksTask = booksCtx.PeopleByBooks.AsNoTracking()
            .Where(p => p.PersonId == id)
            .OrderBy(p => p.Role.Name)
            .Select(p => new PersonByBookDto
             {
                 Id          = p.Id,
                 PersonId    = p.PersonId,
                 BookId      = p.BookId,
                 RoleId      = p.RoleId,
                 Role        = p.Role.Name,
                 BookTitle   = p.Book.Title,
                 Name        = p.Person.Name,
                 Surname     = p.Person.Surname,
                 Alias       = p.Person.Alias,
                 DisplayName = p.Person.DisplayName
             })
            .ToListAsync();

        // Mirrors GetDocumentsByPersonAsync.
        Task<List<PersonByDocumentDto>> documentsTask = documentsCtx.PeopleByDocuments.AsNoTracking()
            .Where(p => p.PersonId == id)
            .OrderBy(p => p.Role.Name)
            .Select(p => new PersonByDocumentDto
             {
                 Id            = p.Id,
                 PersonId      = p.PersonId,
                 DocumentId    = p.DocumentId,
                 RoleId        = p.RoleId,
                 Role          = p.Role.Name,
                 DocumentTitle = p.Document.Title,
                 Name          = p.Person.Name,
                 Surname       = p.Person.Surname,
                 Alias         = p.Person.Alias,
                 DisplayName   = p.Person.DisplayName
             })
            .ToListAsync();

        // Mirrors GetMagazinesByPersonAsync.
        Task<List<PersonByMagazineDto>> magazinesTask = magazinesCtx.PeopleByMagazines.AsNoTracking()
            .Where(p => p.PersonId == id)
            .OrderBy(p => p.Role.Name)
            .Select(p => new PersonByMagazineDto
             {
                 Id            = p.Id,
                 PersonId      = p.PersonId,
                 MagazineId    = p.MagazineId,
                 RoleId        = p.RoleId,
                 Role          = p.Role.Name,
                 MagazineTitle = p.Magazine.Magazine.Title,
                 Name          = p.Person.Name,
                 Surname       = p.Person.Surname,
                 Alias         = p.Person.Alias,
                 DisplayName   = p.Person.DisplayName
             })
            .ToListAsync();

        // Mirrors GetSoftwareByPersonAsync.
        Task<List<PersonBySoftwareDto>> softwareTask = softwareCtx.PeopleBySoftware.AsNoTracking()
            .Where(p => p.PersonId == id)
            .OrderBy(p => p.Software.Name)
            .ThenBy(p => p.DocumentRole != null ? p.DocumentRole.Name : p.Role)
            .Select(p => new PersonBySoftwareDto
             {
                 Id           = p.Id,
                 PersonId     = p.PersonId,
                 SoftwareId   = p.SoftwareId,
                 Role         = p.DocumentRole != null ? p.DocumentRole.Name : p.Role,
                 SoftwareName = p.Software.Name,
                 Name         = p.Person.Name,
                 Surname      = p.Person.Surname,
                 Alias        = p.Person.Alias,
                 DisplayName  = p.Person.DisplayName
             })
            .ToListAsync();

        await Task.WhenAll(headTask,
                           descriptionTask,
                           companiesTask,
                           booksTask,
                           documentsTask,
                           magazinesTask,
                           softwareTask);

        PersonDto person = headTask.Result;

        if(person is null) return NotFound();

        // Sort already runs in SQL inside each child task. Just pass through.
        List<PersonByCompanyDto>  companies = companiesTask.Result;
        List<PersonByBookDto>     books     = booksTask.Result;
        List<PersonByDocumentDto> documents = documentsTask.Result;
        List<PersonByMagazineDto> magazines = magazinesTask.Result;
        List<PersonBySoftwareDto> software  = softwareTask.Result;

        return new PersonFullDto
        {
            Person                  = person,
            DescriptionHtml         = descriptionTask.Result?.Html,
            DescriptionText         = descriptionTask.Result?.Text,
            DescriptionLanguageCode = descriptionTask.Result?.LanguageCode,
            Companies               = companies,
            Books                   = books,
            Documents               = documents,
            Magazines               = magazines,
            SoftwareCredits         = software
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] PersonDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Person model = await context.People.FindAsync(id);

        if(model is null) return NotFound();

        model.Name             = dto.Name;
        model.Surname          = dto.Surname;
        model.CountryOfBirthId = dto.CountryOfBirthId;
        model.BirthDate        = dto.BirthDate;
        model.BirthDatePrecision = dto.BirthDatePrecision;
        model.DeathDate        = dto.DeathDate;
        model.DeathDatePrecision = dto.DeathDatePrecision;
        model.Webpage          = dto.Webpage;
        model.Twitter          = dto.Twitter;
        model.Facebook         = dto.Facebook;
        model.Photo            = dto.Photo ?? Guid.Empty;
        model.Alias            = dto.Alias;
        model.DisplayName      = dto.DisplayName;

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedPersonInDb,
            Name    = dto.DisplayName ?? $"{dto.Name} {dto.Surname}".Trim()
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] PersonDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Person
        {
            Name             = dto.Name,
            Surname          = dto.Surname,
            CountryOfBirthId = dto.CountryOfBirthId,
            BirthDate        = dto.BirthDate,
            BirthDatePrecision = dto.BirthDatePrecision,
            DeathDate        = dto.DeathDate,
            DeathDatePrecision = dto.DeathDatePrecision,
            Webpage          = dto.Webpage,
            Twitter          = dto.Twitter,
            Facebook         = dto.Facebook,
            Photo            = dto.Photo ?? Guid.Empty,
            Alias            = dto.Alias,
            DisplayName      = dto.DisplayName
        };

        await context.People.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.NewPersonInDb,
            Name    = dto.DisplayName ?? $"{dto.Name} {dto.Surname}".Trim()
        });

        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Person item = await context.People.FindAsync(id);

        if(item is null) return NotFound();

        string entityName = item.DisplayName ?? item.Alias ?? $"{item.Name} {item.Surname}".Trim();

        context.People.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        // Mark any pending suggestions for this Person as Stale and notify the suggesting users.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.Person, id, entityName);

        // Cascade: also mark stale every per-language biography suggestion for this Person.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.PersonDescription, id, entityName);

        return Ok();
    }

    [HttpGet("{id:int}/description/text")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetDescriptionTextAsync(int id, [FromQuery] string lang = "eng")
    {
        // Collapse the original two-step lookup (try requested lang, then English
        // fallback) into a single ordered query. Descriptions matching the requested
        // language sort first (key 0); English fallback is key 1; FirstOrDefaultAsync
        // returns the preferred row in one round-trip.
        var description = await context.PersonDescriptions.AsNoTracking()
                                       .Where(d => d.PersonId == id &&
                                                   (d.LanguageCode == lang || d.LanguageCode == "eng"))
                                       .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
                                       .Select(d => new { d.Html, d.Text })
                                       .FirstOrDefaultAsync();

        return description?.Html ?? description?.Text;
    }

    [HttpGet("{id:int}/descriptions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonDescriptionDto>> GetDescriptionsAsync(int id) => context.PersonDescriptions.AsNoTracking()
       .Where(d => d.PersonId == id)
       .Select(d => new PersonDescriptionDto
        {
            Id           = d.Id,
            PersonId     = d.PersonId,
            Html         = d.Html,
            Markdown     = d.Text,
            LanguageCode = d.LanguageCode,
            Language     = d.Language.ReferenceName
        })
       .ToListAsync();

    [HttpGet("{id:int}/description")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<PersonDescriptionDto> GetDescriptionAsync(int id, [FromQuery] string lang = "eng")
    {
        // Collapse the original two-step lookup (try requested lang, then English
        // fallback) into a single ordered query. Descriptions matching the requested
        // language sort first (key 0); English fallback is key 1; FirstOrDefaultAsync
        // returns the preferred row in one round-trip.
        PersonDescriptionDto description = await context.PersonDescriptions.AsNoTracking()
                                                        .Where(d => d.PersonId == id &&
                                                                    (d.LanguageCode == lang ||
                                                                     d.LanguageCode == "eng"))
                                                        .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
                                                        .Select(d => new PersonDescriptionDto
                                                         {
                                                             Id           = d.Id,
                                                             PersonId     = d.PersonId,
                                                             Html         = d.Html,
                                                             Markdown     = d.Text,
                                                             LanguageCode = d.LanguageCode,
                                                             Language     = d.Language.ReferenceName
                                                         })
                                                        .FirstOrDefaultAsync();

        return description;
    }

    [HttpPost("{id:int}/description")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> CreateOrUpdateDescriptionAsync(
        int id, [FromBody] PersonDescriptionDto description)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        PersonDescription current = await context.PersonDescriptions
                                                 .FirstOrDefaultAsync(d => d.PersonId     == id &&
                                                                           d.LanguageCode == description.LanguageCode);

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        string           html     = Markdown.ToHtml(description.Markdown, pipeline);

        if(current is null)
        {
            current = new PersonDescription
            {
                PersonId     = id,
                LanguageCode = description.LanguageCode,
                Html         = html,
                Text         = description.Markdown
            };

            await context.PersonDescriptions.AddAsync(current);
        }
        else
        {
            current.Html = html;
            current.Text = description.Markdown;
        }

        await context.SaveChangesWithUserAsync(userId);

        return current.Id;
    }

    [HttpDelete("{id:int}/description/{languageCode}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteDescriptionAsync(int id, string languageCode)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        PersonDescription description = await context.PersonDescriptions
                                                     .FirstOrDefaultAsync(d => d.PersonId     == id &&
                                                                               d.LanguageCode == languageCode);

        if(description is null) return NotFound();

        // Capture display data BEFORE the cascade, while the Person + language rows are still
        // available for the system message body.
        string personName = await context.People.AsNoTracking()
                                        .Where(p => p.Id == id)
                                        .Select(p => p.DisplayName ?? (p.Name + " " + p.Surname))
                                        .FirstOrDefaultAsync();
        string langName = await context.Iso639.AsNoTracking()
                                       .Where(l => l.Id == languageCode)
                                       .Select(l => l.ReferenceName)
                                       .FirstOrDefaultAsync();
        string subkeyLabel = $"({langName ?? languageCode} biography)";

        context.PersonDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntitySubkeyAsync(
            context, Marechai.Data.SuggestionEntityType.PersonDescription,
            id, languageCode, personName ?? $"#{id}", subkeyLabel);

        return Ok();
    }

    // ───────────────────────────── Admin photo (direct upload/delete) ─────────────────────────────

    /// <summary>
    ///     Allowed file extensions for admin photo uploads. Broader than the collaborator
    ///     pending allow-set: admins may upload originals in any common image format —
    ///     the conversion worker transcodes to web-friendly variants. Mirrors
    ///     <c>BooksController._allowedExtensions</c>.
    /// </summary>
    static readonly HashSet<string> _allowedPhotoExtensions =
        [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedPhotoContentTypes =
        ["image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"];

    /// <summary>
    ///     Admin direct-upload of a person's photo. Replaces any existing photo: the old
    ///     variants are swept by <see cref="Marechai.Server.Suggestions.PersonSuggestionApplier.DeletePersonPhotoFiles"/>
    ///     before the new original is written, then the conversion worker fires in the
    ///     background. Mirrors <c>BooksController.UploadCoverAsync</c>.
    /// </summary>
    [HttpPost("{id:int}/photo/upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PersonDto>> UploadPhotoAsync(int id, IFormFile file)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(file is null || file.Length == 0)
            return BadRequest("No file provided.");

        if(file.Length > 50 * 1024 * 1024)
            return BadRequest("File exceeds 50 MB limit.");

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        if(!_allowedPhotoExtensions.Contains(extension))
            return BadRequest("Unsupported file format. Accepted: JPEG, PNG, WebP, TIFF, BMP.");

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_allowedPhotoContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Unsupported content type.");

        Person person = await context.People.FindAsync(id);

        if(person is null) return NotFound();

        // If photo already exists, delete the old variants first.
        if(person.Photo != Guid.Empty)
            Marechai.Server.Suggestions.PersonSuggestionApplier.DeletePersonPhotoFiles(_assetRootPath, person.Photo);

        Guid photoGuid = Guid.NewGuid();

        // Save original to disk.
        Marechai.Helpers.Photos.EnsureCreated(_assetRootPath, false, "people");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "people", "originals");
        string originalPath = Path.Combine(originalsDir, $"{photoGuid}{extension}");

        await using(var fs = new FileStream(originalPath, FileMode.CreateNew, FileAccess.Write))
        {
            await file.CopyToAsync(fs);
        }

        // Fire the conversion worker (generates all format/resolution variants).
        string sourceFormat = extension.TrimStart('.');

        _ = Task.Run(() =>
        {
            try
            {
                var photos = new Marechai.Helpers.Photos();
                photos.ConversionWorker(_assetRootPath, photoGuid, originalPath, sourceFormat, false, "people");
            }
            catch
            {
                // ignored — conversion can be retried later; original is safe in originals/.
            }
        });

        person.Photo                  = photoGuid;
        person.OriginalPhotoExtension = sourceFormat;
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new PersonDto
        {
            Id                 = person.Id,
            Name               = person.Name,
            Surname            = person.Surname,
            Alias              = person.Alias,
            DisplayName        = person.DisplayName,
            CountryOfBirthId   = person.CountryOfBirthId,
            BirthDate          = person.BirthDate,
            BirthDatePrecision = person.BirthDatePrecision,
            DeathDate          = person.DeathDate,
            DeathDatePrecision = person.DeathDatePrecision,
            Webpage            = person.Webpage,
            Twitter            = person.Twitter,
            Facebook           = person.Facebook,
            Photo              = person.Photo
        });
    }

    /// <summary>
    ///     Admin direct-delete of a person's photo. Sweeps every variant via
    ///     <see cref="Marechai.Server.Suggestions.PersonSuggestionApplier.DeletePersonPhotoFiles"/>
    ///     and clears the <c>Photo</c> + <c>OriginalPhotoExtension</c> fields.
    /// </summary>
    [HttpDelete("{id:int}/photo")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeletePhotoAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        Person person = await context.People.FindAsync(id);

        if(person is null) return NotFound();

        if(person.Photo == Guid.Empty) return NoContent();

        Marechai.Server.Suggestions.PersonSuggestionApplier.DeletePersonPhotoFiles(_assetRootPath, person.Photo);

        person.Photo                  = Guid.Empty;
        person.OriginalPhotoExtension = null;
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    /// <summary>
    ///     Upload a pending photo for a person that the caller is suggesting an edit on.
    ///     Accepts JPG/PNG/WebP up to 50 MB; the file is stored unchanged in
    ///     <c>people/pending/&lt;guid&gt;.&lt;ext&gt;</c> with a sidecar JSON file recording
    ///     the uploader. Auto-deletes any prior pending photos from the same uploader for
    ///     the same person so a user always has at most one pending photo per person in
    ///     flight. The returned <c>{guid, extension}</c> must be embedded in the
    ///     <c>cover_pending_guid</c> field of the subsequent suggestion submission.
    /// </summary>
    [HttpPost("{id:int}/photo/pending")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(PendingImageUploadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PendingImageUploadDto>> UploadPendingPhotoAsync(int id, IFormFile file)
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

        // Verify the targeted person exists; we don't want stray uploads for nonexistent ids.
        bool personExists = await context.People.AsNoTracking().AnyAsync(p => p.Id == id);
        if(!personExists) return NotFound();

        // Cleanup: each user gets at most ONE pending photo per person. Replace any prior
        // upload before storing the new one.
        Marechai.Server.Helpers.PendingImageStore.DeleteByUploaderForEntity(
            _assetRootPath, "people", userId, (byte)Marechai.Data.SuggestionEntityType.Person, id);

        await using var stream = file.OpenReadStream();
        Guid guid = await Marechai.Server.Helpers.PendingImageStore.StoreAsync(
            _assetRootPath, "people", extension,
            (byte)Marechai.Data.SuggestionEntityType.Person, id, userId,
            file.ContentType, stream);

        return Ok(new PendingImageUploadDto { Guid = guid, Extension = extension.TrimStart('.') });
    }

    /// <summary>
    ///     Upload a pending photo for a brand-new person that doesn't exist yet — paired
    ///     with the addition-mode <c>POST /suggestions</c> flow (entityType=Person,
    ///     entityId=null). Identical to <see cref="UploadPendingPhotoAsync" /> except it
    ///     skips the person-existence check and stores the file with <c>EntityId=0</c> in
    ///     the sidecar (since the actual person id only comes into existence when the
    ///     suggestion is accepted). The "one pending photo per (uploader, entity)" cleanup
    ///     keys on <c>(uploader, EntityType=Person, EntityId=0)</c>, so the user always
    ///     has at most one in-flight new-person photo at a time.
    /// </summary>
    [HttpPost("photo/pending/new")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(PendingImageUploadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PendingImageUploadDto>> UploadPendingNewPersonPhotoAsync(IFormFile file)
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

        // Cleanup: each user gets at most ONE pending NEW-person photo at a time. Replace
        // any prior upload (keyed on EntityId=0) before storing the new one.
        Marechai.Server.Helpers.PendingImageStore.DeleteByUploaderForEntity(
            _assetRootPath, "people", userId, (byte)Marechai.Data.SuggestionEntityType.Person, 0L);

        await using var stream = file.OpenReadStream();
        Guid guid = await Marechai.Server.Helpers.PendingImageStore.StoreAsync(
            _assetRootPath, "people", extension,
            (byte)Marechai.Data.SuggestionEntityType.Person, 0L, userId,
            file.ContentType, stream);

        return Ok(new PendingImageUploadDto { Guid = guid, Extension = extension.TrimStart('.') });
    }

    /// <summary>
    ///     Serve a pending photo image. Authorization: the uploader OR any admin/uberadmin
    ///     can view (so the dialog preview works for the contributor and the review queue
    ///     works for the moderator).
    /// </summary>
    [HttpGet("photo/pending/{guid:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPendingPhotoAsync(Guid guid)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        var meta = await Marechai.Server.Helpers.PendingImageStore.GetMetadataAsync(
            _assetRootPath, "people", guid);
        if(meta is null) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");
        if(!Marechai.Server.Helpers.PendingImageStore.CanAccess(meta, userId, isAdmin))
            return Forbid();

        string filePath = await Marechai.Server.Helpers.PendingImageStore.GetImagePathAsync(
            _assetRootPath, "people", guid);
        if(filePath is null) return NotFound();

        string contentType = !string.IsNullOrEmpty(meta.ContentType) ? meta.ContentType : "application/octet-stream";
        return PhysicalFile(filePath, contentType);
    }

    /// <summary>
    ///     Explicitly delete a pending photo (uploader OR admin). Useful for the
    ///     "remove photo before submit" UX in the dialog and for admin-side cleanup of
    ///     orphaned pending uploads.
    /// </summary>
    [HttpDelete("photo/pending/{guid:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePendingPhotoAsync(Guid guid)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        var meta = await Marechai.Server.Helpers.PendingImageStore.GetMetadataAsync(
            _assetRootPath, "people", guid);
        if(meta is null) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");
        if(!Marechai.Server.Helpers.PendingImageStore.CanAccess(meta, userId, isAdmin))
            return Forbid();

        Marechai.Server.Helpers.PendingImageStore.Delete(_assetRootPath, "people", guid);
        return NoContent();
    }
}