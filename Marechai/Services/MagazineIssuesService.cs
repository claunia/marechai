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

namespace Marechai.Services;

public class MagazineIssuesService(MarechaiContext context)
{
    public async Task<List<MagazineIssueDto>> GetAsync() => await context.MagazineIssues
                                                                               .OrderBy(b => b.Magazine.Title)
                                                                               .ThenBy(b => b.Published)
                                                                               .ThenBy(b => b.Caption)
                                                                               .Select(b => new MagazineIssueDto
                                                                                {
                                                                                    Id            = b.Id,
                                                                                    MagazineId    = b.MagazineId,
                                                                                    MagazineTitle = b.Magazine.Title,
                                                                                    Caption       = b.Caption,
                                                                                    NativeCaption = b.NativeCaption,
                                                                                    Published     = b.Published,
                                                                                    ProductCode   = b.ProductCode,
                                                                                    Pages         = b.Pages,
                                                                                    IssueNumber   = b.IssueNumber
                                                                                })
                                                                               .ToListAsync();

    public async Task<MagazineIssueDto> GetAsync(long id) => await context.MagazineIssues.Where(b => b.Id == id)
                                                                      .Select(b => new MagazineIssueDto
                                                                       {
                                                                           Id            = b.Id,
                                                                           MagazineId    = b.MagazineId,
                                                                           MagazineTitle = b.Magazine.Title,
                                                                           Caption       = b.Caption,
                                                                           NativeCaption = b.NativeCaption,
                                                                           Published     = b.Published,
                                                                           ProductCode   = b.ProductCode,
                                                                           Pages         = b.Pages,
                                                                           IssueNumber   = b.IssueNumber
                                                                       })
                                                                      .FirstOrDefaultAsync();

    public async Task UpdateAsync(MagazineIssueDto dto, string userId)
    {
        MagazineIssue model = await context.MagazineIssues.FindAsync(dto.Id);

        if(model is null) return;

        model.MagazineId    = dto.MagazineId;
        model.Caption       = dto.Caption;
        model.NativeCaption = dto.NativeCaption;
        model.Published     = dto.Published;
        model.ProductCode   = dto.ProductCode;
        model.Pages         = dto.Pages;
        model.IssueNumber   = dto.IssueNumber;
        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<long> CreateAsync(MagazineIssueDto dto, string userId)
    {
        var model = new MagazineIssue
        {
            MagazineId    = dto.MagazineId,
            Caption       = dto.Caption,
            NativeCaption = dto.NativeCaption,
            Published     = dto.Published,
            ProductCode   = dto.ProductCode,
            Pages         = dto.Pages,
            IssueNumber   = dto.IssueNumber
        };

        await context.MagazineIssues.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    public async Task DeleteAsync(long id, string userId)
    {
        MagazineIssue item = await context.MagazineIssues.FindAsync(id);

        if(item is null) return;

        context.MagazineIssues.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}