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
                               Id = p.Id, Name = p.Name, Surname = p.Surname, CountryOfBirth = p.CountryOfBirth.Name,
                               BirthDate = p.BirthDate, DeathDate = p.DeathDate, Webpage = p.Webpage,
                               Twitter = p.Twitter, Facebook = p.Facebook, Photo = p.Photo, Alias = p.Alias,
                               DisplayName = p.DisplayName
                           }).ToListAsync();

        public async Task<PersonViewModel> GetAsync(int id) =>
            await _context.People.Where(p => p.Id == id).Select(p => new PersonViewModel
            {
                Id        = p.Id, Name             = p.Name, Surname = p.Surname, CountryOfBirthId = p.CountryOfBirthId,
                BirthDate = p.BirthDate, DeathDate = p.DeathDate, Webpage = p.Webpage, Twitter = p.Twitter,
                Facebook  = p.Facebook, Photo      = p.Photo, Alias = p.Alias, DisplayName = p.DisplayName
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(PersonViewModel viewModel)
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

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Person item = await _context.People.FindAsync(id);

            if(item is null)
                return;

            _context.People.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}