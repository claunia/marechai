/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : InstructionSetExtensionsController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Instruction set extensions admin controller
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
using Microsoft.EntityFrameworkCore;

namespace cicm_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class InstructionSetExtensionsController : Controller
    {
        readonly cicmContext _context;

        public InstructionSetExtensionsController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/InstructionSetExtensions
        public async Task<IActionResult> Index() => View(await _context.InstructionSetExtensions.OrderBy(e => e.Extension).ToListAsync());

        // GET: Admin/InstructionSetExtensions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            InstructionSetExtension instructionSetExtension =
                await _context.InstructionSetExtensions.FirstOrDefaultAsync(m => m.Id == id);
            if(instructionSetExtension == null) return NotFound();

            return View(instructionSetExtension);
        }

        // GET: Admin/InstructionSetExtensions/Create
        public IActionResult Create() => View();

        // POST: Admin/InstructionSetExtensions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Extension")] InstructionSetExtension instructionSetExtension)
        {
            if(ModelState.IsValid)
            {
                _context.Add(instructionSetExtension);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(instructionSetExtension);
        }

        // GET: Admin/InstructionSetExtensions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            InstructionSetExtension instructionSetExtension = await _context.InstructionSetExtensions.FindAsync(id);
            if(instructionSetExtension == null) return NotFound();

            return View(instructionSetExtension);
        }

        // POST: Admin/InstructionSetExtensions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, [Bind("Id,Extension")] InstructionSetExtension instructionSetExtension)
        {
            if(id != instructionSetExtension.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(instructionSetExtension);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!InstructionSetExtensionExists(instructionSetExtension.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(instructionSetExtension);
        }

        // GET: Admin/InstructionSetExtensions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            InstructionSetExtension instructionSetExtension =
                await _context.InstructionSetExtensions.FirstOrDefaultAsync(m => m.Id == id);
            if(instructionSetExtension == null) return NotFound();

            return View(instructionSetExtension);
        }

        // POST: Admin/InstructionSetExtensions/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            InstructionSetExtension instructionSetExtension = await _context.InstructionSetExtensions.FindAsync(id);
            _context.InstructionSetExtensions.Remove(instructionSetExtension);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool InstructionSetExtensionExists(int id)
        {
            return _context.InstructionSetExtensions.Any(e => e.Id == id);
        }
    }
}