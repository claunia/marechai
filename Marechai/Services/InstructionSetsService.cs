using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class InstructionSetsService
    {
        readonly MarechaiContext _context;

        public InstructionSetsService(MarechaiContext context) => _context = context;

        public async Task<List<InstructionSet>> GetAsync() =>
            await _context.InstructionSets.OrderBy(s => s.Name).ToListAsync();

        public async Task<InstructionSet> GetAsync(int id) => await _context.InstructionSets.FindAsync(id);

        public async Task DeleteAsync(int id)
        {
            InstructionSet item = await _context.InstructionSets.FindAsync(id);

            if(item is null)
                return;

            _context.InstructionSets.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}