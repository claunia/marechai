/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : SoundSynthsController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Sound synths admin controller
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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

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
    public class SoundSynthsController : Controller
    {
        readonly MarechaiContext _context;

        public SoundSynthsController(MarechaiContext context) => _context = context;

        // GET: Admin/SoundSynths
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<SoundSynth, Company> marechaiContext = _context.SoundSynths.Include(s => s.Company);

            return View(await marechaiContext.OrderBy(s => s.Company).ThenBy(s => s.Name).ThenBy(s => s.ModelCode).
                                              Select(s => new SoundSynthViewModel
                                              {
                                                  Company   = s.Company.Name, Id = s.Id, Introduced = s.Introduced,
                                                  ModelCode = s.ModelCode, Name  = s.Name, Type     = s.Type
                                              }).ToListAsync());
        }

        // GET: Admin/SoundSynths/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
                return NotFound();

            SoundSynth soundSynth =
                await _context.SoundSynths.Include(s => s.Company).FirstOrDefaultAsync(m => m.Id == id);

            if(soundSynth == null)
                return NotFound();

            return View(soundSynth);
        }

        // GET: Admin/SoundSynths/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");

            return View();
        }

        // POST: Admin/SoundSynths/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,CompanyId,ModelCode,Introduced,Voices,Frequency,Depth,SquareWave,WhiteNoise,Type")]
            SoundSynth soundSynth)
        {
            if(ModelState.IsValid)
            {
                _context.Add(soundSynth);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", soundSynth.CompanyId);

            return View(soundSynth);
        }

        // GET: Admin/SoundSynths/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
                return NotFound();

            SoundSynth soundSynth = await _context.SoundSynths.FindAsync(id);

            if(soundSynth == null)
                return NotFound();

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", soundSynth.CompanyId);

            return View(soundSynth);
        }

        // POST: Admin/SoundSynths/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, [Bind("Id,Name,CompanyId,ModelCode,Introduced,Voices,Frequency,Depth,SquareWave,WhiteNoise,Type")]
            SoundSynth soundSynth)
        {
            if(id != soundSynth.Id)
                return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(soundSynth);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!SoundSynthExists(soundSynth.Id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", soundSynth.CompanyId);

            return View(soundSynth);
        }

        // GET: Admin/SoundSynths/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
                return NotFound();

            SoundSynth soundSynth =
                await _context.SoundSynths.Include(s => s.Company).FirstOrDefaultAsync(m => m.Id == id);

            if(soundSynth == null)
                return NotFound();

            return View(soundSynth);
        }

        // POST: Admin/SoundSynths/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            SoundSynth soundSynth = await _context.SoundSynths.FindAsync(id);
            _context.SoundSynths.Remove(soundSynth);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        bool SoundSynthExists(int id) => _context.SoundSynths.Any(e => e.Id == id);
    }
}