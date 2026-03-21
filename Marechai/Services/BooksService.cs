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

public class BooksService(MarechaiContext context)
{
    public async Task<List<BookDto>> GetAsync() => await context.Books.OrderBy(b => b.NativeTitle)
                                                                      .ThenBy(b => b.Published)
                                                                      .ThenBy(b => b.Title)
                                                                      .Select(b => new BookDto
                                                                       {
                                                                           Id          = b.Id,
                                                                           Title       = b.Title,
                                                                           NativeTitle = b.NativeTitle,
                                                                           Published   = b.Published,
                                                                           Isbn        = b.Isbn,
                                                                           CountryId   = b.CountryId,
                                                                           Pages       = b.Pages,
                                                                           Edition     = b.Edition,
                                                                           PreviousId  = b.PreviousId,
                                                                           SourceId    = b.SourceId,
                                                                           Country     = b.Country.Name
                                                                       })
                                                                      .ToListAsync();

    public async Task<BookDto> GetAsync(long id) => await context.Books.Where(b => b.Id == id)
                                                                        .Select(b => new BookDto
                                                                         {
                                                                             Id          = b.Id,
                                                                             Title       = b.Title,
                                                                             NativeTitle = b.NativeTitle,
                                                                             Published   = b.Published,
                                                                             Isbn        = b.Isbn,
                                                                             CountryId   = b.CountryId,
                                                                             Pages       = b.Pages,
                                                                             Edition     = b.Edition,
                                                                             PreviousId  = b.PreviousId,
                                                                             SourceId    = b.SourceId,
                                                                             Country     = b.Country.Name
                                                                         })
                                                                        .FirstOrDefaultAsync();

    public async Task UpdateAsync(BookDto dto, string userId)
    {
        Book model = await context.Books.FindAsync(dto.Id);

        if(model is null) return;

        model.Title       = dto.Title;
        model.NativeTitle = dto.NativeTitle;
        model.Published   = dto.Published;
        model.CountryId   = dto.CountryId;
        model.Isbn        = dto.Isbn;
        model.Pages       = dto.Pages;
        model.Edition     = dto.Edition;
        model.PreviousId  = dto.PreviousId;
        model.SourceId    = dto.SourceId;
        await context.SaveChangesWithUserAsync(userId);
    }

    public async Task<long> CreateAsync(BookDto dto, string userId)
    {
        var model = new Book
        {
            Title       = dto.Title,
            NativeTitle = dto.NativeTitle,
            Published   = dto.Published,
            CountryId   = dto.CountryId,
            Isbn        = dto.Isbn,
            Pages       = dto.Pages,
            Edition     = dto.Edition,
            PreviousId  = dto.PreviousId,
            SourceId    = dto.SourceId
        };

        await context.Books.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    public async Task DeleteAsync(long id, string userId)
    {
        Book item = await context.Books.FindAsync(id);

        if(item is null) return;

        context.Books.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}