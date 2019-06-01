using System.Linq;
using System.Threading.Tasks;
using Cicm.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ScreensController : Controller
    {
        readonly cicmContext _context;

        public ScreensController(cicmContext context)
        {
            _context = context;
        }

        // GET: Screens
        public async Task<IActionResult> Index() =>
            View(await _context.Screens.OrderBy(s => s.Diagonal).ThenBy(s => s.EffectiveColors).ThenBy(s => s.Type)
                               .ThenBy(s => s.Size).ToListAsync());

        // GET: Screens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            Screen screen = await _context.Screens.FirstOrDefaultAsync(m => m.Id == id);
            if(screen == null) return NotFound();

            return View(screen);
        }

        // GET: Screens/Create
        public IActionResult Create() => View();

        // POST: Screens/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Width,Height,Diagonal,EffectiveColors,Type,Id")]
                                                Screen screen)
        {
            if(ModelState.IsValid)
            {
                _context.Add(screen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(screen);
        }

        // GET: Screens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            Screen screen = await _context.Screens.FindAsync(id);
            if(screen == null) return NotFound();

            return View(screen);
        }

        // POST: Screens/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Width,Height,Diagonal,EffectiveColors,Type,Id")]
                                              Screen screen)
        {
            if(id != screen.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(screen);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!ScreenExists(screen.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(screen);
        }

        // GET: Screens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            Screen screen = await _context.Screens.FirstOrDefaultAsync(m => m.Id == id);
            if(screen == null) return NotFound();

            return View(screen);
        }

        // POST: Screens/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Screen screen = await _context.Screens.FindAsync(id);
            _context.Screens.Remove(screen);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool ScreenExists(int id)
        {
            return _context.Screens.Any(e => e.Id == id);
        }
    }
}