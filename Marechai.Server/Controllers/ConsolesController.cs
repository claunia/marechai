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

[Route("/consoles")]
[ApiController]
public class ConsolesController(MarechaiContext context) : ControllerBase
{
    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetConsolesCountAsync() => context.Machines.CountAsync(c => c.Type == MachineType.Console);

    [HttpGet("minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMinimumYearAsync() => await context.Machines
                                                                 .Where(t => t.Type == MachineType.Console &&
                                                                             t.Introduced.HasValue         &&
                                                                             !t.Prototype)
                                                                 .MinAsync(t => (int?)t.Introduced.Value.Year) ?? 0;

    [HttpGet("maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetMaximumYearAsync() => await context.Machines
                                                                 .Where(t => t.Type == MachineType.Console &&
                                                                             t.Introduced.HasValue         &&
                                                                             !t.Prototype)
                                                                 .MaxAsync(t => (int?)t.Introduced.Value.Year) ?? 0;

    [HttpGet("by-letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetConsolesByLetterAsync(char c) => context.Machines.Include(m => m.Company)
                                                                             .Where(m =>
                                                                                  m.Type == MachineType.Console &&
                                                                                  EF.Functions.Like(m.Name, $"{c}%"))
                                                                             .OrderBy(m => MarechaiContext.NaturalSortKey(m.Company.Name))
                                                                             .ThenBy(m => MarechaiContext.NaturalSortKey(m.Name))
                                                                             .Select(m => new MachineDto
                                                                              {
                                                                                  Id         = m.Id,
                                                                                  Name       = m.Name,
                                                                                  Company    = m.Company.Name,
                                                                                  Introduced = m.Introduced
                                                                              })
                                                                             .ToListAsync();

    [HttpGet("by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetConsolesByYearAsync(int year) => context.Machines.Include(m => m.Company)
                                                                             .Where(m =>
                                                                                  m.Type == MachineType.Console &&
                                                                                  m.Introduced != null &&
                                                                                  m.Introduced.Value.Year == year)
                                                                             .OrderBy(m => MarechaiContext.NaturalSortKey(m.Company.Name))
                                                                             .ThenBy(m => MarechaiContext.NaturalSortKey(m.Name))
                                                                             .Select(m => new MachineDto
                                                                              {
                                                                                  Id         = m.Id,
                                                                                  Name       = m.Name,
                                                                                  Company    = m.Company.Name,
                                                                                  Introduced = m.Introduced
                                                                              })
                                                                             .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetConsolesAsync() => context.Machines.Include(m => m.Company)
                                                               .Where(m => m.Type == MachineType.Console)
                                                               .OrderBy(m => MarechaiContext.NaturalSortKey(m.Company.Name))
                                                               .ThenBy(m => MarechaiContext.NaturalSortKey(m.Name))
                                                               .Select(m => new MachineDto
                                                                {
                                                                    Id         = m.Id,
                                                                    Name       = m.Name,
                                                                    Company    = m.Company.Name,
                                                                    Introduced = m.Introduced
                                                                })
                                                               .ToListAsync();

    [HttpGet("prototypes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetPrototypesAsync() => context.Machines.Include(m => m.Company)
                                                                .Where(m => m.Type == MachineType.Console &&
                                                                            m.Prototype)
                                                                .OrderBy(m => MarechaiContext.NaturalSortKey(m.Company.Name))
                                                                .ThenBy(m => MarechaiContext.NaturalSortKey(m.Name))
                                                                .Select(m => new MachineDto
                                                                 {
                                                                     Id        = m.Id,
                                                                     Name      = m.Name,
                                                                     Company   = m.Company.Name,
                                                                     Prototype = m.Prototype
                                                                 })
                                                                .ToListAsync();

    [HttpGet("companies")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<CompanyDto>> GetCompaniesAsync() => context.Machines
                                                                .Where(m => m.Type == MachineType.Console)
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
       .Where(m => m.Type == MachineType.Console)
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