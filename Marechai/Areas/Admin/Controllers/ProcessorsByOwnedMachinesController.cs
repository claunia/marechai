/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : ProcessorsByOwnedMachinesController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Processors by machine admin controller
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
    public class ProcessorsByOwnedMachinesController : Controller
    {
        readonly MarechaiContext _context;

        public ProcessorsByOwnedMachinesController(MarechaiContext context) => _context = context;

        // GET: Admin/ProcessorsByOwnedMachines
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<ProcessorsByOwnedMachine, Processor> marechaiContext =
                _context.ProcessorsByOwnedMachine.Include(p => p.OwnedMachine).Include(p => p.Processor);

            return View(await marechaiContext.OrderBy(p => p.OwnedMachine.Machine.Name).ThenBy(p => p.Processor.Name).
                                              Select(p => new ProcessorsByMachineViewModel
                                              {
                                                  Id = p.Id,
                                                  Machine =
                                                      $"{p.OwnedMachine.Machine.Company.Name} {p.OwnedMachine.Machine.Name} <{p.OwnedMachine.User.UserName}>",
                                                  Processor = p.Processor.Name, Speed = p.Speed
                                              }).ToListAsync());
        }

        // GET: Admin/ProcessorsByOwnedMachines/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null)
                return NotFound();

            ProcessorsByMachineViewModel processorsByOwnedMachine =
                await _context.ProcessorsByOwnedMachine.OrderBy(p => p.OwnedMachine.Machine.Name).
                               ThenBy(p => p.Processor.Name).Select(p => new ProcessorsByMachineViewModel
                               {
                                   Id = p.Id,
                                   Machine =
                                       $"{p.OwnedMachine.Machine.Company.Name} {p.OwnedMachine.Machine.Name} <{p.OwnedMachine.User.UserName}>",
                                   Processor = p.Processor.Name, Speed = p.Speed
                               }).FirstOrDefaultAsync(m => m.Id == id);

            if(processorsByOwnedMachine == null)
                return NotFound();

            return View(processorsByOwnedMachine);
        }

        // GET: Admin/ProcessorsByOwnedMachines/Create
        public IActionResult Create()
        {
            ViewData["OwnedMachineId"] = new SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).
                                                                 ThenBy(m => m.Machine.Name).
                                                                 ThenBy(m => m.User.UserName).Select(m => new
                                                                 {
                                                                     m.Id,
                                                                     Name =
                                                                         $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"
                                                                 }), "Id", "Name");

            ViewData["ProcessorId"] = new SelectList(_context.Processors, "Id", "Name");

            return View();
        }

        // POST: Admin/ProcessorsByOwnedMachines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProcessorId,OwnedMachineId,Speed,Id")]
                                                ProcessorsByOwnedMachine processorsByOwnedMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(processorsByOwnedMachine);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["OwnedMachineId"] = new SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).
                                                                 ThenBy(m => m.Machine.Name).
                                                                 ThenBy(m => m.User.UserName).Select(m => new
                                                                 {
                                                                     m.Id,
                                                                     Name =
                                                                         $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"
                                                                 }), "Id", "Name",
                                                        processorsByOwnedMachine.OwnedMachineId);

            ViewData["ProcessorId"] =
                new SelectList(_context.Processors, "Id", "Name", processorsByOwnedMachine.ProcessorId);

            return View(processorsByOwnedMachine);
        }

        // GET: Admin/ProcessorsByOwnedMachines/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null)
                return NotFound();

            ProcessorsByOwnedMachine processorsByOwnedMachine = await _context.ProcessorsByOwnedMachine.FindAsync(id);

            if(processorsByOwnedMachine == null)
                return NotFound();

            ViewData["OwnedMachineId"] = new SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).
                                                                 ThenBy(m => m.Machine.Name).
                                                                 ThenBy(m => m.User.UserName).Select(m => new
                                                                 {
                                                                     m.Id,
                                                                     Name =
                                                                         $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"
                                                                 }), "Id", "Name",
                                                        processorsByOwnedMachine.OwnedMachineId);

            ViewData["ProcessorId"] =
                new SelectList(_context.Processors, "Id", "Name", processorsByOwnedMachine.ProcessorId);

            return View(processorsByOwnedMachine);
        }

        // POST: Admin/ProcessorsByOwnedMachines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("ProcessorId,OwnedMachineId,Speed,Id")]
                                              ProcessorsByOwnedMachine processorsByOwnedMachine)
        {
            if(id != processorsByOwnedMachine.Id)
                return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(processorsByOwnedMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!ProcessorsByOwnedMachineExists(processorsByOwnedMachine.Id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["OwnedMachineId"] = new SelectList(_context.OwnedMachines.OrderBy(m => m.Machine.Company.Name).
                                                                 ThenBy(m => m.Machine.Name).
                                                                 ThenBy(m => m.User.UserName).Select(m => new
                                                                 {
                                                                     m.Id,
                                                                     Name =
                                                                         $"{m.Machine.Company.Name} {m.Machine.Name} <{m.User.UserName}>"
                                                                 }), "Id", "Name",
                                                        processorsByOwnedMachine.OwnedMachineId);

            ViewData["ProcessorId"] =
                new SelectList(_context.Processors, "Id", "Name", processorsByOwnedMachine.ProcessorId);

            return View(processorsByOwnedMachine);
        }

        // GET: Admin/ProcessorsByOwnedMachines/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null)
                return NotFound();

            ProcessorsByMachineViewModel processorsByOwnedMachine =
                await _context.ProcessorsByOwnedMachine.OrderBy(p => p.OwnedMachine.Machine.Name).
                               ThenBy(p => p.Processor.Name).Select(p => new ProcessorsByMachineViewModel
                               {
                                   Id = p.Id,
                                   Machine =
                                       $"{p.OwnedMachine.Machine.Company.Name} {p.OwnedMachine.Machine.Name} <{p.OwnedMachine.User.UserName}>",
                                   Processor = p.Processor.Name, Speed = p.Speed
                               }).FirstOrDefaultAsync(m => m.Id == id);

            if(processorsByOwnedMachine == null)
                return NotFound();

            return View(processorsByOwnedMachine);
        }

        // POST: Admin/ProcessorsByOwnedMachines/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            ProcessorsByOwnedMachine processorsByOwnedMachine = await _context.ProcessorsByOwnedMachine.FindAsync(id);
            _context.ProcessorsByOwnedMachine.Remove(processorsByOwnedMachine);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        bool ProcessorsByOwnedMachineExists(long id) => _context.ProcessorsByOwnedMachine.Any(e => e.Id == id);
    }
}