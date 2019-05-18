﻿/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : CompanyDescriptionsController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Company descriptions admin controller
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
    public class CompanyDescriptionsController : Controller
    {
        readonly cicmContext _context;

        public CompanyDescriptionsController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/CompanyDescriptions
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<CompanyDescription, Company> cicmContext =
                _context.CompanyDescriptions.Include(c => c.Company);
            return View(await cicmContext.ToListAsync());
        }

        // GET: Admin/CompanyDescriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            CompanyDescription companyDescription =
                await _context.CompanyDescriptions.Include(c => c.Company).FirstOrDefaultAsync(m => m.Id == id);
            if(companyDescription == null) return NotFound();

            return View(companyDescription);
        }

        // GET: Admin/CompanyDescriptions/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Companies, "Id", "Name");
            return View();
        }

        // POST: Admin/CompanyDescriptions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Text")] CompanyDescription companyDescription)
        {
            if(ModelState.IsValid)
            {
                _context.Add(companyDescription);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Id"] = new SelectList(_context.Companies, "Id", "Name", companyDescription.Id);
            return View(companyDescription);
        }

        // GET: Admin/CompanyDescriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            CompanyDescription companyDescription = await _context.CompanyDescriptions.FindAsync(id);
            if(companyDescription == null) return NotFound();

            ViewData["Id"] = new SelectList(_context.Companies, "Id", "Name", companyDescription.Id);
            return View(companyDescription);
        }

        // POST: Admin/CompanyDescriptions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Text")] CompanyDescription companyDescription)
        {
            if(id != companyDescription.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(companyDescription);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!CompanyDescriptionExists(companyDescription.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["Id"] = new SelectList(_context.Companies, "Id", "Name", companyDescription.Id);
            return View(companyDescription);
        }

        // GET: Admin/CompanyDescriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            CompanyDescription companyDescription =
                await _context.CompanyDescriptions.Include(c => c.Company).FirstOrDefaultAsync(m => m.Id == id);
            if(companyDescription == null) return NotFound();

            return View(companyDescription);
        }

        // POST: Admin/CompanyDescriptions/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            CompanyDescription companyDescription = await _context.CompanyDescriptions.FindAsync(id);
            _context.CompanyDescriptions.Remove(companyDescription);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool CompanyDescriptionExists(int id)
        {
            return _context.CompanyDescriptions.Any(e => e.Id == id);
        }
    }
}