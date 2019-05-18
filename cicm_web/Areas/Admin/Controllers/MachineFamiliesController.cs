/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : MachineFamiliesController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Machine families admin controller
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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class MachineFamiliesController : Controller
    {
        readonly cicmContext _context;

        public MachineFamiliesController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/MachineFamilies
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<MachineFamily, Company> cicmContext = _context.MachineFamilies.Include(m => m.Company);
            return View(await cicmContext.OrderBy(m => m.Company.Name).ThenBy(m => m.Name).ToListAsync());
        }

        // GET: Admin/MachineFamilies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            MachineFamily machineFamily =
                await _context.MachineFamilies.Include(m => m.Company).FirstOrDefaultAsync(m => m.Id == id);
            if(machineFamily == null) return NotFound();

            return View(machineFamily);
        }

        // GET: Admin/MachineFamilies/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            return View();
        }

        // POST: Admin/MachineFamilies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Name")] MachineFamily machineFamily)
        {
            if(ModelState.IsValid)
            {
                _context.Add(machineFamily);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", machineFamily.CompanyId);
            return View(machineFamily);
        }

        // GET: Admin/MachineFamilies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            MachineFamily machineFamily = await _context.MachineFamilies.FindAsync(id);
            if(machineFamily == null) return NotFound();

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", machineFamily.CompanyId);
            return View(machineFamily);
        }

        // POST: Admin/MachineFamilies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name")] MachineFamily machineFamily)
        {
            if(id != machineFamily.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(machineFamily);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!MachineFamilyExists(machineFamily.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", machineFamily.CompanyId);
            return View(machineFamily);
        }

        // GET: Admin/MachineFamilies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            MachineFamily machineFamily =
                await _context.MachineFamilies.Include(m => m.Company).FirstOrDefaultAsync(m => m.Id == id);
            if(machineFamily == null) return NotFound();

            return View(machineFamily);
        }

        // POST: Admin/MachineFamilies/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            MachineFamily machineFamily = await _context.MachineFamilies.FindAsync(id);
            _context.MachineFamilies.Remove(machineFamily);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool MachineFamilyExists(int id)
        {
            return _context.MachineFamilies.Any(e => e.Id == id);
        }
    }
}