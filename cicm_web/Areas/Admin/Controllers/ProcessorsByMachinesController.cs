/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : ProcessorsByMachinesController.cs
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
    public class ProcessorsByMachinesController : Controller
    {
        readonly cicmContext _context;

        public ProcessorsByMachinesController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/ProcessorsByMachines
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<ProcessorsByMachine, Processor> cicmContext =
                _context.ProcessorsByMachine.Include(p => p.Machine).Include(p => p.Processor);
            return View(await cicmContext.OrderBy(p => p.Machine.Name).ThenBy(p => p.Processor.Name)
                                         .Select(p => new ProcessorsByMachineViewModel
                                          {
                                              Id        = p.Id,
                                              Machine   = p.Machine.Name,
                                              Processor = p.Processor.Name,
                                              Speed     = p.Speed
                                          }).ToListAsync());
        }

        // GET: Admin/ProcessorsByMachines/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if(id == null) return NotFound();

            ProcessorsByMachine processorsByMachine =
                await _context.ProcessorsByMachine.Include(p => p.Machine).Include(p => p.Processor)
                              .FirstOrDefaultAsync(m => m.Id == id);
            if(processorsByMachine == null) return NotFound();

            return View(processorsByMachine);
        }

        // GET: Admin/ProcessorsByMachines/Create
        public IActionResult Create()
        {
            ViewData["MachineId"]   = new SelectList(_context.Machines,   "Id", "Name");
            ViewData["ProcessorId"] = new SelectList(_context.Processors, "Id", "Name");
            return View();
        }

        // POST: Admin/ProcessorsByMachines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProcessorId,MachineId,Speed,Id")]
                                                ProcessorsByMachine processorsByMachine)
        {
            if(ModelState.IsValid)
            {
                _context.Add(processorsByMachine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", processorsByMachine.MachineId);
            ViewData["ProcessorId"] =
                new SelectList(_context.Processors, "Id", "Name", processorsByMachine.ProcessorId);
            return View(processorsByMachine);
        }

        // GET: Admin/ProcessorsByMachines/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if(id == null) return NotFound();

            ProcessorsByMachine processorsByMachine = await _context.ProcessorsByMachine.FindAsync(id);
            if(processorsByMachine == null) return NotFound();

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", processorsByMachine.MachineId);
            ViewData["ProcessorId"] =
                new SelectList(_context.Processors, "Id", "Name", processorsByMachine.ProcessorId);
            return View(processorsByMachine);
        }

        // POST: Admin/ProcessorsByMachines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("ProcessorId,MachineId,Speed,Id")]
                                              ProcessorsByMachine processorsByMachine)
        {
            if(id != processorsByMachine.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(processorsByMachine);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!ProcessorsByMachineExists(processorsByMachine.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Name", processorsByMachine.MachineId);
            ViewData["ProcessorId"] =
                new SelectList(_context.Processors, "Id", "Name", processorsByMachine.ProcessorId);
            return View(processorsByMachine);
        }

        // GET: Admin/ProcessorsByMachines/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if(id == null) return NotFound();

            ProcessorsByMachine processorsByMachine =
                await _context.ProcessorsByMachine.Include(p => p.Machine).Include(p => p.Processor)
                              .FirstOrDefaultAsync(m => m.Id == id);
            if(processorsByMachine == null) return NotFound();

            return View(processorsByMachine);
        }

        // POST: Admin/ProcessorsByMachines/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            ProcessorsByMachine processorsByMachine = await _context.ProcessorsByMachine.FindAsync(id);
            _context.ProcessorsByMachine.Remove(processorsByMachine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool ProcessorsByMachineExists(long id)
        {
            return _context.ProcessorsByMachine.Any(e => e.Id == id);
        }
    }
}