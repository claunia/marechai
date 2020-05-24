using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class LicensesService
    {
        readonly MarechaiContext _context;

        public LicensesService(MarechaiContext context) => _context = context;

        public async Task<List<License>> GetAsync() =>
            await _context.Licenses.OrderBy(l => l.Name).Select(l => new License(){FsfApproved = l.FsfApproved, Id= l.Id, Link = l.Link, Name = l.Name, OsiApproved = l.OsiApproved, SPDX = l.SPDX}).ToListAsync();
    }
}