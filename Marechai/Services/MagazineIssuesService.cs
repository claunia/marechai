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
    public class MagazineIssuesService
    {
        readonly MarechaiContext _context;

        public MagazineIssuesService(MarechaiContext context) => _context = context;

        public async Task<List<MagazineIssueViewModel>> GetAsync() => await _context.
                                                                            MagazineIssues.
                                                                            OrderBy(b => b.Magazine.Title).
                                                                            ThenBy(b => b.Published).
                                                                            ThenBy(b => b.Caption).
                                                                            Select(b => new MagazineIssueViewModel
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
                                                                            }).ToListAsync();

        public async Task<MagazineIssueViewModel> GetAsync(long id) =>
            await _context.MagazineIssues.Where(b => b.Id == id).Select(b => new MagazineIssueViewModel
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
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(MagazineIssueViewModel viewModel, string userId)
        {
            MagazineIssue model = await _context.MagazineIssues.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.MagazineId    = viewModel.MagazineId;
            model.Caption       = viewModel.Caption;
            model.NativeCaption = viewModel.NativeCaption;
            model.Published     = viewModel.Published;
            model.ProductCode   = viewModel.ProductCode;
            model.Pages         = viewModel.Pages;
            model.IssueNumber   = viewModel.IssueNumber;
            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<long> CreateAsync(MagazineIssueViewModel viewModel, string userId)
        {
            var model = new MagazineIssue
            {
                MagazineId    = viewModel.MagazineId,
                Caption       = viewModel.Caption,
                NativeCaption = viewModel.NativeCaption,
                Published     = viewModel.Published,
                ProductCode   = viewModel.ProductCode,
                Pages         = viewModel.Pages,
                IssueNumber   = viewModel.IssueNumber
            };

            await _context.MagazineIssues.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(long id, string userId)
        {
            MagazineIssue item = await _context.MagazineIssues.FindAsync(id);

            if(item is null)
                return;

            _context.MagazineIssues.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}