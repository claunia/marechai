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

        public async Task UpdateAsync(DocumentPersonViewModel viewModel)
        {
            DocumentPerson model = await _context.DocumentPeople.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.Alias       = viewModel.Alias;
            model.Name        = viewModel.Name;
            model.Surname     = viewModel.Surname;
            model.DisplayName = viewModel.DisplayName;
            model.PersonId    = viewModel.PersonId;

            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(DocumentPersonViewModel viewModel)
        {
            var model = new DocumentPerson
            {
                Alias       = viewModel.Alias, Name           = viewModel.Name, Surname = viewModel.Surname,
                DisplayName = viewModel.DisplayName, PersonId = viewModel.PersonId
            };

            await _context.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.Id;
        }

        public async Task DeleteAsync(int id)
        {
            DocumentPerson item = await _context.DocumentPeople.FindAsync(id);

            if(item is null)
                return;

            _context.DocumentPeople.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}