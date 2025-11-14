/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
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
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Marechai.Services;

public class NewsService(MarechaiContext context, IStringLocalizer<NewsService> localizer)
{
    public async Task<List<NewsDto>> GetAsync() => await context.News.OrderByDescending(n => n.Date)
                                                                .Select(n => new NewsDto
                                                                 {
                                                                     Id         = n.Id,
                                                                     Timestamp  = n.Date,
                                                                     Type       = n.Type,
                                                                     AffectedId = n.AddedId
                                                                 })
                                                                .ToListAsync();

    public List<NewsDto> GetNews()
    {
        List<NewsDto> news = new();

        foreach(News @new in context.News.OrderByDescending(t => t.Date).Take(10).ToList())
        {
            Machine machine = context.Machines.Find(@new.AddedId);

            if(machine is null) continue;

            switch(@new.Type)
            {
                case NewsType.NewComputerInDb:
                    news.Add(new NewsDto(@new.AddedId,
                                         localizer["New computer in database"],
                                         @new.Date,
                                         "machine",
                                         $"{machine.Company.Name} {machine.Name}"));

                    break;
                case NewsType.NewConsoleInDb:
                    news.Add(new NewsDto(@new.AddedId,
                                         localizer["New console in database"],
                                         @new.Date,
                                         "machine",
                                         $"{machine.Company.Name} {machine.Name}"));

                    break;

                case NewsType.NewComputerInCollection:
                    news.Add(new NewsDto(@new.AddedId,
                                         localizer["New computer in collection"],
                                         @new.Date,
                                         "machine",
                                         $"{machine.Company.Name} {machine.Name}"));

                    break;

                case NewsType.NewConsoleInCollection:
                    news.Add(new NewsDto(@new.AddedId,
                                         localizer["New console in collection"],
                                         @new.Date,
                                         "machine",
                                         $"{machine.Company.Name} {machine.Name}"));

                    break;

                case NewsType.UpdatedComputerInDb:
                    news.Add(new NewsDto(@new.AddedId,
                                         localizer["Updated computer in database"],
                                         @new.Date,
                                         "machine",
                                         $"{machine.Company.Name} {machine.Name}"));

                    break;

                case NewsType.UpdatedConsoleInDb:
                    news.Add(new NewsDto(@new.AddedId,
                                         localizer["Updated console in database"],
                                         @new.Date,
                                         "machine",
                                         $"{machine.Company.Name} {machine.Name}"));

                    break;

                case NewsType.UpdatedComputerInCollection:
                    news.Add(new NewsDto(@new.AddedId,
                                         localizer["Updated computer in collection"],
                                         @new.Date,
                                         "machine",
                                         $"{machine.Company.Name} {machine.Name}"));

                    break;

                case NewsType.UpdatedConsoleInCollection:
                    news.Add(new NewsDto(@new.AddedId,
                                         localizer["Updated console in collection"],
                                         @new.Date,
                                         "machine",
                                         $"{machine.Company.Name} {machine.Name}"));

                    break;

                case NewsType.NewMoneyDonation:
                    // TODO
                    break;

                default:
                    continue;
            }
        }

        return news;
    }

    public async Task DeleteAsync(int id, string userId)
    {
        News item = await context.News.FindAsync(id);

        if(item is null) return;

        context.News.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }
}