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
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/licenses")]
[ApiController]
public class LicensesController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<License>> GetAsync() => context.Licenses.OrderBy(l => l.Name)
                                                    .Select(l => new License
                                                     {
                                                         FsfApproved = l.FsfApproved,
                                                         Id          = l.Id,
                                                         Link        = l.Link,
                                                         Name        = l.Name,
                                                         OsiApproved = l.OsiApproved,
                                                         SPDX        = l.SPDX
                                                     })
                                                    .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<License> GetAsync(int id) => context.Licenses.Where(l => l.Id == id)
                                                    .Select(l => new License
                                                     {
                                                         FsfApproved = l.FsfApproved,
                                                         Id          = l.Id,
                                                         Link        = l.Link,
                                                         Name        = l.Name,
                                                         OsiApproved = l.OsiApproved,
                                                         SPDX        = l.SPDX,
                                                         Text        = l.Text
                                                     })
                                                    .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateAsync(License viewModel)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        License model = await context.Licenses.FindAsync(viewModel.Id);

        if(model is null) return NotFound();

        model.FsfApproved = viewModel.FsfApproved;
        model.Link        = viewModel.Link;
        model.Name        = viewModel.Name;
        model.OsiApproved = viewModel.OsiApproved;
        model.SPDX        = viewModel.SPDX;
        model.Text        = viewModel.Text;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> CreateAsync(License viewModel)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new License
        {
            FsfApproved = viewModel.FsfApproved,
            Link        = viewModel.Link,
            Name        = viewModel.Name,
            OsiApproved = viewModel.OsiApproved,
            SPDX        = viewModel.SPDX,
            Text        = viewModel.Text
        };

        await context.Licenses.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        License item = await context.Licenses.FindAsync(id);

        if(item is null) return NotFound();

        context.Licenses.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}