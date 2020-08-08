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
    public class DumpsService
    {
        readonly MarechaiContext _context;

        public DumpsService(MarechaiContext context) => _context = context;

        public async Task<List<DumpViewModel>> GetAsync() => await _context.
                                                                   Dumps.OrderBy(d => d.Dumper).
                                                                   ThenBy(d => d.DumpingGroup).
                                                                   ThenBy(b => b.Media.Title).ThenBy(d => d.DumpDate).
                                                                   Select(d => new DumpViewModel
                                                                   {
                                                                       Id           = d.Id,
                                                                       Dumper       = d.Dumper,
                                                                       UserId       = d.UserId,
                                                                       DumpingGroup = d.DumpingGroup,
                                                                       DumpDate     = d.DumpDate,
                                                                       UserName     = d.User.UserName,
                                                                       MediaId      = d.MediaId,
                                                                       MediaTitle   = d.Media.Title,
                                                                       MediaDumpId  = d.MediaDumpId
                                                                   }).ToListAsync();

        public async Task<DumpViewModel> GetAsync(ulong id) => await _context.Dumps.Where(d => d.Id == id).
                                                                              Select(d => new DumpViewModel
                                                                              {
                                                                                  Id           = d.Id,
                                                                                  Dumper       = d.Dumper,
                                                                                  UserId       = d.User.Id,
                                                                                  DumpingGroup = d.DumpingGroup,
                                                                                  DumpDate     = d.DumpDate,
                                                                                  UserName     = d.User.UserName,
                                                                                  MediaId      = d.MediaId,
                                                                                  MediaTitle   = d.Media.Title,
                                                                                  MediaDumpId  = d.MediaDumpId
                                                                              }).FirstOrDefaultAsync();

        public async Task UpdateAsync(DumpViewModel viewModel, string userId)
        {
            Dump model = await _context.Dumps.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Dumper       = viewModel.Dumper;
            model.UserId       = viewModel.UserId;
            model.DumpingGroup = viewModel.DumpingGroup;
            model.DumpDate     = viewModel.DumpDate;
            model.MediaId      = viewModel.MediaId;
            model.MediaDumpId  = viewModel.MediaDumpId;
            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<ulong> CreateAsync(DumpViewModel viewModel, string userId)
        {
            var model = new Dump
            {
                Dumper       = viewModel.Dumper,
                UserId       = viewModel.UserId,
                DumpingGroup = viewModel.DumpingGroup,
                DumpDate     = viewModel.DumpDate,
                MediaId      = viewModel.MediaId,
                MediaDumpId  = viewModel.MediaDumpId
            };

            await _context.Dumps.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(ulong id, string userId)
        {
            Dump item = await _context.Dumps.FindAsync(id);

            if(item is null)
                return;

            _context.Dumps.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}