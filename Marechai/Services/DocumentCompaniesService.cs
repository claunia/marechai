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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class DocumentCompaniesService
    {
        readonly MarechaiContext _context;

        public DocumentCompaniesService(MarechaiContext context) => _context = context;

        public async Task<List<DocumentCompanyViewModel>> GetAsync() => await _context.
                                                                              DocumentCompanies.OrderBy(c => c.Name).
                                                                              Select(d => new DocumentCompanyViewModel
                                                                              {
                                                                                  Id        = d.Id, Name = d.Name,
                                                                                  Company   = d.Company.Name,
                                                                                  CompanyId = d.CompanyId
                                                                              }).ToListAsync();

        public async Task<DocumentCompanyViewModel> GetAsync(int id) =>
            await _context.DocumentCompanies.Where(d => d.Id == id).Select(d => new DocumentCompanyViewModel
            {
                Id = d.Id, Name = d.Name, CompanyId = d.CompanyId
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(DocumentCompanyViewModel viewModel)
        {
            DocumentCompany model = await _context.DocumentCompanies.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.CompanyId = viewModel.CompanyId;
            model.Name      = viewModel.Name;

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(DocumentCompanyViewModel viewModel)
        {
            var model = new DocumentCompany
            {
                CompanyId = viewModel.CompanyId, Name = viewModel.Name
            };

            await _context.DocumentCompanies.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.Id;
        }

        public async Task DeleteAsync(int id)
        {
            DocumentCompany item = await _context.DocumentCompanies.FindAsync(id);

            if(item is null)
                return;

            _context.DocumentCompanies.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}