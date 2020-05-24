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
    }
}