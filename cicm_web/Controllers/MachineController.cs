/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ConsoleController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Machine controller
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

namespace cicm_web.Controllers
{
    public class MachineController : Controller
    {
        readonly cicmContext         _context;
        readonly IHostingEnvironment hostingEnvironment;

        public MachineController(IHostingEnvironment env, cicmContext context)
        {
            hostingEnvironment = env;
            _context           = context;
        }

        public IActionResult View(int id)
        {
            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;

            return View(_context.Machines.FirstOrDefault(m => m.Id == id));
        }
    }
}