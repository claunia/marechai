/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : GpusController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     GPUs admin controller
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using System.Linq;
using System.Threading.Tasks;
using Cicm.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GpusController : Controller
    {
        readonly cicmContext _context;

        public GpusController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/Gpus
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<Gpu, Company> cicmContext = _context.Gpus.Include(g => g.Company);
            return View(await cicmContext.ToListAsync());
        }

        // GET: Admin/Gpus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            Gpu gpu = await _context.Gpus.Include(g => g.Company).FirstOrDefaultAsync(m => m.Id == id);
            if(gpu == null) return NotFound();

            return View(gpu);
        }

        // GET: Admin/Gpus/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            return View();
        }

        // POST: Admin/Gpus/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,CompanyId,ModelCode,Introduced,Package,Process,ProcessNm,DieSize,Transistors")]
            Gpu gpu)
        {
            if(ModelState.IsValid)
            {
                _context.Add(gpu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", gpu.CompanyId);
            return View(gpu);
        }

        // GET: Admin/Gpus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            Gpu gpu = await _context.Gpus.FindAsync(id);
            if(gpu == null) return NotFound();

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", gpu.CompanyId);
            return View(gpu);
        }

        // POST: Admin/Gpus/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, [Bind("Id,Name,CompanyId,ModelCode,Introduced,Package,Process,ProcessNm,DieSize,Transistors")]
            Gpu gpu)
        {
            if(id != gpu.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(gpu);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!GpuExists(gpu.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", gpu.CompanyId);
            return View(gpu);
        }

        // GET: Admin/Gpus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            Gpu gpu = await _context.Gpus.Include(g => g.Company).FirstOrDefaultAsync(m => m.Id == id);
            if(gpu == null) return NotFound();

            return View(gpu);
        }

        // POST: Admin/Gpus/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Gpu gpu = await _context.Gpus.FindAsync(id);
            _context.Gpus.Remove(gpu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool GpuExists(int id)
        {
            return _context.Gpus.Any(e => e.Id == id);
        }
    }
}