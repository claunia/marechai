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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class BookScansService
    {
        readonly MarechaiContext _context;

        public BookScansService(MarechaiContext context) => _context = context;

        public async Task<List<Guid>> GetGuidsByBookAsync(long bookId) =>
            await _context.BookScans.Where(p => p.BookId == bookId).Select(p => p.Id).ToListAsync();

        public async Task<BookScanViewModel> GetAsync(Guid id) =>
            await _context.BookScans.Where(p => p.Id == id).Select(p => new BookScanViewModel
            {
                Author               = p.Author,
                BookId               = p.Book.Id,
                ColorSpace           = p.ColorSpace,
                Comments             = p.Comments,
                CreationDate         = p.CreationDate,
                ExifVersion          = p.ExifVersion,
                HorizontalResolution = p.HorizontalResolution,
                Id                   = p.Id,
                ResolutionUnit       = p.ResolutionUnit,
                Page                 = p.Page,
                ScannerManufacturer  = p.ScannerManufacturer,
                ScannerModel         = p.ScannerModel,
                SoftwareUsed         = p.SoftwareUsed,
                Type                 = p.Type,
                UploadDate           = p.UploadDate,
                UserId               = p.UserId,
                VerticalResolution   = p.VerticalResolution,
                OriginalExtension    = p.OriginalExtension
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(BookScanViewModel viewModel, string userId)
        {
            BookScan model = await _context.BookScans.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Author               = viewModel.Author;
            model.ColorSpace           = viewModel.ColorSpace;
            model.Comments             = viewModel.Comments;
            model.CreationDate         = viewModel.CreationDate;
            model.ExifVersion          = viewModel.ExifVersion;
            model.HorizontalResolution = viewModel.HorizontalResolution;
            model.ResolutionUnit       = viewModel.ResolutionUnit;
            model.Page                 = viewModel.Page;
            model.ScannerManufacturer  = viewModel.ScannerManufacturer;
            model.ScannerModel         = viewModel.ScannerModel;
            model.Type                 = viewModel.Type;
            model.SoftwareUsed         = viewModel.SoftwareUsed;
            model.VerticalResolution   = viewModel.VerticalResolution;

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<Guid> CreateAsync(BookScanViewModel viewModel, string userId)
        {
            var model = new BookScan
            {
                Author               = viewModel.Author,
                BookId               = viewModel.BookId,
                ColorSpace           = viewModel.ColorSpace,
                Comments             = viewModel.Comments,
                CreationDate         = viewModel.CreationDate,
                ExifVersion          = viewModel.ExifVersion,
                HorizontalResolution = viewModel.HorizontalResolution,
                Id                   = viewModel.Id,
                ResolutionUnit       = viewModel.ResolutionUnit,
                Page                 = viewModel.Page,
                ScannerManufacturer  = viewModel.ScannerManufacturer,
                ScannerModel         = viewModel.ScannerModel,
                Type                 = viewModel.Type,
                SoftwareUsed         = viewModel.SoftwareUsed,
                UploadDate           = viewModel.UploadDate,
                UserId               = viewModel.UserId,
                VerticalResolution   = viewModel.VerticalResolution,
                OriginalExtension    = viewModel.OriginalExtension
            };

            await _context.BookScans.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            BookScan item = await _context.BookScans.FindAsync(id);

            if(item is null)
                return;

            _context.BookScans.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}