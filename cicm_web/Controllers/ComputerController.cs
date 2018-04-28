/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ComputerController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Computer controller
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

using System.Collections.Generic;
using System.Linq;
using cicm_web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Machine = Cicm.Database.Schemas.Machine;

namespace cicm_web.Controllers
{
    public class ComputerController : Controller
    {
        readonly IHostingEnvironment hostingEnvironment;

        public ComputerController(IHostingEnvironment env)
        {
            hostingEnvironment = env;
        }

        public IActionResult Index()
        {
            Program.Database.Operations.GetComputers(out List<Machine> computers);

            ViewBag.ItemCount = computers.Count;

            ViewBag.MinYear = computers.Where(t => t.Year > 1000).Min(t => t.Year);
            ViewBag.MaxYear = computers.Where(t => t.Year > 1000).Max(t => t.Year);

            return View();
        }

        public IActionResult ByLetter(char id)
        {
            // ToUpper()
            if(id >= 'a' && id <= 'z') id -= (char)32;
            // Check if not letter
            if(id < 'A' || id > 'Z') id = '\0';

            ViewBag.Letter = id;

            MachineMini[] computers =
                id == '\0' ? ComputerMini.GetAllItems() : ComputerMini.GetItemsStartingWithLetter(id);

            return View(computers);
        }

        public IActionResult ByYear(int id)
        {
            ViewBag.Year = id;

            return View(ComputerMini.GetItemsFromYear(id));
        }
    }
}