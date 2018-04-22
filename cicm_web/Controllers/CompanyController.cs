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

using cicm_web.Models;
using Cicm.Database.Schemas;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Company = cicm_web.Models.Company;

namespace cicm_web.Controllers
{
    public class CompanyController : Controller
    {
        readonly IHostingEnvironment hostingEnvironment;

        public CompanyController(IHostingEnvironment env)
        {
            hostingEnvironment = env;
        }

        public IActionResult ByLetter(char id)
        {
            // ToUpper()
            if(id >= 'a' && id <= 'z') id -= (char)32;
            // Check if not letter
            if(id < 'A' || id > 'Z') id = '\0';

            ViewBag.Letter = id;

            Company[] companies = id == '\0' ? Company.GetAllItems() : Company.GetItemsStartingWithLetter(id);

            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;
            return View(companies);
        }

        public IActionResult View(int id)
        {
            CompanyWithItems company = CompanyWithItems.GetItem(id);

            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;
            return View(company);
        }

        public IActionResult ByCountry(ushort id)
        {
            Iso3166 iso3166 = Program.Database.Operations.GetIso3166(id);

            ViewBag.Iso3166 = iso3166;

            Company[] companies = iso3166 == null ? Company.GetAllItems() : Company.GetItemsByCountry(id);

            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;
            return View(companies);
        }

        public IActionResult Index()
        {
            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;
            return View(Company.GetAllItems());
        }
    }
}