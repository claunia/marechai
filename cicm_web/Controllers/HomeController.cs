/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : HomeController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Home controller
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
using System.Diagnostics;
using System.Linq;
using Cicm.Database;
using Cicm.Database.Models;
using cicm_web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace cicm_web.Controllers
{
    public class HomeController : Controller
    {
        readonly cicmContext         _context;
        readonly IHostingEnvironment hostingEnvironment;

        public HomeController(IHostingEnvironment env, cicmContext context)
        {
            hostingEnvironment = env;
            _context           = context;
        }

        public IActionResult Index()
        {
            ViewBag.WebRootPath = hostingEnvironment.WebRootPath;

            List<NewsModel> news = new List<NewsModel>();

            foreach(News @new in _context.News.OrderByDescending(t => t.Date).Take(10))
            {
                Machine machine = _context.Machines.Find(@new.AddedId);

                if(machine is null) continue;

                switch(@new.Type)
                {
                    case NewsType.NewComputerInDb:
                        news.Add(new NewsModel(@new.AddedId, "New computer in database", @new.Date, "Machine", "View",
                                               $"{machine.Company.Name} {machine.Name}"));
                        break;
                    case NewsType.NewConsoleInDb:
                        news.Add(new NewsModel(@new.AddedId, "New console in database", @new.Date, "Machine", "View",
                                               $"{machine.Company.Name} {machine.Name}"));
                        break;

                    case NewsType.NewComputerInCollection:
                        news.Add(new NewsModel(@new.AddedId, "New computer in collection", @new.Date, "Machine", "View",
                                               $"{machine.Company.Name} {machine.Name}"));
                        break;

                    case NewsType.NewConsoleInCollection:
                        news.Add(new NewsModel(@new.AddedId, "New console in collection", @new.Date, "Machine", "View",
                                               $"{machine.Company.Name} {machine.Name}"));
                        break;

                    case NewsType.UpdatedComputerInDb:
                        news.Add(new NewsModel(@new.AddedId, "Updated computer in database", @new.Date, "Machine",
                                               "View", $"{machine.Company.Name} {machine.Name}"));
                        break;

                    case NewsType.UpdatedConsoleInDb:
                        news.Add(new NewsModel(@new.AddedId, "Updated console in database", @new.Date, "Machine",
                                               "View", $"{machine.Company.Name} {machine.Name}"));
                        break;

                    case NewsType.UpdatedComputerInCollection:
                        news.Add(new NewsModel(@new.AddedId, "Updated computer in collection", @new.Date, "Machine",
                                               "View", $"{machine.Company.Name} {machine.Name}"));
                        break;

                    case NewsType.UpdatedConsoleInCollection:
                        news.Add(new NewsModel(@new.AddedId, "Updated console in collection", @new.Date, "Machine",
                                               "View", $"{machine.Company.Name} {machine.Name}"));
                        break;

                    case NewsType.NewMoneyDonation:
                        // TODO
                        break;

                    default: continue;
                }
            }

            return View(news);
        }

        public IActionResult About() => View();

        public IActionResult Contact() => View();

        public IActionResult Error() =>
            View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}