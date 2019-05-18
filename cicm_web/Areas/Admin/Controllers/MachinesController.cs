/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : MachinesController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Machines admin controller
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
    public class MachinesController : Controller
    {
        readonly cicmContext _context;

        public MachinesController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/Machines
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<Machine, MachineFamily> cicmContext =
                _context.Machines.Include(m => m.Company).Include(m => m.Family);
            return View(await cicmContext.ToListAsync());
        }

        // GET: Admin/Machines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            Machine machine = await _context.Machines.Include(m => m.Company).Include(m => m.Family)
                                            .FirstOrDefaultAsync(m => m.Id == id);
            if(machine == null) return NotFound();

            return View(machine);
        }

        // GET: Admin/Machines/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies,       "Id", "Name");
            ViewData["FamilyId"]  = new SelectList(_context.MachineFamilies, "Id", "Name");
            return View();
        }

        // POST: Admin/Machines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Name,Type,Introduced,FamilyId,Model")]
                                                Machine machine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(machine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies,       "Id", "Name", machine.CompanyId);
            ViewData["FamilyId"]  = new SelectList(_context.MachineFamilies, "Id", "Name", machine.FamilyId);
            return View(machine);
        }

        // GET: Admin/Machines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            Machine machine = await _context.Machines.FindAsync(id);
            if(machine == null) return NotFound();

            ViewData["CompanyId"] = new SelectList(_context.Companies,       "Id", "Name", machine.CompanyId);
            ViewData["FamilyId"]  = new SelectList(_context.MachineFamilies, "Id", "Name", machine.FamilyId);
            return View(machine);
        }

        // POST: Admin/Machines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Type,Introduced,FamilyId,Model")]
                                              Machine machine)
        {
            if(id != machine.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(machine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!MachineExists(machine.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies,       "Id", "Name", machine.CompanyId);
            ViewData["FamilyId"]  = new SelectList(_context.MachineFamilies, "Id", "Name", machine.FamilyId);
            return View(machine);
        }

        // GET: Admin/Machines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            Machine machine = await _context.Machines.Include(m => m.Company).Include(m => m.Family)
                                            .FirstOrDefaultAsync(m => m.Id == id);
            if(machine == null) return NotFound();

            return View(machine);
        }

        // POST: Admin/Machines/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Machine machine = await _context.Machines.FindAsync(id);
            _context.Machines.Remove(machine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool MachineExists(int id)
        {
            return _context.Machines.Any(e => e.Id == id);
        }
    }
}