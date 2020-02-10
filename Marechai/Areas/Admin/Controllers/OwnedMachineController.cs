using System.Linq;
using System.Threading.Tasks;
using Marechai.Areas.Admin.Models;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize]
    public class OwnedMachineController : Controller
    {
        readonly MarechaiContext _context;

        public OwnedMachineController(MarechaiContext context) => _context = context;

        // GET: OwnedMachine
        public async Task<IActionResult> Index()
        {
            IQueryable<OwnedMachineViewModel> marechaiContext = _context.
                                                                OwnedMachines.Include(o => o.Machine).
                                                                OrderBy(o => o.Machine.Company.Name).
                                                                ThenBy(o => o.Machine.Name).
                                                                ThenBy(o => o.User.UserName).
                                                                ThenBy(o => o.AcquisitionDate).
                                                                Select(o => new OwnedMachineViewModel
                                                                {
                                                                    AcquisitionDate = o.AcquisitionDate, Id = o.Id,
                                                                    Machine =
                                                                        $"{o.Machine.Company.Name} {o.Machine.Name}",
                                                                    Status = o.Status, User = o.User.UserName
                                                                });

            return View(await marechaiContext.ToListAsync());
        }

        // GET: OwnedMachine/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null)
                return NotFound();

            OwnedMachineViewModel ownedMachine =
                await _context.OwnedMachines.Include(o => o.Machine).Select(o => new OwnedMachineViewModel
                {
                    AcquisitionDate = o.AcquisitionDate, Boxed = o.Boxed,
                    LastStatusDate  = o.LastStatusDate,
                    LostDate        = o.LostDate,
                    Machine         = $"{o.Machine.Company.Name} {o.Machine.Name}", Manuals = o.Manuals,
                    SerialNumber    = o.SerialNumber, SerialNumberVisible                   = o.SerialNumberVisible,
                    Status          = o.Status,
                    User            = o.User.UserName, Id = o.Id
                }).FirstOrDefaultAsync(m => m.Id == id);

            if(ownedMachine == null)
                return NotFound();

            return View(ownedMachine);
        }

        // GET: OwnedMachine/Create
        public IActionResult Create()
        {
            ViewData["MachineId"] = new SelectList(_context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).
                                                            Select(m => new
                                                            {
                                                                m.Id, Name = $"{m.Company.Name} {m.Name}"
                                                            }), "Id", "Name");

            ViewData["UserId"] = new SelectList(_context.Users.OrderBy(u => u.UserName).Select(u => new
            {
                u.Id, u.UserName
            }), "Id", "UserName");

            return View();
        }

        // POST: OwnedMachine/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("AcquisitionDate,LostDate,Status,LastStatusDate,Trade,Boxed,Manuals,SerialNumber,SerialNumberVisible,MachineId,UserId,Id")]
            OwnedMachine ownedMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(ownedMachine);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"] = new SelectList(_context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).
                                                            Select(m => new
                                                            {
                                                                m.Id, Name = $"{m.Company.Name} {m.Name}"
                                                            }), "Id", "Name");

            ViewData["UserId"] = new SelectList(_context.Users.OrderBy(u => u.UserName).Select(u => new
            {
                u.Id, u.UserName
            }), "Id", "UserName");

            return View(ownedMachine);
        }

        // GET: OwnedMachine/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null)
                return NotFound();

            OwnedMachine ownedMachine = await _context.OwnedMachines.FindAsync(id);

            if(ownedMachine == null)
                return NotFound();

            ViewData["MachineId"] = new SelectList(_context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).
                                                            Select(m => new
                                                            {
                                                                m.Id, Name = $"{m.Company.Name} {m.Name}"
                                                            }), "Id", "Name");

            ViewData["UserId"] = new SelectList(_context.Users.OrderBy(u => u.UserName).
                                                         Where(u => u.Id == ownedMachine.UserId).Select(u => new
                                                         {
                                                             u.Id, u.UserName
                                                         }), "Id", "UserName");

            return View(ownedMachine);
        }

        // POST: OwnedMachine/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            long id,
            [Bind("AcquisitionDate,LostDate,Status,LastStatusDate,Trade,Boxed,Manuals,SerialNumber,SerialNumberVisible,MachineId,Id")]
            OwnedMachine ownedMachine)
        {
            if(id != ownedMachine.Id)
                return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(ownedMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!OwnedMachineExists(ownedMachine.Id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"] = new SelectList(_context.Machines.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).
                                                            Select(m => new
                                                            {
                                                                m.Id, Name = $"{m.Company.Name} {m.Name}"
                                                            }), "Id", "Name");

            ViewData["UserId"] = new SelectList(_context.Users.OrderBy(u => u.UserName).
                                                         Where(u => u.Id == ownedMachine.UserId).Select(u => new
                                                         {
                                                             u.Id, u.UserName
                                                         }), "Id", "UserName");

            return View(ownedMachine);
        }

        // GET: OwnedMachine/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null)
                return NotFound();

            OwnedMachineViewModel ownedMachine =
                await _context.OwnedMachines.Include(o => o.Machine).Select(o => new OwnedMachineViewModel
                {
                    AcquisitionDate = o.AcquisitionDate, Boxed = o.Boxed,
                    LastStatusDate  = o.LastStatusDate,
                    LostDate        = o.LostDate,
                    Machine         = $"{o.Machine.Company.Name} {o.Machine.Name}", Manuals = o.Manuals,
                    SerialNumber    = o.SerialNumber, SerialNumberVisible                   = o.SerialNumberVisible,
                    Status          = o.Status,
                    User            = o.User.UserName, Id = o.Id
                }).FirstOrDefaultAsync(m => m.Id == id);

            if(ownedMachine == null)
                return NotFound();

            return View(ownedMachine);
        }

        // POST: OwnedMachine/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            OwnedMachine ownedMachine = await _context.OwnedMachines.FindAsync(id);
            _context.OwnedMachines.Remove(ownedMachine);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        bool OwnedMachineExists(long id) => _context.OwnedMachines.Any(e => e.Id == id);
    }
}