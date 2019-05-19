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
    public class GpusByMachineController : Controller
    {
        readonly cicmContext _context;

        public GpusByMachineController(cicmContext context)
        {
            _context = context;
        }

        // GET: GpusByMachine
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<GpusByMachine, Machine> cicmContext =
                _context.GpusByMachine.Include(g => g.Gpu).Include(g => g.Machine);
            return View(await cicmContext.OrderBy(g => g.Machine.Name).ThenBy(g => g.Gpu.Name).ToListAsync());
        }

        // GET: GpusByMachine/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null) return NotFound();

            GpusByMachine gpusByMachine = await _context.GpusByMachine
                                                        .Include(g => g.Gpu).Include(g => g.Machine)
                                                        .FirstOrDefaultAsync(m => m.Id == id);
            if(gpusByMachine == null) return NotFound();

            return View(gpusByMachine);
        }

        // GET: GpusByMachine/Create
        public IActionResult Create()
        {
            ViewData["GpuId"]     = new SelectList(_context.Gpus.OrderBy(g => g.Name),     "Id", "Name");
            ViewData["MachineId"] = new SelectList(_context.Machines.OrderBy(m => m.Name), "Id", "Name");
            return View();
        }

        // POST: GpusByMachine/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GpuId,MachineId,Id")] GpusByMachine gpusByMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(gpusByMachine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["GpuId"] = new SelectList(_context.Gpus.OrderBy(g => g.Name), "Id", "Name", gpusByMachine.GpuId);
            ViewData["MachineId"] =
                new SelectList(_context.Machines.OrderBy(m => m.Name), "Id", "Name", gpusByMachine.MachineId);
            return View(gpusByMachine);
        }

        // GET: GpusByMachine/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null) return NotFound();

            GpusByMachine gpusByMachine = await _context.GpusByMachine.FindAsync(id);
            if(gpusByMachine == null) return NotFound();

            ViewData["GpuId"] = new SelectList(_context.Gpus.OrderBy(g => g.Name), "Id", "Name", gpusByMachine.GpuId);
            ViewData["MachineId"] =
                new SelectList(_context.Machines.OrderBy(m => m.Name), "Id", "Name", gpusByMachine.MachineId);
            return View(gpusByMachine);
        }

        // POST: GpusByMachine/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("GpuId,MachineId,Id")] GpusByMachine gpusByMachine)
        {
            if(id != gpusByMachine.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(gpusByMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!GpusByMachineExists(gpusByMachine.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["GpuId"] = new SelectList(_context.Gpus.OrderBy(g => g.Name), "Id", "Name", gpusByMachine.GpuId);
            ViewData["MachineId"] =
                new SelectList(_context.Machines.OrderBy(m => m.Name), "Id", "Name", gpusByMachine.MachineId);
            return View(gpusByMachine);
        }

        // GET: GpusByMachine/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null) return NotFound();

            GpusByMachine gpusByMachine = await _context.GpusByMachine
                                                        .Include(g => g.Gpu).Include(g => g.Machine)
                                                        .FirstOrDefaultAsync(m => m.Id == id);
            if(gpusByMachine == null) return NotFound();

            return View(gpusByMachine);
        }

        // POST: GpusByMachine/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            GpusByMachine gpusByMachine = await _context.GpusByMachine.FindAsync(id);
            _context.GpusByMachine.Remove(gpusByMachine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool GpusByMachineExists(long id)
        {
            return _context.GpusByMachine.Any(e => e.Id == id);
        }
    }
}