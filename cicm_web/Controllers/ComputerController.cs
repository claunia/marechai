/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
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

using System.Linq;
using Marechai.Database;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace cicm_web.Controllers
{
    public class ComputerController : Controller
    {
        readonly cicmContext         _context;
        readonly IHostingEnvironment hostingEnvironment;

        public ComputerController(IHostingEnvironment env, cicmContext context)
        {
            hostingEnvironment = env;
            _context           = context;
        }

        public IActionResult Index()
        {
            ViewBag.ItemCount = _context.Machines.Count(m => m.Type == MachineType.Computer);
            ViewBag.MinYear = _context
                             .Machines.Where(t => t.Type == MachineType.Computer &&
                                                  t.Introduced.HasValue          &&
                                                  t.Introduced.Value.Year > 1000).Min(t => t.Introduced.Value.Year);
            ViewBag.MaxYear = _context
                             .Machines.Where(t => t.Type == MachineType.Computer &&
                                                  t.Introduced.HasValue          &&
                                                  t.Introduced.Value.Year > 1000).Max(t => t.Introduced.Value.Year);

            return View();
        }

        public IActionResult ByLetter(char id)
        {
            // ToUpper()
            if(id >= 'a' && id <= 'z') id -= (char)32;
            // Check if not letter
            if(id < 'A' || id > 'Z') id = '\0';

            ViewBag.Letter = id;

            return View(id == '\0'
                            ? _context.Machines.Where(m => m.Type == MachineType.Computer).ToArray()
                            : _context.Machines.Where(m => m.Type == MachineType.Computer && m.Name.StartsWith(id))
                                      .ToArray());
        }

        public IActionResult ByYear(int id)
        {
            ViewBag.Year = id;

            return View(_context.Machines.Where(t => t.Type == MachineType.Computer &&
                                                     t.Introduced.HasValue          &&
                                                     t.Introduced.Value.Year == id).ToArray());
        }
    }
}