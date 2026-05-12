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
using System.Threading;
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
public class ProcessorsController(MarechaiContext context, IDbContextFactory<MarechaiContext> dbFactory)
    : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<ProcessorDto>> GetAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                             CancellationToken cancellationToken = default)
    {
        IQueryable<Processor> ordered = context.Processors.AsNoTracking()
                                               .OrderBy(p => p.Company.Name)
                                               .ThenBy(p => p.Name)
                                               .ThenBy(p => p.ModelCode);

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(p => new ProcessorDto
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
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetCountAsync(CancellationToken cancellationToken = default) =>
        context.Processors.CountAsync(cancellationToken);

    [HttpGet("{processorId:int}/machines")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetMachinesByProcessorAsync(int processorId) =>
        context.ProcessorsByMachine.AsNoTracking()
               .Where(p => p.ProcessorId == processorId)
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
    public Task<List<ProcessorDto>> GetByMachineAsync(int machineId) => context.ProcessorsByMachine.AsNoTracking()
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
    public Task<ProcessorDto> GetAsync(int id) => context.Processors.AsNoTracking()
                                                         .Where(p => p.Id == id)
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

    /// <summary>
    /// Consolidated payload for the public /processor/{Id} view page. Replaces five
    /// sequential HTTP round-trips (head + machines + description + photos + videos)
    /// with a single response. The head + company name + company logo + instruction-set
    /// extensions are projected in one query (logo is an inline correlated subquery so
    /// it costs zero extra DB round-trips); the description language fallback is
    /// collapsed into a single ordered query; the four child collections are fetched in
    /// parallel using independent <see cref="MarechaiContext"/> instances from
    /// <c>IDbContextFactory</c> (DbContext is not thread-safe; sharing the request-scoped
    /// context across parallel branches throws <c>InvalidOperationException</c>).
    /// </summary>
    [HttpGet("{id:int}/full")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProcessorFullDto>> GetFullAsync(int id, [FromQuery] string lang = "eng")
    {
        // The four child collections plus the description only depend on `id` (from
        // the URL), and not on any value projected by the head query, so we fire all
        // FIVE queries — head + description + machines + photos + videos — in parallel
        // using independent DbContext instances from the factory (DbContext is not
        // thread-safe). The company-logo lookup is folded into the head query as an
        // inline correlated subquery so it adds zero extra DB round-trips.
        await using var headCtx        = await dbFactory.CreateDbContextAsync();
        await using var descriptionCtx = await dbFactory.CreateDbContextAsync();
        await using var machinesCtx    = await dbFactory.CreateDbContextAsync();
        await using var photosCtx      = await dbFactory.CreateDbContextAsync();
        await using var videosCtx      = await dbFactory.CreateDbContextAsync();

        // Head + company name + instruction set + instruction-set extensions + company
        // logo in a single projected join. The CompanyLogo lookup uses an inline
        // correlated subquery ordered such that logos issued on or after the
        // processor's introduction year sort first (key 0), then any other logo by
        // ascending year. Returns null when the processor does not exist; surfaced as
        // 404 below.
        var headTask = headCtx.Processors.AsNoTracking()
                              .Where(p => p.Id == id)
                              .Select(p => new
                               {
                                   p.Id,
                                   p.Name,
                                   p.CompanyId,
                                   CompanyName         = p.Company.Name,
                                   p.ModelCode,
                                   p.Introduced,
                                   p.IntroducedPrecision,
                                   p.Speed,
                                   p.Package,
                                   p.Gprs,
                                   p.GprSize,
                                   p.Fprs,
                                   p.FprSize,
                                   p.Cores,
                                   p.ThreadsPerCore,
                                   p.Process,
                                   p.ProcessNm,
                                   p.DieSize,
                                   p.Transistors,
                                   p.DataBus,
                                   p.AddrBus,
                                   p.SimdRegisters,
                                   p.SimdSize,
                                   p.L1Instruction,
                                   p.L1Data,
                                   p.L2,
                                   p.L3,
                                   InstructionSetName       = p.InstructionSet.Name,
                                   p.InstructionSetId,
                                   InstructionSetExtensions = p.InstructionSetExtensions
                                                               .Select(e => e.Extension.Extension)
                                                               .ToList(),
                                   CompanyLogo = p.Company.Logos
                                                  .OrderBy(l => p.Introduced.HasValue && l.Year >= p.Introduced.Value.Year
                                                                    ? 0
                                                                    : 1)
                                                  .ThenBy(l => l.Year)
                                                  .Select(l => (Guid?)l.Guid)
                                                  .FirstOrDefault()
                               })
                              .FirstOrDefaultAsync();

        // Description: collapse the original two-step lookup (try requested lang, then
        // English fallback) into a single ordered query. Matches for the requested
        // language sort first (key 0); English fallback is key 1; FirstOrDefaultAsync
        // returns the preferred row.
        var descriptionTask = descriptionCtx.ProcessorDescriptions.AsNoTracking()
            .Where(d => d.ProcessorId == id && (d.LanguageCode == lang || d.LanguageCode == "eng"))
            .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
            .Select(d => new { d.Html, d.Text, d.LanguageCode })
            .FirstOrDefaultAsync();

        // Mirrors GetMachinesByProcessorAsync above.
        Task<List<MachineDto>> machinesTask = machinesCtx.ProcessorsByMachine.AsNoTracking()
            .Where(p => p.ProcessorId == id)
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

        // Mirrors ProcessorPhotosController.GetGuidsByProcessorAsync. Will benefit
        // from the (ProcessorId, CreatedOn, Id) index added in MarechaiContext.
        Task<List<Guid>> photosTask = photosCtx.ProcessorPhotos.AsNoTracking()
                                               .Where(p => p.ProcessorId == id)
                                               .OrderBy(p => p.CreatedOn)
                                               .ThenBy(p => p.Id)
                                               .Select(p => p.Id)
                                               .ToListAsync();

        // Mirrors ProcessorVideosController.GetVideosByProcessorAsync.
        Task<List<ProcessorVideoDto>> videosTask = videosCtx.ProcessorVideos.AsNoTracking()
            .Where(v => v.ProcessorId == id)
            .OrderBy(v => v.Title)
            .Select(v => new ProcessorVideoDto
             {
                 Id            = v.Id,
                 ProcessorId   = v.ProcessorId,
                 ProcessorName = v.Processor.Name,
                 Provider      = v.Provider,
                 VideoId       = v.VideoId,
                 Title         = v.Title
             })
            .ToListAsync();

        await Task.WhenAll(headTask, descriptionTask, machinesTask, photosTask, videosTask);

        var head = headTask.Result;

        if(head is null) return NotFound();

        var processor = new ProcessorDto
        {
            Id                       = head.Id,
            Name                     = head.Name,
            CompanyId                = head.CompanyId,
            CompanyName              = head.CompanyName,
            ModelCode                = head.ModelCode,
            Introduced               = head.Introduced,
            IntroducedPrecision      = head.IntroducedPrecision,
            Speed                    = head.Speed,
            Package                  = head.Package,
            Gprs                     = head.Gprs,
            GprSize                  = head.GprSize,
            Fprs                     = head.Fprs,
            FprSize                  = head.FprSize,
            Cores                    = head.Cores,
            ThreadsPerCore           = head.ThreadsPerCore,
            Process                  = head.Process,
            ProcessNm                = head.ProcessNm,
            DieSize                  = head.DieSize,
            Transistors              = head.Transistors,
            DataBus                  = head.DataBus,
            AddrBus                  = head.AddrBus,
            SimdRegisters            = head.SimdRegisters,
            SimdSize                 = head.SimdSize,
            L1Instruction            = head.L1Instruction,
            L1Data                   = head.L1Data,
            L2                       = head.L2,
            L3                       = head.L3,
            InstructionSet           = head.InstructionSetName,
            InstructionSetId         = head.InstructionSetId,
            InstructionSetExtensions = head.InstructionSetExtensions
        };

        var description = descriptionTask.Result;

        return new ProcessorFullDto
        {
            Processor               = processor,
            CompanyLogo             = head.CompanyLogo,
            DescriptionHtml         = description?.Html,
            DescriptionText         = description?.Text,
            DescriptionLanguageCode = description?.LanguageCode,
            Machines                = machinesTask.Result,
            Photos                  = photosTask.Result,
            Videos                  = videosTask.Result
        };
    }

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

        string entityName = item.Name;

        context.Processors.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        // Mark any pending suggestions for this Processor as Stale and notify the suggesting users.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.Processor, id, entityName);

        // Cascade: also mark stale every per-language description suggestion for this Processor.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.ProcessorDescription, id, entityName);

        return Ok();
    }

    [HttpGet("{id:int}/description/text")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetDescriptionTextAsync(int id, [FromQuery] string lang = "eng")
    {
        // Single ordered query collapses the original two-step pattern (try requested
        // language, then English fallback) into one round-trip. Matches for the
        // requested language sort first (key 0); English fallback is key 1.
        var description = await context.ProcessorDescriptions.AsNoTracking()
            .Where(d => d.ProcessorId == id && (d.LanguageCode == lang || d.LanguageCode == "eng"))
            .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
            .Select(d => new { d.Html, d.Text })
            .FirstOrDefaultAsync();

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

        // Capture display data BEFORE the cascade, while the Processor + language rows are still
        // available for the system message body.
        string processorName = await context.Processors.AsNoTracking()
                                            .Where(p => p.Id == id)
                                            .Select(p => p.Name)
                                            .FirstOrDefaultAsync();
        string langName = await context.Iso639.AsNoTracking()
                                       .Where(l => l.Id == languageCode)
                                       .Select(l => l.ReferenceName)
                                       .FirstOrDefaultAsync();
        string subkeyLabel = $"({langName ?? languageCode} description)";

        context.ProcessorDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntitySubkeyAsync(
            context, Marechai.Data.SuggestionEntityType.ProcessorDescription,
            id, languageCode, processorName ?? $"#{id}", subkeyLabel);

        return Ok();
    }
}