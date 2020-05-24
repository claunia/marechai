using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class GpusService
    {
        readonly MarechaiContext _context;

        public GpusService(MarechaiContext context) => _context = context;

        public async Task<List<GpuViewModel>> GetAsync() => await _context.Gpus.Include(g => g.Company).OrderBy(g => g.Company.Name).ThenBy(g => g.Name).
                                                                           ThenBy(g => g.Introduced).Select(g => new GpuViewModel
                                                                           {
                                                                               Id         = g.Id, Company = g.Company.Name,
                                                                               Introduced = g.Introduced,
                                                                               ModelCode  = g.ModelCode, Name = g.Name
                                                                           }).ToListAsync();
    }
}