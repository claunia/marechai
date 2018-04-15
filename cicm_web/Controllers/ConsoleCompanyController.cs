/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ConsoleCompanyController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Console company controller
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace cicm_web.Controllers
{
    public class ConsoleCompanyController : Controller
    {
        readonly IHostingEnvironment hostingEnvironment;

        public ConsoleCompanyController(IHostingEnvironment env)
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

            ConsoleCompany[] companies =
                id == '\0' ? ConsoleCompany.GetAllItems() : ConsoleCompany.GetItemsStartingWithLetter(id);

            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;
            return View(companies);
        }
        
        public IActionResult View(int id)
        {
            ConsoleCompany company = ConsoleCompany.GetItem(id);
            ViewBag.Company = company;
            Console[] consoles = Console.GetItemsFromCompany(id);

            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;
            return View(consoles);
        }
    }
}