/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ConsoleController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Videogame console controller
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
    public class ConsoleController : Controller
    {
        readonly IHostingEnvironment hostingEnvironment;

        public ConsoleController(IHostingEnvironment env)
        {
            hostingEnvironment = env;
        }

        public IActionResult Index()
        {
            Program.Database.Operations.GetConsoles(out List<Machine> consoles);

            ViewBag.ItemCount = consoles.Count;

            ViewBag.MinYear = consoles.Where(t => t.Year > 1000).Min(t => t.Year);
            ViewBag.MaxYear = consoles.Where(t => t.Year > 1000).Max(t => t.Year);

            return View();
        }

        public IActionResult ByLetter(char id)
        {
            // ToUpper()
            if(id >= 'a' && id <= 'z') id -= (char)32;
            // Check if not letter
            if(id < 'A' || id > 'Z') id = '\0';

            ViewBag.Letter = id;

            MachineMini[] consoles =
                id == '\0' ? ConsoleMini.GetAllItems() : ConsoleMini.GetItemsStartingWithLetter(id);

            return View(consoles);
        }

        public IActionResult ByYear(int id)
        {
            ViewBag.Year = id;

            return View(ConsoleMini.GetItemsFromYear(id));
        }
    }
}