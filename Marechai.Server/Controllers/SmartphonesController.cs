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

[Route("/smartphones")]
[ApiController]
public class SmartphonesController(MarechaiContext context, IMemoryCache cache) : ControllerBase
{
    // Cache keys + TTL for the /smartphones landing-page aggregate endpoints. These
    // are queried on every Smartphones landing-page render but barely change
    // between requests, so a short memory cache turns repeated COUNT()s and
    // company-list materializations into ~0 ms hits.
    const           string   SMARTPHONES_COUNT_KEY            = "smartphones:count";
    const           string   SMARTPHONES_MIN_YEAR_KEY         = "smartphones:min-year";
    const           string   SMARTPHONES_MAX_YEAR_KEY         = "smartphones:max-year";
    const           string   SMARTPHONES_PROTOTYPES_COUNT_KEY = "smartphones:proto:count";
    const           string   SMARTPHONES_COMPANIES_KEY        = "smartphones:companies";
    static readonly TimeSpan _catalogCacheTtl                 = TimeSpan.FromMinutes(5);

    static string LetterCountKey(char c)     => $"smartphones:count:letter:{char.ToUpperInvariant(c)}";
    static string YearCountKey(int year)     => $"smartphones:count:year:{year}";
    static string LetterCompaniesKey(char c) => $"smartphones:companies:letter:{char.ToUpperInvariant(c)}";

    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetSmartphonesCountAsync()
    {
        if(cache.TryGetValue(SMARTPHONES_COUNT_KEY, out int cached)) return cached;

        int total = await context.Machines.CountAsync(c => c.Type == MachineType.Smartphone);
        cache.Set(SMARTPHONES_COUNT_KEY, total, _catalogCacheTtl);

        return total;
    }

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMinimumYearAsync()
    {
        if(cache.TryGetValue(SMARTPHONES_MIN_YEAR_KEY, out int cached)) return cached;

        int min = await context.Machines
                               .Where(t => t.Type == MachineType.Smartphone &&
                                           t.Introduced.HasValue            &&
                                           !t.Prototype)
                               .MinAsync(t => (int?)t.Introduced.Value.Year) ?? 0;

        cache.Set(SMARTPHONES_MIN_YEAR_KEY, min, _catalogCacheTtl);

        return min;
    }

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMaximumYearAsync()
    {
        if(cache.TryGetValue(SMARTPHONES_MAX_YEAR_KEY, out int cached)) return cached;

        int max = await context.Machines
                               .Where(t => t.Type == MachineType.Smartphone &&
                                           t.Introduced.HasValue            &&
                                           !t.Prototype)
                               .MaxAsync(t => (int?)t.Introduced.Value.Year) ?? 0;

        cache.Set(SMARTPHONES_MAX_YEAR_KEY, max, _catalogCacheTtl);

        return max;
    }

    [HttpGet("by-letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetSmartphonesByLetterAsync(char c, [FromQuery] int? skip = null,
                                                              [FromQuery] int? take = null,
                                                              CancellationToken cancellationToken = default)
    {
        IQueryable<Machine> ordered = context.Machines.Include(m => m.Company)
                                             .Where(m => m.Type == MachineType.Smartphone &&
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
    public async Task<int> GetSmartphonesByLetterCountAsync(char c, CancellationToken cancellationToken = default)
    {
        string key = LetterCountKey(c);
        if(cache.TryGetValue(key, out int cached)) return cached;

        int total = await context.Machines
                                 .Where(m => m.Type == MachineType.Smartphone && EF.Functions.Like(m.Name, $"{c}%"))
                                 .CountAsync(cancellationToken);

        cache.Set(key, total, _catalogCacheTtl);

        return total;
    }

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetSmartphonesByYearAsync(int year, [FromQuery] int? skip = null,
                                                            [FromQuery] int? take = null,
                                                            CancellationToken cancellationToken = default)
    {
        IQueryable<Machine> ordered = context.Machines.Include(m => m.Company)
                                             .Where(m => m.Type == MachineType.Smartphone &&
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
    public async Task<int> GetSmartphonesByYearCountAsync(int year, CancellationToken cancellationToken = default)
    {
        string key = YearCountKey(year);
        if(cache.TryGetValue(key, out int cached)) return cached;

        int total = await context.Machines
                                 .Where(m => m.Type == MachineType.Smartphone &&
                                             m.Introduced != null             &&
                                             m.Introduced.Value.Year == year)
                                 .CountAsync(cancellationToken);

        cache.Set(key, total, _catalogCacheTtl);

        return total;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetSmartphonesAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                                      CancellationToken cancellationToken = default)
    {
        IQueryable<Machine> ordered = context.Machines.Include(m => m.Company)
                                             .Where(m => m.Type == MachineType.Smartphone)
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
                                             .Where(m => m.Type == MachineType.Smartphone && m.Prototype)
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
        if(cache.TryGetValue(SMARTPHONES_PROTOTYPES_COUNT_KEY, out int cached)) return cached;

        int total = await context.Machines
                                 .Where(m => m.Type == MachineType.Smartphone && m.Prototype)
                                 .CountAsync(cancellationToken);

        cache.Set(SMARTPHONES_PROTOTYPES_COUNT_KEY, total, _catalogCacheTtl);

        return total;
    }

    [HttpGet("companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<CompanyDto>> GetCompaniesAsync()
    {
        if(cache.TryGetValue(SMARTPHONES_COMPANIES_KEY, out List<CompanyDto> cached) && cached is not null)
            return cached;

        List<CompanyDto> companies = await context.Machines
                                                  .Where(m => m.Type == MachineType.Smartphone)
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

        cache.Set(SMARTPHONES_COMPANIES_KEY, companies, _catalogCacheTtl);

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
                                                  .Where(m => m.Type == MachineType.Smartphone)
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
