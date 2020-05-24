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
    }
}