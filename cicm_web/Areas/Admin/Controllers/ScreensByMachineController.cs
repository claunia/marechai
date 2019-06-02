using System.Linq;
using System.Threading.Tasks;
using Cicm.Database.Models;
using cicm_web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ScreensByMachineController : Controller
    {
        readonly cicmContext _context;

        public ScreensByMachineController(cicmContext context)
        {
            _context = context;
        }

        // GET: ScreensByMachine
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<ScreensByMachine, Screen> cicmContext =
                _context.ScreensByMachine.Include(s => s.Machine).Include(s => s.Screen);
            return View(await cicmContext.Select(s => new ScreensByMachineViewModel
            {
                Id = s.Id,
                Screen = s.Screen.NativeResolution != null
                             ? $"{s.Screen.Diagonal}\" {s.Screen.Type} with {s.Screen.NativeResolution}"
                             : $"{s.Screen.Diagonal}\" {s.Screen}",
                Machine = $"{s.Machine.Company.Name} {s.Machine.Name}"
            }).OrderBy(s => s.Machine).ThenBy(s => s.Screen).ToListAsync());
        }

        // GET: ScreensByMachine/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null) return NotFound();

            ScreensByMachine screensByMachine = await _context.ScreensByMachine
                                                              .Include(s => s.Machine).Include(s => s.Screen)
                                                              .FirstOrDefaultAsync(m => m.Id == id);
            if(screensByMachine == null) return NotFound();

            return View(screensByMachine);
        }

        // GET: ScreensByMachine/Create
        public IActionResult Create()
        {
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name");
            ViewData["ScreenId"]  = new SelectList(_context.Screens,  "Id", "Type");
            return View();
        }

        // POST: ScreensByMachine/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ScreenId,MachineId,Id")] ScreensByMachine screensByMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(screensByMachine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", screensByMachine.MachineId);
            ViewData["ScreenId"]  = new SelectList(_context.Screens,  "Id", "Type", screensByMachine.ScreenId);
            return View(screensByMachine);
        }

        // GET: ScreensByMachine/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null) return NotFound();

            ScreensByMachine screensByMachine = await _context.ScreensByMachine.FindAsync(id);
            if(screensByMachine == null) return NotFound();

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", screensByMachine.MachineId);
            ViewData["ScreenId"]  = new SelectList(_context.Screens,  "Id", "Type", screensByMachine.ScreenId);
            return View(screensByMachine);
        }

        // POST: ScreensByMachine/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            long id, [Bind("ScreenId,MachineId,Id")] ScreensByMachine screensByMachine)
        {
            if(id != screensByMachine.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(screensByMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!ScreensByMachineExists(screensByMachine.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", screensByMachine.MachineId);
            ViewData["ScreenId"]  = new SelectList(_context.Screens,  "Id", "Type", screensByMachine.ScreenId);
            return View(screensByMachine);
        }

        // GET: ScreensByMachine/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null) return NotFound();

            ScreensByMachine screensByMachine = await _context.ScreensByMachine
                                                              .Include(s => s.Machine).Include(s => s.Screen)
                                                              .FirstOrDefaultAsync(m => m.Id == id);
            if(screensByMachine == null) return NotFound();

            return View(screensByMachine);
        }

        // POST: ScreensByMachine/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            ScreensByMachine screensByMachine = await _context.ScreensByMachine.FindAsync(id);
            _context.ScreensByMachine.Remove(screensByMachine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool ScreensByMachineExists(long id)
        {
            return _context.ScreensByMachine.Any(e => e.Id == id);
        }
    }
}