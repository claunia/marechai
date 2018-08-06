/******************************************************************************
// Canary Islands Computer Museum Website
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
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using System.Linq;
using Cicm.Database.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cicm_web.Controllers
{
    public class CompanyController : Controller
    {
        readonly cicmContext         _context;
        readonly IHostingEnvironment hostingEnvironment;

        public CompanyController(IHostingEnvironment env, cicmContext context)
        {
            hostingEnvironment = env;
            _context           = context;
        }

        public IActionResult ByLetter(char id)
        {
            // ToUpper()
            if(id >= 'a' && id <= 'z') id -= (char)32;
            // Check if not letter
            if(id < 'A' || id > 'Z') id = '\0';

            ViewBag.Letter = id;

            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;
            return View(id == '\0'
                            ? _context.Companies.ToArray()
                            : _context.Companies.Where(c => c.Name.StartsWith(id)).ToArray());
        }

        public IActionResult View(int id)
        {
            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;
            Companies company = _context.Companies.Where(c => c.Id == id).Include(c => c.Description)
                                        .Include(c => c.Machines).Include(c => c.Country).FirstOrDefault();

            if(company == null) return Index();

            ViewBag.LastLogo     = company.CompanyLogos.OrderByDescending(l => l.Year).FirstOrDefault();
            ViewBag.CompanyLogos = company.CompanyLogos.OrderByDescending(l => l.Year).ToList();

            return View(company);
        }

        public IActionResult ByCountry(short id)
        {
            ViewBag.Iso3166 = _context.Iso31661Numeric.FirstOrDefault(i => i.Id == id);

            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;
            return View(ViewBag.Iso3166 == null
                            ? _context.Companies.ToArray()
                            : _context.Companies.Where(c => c.CountryId == id).ToArray());
        }

        public IActionResult Index()
        {
            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;
            return View(_context.Companies.ToArray());
        }
    }
}