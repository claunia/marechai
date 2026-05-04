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
using Markdig;
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
                                                              IntroducedPrecision = p.IntroducedPrecision,
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

    [HttpGet("{processorId:int}/machines")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetMachinesByProcessorAsync(int processorId) =>
        context.ProcessorsByMachine.Where(p => p.ProcessorId == processorId)
               .Select(p => p.Machine)
               .OrderBy(m => m.Company.Name)
               .ThenBy(m => m.Name)
               .Select(m => new MachineDto
                {
                    Id                  = m.Id,
                    Company             = m.Company.Name,
                    CompanyId           = m.Company.Id,
                    Name                = m.Name,
                    Model               = m.Model,
                    Introduced          = m.Introduced,
                    IntroducedPrecision = m.IntroducedPrecision,
                    Type                = m.Type,
                    FamilyId            = m.FamilyId
                })
               .ToListAsync();

    [HttpGet("/machines/{machineId:int}/processors")]
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

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ProcessorDto> GetAsync(int id) => context.Processors.Where(p => p.Id == id)
                                                         .Select(p => new ProcessorDto
                                                          {
                                                              Id               = p.Id,
                                                              Name             = p.Name,
                                                              CompanyName      = p.Company.Name,
                                                              CompanyId        = p.Company.Id,
                                                              ModelCode        = p.ModelCode,
                                                              Introduced       = p.Introduced,
                                                              IntroducedPrecision = p.IntroducedPrecision,
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

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] ProcessorDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Processor model = await context.Processors.FindAsync(id);

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
        model.IntroducedPrecision = dto.IntroducedPrecision;
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

        Company company  = dto.CompanyId is not null ? await context.Companies.FindAsync(dto.CompanyId) : null;
        string  newsName = company is not null ? $"{company.Name} {dto.Name}" : dto.Name;

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedProcessorInDb,
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
    public async Task<ActionResult<long>> CreateAsync([FromBody] ProcessorDto dto)
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
            IntroducedPrecision = dto.IntroducedPrecision,
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

        Company company  = dto.CompanyId is not null ? await context.Companies.FindAsync(dto.CompanyId) : null;
        string  newsName = company is not null ? $"{company.Name} {dto.Name}" : dto.Name;

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.NewProcessorInDb,
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
        Processor item = await context.Processors.FindAsync(id);

        if(item is null) return NotFound();

        context.Processors.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpGet("{id:int}/description/text")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetDescriptionTextAsync(int id, [FromQuery] string lang = "eng")
    {
        ProcessorDescription description =
            await context.ProcessorDescriptions.FirstOrDefaultAsync(d => d.ProcessorId == id && d.LanguageCode == lang);

        // Fallback to English if requested language not found
        if(description is null && lang != "eng")
            description = await context.ProcessorDescriptions.FirstOrDefaultAsync(d => d.ProcessorId == id &&
                              d.LanguageCode == "eng");

        return description?.Html ?? description?.Text;
    }

    [HttpGet("{id:int}/descriptions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<ProcessorDescriptionDto>> GetDescriptionsAsync(int id) => context.ProcessorDescriptions
       .Where(d => d.ProcessorId == id)
       .Select(d => new ProcessorDescriptionDto
        {
            Id           = d.Id,
            ProcessorId  = d.ProcessorId,
            Html         = d.Html,
            Markdown     = d.Text,
            LanguageCode = d.LanguageCode,
            Language     = d.Language.ReferenceName
        })
       .ToListAsync();

    [HttpGet("{id:int}/description")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ProcessorDescriptionDto> GetDescriptionAsync(int id, [FromQuery] string lang = "eng")
    {
        ProcessorDescriptionDto description = await context.ProcessorDescriptions
                                                         .Where(d => d.ProcessorId == id && d.LanguageCode == lang)
                                                         .Select(d => new ProcessorDescriptionDto
                                                          {
                                                              Id           = d.Id,
                                                              ProcessorId  = d.ProcessorId,
                                                              Html         = d.Html,
                                                              Markdown     = d.Text,
                                                              LanguageCode = d.LanguageCode,
                                                              Language     = d.Language.ReferenceName
                                                          })
                                                         .FirstOrDefaultAsync();

        // Fallback to English if requested language not found
        if(description is null && lang != "eng")
            description = await context.ProcessorDescriptions
                                       .Where(d => d.ProcessorId == id && d.LanguageCode == "eng")
                                       .Select(d => new ProcessorDescriptionDto
                                        {
                                            Id           = d.Id,
                                            ProcessorId  = d.ProcessorId,
                                            Html         = d.Html,
                                            Markdown     = d.Text,
                                            LanguageCode = d.LanguageCode,
                                            Language     = d.Language.ReferenceName
                                        })
                                       .FirstOrDefaultAsync();

        return description;
    }

    [HttpPost("{id:int}/description")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> CreateOrUpdateDescriptionAsync(
        int id, [FromBody] ProcessorDescriptionDto description)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        ProcessorDescription current = await context.ProcessorDescriptions
                                                  .FirstOrDefaultAsync(d => d.ProcessorId    == id &&
                                                                            d.LanguageCode == description.LanguageCode);

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        string             html     = Markdown.ToHtml(description.Markdown, pipeline);

        if(current is null)
        {
            current = new ProcessorDescription
            {
                ProcessorId  = id,
                LanguageCode = description.LanguageCode,
                Html         = html,
                Text         = description.Markdown
            };

            await context.ProcessorDescriptions.AddAsync(current);
        }
        else
        {
            current.Html = html;
            current.Text = description.Markdown;
        }

        await context.SaveChangesWithUserAsync(userId);

        return current.Id;
    }

    [HttpDelete("{id:int}/description/{languageCode}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteDescriptionAsync(int id, string languageCode)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        ProcessorDescription description = await context.ProcessorDescriptions
                                                      .FirstOrDefaultAsync(d => d.ProcessorId    == id &&
                                                                                d.LanguageCode == languageCode);

        if(description is null) return NotFound();

        context.ProcessorDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}