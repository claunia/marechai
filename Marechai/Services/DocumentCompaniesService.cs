using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Services
{
    public class DocumentCompaniesService
    {
        readonly MarechaiContext _context;

        public DocumentCompaniesService(MarechaiContext context) => _context = context;

        public async Task<List<DocumentCompanyViewModel>> GetAsync() => await _context.
                                                                              DocumentCompanies.OrderBy(c => c.Name).
                                                                              Select(d => new DocumentCompanyViewModel
                                                                              {
                                                                                  Id        = d.Id, Name = d.Name,
                                                                                  Company   = d.Company.Name,
                                                                                  CompanyId = d.CompanyId
                                                                              }).ToListAsync();

        public async Task DeleteAsync(int id)
        {
            DocumentCompany item = await _context.DocumentCompanies.FindAsync(id);

            if(item is null)
                return;

            _context.DocumentCompanies.Remove(item);

            await _context.SaveChangesAsync();
        }
    }
}