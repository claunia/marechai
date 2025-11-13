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

using System;
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
using Microsoft.Extensions.Localization;

namespace Marechai.Server.Controllers;

[Route("/instruction-set-extensions")]
[ApiController]
public class InstructionSetExtensionsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<InstructionSetExtension>> GetAsync() => await context.InstructionSetExtensions
                                                                                .OrderBy(e => e.Extension)
                                                                                .Select(e => new InstructionSetExtension
                                                                                 {
                                                                                     Extension = e.Extension,
                                                                                     Id        = e.Id
                                                                                 })
                                                                                .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<InstructionSetExtension> GetAsync(int id) => await context.InstructionSetExtensions
                                                                      .Where(e => e.Id == id)
                                                                      .Select(e => new InstructionSetExtension
                                                                       {
                                                                           Extension = e.Extension,
                                                                           Id        = e.Id
                                                                       })
                                                                      .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task UpdateAsync(InstructionSetExtension viewModel)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        InstructionSetExtension model = await context.InstructionSetExtensions.FindAsync(viewModel.Id);

        if(model is null) return;

        model.Extension = viewModel.Extension;

        await context.SaveChangesWithUserAsync(userId);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> CreateAsync(InstructionSetExtension viewModel)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return 0;
        var model = new InstructionSetExtension
        {
            Extension = viewModel.Extension
        };

        await context.InstructionSetExtensions.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return;
        InstructionSetExtension item = await context.InstructionSetExtensions.FindAsync(id);

        if(item is null) return;

        context.InstructionSetExtensions.Remove(item);

        await context.SaveChangesWithUserAsync(userId);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public bool VerifyUnique(string extension) =>
        !context.InstructionSetExtensions.Any(i => string.Equals(i.Extension,
                                                                  extension,
                                                                  StringComparison.InvariantCultureIgnoreCase));
}
