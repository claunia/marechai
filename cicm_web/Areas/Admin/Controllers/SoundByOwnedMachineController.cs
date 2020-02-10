using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
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
    public class SoundByOwnedMachineController : Controller
    {
        readonly cicmContext _context;

        public SoundByOwnedMachineController(cicmContext context)
        {
            _context = context;
        }

        // GET: SoundByOwnedMachine
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<SoundByOwnedMachine, SoundSynth> cicmContext =
                _context.SoundByOwnedMachine.Include(s => s.OwnedMachine).Include(s => s.SoundSynth);
            return View(await cicmContext.OrderBy(s => s.OwnedMachine.Machine.Company.Name)
                                         .ThenBy(s => s.OwnedMachine.Machine.Name)
                                         .ThenBy(s => s.OwnedMachine.User.UserName).ThenBy(s => s.SoundSynth.Name)
                                         .Select(s => new SoundByMachineViewModel
                                          {
                                              Id = s.Id,
                                              Machine =
                                                  $"{s.OwnedMachine.Machine.Company.Name} {s.OwnedMachine.Machine.Name} <{s.OwnedMachine.User.UserName}>",
                                              SoundSynth = s.SoundSynth.Name
                                          }).ToListAsync());
        }

        // GET: SoundByOwnedMachine/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null) return NotFound();

            SoundByMachineViewModel soundByOwnedMachine = await _context.SoundByOwnedMachine
                                                                        .OrderBy(s => s.OwnedMachine.Machine.Company
                                                                                       .Name)
                                                                        .ThenBy(s => s.OwnedMachine.Machine.Name)
                                                                        .ThenBy(s => s.OwnedMachine.User.UserName)
                                                                        .ThenBy(s => s.SoundSynth.Name)
                                                                        .Select(s => new SoundByMachineViewModel
                                                                         {
                                                                             Id = s.Id,
                                                                             Machine =
                                                                                 $"{s.OwnedMachine.Machine.Company.Name} {s.OwnedMachine.Machine.Name} <{s.OwnedMachine.User.UserName}>",
                                                                             SoundSynth = s.SoundSynth.Name
                                                                         }).FirstOrDefaultAsync(m => m.Id ==
                                                                                                     id);
            if(soundByOwnedMachine == null) return NotFound();

            return View(soundByOwnedMachine);
        }

        // GET: SoundByOwnedMachine/Create
        public IActionResult Create()
        {
            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name");
            ViewData["SoundSynthId"] = new SelectList(_context.SoundSynths, "Id", "Name");
            return View();
        }

        // POST: SoundByOwnedMachine/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SoundSynthId,OwnedMachineId,Id")]
                                                SoundByOwnedMachine soundByOwnedMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(soundByOwnedMachine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name", soundByOwnedMachine.OwnedMachineId);
            ViewData["SoundSynthId"] =
                new SelectList(_context.SoundSynths, "Id", "Name", soundByOwnedMachine.SoundSynthId);
            return View(soundByOwnedMachine);
        }

        // GET: SoundByOwnedMachine/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null) return NotFound();

            SoundByOwnedMachine soundByOwnedMachine = await _context.SoundByOwnedMachine.FindAsync(id);
            if(soundByOwnedMachine == null) return NotFound();

            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name", soundByOwnedMachine.OwnedMachineId);
            ViewData["SoundSynthId"] =
                new SelectList(_context.SoundSynths, "Id", "Name", soundByOwnedMachine.SoundSynthId);
            return View(soundByOwnedMachine);
        }

        // POST: SoundByOwnedMachine/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("SoundSynthId,OwnedMachineId,Id")]
                                              SoundByOwnedMachine soundByOwnedMachine)
        {
            if(id != soundByOwnedMachine.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(soundByOwnedMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!SoundByOwnedMachineExists(soundByOwnedMachine.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name", soundByOwnedMachine.OwnedMachineId);
            ViewData["SoundSynthId"] =
                new SelectList(_context.SoundSynths, "Id", "Name", soundByOwnedMachine.SoundSynthId);
            return View(soundByOwnedMachine);
        }

        // GET: SoundByOwnedMachine/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null) return NotFound();

            SoundByMachineViewModel soundByOwnedMachine = await _context.SoundByOwnedMachine
                                                                        .OrderBy(s => s.OwnedMachine.Machine.Company
                                                                                       .Name)
                                                                        .ThenBy(s => s.OwnedMachine.Machine.Name)
                                                                        .ThenBy(s => s.OwnedMachine.User.UserName)
                                                                        .ThenBy(s => s.SoundSynth.Name)
                                                                        .Select(s => new SoundByMachineViewModel
                                                                         {
                                                                             Id = s.Id,
                                                                             Machine =
                                                                                 $"{s.OwnedMachine.Machine.Company.Name} {s.OwnedMachine.Machine.Name} <{s.OwnedMachine.User.UserName}>",
                                                                             SoundSynth = s.SoundSynth.Name
                                                                         }).FirstOrDefaultAsync(m => m.Id ==
                                                                                                     id);
            if(soundByOwnedMachine == null) return NotFound();

            return View(soundByOwnedMachine);
        }

        // POST: SoundByOwnedMachine/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            SoundByOwnedMachine soundByOwnedMachine = await _context.SoundByOwnedMachine.FindAsync(id);
            _context.SoundByOwnedMachine.Remove(soundByOwnedMachine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool SoundByOwnedMachineExists(long id)
        {
            return _context.SoundByOwnedMachine.Any(e => e.Id == id);
        }
    }
}