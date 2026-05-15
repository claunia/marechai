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
using System.Threading;
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

[Route("/computers")]
[ApiController]
public class ComputersController(MarechaiContext context, IMemoryCache cache) : ControllerBase
{
    // Cache keys + TTL for the /computers landing-page aggregate endpoints. These
    // are queried on every Computers landing-page render but barely change
    // between requests, so a short memory cache turns repeated COUNT()s and
    // company-list materializations into ~0 ms hits.
    const           string   COMPUTERS_COUNT_KEY            = "computers:count";
    const           string   COMPUTERS_MIN_YEAR_KEY         = "computers:min-year";
    const           string   COMPUTERS_MAX_YEAR_KEY         = "computers:max-year";
    const           string   COMPUTERS_PROTOTYPES_COUNT_KEY = "computers:proto:count";
    const           string   COMPUTERS_COMPANIES_KEY        = "computers:companies";
    static readonly TimeSpan _catalogCacheTtl               = TimeSpan.FromMinutes(5);

    static string LetterCountKey(char c)  => $"computers:count:letter:{char.ToUpperInvariant(c)}";
    static string YearCountKey(int year)  => $"computers:count:year:{year}";
    static string LetterCompaniesKey(char c) => $"computers:companies:letter:{char.ToUpperInvariant(c)}";

    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetComputersCountAsync()
    {
        if(cache.TryGetValue(COMPUTERS_COUNT_KEY, out int cached)) return cached;

        int total = await context.Machines.CountAsync(c => c.Type == MachineType.Computer);
        cache.Set(COMPUTERS_COUNT_KEY, total, _catalogCacheTtl);

        return total;
    }

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMinimumYearAsync()
    {
        if(cache.TryGetValue(COMPUTERS_MIN_YEAR_KEY, out int cached)) return cached;

        int min = await context.Machines
                               .Where(t => t.Type == MachineType.Computer &&
                                           t.Introduced.HasValue          &&
                                           !t.Prototype)
                               .MinAsync(t => (int?)t.Introduced.Value.Year) ?? 0;

        cache.Set(COMPUTERS_MIN_YEAR_KEY, min, _catalogCacheTtl);

        return min;
    }

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMaximumYearAsync()
    {
        if(cache.TryGetValue(COMPUTERS_MAX_YEAR_KEY, out int cached)) return cached;

        int max = await context.Machines
                               .Where(t => t.Type == MachineType.Computer &&
                                           t.Introduced.HasValue          &&
                                           !t.Prototype)
                               .MaxAsync(t => (int?)t.Introduced.Value.Year) ?? 0;

        cache.Set(COMPUTERS_MAX_YEAR_KEY, max, _catalogCacheTtl);

        return max;
    }

    [HttpGet("by-letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetComputersByLetterAsync(char c, [FromQuery] int? skip = null,
                                                            [FromQuery] int? take = null,
                                                            CancellationToken cancellationToken = default)
    {
        IQueryable<Machine> ordered = context.Machines.Include(m => m.Company)
                                             .Where(m => m.Type == MachineType.Computer &&
                                                         EF.Functions.Like(m.Name, $"{c}%"))
                                             .OrderBy(m => MarechaiContext.NaturalSortKey(m.Company.Name))
                                             .ThenBy(m => MarechaiContext.NaturalSortKey(m.Name));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(m => new MachineDto
                       {
                           Id         = m.Id,
                           Name       = m.Name,
                           Company    = m.Company.Name,
                           Introduced = m.Introduced
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("by-letter/{c}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetComputersByLetterCountAsync(char c, CancellationToken cancellationToken = default)
    {
        string key = LetterCountKey(c);
        if(cache.TryGetValue(key, out int cached)) return cached;

        int total = await context.Machines
                                 .Where(m => m.Type == MachineType.Computer && EF.Functions.Like(m.Name, $"{c}%"))
                                 .CountAsync(cancellationToken);

        cache.Set(key, total, _catalogCacheTtl);

        return total;
    }

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetComputersByYearAsync(int year, [FromQuery] int? skip = null,
                                                          [FromQuery] int? take = null,
                                                          CancellationToken cancellationToken = default)
    {
        IQueryable<Machine> ordered = context.Machines.Include(m => m.Company)
                                             .Where(m => m.Type == MachineType.Computer &&
                                                         m.Introduced != null &&
                                                         m.Introduced.Value.Year == year)
                                             .OrderBy(m => MarechaiContext.NaturalSortKey(m.Company.Name))
                                             .ThenBy(m => MarechaiContext.NaturalSortKey(m.Name));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(m => new MachineDto
                       {
                           Id         = m.Id,
                           Name       = m.Name,
                           Company    = m.Company.Name,
                           Introduced = m.Introduced
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("by-year/{year:int}/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetComputersByYearCountAsync(int year, CancellationToken cancellationToken = default)
    {
        string key = YearCountKey(year);
        if(cache.TryGetValue(key, out int cached)) return cached;

        int total = await context.Machines
                                 .Where(m => m.Type == MachineType.Computer &&
                                             m.Introduced != null           &&
                                             m.Introduced.Value.Year == year)
                                 .CountAsync(cancellationToken);

        cache.Set(key, total, _catalogCacheTtl);

        return total;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetComputersAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                                    CancellationToken cancellationToken = default)
    {
        IQueryable<Machine> ordered = context.Machines.Include(m => m.Company)
                                             .Where(m => m.Type == MachineType.Computer)
                                             .OrderBy(m => MarechaiContext.NaturalSortKey(m.Company.Name))
                                             .ThenBy(m => MarechaiContext.NaturalSortKey(m.Name));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(m => new MachineDto
                       {
                           Id         = m.Id,
                           Name       = m.Name,
                           Company    = m.Company.Name,
                           Introduced = m.Introduced
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("prototypes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetPrototypesAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                                     CancellationToken cancellationToken = default)
    {
        IQueryable<Machine> ordered = context.Machines.Include(m => m.Company)
                                             .Where(m => m.Type == MachineType.Computer && m.Prototype)
                                             .OrderBy(m => MarechaiContext.NaturalSortKey(m.Company.Name))
                                             .ThenBy(m => MarechaiContext.NaturalSortKey(m.Name));

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(m => new MachineDto
                       {
                           Id        = m.Id,
                           Name      = m.Name,
                           Company   = m.Company.Name,
                           Prototype = m.Prototype
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("prototypes/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetPrototypesCountAsync(CancellationToken cancellationToken = default)
    {
        if(cache.TryGetValue(COMPUTERS_PROTOTYPES_COUNT_KEY, out int cached)) return cached;

        int total = await context.Machines
                                 .Where(m => m.Type == MachineType.Computer && m.Prototype)
                                 .CountAsync(cancellationToken);

        cache.Set(COMPUTERS_PROTOTYPES_COUNT_KEY, total, _catalogCacheTtl);

        return total;
    }

    [HttpGet("companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<CompanyDto>> GetCompaniesAsync()
    {
        if(cache.TryGetValue(COMPUTERS_COMPANIES_KEY, out List<CompanyDto> cached) && cached is not null)
            return cached;

        List<CompanyDto> companies = await context.Machines
                                                  .Where(m => m.Type == MachineType.Computer)
                                                  .Select(m => m.Company)
                                                  .Distinct()
                                                  .Include(c => c.Logos)
                                                  .OrderBy(c => MarechaiContext.NaturalSortKey(c.Name))
                                                  .Select(c => new CompanyDto
                                                   {
                                                       Id = c.Id,
                                                       LastLogo =
                                                           c.Logos.OrderByDescending(l => l.Year)
                                                            .FirstOrDefault()
                                                            .Guid,
                                                       Name = c.Name
                                                   })
                                                  .ToListAsync();

        cache.Set(COMPUTERS_COMPANIES_KEY, companies, _catalogCacheTtl);

        return companies;
    }

    [HttpGet("companies/letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<CompanyDto>> GetCompaniesByLetterAsync(char c)
    {
        string key = LetterCompaniesKey(c);
        if(cache.TryGetValue(key, out List<CompanyDto> cached) && cached is not null) return cached;

        List<CompanyDto> companies = await context.Machines
                                                  .Where(m => m.Type == MachineType.Computer)
                                                  .Select(m => m.Company)
                                                  .Distinct()
                                                  .Include(c => c.Logos)
                                                  .Where(co => EF.Functions.Like(co.Name, $"{c}%"))
                                                  .OrderBy(co => MarechaiContext.NaturalSortKey(co.Name))
                                                  .Select(co => new CompanyDto
                                                   {
                                                       Id = co.Id,
                                                       LastLogo =
                                                           co.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
                                                       Name = co.Name
                                                   })
                                                  .ToListAsync();

        cache.Set(key, companies, _catalogCacheTtl);

        return companies;
    }
}