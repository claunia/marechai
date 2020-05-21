/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : CompanyController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Company controller
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
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Controllers
{
    public class CompanyController : Controller
    {
        readonly MarechaiContext     _context;
        readonly IWebHostEnvironment hostingEnvironment;

        public CompanyController(IWebHostEnvironment env, MarechaiContext context)
        {
            hostingEnvironment = env;
            _context           = context;
        }

        public IActionResult ByLetter(char id)
        {
            // ToUpper()
            if(id >= 'a' &&
               id <= 'z')
                id -= (char)32;

            // Check if not letter
            if(id < 'A' ||
               id > 'Z')
                id = '\0';

            ViewBag.Letter = id;

            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;

            if(id == '\0')
                return RedirectToAction(nameof(Index));

            return View(_context.Companies.Include(c => c.Logos).Where(c => c.Name.StartsWith(id)).OrderBy(c => c.Name).
                                 Select(c => new CompanyViewModel
                                 {
                                     Id = c.Id, LastLogo = c.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
                                     Name = c.Name
                                 }).ToList());
        }

        public IActionResult Index()
        {
            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;

            return View(_context.Companies.Include(c => c.Logos).OrderBy(c => c.Name).Select(c => new CompanyViewModel
            {
                Id = c.Id, LastLogo = c.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid, Name = c.Name
            }).ToList());
        }
    }
}