using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Marechai.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ScreensByMachineController : Controller
    {
        readonly MarechaiContext _context;

        public ScreensByMachineController(MarechaiContext context)
        {
            _context = context;
        }

        // GET: ScreensByMachine
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<ScreensByMachine, Screen> marechaiContext =
                _context.ScreensByMachine.Include(s => s.Machine).Include(s => s.Screen);
            return View(await marechaiContext.Select(s => new ScreensByMachineViewModel
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

            ScreensByMachineViewModel screensByMachine = await _context.ScreensByMachine
                                                                       .Include(s => s.Machine).Include(s => s.Screen)
                                                                       .Select(s => new ScreensByMachineViewModel
                                                                        {
                                                                            Id = s.Id,
                                                                            Screen =
                                                                                s.Screen.NativeResolution !=
                                                                                null
                                                                                    ? $"{s.Screen.Diagonal}\" {s.Screen.Type} with {s.Screen.NativeResolution}"
                                                                                    : $"{s.Screen.Diagonal}\" {s.Screen}",
                                                                            Machine =
                                                                                $"{s.Machine.Company.Name} {s.Machine.Name}"
                                                                        }).FirstOrDefaultAsync(m => m.Id ==
                                                                                                    id);
            if(screensByMachine == null) return NotFound();

            return View(screensByMachine);
        }

        // GET: ScreensByMachine/Create
        public IActionResult Create()
        {
            ViewData["MachineId"] =
                new
                    SelectList(_context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).Select(m => new {m.Id, Name = $"{m.Company.Name} {m.Name}"}),
                               "Id", "Name");
            ViewData["ScreenId"] =
                new
                    SelectList(_context.Screens.Select(s => new {s.Id, Name = s.NativeResolution != null ? $"{s.Diagonal}\" {s.Type} with {s.NativeResolution}" : $"{s.Diagonal}\" {s.Type}"}).OrderBy(s => s.Name),
                               "Id", "Name");
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

            ViewData["MachineId"] =
                new
                    SelectList(_context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).Select(m => new {m.Id, Name = $"{m.Company.Name} {m.Name}"}),
                               "Id", "Name", screensByMachine.MachineId);
            ViewData["ScreenId"] =
                new
                    SelectList(_context.Screens.Select(s => new {s.Id, Name = s.NativeResolution != null ? $"{s.Diagonal}\" {s.Type} with {s.NativeResolution}" : $"{s.Diagonal}\" {s.Type}"}).OrderBy(s => s.Name),
                               "Id", "Name", screensByMachine.ScreenId);
            return View(screensByMachine);
        }

        // GET: ScreensByMachine/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null) return NotFound();

            ScreensByMachine screensByMachine = await _context.ScreensByMachine.FindAsync(id);
            if(screensByMachine == null) return NotFound();

            ViewData["MachineId"] =
                new
                    SelectList(_context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).Select(m => new {m.Id, Name = $"{m.Company.Name} {m.Name}"}),
                               "Id", "Name", screensByMachine.MachineId);
            ViewData["ScreenId"] =
                new
                    SelectList(_context.Screens.Select(s => new {s.Id, Name = s.NativeResolution != null ? $"{s.Diagonal}\" {s.Type} with {s.NativeResolution}" : $"{s.Diagonal}\" {s.Type}"}).OrderBy(s => s.Name),
                               "Id", "Name", screensByMachine.ScreenId);
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

            ViewData["MachineId"] =
                new
                    SelectList(_context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).Select(m => new {m.Id, Name = $"{m.Company.Name} {m.Name}"}),
                               "Id", "Name", screensByMachine.MachineId);
            ViewData["ScreenId"] =
                new
                    SelectList(_context.Screens.Select(s => new {s.Id, Name = s.NativeResolution != null ? $"{s.Diagonal}\" {s.Type} with {s.NativeResolution}" : $"{s.Diagonal}\" {s.Type}"}).OrderBy(s => s.Name),
                               "Id", "Name", screensByMachine.ScreenId);
            return View(screensByMachine);
        }

        // GET: ScreensByMachine/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null) return NotFound();

            ScreensByMachineViewModel screensByMachine = await _context.ScreensByMachine
                                                                       .Include(s => s.Machine).Include(s => s.Screen)
                                                                       .Select(s => new ScreensByMachineViewModel
                                                                        {
                                                                            Id = s.Id,
                                                                            Screen =
                                                                                s.Screen.NativeResolution !=
                                                                                null
                                                                                    ? $"{s.Screen.Diagonal}\" {s.Screen.Type} with {s.Screen.NativeResolution}"
                                                                                    : $"{s.Screen.Diagonal}\" {s.Screen}",
                                                                            Machine =
                                                                                $"{s.Machine.Company.Name} {s.Machine.Name}"
                                                                        }).FirstOrDefaultAsync(m => m.Id ==
                                                                                                    id);
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

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> VerifyUnique(int screenId, int machineId)
        {
            return await _context.ScreensByMachine.FirstOrDefaultAsync(i => i.ScreenId  == screenId &&
                                                                            i.MachineId == machineId) is null
                       ? Json(true)
                       : Json("The selected machine already has the selected screen.");
        }
    }
}