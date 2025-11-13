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
// Copyright © 2003-2025 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/dumps")]
[ApiController]
public class DumpsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<DumpDto>> GetAsync() => context.Dumps.OrderBy(d => d.Dumper)
                                                    .ThenBy(d => d.DumpingGroup)
                                                    .ThenBy(b => b.Media.Title)
                                                    .ThenBy(d => d.DumpDate)
                                                    .Select(d => new DumpDto
                                                     {
                                                         Id           = d.Id,
                                                         Dumper       = d.Dumper,
                                                         UserId       = d.UserId,
                                                         DumpingGroup = d.DumpingGroup,
                                                         DumpDate     = d.DumpDate,
                                                         UserName     = d.User.UserName,
                                                         MediaId      = d.MediaId,
                                                         MediaTitle   = d.Media.Title,
                                                         MediaDumpId  = d.MediaDumpId
                                                     })
                                                    .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<DumpDto> GetAsync(ulong id) => context.Dumps.Where(d => d.Id == id)
                                                      .Select(d => new DumpDto
                                                       {
                                                           Id           = d.Id,
                                                           Dumper       = d.Dumper,
                                                           UserId       = d.User.Id,
                                                           DumpingGroup = d.DumpingGroup,
                                                           DumpDate     = d.DumpDate,
                                                           UserName     = d.User.UserName,
                                                           MediaId      = d.MediaId,
                                                           MediaTitle   = d.Media.Title,
                                                           MediaDumpId  = d.MediaDumpId
                                                       })
                                                      .FirstOrDefaultAsync();

    [HttpPut("{id:ulong}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ulong id, [FromBody] DumpDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Dump model = await context.Dumps.FindAsync(id);

        if(model is null) return NotFound();

        model.Dumper       = dto.Dumper;
        model.UserId       = dto.UserId;
        model.DumpingGroup = dto.DumpingGroup;
        model.DumpDate     = dto.DumpDate;
        model.MediaId      = dto.MediaId;
        model.MediaDumpId  = dto.MediaDumpId;
        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ulong>> CreateAsync([FromBody] DumpDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Dump
        {
            Dumper       = dto.Dumper,
            UserId       = dto.UserId,
            DumpingGroup = dto.DumpingGroup,
            DumpDate     = dto.DumpDate,
            MediaId      = dto.MediaId,
            MediaDumpId  = dto.MediaDumpId
        };

        await context.Dumps.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(ulong id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Dump item = await context.Dumps.FindAsync(id);

        if(item is null) return NotFound();

        context.Dumps.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}