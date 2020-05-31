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

        public async Task<List<CompanyViewModel>> GetAsync() => await _context.
                                                                      Companies.Include(c => c.Logos).
                                                                      OrderBy(c => c.Name).
                                                                      Select(c => new CompanyViewModel
                                                                      {
                                                                          Id = c.Id,
                                                                          LastLogo = c.
                                                                                     Logos.
                                                                                     OrderByDescending(l => l.Year).
                                                                                     FirstOrDefault().Guid,
                                                                          Name       = c.Name, Founded     = c.Founded,
                                                                          Sold       = c.Sold, SoldToId    = c.SoldToId,
                                                                          CountryId  = c.CountryId, Status = c.Status,
                                                                          Website    = c.Website, Twitter  = c.Twitter,
                                                                          Facebook   = c.Facebook, Address = c.Address,
                                                                          City       = c.City, Province    = c.Province,
                                                                          PostalCode = c.PostalCode,
                                                                          Country    = c.Country.Name
                                                                      }).ToListAsync();

        public async Task<CompanyViewModel> GetAsync(int id) => await _context.Companies.Where(c => c.Id == id).
                                                                               Select(c => new CompanyViewModel
                                                                               {
                                                                                   Id = c.Id,
                                                                                   LastLogo = c.
                                                                                              Logos.
                                                                                              OrderByDescending(l => l.
                                                                                                                    Year).
                                                                                              FirstOrDefault().Guid,
                                                                                   Name = c.Name, Founded = c.Founded,
                                                                                   Sold = c.Sold, SoldToId = c.SoldToId,
                                                                                   CountryId = c.CountryId,
                                                                                   Status = c.Status,
                                                                                   Website = c.Website,
                                                                                   Twitter = c.Twitter,
                                                                                   Facebook = c.Facebook,
                                                                                   Address = c.Address, City = c.City,
                                                                                   Province = c.Province,
                                                                                   PostalCode = c.PostalCode,
                                                                                   Country = c.Country.Name
                                                                               }).FirstOrDefaultAsync();

        public async Task UpdateAsync(CompanyViewModel viewModel)
        {
            Company model = await _context.Companies.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Name       = viewModel.Name;
            model.Founded    = viewModel.Founded;
            model.Sold       = viewModel.Sold;
            model.SoldToId   = viewModel.SoldToId;
            model.CountryId  = viewModel.CountryId;
            model.Status     = viewModel.Status;
            model.Website    = viewModel.Website;
            model.Twitter    = viewModel.Twitter;
            model.Facebook   = viewModel.Facebook;
            model.Address    = viewModel.Address;
            model.City       = viewModel.City;
            model.Province   = viewModel.Province;
            model.PostalCode = viewModel.PostalCode;

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(CompanyViewModel viewModel)
        {
            var model = new Company
            {
                Name       = viewModel.Name, Founded       = viewModel.Founded, Sold     = viewModel.Sold,
                SoldToId   = viewModel.SoldToId, CountryId = viewModel.CountryId, Status = viewModel.Status,
                Website    = viewModel.Website, Twitter    = viewModel.Twitter, Facebook = viewModel.Facebook,
                Address    = viewModel.Address, City       = viewModel.City, Province    = viewModel.Province,
                PostalCode = viewModel.PostalCode
            };

            await _context.Companies.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.Id;
        }

        public async Task<List<Machine>> GetMachinesAsync(int id) =>
            await _context.Machines.Where(m => m.CompanyId == id).OrderBy(m => m.Name).Select(m => new Machine
            {
                Id = m.Id, Name = m.Name, Type = m.Type
            }).ToListAsync();

        public async Task<string> GetDescriptionTextAsync(int id)
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

        public async Task DeleteAsync(int id)
        {
            Company item = await _context.Companies.FindAsync(id);

            if(item is null)
                return;

            _context.Companies.Remove(item);

            await _context.SaveChangesAsync();
        }

        public async Task<CompanyDescriptionViewModel> GetDescriptionAsync(int id) => await _context.
                                                                                            CompanyDescriptions.
                                                                                            Where(d => d.CompanyId ==
                                                                                                       id).
                                                                                            Select(d =>
                                                                                                       new
                                                                                                           CompanyDescriptionViewModel
                                                                                                           {
                                                                                                               Id =
                                                                                                                   d.Id,
                                                                                                               CompanyId
                                                                                                                   = d.
                                                                                                                       CompanyId,
                                                                                                               Html = d.
                                                                                                                   Html,
                                                                                                               Markdown
                                                                                                                   = d.
                                                                                                                       Text
                                                                                                           }).
                                                                                            FirstOrDefaultAsync();

        public async Task<int> CreateOrUpdateDescriptionAsync(int id, CompanyDescriptionViewModel description)
        {
            CompanyDescription current = await _context.CompanyDescriptions.FirstOrDefaultAsync(d => d.CompanyId == id);

            if(current is null)
            {
                current = new CompanyDescription
                {
                    CompanyId = id, Html = description.Html, Text = description.Markdown
                };

                await _context.CompanyDescriptions.AddAsync(current);
            }
            else
            {
                current.Html = description.Html;
                current.Text = description.Markdown;
            }

            await _context.SaveChangesAsync();

            return current.Id;
        }
    }
}