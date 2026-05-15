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

using System;
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
using Microsoft.Extensions.Caching.Memory;

namespace Marechai.Server.Controllers;

[Route("/un-m49")]
[ApiController]
public class UnM49Controller(MarechaiContext context, IMemoryCache cache) : ControllerBase
{
    // UN M.49 geographic region list — reference standard, updated ~yearly. 24-hour TTL.
    const           string   UN_M49_ALL_KEY       = "un-m49:all";
    const           string   UN_M49_REGIONS_KEY   = "un-m49:regions";
    const           string   UN_M49_COUNTRIES_KEY = "un-m49:countries";
    static readonly TimeSpan _referenceTtl        = TimeSpan.FromHours(24);

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<UnM49Dto>> GetAsync()
    {
        if(cache.TryGetValue(UN_M49_ALL_KEY, out List<UnM49Dto> cached) && cached is not null) return cached;

        List<UnM49Dto> list = await context.UnM49
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

        cache.Set(UN_M49_ALL_KEY, list, _referenceTtl);

        return list;
    }

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
    public async Task<List<UnM49Dto>> GetRegionsAsync()
    {
        if(cache.TryGetValue(UN_M49_REGIONS_KEY, out List<UnM49Dto> cached) && cached is not null) return cached;

        List<UnM49Dto> list = await context.UnM49
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

        cache.Set(UN_M49_REGIONS_KEY, list, _referenceTtl);

        return list;
    }

    [HttpGet("countries")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<UnM49Dto>> GetCountriesAsync()
    {
        if(cache.TryGetValue(UN_M49_COUNTRIES_KEY, out List<UnM49Dto> cached) && cached is not null) return cached;

        List<UnM49Dto> list = await context.UnM49
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

        cache.Set(UN_M49_COUNTRIES_KEY, list, _referenceTtl);

        return list;
    }
}
