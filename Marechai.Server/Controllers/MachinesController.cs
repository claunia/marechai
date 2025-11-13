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
// Copyright © 2003-2025 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Marechai.Server.Controllers;

[Route("/machines")]
[ApiController]
public class MachinesController
(
    MarechaiContext                   context,
    IStringLocalizer<MachinesService> localizer,
    GpusService                       gpusService,
    ProcessorsService                 processorsService,
    SoundSynthsService                soundSynthsService
) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetAsync() => context.Machines.OrderBy(m => m.Company.Name)
                                                       .ThenBy(m => m.Name)
                                                       .ThenBy(m => m.Family.Name)
                                                       .Select(m => new MachineDto
                                                        {
                                                            Id         = m.Id,
                                                            Company    = m.Company.Name,
                                                            Name       = m.Name,
                                                            Model      = m.Model,
                                                            Introduced = m.Introduced,
                                                            Type       = m.Type,
                                                            Family     = m.Family.Name
                                                        })
                                                       .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<MachineDto> GetAsync(int id) => context.Machines.Where(m => m.Id == id)
                                                       .Select(m => new MachineDto
                                                        {
                                                            Id         = m.Id,
                                                            Company    = m.Company.Name,
                                                            CompanyId  = m.CompanyId,
                                                            Name       = m.Name,
                                                            Model      = m.Model,
                                                            Introduced = m.Introduced,
                                                            Type       = m.Type,
                                                            FamilyId   = m.FamilyId
                                                        })
                                                       .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateAsync(MachineDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Machine model = await context.Machines.FindAsync(dto.Id);

        if(model is null) return NotFound();

        model.CompanyId  = dto.CompanyId;
        model.Name       = dto.Name;
        model.Model      = dto.Model;
        model.Introduced = dto.Introduced;
        model.Type       = dto.Type;
        model.FamilyId   = dto.FamilyId;

        var news = new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow
        };

        switch(model.Type)
        {
            case MachineType.Computer:
                news.Type = NewsType.UpdatedComputerInDb;

                break;
            case MachineType.Console:
                news.Type = NewsType.UpdatedConsoleInDb;

                break;
            default:
                news = null;

                break;
        }

        if(news != null) await context.News.AddAsync(news);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> CreateAsync(MachineDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Machine
        {
            CompanyId  = dto.CompanyId,
            Name       = dto.Name,
            Model      = dto.Model,
            Introduced = dto.Introduced,
            Type       = dto.Type,
            FamilyId   = dto.FamilyId
        };

        await context.Machines.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        var news = new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow
        };

        switch(model.Type)
        {
            case MachineType.Computer:
                news.Type = NewsType.NewComputerInDb;

                break;
            case MachineType.Console:
                news.Type = NewsType.NewConsoleInDb;

                break;
            default:
                news = null;

                break;
        }

        if(news != null)
        {
            await context.News.AddAsync(news);
            await context.SaveChangesWithUserAsync(userId);
        }

        return model.Id;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<MachineDto> GetMachine(int id)
    {
        Machine machine = await context.Machines.FindAsync(id);

        if(machine is null) return null;

        var model = new MachineDto
        {
            Introduced = machine.Introduced,
            Name       = machine.Name,
            CompanyId  = machine.CompanyId,
            Model      = machine.Model,
            Type       = machine.Type
        };

        Company company = await context.Companies.FindAsync(model.CompanyId);

        if(company != null)
        {
            model.Company = company.Name;

            IQueryable<CompanyLogo> logos = context.CompanyLogos.Where(l => l.CompanyId == company.Id);

            if(model.Introduced.HasValue)
                model.CompanyLogo = (await logos.FirstOrDefaultAsync(l => l.Year >= model.Introduced.Value.Year))?.Guid;

            if(model.CompanyLogo is null && logos.Any()) model.CompanyLogo = (await logos.FirstAsync())?.Guid;
        }

        MachineFamily family = await context.MachineFamilies.FindAsync(machine.FamilyId);

        if(family != null)
        {
            model.FamilyName = family.Name;
            model.FamilyId   = family.Id;
        }

        model.Gpus = await gpusService.GetByMachineAsync(machine.Id);

        model.Memory = await context.MemoryByMachine.Where(m => m.MachineId == machine.Id)
                                    .Select(m => new MemoryDto
                                     {
                                         Type  = m.Type,
                                         Usage = m.Usage,
                                         Size  = m.Size,
                                         Speed = m.Speed
                                     })
                                    .ToListAsync();

        model.Processors = await processorsService.GetByMachineAsync(machine.Id);

        model.SoundSynthesizers = await soundSynthsService.GetByMachineAsync(machine.Id);

        model.Storage = await context.StorageByMachine.Where(s => s.MachineId == machine.Id)
                                     .Select(s => new StorageDto
                                      {
                                          Type      = s.Type,
                                          Interface = s.Interface,
                                          Capacity  = s.Capacity
                                      })
                                     .ToListAsync();

        return model;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Machine item = await context.Machines.FindAsync(id);

        if(item is null) return NotFound();

        context.Machines.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}