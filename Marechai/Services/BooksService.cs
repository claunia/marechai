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
    public class BooksService
    {
        readonly MarechaiContext _context;

        public BooksService(MarechaiContext context) => _context = context;

        public async Task<List<BookViewModel>> GetAsync() => await _context.
                                                                   Books.OrderBy(b => b.NativeTitle).
                                                                   ThenBy(b => b.Published).ThenBy(b => b.Title).
                                                                   Select(b => new BookViewModel
                                                                   {
                                                                       Id          = b.Id,
                                                                       Title       = b.Title,
                                                                       NativeTitle = b.NativeTitle,
                                                                       Published   = b.Published,
                                                                       Synopsis    = b.Synopsis,
                                                                       Isbn        = b.Isbn,
                                                                       CountryId   = b.CountryId,
                                                                       Pages       = b.Pages,
                                                                       Edition     = b.Edition,
                                                                       PreviousId  = b.PreviousId,
                                                                       SourceId    = b.SourceId,
                                                                       Country     = b.Country.Name
                                                                   }).ToListAsync();

        public async Task<BookViewModel> GetAsync(long id) => await _context.Books.Where(b => b.Id == id).
                                                                             Select(b => new BookViewModel
                                                                             {
                                                                                 Id          = b.Id,
                                                                                 Title       = b.Title,
                                                                                 NativeTitle = b.NativeTitle,
                                                                                 Published   = b.Published,
                                                                                 Synopsis    = b.Synopsis,
                                                                                 Isbn        = b.Isbn,
                                                                                 CountryId   = b.CountryId,
                                                                                 Pages       = b.Pages,
                                                                                 Edition     = b.Edition,
                                                                                 PreviousId  = b.PreviousId,
                                                                                 SourceId    = b.SourceId,
                                                                                 Country     = b.Country.Name
                                                                             }).FirstOrDefaultAsync();

        public async Task UpdateAsync(BookViewModel viewModel, string userId)
        {
            Book model = await _context.Books.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Title       = viewModel.Title;
            model.NativeTitle = viewModel.NativeTitle;
            model.Published   = viewModel.Published;
            model.Synopsis    = viewModel.Synopsis;
            model.CountryId   = viewModel.CountryId;
            model.Isbn        = viewModel.Isbn;
            model.Pages       = viewModel.Pages;
            model.Edition     = viewModel.Edition;
            model.PreviousId  = viewModel.PreviousId;
            model.SourceId    = viewModel.SourceId;
            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(BookViewModel viewModel, string userId)
        {
            var model = new Book
            {
                Title       = viewModel.Title,
                NativeTitle = viewModel.NativeTitle,
                Published   = viewModel.Published,
                Synopsis    = viewModel.Synopsis,
                CountryId   = viewModel.CountryId,
                Isbn        = viewModel.Isbn,
                Pages       = viewModel.Pages,
                Edition     = viewModel.Edition,
                PreviousId  = viewModel.PreviousId,
                SourceId    = viewModel.SourceId
            };

            await _context.Books.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task<string> GetSynopsisTextAsync(int id) =>
            (await _context.Books.FirstOrDefaultAsync(d => d.Id == id))?.Synopsis;

        public async Task DeleteAsync(long id, string userId)
        {
            Book item = await _context.Books.FindAsync(id);

            if(item is null)
                return;

            _context.Books.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}