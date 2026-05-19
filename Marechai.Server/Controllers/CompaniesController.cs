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
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/companies")]
[ApiController]
public class CompaniesController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetAsync([FromQuery] int? skip   = null, [FromQuery] int? take = null,
                                           [FromQuery] string search = null,
                                           [FromQuery] string sortBy = null,
                                           [FromQuery] bool sortDescending = false)
    {
        IQueryable<Company> query = context.Companies.Include(c => c.Logos);

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || (c.LegalName != null && c.LegalName.Contains(search)));

        query = sortBy switch
        {
            "Name"      => sortDescending ? query.OrderByDescending(c => MarechaiContext.NaturalSortKey(c.Name))      : query.OrderBy(c => MarechaiContext.NaturalSortKey(c.Name)),
            "LegalName" => sortDescending ? query.OrderByDescending(c => MarechaiContext.NaturalSortKey(c.LegalName)) : query.OrderBy(c => MarechaiContext.NaturalSortKey(c.LegalName)),
            "Country"   => sortDescending ? query.OrderByDescending(c => MarechaiContext.NaturalSortKey(c.Country.Name)) : query.OrderBy(c => MarechaiContext.NaturalSortKey(c.Country.Name)),
            "Status"    => sortDescending ? query.OrderByDescending(c => c.Status)  : query.OrderBy(c => c.Status),
            "Founded"   => sortDescending ? query.OrderByDescending(c => c.Founded) : query.OrderBy(c => c.Founded),
            _           => query.OrderBy(c => MarechaiContext.NaturalSortKey(c.Name))
        };

        if(skip.HasValue) query = query.Skip(skip.Value);

        if(take.HasValue) query = query.Take(take.Value);

        return query.Select(c => new CompanyDto
                     {
                         Id = c.Id,
                         LastLogo =
                             c.Logos.OrderByDescending(l => l.Year)
                              .FirstOrDefault()
                              .Guid,
                         Name                  = c.Name,
                         Founded               = c.Founded,
                         Sold                  = c.Sold,
                         SoldToId              = c.SoldToId,
                         CountryId             = c.CountryId,
                         Status                = c.Status,
                         Website               = c.Website,
                         Twitter               = c.Twitter,
                         Facebook              = c.Facebook,
                         Address               = c.Address,
                         City                  = c.City,
                         Province              = c.Province,
                         PostalCode            = c.PostalCode,
                         Country               = c.Country.Name,
                         FoundedPrecision      = c.FoundedPrecision,
                         SoldPrecision         = c.SoldPrecision,
                         LegalName             = c.LegalName
                     })
                    .ToListAsync();
    }

    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<int> GetCountAsync([FromQuery] string search = null)
    {
        IQueryable<Company> query = context.Companies;

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || (c.LegalName != null && c.LegalName.Contains(search)));

        return query.CountAsync();
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<CompanyDto> GetAsync(int id) => context.Companies.Where(c => c.Id == id)
                                                       .Select(c => new CompanyDto
                                                        {
                                                            Id = c.Id,
                                                            LastLogo =
                                                                c.Logos.OrderByDescending(l => l.Year)
                                                                 .FirstOrDefault()
                                                                 .Guid,
                                                            Name                  = c.Name,
                                                            Founded               = c.Founded,
                                                            Sold                  = c.Sold,
                                                            SoldToId              = c.SoldToId,
                                                            CountryId             = c.CountryId,
                                                            Status                = c.Status,
                                                            Website               = c.Website,
                                                            Twitter               = c.Twitter,
                                                            Facebook              = c.Facebook,
                                                            Address               = c.Address,
                                                            City                  = c.City,
                                                            Province              = c.Province,
                                                            PostalCode            = c.PostalCode,
                                                            Country               = c.Country.Name,
                                                            FoundedPrecision      = c.FoundedPrecision,
                                                            SoldPrecision         = c.SoldPrecision,
                                                            LegalName             = c.LegalName
                                                        })
                                                       .FirstOrDefaultAsync();

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] CompanyDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Company model = await context.Companies.FindAsync(id);

        if(model is null) return NotFound();

        model.Name                  = dto.Name;
        model.Founded               = dto.Founded;
        model.Sold                  = dto.Sold;
        model.SoldToId              = dto.SoldToId;
        model.CountryId             = dto.CountryId;
        model.Status                = dto.Status;
        model.Website               = dto.Website;
        model.Twitter               = dto.Twitter;
        model.Facebook              = dto.Facebook;
        model.Address               = dto.Address;
        model.City                  = dto.City;
        model.Province              = dto.Province;
        model.PostalCode            = dto.PostalCode;
        model.FoundedPrecision      = dto.FoundedPrecision;
        model.SoldPrecision         = dto.SoldPrecision;
        model.LegalName             = dto.LegalName;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> CreateAsync([FromBody] CompanyDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Company
        {
            Name                  = dto.Name,
            Founded               = dto.Founded,
            Sold                  = dto.Sold,
            SoldToId              = dto.SoldToId,
            CountryId             = dto.CountryId,
            Status                = dto.Status,
            Website               = dto.Website,
            Twitter               = dto.Twitter,
            Facebook              = dto.Facebook,
            Address               = dto.Address,
            City                  = dto.City,
            Province              = dto.Province,
            PostalCode            = dto.PostalCode,
            FoundedPrecision      = dto.FoundedPrecision,
            SoldPrecision         = dto.SoldPrecision,
            LegalName             = dto.LegalName
        };

        await context.Companies.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpGet("{id:int}/machines")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetMachinesAsync(int id) => context.Machines.Where(m => m.CompanyId == id)
                                                                     .OrderBy(m => MarechaiContext.NaturalSortKey(m.Name))
                                                                     .Select(m => new MachineDto
                                                                      {
                                                                          Id   = m.Id,
                                                                          Name = m.Name,
                                                                          Type = m.Type
                                                                      })
                                                                     .ToListAsync();

    [HttpGet("{id:int}/gpus")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<GpuDto>> GetGpusAsync(int id) => context.Gpus.Where(g => g.CompanyId == id)
                                                              .OrderBy(g => MarechaiContext.NaturalSortKey(g.Name))
                                                              .Select(g => new GpuDto
                                                               {
                                                                   Id   = g.Id,
                                                                   Name = g.Name
                                                               })
                                                              .ToListAsync();

    [HttpGet("{id:int}/sound-synths")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoundSynthDto>> GetSoundSynthsAsync(int id) => context.SoundSynths
       .Where(s => s.CompanyId == id)
       .OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
       .Select(s => new SoundSynthDto
        {
            Id   = s.Id,
            Name = s.Name
        })
       .ToListAsync();

    [HttpGet("{id:int}/processors")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<ProcessorDto>> GetProcessorsAsync(int id) => context.Processors
       .Where(p => p.CompanyId == id)
       .OrderBy(p => MarechaiContext.NaturalSortKey(p.Name))
       .Select(p => new ProcessorDto
        {
            Id   = p.Id,
            Name = p.Name
        })
       .ToListAsync();

    [HttpGet("{id:int}/machine-families")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineFamilyDto>> GetMachineFamiliesAsync(int id) => context.MachineFamilies
       .Where(f => f.CompanyId == id)
       .OrderBy(f => MarechaiContext.NaturalSortKey(f.Name))
       .Select(f => new MachineFamilyDto
        {
            Id   = f.Id,
            Name = f.Name
        })
       .ToListAsync();

    [HttpGet("{id:int}/books")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<BookDto>> GetBooksAsync(int id) => context.CompaniesByBooks
       .Where(cb => cb.CompanyId == id)
       .Select(cb => cb.Book)
       .Distinct()
       .OrderBy(b => MarechaiContext.NaturalSortKey(b.Title))
       .Select(b => new BookDto
        {
            Id    = b.Id,
            Title = b.Title
        })
       .ToListAsync();

    [HttpGet("{id:int}/documents")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DocumentDto>> GetDocumentsAsync(int id) => context.CompaniesByDocuments
       .Where(cd => cd.CompanyId == id)
       .Select(cd => cd.Document)
       .Distinct()
       .OrderBy(d => MarechaiContext.NaturalSortKey(d.Title))
       .Select(d => new DocumentDto
        {
            Id    = d.Id,
            Title = d.Title
        })
       .ToListAsync();

    [HttpGet("{id:int}/magazines")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MagazineDto>> GetMagazinesAsync(int id) => context.CompaniesByMagazines
       .Where(cm => cm.CompanyId == id)
       .Select(cm => cm.Magazine)
       .Distinct()
       .OrderBy(m => MarechaiContext.NaturalSortKey(m.Title))
       .Select(m => new MagazineDto
        {
            Id    = m.Id,
            Title = m.Title
        })
       .ToListAsync();

    [HttpGet("{id:int}/software")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareAsync(int id) => context.SoftwareCompanyRoles
       .Where(sr => sr.CompanyId == id)
       .Select(sr => sr.Software)
       .Distinct()
       .OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
       .Select(s => new SoftwareDto
        {
            Id   = s.Id,
            Name = s.Name
        })
       .ToListAsync();

    [HttpGet("{id:int}/description/text")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetDescriptionTextAsync(int id, [FromQuery] string lang = "eng")
    {
        CompanyDescription description =
            await context.CompanyDescriptions.FirstOrDefaultAsync(d => d.CompanyId == id && d.LanguageCode == lang);

        // Fallback to English if requested language not found
        if(description is null && lang != "eng")
            description = await context.CompanyDescriptions.FirstOrDefaultAsync(d => d.CompanyId == id &&
                              d.LanguageCode == "eng");

        return description?.Html ?? description?.Text;
    }

    [HttpGet("{id:int}/soldto")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<CompanyDto> GetSoldToAsync(int? id) => context.Companies.Select(c => new CompanyDto
                                                               {
                                                                   Id   = c.Id,
                                                                   Name = c.Name
                                                               })
                                                              .FirstOrDefaultAsync(c => c.Id == id);

    [HttpGet("/countries/{id:int}/name")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetCountryNameAsync(int id) =>
        (await context.Iso31661Numeric.FirstOrDefaultAsync(c => c.Id == id))?.Name;

    [HttpGet("/countries/{id:int}/companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetCompaniesByCountryAsync(int id) => context.Companies.Include(c => c.Logos)
       .Where(c => c.CountryId == id)
       .OrderBy(c => MarechaiContext.NaturalSortKey(c.Name))
       .Select(c => new CompanyDto
        {
            Id       = c.Id,
            LastLogo = c.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
            Name     = c.Name
        })
       .ToListAsync();

    [HttpGet("/companies/letter/{id:char}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetCompaniesByLetterAsync(char id) => context.Companies.Include(c => c.Logos)
       .Where(c => EF.Functions.Like(c.Name, $"{id}%"))
       .OrderBy(c => MarechaiContext.NaturalSortKey(c.Name))
       .Select(c => new CompanyDto
        {
            Id       = c.Id,
            LastLogo = c.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
            Name     = c.Name
        })
       .ToListAsync();

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
        Company item = await context.Companies.FindAsync(id);

        if(item is null) return NotFound();

        string entityName = item.Name;

        context.Companies.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        // Mark any pending suggestions for this company as Stale and notify the suggesting users.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.Company, id, entityName);

        // Cascade: also mark stale every per-language description suggestion for this company.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.CompanyDescription, id, entityName);

        return Ok();
    }

    [HttpGet("{targetId:int}/merge-preview/{sourceId:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CompanyMergePreviewDto>> GetMergePreviewAsync(int targetId, int sourceId)
    {
        if(targetId == sourceId) return BadRequest("Cannot merge a company into itself.");

        Company target = await context.Companies.FindAsync(targetId);

        if(target is null) return NotFound("Target company not found.");

        Company source = await context.Companies.FindAsync(sourceId);

        if(source is null) return NotFound("Source company not found.");

        // Logos — direct re-parent, no dedup
        int logosCount = await context.CompanyLogos.CountAsync(l => l.CompanyId == sourceId);

        // Descriptions — dedup by LanguageCode
        List<string> sourceDescLangs = await context.CompanyDescriptions
                                                    .Where(d => d.CompanyId == sourceId)
                                                    .Select(d => d.LanguageCode)
                                                    .ToListAsync();

        HashSet<string> targetDescLangs = (await context.CompanyDescriptions
                                                        .Where(d => d.CompanyId == targetId)
                                                        .Select(d => d.LanguageCode)
                                                        .ToListAsync())
           .ToHashSet();

        int descriptionsDuplicates = sourceDescLangs.Count(l => targetDescLangs.Contains(l));

        // Single-value re-parent counts
        int gpusCount            = await context.Gpus.CountAsync(g => g.CompanyId == sourceId);
        int processorsCount      = await context.Processors.CountAsync(p => p.CompanyId == sourceId);
        int soundSynthsCount     = await context.SoundSynths.CountAsync(s => s.CompanyId == sourceId);
        int machinesCount        = await context.Machines.CountAsync(m => m.CompanyId == sourceId);
        int machineFamiliesCount = await context.MachineFamilies.CountAsync(f => f.CompanyId == sourceId);
        int softwareReleasesCount =
            await context.SoftwareReleases.CountAsync(r => r.PublisherId == sourceId);

        // SoftwareCompanyRoles — composite PK (SoftwareId, CompanyId, RoleId); dedup by (SoftwareId, RoleId)
        var sourceSoftwareRoles = await context.SoftwareCompanyRoles
                                               .Where(cr => cr.CompanyId == sourceId)
                                               .Select(cr => new { cr.SoftwareId, cr.RoleId })
                                               .ToListAsync();

        HashSet<(ulong, string)> targetSoftwareRoleKeys = (await context.SoftwareCompanyRoles
                                                                        .Where(cr => cr.CompanyId == targetId)
                                                                        .Select(cr => new { cr.SoftwareId, cr.RoleId })
                                                                        .ToListAsync())
           .Select(cr => (cr.SoftwareId, cr.RoleId))
           .ToHashSet();

        int softwareRolesDuplicates =
            sourceSoftwareRoles.Count(cr => targetSoftwareRoleKeys.Contains((cr.SoftwareId, cr.RoleId)));

        // PeopleByCompany — no dedup; Start/End make duplicate (PersonId, CompanyId) legitimate history
        int peopleCount = await context.PeopleByCompanies.CountAsync(p => p.CompanyId == sourceId);

        // CompaniesByBook — Id PK; dedup by (BookId, RoleId)
        var sourceBooks = await context.CompaniesByBooks
                                       .Where(b => b.CompanyId == sourceId)
                                       .Select(b => new { b.BookId, b.RoleId })
                                       .ToListAsync();

        HashSet<(long, string)> targetBookKeys = (await context.CompaniesByBooks
                                                               .Where(b => b.CompanyId == targetId)
                                                               .Select(b => new { b.BookId, b.RoleId })
                                                               .ToListAsync())
           .Select(b => (b.BookId, b.RoleId))
           .ToHashSet();

        int booksDuplicates = sourceBooks.Count(b => targetBookKeys.Contains((b.BookId, b.RoleId)));

        // CompaniesByDocument — dedup by (DocumentId, RoleId)
        var sourceDocuments = await context.CompaniesByDocuments
                                           .Where(d => d.CompanyId == sourceId)
                                           .Select(d => new { d.DocumentId, d.RoleId })
                                           .ToListAsync();

        HashSet<(long, string)> targetDocumentKeys = (await context.CompaniesByDocuments
                                                                    .Where(d => d.CompanyId == targetId)
                                                                    .Select(d => new { d.DocumentId, d.RoleId })
                                                                    .ToListAsync())
           .Select(d => (d.DocumentId, d.RoleId))
           .ToHashSet();

        int documentsDuplicates =
            sourceDocuments.Count(d => targetDocumentKeys.Contains((d.DocumentId, d.RoleId)));

        // CompaniesByMagazine — dedup by (MagazineId, RoleId)
        var sourceMagazines = await context.CompaniesByMagazines
                                           .Where(m => m.CompanyId == sourceId)
                                           .Select(m => new { m.MagazineId, m.RoleId })
                                           .ToListAsync();

        HashSet<(long, string)> targetMagazineKeys = (await context.CompaniesByMagazines
                                                                    .Where(m => m.CompanyId == targetId)
                                                                    .Select(m => new { m.MagazineId, m.RoleId })
                                                                    .ToListAsync())
           .Select(m => (m.MagazineId, m.RoleId))
           .ToHashSet();

        int magazinesDuplicates =
            sourceMagazines.Count(m => targetMagazineKeys.Contains((m.MagazineId, m.RoleId)));

        // CompanyBySoftwareVersion — dedup by (SoftwareVersionId, RoleId)
        var sourceSoftwareVersions = await context.CompaniesBySoftwareVersions
                                                  .Where(v => v.CompanyId == sourceId)
                                                  .Select(v => new { v.SoftwareVersionId, v.RoleId })
                                                  .ToListAsync();

        HashSet<(ulong, string)> targetSoftwareVersionKeys = (await context.CompaniesBySoftwareVersions
                                                                            .Where(v => v.CompanyId == targetId)
                                                                            .Select(v => new
                                                                                {
                                                                                    v.SoftwareVersionId,
                                                                                    v.RoleId
                                                                                })
                                                                            .ToListAsync())
           .Select(v => (v.SoftwareVersionId, v.RoleId))
           .ToHashSet();

        int softwareVersionsDuplicates =
            sourceSoftwareVersions.Count(v => targetSoftwareVersionKeys.Contains((v.SoftwareVersionId, v.RoleId)));

        // CompanyBySoftwareFamily — dedup by (SoftwareFamilyId, RoleId)
        var sourceSoftwareFamilies = await context.CompaniesBySoftwareFamilies
                                                  .Where(f => f.CompanyId == sourceId)
                                                  .Select(f => new { f.SoftwareFamilyId, f.RoleId })
                                                  .ToListAsync();

        HashSet<(ulong, string)> targetSoftwareFamilyKeys = (await context.CompaniesBySoftwareFamilies
                                                                           .Where(f => f.CompanyId == targetId)
                                                                           .Select(f => new
                                                                               {
                                                                                   f.SoftwareFamilyId,
                                                                                   f.RoleId
                                                                               })
                                                                           .ToListAsync())
           .Select(f => (f.SoftwareFamilyId, f.RoleId))
           .ToHashSet();

        int softwareFamiliesDuplicates =
            sourceSoftwareFamilies.Count(f => targetSoftwareFamilyKeys.Contains((f.SoftwareFamilyId, f.RoleId)));

        // InverseSoldTo — other companies that have SoldToId == sourceId (need re-pointing, exclude source itself)
        int inverseSoldToCount = await context.Companies
                                              .CountAsync(c => c.SoldToId == sourceId && c.Id != sourceId);

        // SearchEntries — denormalized search cache. Includes the source's own entry plus any
        // entry that references source via the CompanyId column.
        int searchEntryCount = await context.SearchEntries
                                            .CountAsync(s => (s.EntityType == Marechai.Data.SearchEntityType.Company &&
                                                              s.EntityId == sourceId) ||
                                                             s.CompanyId == sourceId);

        return new CompanyMergePreviewDto
        {
            TargetId                   = targetId,
            SourceId                   = sourceId,
            TargetName                 = target.Name,
            SourceName                 = source.Name,
            LogosCount                 = logosCount,
            DescriptionsTotal          = sourceDescLangs.Count,
            DescriptionsDuplicates     = descriptionsDuplicates,
            GpusCount                  = gpusCount,
            ProcessorsCount            = processorsCount,
            SoundSynthsCount           = soundSynthsCount,
            MachinesCount              = machinesCount,
            MachineFamiliesCount       = machineFamiliesCount,
            SoftwareReleasesCount      = softwareReleasesCount,
            SoftwareRolesTotal         = sourceSoftwareRoles.Count,
            SoftwareRolesDuplicates    = softwareRolesDuplicates,
            PeopleCount                = peopleCount,
            BooksTotal                 = sourceBooks.Count,
            BooksDuplicates            = booksDuplicates,
            DocumentsTotal             = sourceDocuments.Count,
            DocumentsDuplicates        = documentsDuplicates,
            MagazinesTotal             = sourceMagazines.Count,
            MagazinesDuplicates        = magazinesDuplicates,
            SoftwareVersionsTotal      = sourceSoftwareVersions.Count,
            SoftwareVersionsDuplicates = softwareVersionsDuplicates,
            SoftwareFamiliesTotal      = sourceSoftwareFamilies.Count,
            SoftwareFamiliesDuplicates = softwareFamiliesDuplicates,
            InverseSoldToCount         = inverseSoldToCount,
            SearchEntryCompanyRefCount = searchEntryCount
        };
    }

    [HttpPost("{targetId:int}/merge/{sourceId:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> MergeAsync(int targetId, int sourceId,
                                               [FromBody] CompanyMergeRequestDto request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(request is null) return BadRequest("Merge request body is required.");

        if(string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required.");

        if(targetId == sourceId) return BadRequest("Cannot merge a company into itself.");

        Company target = await context.Companies.FindAsync(targetId);

        if(target is null) return NotFound("Target company not found.");

        Company source = await context.Companies.FindAsync(sourceId);

        if(source is null) return NotFound("Source company not found.");

        string sourceName = source.Name;

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 1. Apply chosen scalar values to the surviving target row.
            //    Self-loop guard: never let SoldToId point at the target itself or the about-to-be-deleted source.
            int? chosenSoldToId = request.SoldToId;
            if(chosenSoldToId == targetId || chosenSoldToId == sourceId) chosenSoldToId = null;

            target.Name             = request.Name;
            target.LegalName        = request.LegalName;
            target.Status           = request.Status;
            target.Founded          = request.Founded;
            target.FoundedPrecision = request.FoundedPrecision;
            target.Sold             = request.Sold;
            target.SoldPrecision    = request.SoldPrecision;
            target.SoldToId         = chosenSoldToId;
            target.CountryId        = request.CountryId;
            target.Address          = request.Address;
            target.City             = request.City;
            target.Province         = request.Province;
            target.PostalCode       = request.PostalCode;
            target.Website          = request.Website;
            target.Twitter          = request.Twitter;
            target.Facebook         = request.Facebook;

            // 2. Re-parent direct one-to-many FKs (no dedup needed).
            List<CompanyLogo> sourceLogos =
                await context.CompanyLogos.Where(l => l.CompanyId == sourceId).ToListAsync();

            foreach(CompanyLogo logo in sourceLogos) logo.CompanyId = targetId;

            List<Gpu> sourceGpus = await context.Gpus.Where(g => g.CompanyId == sourceId).ToListAsync();

            foreach(Gpu gpu in sourceGpus) gpu.CompanyId = targetId;

            List<Processor> sourceProcessors =
                await context.Processors.Where(p => p.CompanyId == sourceId).ToListAsync();

            foreach(Processor processor in sourceProcessors) processor.CompanyId = targetId;

            List<SoundSynth> sourceSoundSynths =
                await context.SoundSynths.Where(s => s.CompanyId == sourceId).ToListAsync();

            foreach(SoundSynth synth in sourceSoundSynths) synth.CompanyId = targetId;

            List<Machine> sourceMachines =
                await context.Machines.Where(m => m.CompanyId == sourceId).ToListAsync();

            foreach(Machine machine in sourceMachines) machine.CompanyId = targetId;

            List<MachineFamily> sourceMachineFamilies =
                await context.MachineFamilies.Where(f => f.CompanyId == sourceId).ToListAsync();

            foreach(MachineFamily family in sourceMachineFamilies) family.CompanyId = targetId;

            List<SoftwareRelease> sourceReleases =
                await context.SoftwareReleases.Where(r => r.PublisherId == sourceId).ToListAsync();

            foreach(SoftwareRelease release in sourceReleases) release.PublisherId = targetId;

            // 3. InverseSoldTo: re-point other companies that referenced source.
            List<Company> inverseSoldTo = await context.Companies
                                                       .Where(c => c.SoldToId == sourceId && c.Id != sourceId &&
                                                                   c.Id != targetId)
                                                       .ToListAsync();

            foreach(Company c in inverseSoldTo) c.SoldToId = targetId;

            // Clear any target row that pointed at source (would dangle on delete).
            if(target.SoldToId == sourceId) target.SoldToId = chosenSoldToId == sourceId ? null : chosenSoldToId;

            // 4. CompanyDescriptions — unique on (CompanyId, LanguageCode).
            List<CompanyDescription> sourceDescriptions =
                await context.CompanyDescriptions.Where(d => d.CompanyId == sourceId).ToListAsync();

            HashSet<string> targetDescriptionLangs = (await context.CompanyDescriptions
                                                                    .Where(d => d.CompanyId == targetId)
                                                                    .Select(d => d.LanguageCode)
                                                                    .ToListAsync())
               .ToHashSet();

            foreach(CompanyDescription desc in sourceDescriptions)
            {
                if(targetDescriptionLangs.Contains(desc.LanguageCode))
                    context.CompanyDescriptions.Remove(desc);
                else
                {
                    desc.CompanyId = targetId;
                    targetDescriptionLangs.Add(desc.LanguageCode);
                }
            }

            // 5. SoftwareCompanyRole — composite PK (SoftwareId, CompanyId, RoleId); dedup by (SoftwareId, RoleId).
            List<SoftwareCompanyRole> sourceSoftwareRoles =
                await context.SoftwareCompanyRoles.Where(cr => cr.CompanyId == sourceId).ToListAsync();

            HashSet<(ulong, string)> targetSoftwareRoleKeys = (await context.SoftwareCompanyRoles
                                                                            .Where(cr => cr.CompanyId == targetId)
                                                                            .Select(cr => new
                                                                                {
                                                                                    cr.SoftwareId,
                                                                                    cr.RoleId
                                                                                })
                                                                            .ToListAsync())
               .Select(cr => (cr.SoftwareId, cr.RoleId))
               .ToHashSet();

            foreach(SoftwareCompanyRole cr in sourceSoftwareRoles)
            {
                context.SoftwareCompanyRoles.Remove(cr);

                if(!targetSoftwareRoleKeys.Contains((cr.SoftwareId, cr.RoleId)))
                {
                    context.SoftwareCompanyRoles.Add(new SoftwareCompanyRole
                    {
                        SoftwareId = cr.SoftwareId,
                        CompanyId  = targetId,
                        RoleId     = cr.RoleId
                    });

                    targetSoftwareRoleKeys.Add((cr.SoftwareId, cr.RoleId));
                }
            }

            // 6. CompaniesByBook — Id PK; dedup by (BookId, RoleId) — remove duplicate row, repoint survivor.
            List<CompaniesByBook> sourceBooks =
                await context.CompaniesByBooks.Where(b => b.CompanyId == sourceId).ToListAsync();

            HashSet<(long, string)> targetBookKeys = (await context.CompaniesByBooks
                                                                    .Where(b => b.CompanyId == targetId)
                                                                    .Select(b => new { b.BookId, b.RoleId })
                                                                    .ToListAsync())
               .Select(b => (b.BookId, b.RoleId))
               .ToHashSet();

            foreach(CompaniesByBook book in sourceBooks)
            {
                if(targetBookKeys.Contains((book.BookId, book.RoleId)))
                    context.CompaniesByBooks.Remove(book);
                else
                {
                    book.CompanyId = targetId;
                    targetBookKeys.Add((book.BookId, book.RoleId));
                }
            }

            // 7. CompaniesByDocument — dedup by (DocumentId, RoleId).
            List<CompaniesByDocument> sourceDocuments =
                await context.CompaniesByDocuments.Where(d => d.CompanyId == sourceId).ToListAsync();

            HashSet<(long, string)> targetDocumentKeys = (await context.CompaniesByDocuments
                                                                        .Where(d => d.CompanyId == targetId)
                                                                        .Select(d => new
                                                                            {
                                                                                d.DocumentId,
                                                                                d.RoleId
                                                                            })
                                                                        .ToListAsync())
               .Select(d => (d.DocumentId, d.RoleId))
               .ToHashSet();

            foreach(CompaniesByDocument doc in sourceDocuments)
            {
                if(targetDocumentKeys.Contains((doc.DocumentId, doc.RoleId)))
                    context.CompaniesByDocuments.Remove(doc);
                else
                {
                    doc.CompanyId = targetId;
                    targetDocumentKeys.Add((doc.DocumentId, doc.RoleId));
                }
            }

            // 8. CompaniesByMagazine — dedup by (MagazineId, RoleId).
            List<CompaniesByMagazine> sourceMagazines =
                await context.CompaniesByMagazines.Where(m => m.CompanyId == sourceId).ToListAsync();

            HashSet<(long, string)> targetMagazineKeys = (await context.CompaniesByMagazines
                                                                        .Where(m => m.CompanyId == targetId)
                                                                        .Select(m => new
                                                                            {
                                                                                m.MagazineId,
                                                                                m.RoleId
                                                                            })
                                                                        .ToListAsync())
               .Select(m => (m.MagazineId, m.RoleId))
               .ToHashSet();

            foreach(CompaniesByMagazine mag in sourceMagazines)
            {
                if(targetMagazineKeys.Contains((mag.MagazineId, mag.RoleId)))
                    context.CompaniesByMagazines.Remove(mag);
                else
                {
                    mag.CompanyId = targetId;
                    targetMagazineKeys.Add((mag.MagazineId, mag.RoleId));
                }
            }

            // 9. CompanyBySoftwareVersion — dedup by (SoftwareVersionId, RoleId).
            List<CompanyBySoftwareVersion> sourceSoftwareVersions =
                await context.CompaniesBySoftwareVersions.Where(v => v.CompanyId == sourceId).ToListAsync();

            HashSet<(ulong, string)> targetSoftwareVersionKeys =
                (await context.CompaniesBySoftwareVersions
                              .Where(v => v.CompanyId == targetId)
                              .Select(v => new { v.SoftwareVersionId, v.RoleId })
                              .ToListAsync())
               .Select(v => (v.SoftwareVersionId, v.RoleId))
               .ToHashSet();

            foreach(CompanyBySoftwareVersion ver in sourceSoftwareVersions)
            {
                if(targetSoftwareVersionKeys.Contains((ver.SoftwareVersionId, ver.RoleId)))
                    context.CompaniesBySoftwareVersions.Remove(ver);
                else
                {
                    ver.CompanyId = targetId;
                    targetSoftwareVersionKeys.Add((ver.SoftwareVersionId, ver.RoleId));
                }
            }

            // 10. CompanyBySoftwareFamily — dedup by (SoftwareFamilyId, RoleId).
            List<CompanyBySoftwareFamily> sourceSoftwareFamilies =
                await context.CompaniesBySoftwareFamilies.Where(f => f.CompanyId == sourceId).ToListAsync();

            HashSet<(ulong, string)> targetSoftwareFamilyKeys =
                (await context.CompaniesBySoftwareFamilies
                              .Where(f => f.CompanyId == targetId)
                              .Select(f => new { f.SoftwareFamilyId, f.RoleId })
                              .ToListAsync())
               .Select(f => (f.SoftwareFamilyId, f.RoleId))
               .ToHashSet();

            foreach(CompanyBySoftwareFamily fam in sourceSoftwareFamilies)
            {
                if(targetSoftwareFamilyKeys.Contains((fam.SoftwareFamilyId, fam.RoleId)))
                    context.CompaniesBySoftwareFamilies.Remove(fam);
                else
                {
                    fam.CompanyId = targetId;
                    targetSoftwareFamilyKeys.Add((fam.SoftwareFamilyId, fam.RoleId));
                }
            }

            // 11. PeopleByCompany — bulk re-parent, no dedup (Start/End make duplicates legitimate).
            List<PeopleByCompany> sourcePeople =
                await context.PeopleByCompanies.Where(p => p.CompanyId == sourceId).ToListAsync();

            foreach(PeopleByCompany person in sourcePeople) person.CompanyId = targetId;

            // 12. SearchEntries: delete the source's own entry; redirect every other CompanyId reference.
            List<SearchEntry> sourceOwnSearchEntries =
                await context.SearchEntries
                             .Where(s => s.EntityType == Marechai.Data.SearchEntityType.Company &&
                                         s.EntityId == sourceId)
                             .ToListAsync();

            foreach(SearchEntry entry in sourceOwnSearchEntries) context.SearchEntries.Remove(entry);

            List<SearchEntry> searchEntriesReferencingSource =
                await context.SearchEntries.Where(s => s.CompanyId == sourceId).ToListAsync();

            foreach(SearchEntry entry in searchEntriesReferencingSource) entry.CompanyId = targetId;

            // 13. Flush every FK change before removing the source so cascade-delete has nothing left to do.
            await context.SaveChangesWithUserAsync(userId);

            context.Companies.Remove(source);
            await context.SaveChangesWithUserAsync(userId);

            await transaction.CommitAsync();
        }
        catch(Exception ex)
        {
            await transaction.RollbackAsync();

            return Conflict($"Merge failed: {ex.Message}");
        }

        // Mark pending suggestions for the deleted source company as Stale (mirrors DeleteAsync).
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.Company, sourceId, sourceName);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.CompanyDescription, sourceId, sourceName);

        return Ok();
    }

    [HttpGet("{id:int}/descriptions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDescriptionDto>> GetDescriptionsAsync(int id) => context.CompanyDescriptions
       .Where(d => d.CompanyId == id)
       .Select(d => new CompanyDescriptionDto
        {
            Id           = d.Id,
            CompanyId    = d.CompanyId,
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
    public async Task<CompanyDescriptionDto> GetDescriptionAsync(int id, [FromQuery] string lang = "eng")
    {
        CompanyDescriptionDto description = await context.CompanyDescriptions
                                                         .Where(d => d.CompanyId == id && d.LanguageCode == lang)
                                                         .Select(d => new CompanyDescriptionDto
                                                          {
                                                              Id           = d.Id,
                                                              CompanyId    = d.CompanyId,
                                                              Html         = d.Html,
                                                              Markdown     = d.Text,
                                                              LanguageCode = d.LanguageCode,
                                                              Language     = d.Language.ReferenceName
                                                          })
                                                         .FirstOrDefaultAsync();

        // Fallback to English if requested language not found
        if(description is null && lang != "eng")
            description = await context.CompanyDescriptions
                                       .Where(d => d.CompanyId == id && d.LanguageCode == "eng")
                                       .Select(d => new CompanyDescriptionDto
                                        {
                                            Id           = d.Id,
                                            CompanyId    = d.CompanyId,
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
        int id, [FromBody] CompanyDescriptionDto description)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        CompanyDescription current = await context.CompanyDescriptions
                                                  .FirstOrDefaultAsync(d => d.CompanyId    == id &&
                                                                            d.LanguageCode == description.LanguageCode);

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        string             html     = Markdown.ToHtml(description.Markdown, pipeline);

        if(current is null)
        {
            current = new CompanyDescription
            {
                CompanyId    = id,
                LanguageCode = description.LanguageCode,
                Html         = html,
                Text         = description.Markdown
            };

            await context.CompanyDescriptions.AddAsync(current);
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

        CompanyDescription description = await context.CompanyDescriptions
                                                      .FirstOrDefaultAsync(d => d.CompanyId    == id &&
                                                                                d.LanguageCode == languageCode);

        if(description is null) return NotFound();

        // Capture display data BEFORE the cascade, while the company + language rows are still
        // available for the system message body.
        string companyName  = await context.Companies.AsNoTracking()
                                           .Where(c => c.Id == id)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
        string langName     = await context.Iso639.AsNoTracking()
                                           .Where(l => l.Id == languageCode)
                                           .Select(l => l.ReferenceName)
                                           .FirstOrDefaultAsync();
        string subkeyLabel  = $"({langName ?? languageCode} description)";

        context.CompanyDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntitySubkeyAsync(
            context, Marechai.Data.SuggestionEntityType.CompanyDescription,
            id, languageCode, companyName ?? $"#{id}", subkeyLabel);

        return Ok();
    }
}