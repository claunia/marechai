/*******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ---------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] -----------------------------------------------------------
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
// ---------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/news")]
[ApiController]
public class NewsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<NewsDto>> GetAsync() => context.News.OrderByDescending(n => n.Date)
                                                    .Select(n => new NewsDto
                                                     {
                                                         Id         = n.Id,
                                                         Timestamp  = n.Date,
                                                         Type       = n.Type,
                                                         AffectedId = n.AddedId,
                                                         Name       = n.Name
                                                     })
                                                    .ToListAsync();

    [HttpGet("latest")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public List<NewsDto> GetNews()
    {
        List<News> latestNews = context.News.OrderByDescending(t => t.Date).Take(10).ToList();

        List<NewsDto> news = [];

        foreach(News @new in latestNews)
        {
            string controller;
            string itemName = @new.Name;

            switch(@new.Type)
            {
                case NewsType.NewComputerInDb:
                case NewsType.NewComputerInCollection:
                case NewsType.UpdatedComputerInDb:
                case NewsType.UpdatedComputerInCollection:
                    controller = "computers";

                    if(string.IsNullOrEmpty(itemName))
                    {
                        Machine machine = context.Machines.Include(m => m.Company)
                                                 .FirstOrDefault(m => m.Id == @new.AddedId);

                        if(machine is not null) itemName = $"{machine.Company?.Name} {machine.Name}";
                    }

                    break;

                case NewsType.NewConsoleInDb:
                case NewsType.NewConsoleInCollection:
                case NewsType.UpdatedConsoleInDb:
                case NewsType.UpdatedConsoleInCollection:
                    controller = "consoles";

                    if(string.IsNullOrEmpty(itemName))
                    {
                        Machine console = context.Machines.Include(m => m.Company)
                                                 .FirstOrDefault(m => m.Id == @new.AddedId);

                        if(console is not null) itemName = $"{console.Company?.Name} {console.Name}";
                    }

                    break;

                case NewsType.NewMoneyDonation:
                    // TODO
                    continue;

                case NewsType.NewBookInDb or NewsType.UpdatedBookInDb:
                    controller = "books";

                    break;

                case NewsType.NewDocumentInDb or NewsType.UpdatedDocumentInDb:
                    controller = "documents";

                    break;

                case NewsType.NewMagazineInDb or NewsType.UpdatedMagazineInDb:
                    controller = "magazines";

                    break;

                case NewsType.NewPersonInDb or NewsType.UpdatedPersonInDb:
                    controller = "people";

                    break;

                case NewsType.NewSoftwareInDb or NewsType.UpdatedSoftwareInDb:
                    controller = "software";

                    break;

                case NewsType.NewSoftwareVersionInDb or NewsType.UpdatedSoftwareVersionInDb:
                    controller = "software-versions";

                    break;

                case NewsType.NewSoftwareReleaseInDb or NewsType.UpdatedSoftwareReleaseInDb:
                    controller = "software-releases";

                    break;

                case NewsType.NewGpuInDb or NewsType.UpdatedGpuInDb:
                    controller = "gpus";

                    break;

                case NewsType.NewSoundSynthInDb or NewsType.UpdatedSoundSynthInDb:
                    controller = "sound-synths";

                    break;

                case NewsType.NewProcessorInDb or NewsType.UpdatedProcessorInDb:
                    controller = "processors";

                    break;

                default:
                    continue;
            }

            news.Add(new NewsDto
            {
                Id         = @new.Id,
                AffectedId = @new.AddedId,
                Timestamp  = @new.Date,
                Type       = @new.Type,
                Controller = controller,
                ItemName   = itemName,
                Name       = @new.Name
            });
        }

        return news;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        News item = await context.News.FindAsync(id);

        if(item is null) return NotFound();

        context.News.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}