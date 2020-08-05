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
    public class PeopleService
    {
        readonly MarechaiContext _context;

        public PeopleService(MarechaiContext context) => _context = context;

        public async Task<List<PersonViewModel>> GetAsync() =>
            await _context.People.OrderBy(p => p.DisplayName).ThenBy(p => p.Alias).ThenBy(p => p.Name).
                           ThenBy(p => p.Surname).Select(p => new PersonViewModel
                           {
                               Id             = p.Id,
                               Name           = p.Name,
                               Surname        = p.Surname,
                               CountryOfBirth = p.CountryOfBirth.Name,
                               BirthDate      = p.BirthDate,
                               DeathDate      = p.DeathDate,
                               Webpage        = p.Webpage,
                               Twitter        = p.Twitter,
                               Facebook       = p.Facebook,
                               Photo          = p.Photo,
                               Alias          = p.Alias,
                               DisplayName    = p.DisplayName
                           }).ToListAsync();

        public async Task<PersonViewModel> GetAsync(int id) =>
            await _context.People.Where(p => p.Id == id).Select(p => new PersonViewModel
            {
                Id               = p.Id,
                Name             = p.Name,
                Surname          = p.Surname,
                CountryOfBirthId = p.CountryOfBirthId,
                BirthDate        = p.BirthDate,
                DeathDate        = p.DeathDate,
                Webpage          = p.Webpage,
                Twitter          = p.Twitter,
                Facebook         = p.Facebook,
                Photo            = p.Photo,
                Alias            = p.Alias,
                DisplayName      = p.DisplayName
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(PersonViewModel viewModel, string userId)
        {
            Person model = await _context.People.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Name             = viewModel.Name;
            model.Surname          = viewModel.Surname;
            model.CountryOfBirthId = viewModel.CountryOfBirthId;
            model.BirthDate        = viewModel.BirthDate;
            model.DeathDate        = viewModel.DeathDate;
            model.Webpage          = viewModel.Webpage;
            model.Twitter          = viewModel.Twitter;
            model.Facebook         = viewModel.Facebook;
            model.Photo            = viewModel.Photo;
            model.Alias            = viewModel.Alias;
            model.DisplayName      = viewModel.DisplayName;

            await _context.SaveChangesWithUserAsync(userId);
        }

        public async Task<int> CreateAsync(PersonViewModel viewModel, string userId)
        {
            var model = new Person
            {
                Name             = viewModel.Name,
                Surname          = viewModel.Surname,
                CountryOfBirthId = viewModel.CountryOfBirthId,
                BirthDate        = viewModel.BirthDate,
                DeathDate        = viewModel.DeathDate,
                Webpage          = viewModel.Webpage,
                Twitter          = viewModel.Twitter,
                Facebook         = viewModel.Facebook,
                Photo            = viewModel.Photo,
                Alias            = viewModel.Alias,
                DisplayName      = viewModel.DisplayName
            };

            await _context.People.AddAsync(model);
            await _context.SaveChangesWithUserAsync(userId);

            return model.Id;
        }

        public async Task DeleteAsync(int id, string userId)
        {
            Person item = await _context.People.FindAsync(id);

            if(item is null)
                return;

            _context.People.Remove(item);

            await _context.SaveChangesWithUserAsync(userId);
        }
    }
}