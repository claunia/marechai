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

[Route("/companies")]
[ApiController]
public class CompaniesController(MarechaiContext context, IStringLocalizer<CompaniesService> localizer) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<CompanyDto>> GetAsync() => await context.Companies.Include(c => c.Logos)
                                                                          .OrderBy(c => c.Name)
                                                                          .Select(c => new CompanyDto
                                                                           {
                                                                               Id = c.Id,
                                                                               LastLogo =
                                                                                   c.Logos
                                                                                    .OrderByDescending(l => l.Year)
                                                                                    .FirstOrDefault()
                                                                                    .Guid,
                                                                               Name       = c.Name,
                                                                               Founded    = c.Founded,
                                                                               Sold       = c.Sold,
                                                                               SoldToId   = c.SoldToId,
                                                                               CountryId  = c.CountryId,
                                                                               Status     = c.Status,
                                                                               Website    = c.Website,
                                                                               Twitter    = c.Twitter,
                                                                               Facebook   = c.Facebook,
                                                                               Address    = c.Address,
                                                                               City       = c.City,
                                                                               Province   = c.Province,
                                                                               PostalCode = c.PostalCode,
                                                                               Country    = c.Country.Name,
                                                                               FoundedDayIsUnknown =
                                                                                   c.FoundedDayIsUnknown,
                                                                               FoundedMonthIsUnknown =
                                                                                   c.FoundedMonthIsUnknown,
                                                                               SoldDayIsUnknown =
                                                                                   c.SoldDayIsUnknown,
                                                                               SoldMonthIsUnknown =
                                                                                   c.SoldMonthIsUnknown,
                                                                               LegalName = c.LegalName
                                                                           })
                                                                          .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<CompanyDto> GetAsync(int id) => await context.Companies.Where(c => c.Id == id)
                                                                          .Select(c => new CompanyDto
                                                                           {
                                                                               Id = c.Id,
                                                                               LastLogo =
                                                                                   c.Logos
                                                                                      .OrderByDescending(l => l.Year)
                                                                                      .FirstOrDefault()
                                                                                      .Guid,
                                                                               Name       = c.Name,
                                                                               Founded    = c.Founded,
                                                                               Sold       = c.Sold,
                                                                               SoldToId   = c.SoldToId,
                                                                               CountryId  = c.CountryId,
                                                                               Status     = c.Status,
                                                                               Website    = c.Website,
                                                                               Twitter    = c.Twitter,
                                                                               Facebook   = c.Facebook,
                                                                               Address    = c.Address,
                                                                               City       = c.City,
                                                                               Province   = c.Province,
                                                                               PostalCode = c.PostalCode,
                                                                               Country    = c.Country.Name,
                                                                               FoundedDayIsUnknown =
                                                                                   c.FoundedDayIsUnknown,
                                                                               FoundedMonthIsUnknown =
                                                                                   c.FoundedMonthIsUnknown,
                                                                               SoldDayIsUnknown =
                                                                                   c.SoldDayIsUnknown,
                                                                               SoldMonthIsUnknown =
                                                                                   c.SoldMonthIsUnknown,
                                                                               LegalName = c.LegalName
                                                                           })
                                                                          .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task UpdateAsync(CompanyDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        Company model = await context.Companies.FindAsync(dto.Id);

        if(model is null) return;

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
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> CreateAsync(CompanyDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return 0;
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

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<Machine>> GetMachinesAsync(int id) => await context.Machines.Where(m => m.CompanyId == id)
                                                                    .OrderBy(m => m.Name)
                                                                    .Select(m => new Machine
                                                                     {
                                                                         Id   = m.Id,
                                                                         Name = m.Name,
                                                                         Type = m.Type
                                                                     })
                                                                    .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetDescriptionTextAsync(int id)
    {
        CompanyDescription description = await context.CompanyDescriptions.FirstOrDefaultAsync(d => d.CompanyId == id);

        return description?.Html ?? description?.Text;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Company> GetSoldToAsync(int? id) => await context.Companies.Select(c => new Company
                                                                         {
                                                                             Id   = c.Id,
                                                                             Name = c.Name
                                                                         })
                                                                        .FirstOrDefaultAsync(c => c.Id == id);

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetCountryNameAsync(int id) =>
        (await context.Iso31661Numeric.FirstOrDefaultAsync(c => c.Id == id))?.Name;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetCompaniesByCountryAsync(int countryId) => context.Companies
       .Include(c => c.Logos)
       .Where(c => c.CountryId == countryId)
       .OrderBy(c => c.Name)
       .Select(c => new CompanyDto
        {
            Id       = c.Id,
            LastLogo = c.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
            Name     = c.Name
        })
       .ToListAsync();

    [HttpGet]
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

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        Company item = await context.Companies.FindAsync(id);

        if(item is null) return;

        context.Companies.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<CompanyDescriptionDto> GetDescriptionAsync(int id) => await context.CompanyDescriptions
       .Where(d => d.CompanyId == id)
       .Select(d => new CompanyDescriptionDto
        {
            Id        = d.Id,
            CompanyId = d.CompanyId,
            Html      = d.Html,
            Markdown  = d.Text
        })
       .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> CreateOrUpdateDescriptionAsync(int    id, CompanyDescriptionDto description)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return 0;
        CompanyDescription current = await context.CompanyDescriptions.FirstOrDefaultAsync(d => d.CompanyId == id);

        if(current is null)
        {
            current = new CompanyDescription
            {
                CompanyId = id,
                Html      = description.Html,
                Text      = description.Markdown
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
}
