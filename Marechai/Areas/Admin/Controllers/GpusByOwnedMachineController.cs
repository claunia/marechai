using System.Linq;
using System.Threading.Tasks;
using Marechai.Areas.Admin.Models;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Marechai.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize]
    public class GpusByOwnedMachineController : Controller
    {
        readonly MarechaiContext _context;

        public GpusByOwnedMachineController(MarechaiContext context) => _context = context;

        // GET: GpusByOwnedMachine
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<GpusByOwnedMachine, OwnedMachine> marechaiContext =
                _context.GpusByOwnedMachine.Include(g => g.Gpu).Include(g => g.OwnedMachine);

            return View(await marechaiContext.OrderBy(g => g.OwnedMachine.Machine.Name).ThenBy(g => g.Gpu.Name).
                                              Select(g => new GpusByMachineViewModel
                                              {
                                                  Id = g.Id, Gpu = g.Gpu.Name,
                                                  Machine =
                                                      $"{g.OwnedMachine.Machine.Company.Name} {g.OwnedMachine.Machine.Name} <{g.OwnedMachine.User.UserName}>"
                                              }).ToListAsync());
        }

        // GET: GpusByOwnedMachine/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null)
                return NotFound();

            GpusByMachineViewModel gpusByOwnedMachine =
                await _context.GpusByOwnedMachine.Include(g => g.Gpu).Include(g => g.OwnedMachine).
                               Select(o => new GpusByMachineViewModel
                               {
                                   Gpu = o.Gpu.Name, Id = o.Id,
                                   Machine =
                                       $"{o.OwnedMachine.Machine.Company.Name} {o.OwnedMachine.Machine.Name} <{o.OwnedMachine.User.UserName}>"
                               }).FirstOrDefaultAsync(m => m.Id == id);

            if(gpusByOwnedMachine == null)
                return NotFound();

            return View(gpusByOwnedMachine);
        }

        // GET: GpusByOwnedMachine/Create
        public IActionResult Create()
        {
            ViewData["GpuId"] = new SelectList(_context.Gpus.OrderBy(g => g.Name), "Id", "Name");

            ViewData["OwnedMachineId"] = new SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).
                                                                 ThenBy(m => m.Machine.Name).
                                                                 ThenBy(m => m.User.UserName).Select(m => new
                                                                 {
                                                                     m.Id,
                                                                     Name =
                                                                         $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"
                                                                 }), "Id", "Name");

            return View();
        }

        // POST: GpusByOwnedMachine/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GpuId,OwnedMachineId,Id")] GpusByOwnedMachine gpusByOwnedMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(gpusByOwnedMachine);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["GpuId"] =
                new SelectList(_context.Gpus.OrderBy(g => g.Name), "Id", "Name", gpusByOwnedMachine.GpuId);

            ViewData["OwnedMachineId"] = new SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).
                                                                 ThenBy(m => m.Machine.Name).
                                                                 ThenBy(m => m.User.UserName).Select(m => new
                                                                 {
                                                                     m.Id,
                                                                     Name =
                                                                         $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"
                                                                 }), "Id", "Name", gpusByOwnedMachine.OwnedMachineId);

            return View(gpusByOwnedMachine);
        }

        // GET: GpusByOwnedMachine/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null)
                return NotFound();

            GpusByOwnedMachine gpusByOwnedMachine = await _context.GpusByOwnedMachine.FindAsync(id);

            if(gpusByOwnedMachine == null)
                return NotFound();

            ViewData["GpuId"] =
                new SelectList(_context.Gpus.OrderBy(g => g.Name), "Id", "Name", gpusByOwnedMachine.GpuId);

            ViewData["OwnedMachineId"] = new SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).
                                                                 ThenBy(m => m.Machine.Name).
                                                                 ThenBy(m => m.User.UserName).Select(m => new
                                                                 {
                                                                     m.Id,
                                                                     Name =
                                                                         $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"
                                                                 }), "Id", "Name", gpusByOwnedMachine.OwnedMachineId);

            return View(gpusByOwnedMachine);
        }

        // POST: GpusByOwnedMachine/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            long id, [Bind("GpuId,OwnedMachineId,Id")] GpusByOwnedMachine gpusByOwnedMachine)
        {
            if(id != gpusByOwnedMachine.Id)
                return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(gpusByOwnedMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!GpusByOwnedMachineExists(gpusByOwnedMachine.Id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["GpuId"] =
                new SelectList(_context.Gpus.OrderBy(g => g.Name), "Id", "Name", gpusByOwnedMachine.GpuId);

            ViewData["OwnedMachineId"] = new SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).
                                                                 ThenBy(m => m.Machine.Name).
                                                                 ThenBy(m => m.User.UserName).Select(m => new
                                                                 {
                                                                     m.Id,
                                                                     Name =
                                                                         $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"
                                                                 }), "Id", "Name", gpusByOwnedMachine.OwnedMachineId);

            return View(gpusByOwnedMachine);
        }

        // GET: GpusByOwnedMachine/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null)
                return NotFound();

            GpusByMachineViewModel gpusByOwnedMachine =
                await _context.GpusByOwnedMachine.Include(g => g.Gpu).Include(g => g.OwnedMachine).
                               Select(o => new GpusByMachineViewModel
                               {
                                   Gpu = o.Gpu.Name, Id = o.Id,
                                   Machine =
                                       $"{o.OwnedMachine.Machine.Company.Name} {o.OwnedMachine.Machine.Name} <{o.OwnedMachine.User.UserName}>"
                               }).FirstOrDefaultAsync(m => m.Id == id);

            if(gpusByOwnedMachine == null)
                return NotFound();

            return View(gpusByOwnedMachine);
        }

        // POST: GpusByOwnedMachine/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            GpusByOwnedMachine gpusByOwnedMachine = await _context.GpusByOwnedMachine.FindAsync(id);
            _context.GpusByOwnedMachine.Remove(gpusByOwnedMachine);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        bool GpusByOwnedMachineExists(long id) => _context.GpusByOwnedMachine.Any(e => e.Id == id);
    }
}