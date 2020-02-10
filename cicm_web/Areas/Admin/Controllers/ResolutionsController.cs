/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : ResolutionsController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Resolutions admin controller
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ResolutionsController : Controller
    {
        readonly cicmContext _context;

        public ResolutionsController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/Resolutions
        public async Task<IActionResult> Index() =>
            View(await _context.Resolutions.OrderBy(r => r.Chars).ThenBy(r => r.Width).ThenBy(r => r.Height)
                               .ThenBy(r => r.Colors).ThenBy(r => r.Grayscale).ToListAsync());

        // GET: Admin/Resolutions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            Resolution resolution = await _context.Resolutions.FirstOrDefaultAsync(m => m.Id == id);
            if(resolution == null) return NotFound();

            return View(resolution);
        }

        // GET: Admin/Resolutions/Create
        public IActionResult Create() => View();

        // POST: Admin/Resolutions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Width,Height,Colors,Palette,Chars,Grayscale")]
                                                Resolution resolution)
        {
            if(ModelState.IsValid)
            {
                _context.Add(resolution);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(resolution);
        }

        // GET: Admin/Resolutions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            Resolution resolution = await _context.Resolutions.FindAsync(id);
            if(resolution == null) return NotFound();

            return View(resolution);
        }

        // POST: Admin/Resolutions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Width,Height,Colors,Palette,Chars,Grayscale")]
                                              Resolution resolution)
        {
            if(id != resolution.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(resolution);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!ResolutionExists(resolution.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(resolution);
        }

        // GET: Admin/Resolutions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            Resolution resolution = await _context.Resolutions.FirstOrDefaultAsync(m => m.Id == id);
            if(resolution == null) return NotFound();

            return View(resolution);
        }

        // POST: Admin/Resolutions/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Resolution resolution = await _context.Resolutions.FindAsync(id);
            _context.Resolutions.Remove(resolution);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool ResolutionExists(int id)
        {
            return _context.Resolutions.Any(e => e.Id == id);
        }
    }
}