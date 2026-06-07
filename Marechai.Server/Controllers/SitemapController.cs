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
using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

/// <summary>
///     Lightweight endpoint that returns only entity IDs for sitemap generation.
///     Avoids the overhead of full DTO projections, joins, and serialization.
/// </summary>
[Route("/sitemap")]
[ApiController]
public class SitemapController(MarechaiContext context) : ControllerBase
{
    [HttpGet("machines/ids")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<int>> GetMachineIdsAsync([FromQuery] int? skip = null,
                                              [FromQuery] int? take = null,
                                              CancellationToken ct  = default)
    {
        IQueryable<int> query = context.Machines.AsNoTracking().Select(m => m.Id).OrderBy(id => id);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.ToListAsync(ct);
    }

    [HttpGet("companies/ids")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<int>> GetCompanyIdsAsync([FromQuery] int? skip = null,
                                              [FromQuery] int? take = null,
                                              CancellationToken ct  = default)
    {
        IQueryable<int> query = context.Companies.AsNoTracking().Select(c => c.Id).OrderBy(id => id);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.ToListAsync(ct);
    }

    [HttpGet("software/ids")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<ulong>> GetSoftwareIdsAsync([FromQuery] int? skip = null,
                                                  [FromQuery] int? take = null,
                                                  CancellationToken ct  = default)
    {
        IQueryable<ulong> query = context.Softwares.AsNoTracking().Select(s => s.Id).OrderBy(id => id);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.ToListAsync(ct);
    }

    [HttpGet("books/ids")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<long>> GetBookIdsAsync([FromQuery] int? skip = null,
                                            [FromQuery] int? take = null,
                                            CancellationToken ct  = default)
    {
        IQueryable<long> query = context.Books.AsNoTracking().Select(b => b.Id).OrderBy(id => id);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.ToListAsync(ct);
    }

    [HttpGet("documents/ids")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<long>> GetDocumentIdsAsync([FromQuery] int? skip = null,
                                                [FromQuery] int? take = null,
                                                CancellationToken ct  = default)
    {
        IQueryable<long> query = context.Documents.AsNoTracking().Select(d => d.Id).OrderBy(id => id);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.ToListAsync(ct);
    }

    [HttpGet("magazines/ids")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<long>> GetMagazineIdsAsync([FromQuery] int? skip = null,
                                                [FromQuery] int? take = null,
                                                CancellationToken ct  = default)
    {
        IQueryable<long> query = context.Magazines.AsNoTracking().Select(m => m.Id).OrderBy(id => id);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.ToListAsync(ct);
    }

    [HttpGet("gpus/ids")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<int>> GetGpuIdsAsync([FromQuery] int? skip = null,
                                           [FromQuery] int? take = null,
                                           CancellationToken ct  = default)
    {
        IQueryable<int> query = context.Gpus.AsNoTracking().Select(g => g.Id).OrderBy(id => id);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.ToListAsync(ct);
    }

    [HttpGet("sound-synths/ids")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<int>> GetSoundSynthIdsAsync([FromQuery] int? skip = null,
                                                  [FromQuery] int? take = null,
                                                  CancellationToken ct  = default)
    {
        IQueryable<int> query = context.SoundSynths.AsNoTracking().Select(s => s.Id).OrderBy(id => id);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.ToListAsync(ct);
    }

    [HttpGet("processors/ids")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<int>> GetProcessorIdsAsync([FromQuery] int? skip = null,
                                                [FromQuery] int? take = null,
                                                CancellationToken ct  = default)
    {
        IQueryable<int> query = context.Processors.AsNoTracking().Select(p => p.Id).OrderBy(id => id);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.ToListAsync(ct);
    }

    [HttpGet("people/ids")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<int>> GetPersonIdsAsync([FromQuery] int? skip = null,
                                              [FromQuery] int? take = null,
                                              CancellationToken ct  = default)
    {
        IQueryable<int> query = context.People.AsNoTracking().Select(p => p.Id).OrderBy(id => id);

        if(skip.HasValue) query = query.Skip(skip.Value);
        if(take.HasValue) query = query.Take(take.Value);

        return query.ToListAsync(ct);
    }
}
