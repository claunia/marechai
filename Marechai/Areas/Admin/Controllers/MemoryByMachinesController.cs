/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : MemoryByMachinesController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Memory by machines admin controller
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
    public class MemoryByMachinesController : Controller
    {
        readonly MarechaiContext _context;

        public MemoryByMachinesController(MarechaiContext context) => _context = context;

        // GET: Admin/MemoryByMachines
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<MemoryByMachine, Machine> marechaiContext =
                _context.MemoryByMachine.Include(m => m.Machine);

            return View(await marechaiContext.OrderBy(m => m.Machine.Name).ThenBy(m => m.Usage).ThenBy(m => m.Size).
                                              ThenBy(m => m.Type).Select(m => new MemoryByMachineViewModel
                                              {
                                                  Id   = m.Id, Machine = m.Machine.Name, Size = m.Size, Speed = m.Speed,
                                                  Type = m.Type, Usage = m.Usage
                                              }).ToListAsync());
        }

        // GET: Admin/MemoryByMachines/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null)
                return NotFound();

            MemoryByMachine memoryByMachine =
                await _context.MemoryByMachine.Include(m => m.Machine).FirstOrDefaultAsync(m => m.Id == id);

            if(memoryByMachine == null)
                return NotFound();

            return View(memoryByMachine);
        }

        // GET: Admin/MemoryByMachines/Create
        public IActionResult Create()
        {
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name");

            return View();
        }

        // POST: Admin/MemoryByMachines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MachineId,Type,Usage,Size,Speed,Id")]
                                                MemoryByMachine memoryByMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(memoryByMachine);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", memoryByMachine.MachineId);

            return View(memoryByMachine);
        }

        // GET: Admin/MemoryByMachines/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null)
                return NotFound();

            MemoryByMachine memoryByMachine = await _context.MemoryByMachine.FindAsync(id);

            if(memoryByMachine == null)
                return NotFound();

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", memoryByMachine.MachineId);

            return View(memoryByMachine);
        }

        // POST: Admin/MemoryByMachines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("MachineId,Type,Usage,Size,Speed,Id")]
                                              MemoryByMachine memoryByMachine)
        {
            if(id != memoryByMachine.Id)
                return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(memoryByMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!MemoryByMachineExists(memoryByMachine.Id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", memoryByMachine.MachineId);

            return View(memoryByMachine);
        }

        // GET: Admin/MemoryByMachines/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null)
                return NotFound();

            MemoryByMachine memoryByMachine =
                await _context.MemoryByMachine.Include(m => m.Machine).FirstOrDefaultAsync(m => m.Id == id);

            if(memoryByMachine == null)
                return NotFound();

            return View(memoryByMachine);
        }

        // POST: Admin/MemoryByMachines/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            MemoryByMachine memoryByMachine = await _context.MemoryByMachine.FindAsync(id);
            _context.MemoryByMachine.Remove(memoryByMachine);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        bool MemoryByMachineExists(long id) => _context.MemoryByMachine.Any(e => e.Id == id);
    }
}