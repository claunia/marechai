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
    public class ResolutionsByGpuController : Controller
    {
        readonly cicmContext _context;

        public ResolutionsByGpuController(cicmContext context)
        {
            _context = context;
        }

        // GET: ResolutionsByGpu
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<ResolutionsByGpu, Resolution> cicmContext =
                _context.ResolutionsByGpu.Include(r => r.Gpu).Include(r => r.Resolution);
            return View(await cicmContext.OrderBy(r => r.Gpu.Company.Name).ThenBy(r => r.Gpu.Name)
                                         .ThenBy(r => r.Resolution.Chars).ThenBy(r => r.Resolution.Width)
                                         .ThenBy(r => r.Resolution.Height).ThenBy(r => r.Resolution.Colors)
                                         .Select(r => new ResolutionsByGpuViewModel
                                          {
                                              Gpu        = r.Gpu.Name,
                                              GpuCompany = r.Gpu.Company.Name,
                                              Id         = r.Id,
                                              Resolution = r.Resolution
                                          }).ToListAsync());
        }

        // GET: ResolutionsByGpu/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null) return NotFound();

            ResolutionsByGpu resolutionsByGpu = await _context.ResolutionsByGpu
                                                              .Include(r => r.Gpu).Include(r => r.Resolution)
                                                              .FirstOrDefaultAsync(m => m.Id == id);
            if(resolutionsByGpu == null) return NotFound();

            return View(resolutionsByGpu);
        }

        // GET: ResolutionsByGpu/Create
        public IActionResult Create()
        {
            ViewData["GpuId"] =
                new
                    SelectList(_context.Gpus.OrderBy(r => r.Company.Name).ThenBy(r => r.Name).Select(g => new {g.Id, Name = $"{g.Company.Name} {g.Name}"}),
                               "Id", "Name");
            ViewData["ResolutionId"] =
                new
                    SelectList(_context.Resolutions.OrderBy(r => r.Chars).ThenBy(r => r.Width).ThenBy(r => r.Height).ThenBy(r => r.Colors).Select(r => new {r.Id, Name = r.ToString()}),
                               "Id", "Name");
            return View();
        }

        // POST: ResolutionsByGpu/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GpuId,ResolutionId,Id")] ResolutionsByGpu resolutionsByGpu)
        {
            if(ModelState.IsValid)
            {
                _context.Add(resolutionsByGpu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["GpuId"] =
                new
                    SelectList(_context.Gpus.OrderBy(r => r.Company.Name).ThenBy(r => r.Name).Select(g => new {g.Id, Name = $"{g.Company.Name} {g.Name}"}),
                               "Id", "Name", resolutionsByGpu.GpuId);
            ViewData["ResolutionId"] =
                new
                    SelectList(_context.Resolutions.OrderBy(r => r.Chars).ThenBy(r => r.Width).ThenBy(r => r.Height).ThenBy(r => r.Colors).Select(r => new {r.Id, Name = r.ToString()}),
                               "Id", "Name", resolutionsByGpu.ResolutionId);
            return View(resolutionsByGpu);
        }

        // GET: ResolutionsByGpu/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null) return NotFound();

            ResolutionsByGpu resolutionsByGpu = await _context.ResolutionsByGpu.FindAsync(id);
            if(resolutionsByGpu == null) return NotFound();

            ViewData["GpuId"] =
                new
                    SelectList(_context.Gpus.OrderBy(r => r.Company.Name).ThenBy(r => r.Name).Select(g => new {g.Id, Name = $"{g.Company.Name} {g.Name}"}),
                               "Id", "Name", resolutionsByGpu.GpuId);
            ViewData["ResolutionId"] =
                new
                    SelectList(_context.Resolutions.OrderBy(r => r.Chars).ThenBy(r => r.Width).ThenBy(r => r.Height).ThenBy(r => r.Colors).Select(r => new {r.Id, Name = r.ToString()}),
                               "Id", "Name", resolutionsByGpu.ResolutionId);
            return View(resolutionsByGpu);
        }

        // POST: ResolutionsByGpu/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            long id, [Bind("GpuId,ResolutionId,Id")] ResolutionsByGpu resolutionsByGpu)
        {
            if(id != resolutionsByGpu.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(resolutionsByGpu);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!ResolutionsByGpuExists(resolutionsByGpu.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["GpuId"] =
                new
                    SelectList(_context.Gpus.OrderBy(r => r.Company.Name).ThenBy(r => r.Name).Select(g => new {g.Id, Name = $"{g.Company.Name} {g.Name}"}),
                               "Id", "Name", resolutionsByGpu.GpuId);
            ViewData["ResolutionId"] =
                new
                    SelectList(_context.Resolutions.OrderBy(r => r.Chars).ThenBy(r => r.Width).ThenBy(r => r.Height).ThenBy(r => r.Colors).Select(r => new {r.Id, Name = r.ToString()}),
                               "Id", "Name", resolutionsByGpu.ResolutionId);
            return View(resolutionsByGpu);
        }

        // GET: ResolutionsByGpu/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null) return NotFound();

            ResolutionsByGpu resolutionsByGpu = await _context.ResolutionsByGpu
                                                              .Include(r => r.Gpu).Include(r => r.Resolution)
                                                              .FirstOrDefaultAsync(m => m.Id == id);
            if(resolutionsByGpu == null) return NotFound();

            return View(resolutionsByGpu);
        }

        // POST: ResolutionsByGpu/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            ResolutionsByGpu resolutionsByGpu = await _context.ResolutionsByGpu.FindAsync(id);
            _context.ResolutionsByGpu.Remove(resolutionsByGpu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool ResolutionsByGpuExists(long id)
        {
            return _context.ResolutionsByGpu.Any(e => e.Id == id);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> VerifyUnique(int gpuId, int resolutionId)
        {
            return await _context.ResolutionsByGpu.FirstOrDefaultAsync(i => i.GpuId        == gpuId &&
                                                                            i.ResolutionId == resolutionId) is null
                       ? Json(true)
                       : Json("The selected GPU already has the selected resolution.");
        }
    }
}