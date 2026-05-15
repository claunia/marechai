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

[Route("/iso31661-numeric")]
[ApiController]
public class Iso31661NumericController(MarechaiContext context, IMemoryCache cache) : ControllerBase
{
    // ISO 3166-1 country list — reference standard, updated ~yearly. 24-hour TTL.
    const           string   ISO_31661_ALL_KEY = "iso31661:all";
    static readonly TimeSpan _referenceTtl     = TimeSpan.FromHours(24);

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<Iso31661NumericDto>> GetAsync()
    {
        if(cache.TryGetValue(ISO_31661_ALL_KEY, out List<Iso31661NumericDto> cached) && cached is not null)
            return cached;

        List<Iso31661NumericDto> list = await context.Iso31661Numeric.OrderBy(c => c.Name)
                                                     .Select(c => new Iso31661NumericDto
                                                      {
                                                          Id   = c.Id,
                                                          Name = c.Name
                                                      })
                                                     .ToListAsync();

        cache.Set(ISO_31661_ALL_KEY, list, _referenceTtl);

        return list;
    }
}