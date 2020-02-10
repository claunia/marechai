using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cicm_web
{
    [Area("Admin")]
    [Authorize]
    public class LicensesController : Controller
    {
        readonly cicmContext _context;

        public LicensesController(cicmContext context)
        {
            _context = context;
        }

        // GET: Licenses
        public async Task<IActionResult> Index()
        {
            return View(await _context.Licenses.OrderBy(l => l.Name).ToListAsync());
        }

        // GET: Licenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            License license = await _context.Licenses.FirstOrDefaultAsync(m => m.Id == id);
            if(license == null) return NotFound();

            return View(license);
        }

        // GET: Licenses/Create
        public IActionResult Create() => View();

        // POST: Licenses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,SPDX,FsfApproved,OsiApproved,Link,Text,Id")]
                                                License license)
        {
            if(ModelState.IsValid)
            {
                _context.Add(license);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(license);
        }

        // GET: Licenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            License license = await _context.Licenses.FindAsync(id);
            if(license == null) return NotFound();

            return View(license);
        }

        // POST: Licenses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,SPDX,FsfApproved,OsiApproved,Link,Text,Id")]
                                              License license)
        {
            if(id != license.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(license);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!LicenseExists(license.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(license);
        }

        // GET: Licenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            License license = await _context.Licenses.FirstOrDefaultAsync(m => m.Id == id);
            if(license == null) return NotFound();

            return View(license);
        }

        // POST: Licenses/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            License license = await _context.Licenses.FindAsync(id);
            _context.Licenses.Remove(license);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool LicenseExists(int id)
        {
            return _context.Licenses.Any(e => e.Id == id);
        }
    }
}