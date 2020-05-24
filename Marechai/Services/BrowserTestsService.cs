using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class BrowserTestsService
    {
        readonly MarechaiContext _context;

        public BrowserTestsService(MarechaiContext context) => _context = context;

        public Task<List<BrowserTest>> GetAsync() => _context.
                                                     BrowserTests.OrderBy(b => b.Browser).ThenBy(b => b.Version).
                                                     ThenBy(b => b.Os).ThenBy(b => b.Platform).ThenBy(b => b.UserAgent).
                                                     ToListAsync();
    }
}