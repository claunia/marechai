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

[Route("/machines")]
[ApiController]
public class MachinesController(MarechaiContext context, IDbContextFactory<MarechaiContext> dbFactory)
    : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetAsync([FromQuery] int?    skip           = null,
                                           [FromQuery] int?    take           = null,
                                           [FromQuery] string  sortBy         = null,
                                           [FromQuery] bool    sortDescending = false,
                                           [FromQuery(Name = "filters")] string[] filters = null,
                                           CancellationToken                       cancellationToken = default)
    {
        IQueryable<Machine> query = ApplyFilters(context.Machines.AsNoTracking(), filters);

        // When no user-supplied sort is set, keep the legacy default ordering
        // (Company.Name → Name → Family.Name) so anonymous unfiltered consumers
        // (App admin ViewModels, MagazinesService, BooksService, DocumentsService,
        // etc.) see the same order they get today. A user-clicked column header
        // suspends that ordering so the sort UX is predictable.
        IOrderedQueryable<Machine> ordered = sortBy switch
        {
            "Name" => sortDescending
                          ? query.OrderByDescending(m => MarechaiContext.NaturalSortKey(m.Name))
                          : query.OrderBy(m => MarechaiContext.NaturalSortKey(m.Name)),
            "Company" => sortDescending
                             ? query.OrderByDescending(m => MarechaiContext.NaturalSortKey(m.Company.Name))
                             : query.OrderBy(m => MarechaiContext.NaturalSortKey(m.Company.Name)),
            "Model" => sortDescending
                           ? query.OrderByDescending(m => MarechaiContext.NaturalSortKey(m.Model))
                           : query.OrderBy(m => MarechaiContext.NaturalSortKey(m.Model)),
            "Type" => sortDescending
                          ? query.OrderByDescending(m => m.Type)
                          : query.OrderBy(m => m.Type),
            "Prototype" => sortDescending
                               ? query.OrderByDescending(m => m.Prototype)
                               : query.OrderBy(m => m.Prototype),
            "Introduced" => sortDescending
                                ? query.OrderByDescending(m => m.Introduced)
                                : query.OrderBy(m => m.Introduced),
            "Family" => sortDescending
                            ? query.OrderByDescending(m => MarechaiContext.NaturalSortKey(m.Family.Name))
                            : query.OrderBy(m => MarechaiContext.NaturalSortKey(m.Family.Name)),
            _ => query.OrderBy(m => m.Company.Name)
                      .ThenBy(m => m.Name)
                      .ThenBy(m => m.Family.Name)
        };

        IQueryable<Machine> paged = ordered;

        if(skip.HasValue) paged = paged.Skip(skip.Value);
        if(take.HasValue) paged = paged.Take(take.Value);

        return paged.Select(m => new MachineDto
                     {
                         Id                  = m.Id,
                         Company             = m.Company.Name,
                         Name                = m.Name,
                         Model               = m.Model,
                         Introduced          = m.Introduced,
                         IntroducedPrecision = m.IntroducedPrecision,
                         Prototype           = m.Prototype,
                         Type                = m.Type,
                         Family              = m.Family.Name
                     })
                    .ToListAsync(cancellationToken);
    }

    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetCountAsync([FromQuery(Name = "filters")] string[] filters           = null,
                                   CancellationToken                      cancellationToken = default) =>
        ApplyFilters(context.Machines.AsNoTracking(), filters).CountAsync(cancellationToken);

    /// <summary>
    /// Translates MudDataGrid <c>FilterDefinition</c>s wired over the network as
    /// <c>"{Column}||{Operator}||{Value}"</c> triples into LINQ predicates against
    /// the <see cref="Machine"/> entity. Recognized columns mirror the
    /// <c>PropertyColumn</c> names emitted by the admin grid: <c>Name</c>,
    /// <c>Company</c>, <c>Model</c>, <c>Type</c>, <c>Prototype</c>,
    /// <c>Introduced</c>, <c>Family</c>. Unknown columns and operators are
    /// silently ignored — filters MudBlazor may emit for non-existent columns
    /// (or future ones) must never 400 the listing call.
    /// </summary>
    static IQueryable<Machine> ApplyFilters(IQueryable<Machine> query, string[] filters)
    {
        if(filters is null || filters.Length == 0) return query;

        foreach(string raw in filters)
        {
            if(string.IsNullOrWhiteSpace(raw)) continue;

            string[] parts = raw.Split("||", 3, StringSplitOptions.None);

            if(parts.Length < 2) continue;

            string column   = parts[0];
            string op       = parts[1];
            string value    = parts.Length >= 3 ? parts[2] : string.Empty;
            bool   isEmpty  = op == "is empty";
            bool   isNotEmp = op == "is not empty";

            // Skip non-empty-check operators with no value supplied so a stray
            // open-but-unfilled filter UI doesn't accidentally hide every row.
            if(!isEmpty && !isNotEmp && string.IsNullOrEmpty(value)) continue;

            switch(column)
            {
                case "Name":
                    query = op switch
                    {
                        "contains"     => query.Where(m => m.Name.Contains(value)),
                        "not contains" => query.Where(m => !m.Name.Contains(value)),
                        "equals"       => query.Where(m => m.Name == value),
                        "not equals"   => query.Where(m => m.Name != value),
                        "starts with"  => query.Where(m => m.Name.StartsWith(value)),
                        "ends with"    => query.Where(m => m.Name.EndsWith(value)),
                        "is empty"     => query.Where(m => m.Name == null || m.Name == string.Empty),
                        "is not empty" => query.Where(m => m.Name != null && m.Name != string.Empty),
                        _              => query
                    };
                    break;

                case "Company":
                    query = op switch
                    {
                        "contains"     => query.Where(m => m.Company.Name.Contains(value)),
                        "not contains" => query.Where(m => !m.Company.Name.Contains(value)),
                        "equals"       => query.Where(m => m.Company.Name == value),
                        "not equals"   => query.Where(m => m.Company.Name != value),
                        "starts with"  => query.Where(m => m.Company.Name.StartsWith(value)),
                        "ends with"    => query.Where(m => m.Company.Name.EndsWith(value)),
                        "is empty"     => query.Where(m => m.Company.Name == null || m.Company.Name == string.Empty),
                        "is not empty" => query.Where(m => m.Company.Name != null && m.Company.Name != string.Empty),
                        _              => query
                    };
                    break;

                case "Model":
                    query = op switch
                    {
                        "contains"     => query.Where(m => m.Model != null && m.Model.Contains(value)),
                        "not contains" => query.Where(m => m.Model == null || !m.Model.Contains(value)),
                        "equals"       => query.Where(m => m.Model == value),
                        "not equals"   => query.Where(m => m.Model != value),
                        "starts with"  => query.Where(m => m.Model != null && m.Model.StartsWith(value)),
                        "ends with"    => query.Where(m => m.Model != null && m.Model.EndsWith(value)),
                        "is empty"     => query.Where(m => m.Model == null || m.Model == string.Empty),
                        "is not empty" => query.Where(m => m.Model != null && m.Model != string.Empty),
                        _              => query
                    };
                    break;

                case "Family":
                    query = op switch
                    {
                        "contains" => query.Where(m => m.Family != null && m.Family.Name.Contains(value)),
                        "not contains" => query.Where(m =>
                                                          m.Family == null || !m.Family.Name.Contains(value)),
                        "equals"      => query.Where(m => m.Family != null && m.Family.Name == value),
                        "not equals"  => query.Where(m => m.Family == null || m.Family.Name != value),
                        "starts with" => query.Where(m => m.Family != null && m.Family.Name.StartsWith(value)),
                        "ends with"   => query.Where(m => m.Family != null && m.Family.Name.EndsWith(value)),
                        "is empty"    => query.Where(m => m.Family == null || m.Family.Name == string.Empty),
                        "is not empty" => query.Where(m =>
                                                          m.Family != null && m.Family.Name != string.Empty),
                        _ => query
                    };
                    break;

                case "Type":
                    if(!int.TryParse(value, System.Globalization.NumberStyles.Integer,
                                     System.Globalization.CultureInfo.InvariantCulture, out int typeInt))
                        continue;

                    MachineType typeVal = (MachineType)typeInt;

                    query = op switch
                    {
                        "="          => query.Where(m => m.Type == typeVal),
                        "equals"     => query.Where(m => m.Type == typeVal),
                        "!="         => query.Where(m => m.Type != typeVal),
                        "not equals" => query.Where(m => m.Type != typeVal),
                        ">"          => query.Where(m => m.Type > typeVal),
                        ">="         => query.Where(m => m.Type >= typeVal),
                        "<"          => query.Where(m => m.Type < typeVal),
                        "<="         => query.Where(m => m.Type <= typeVal),
                        _            => query
                    };
                    break;

                case "Prototype":
                    if(!bool.TryParse(value, out bool protoVal)) continue;

                    query = op switch
                    {
                        "is"     => query.Where(m => m.Prototype == protoVal),
                        "equals" => query.Where(m => m.Prototype == protoVal),
                        "is not" => query.Where(m => m.Prototype != protoVal),
                        "not equals" => query.Where(m => m.Prototype != protoVal),
                        _        => query
                    };
                    break;

                case "Introduced":
                    if(op == "is empty")
                    {
                        query = query.Where(m => m.Introduced == null);
                        break;
                    }

                    if(op == "is not empty")
                    {
                        query = query.Where(m => m.Introduced != null);
                        break;
                    }

                    if(!DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                                          System.Globalization.DateTimeStyles.AssumeUniversal |
                                          System.Globalization.DateTimeStyles.AdjustToUniversal,
                                          out DateTime parsed))
                        continue;

                    DateTime day = parsed.Date;

                    query = op switch
                    {
                        "is"              => query.Where(m => m.Introduced.HasValue && m.Introduced.Value.Date == day),
                        "is not"          => query.Where(m => m.Introduced.HasValue && m.Introduced.Value.Date != day),
                        "is after"        => query.Where(m => m.Introduced.HasValue && m.Introduced.Value.Date > day),
                        "is before"       => query.Where(m => m.Introduced.HasValue && m.Introduced.Value.Date < day),
                        "is on or after"  => query.Where(m => m.Introduced.HasValue && m.Introduced.Value.Date >= day),
                        "is on or before" => query.Where(m => m.Introduced.HasValue && m.Introduced.Value.Date <= day),
                        _                 => query
                    };
                    break;
            }
        }

        return query;
    }

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
        // Fetch the head row + the company name + the family scalar fields in
        // a single round-trip via a projected join. Replaces three separate
        // FindAsync() calls (Machines, Companies, MachineFamilies) which on a
        // remote MariaDB cost one ~57 ms RTT each.
        var head = await context.Machines.AsNoTracking()
                                .Where(m => m.Id == id)
                                .Select(m => new
                                 {
                                     m.Introduced,
                                     m.IntroducedPrecision,
                                     m.Name,
                                     m.CompanyId,
                                     m.Model,
                                     m.Prototype,
                                     m.Type,
                                     CompanyName = m.Company.Name,
                                     FamilyId    = (int?)m.FamilyId,
                                     FamilyName  = m.Family.Name
                                 })
                                .FirstOrDefaultAsync();

        if(head is null) return null;

        var model = new MachineDto
        {
            Introduced          = head.Introduced,
            IntroducedPrecision = head.IntroducedPrecision,
            Name                = head.Name,
            CompanyId           = head.CompanyId,
            Company             = head.CompanyName,
            Model               = head.Model,
            Prototype           = head.Prototype,
            Type                = head.Type,
            FamilyName          = head.FamilyName,
            FamilyId            = head.FamilyId ?? 0
        };

        // Run the company-logo lookup and the five child collection queries
        // in parallel using independent DbContext instances from the factory
        // (DbContext is not thread-safe; sharing the request-scoped context
        // would throw InvalidOperationException). On the remote MariaDB this
        // collapses six sequential ~200 ms round-trips into one parallel batch.
        int  companyId          = head.CompanyId;
        int  machineId          = id;
        int? introducedYear     = head.Introduced?.Year;

        await using var logoCtx       = await dbFactory.CreateDbContextAsync();
        await using var gpusCtx       = await dbFactory.CreateDbContextAsync();
        await using var memoryCtx     = await dbFactory.CreateDbContextAsync();
        await using var processorsCtx = await dbFactory.CreateDbContextAsync();
        await using var soundCtx      = await dbFactory.CreateDbContextAsync();
        await using var storageCtx    = await dbFactory.CreateDbContextAsync();

        // Company logo: collapse the original two-step lookup
        // (FirstOrDefaultAsync(year-filter) + Any() + FirstAsync()) into a
        // single ordered query. Logos that satisfy the year cut-off sort
        // first (key 0); any other logo falls back as key 1.
        Task<Guid?> companyLogoTask = logoCtx.CompanyLogos.AsNoTracking()
                                             .Where(l => l.CompanyId == companyId)
                                             .OrderBy(l => introducedYear.HasValue && l.Year >= introducedYear.Value
                                                               ? 0
                                                               : 1)
                                             .ThenBy(l => l.Year)
                                             .Select(l => (Guid?)l.Guid)
                                             .FirstOrDefaultAsync();

        Task<List<GpuDto>> gpusTask = gpusCtx.GpusByMachine.AsNoTracking()
                                             .Where(g => g.MachineId == machineId)
                                             .Select(g => g.Gpu)
                                             .OrderBy(g => g.Company.Name)
                                             .ThenBy(g => g.Name)
                                             .Select(g => new GpuDto
                                              {
                                                  Id                  = g.Id,
                                                  Name                = g.Name,
                                                  Company             = g.Company.Name,
                                                  CompanyId           = g.Company.Id,
                                                  ModelCode           = g.ModelCode,
                                                  Introduced          = g.Introduced,
                                                  IntroducedPrecision = g.IntroducedPrecision,
                                                  Package             = g.Package,
                                                  Process             = g.Process,
                                                  ProcessNm           = g.ProcessNm,
                                                  DieSize             = g.DieSize,
                                                  Transistors         = g.Transistors
                                              })
                                             .ToListAsync();

        Task<List<MemoryDto>> memoryTask = memoryCtx.MemoryByMachine.AsNoTracking()
                                                    .Where(m => m.MachineId == machineId)
                                                    .Select(m => new MemoryDto
                                                     {
                                                         Type  = m.Type,
                                                         Usage = m.Usage,
                                                         Size  = m.Size,
                                                         Speed = m.Speed
                                                     })
                                                    .ToListAsync();

        Task<List<ProcessorDto>> processorsTask = processorsCtx.ProcessorsByMachine.AsNoTracking()
            .Where(p => p.MachineId == machineId)
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
                 InstructionSetExtensions =
                     p.Processor.InstructionSetExtensions.Select(e => e.Extension.Extension).ToList()
             })
            .ToListAsync();

        Task<List<SoundSynthDto>> soundTask = soundCtx.SoundByMachine.AsNoTracking()
                                                      .Where(s => s.MachineId == machineId)
                                                      .Select(s => s.SoundSynth)
                                                      .OrderBy(s => s.Company.Name)
                                                      .ThenBy(s => s.Name)
                                                      .ThenBy(s => s.ModelCode)
                                                      .Select(s => new SoundSynthDto
                                                       {
                                                           Id                  = s.Id,
                                                           Name                = s.Name,
                                                           CompanyId           = s.Company.Id,
                                                           CompanyName         = s.Company.Name,
                                                           ModelCode           = s.ModelCode,
                                                           Introduced          = s.Introduced,
                                                           IntroducedPrecision = s.IntroducedPrecision,
                                                           Voices              = s.Voices,
                                                           Frequency           = s.Frequency,
                                                           Depth               = s.Depth,
                                                           SquareWave          = s.SquareWave,
                                                           WhiteNoise          = s.WhiteNoise,
                                                           Type                = s.Type
                                                       })
                                                      .ToListAsync();

        Task<List<StorageDto>> storageTask = storageCtx.StorageByMachine.AsNoTracking()
                                                       .Where(s => s.MachineId == machineId)
                                                       .Select(s => new StorageDto
                                                        {
                                                            Type      = s.Type,
                                                            Interface = s.Interface,
                                                            Capacity  = s.Capacity
                                                        })
                                                       .ToListAsync();

        await Task.WhenAll(companyLogoTask, gpusTask, memoryTask, processorsTask, soundTask, storageTask);

        model.CompanyLogo       = companyLogoTask.Result;
        model.Gpus              = gpusTask.Result;
        model.Memory            = memoryTask.Result;
        model.Processors        = processorsTask.Result;
        model.SoundSynthesizers = soundTask.Result;
        model.Storage           = storageTask.Result;

        return model;
    }

    [HttpGet("{id:int}/software")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<List<SoftwareDto>> GetSoftwareByMachineAsync(int id,
                                                                    [FromQuery] int?   skip           = null,
                                                                    [FromQuery] int?   take           = null,
                                                                    [FromQuery] string search         = null,
                                                                    [FromQuery] string sortBy         = null,
                                                                    [FromQuery] bool   sortDescending = false,
                                                                    CancellationToken  cancellationToken = default)
    {
        // Step 1: fast indexed lookup — one small table, no joins.
        List<ulong> platformIds = await context.SoftwarePlatformsByMachine
                                               .Where(sp => sp.MachineId == id)
                                               .Select(sp => sp.SoftwarePlatformId)
                                               .ToListAsync(cancellationToken);

        if(platformIds.Count == 0)
            return [];

        // Step 2: resolve software IDs via flat JOINs from SoftwareReleases outward.
        IQueryable<ulong> viaDirect = context.SoftwareReleases
                                             .Where(r => r.SoftwareId != null &&
                                                         r.PlatformId != null &&
                                                         platformIds.Contains(r.PlatformId.Value))
                                             .Select(r => r.SoftwareId!.Value);

        IQueryable<ulong> viaVersion = context.SoftwareReleases
                                              .Where(r => r.SoftwareVersionId != null &&
                                                          r.PlatformId != null &&
                                                          platformIds.Contains(r.PlatformId.Value))
                                              .Select(r => r.SoftwareVersion.SoftwareId);

        List<ulong> softwareIds = await viaDirect.Union(viaVersion).Distinct().ToListAsync(cancellationToken);

        if(softwareIds.Count == 0)
            return [];

        // Step 3: fetch software rows with optional search filter, sort and paging.
        IQueryable<Software> baseQuery = context.Softwares.Where(s => softwareIds.Contains(s.Id));

        if(!string.IsNullOrWhiteSpace(search))
            baseQuery = baseQuery.Where(s => s.Name.Contains(search));

        IQueryable<SoftwareDto> projected = baseQuery.Select(s => new SoftwareDto
        {
            Id       = s.Id,
            Name     = s.Name,
            FamilyId = s.FamilyId,
            Family   = s.Family.Name,
            Kind     = s.Kind
        });

        IQueryable<SoftwareDto> ordered = sortBy switch
        {
            "Name"   => sortDescending ? projected.OrderByDescending(s => MarechaiContext.NaturalSortKey(s.Name))
                                       : projected.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name)),
            "Family" => sortDescending ? projected.OrderByDescending(s => MarechaiContext.NaturalSortKey(s.Family))
                                       : projected.OrderBy(s => MarechaiContext.NaturalSortKey(s.Family)),
            "Kind"   => sortDescending ? projected.OrderByDescending(s => s.Kind)
                                       : projected.OrderBy(s => s.Kind),
            _        => projected.OrderBy(s => MarechaiContext.NaturalSortKey(s.Name))
        };

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        List<SoftwareDto> softwares = await ordered.ToListAsync(cancellationToken);

        if(softwares.Count == 0)
            return softwares;

        // Step 4: bulk cover lookup — only for the IDs on this page (at most pageSize items).
        List<ulong> pageIds = softwares.Select(s => s.Id).ToList();

        Dictionary<ulong, Guid> frontCovers =
            (await context.SoftwareCovers
                          .Where(c => c.Type               == SoftwareCoverType.Front &&
                                      c.Release.SoftwareId != null                   &&
                                      pageIds.Contains(c.Release.SoftwareId.Value))
                          .Select(c => new { SoftwareId = c.Release.SoftwareId!.Value, CoverId = c.Id })
                          .ToListAsync(cancellationToken))
            .GroupBy(x => x.SoftwareId)
            .ToDictionary(g => g.Key, g => g.First().CoverId);

        List<ulong> uncoveredIds = pageIds.Except(frontCovers.Keys).ToList();

        if(uncoveredIds.Count > 0)
        {
            foreach(var row in await context.SoftwareCovers
                                            .Where(c => c.Type                      == SoftwareCoverType.Front &&
                                                        c.Release.SoftwareVersionId != null                   &&
                                                        uncoveredIds.Contains(c.Release.SoftwareVersion.SoftwareId))
                                            .Select(c => new
                                             {
                                                 SoftwareId = c.Release.SoftwareVersion.SoftwareId,
                                                 CoverId    = c.Id
                                             })
                                            .ToListAsync(cancellationToken))
                frontCovers.TryAdd(row.SoftwareId, row.CoverId);
        }

        foreach(SoftwareDto s in softwares)
            if(frontCovers.TryGetValue(s.Id, out Guid coverId))
                s.FrontCoverId = coverId;

        return softwares;
    }

    [HttpGet("{id:int}/software/count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<int> GetSoftwareByMachineCountAsync(int id,
                                                           [FromQuery] string search = null,
                                                           CancellationToken cancellationToken = default)
    {
        List<ulong> platformIds = await context.SoftwarePlatformsByMachine
                                               .Where(sp => sp.MachineId == id)
                                               .Select(sp => sp.SoftwarePlatformId)
                                               .ToListAsync(cancellationToken);

        if(platformIds.Count == 0)
            return 0;

        IQueryable<ulong> viaDirect = context.SoftwareReleases
                                             .Where(r => r.SoftwareId != null &&
                                                         r.PlatformId != null &&
                                                         platformIds.Contains(r.PlatformId.Value))
                                             .Select(r => r.SoftwareId!.Value);

        IQueryable<ulong> viaVersion = context.SoftwareReleases
                                              .Where(r => r.SoftwareVersionId != null &&
                                                          r.PlatformId != null &&
                                                          platformIds.Contains(r.PlatformId.Value))
                                              .Select(r => r.SoftwareVersion.SoftwareId);

        List<ulong> softwareIds = await viaDirect.Union(viaVersion).Distinct().ToListAsync(cancellationToken);

        if(softwareIds.Count == 0)
            return 0;

        IQueryable<Software> query = context.Softwares.Where(s => softwareIds.Contains(s.Id));

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Name.Contains(search));

        return await query.CountAsync(cancellationToken);
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

        string entityName = item.Name;

        context.Machines.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        // Mark any pending suggestions for this machine as Stale and notify the suggesting users.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.Machine, id, entityName);

        // Cascade: also mark stale every per-language description suggestion for this machine.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.MachineDescription, id, entityName);

        return Ok();
    }

    [HttpGet("{id:int}/description/text")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetDescriptionTextAsync(int id, [FromQuery] string lang = "eng")
    {
        // Collapse the requested-language + English-fallback lookup into a single
        // ordered query: the requested language sorts first (key 0), English (key
        // 1) is the fallback, anything else (key 2) is filtered out by Take(1).
        // Saves one ~57 ms RTT when the requested language has no description.
        var description = await context.MachineDescriptions.AsNoTracking()
                                       .Where(d => d.MachineId == id &&
                                                   (d.LanguageCode == lang || d.LanguageCode == "eng"))
                                       .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
                                       .Select(d => new { d.Html, d.Text })
                                       .FirstOrDefaultAsync();

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
    public Task<MachineDescriptionDto> GetDescriptionAsync(int id, [FromQuery] string lang = "eng")
    {
        // Collapse the requested-language + English-fallback lookup into a single
        // ordered query (see GetDescriptionTextAsync for rationale).
        return context.MachineDescriptions.AsNoTracking()
                      .Where(d => d.MachineId == id && (d.LanguageCode == lang || d.LanguageCode == "eng"))
                      .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
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

        // Capture display data BEFORE the cascade, while the machine + language rows are still
        // available for the system message body.
        string machineName = await context.Machines.AsNoTracking()
                                          .Where(m => m.Id == id)
                                          .Select(m => m.Name)
                                          .FirstOrDefaultAsync();
        string langName    = await context.Iso639.AsNoTracking()
                                          .Where(l => l.Id == languageCode)
                                          .Select(l => l.ReferenceName)
                                          .FirstOrDefaultAsync();
        string subkeyLabel = $"({langName ?? languageCode} description)";

        context.MachineDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntitySubkeyAsync(
            context, Marechai.Data.SuggestionEntityType.MachineDescription,
            id, languageCode, machineName ?? $"#{id}", subkeyLabel);

        return Ok();
    }
}