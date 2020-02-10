/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : StorageByMachinesController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Storage by machines admin controller
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
using Marechai.Database.Models;
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
    public class StorageByMachinesController : Controller
    {
        readonly cicmContext _context;

        public StorageByMachinesController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/StorageByMachines
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<StorageByMachine, Machine> cicmContext =
                _context.StorageByMachine.Include(s => s.Machine);
            return View(await cicmContext.OrderBy(s => s.Machine.Company.Name).ThenBy(s => s.Machine.Name)
                                         .Select(s => new StorageByMachineViewModel
                                          {
                                              Id        = s.Id,
                                              Company   = s.Machine.Company.Name,
                                              Machine   = s.Machine.Name,
                                              Type      = s.Type,
                                              Interface = s.Interface,
                                              Capacity  = s.Capacity
                                          }).ToListAsync());
        }

        // GET: Admin/StorageByMachines/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null) return NotFound();

            StorageByMachine storageByMachine =
                await _context.StorageByMachine.Include(s => s.Machine).FirstOrDefaultAsync(m => m.Id == id);
            if(storageByMachine == null) return NotFound();

            return View(storageByMachine);
        }

        // GET: Admin/StorageByMachines/Create
        public IActionResult Create()
        {
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name");
            return View();
        }

        // POST: Admin/StorageByMachines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MachineId,Type,Interface,Capacity,Id")]
                                                StorageByMachine storageByMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(storageByMachine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", storageByMachine.MachineId);
            return View(storageByMachine);
        }

        // GET: Admin/StorageByMachines/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null) return NotFound();

            StorageByMachine storageByMachine = await _context.StorageByMachine.FindAsync(id);
            if(storageByMachine == null) return NotFound();

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", storageByMachine.MachineId);
            return View(storageByMachine);
        }

        // POST: Admin/StorageByMachines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("MachineId,Type,Interface,Capacity,Id")]
                                              StorageByMachine storageByMachine)
        {
            if(id != storageByMachine.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(storageByMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!StorageByMachineExists(storageByMachine.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", storageByMachine.MachineId);
            return View(storageByMachine);
        }

        // GET: Admin/StorageByMachines/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null) return NotFound();

            StorageByMachine storageByMachine =
                await _context.StorageByMachine.Include(s => s.Machine).FirstOrDefaultAsync(m => m.Id == id);
            if(storageByMachine == null) return NotFound();

            return View(storageByMachine);
        }

        // POST: Admin/StorageByMachines/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            StorageByMachine storageByMachine = await _context.StorageByMachine.FindAsync(id);
            _context.StorageByMachine.Remove(storageByMachine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool StorageByMachineExists(long id)
        {
            return _context.StorageByMachine.Any(e => e.Id == id);
        }
    }
}