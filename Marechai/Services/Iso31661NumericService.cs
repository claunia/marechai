using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class Iso31661NumericService
    {
        readonly MarechaiContext _context;

        public Iso31661NumericService(MarechaiContext context) => _context = context;

        public async Task<List<Iso31661Numeric>> GetAsync() =>
            await _context.Iso31661Numeric.OrderBy(c => c.Name).ToListAsync();
    }
}