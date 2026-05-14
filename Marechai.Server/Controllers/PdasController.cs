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
using System.Threading;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/pdas")]
[ApiController]
public class PdasController(MarechaiContext context) : ControllerBase
{
    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetPdasCountAsync() => context.Machines.CountAsync(c => c.Type == MachineType.Pda);

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMinimumYearAsync() => await context.Machines
                                                                 .Where(t => t.Type == MachineType.Pda &&
                                                                             t.Introduced.HasValue            &&
                                                                             !t.Prototype)
                                                                 .MinAsync(t => (int?)t.Introduced.Value.Year) ?? 0;

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMaximumYearAsync() => await context.Machines
                                                                 .Where(t => t.Type == MachineType.Pda &&
                                                                             t.Introduced.HasValue            &&
                                                                             !t.Prototype)
                                                                 .MaxAsync(t => (int?)t.Introduced.Value.Year) ?? 0;

    [HttpGet("by-letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetPdasByLetterAsync(char c, [FromQuery] int? skip = null,
                                                              [FromQuery] int? take = null,
                                                              CancellationToken cancellationToken = default)
    {
        IQueryable<Machine> ordered = context.Machines.Include(m => m.Company)
                                             .Where(m => m.Type == MachineType.Pda &&
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
    public Task<int> GetPdasByLetterCountAsync(char c, CancellationToken cancellationToken = default) =>
        context.Machines
               .Where(m => m.Type == MachineType.Pda && EF.Functions.Like(m.Name, $"{c}%"))
               .CountAsync(cancellationToken);

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetPdasByYearAsync(int year, [FromQuery] int? skip = null,
                                                            [FromQuery] int? take = null,
                                                            CancellationToken cancellationToken = default)
    {
        IQueryable<Machine> ordered = context.Machines.Include(m => m.Company)
                                             .Where(m => m.Type == MachineType.Pda &&
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
    public Task<int> GetPdasByYearCountAsync(int year, CancellationToken cancellationToken = default) =>
        context.Machines
               .Where(m => m.Type == MachineType.Pda &&
                           m.Introduced != null &&
                           m.Introduced.Value.Year == year)
               .CountAsync(cancellationToken);

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetPdasAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                                      CancellationToken cancellationToken = default)
    {
        IQueryable<Machine> ordered = context.Machines.Include(m => m.Company)
                                             .Where(m => m.Type == MachineType.Pda)
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
                                             .Where(m => m.Type == MachineType.Pda && m.Prototype)
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
    public Task<int> GetPrototypesCountAsync(CancellationToken cancellationToken = default) =>
        context.Machines
               .Where(m => m.Type == MachineType.Pda && m.Prototype)
               .CountAsync(cancellationToken);

    [HttpGet("companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetCompaniesAsync() => context.Machines
                                                                .Where(m => m.Type == MachineType.Pda)
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

    [HttpGet("companies/letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetCompaniesByLetterAsync(char c) => context.Machines
       .Where(m => m.Type == MachineType.Pda)
       .Select(m => m.Company)
       .Distinct()
       .Include(c => c.Logos)
       .Where(co => EF.Functions.Like(co.Name, $"{c}%"))
       .OrderBy(co => MarechaiContext.NaturalSortKey(co.Name))
       .Select(co => new CompanyDto
        {
            Id       = co.Id,
            LastLogo = co.Logos.OrderByDescending(l => l.Year).FirstOrDefault().Guid,
            Name     = co.Name
        })
       .ToListAsync();
}
