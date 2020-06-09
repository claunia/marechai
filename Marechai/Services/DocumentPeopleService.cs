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
    public class DocumentPeopleService
    {
        readonly MarechaiContext _context;

        public DocumentPeopleService(MarechaiContext context) => _context = context;

        public async Task<List<DocumentPersonViewModel>> GetAsync() => await _context.
                                                                             DocumentPeople.OrderBy(d => d.DisplayName).
                                                                             ThenBy(d => d.Alias).ThenBy(d => d.Name).
                                                                             ThenBy(d => d.Surname).
                                                                             Select(d => new DocumentPersonViewModel
                                                                             {
                                                                                 Id       = d.Id, Name = d.FullName,
                                                                                 Person   = d.Person.FullName,
                                                                                 PersonId = d.PersonId
                                                                             }).ToListAsync();

        public async Task<DocumentPersonViewModel> GetAsync(int id) =>
            await _context.DocumentPeople.Where(p => p.Id == id).Select(d => new DocumentPersonViewModel
            {
                Id          = d.Id, Alias             = d.Alias, Name = d.Name, Surname = d.Surname,
                DisplayName = d.DisplayName, PersonId = d.PersonId
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(DocumentPersonViewModel viewModel, string userId)
        {
            DocumentPerson model = await _context.DocumentPeople.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Alias       = viewModel.Alias;
            model.Name        = viewModel.Name;
            model.Surname     = viewModel.Surname;
            model.DisplayName = viewModel.DisplayName;
            model.PersonId    = viewModel.PersonId;

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<int> CreateAsync(DocumentPersonViewModel viewModel, string userId)
        {
            var model = new DocumentPerson
            {
                Alias       = viewModel.Alias, Name           = viewModel.Name, Surname = viewModel.Surname,
                DisplayName = viewModel.DisplayName, PersonId = viewModel.PersonId
            };

            await _context.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(int id, string userId)
        {
            DocumentPerson item = await _context.DocumentPeople.FindAsync(id);

            if(item is null)
                return;

            _context.DocumentPeople.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}