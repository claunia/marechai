/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : StorageByOwnedMachinesController.cs
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
    public class StorageByOwnedMachinesController : Controller
    {
        readonly MarechaiContext _context;

        public StorageByOwnedMachinesController(MarechaiContext context)
        {
            _context = context;
        }

        // GET: Admin/StorageByOwnedMachines
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<StorageByOwnedMachine, OwnedMachine> marechaiContext =
                _context.StorageByOwnedMachine.Include(s => s.OwnedMachine);
            return View(await marechaiContext.OrderBy(s => s.OwnedMachine.Machine.Company.Name)
                                         .ThenBy(s => s.OwnedMachine.Machine.Name)
                                         .ThenBy(s => s.OwnedMachine.User.UserName)
                                         .Select(s => new StorageByMachineViewModel
                                          {
                                              Id      = s.Id,
                                              Company = s.OwnedMachine.Machine.Company.Name,
                                              Machine =
                                                  $"{s.OwnedMachine.Machine.Company.Name} {s.OwnedMachine.Machine.Name} <{s.OwnedMachine.User.UserName}>",
                                              Type      = s.Type,
                                              Interface = s.Interface,
                                              Capacity  = s.Capacity
                                          }).ToListAsync());
        }

        // GET: Admin/StorageByOwnedMachines/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null) return NotFound();

            StorageByMachineViewModel storageByOwnedMachine = await _context
                                                                   .StorageByOwnedMachine
                                                                   .OrderBy(s => s.OwnedMachine.Machine.Company.Name)
                                                                   .ThenBy(s => s.OwnedMachine.Machine.Name)
                                                                   .ThenBy(s => s.OwnedMachine.User.UserName)
                                                                   .Select(s => new StorageByMachineViewModel
                                                                    {
                                                                        Id = s.Id,
                                                                        Company =
                                                                            s.OwnedMachine.Machine.Company
                                                                             .Name,
                                                                        Machine =
                                                                            $"{s.OwnedMachine.Machine.Company.Name} {s.OwnedMachine.Machine.Name} <{s.OwnedMachine.User.UserName}>",
                                                                        Type      = s.Type,
                                                                        Interface = s.Interface,
                                                                        Capacity  = s.Capacity
                                                                    }).FirstOrDefaultAsync(m => m.Id == id);
            if(storageByOwnedMachine == null) return NotFound();

            return View(storageByOwnedMachine);
        }

        // GET: Admin/StorageByOwnedMachines/Create
        public IActionResult Create()
        {
            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name");
            return View();
        }

        // POST: Admin/StorageByOwnedMachines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OwnedMachineId,Type,Interface,Capacity,Id")]
                                                StorageByOwnedMachine storageByOwnedMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(storageByOwnedMachine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name", storageByOwnedMachine.OwnedMachineId);
            return View(storageByOwnedMachine);
        }

        // GET: Admin/StorageByOwnedMachines/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null) return NotFound();

            StorageByOwnedMachine storageByOwnedMachine = await _context.StorageByOwnedMachine.FindAsync(id);
            if(storageByOwnedMachine == null) return NotFound();

            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name", storageByOwnedMachine.OwnedMachineId);
            return View(storageByOwnedMachine);
        }

        // POST: Admin/StorageByOwnedMachines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("OwnedMachineId,Type,Interface,Capacity,Id")]
                                              StorageByOwnedMachine storageByOwnedMachine)
        {
            if(id != storageByOwnedMachine.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(storageByOwnedMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!StorageByOwnedMachineExists(storageByOwnedMachine.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["OwnedMachineId"] =
                new
                    SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).ThenBy(m => m.Machine.Name).ThenBy(m => m.User.UserName).Select(m => new {m.Id, Name = $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"}),
                               "Id", "Name", storageByOwnedMachine.OwnedMachineId);
            return View(storageByOwnedMachine);
        }

        // GET: Admin/StorageByOwnedMachines/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null) return NotFound();

            StorageByMachineViewModel storageByOwnedMachine = await _context
                                                                   .StorageByOwnedMachine
                                                                   .OrderBy(s => s.OwnedMachine.Machine.Company.Name)
                                                                   .ThenBy(s => s.OwnedMachine.Machine.Name)
                                                                   .ThenBy(s => s.OwnedMachine.User.UserName)
                                                                   .Select(s => new StorageByMachineViewModel
                                                                    {
                                                                        Id = s.Id,
                                                                        Company =
                                                                            s.OwnedMachine.Machine.Company
                                                                             .Name,
                                                                        Machine =
                                                                            $"{s.OwnedMachine.Machine.Company.Name} {s.OwnedMachine.Machine.Name} <{s.OwnedMachine.User.UserName}>",
                                                                        Type      = s.Type,
                                                                        Interface = s.Interface,
                                                                        Capacity  = s.Capacity
                                                                    }).FirstOrDefaultAsync(m => m.Id == id);
            if(storageByOwnedMachine == null) return NotFound();

            return View(storageByOwnedMachine);
        }

        // POST: Admin/StorageByOwnedMachines/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            StorageByOwnedMachine storageByOwnedMachine = await _context.StorageByOwnedMachine.FindAsync(id);
            _context.StorageByOwnedMachine.Remove(storageByOwnedMachine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool StorageByOwnedMachineExists(long id)
        {
            return _context.StorageByOwnedMachine.Any(e => e.Id == id);
        }
    }
}