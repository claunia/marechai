using System.Linq;
using System.Threading.Tasks;
using Cicm.Database.Models;
using cicm_web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DocumentCompaniesController : Controller
    {
        readonly cicmContext _context;

        public DocumentCompaniesController(cicmContext context)
        {
            _context = context;
        }

        // GET: DocumentCompanies
        public async Task<IActionResult> Index()
        {
            return View(await _context.DocumentCompanies.Select(d => new CompanyViewModel {Id = d.Id, Name = d.Name})
                                      .ToListAsync());
        }

        // GET: DocumentCompanies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            DocumentCompany documentCompany = await _context.DocumentCompanies.FirstOrDefaultAsync(m => m.Id == id);
            if(documentCompany == null) return NotFound();

            return View(documentCompany);
        }

        // GET: DocumentCompanies/Create
        public IActionResult Create() => View();

        // POST: DocumentCompanies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,CompanyId,Id")] DocumentCompany documentCompany)
        {
            if(ModelState.IsValid)
            {
                _context.Add(documentCompany);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(documentCompany);
        }

        // GET: DocumentCompanies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            DocumentCompany documentCompany = await _context.DocumentCompanies.FindAsync(id);
            if(documentCompany == null) return NotFound();

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

            return View(documentCompany);
        }

        // GET: DocumentCompanies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            DocumentCompany documentCompany = await _context.DocumentCompanies.FirstOrDefaultAsync(m => m.Id == id);
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