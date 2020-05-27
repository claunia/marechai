using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class CompanyLogosService
    {
        readonly MarechaiContext                    _context;

        public CompanyLogosService(MarechaiContext context)
        {
            _context = context;
        }

        public async Task<List<CompanyLogo>> GetByCompany(int companyId) => await _context.CompanyLogos.Where(l => l.CompanyId == companyId).OrderBy(l => l.Year).ToListAsync();
    }
}