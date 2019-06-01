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
    public class ResolutionsByScreenController : Controller
    {
        readonly cicmContext _context;

        public ResolutionsByScreenController(cicmContext context)
        {
            _context = context;
        }

        // GET: ResolutionsByScreen
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<ResolutionsByScreen, Screen> cicmContext =
                _context.ResolutionsByScreen.Include(r => r.Resolution).Include(r => r.Screen);
            return View(await cicmContext.ToListAsync());
        }

        // GET: ResolutionsByScreen/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null) return NotFound();

            ResolutionsByScreen resolutionsByScreen = await _context.ResolutionsByScreen
                                                                    .Include(r => r.Resolution).Include(r => r.Screen)
                                                                    .FirstOrDefaultAsync(m => m.Id == id);
            if(resolutionsByScreen == null) return NotFound();

            return View(resolutionsByScreen);
        }

        // GET: ResolutionsByScreen/Create
        public IActionResult Create()
        {
            ViewData["ResolutionId"] = new SelectList(_context.Resolutions, "Id", "Id");
            ViewData["ScreenId"]     = new SelectList(_context.Screens,     "Id", "Type");
            return View();
        }

        // POST: ResolutionsByScreen/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ScreenId,ResolutionId,Id")] ResolutionsByScreen resolutionsByScreen)
        {
            if(ModelState.IsValid)
            {
                _context.Add(resolutionsByScreen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ResolutionId"] =
                new SelectList(_context.Resolutions, "Id", "Id", resolutionsByScreen.ResolutionId);
            ViewData["ScreenId"] = new SelectList(_context.Screens, "Id", "Type", resolutionsByScreen.ScreenId);
            return View(resolutionsByScreen);
        }

        // GET: ResolutionsByScreen/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null) return NotFound();

            ResolutionsByScreen resolutionsByScreen = await _context.ResolutionsByScreen.FindAsync(id);
            if(resolutionsByScreen == null) return NotFound();

            ViewData["ResolutionId"] =
                new SelectList(_context.Resolutions, "Id", "Id", resolutionsByScreen.ResolutionId);
            ViewData["ScreenId"] = new SelectList(_context.Screens, "Id", "Type", resolutionsByScreen.ScreenId);
            return View(resolutionsByScreen);
        }

        // POST: ResolutionsByScreen/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            long id, [Bind("ScreenId,ResolutionId,Id")] ResolutionsByScreen resolutionsByScreen)
        {
            if(id != resolutionsByScreen.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(resolutionsByScreen);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!ResolutionsByScreenExists(resolutionsByScreen.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ResolutionId"] =
                new SelectList(_context.Resolutions, "Id", "Id", resolutionsByScreen.ResolutionId);
            ViewData["ScreenId"] = new SelectList(_context.Screens, "Id", "Type", resolutionsByScreen.ScreenId);
            return View(resolutionsByScreen);
        }

        // GET: ResolutionsByScreen/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null) return NotFound();

            ResolutionsByScreen resolutionsByScreen = await _context.ResolutionsByScreen
                                                                    .Include(r => r.Resolution).Include(r => r.Screen)
                                                                    .FirstOrDefaultAsync(m => m.Id == id);
            if(resolutionsByScreen == null) return NotFound();

            return View(resolutionsByScreen);
        }

        // POST: ResolutionsByScreen/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            ResolutionsByScreen resolutionsByScreen = await _context.ResolutionsByScreen.FindAsync(id);
            _context.ResolutionsByScreen.Remove(resolutionsByScreen);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool ResolutionsByScreenExists(long id)
        {
            return _context.ResolutionsByScreen.Any(e => e.Id == id);
        }
    }
}