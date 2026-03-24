/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
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
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Marechai.Services;

public class CompaniesService(MarechaiContext context, IStringLocalizer<CompaniesService> localizer)
{
    readonly IStringLocalizer<CompaniesService> _l       = localizer;

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

    public async Task<List<Machine>> GetMachinesAsync(int id) => await context.Machines.Where(m => m.CompanyId == id)
                                                                    .OrderBy(m => m.Name)
                                                                    .Select(m => new Machine
                                                                     {
                                                                         Id   = m.Id,
                                                                         Name = m.Name,
                                                                         Type = m.Type
                                                                     })
                                                                    .ToListAsync();

    public async Task<string> GetDescriptionTextAsync(int id)
    {
        CompanyDescription description = await context.CompanyDescriptions.FirstOrDefaultAsync(d => d.CompanyId == id);

        return description?.Html ?? description?.Text;
    }

    public async Task<Company> GetSoldToAsync(int? id) => await context.Companies.Select(c => new Company
                                                                         {
                                                                             Id   = c.Id,
                                                                             Name = c.Name
                                                                         })
                                                                        .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<string> GetCountryNameAsync(int id) =>
        (await context.Iso31661Numeric.FirstOrDefaultAsync(c => c.Id == id))?.Name;

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

}