using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DocumentCompaniesController : Controller
    {
        readonly MarechaiContext _context;

        public DocumentCompaniesController(MarechaiContext context)
        {
            _context = context;
        }

        // GET: DocumentCompanies
        public async Task<IActionResult> Index()
        {
            return View(await _context.DocumentCompanies.OrderBy(c => c.Name)
                                      .Select(d => new DocumentCompanyViewModel
                                       {
                                           Id        = d.Id,
                                           Name      = d.Name,
                                           Company   = d.Company.Name,
                                           CompanyId = d.CompanyId
                                       }).ToListAsync());
        }

        // GET: DocumentCompanies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            DocumentCompanyViewModel documentCompany =
                await _context.DocumentCompanies
                              .Select(d => new DocumentCompanyViewModel
                               {
                                   Id        = d.Id,
                                   Name      = d.Name,
                                   Company   = d.Company.Name,
                                   CompanyId = d.CompanyId
                               }).FirstOrDefaultAsync(m => m.Id == id);
            if(documentCompany == null) return NotFound();

            return View(documentCompany);
        }

        // GET: DocumentCompanies/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] =
                new SelectList(_context.Companies.OrderBy(c => c.Name).Select(c => new {c.Id, c.Name}), "Id", "Name");
            return View();
        }

        // POST: DocumentCompanies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,CompanyId,Id")] DocumentCompany documentCompany)
        {
            if(!ModelState.IsValid) return View(documentCompany);

            _context.Add(documentCompany);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: DocumentCompanies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            DocumentCompany documentCompany = await _context.DocumentCompanies.FindAsync(id);

            if(documentCompany == null) return NotFound();

            ViewData["CompanyId"] =
                new SelectList(_context.Companies.OrderBy(c => c.Name).Select(c => new {c.Id, c.Name}), "Id", "Name",
                               documentCompany.CompanyId);

            return View(documentCompany);
        }

        // POST: DocumentCompanies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,CompanyId,Id")] DocumentCompany documentCompany)
        {
            if(id != documentCompany.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(documentCompany);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!DocumentCompanyExists(documentCompany.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] =
                new SelectList(_context.Companies.OrderBy(c => c.Name).Select(c => new {c.Id, c.Name}), "Id", "Name",
                               documentCompany.CompanyId);

            return View(documentCompany);
        }

        // GET: DocumentCompanies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            DocumentCompanyViewModel documentCompany =
                await _context.DocumentCompanies
                              .Select(d => new DocumentCompanyViewModel
                               {
                                   Id        = d.Id,
                                   Name      = d.Name,
                                   Company   = d.Company.Name,
                                   CompanyId = d.CompanyId
                               }).FirstOrDefaultAsync(m => m.Id == id);
            if(documentCompany == null) return NotFound();

            return View(documentCompany);
        }

        // POST: DocumentCompanies/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            DocumentCompany documentCompany = await _context.DocumentCompanies.FindAsync(id);
            _context.DocumentCompanies.Remove(documentCompany);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool DocumentCompanyExists(int id)
        {
            return _context.DocumentCompanies.Any(e => e.Id == id);
        }
    }
}