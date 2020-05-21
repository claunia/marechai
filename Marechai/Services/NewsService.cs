/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : NewsService.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     News service
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

using System.Collections.Generic;
using System.Linq;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.ViewModels;

namespace Marechai.Services
{
    public class NewsService
    {
        readonly MarechaiContext _context;

        public NewsService(MarechaiContext context) => _context = context;

        public List<NewsViewModel> GetNews()
        {
            List<NewsViewModel> news = new List<NewsViewModel>();

            foreach(News @new in _context.News.OrderByDescending(t => t.Date).Take(10).ToList())
            {
                Machine machine = _context.Machines.Find(@new.AddedId);

                if(machine is null)
                    continue;

                switch(@new.Type)
                {
                    case NewsType.NewComputerInDb:
                        news.Add(new NewsViewModel(@new.AddedId, "New computer in database", @new.Date, "Machine",
                                                   $"{machine.Company.Name} {machine.Name}"));

                        break;
                    case NewsType.NewConsoleInDb:
                        news.Add(new NewsViewModel(@new.AddedId, "New console in database", @new.Date, "Machine",
                                                   $"{machine.Company.Name} {machine.Name}"));

                        break;

                    case NewsType.NewComputerInCollection:
                        news.Add(new NewsViewModel(@new.AddedId, "New computer in collection", @new.Date, "Machine",
                                                   $"{machine.Company.Name} {machine.Name}"));

                        break;

                    case NewsType.NewConsoleInCollection:
                        news.Add(new NewsViewModel(@new.AddedId, "New console in collection", @new.Date, "Machine",
                                                   $"{machine.Company.Name} {machine.Name}"));

                        break;

                    case NewsType.UpdatedComputerInDb:
                        news.Add(new NewsViewModel(@new.AddedId, "Updated computer in database", @new.Date, "Machine",
                                                   $"{machine.Company.Name} {machine.Name}"));

                        break;

                    case NewsType.UpdatedConsoleInDb:
                        news.Add(new NewsViewModel(@new.AddedId, "Updated console in database", @new.Date, "Machine",
                                                   $"{machine.Company.Name} {machine.Name}"));

                        break;

                    case NewsType.UpdatedComputerInCollection:
                        news.Add(new NewsViewModel(@new.AddedId, "Updated computer in collection", @new.Date, "Machine",
                                                   $"{machine.Company.Name} {machine.Name}"));

                        break;

                    case NewsType.UpdatedConsoleInCollection:
                        news.Add(new NewsViewModel(@new.AddedId, "Updated console in collection", @new.Date, "Machine",
                                                   $"{machine.Company.Name} {machine.Name}"));

                        break;

                    case NewsType.NewMoneyDonation:
                        // TODO
                        break;

                    default: continue;
                }
            }

            return news;
        }
    }
}