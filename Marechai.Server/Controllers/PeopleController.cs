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
using Markdig;

namespace Marechai.Server.Controllers;

[Route("/people")]
[ApiController]
public class PeopleController(
    MarechaiContext                    context,
    IDbContextFactory<MarechaiContext> dbFactory) : ControllerBase
{
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
    public async Task<List<PersonByBookDto>> GetBooksByPersonAsync(int personId) =>
        (await context.PeopleByBooks
                      .Where(p => p.PersonId == personId)
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
                      .ToListAsync()).OrderBy(p => p.Role)
           .ToList();

    [HttpGet("{personId:int}/documents")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<PersonByDocumentDto>> GetDocumentsByPersonAsync(int personId) =>
        (await context.PeopleByDocuments
                      .Where(p => p.PersonId == personId)
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
                      .ToListAsync()).OrderBy(p => p.Role)
           .ToList();

    [HttpGet("{personId:int}/magazines")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<PersonByMagazineDto>> GetMagazinesByPersonAsync(int personId) =>
        (await context.PeopleByMagazines
                      .Where(p => p.PersonId == personId)
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
                      .ToListAsync()).OrderBy(p => p.Role)
           .ToList();

    [HttpGet("{personId:int}/software")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<PersonBySoftwareDto>> GetSoftwareByPersonAsync(int personId) =>
        (await context.PeopleBySoftware
                      .Where(p => p.PersonId == personId)
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
                      .ToListAsync()).OrderBy(p => p.SoftwareName)
           .ThenBy(p => p.Role)
           .ToList();

    [HttpGet("{personId:int}/companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<PersonByCompanyDto>> GetCompaniesByPersonAsync(int personId) =>
        (await context.PeopleByCompanies
                      .Where(p => p.PersonId == personId)
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
                      .ToListAsync()).OrderBy(p => p.CompanyName)
           .ThenBy(p => p.Position)
           .ThenBy(p => p.Start)
           .ToList();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<PersonDto>> GetAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                          CancellationToken cancellationToken = default)
    {
        IQueryable<Person> ordered = context.People
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

        // Mirrors GetCompaniesByPersonAsync. The original sorts in memory because
        // PersonByCompanyDto.FullName etc. are computed properties; we keep the
        // same in-memory sort after Task.WhenAll completes.
        Task<List<PersonByCompanyDto>> companiesTask = companiesCtx.PeopleByCompanies.AsNoTracking()
            .Where(p => p.PersonId == id)
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

        // Same in-memory sort orders as the legacy individual endpoints, so the
        // page renders rows in the same sequence as before.
        List<PersonByCompanyDto> companies = companiesTask.Result.OrderBy(p => p.CompanyName)
                                                          .ThenBy(p => p.Position)
                                                          .ThenBy(p => p.Start)
                                                          .ToList();
        List<PersonByBookDto>      books      = booksTask.Result.OrderBy(p => p.Role).ToList();
        List<PersonByDocumentDto>  documents  = documentsTask.Result.OrderBy(p => p.Role).ToList();
        List<PersonByMagazineDto>  magazines  = magazinesTask.Result.OrderBy(p => p.Role).ToList();
        List<PersonBySoftwareDto>  software   = softwareTask.Result.OrderBy(p => p.SoftwareName)
                                                            .ThenBy(p => p.Role)
                                                            .ToList();

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

        context.People.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

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

        context.PersonDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}