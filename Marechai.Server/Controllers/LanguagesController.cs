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
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Marechai.Server.Controllers;

[Route("/languages")]
[ApiController]
public class LanguagesController(MarechaiContext context, IMemoryCache cache) : ControllerBase
{
    // ISO 639 language list — reference standard, essentially static. 24-hour TTL.
    const           string   LANGUAGES_ALL_KEY = "languages:all";
    static readonly TimeSpan _referenceTtl     = TimeSpan.FromHours(24);

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<Iso639Dto>> GetAsync()
    {
        if(cache.TryGetValue(LANGUAGES_ALL_KEY, out List<Iso639Dto> cached) && cached is not null) return cached;

        List<Iso639Dto> list = await context.Iso639
                                            .OrderBy(l => l.ReferenceName)
                                            .Select(l => new Iso639Dto
                                             {
                                                 Id            = l.Id,
                                                 ReferenceName = l.ReferenceName,
                                                 Part1         = l.Part1
                                             })
                                            .ToListAsync();

        cache.Set(LANGUAGES_ALL_KEY, list, _referenceTtl);

        return list;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Iso639Dto>> GetAsync(string id)
    {
        Iso639Dto dto = await context.Iso639.Where(l => l.Id == id)
                                     .Select(l => new Iso639Dto
                                      {
                                          Id            = l.Id,
                                          ReferenceName = l.ReferenceName,
                                          Part1         = l.Part1
                                      })
                                     .FirstOrDefaultAsync();

        if(dto is null) return NotFound();

        return dto;
    }
}
