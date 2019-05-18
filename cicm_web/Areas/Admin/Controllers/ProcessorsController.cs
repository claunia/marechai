/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ProcessorsController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Processors admin controller
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
    public class ProcessorsController : Controller
    {
        readonly cicmContext _context;

        public ProcessorsController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/Processors
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<Processor, InstructionSet> cicmContext =
                _context.Processors.Include(p => p.Company).Include(p => p.InstructionSet);
            return View(await cicmContext.ToListAsync());
        }

        // GET: Admin/Processors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            Processor processor = await _context.Processors.Include(p => p.Company).Include(p => p.InstructionSet)
                                                .FirstOrDefaultAsync(m => m.Id == id);
            if(processor == null) return NotFound();

            return View(processor);
        }

        // GET: Admin/Processors/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"]        = new SelectList(_context.Companies,       "Id", "Name");
            ViewData["InstructionSetId"] = new SelectList(_context.InstructionSets, "Id", "Name");
            return View();
        }

        // POST: Admin/Processors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "Id,Name,CompanyId,ModelCode,Introduced,InstructionSetId,Speed,Package,Gprs,GprSize,Fprs,FprSize,Cores,ThreadsPerCore,Process,ProcessNm,DieSize,Transistors,DataBus,AddrBus,SimdRegisters,SimdSize,L1Instruction,L1Data,L2,L3")]
            Processor processor)
        {
            if(ModelState.IsValid)
            {
                _context.Add(processor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", processor.CompanyId);
            ViewData["InstructionSetId"] =
                new SelectList(_context.InstructionSets, "Id", "Name", processor.InstructionSetId);
            return View(processor);
        }

        // GET: Admin/Processors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            Processor processor = await _context.Processors.FindAsync(id);
            if(processor == null) return NotFound();

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", processor.CompanyId);
            ViewData["InstructionSetId"] =
                new SelectList(_context.InstructionSets, "Id", "Name", processor.InstructionSetId);
            return View(processor);
        }

        // POST: Admin/Processors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, [Bind(
                "Id,Name,CompanyId,ModelCode,Introduced,InstructionSetId,Speed,Package,Gprs,GprSize,Fprs,FprSize,Cores,ThreadsPerCore,Process,ProcessNm,DieSize,Transistors,DataBus,AddrBus,SimdRegisters,SimdSize,L1Instruction,L1Data,L2,L3")]
            Processor processor)
        {
            if(id != processor.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(processor);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!ProcessorExists(processor.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", processor.CompanyId);
            ViewData["InstructionSetId"] =
                new SelectList(_context.InstructionSets, "Id", "Name", processor.InstructionSetId);
            return View(processor);
        }

        // GET: Admin/Processors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            Processor processor = await _context.Processors.Include(p => p.Company).Include(p => p.InstructionSet)
                                                .FirstOrDefaultAsync(m => m.Id == id);
            if(processor == null) return NotFound();

            return View(processor);
        }

        // POST: Admin/Processors/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Processor processor = await _context.Processors.FindAsync(id);
            _context.Processors.Remove(processor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool ProcessorExists(int id)
        {
            return _context.Processors.Any(e => e.Id == id);
        }
    }
}