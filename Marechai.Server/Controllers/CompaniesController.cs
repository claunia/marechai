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
                                           [FromQuery] string search = null)
    {
        IQueryable<Company> query = context.Companies.Include(c => c.Logos);

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || (c.LegalName != null && c.LegalName.Contains(search)));

        query = query.OrderBy(c => c.Name);

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
                         FoundedDayIsUnknown   = c.FoundedDayIsUnknown,
                         FoundedMonthIsUnknown = c.FoundedMonthIsUnknown,
                         SoldDayIsUnknown      = c.SoldDayIsUnknown,
                         SoldMonthIsUnknown    = c.SoldMonthIsUnknown,
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
                                                            FoundedDayIsUnknown   = c.FoundedDayIsUnknown,
                                                            FoundedMonthIsUnknown = c.FoundedMonthIsUnknown,
                                                            SoldDayIsUnknown      = c.SoldDayIsUnknown,
                                                            SoldMonthIsUnknown    = c.SoldMonthIsUnknown,
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
        model.FoundedDayIsUnknown   = dto.FoundedDayIsUnknown;
        model.FoundedMonthIsUnknown = dto.FoundedMonthIsUnknown;
        model.SoldDayIsUnknown      = dto.SoldDayIsUnknown;
        model.SoldMonthIsUnknown    = dto.SoldMonthIsUnknown;
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
            FoundedDayIsUnknown   = dto.FoundedDayIsUnknown,
            FoundedMonthIsUnknown = dto.FoundedMonthIsUnknown,
            SoldDayIsUnknown      = dto.SoldDayIsUnknown,
            SoldMonthIsUnknown    = dto.SoldMonthIsUnknown,
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
                                                                     .OrderBy(m => m.Name)
                                                                     .Select(m => new MachineDto
                                                                      {
                                                                          Id   = m.Id,
                                                                          Name = m.Name,
                                                                          Type = m.Type
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
       .OrderBy(c => c.Name)
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
       .OrderBy(c => c.Name)
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

        context.Companies.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

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

        if(current is null)
        {
            current = new CompanyDescription
            {
                CompanyId    = id,
                LanguageCode = description.LanguageCode,
                Html         = description.Html,
                Text         = description.Markdown
            };

            await context.CompanyDescriptions.AddAsync(current);
        }
        else
        {
            current.Html = description.Html;
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

        context.CompanyDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}