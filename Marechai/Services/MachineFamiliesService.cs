using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class MachineFamiliesService
    {
        readonly MarechaiContext _context;

        public MachineFamiliesService(MarechaiContext context) => _context = context;

        public async Task<List<MachineFamilyViewModel>> GetAsync() =>
            await _context.MachineFamilies.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).
                           Select(m => new MachineFamilyViewModel
                           {
                               Id = m.Id, Company = m.Company.Name, Name = m.Name
                           }).ToListAsync();

        public async Task<MachineFamily> GetAsync(int id) => await _context.MachineFamilies.FindAsync(id);

        public async Task DeleteAsync(int id)
        {
            MachineFamily item = await _context.MachineFamilies.FindAsync(id);

            if(item is null)
                return;

            _context.MachineFamilies.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}