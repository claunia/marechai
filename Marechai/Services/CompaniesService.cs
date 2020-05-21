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
    }
}