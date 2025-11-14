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
using Marechai.Data.Dtos;
using Marechai.Database;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/computers")]
[ApiController]
public class ComputersController(MarechaiContext context) : ControllerBase
{
    [HttpGet("/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetComputersCountAsync() => context.Machines.CountAsync(c => c.Type == MachineType.Computer);

    [HttpGet("/minimum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetMinimumYearAsync() => context.Machines
                                                     .Where(t => t.Type == MachineType.Computer &&
                                                                 t.Introduced.HasValue          &&
                                                                 t.Introduced.Value.Year > 1000)
                                                     .MinAsync(t => t.Introduced.Value.Year);

    [HttpGet("/maximum-year")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetMaximumYearAsync() => context.Machines
                                                     .Where(t => t.Type == MachineType.Computer &&
                                                                 t.Introduced.HasValue          &&
                                                                 t.Introduced.Value.Year > 1000)
                                                     .MaxAsync(t => t.Introduced.Value.Year);

    [HttpGet("/by-letter/{c}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetComputersByLetterAsync(char c) => context.Machines.Include(m => m.Company)
                                                                              .Where(m =>
                                                                                   m.Type == MachineType.Computer &&
                                                                                   EF.Functions.Like(m.Name, $"{c}%"))
                                                                              .OrderBy(m => m.Company.Name)
                                                                              .ThenBy(m => m.Name)
                                                                              .Select(m => new MachineDto
                                                                               {
                                                                                   Id      = m.Id,
                                                                                   Name    = m.Name,
                                                                                   Company = m.Company.Name
                                                                               })
                                                                              .ToListAsync();

    [HttpGet("/by-year/{year:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetComputersByYearAsync(int year) => context.Machines.Include(m => m.Company)
                                                                              .Where(m =>
                                                                                   m.Type == MachineType.Computer &&
                                                                                   m.Introduced != null &&
                                                                                   m.Introduced.Value.Year == year)
                                                                              .OrderBy(m => m.Company.Name)
                                                                              .ThenBy(m => m.Name)
                                                                              .Select(m => new MachineDto
                                                                               {
                                                                                   Id      = m.Id,
                                                                                   Name    = m.Name,
                                                                                   Company = m.Company.Name
                                                                               })
                                                                              .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetComputersAsync() => context.Machines.Include(m => m.Company)
                                                                .Where(m => m.Type == MachineType.Computer)
                                                                .OrderBy(m => m.Company.Name)
                                                                .ThenBy(m => m.Name)
                                                                .Select(m => new MachineDto
                                                                 {
                                                                     Id      = m.Id,
                                                                     Name    = m.Name,
                                                                     Company = m.Company.Name
                                                                 })
                                                                .ToListAsync();
}