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
using Marechai.Data.Dtos;
using Marechai.Database;
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
                                                         AffectedId = n.AddedId
                                                     })
                                                    .ToListAsync();

    [HttpGet("latest")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public List<NewsDto> GetNews()
    {
        List<NewsDto> news = [];

        var newsWithMachines = context.News.OrderByDescending(t => t.Date)
                                      .Take(10)
                                      .Join(context.Machines.Include(m => m.Company),
                                            n => n.AddedId,
                                            m => m.Id,
                                            (n, m) => new
                                            {
                                                News    = n,
                                                Machine = m
                                            })
                                      .ToList();

        foreach(var item in newsWithMachines)
        {
            News    @new    = item.News;
            Machine machine = item.Machine;

            if(machine is null) continue;

            switch(@new.Type)
            {
                case NewsType.NewComputerInDb:
                case NewsType.NewConsoleInDb:
                case NewsType.NewComputerInCollection:
                case NewsType.NewConsoleInCollection:
                case NewsType.UpdatedComputerInDb:
                case NewsType.UpdatedConsoleInDb:
                case NewsType.UpdatedComputerInCollection:
                case NewsType.UpdatedConsoleInCollection:
                    news.Add(new NewsDto
                    {
                        Id         = @new.Id,
                        AffectedId = @new.AddedId,
                        Timestamp  = @new.Date,
                        Type       = @new.Type,
                        Controller =
                            @new.Type is NewsType.NewComputerInDb
                                      or NewsType.NewComputerInCollection
                                      or NewsType.UpdatedComputerInDb
                                      or NewsType.UpdatedComputerInCollection
                                ? "computers"
                                : "consoles",
                        ItemName = $"{machine.Company.Name} {machine.Name}"
                    });

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