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

public class ResolutionsService(MarechaiContext context)
{
    public async Task<List<ResolutionDto>> GetAsync() => await context.Resolutions
                                                                            .Select(r => new ResolutionDto
                                                                             {
                                                                                 Id        = r.Id,
                                                                                 Width     = r.Width,
                                                                                 Height    = r.Height,
                                                                                 Colors    = r.Colors,
                                                                                 Palette   = r.Palette,
                                                                                 Chars     = r.Chars,
                                                                                 Grayscale = r.Grayscale
                                                                             })
                                                                            .OrderBy(r => r.Width)
                                                                            .ThenBy(r => r.Height)
                                                                            .ThenBy(r => r.Chars)
                                                                            .ThenBy(r => r.Grayscale)
                                                                            .ThenBy(r => r.Colors)
                                                                            .ThenBy(r => r.Palette)
                                                                            .ToListAsync();

    public async Task<ResolutionDto> GetAsync(int id) => await context.Resolutions.Where(r => r.Id == id)
                                                                             .Select(r => new ResolutionDto
                                                                              {
                                                                                  Id        = r.Id,
                                                                                  Width     = r.Width,
                                                                                  Height    = r.Height,
                                                                                  Colors    = r.Colors,
                                                                                  Palette   = r.Palette,
                                                                                  Chars     = r.Chars,
                                                                                  Grayscale = r.Grayscale
                                                                              })
                                                                             .FirstOrDefaultAsync();

    public async Task UpdateAsync(ResolutionDto dto, string userId)
    {
        Resolution model = await context.Resolutions.FindAsync(dto.Id);

        if(model is null) return;

        model.Chars     = dto.Chars;
        model.Colors    = dto.Colors;
        model.Grayscale = dto.Grayscale;
        model.Height    = dto.Height;
        model.Palette   = dto.Palette;
        model.Width     = dto.Width;

        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<int> CreateAsync(ResolutionDto dto, string userId)
    {
        var model = new Resolution
        {
            Chars     = dto.Chars,
            Colors    = dto.Colors,
            Grayscale = dto.Grayscale,
            Height    = dto.Height,
            Palette   = dto.Palette,
            Width     = dto.Width
        };

        await context.Resolutions.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    public async Task DeleteAsync(int id, string userId)
    {
        Resolution item = await context.Resolutions.FindAsync(id);

        if(item is null) return;

        context.Resolutions.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}