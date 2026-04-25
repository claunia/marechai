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
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/gpus")]
[ApiController]
public class GpusController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<GpuDto>> GetAsync() => context.Gpus.OrderBy(g => g.Company.Name)
                                                   .ThenBy(g => g.Name)
                                                   .ThenBy(g => g.Introduced)
                                                   .Select(g => new GpuDto
                                                    {
                                                        Id         = g.Id,
                                                        Company    = g.Company.Name,
                                                        Introduced = g.Introduced,
                                                        IntroducedPrecision = g.IntroducedPrecision,
                                                        ModelCode  = g.ModelCode,
                                                        Name       = g.Name
                                                    })
                                                   .ToListAsync();

    [HttpGet("/machines/{machineId:int}/gpus")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<GpuDto>> GetByMachineAsync(int machineId) => context.GpusByMachine
                                                                         .Where(g => g.MachineId == machineId)
                                                                         .Select(g => g.Gpu)
                                                                         .OrderBy(g => g.Company.Name)
                                                                         .ThenBy(g => g.Name)
                                                                         .Select(g => new GpuDto
                                                                          {
                                                                              Id          = g.Id,
                                                                              Name        = g.Name,
                                                                              Company     = g.Company.Name,
                                                                              CompanyId   = g.Company.Id,
                                                                              ModelCode   = g.ModelCode,
                                                                              Introduced  = g.Introduced,
                                                                              IntroducedPrecision = g.IntroducedPrecision,
                                                                              Package     = g.Package,
                                                                              Process     = g.Process,
                                                                              ProcessNm   = g.ProcessNm,
                                                                              DieSize     = g.DieSize,
                                                                              Transistors = g.Transistors
                                                                          })
                                                                         .ToListAsync();

    [HttpGet("{gpuId:int}/machines")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetMachinesByGpuAsync(int gpuId) => context.GpusByMachine.Where(g => g.GpuId == gpuId)
                                                                             .Select(g => g.Machine)
                                                                             .OrderBy(m => m.Company.Name)
                                                                             .ThenBy(m => m.Name)
                                                                             .Select(m => new MachineDto
                                                                              {
                                                                                  Id         = m.Id,
                                                                                  Company    = m.Company.Name,
                                                                                  CompanyId  = m.Company.Id,
                                                                                  Name       = m.Name,
                                                                                  Model      = m.Model,
                                                                                  Introduced = m.Introduced,
                                                                                  IntroducedPrecision = m.IntroducedPrecision,
                                                                                  Type       = m.Type,
                                                                                  FamilyId   = m.FamilyId
                                                                              })
                                                                             .ToListAsync();

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<GpuDto> GetAsync(int id) => context.Gpus.Where(g => g.Id == id)
                                                   .Select(g => new GpuDto
                                                    {
                                                        Id          = g.Id,
                                                        Name        = g.Name,
                                                        CompanyId   = g.Company.Id,
                                                        ModelCode   = g.ModelCode,
                                                        Introduced  = g.Introduced,
                                                        IntroducedPrecision = g.IntroducedPrecision,
                                                        Package     = g.Package,
                                                        Process     = g.Process,
                                                        ProcessNm   = g.ProcessNm,
                                                        DieSize     = g.DieSize,
                                                        Transistors = g.Transistors
                                                    })
                                                   .FirstOrDefaultAsync();

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] GpuDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Gpu model = await context.Gpus.FindAsync(id);

        if(model is null) return NotFound();

        model.Name        = dto.Name;
        model.CompanyId   = dto.CompanyId;
        model.ModelCode   = dto.ModelCode;
        model.Introduced  = dto.Introduced;
        model.IntroducedPrecision = dto.IntroducedPrecision;
        model.Package     = dto.Package;
        model.Process     = dto.Process;
        model.ProcessNm   = dto.ProcessNm;
        model.DieSize     = dto.DieSize;
        model.Transistors = dto.Transistors;

        Company company  = dto.CompanyId is not null ? await context.Companies.FindAsync(dto.CompanyId) : null;
        string  newsName = company is not null ? $"{company.Name} {dto.Name}" : dto.Name;

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedGpuInDb,
            Name    = newsName
        });

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] GpuDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Gpu
        {
            Name        = dto.Name,
            CompanyId   = dto.CompanyId,
            ModelCode   = dto.ModelCode,
            Introduced  = dto.Introduced,
            IntroducedPrecision = dto.IntroducedPrecision,
            Package     = dto.Package,
            Process     = dto.Process,
            ProcessNm   = dto.ProcessNm,
            DieSize     = dto.DieSize,
            Transistors = dto.Transistors
        };

        await context.Gpus.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        Company company  = dto.CompanyId is not null ? await context.Companies.FindAsync(dto.CompanyId) : null;
        string  newsName = company is not null ? $"{company.Name} {dto.Name}" : dto.Name;

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.NewGpuInDb,
            Name    = newsName
        });

        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Gpu item = await context.Gpus.FindAsync(id);

        if(item is null) return NotFound();

        context.Gpus.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}