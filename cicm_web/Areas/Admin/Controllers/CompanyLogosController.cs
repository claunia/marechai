using System.Linq;
using System.Threading.Tasks;
using Cicm.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CompanyLogosController : Controller
    {
        readonly cicmContext _context;

        public CompanyLogosController(cicmContext context)
        {
            _context = context;
        }

        // GET: CompanyLogos
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<CompanyLogo, Company> cicmContext = _context.CompanyLogos.Include(c => c.Company);
            return View(await cicmContext.OrderBy(l => l.Company.Name).ThenBy(l => l.Year)
                                         .Select(l => new CompanyLogoViewModel
                                          {
                                              Company = l.Company.Name, Id = l.Id, Year = l.Year
                                          }).ToListAsync());
        }

        // GET: CompanyLogos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            CompanyLogo companyLogo = await _context.CompanyLogos
                                                    .Include(c => c.Company).FirstOrDefaultAsync(m => m.Id == id);
            if(companyLogo == null) return NotFound();

            return View(companyLogo);
        }

        // GET: CompanyLogos/Create
        // TODO: Upload
        // public IActionResult Create()
        // {
        //     ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
        //     return View();
        // }

        // POST: CompanyLogos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        // TODO: Upload
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create([Bind("Id,CompanyId,Year,Guid")] CompanyLogo companyLogo)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         _context.Add(companyLogo);
        //         await _context.SaveChangesAsync();
        //         return RedirectToAction(nameof(Index));
        //     }
        //     ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", companyLogo.CompanyId);
        //     return View(companyLogo);
        // }

        // GET: CompanyLogos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            CompanyLogo companyLogo = await _context.CompanyLogos.FirstOrDefaultAsync(c => c.Id == id);
            if(companyLogo == null) return NotFound();

            ViewData["CompanyId"] =
                new SelectList(_context.Companies.OrderBy(l => l.Name), "Id", "Name", companyLogo.CompanyId);
            return View(companyLogo);
        }

        // POST: CompanyLogos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Year,Guid")] CompanyLogo companyLogo)
        {
            if(id != companyLogo.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(companyLogo);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!CompanyLogoExists(companyLogo.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] =
                new SelectList(_context.Companies.OrderBy(l => l.Name), "Id", "Name", companyLogo.CompanyId);
            return View(companyLogo);
        }

        // GET: CompanyLogos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            CompanyLogo companyLogo = await _context.CompanyLogos
                                                    .Include(c => c.Company).FirstOrDefaultAsync(m => m.Id == id);
            if(companyLogo == null) return NotFound();

            return View(companyLogo);
        }

        // POST: CompanyLogos/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            CompanyLogo companyLogo = await _context.CompanyLogos.FindAsync(id);
            _context.CompanyLogos.Remove(companyLogo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool CompanyLogoExists(int id)
        {
            return _context.CompanyLogos.Any(e => e.Id == id);
        }
    }
}