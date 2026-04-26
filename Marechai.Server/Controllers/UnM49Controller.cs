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
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/un-m49")]
[ApiController]
public class UnM49Controller(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<UnM49Dto>> GetAsync() => context.UnM49
       .OrderBy(r => r.Type)
       .ThenBy(r => r.Name)
       .Select(r => new UnM49Dto
        {
            Id         = r.Id,
            Name       = r.Name,
            ParentId   = r.ParentId,
            ParentName = r.Parent.Name,
            Type       = r.Type
        })
       .ToListAsync();

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UnM49Dto>> GetAsync(short id)
    {
        UnM49Dto dto = await context.UnM49.Where(r => r.Id == id)
                                    .Select(r => new UnM49Dto
                                     {
                                         Id         = r.Id,
                                         Name       = r.Name,
                                         ParentId   = r.ParentId,
                                         ParentName = r.Parent.Name,
                                         Type       = r.Type
                                     })
                                    .FirstOrDefaultAsync();

        if(dto is null) return NotFound();

        return dto;
    }

    [HttpGet("regions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<UnM49Dto>> GetRegionsAsync() => context.UnM49
       .Where(r => r.Type != UnM49Type.Country)
       .OrderBy(r => r.Type)
       .ThenBy(r => r.Name)
       .Select(r => new UnM49Dto
        {
            Id         = r.Id,
            Name       = r.Name,
            ParentId   = r.ParentId,
            ParentName = r.Parent.Name,
            Type       = r.Type
        })
       .ToListAsync();

    [HttpGet("countries")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<UnM49Dto>> GetCountriesAsync() => context.UnM49
       .Where(r => r.Type == UnM49Type.Country)
       .OrderBy(r => r.Name)
       .Select(r => new UnM49Dto
        {
            Id         = r.Id,
            Name       = r.Name,
            ParentId   = r.ParentId,
            ParentName = r.Parent.Name,
            Type       = r.Type
        })
       .ToListAsync();
}
