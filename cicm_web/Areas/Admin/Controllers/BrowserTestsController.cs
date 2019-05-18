/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : BrowserTestsController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Browser test admin controller
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
    public class BrowserTestsController : Controller
    {
        readonly cicmContext _context;

        public BrowserTestsController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/BrowserTests
        public async Task<IActionResult> Index()
        {
            return View(await _context.BrowserTests.ToListAsync());
        }

        // GET: Admin/BrowserTests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            BrowserTest browserTest = await _context.BrowserTests.FirstOrDefaultAsync(m => m.Id == id);
            if(browserTest == null) return NotFound();

            return View(browserTest);
        }

        // GET: Admin/BrowserTests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/BrowserTests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "Id,UserAgent,Browser,Version,Os,Platform,Gif87,Gif89,Jpeg,Png,Pngt,Agif,Table,Colors,Js,Frames,Flash")]
            BrowserTest browserTest)
        {
            if(ModelState.IsValid)
            {
                _context.Add(browserTest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(browserTest);
        }

        // GET: Admin/BrowserTests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            BrowserTest browserTest = await _context.BrowserTests.FindAsync(id);
            if(browserTest == null) return NotFound();

            return View(browserTest);
        }

        // POST: Admin/BrowserTests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, [Bind(
                "Id,UserAgent,Browser,Version,Os,Platform,Gif87,Gif89,Jpeg,Png,Pngt,Agif,Table,Colors,Js,Frames,Flash")]
            BrowserTest browserTest)
        {
            if(id != browserTest.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(browserTest);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!BrowserTestExists(browserTest.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(browserTest);
        }

        // GET: Admin/BrowserTests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            BrowserTest browserTest = await _context.BrowserTests.FirstOrDefaultAsync(m => m.Id == id);
            if(browserTest == null) return NotFound();

            return View(browserTest);
        }

        // POST: Admin/BrowserTests/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            BrowserTest browserTest = await _context.BrowserTests.FindAsync(id);
            _context.BrowserTests.Remove(browserTest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool BrowserTestExists(int id)
        {
            return _context.BrowserTests.Any(e => e.Id == id);
        }
    }
}