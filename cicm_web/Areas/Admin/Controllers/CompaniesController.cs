/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : CompaniesController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Companies admin controller
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
    public class CompaniesController : Controller
    {
        readonly cicmContext _context;

        public CompaniesController(cicmContext context)
        {
            _context = context;
        }

        // GET: Admin/Companies
        public async Task<IActionResult> Index()
        {
            IIncludableQueryable<Company, Company> cicmContext =
                _context.Companies.Include(c => c.Country).Include(c => c.SoldTo);
            return View(cicmContext.OrderBy(c => c.Name).Select(c => new CompanyViewModel
            {
                Id      = c.Id,
                Name    = c.Name,
                Founded = c.Founded,
                Status  = c.Status,
                Country = c.Country.Name,
                Sold    = c.Sold,
                SoldTo  = c.SoldTo.Name
            }));
        }

        // GET: Admin/Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) return NotFound();

            Company company = await _context.Companies.Include(c => c.Country).Include(c => c.SoldTo)
                                            .FirstOrDefaultAsync(m => m.Id == id);
            if(company == null) return NotFound();

            return View(company);
        }

        // GET: Admin/Companies/Create
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Iso31661Numeric.OrderBy(c => c.Name), "Id", "Name");
            ViewData["SoldToId"]  = new SelectList(_context.Companies.OrderBy(c => c.Name),       "Id", "Name");
            return View();
        }

        // POST: Admin/Companies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "Id,Name,Founded,Website,Twitter,Facebook,Sold,SoldToId,Address,City,Province,PostalCode,CountryId,Status")]
            Company company)
        {
            if(ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CountryId"] =
                new SelectList(_context.Iso31661Numeric.OrderBy(c => c.Name), "Id", "Name", company.CountryId);
            ViewData["SoldToId"] =
                new SelectList(_context.Companies.OrderBy(c => c.Name), "Id", "Name", company.SoldToId);
            return View(company);
        }

        // GET: Admin/Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null) return NotFound();

            Company company = await _context.Companies.FindAsync(id);
            if(company == null) return NotFound();

            ViewData["CountryId"] =
                new SelectList(_context.Iso31661Numeric.OrderBy(c => c.Name), "Id", "Name", company.CountryId);
            ViewData["SoldToId"] =
                new SelectList(_context.Companies.OrderBy(c => c.Name), "Id", "Name", company.SoldToId);
            return View(company);
        }

        // POST: Admin/Companies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, [Bind(
                "Id,Name,Founded,Website,Twitter,Facebook,Sold,SoldToId,Address,City,Province,PostalCode,CountryId,Status")]
            Company company)
        {
            if(id != company.Id) return NotFound();

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!CompanyExists(company.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CountryId"] =
                new SelectList(_context.Iso31661Numeric.OrderBy(c => c.Name), "Id", "Name", company.CountryId);
            ViewData["SoldToId"] =
                new SelectList(_context.Companies.OrderBy(c => c.Name), "Id", "Name", company.SoldToId);
            return View(company);
        }

        // GET: Admin/Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return NotFound();

            Company company = await _context.Companies.Include(c => c.Country).Include(c => c.SoldTo)
                                            .FirstOrDefaultAsync(m => m.Id == id);
            if(company == null) return NotFound();

            return View(company);
        }

        // POST: Admin/Companies/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Company company = await _context.Companies.FindAsync(id);
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}