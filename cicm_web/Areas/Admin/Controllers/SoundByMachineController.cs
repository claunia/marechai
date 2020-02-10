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
    public class SoundByMachineController : Controller
    {
        readonly cicmContext _context;

        public SoundByMachineController(cicmContext context)
        {
            _context = context;
        }

        // GET: SoundByMachine
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<SoundByMachine, SoundSynth> cicmContext =
                _context.SoundByMachine.Include(s => s.Machine).Include(s => s.SoundSynth);
            return View(await cicmContext.OrderBy(s => s.Machine.Name).ThenBy(s => s.SoundSynth.Name)
                                         .Select(s => new SoundByMachineViewModel
                                          {
                                              Id         = s.Id,
                                              Machine    = s.Machine.Name,
                                              SoundSynth = s.SoundSynth.Name
                                          }).ToListAsync());
        }

        // GET: SoundByMachine/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null) return NotFound();

            SoundByMachine soundByMachine = await _context.SoundByMachine
                                                          .Include(s => s.Machine).Include(s => s.SoundSynth)
                                                          .FirstOrDefaultAsync(m => m.Id == id);
            if(soundByMachine == null) return NotFound();

            return View(soundByMachine);
        }

        // GET: SoundByMachine/Create
        public IActionResult Create()
        {
            ViewData["MachineId"]    = new SelectList(_context.Machines,    "Id", "Name");
            ViewData["SoundSynthId"] = new SelectList(_context.SoundSynths, "Id", "Name");
            return View();
        }

        // POST: SoundByMachine/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SoundSynthId,MachineId,Id")] SoundByMachine soundByMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(soundByMachine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"]    = new SelectList(_context.Machines,    "Id", "Name", soundByMachine.MachineId);
            ViewData["SoundSynthId"] = new SelectList(_context.SoundSynths, "Id", "Name", soundByMachine.SoundSynthId);
            return View(soundByMachine);
        }

        // GET: SoundByMachine/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null) return NotFound();

            SoundByMachine soundByMachine = await _context.SoundByMachine.FindAsync(id);
            if(soundByMachine == null) return NotFound();

            ViewData["MachineId"]    = new SelectList(_context.Machines,    "Id", "Name", soundByMachine.MachineId);
            ViewData["SoundSynthId"] = new SelectList(_context.SoundSynths, "Id", "Name", soundByMachine.SoundSynthId);
            return View(soundByMachine);
        }

        // POST: SoundByMachine/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            long id, [Bind("SoundSynthId,MachineId,Id")] SoundByMachine soundByMachine)
        {
            if(id != soundByMachine.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(soundByMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!SoundByMachineExists(soundByMachine.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"]    = new SelectList(_context.Machines,    "Id", "Name", soundByMachine.MachineId);
            ViewData["SoundSynthId"] = new SelectList(_context.SoundSynths, "Id", "Name", soundByMachine.SoundSynthId);
            return View(soundByMachine);
        }

        // GET: SoundByMachine/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null) return NotFound();

            SoundByMachine soundByMachine = await _context.SoundByMachine
                                                          .Include(s => s.Machine).Include(s => s.SoundSynth)
                                                          .FirstOrDefaultAsync(m => m.Id == id);
            if(soundByMachine == null) return NotFound();

            return View(soundByMachine);
        }

        // POST: SoundByMachine/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            SoundByMachine soundByMachine = await _context.SoundByMachine.FindAsync(id);
            _context.SoundByMachine.Remove(soundByMachine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool SoundByMachineExists(long id)
        {
            return _context.SoundByMachine.Any(e => e.Id == id);
        }
    }
}