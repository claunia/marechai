/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : CompaniesService.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Companies service
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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Marechai.Services
{
    public class CompaniesService
    {
        readonly MarechaiContext                    _context;
        readonly IStringLocalizer<CompaniesService> _l;

        public CompaniesService(MarechaiContext context, IStringLocalizer<CompaniesService> localizer)
        {
            _context = context;
            _l       = localizer;
        }

        public Task<List<CompanyViewModel>> GetCompaniesAsync() => _context.
                                                                   Companies.Include(c => c.Logos).OrderBy(c => c.Name).
                                                                   Select(c => new CompanyViewModel
                                                                   {
                                                                       Id = c.Id,
                                                                       LastLogo = c.
                                                                                  Logos.OrderByDescending(l => l.Year).
                                                                                  FirstOrDefault().Guid,
                                                                       Name = c.Name
                                                                   }).ToListAsync();

        public Task<Company> GetCompanyAsync(int id) => _context.Companies.FirstOrDefaultAsync(c => c.Id == id);

        public async Task<List<Machine>> GetMachinesAsync(int id) =>
            await _context.Machines.Where(m => m.CompanyId == id).OrderBy(m => m.Name).Select(m => new Machine
            {
                Id = m.Id, Name = m.Name, Type = m.Type
            }).ToListAsync();

        public async Task<string> GetDescriptionAsync(int id)
        {
            CompanyDescription description =
                await _context.CompanyDescriptions.FirstOrDefaultAsync(d => d.CompanyId == id);

            return description?.Html ?? description?.Text;
        }

        public async Task<Company> GetSoldToAsync(int? id) => await _context.Companies.Select(c => new Company
        {
            Id = c.Id, Name = c.Name
        }).FirstOrDefaultAsync(c => c.Id == id);

        public async Task<string> GetCountryNameAsync(int id) =>
            (await _context.Iso31661Numeric.FirstOrDefaultAsync(c => c.Id == id))?.Name;

        public Task<List<CompanyViewModel>> GetCompaniesByCountryAsync(int countryId) => _context.
                                                                                         Companies.
                                                                                         Include(c => c.Logos).
                                                                                         Where(c => c.CountryId ==
                                                                                                    countryId).
                                                                                         OrderBy(c => c.Name).
                                                                                         Select(c =>
                                                                                                    new CompanyViewModel
                                                                                                    {
                                                                                                        Id = c.Id,
                                                                                                        LastLogo = c.
                                                                                                                   Logos.
                                                                                                                   OrderByDescending(l =>
                                                                                                                                         l.
                                                                                                                                             Year).
                                                                                                                   FirstOrDefault().
                                                                                                                   Guid,
                                                                                                        Name = c.Name
                                                                                                    }).ToListAsync();

        public Task<List<CompanyViewModel>> GetCompaniesByLetterAsync(char id) => _context.
                                                                                  Companies.Include(c => c.Logos).
                                                                                  Where(c => EF.Functions.Like(c.Name,
                                                                                                               $"{id}%")).
                                                                                  OrderBy(c => c.Name).
                                                                                  Select(c => new CompanyViewModel
                                                                                  {
                                                                                      Id = c.Id,
                                                                                      LastLogo = c.
                                                                                                 Logos.
                                                                                                 OrderByDescending(l =>
                                                                                                                       l.
                                                                                                                           Year).
                                                                                                 FirstOrDefault().Guid,
                                                                                      Name = c.Name
                                                                                  }).ToListAsync();
    }
}