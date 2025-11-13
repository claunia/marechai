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

public class MagazinesService(MarechaiContext context)
{
    public async Task<List<MagazineDto>> GetAsync() => await context.Magazines.OrderBy(b => b.NativeTitle)
                                                                          .ThenBy(b => b.FirstPublication)
                                                                          .ThenBy(b => b.Title)
                                                                          .Select(b => new MagazineDto
                                                                           {
                                                                               Id               = b.Id,
                                                                               Title            = b.Title,
                                                                               NativeTitle      = b.NativeTitle,
                                                                               FirstPublication = b.FirstPublication,
                                                                               Synopsis         = b.Synopsis,
                                                                               Issn             = b.Issn,
                                                                               CountryId        = b.CountryId,
                                                                               Country          = b.Country.Name
                                                                           })
                                                                          .ToListAsync();

    public async Task<List<MagazineDto>> GetTitlesAsync() => await context.Magazines.OrderBy(b => b.Title)
                                                                      .ThenBy(b => b.FirstPublication)
                                                                      .Select(b => new MagazineDto
                                                                       {
                                                                           Id    = b.Id,
                                                                           Title = $"{b.Title} ({b.Country.Name}"
                                                                       })
                                                                      .ToListAsync();

    public async Task<MagazineDto> GetAsync(long id) => await context.Magazines.Where(b => b.Id == id)
                                                                            .Select(b => new MagazineDto
                                                                             {
                                                                                 Id               = b.Id,
                                                                                 Title            = b.Title,
                                                                                 NativeTitle      = b.NativeTitle,
                                                                                 FirstPublication = b.FirstPublication,
                                                                                 Synopsis         = b.Synopsis,
                                                                                 Issn             = b.Issn,
                                                                                 CountryId        = b.CountryId,
                                                                                 Country          = b.Country.Name
                                                                             })
                                                                            .FirstOrDefaultAsync();

    public async Task UpdateAsync(MagazineDto dto, string userId)
    {
        Magazine model = await context.Magazines.FindAsync(dto.Id);

        if(model is null) return;

        model.Title            = dto.Title;
        model.NativeTitle      = dto.NativeTitle;
        model.FirstPublication = dto.FirstPublication;
        model.Synopsis         = dto.Synopsis;
        model.CountryId        = dto.CountryId;
        model.Issn             = dto.Issn;
        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<long> CreateAsync(MagazineDto dto, string userId)
    {
        var model = new Magazine
        {
            Title            = dto.Title,
            NativeTitle      = dto.NativeTitle,
            FirstPublication = dto.FirstPublication,
            Synopsis         = dto.Synopsis,
            CountryId        = dto.CountryId,
            Issn             = dto.Issn
        };

        await context.Magazines.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    public async Task<string> GetSynopsisTextAsync(int id) =>
        (await context.Magazines.FirstOrDefaultAsync(d => d.Id == id))?.Synopsis;

    public async Task DeleteAsync(long id, string userId)
    {
        Magazine item = await context.Magazines.FindAsync(id);

        if(item is null) return;

        context.Magazines.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}