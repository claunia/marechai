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

        public async Task<DocumentCompanyViewModel> GetAsync(int id) =>
            await _context.DocumentCompanies.Where(d => d.Id == id).Select(d => new DocumentCompanyViewModel
            {
                Id = d.Id, Name = d.Name, CompanyId = d.CompanyId
            }).FirstOrDefaultAsync();

        public async Task UpdateAsync(DocumentCompanyViewModel viewModel)
        {
            DocumentCompany model = await _context.DocumentCompanies.FindAsync(viewModel.Id);

            if(model is null)
                return;

            model.CompanyId = viewModel.CompanyId;
            model.Name      = viewModel.Name;

            await _context.SaveChangesAsync();
        }

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