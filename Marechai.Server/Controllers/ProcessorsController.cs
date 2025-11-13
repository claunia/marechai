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

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("/processors")]
[ApiController]
public class ProcessorsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<ProcessorDto>> GetAsync() => context.Processors.Select(p => new ProcessorDto
                                                          {
                                                              Name           = p.Name,
                                                              CompanyName    = p.Company.Name,
                                                              CompanyId      = p.Company.Id,
                                                              ModelCode      = p.ModelCode,
                                                              Introduced     = p.Introduced,
                                                              Speed          = p.Speed,
                                                              Package        = p.Package,
                                                              Gprs           = p.Gprs,
                                                              GprSize        = p.GprSize,
                                                              Fprs           = p.Fprs,
                                                              FprSize        = p.FprSize,
                                                              Cores          = p.Cores,
                                                              ThreadsPerCore = p.ThreadsPerCore,
                                                              Process        = p.Process,
                                                              ProcessNm      = p.ProcessNm,
                                                              DieSize        = p.DieSize,
                                                              Transistors    = p.Transistors,
                                                              DataBus        = p.DataBus,
                                                              AddrBus        = p.AddrBus,
                                                              SimdRegisters  = p.SimdRegisters,
                                                              SimdSize       = p.SimdSize,
                                                              L1Instruction  = p.L1Instruction,
                                                              L1Data         = p.L1Data,
                                                              L2             = p.L2,
                                                              L3             = p.L3,
                                                              InstructionSet = p.InstructionSet.Name,
                                                              Id             = p.Id,
                                                              InstructionSetExtensions = p.InstructionSetExtensions
                                                                 .Select(e => e.Extension.Extension)
                                                                 .ToList()
                                                          })
                                                         .OrderBy(p => p.CompanyName)
                                                         .ThenBy(p => p.Name)
                                                         .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<ProcessorDto>> GetByMachineAsync(int machineId) => context.ProcessorsByMachine
       .Where(p => p.MachineId == machineId)
       .Select(p => new ProcessorDto
        {
            Name                     = p.Processor.Name,
            CompanyName              = p.Processor.Company.Name,
            CompanyId                = p.Processor.Company.Id,
            ModelCode                = p.Processor.ModelCode,
            Introduced               = p.Processor.Introduced,
            Speed                    = p.Speed,
            Package                  = p.Processor.Package,
            Gprs                     = p.Processor.Gprs,
            GprSize                  = p.Processor.GprSize,
            Fprs                     = p.Processor.Fprs,
            FprSize                  = p.Processor.FprSize,
            Cores                    = p.Processor.Cores,
            ThreadsPerCore           = p.Processor.ThreadsPerCore,
            Process                  = p.Processor.Process,
            ProcessNm                = p.Processor.ProcessNm,
            DieSize                  = p.Processor.DieSize,
            Transistors              = p.Processor.Transistors,
            DataBus                  = p.Processor.DataBus,
            AddrBus                  = p.Processor.AddrBus,
            SimdRegisters            = p.Processor.SimdRegisters,
            SimdSize                 = p.Processor.SimdSize,
            L1Instruction            = p.Processor.L1Instruction,
            L1Data                   = p.Processor.L1Data,
            L2                       = p.Processor.L2,
            L3                       = p.Processor.L3,
            InstructionSet           = p.Processor.InstructionSet.Name,
            Id                       = p.Processor.Id,
            InstructionSetExtensions = p.Processor.InstructionSetExtensions.Select(e => e.Extension.Extension).ToList()
        })
       .OrderBy(p => p.CompanyName)
       .ThenBy(p => p.Name)
       .ToListAsync();

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ProcessorDto> GetAsync(int id) => context.Processors.Where(p => p.Id == id)
                                                         .Select(p => new ProcessorDto
                                                          {
                                                              Name             = p.Name,
                                                              CompanyName      = p.Company.Name,
                                                              CompanyId        = p.Company.Id,
                                                              ModelCode        = p.ModelCode,
                                                              Introduced       = p.Introduced,
                                                              Speed            = p.Speed,
                                                              Package          = p.Package,
                                                              Gprs             = p.Gprs,
                                                              GprSize          = p.GprSize,
                                                              Fprs             = p.Fprs,
                                                              FprSize          = p.FprSize,
                                                              Cores            = p.Cores,
                                                              ThreadsPerCore   = p.ThreadsPerCore,
                                                              Process          = p.Process,
                                                              ProcessNm        = p.ProcessNm,
                                                              DieSize          = p.DieSize,
                                                              Transistors      = p.Transistors,
                                                              DataBus          = p.DataBus,
                                                              AddrBus          = p.AddrBus,
                                                              SimdRegisters    = p.SimdRegisters,
                                                              SimdSize         = p.SimdSize,
                                                              L1Instruction    = p.L1Instruction,
                                                              L1Data           = p.L1Data,
                                                              L2               = p.L2,
                                                              L3               = p.L3,
                                                              InstructionSet   = p.InstructionSet.Name,
                                                              InstructionSetId = p.InstructionSetId
                                                          })
                                                         .FirstOrDefaultAsync();

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(ProcessorDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Processor model = await context.Processors.FindAsync(dto.Id);

        if(model is null) return NotFound();

        model.AddrBus          = dto.AddrBus;
        model.CompanyId        = dto.CompanyId;
        model.Cores            = dto.Cores;
        model.DataBus          = dto.DataBus;
        model.DieSize          = dto.DieSize;
        model.Fprs             = dto.Fprs;
        model.FprSize          = dto.FprSize;
        model.Gprs             = dto.Gprs;
        model.GprSize          = dto.GprSize;
        model.InstructionSetId = dto.InstructionSetId;
        model.Introduced       = dto.Introduced;
        model.L1Data           = dto.L1Data;
        model.L1Instruction    = dto.L1Instruction;
        model.L2               = dto.L2;
        model.L3               = dto.L3;
        model.ModelCode        = dto.ModelCode;
        model.Name             = dto.Name;
        model.Package          = dto.Package;
        model.Process          = dto.Process;
        model.ProcessNm        = dto.ProcessNm;
        model.SimdRegisters    = dto.SimdRegisters;
        model.SimdSize         = dto.SimdSize;
        model.Speed            = dto.Speed;
        model.ThreadsPerCore   = dto.ThreadsPerCore;
        model.Transistors      = dto.Transistors;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> CreateAsync(ProcessorDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Processor
        {
            AddrBus          = dto.AddrBus,
            CompanyId        = dto.CompanyId,
            Cores            = dto.Cores,
            DataBus          = dto.DataBus,
            DieSize          = dto.DieSize,
            Fprs             = dto.Fprs,
            FprSize          = dto.FprSize,
            Gprs             = dto.Gprs,
            GprSize          = dto.GprSize,
            InstructionSetId = dto.InstructionSetId,
            Introduced       = dto.Introduced,
            L1Data           = dto.L1Data,
            L1Instruction    = dto.L1Instruction,
            L2               = dto.L2,
            L3               = dto.L3,
            ModelCode        = dto.ModelCode,
            Name             = dto.Name,
            Package          = dto.Package,
            Process          = dto.Process,
            ProcessNm        = dto.ProcessNm,
            SimdRegisters    = dto.SimdRegisters,
            SimdSize         = dto.SimdSize,
            Speed            = dto.Speed,
            ThreadsPerCore   = dto.ThreadsPerCore,
            Transistors      = dto.Transistors
        };

        await context.Processors.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return model.Id;
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Processor item = await context.Processors.FindAsync(id);

        if(item is null) return NotFound();

        context.Processors.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}