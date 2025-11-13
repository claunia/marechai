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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services;

public class DocumentCompaniesService(MarechaiContext context)
{
    public async Task<List<DocumentCompanyDto>> GetAsync() => await context.DocumentCompanies
                                                                                 .OrderBy(c => c.Name)
                                                                                 .Select(d => new DocumentCompanyDto
                                                                                  {
                                                                                      Id        = d.Id,
                                                                                      Name      = d.Name,
                                                                                      Company   = d.Company.Name,
                                                                                      CompanyId = d.CompanyId
                                                                                  })
                                                                                 .ToListAsync();

    public async Task<DocumentCompanyDto> GetAsync(int id) => await context.DocumentCompanies
                                                                       .Where(d => d.Id == id)
                                                                       .Select(d => new DocumentCompanyDto
                                                                        {
                                                                            Id        = d.Id,
                                                                            Name      = d.Name,
                                                                            CompanyId = d.CompanyId
                                                                        })
                                                                       .FirstOrDefaultAsync();

    public async Task UpdateAsync(DocumentCompanyDto dto, string userId)
    {
        DocumentCompany model = await context.DocumentCompanies.FindAsync(dto.Id);

        if(model is null) return;

        model.CompanyId = dto.CompanyId;
        model.Name      = dto.Name;

        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<int> CreateAsync(DocumentCompanyDto dto, string userId)
    {
        var model = new DocumentCompany
        {
            CompanyId = dto.CompanyId,
            Name      = dto.Name
        };

        await context.DocumentCompanies.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    public async Task DeleteAsync(int id, string userId)
    {
        DocumentCompany item = await context.DocumentCompanies.FindAsync(id);

        if(item is null) return;

        context.DocumentCompanies.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}