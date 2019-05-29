using System.Linq;
using System.Threading.Tasks;
using Cicm.Database.Models;
using cicm_web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OwnedMachineController : Controller
    {
        private readonly cicmContext _context;

        public OwnedMachineController(cicmContext context)
        {
            _context = context;
        }

        // GET: OwnedMachine
        public async Task<IActionResult> Index()
        {
            var cicmContext = _context.OwnedMachines.Include(o => o.Machine).OrderBy(o => o.Machine.Company.Name).ThenBy(o => o.Machine.Name).ThenBy(o => o.User.UserName).ThenBy(o => o.AcquisitionDate).Select(o => new OwnedMachineViewModel
            {
                AcquisitionDate = o.AcquisitionDate,
                Id = o.Id,
                Machine = $"{o.Machine.Company.Name} {o.Machine.Name}",
                Status = o.Status,
                User = o.User.UserName
            });

            return View(await cicmContext.ToListAsync());
        }

        // GET: OwnedMachine/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ownedMachine = await _context.OwnedMachines
                .Include(o => o.Machine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ownedMachine == null)
            {
                return NotFound();
            }

            return View(ownedMachine);
        }

        // GET: OwnedMachine/Create
        public IActionResult Create()
        {
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name");
            return View();
        }

        // POST: OwnedMachine/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AcquisitionDate,LostDate,Status,LastStatusDate,Trade,Boxed,Manuals,SerialNumber,SerialNumberVisible,MachineId,Id")] OwnedMachine ownedMachine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ownedMachine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", ownedMachine.MachineId);
            return View(ownedMachine);
        }

        // GET: OwnedMachine/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ownedMachine = await _context.OwnedMachines.FindAsync(id);
            if (ownedMachine == null)
            {
                return NotFound();
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", ownedMachine.MachineId);
            return View(ownedMachine);
        }

        // POST: OwnedMachine/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("AcquisitionDate,LostDate,Status,LastStatusDate,Trade,Boxed,Manuals,SerialNumber,SerialNumberVisible,MachineId,Id")] OwnedMachine ownedMachine)
        {
            if (id != ownedMachine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ownedMachine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OwnedMachineExists(ownedMachine.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", ownedMachine.MachineId);
            return View(ownedMachine);
        }

        // GET: OwnedMachine/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ownedMachine = await _context.OwnedMachines
                .Include(o => o.Machine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ownedMachine == null)
            {
                return NotFound();
            }

            return View(ownedMachine);
        }

        // POST: OwnedMachine/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ownedMachine = await _context.OwnedMachines.FindAsync(id);
            _context.OwnedMachines.Remove(ownedMachine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OwnedMachineExists(long id)
        {
            return _context.OwnedMachines.Any(e => e.Id == id);
        }
    }
}
