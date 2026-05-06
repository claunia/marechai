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

[Route("/machines")]
[ApiController]
public class MachinesController(MarechaiContext context) : ControllerBase
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
                                                            IntroducedPrecision = m.IntroducedPrecision,
                                                            Prototype  = m.Prototype,
                                                            Type       = m.Type,
                                                            Family     = m.Family.Name
                                                        })
                                                       .ToListAsync();

    [HttpGet("{id:int}")]
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
                                                            IntroducedPrecision = m.IntroducedPrecision,
                                                            Prototype  = m.Prototype,
                                                            Type       = m.Type,
                                                            FamilyId   = m.FamilyId
                                                        })
                                                       .FirstOrDefaultAsync();

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] MachineDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        Machine model = await context.Machines.FindAsync(id);

        if(model is null) return NotFound();

        model.CompanyId  = dto.CompanyId;
        model.Name       = dto.Name;
        model.Model      = dto.Model;
        model.Prototype  = dto.Prototype;
        model.Introduced = dto.Prototype ? null : dto.Introduced;
        model.IntroducedPrecision = dto.IntroducedPrecision;
        model.Type       = dto.Type;
        model.FamilyId   = dto.FamilyId;

        var news = new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow
        };

        Company company = await context.Companies.FindAsync(model.CompanyId);
        news.Name = company is not null ? $"{company.Name} {model.Name}" : model.Name;

        switch(model.Type)
        {
            case MachineType.Computer:
                news.Type = NewsType.UpdatedComputerInDb;

                break;
            case MachineType.Console:
                news.Type = NewsType.UpdatedConsoleInDb;

                break;
            case MachineType.Smartphone:
                news.Type = NewsType.UpdatedSmartphoneInDb;

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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] MachineDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new Machine
        {
            CompanyId  = dto.CompanyId,
            Name       = dto.Name,
            Model      = dto.Model,
            Prototype  = dto.Prototype,
            Introduced = dto.Prototype ? null : dto.Introduced,
            IntroducedPrecision = dto.IntroducedPrecision,
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

        Company company = await context.Companies.FindAsync(dto.CompanyId);
        news.Name = company is not null ? $"{company.Name} {model.Name}" : model.Name;

        switch(model.Type)
        {
            case MachineType.Computer:
                news.Type = NewsType.NewComputerInDb;

                break;
            case MachineType.Console:
                news.Type = NewsType.NewConsoleInDb;

                break;
            case MachineType.Smartphone:
                news.Type = NewsType.NewSmartphoneInDb;

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

    [HttpGet("{id:int}/full")]
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
            IntroducedPrecision = machine.IntroducedPrecision,
            Name       = machine.Name,
            CompanyId  = machine.CompanyId,
            Model      = machine.Model,
            Prototype  = machine.Prototype,
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

        model.Gpus = await context.GpusByMachine.Where(g => g.MachineId == machine.Id)
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

        model.Memory = await context.MemoryByMachine.Where(m => m.MachineId == machine.Id)
                                    .Select(m => new MemoryDto
                                     {
                                         Type  = m.Type,
                                         Usage = m.Usage,
                                         Size  = m.Size,
                                         Speed = m.Speed
                                     })
                                    .ToListAsync();

        model.Processors = await context.ProcessorsByMachine.Where(p => p.MachineId == machine.Id)
                                        .Select(p => new ProcessorDto
                                         {
                                             Name           = p.Processor.Name,
                                             CompanyName    = p.Processor.Company.Name,
                                             CompanyId      = p.Processor.Company.Id,
                                             ModelCode      = p.Processor.ModelCode,
                                             Introduced     = p.Processor.Introduced,
                                             Speed          = p.Speed,
                                             Package        = p.Processor.Package,
                                             Gprs           = p.Processor.Gprs,
                                             GprSize        = p.Processor.GprSize,
                                             Fprs           = p.Processor.Fprs,
                                             FprSize        = p.Processor.FprSize,
                                             Cores          = p.Processor.Cores,
                                             ThreadsPerCore = p.Processor.ThreadsPerCore,
                                             Process        = p.Processor.Process,
                                             ProcessNm      = p.Processor.ProcessNm,
                                             DieSize        = p.Processor.DieSize,
                                             Transistors    = p.Processor.Transistors,
                                             DataBus        = p.Processor.DataBus,
                                             AddrBus        = p.Processor.AddrBus,
                                             SimdRegisters  = p.Processor.SimdRegisters,
                                             SimdSize       = p.Processor.SimdSize,
                                             L1Instruction  = p.Processor.L1Instruction,
                                             L1Data         = p.Processor.L1Data,
                                             L2             = p.Processor.L2,
                                             L3             = p.Processor.L3,
                                             InstructionSet = p.Processor.InstructionSet.Name,
                                             Id             = p.Processor.Id,
                                             InstructionSetExtensions = p.Processor.InstructionSetExtensions
                                                                         .Select(e => e.Extension.Extension)
                                                                         .ToList()
                                         })
                                        .ToListAsync();

        model.SoundSynthesizers = await context.SoundByMachine.Where(s => s.MachineId == machine.Id)
                                               .Select(s => s.SoundSynth)
                                               .OrderBy(s => s.Company.Name)
                                               .ThenBy(s => s.Name)
                                               .ThenBy(s => s.ModelCode)
                                               .Select(s => new SoundSynthDto
                                                {
                                                    Id          = s.Id,
                                                    Name        = s.Name,
                                                    CompanyId   = s.Company.Id,
                                                    CompanyName = s.Company.Name,
                                                    ModelCode   = s.ModelCode,
                                                    Introduced  = s.Introduced,
                                                    IntroducedPrecision = s.IntroducedPrecision,
                                                    Voices      = s.Voices,
                                                    Frequency   = s.Frequency,
                                                    Depth       = s.Depth,
                                                    SquareWave  = s.SquareWave,
                                                    WhiteNoise  = s.WhiteNoise,
                                                    Type        = s.Type
                                                })
                                               .ToListAsync();

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

    [HttpGet("{id:int}/software")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoftwareDto>> GetSoftwareByMachineAsync(int id)
    {
        IQueryable<ulong> platformIds = context.SoftwarePlatformsByMachine
                                               .Where(sp => sp.MachineId == id)
                                               .Select(sp => sp.SoftwarePlatformId);

        return context.Softwares
                      .Where(s => s.Versions.Any(v => v.Releases.Any(r => r.PlatformId != null &&
                                                                          platformIds.Contains(r.PlatformId.Value)))
                                || s.DirectReleases.Any(r => r.PlatformId != null &&
                                                             platformIds.Contains(r.PlatformId.Value)))
                      .OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
                      .Select(s => new SoftwareDto
                       {
                           Id                = s.Id,
                           Name              = s.Name,
                           FamilyId          = s.FamilyId,
                           Family            = s.Family.Name,
                           Kind              = s.Kind,
                           FrontCoverId = context.SoftwareCovers
                                                 .Where(c => (c.Release.SoftwareId == s.Id ||
                                                               c.Release.SoftwareVersion.SoftwareId == s.Id) &&
                                                              c.Type == SoftwareCoverType.Front)
                                                 .Select(c => (Guid?)c.Id)
                                                 .FirstOrDefault()
                       })
                      .ToListAsync();
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
        Machine item = await context.Machines.FindAsync(id);

        if(item is null) return NotFound();

        context.Machines.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpGet("{id:int}/description/text")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetDescriptionTextAsync(int id, [FromQuery] string lang = "eng")
    {
        MachineDescription description =
            await context.MachineDescriptions.FirstOrDefaultAsync(d => d.MachineId == id && d.LanguageCode == lang);

        // Fallback to English if requested language not found
        if(description is null && lang != "eng")
            description = await context.MachineDescriptions.FirstOrDefaultAsync(d => d.MachineId == id &&
                              d.LanguageCode == "eng");

        return description?.Html ?? description?.Text;
    }

    [HttpGet("{id:int}/descriptions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDescriptionDto>> GetDescriptionsAsync(int id) => context.MachineDescriptions
       .Where(d => d.MachineId == id)
       .Select(d => new MachineDescriptionDto
        {
            Id           = d.Id,
            MachineId    = d.MachineId,
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
    public async Task<MachineDescriptionDto> GetDescriptionAsync(int id, [FromQuery] string lang = "eng")
    {
        MachineDescriptionDto description = await context.MachineDescriptions
                                                         .Where(d => d.MachineId == id && d.LanguageCode == lang)
                                                         .Select(d => new MachineDescriptionDto
                                                          {
                                                              Id           = d.Id,
                                                              MachineId    = d.MachineId,
                                                              Html         = d.Html,
                                                              Markdown     = d.Text,
                                                              LanguageCode = d.LanguageCode,
                                                              Language     = d.Language.ReferenceName
                                                          })
                                                         .FirstOrDefaultAsync();

        // Fallback to English if requested language not found
        if(description is null && lang != "eng")
            description = await context.MachineDescriptions
                                       .Where(d => d.MachineId == id && d.LanguageCode == "eng")
                                       .Select(d => new MachineDescriptionDto
                                        {
                                            Id           = d.Id,
                                            MachineId    = d.MachineId,
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
        int id, [FromBody] MachineDescriptionDto description)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        MachineDescription current = await context.MachineDescriptions
                                                  .FirstOrDefaultAsync(d => d.MachineId    == id &&
                                                                            d.LanguageCode == description.LanguageCode);

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        string             html     = Markdown.ToHtml(description.Markdown, pipeline);

        if(current is null)
        {
            current = new MachineDescription
            {
                MachineId    = id,
                LanguageCode = description.LanguageCode,
                Html         = html,
                Text         = description.Markdown
            };

            await context.MachineDescriptions.AddAsync(current);
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

        MachineDescription description = await context.MachineDescriptions
                                                      .FirstOrDefaultAsync(d => d.MachineId    == id &&
                                                                                d.LanguageCode == languageCode);

        if(description is null) return NotFound();

        context.MachineDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }
}