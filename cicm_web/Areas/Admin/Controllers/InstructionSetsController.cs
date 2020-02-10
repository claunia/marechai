/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : InstructionSetsController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Instruction sets admin controller
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

using System;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class InstructionSetsController : Controller
    {
        readonly cicmContext _context;

        public InstructionSetsController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/InstructionSets
        public async Task<IActionResult> Index() =>
            View(await _context.InstructionSets.OrderBy(s => s.Name).ToListAsync());

        // GET: Admin/InstructionSets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            InstructionSet instructionSet = await _context.InstructionSets.FirstOrDefaultAsync(m => m.Id == id);
            if(instructionSet == null) return NotFound();

            return View(instructionSet);
        }

        // GET: Admin/InstructionSets/Create
        public IActionResult Create() => View();

        // POST: Admin/InstructionSets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] InstructionSet instructionSet)
        {
            if(!ModelState.IsValid) return View(instructionSet);

            _context.Add(instructionSet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/InstructionSets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            InstructionSet instructionSet = await _context.InstructionSets.FindAsync(id);
            if(instructionSet == null) return NotFound();

            return View(instructionSet);
        }

        // POST: Admin/InstructionSets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] InstructionSet instructionSet)
        {
            if(id != instructionSet.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(instructionSet);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!InstructionSetExists(instructionSet.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(instructionSet);
        }

        // GET: Admin/InstructionSets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            InstructionSet instructionSet = await _context.InstructionSets.FirstOrDefaultAsync(m => m.Id == id);
            if(instructionSet == null) return NotFound();

            return View(instructionSet);
        }

        // POST: Admin/InstructionSets/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            InstructionSet instructionSet = await _context.InstructionSets.FindAsync(id);
            _context.InstructionSets.Remove(instructionSet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool InstructionSetExists(int id)
        {
            return _context.InstructionSets.Any(e => e.Id == id);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult VerifyUnique(string name) =>
            _context.InstructionSets.Any(i => string.Equals(i.Name, name, StringComparison.InvariantCultureIgnoreCase))
                ? Json("Instruction set already exists.")
                : Json(true);
    }
}