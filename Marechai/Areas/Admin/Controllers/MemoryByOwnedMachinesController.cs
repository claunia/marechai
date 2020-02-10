/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : MemoryByOwnedMachinesController.cs
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
using Marechai.Database.Models;
using Marechai.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Marechai.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class MemoryByOwnedMachinesController : Controller
    {
        readonly MarechaiContext _context;

        public MemoryByOwnedMachinesController(MarechaiContext context)
        {
            _context = context;
        }

        // GET: Admin/MemoryByOwnedMachines
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<MemoryByOwnedMachine, OwnedMachine> marechaiContext =
                _context.MemoryByOwnedMachine.Include(m => m.OwnedMachine);
            return View(await marechaiContext.OrderBy(m => m.OwnedMachine.Machine.Company.Name)
                                         .ThenBy(m => m.OwnedMachine.Machine.Name)
                                         .ThenBy(m => m.OwnedMachine.User.UserName).ThenBy(m => m.Usage)
                                         .ThenBy(m => m.Size).ThenBy(m => m.Type)
                                         .Select(m => new MemoryByMachineViewModel
                                          {
                                              Id = m.Id,
                                              Machine =
                                                  $"{m.OwnedMachine.Machine.Company.Name} {m.OwnedMachine.Machine.Name} <{m.OwnedMachine.User.UserName}>",
                                              Size  = m.Size,
                                              Speed = m.Speed,
                                              Type  = m.Type,
                                              Usage = m.Usage
                                          }).ToListAsync());
        }

        // GET: Admin/MemoryByOwnedMachines/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null) return NotFound();

            IIncludableQueryable<MemoryByOwnedMachine, OwnedMachine> marechaiContext =
                _context.MemoryByOwnedMachine.Include(m => m.OwnedMachine);
            MemoryByMachineViewModel memoryByOwnedMachine = await marechaiContext
                                                                 .OrderBy(m => m.OwnedMachine.Machine.Company.Name)
                                                                 .ThenBy(m => m.OwnedMachine.Machine.Name)
                                                                 .ThenBy(m => m.OwnedMachine.User.UserName)
                                                                 .ThenBy(m => m.Usage).ThenBy(m => m.Size)
                                                                 .ThenBy(m => m.Type)
                                                                 .Select(m => new MemoryByMachineViewModel
                                                                  {
                                                                      Id = m.Id,
                                                                      Machine =
                                                                          $"{m.OwnedMachine.Machine.Company.Name} {m.OwnedMachine.Machine.Name} <{m.OwnedMachine.User.UserName}>",
                                                                      Size  = m.Size,
                                                                      Speed = m.Speed,
                                                                      Type  = m.Type,
                                                                      Usage = m.Usage
                                                                  }).FirstOrDefaultAsync(m => m.Id == id);
            if(memoryByOwnedMachine == null) return NotFound();

            return View(memoryByOwnedMachine);
        }

        // GET: Admin/MemoryByOwnedMachines/Create
        public IActionResult Create()
        {
            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name");
            return View();
        }

        // POST: Admin/MemoryByOwnedMachines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OwnedMachineId,Type,Usage,Size,Speed,Id")]
                                                MemoryByOwnedMachine memoryByOwnedMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(memoryByOwnedMachine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name", memoryByOwnedMachine.OwnedMachineId);
            return View(memoryByOwnedMachine);
        }

        // GET: Admin/MemoryByOwnedMachines/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null) return NotFound();

            MemoryByOwnedMachine memoryByOwnedMachine = await _context.MemoryByOwnedMachine.FindAsync(id);
            if(memoryByOwnedMachine == null) return NotFound();

            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name", memoryByOwnedMachine.OwnedMachineId);
            return View(memoryByOwnedMachine);
        }

        // POST: Admin/MemoryByOwnedMachines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("OwnedMachineId,Type,Usage,Size,Speed,Id")]
                                              MemoryByOwnedMachine memoryByOwnedMachine)
        {
            if(id != memoryByOwnedMachine.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(memoryByOwnedMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!MemoryByOwnedMachineExists(memoryByOwnedMachine.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name", memoryByOwnedMachine.OwnedMachineId);
            return View(memoryByOwnedMachine);
        }

        // GET: Admin/MemoryByOwnedMachines/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null) return NotFound();

            MemoryByOwnedMachine memoryByOwnedMachine =
                await _context.MemoryByOwnedMachine.Include(m => m.OwnedMachine).FirstOrDefaultAsync(m => m.Id == id);
            if(memoryByOwnedMachine == null) return NotFound();

            return View(memoryByOwnedMachine);
        }

        // POST: Admin/MemoryByOwnedMachines/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            MemoryByOwnedMachine memoryByOwnedMachine = await _context.MemoryByOwnedMachine.FindAsync(id);
            _context.MemoryByOwnedMachine.Remove(memoryByOwnedMachine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool MemoryByOwnedMachineExists(long id)
        {
            return _context.MemoryByOwnedMachine.Any(e => e.Id == id);
        }
    }
}