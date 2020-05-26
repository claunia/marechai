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

        public async Task<Person> GetAsync(int id) => await _context.People.FindAsync(id);

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