using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class InstructionSetExtensionsService
    {
        readonly MarechaiContext _context;

        public InstructionSetExtensionsService(MarechaiContext context) => _context = context;

        public async Task<List<InstructionSetExtension>> GetAsync() =>
            await _context.InstructionSetExtensions.OrderBy(e => e.Extension).ToListAsync();

        public async Task DeleteAsync(int id)
        {
            InstructionSetExtension item = await _context.InstructionSetExtensions.FindAsync(id);

            if(item is null)
                return;

            _context.InstructionSetExtensions.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}