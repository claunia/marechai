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
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class DocumentsService
    {
        readonly MarechaiContext _context;

        public DocumentsService(MarechaiContext context) => _context = context;

        public async Task<List<DocumentViewModel>> GetAsync() => await _context.
                                                                       Documents.OrderBy(b => b.NativeTitle).
                                                                       ThenBy(b => b.Published).ThenBy(b => b.Title).
                                                                       Select(b => new DocumentViewModel
                                                                       {
                                                                           Id          = b.Id,
                                                                           Title       = b.Title,
                                                                           NativeTitle = b.NativeTitle,
                                                                           Published   = b.Published,
                                                                           Synopsis    = b.Synopsis,
                                                                           CountryId   = b.CountryId,
                                                                           Country     = b.Country.Name
                                                                       }).ToListAsync();

        public async Task<DocumentViewModel> GetAsync(long id) => await _context.Documents.Where(b => b.Id == id).
                                                                      Select(b => new DocumentViewModel
                                                                      {
                                                                          Id          = b.Id,
                                                                          Title       = b.Title,
                                                                          NativeTitle = b.NativeTitle,
                                                                          Published   = b.Published,
                                                                          Synopsis    = b.Synopsis,
                                                                          CountryId   = b.CountryId,
                                                                          Country     = b.Country.Name
                                                                      }).FirstOrDefaultAsync();

        public async Task UpdateAsync(DocumentViewModel viewModel, string userId)
        {
            Document model = await _context.Documents.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Title       = viewModel.Title;
            model.NativeTitle = viewModel.NativeTitle;
            model.Published   = viewModel.Published;
            model.Synopsis    = viewModel.Synopsis;
            model.CountryId   = viewModel.CountryId;
            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(DocumentViewModel viewModel, string userId)
        {
            var model = new Document
            {
                Title       = viewModel.Title,
                NativeTitle = viewModel.NativeTitle,
                Published   = viewModel.Published,
                Synopsis    = viewModel.Synopsis,
                CountryId   = viewModel.CountryId
            };

            await _context.Documents.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task<string> GetSynopsisTextAsync(int id) =>
            (await _context.Documents.FirstOrDefaultAsync(d => d.Id == id))?.Synopsis;

        public async Task DeleteAsync(long id, string userId)
        {
            Document item = await _context.Documents.FindAsync(id);

            if(item is null)
                return;

            _context.Documents.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}