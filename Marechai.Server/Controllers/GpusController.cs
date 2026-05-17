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

[Route("/gpus")]
[ApiController]
public class GpusController(MarechaiContext context, IDbContextFactory<MarechaiContext> dbFactory) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<GpuDto>> GetAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                       [FromQuery] string sortBy = null,
                                       [FromQuery] bool sortDescending = false,
                                       [FromQuery(Name = "filters")] string[] filters = null,
                                       CancellationToken cancellationToken = default)
    {
        IQueryable<Gpu> query = ApplyFilters(context.Gpus.AsNoTracking(), filters);

        // When no user-supplied sort is set, keep the legacy default ordering that
        // pins the special "DB_FRAMEBUFFER", "DB_SOFTWARE" and "DB_NONE" rows to the
        // top so the public /gpus page can display them first across paginated
        // batches. A user-clicked column header suspends the pin so the sort UX is
        // predictable.
        IOrderedQueryable<Gpu> ordered = sortBy switch
        {
            "Name"      => sortDescending
                               ? query.OrderByDescending(g => MarechaiContext.NaturalSortKey(g.Name))
                               : query.OrderBy(g => MarechaiContext.NaturalSortKey(g.Name)),
            "Company"   => sortDescending
                               ? query.OrderByDescending(g => MarechaiContext.NaturalSortKey(g.Company.Name))
                               : query.OrderBy(g => MarechaiContext.NaturalSortKey(g.Company.Name)),
            "ModelCode" => sortDescending
                               ? query.OrderByDescending(g => MarechaiContext.NaturalSortKey(g.ModelCode))
                               : query.OrderBy(g => MarechaiContext.NaturalSortKey(g.ModelCode)),
            "Introduced" => sortDescending
                                ? query.OrderByDescending(g => g.Introduced)
                                : query.OrderBy(g => g.Introduced),
            _ => query.OrderBy(g => g.Name == "DB_FRAMEBUFFER" ? 0 :
                                    g.Name == "DB_SOFTWARE"    ? 1 :
                                    g.Name == "DB_NONE"        ? 2 : 3)
                      .ThenBy(g => g.Company.Name)
                      .ThenBy(g => g.Name)
                      .ThenBy(g => g.Introduced)
        };

        IQueryable<Gpu> paged = ordered;

        if(skip.HasValue) paged = paged.Skip(skip.Value);
        if(take.HasValue) paged = paged.Take(take.Value);

        return paged.Select(g => new GpuDto
                     {
                         Id                  = g.Id,
                         Company             = g.Company.Name,
                         CompanyId           = g.CompanyId,
                         Introduced          = g.Introduced,
                         IntroducedPrecision = g.IntroducedPrecision,
                         ModelCode           = g.ModelCode,
                         Name                = g.Name
                     })
                    .ToListAsync(cancellationToken);
    }

    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetCountAsync([FromQuery(Name = "filters")] string[] filters = null,
                                   CancellationToken cancellationToken = default) =>
        ApplyFilters(context.Gpus.AsNoTracking(), filters).CountAsync(cancellationToken);

    /// <summary>
    /// Translates MudDataGrid <c>FilterDefinition</c>s wired over the network as
    /// <c>"{Column}||{Operator}||{Value}"</c> triples into LINQ predicates against
    /// the <see cref="Gpu"/> entity. Recognized columns mirror the
    /// <c>PropertyColumn</c> names emitted by the admin grid: <c>Name</c>,
    /// <c>Company</c>, <c>ModelCode</c>, <c>Introduced</c>. Unknown columns and
    /// operators are silently ignored — filters MudBlazor may emit for non-
    /// existent columns (or future ones) must never 400 the listing call.
    /// </summary>
    static IQueryable<Gpu> ApplyFilters(IQueryable<Gpu> query, string[] filters)
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
                        "contains"      => query.Where(g => g.Name.Contains(value)),
                        "not contains"  => query.Where(g => !g.Name.Contains(value)),
                        "equals"        => query.Where(g => g.Name == value),
                        "not equals"    => query.Where(g => g.Name != value),
                        "starts with"   => query.Where(g => g.Name.StartsWith(value)),
                        "ends with"     => query.Where(g => g.Name.EndsWith(value)),
                        "is empty"      => query.Where(g => g.Name == null || g.Name == string.Empty),
                        "is not empty"  => query.Where(g => g.Name != null && g.Name != string.Empty),
                        _               => query
                    };
                    break;

                case "Company":
                    query = op switch
                    {
                        "contains"      => query.Where(g => g.Company.Name.Contains(value)),
                        "not contains"  => query.Where(g => !g.Company.Name.Contains(value)),
                        "equals"        => query.Where(g => g.Company.Name == value),
                        "not equals"    => query.Where(g => g.Company.Name != value),
                        "starts with"   => query.Where(g => g.Company.Name.StartsWith(value)),
                        "ends with"     => query.Where(g => g.Company.Name.EndsWith(value)),
                        "is empty"      => query.Where(g => g.Company.Name == null || g.Company.Name == string.Empty),
                        "is not empty"  => query.Where(g => g.Company.Name != null && g.Company.Name != string.Empty),
                        _               => query
                    };
                    break;

                case "ModelCode":
                    query = op switch
                    {
                        "contains"      => query.Where(g => g.ModelCode != null && g.ModelCode.Contains(value)),
                        "not contains"  => query.Where(g => g.ModelCode == null || !g.ModelCode.Contains(value)),
                        "equals"        => query.Where(g => g.ModelCode == value),
                        "not equals"    => query.Where(g => g.ModelCode != value),
                        "starts with"   => query.Where(g => g.ModelCode != null && g.ModelCode.StartsWith(value)),
                        "ends with"     => query.Where(g => g.ModelCode != null && g.ModelCode.EndsWith(value)),
                        "is empty"      => query.Where(g => g.ModelCode == null || g.ModelCode == string.Empty),
                        "is not empty"  => query.Where(g => g.ModelCode != null && g.ModelCode != string.Empty),
                        _               => query
                    };
                    break;

                case "Introduced":
                    if(op == "is empty")
                    {
                        query = query.Where(g => g.Introduced == null);
                        break;
                    }

                    if(op == "is not empty")
                    {
                        query = query.Where(g => g.Introduced != null);
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
                        "is"               => query.Where(g => g.Introduced.HasValue && g.Introduced.Value.Date == day),
                        "is not"           => query.Where(g => g.Introduced.HasValue && g.Introduced.Value.Date != day),
                        "is after"         => query.Where(g => g.Introduced.HasValue && g.Introduced.Value.Date > day),
                        "is before"        => query.Where(g => g.Introduced.HasValue && g.Introduced.Value.Date < day),
                        "is on or after"   => query.Where(g => g.Introduced.HasValue && g.Introduced.Value.Date >= day),
                        "is on or before"  => query.Where(g => g.Introduced.HasValue && g.Introduced.Value.Date <= day),
                        _                  => query
                    };
                    break;
            }
        }

        return query;
    }

    [HttpGet("/machines/{machineId:int}/gpus")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<GpuDto>> GetByMachineAsync(int machineId) => context.GpusByMachine.AsNoTracking()
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
    public Task<List<MachineDto>> GetMachinesByGpuAsync(int gpuId) => context.GpusByMachine.AsNoTracking()
                                                                             .Where(g => g.GpuId == gpuId)
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
    public Task<GpuDto> GetAsync(int id) => context.Gpus.AsNoTracking()
                                                   .Where(g => g.Id == id)
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
                                                   .FirstOrDefaultAsync();

    /// <summary>
    /// Consolidated payload for the public /gpu/{Id} view page. Replaces six
    /// sequential HTTP round-trips (head + resolutions + machines + description +
    /// photos + videos) with a single response. The head + company name + company
    /// logo are projected in one query (logo is an inline correlated subquery so
    /// it costs zero extra DB round-trips); the description language fallback is
    /// collapsed into a single ordered query; the four child collections are
    /// fetched in parallel using independent <see cref="MarechaiContext"/>
    /// instances from <c>IDbContextFactory</c> (DbContext is not thread-safe;
    /// sharing the request-scoped context across parallel branches throws
    /// <c>InvalidOperationException</c>).
    /// </summary>
    [HttpGet("{id:int}/full")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GpuFullDto>> GetFullAsync(int id, [FromQuery] string lang = "eng")
    {
        // The four child collections plus the description only depend on `id`
        // (from the URL), and not on any value projected by the head query, so
        // we fire all SIX queries — head + description + resolutions + machines
        // + photos + videos — in parallel using independent DbContext instances
        // from the factory (DbContext is not thread-safe). Compared to running
        // head first and then the children in parallel, this saves one full
        // ~180 ms RTT in the common case. The company-logo lookup is folded
        // into the head query as an inline correlated subquery so it adds zero
        // extra DB round-trips.
        await using var headCtx        = await dbFactory.CreateDbContextAsync();
        await using var descriptionCtx = await dbFactory.CreateDbContextAsync();
        await using var resolutionsCtx = await dbFactory.CreateDbContextAsync();
        await using var machinesCtx    = await dbFactory.CreateDbContextAsync();
        await using var photosCtx      = await dbFactory.CreateDbContextAsync();
        await using var videosCtx      = await dbFactory.CreateDbContextAsync();

        // Head + company name + company logo in a single projected join.
        // The CompanyLogo lookup uses an inline correlated subquery ordered
        // such that logos issued on or after the GPU's introduction year sort
        // first (key 0), then any other logo by ascending year. Returns null
        // when the GPU does not exist; we surface that as 404 below.
        var headTask = headCtx.Gpus.AsNoTracking()
                              .Where(g => g.Id == id)
                              .Select(g => new
                               {
                                   g.Id,
                                   g.Name,
                                   g.CompanyId,
                                   CompanyName         = g.Company.Name,
                                   g.ModelCode,
                                   g.Introduced,
                                   g.IntroducedPrecision,
                                   g.Package,
                                   g.Process,
                                   g.ProcessNm,
                                   g.DieSize,
                                   g.Transistors,
                                   IntroducedYear      = (int?)(g.Introduced.HasValue ? g.Introduced.Value.Year : (int?)null),
                                   CompanyLogo         = g.Company.Logos
                                                          .OrderBy(l => g.Introduced.HasValue && l.Year >= g.Introduced.Value.Year
                                                                            ? 0
                                                                            : 1)
                                                          .ThenBy(l => l.Year)
                                                          .Select(l => (Guid?)l.Guid)
                                                          .FirstOrDefault()
                               })
                              .FirstOrDefaultAsync();

        // Description: collapse the original two-step lookup (try requested
        // lang, then English fallback) into a single ordered query. Matches
        // for the requested language sort first (key 0); English fallback is
        // key 1; FirstOrDefaultAsync returns the preferred row.
        var descriptionTask = descriptionCtx.GpuDescriptions.AsNoTracking()
            .Where(d => d.GpuId == id && (d.LanguageCode == lang || d.LanguageCode == "eng"))
            .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
            .Select(d => new { d.Html, d.Text, d.LanguageCode })
            .FirstOrDefaultAsync();

        // Mirrors ResolutionsByGpuController.GetByGpu — projection identical;
        // ordering happens client-side after materialization to mirror existing
        // contract (multi-key sort across nested fields is awkward in SQL).
        Task<List<ResolutionByGpuDto>> resolutionsTask = resolutionsCtx.ResolutionsByGpu.AsNoTracking()
            .Where(r => r.GpuId == id)
            .Select(r => new ResolutionByGpuDto
             {
                 Id    = r.Id,
                 GpuId = r.GpuId,
                 Resolution = new ResolutionDto
                 {
                     Id        = r.Resolution.Id,
                     Width     = r.Resolution.Width,
                     Height    = r.Resolution.Height,
                     Colors    = r.Resolution.Colors,
                     Palette   = r.Resolution.Palette,
                     Chars     = r.Resolution.Chars,
                     Grayscale = r.Resolution.Grayscale
                 },
                 ResolutionId = r.ResolutionId
             })
            .ToListAsync();

        // Mirrors GpusController.GetMachinesByGpuAsync.
        Task<List<MachineDto>> machinesTask = machinesCtx.GpusByMachine.AsNoTracking()
            .Where(g => g.GpuId == id)
            .Select(g => g.Machine)
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

        // Mirrors GpuPhotosController.GetGuidsByGpuAsync.
        Task<List<Guid>> photosTask = photosCtx.GpuPhotos.AsNoTracking()
                                               .Where(p => p.GpuId == id)
                                               .OrderBy(p => p.CreatedOn)
                                               .ThenBy(p => p.Id)
                                               .Select(p => p.Id)
                                               .ToListAsync();

        // Mirrors GpuVideosController.GetVideosByGpuAsync.
        Task<List<GpuVideoDto>> videosTask = videosCtx.GpuVideos.AsNoTracking()
            .Where(v => v.GpuId == id)
            .OrderBy(v => v.Title)
            .Select(v => new GpuVideoDto
             {
                 Id       = v.Id,
                 GpuId    = v.GpuId,
                 GpuName  = v.Gpu.Name,
                 Provider = v.Provider,
                 VideoId  = v.VideoId,
                 Title    = v.Title
             })
            .ToListAsync();

        await Task.WhenAll(headTask, descriptionTask, resolutionsTask, machinesTask, photosTask, videosTask);

        var head = headTask.Result;

        if(head is null) return NotFound();

        var gpu = new GpuDto
        {
            Id                  = head.Id,
            Name                = head.Name,
            CompanyId           = head.CompanyId,
            Company             = head.CompanyName,
            ModelCode           = head.ModelCode,
            Introduced          = head.Introduced,
            IntroducedPrecision = head.IntroducedPrecision,
            Package             = head.Package,
            Process             = head.Process,
            ProcessNm           = head.ProcessNm,
            DieSize             = head.DieSize,
            Transistors         = head.Transistors
        };

        // Multi-key ordering applied client-side, mirroring
        // ResolutionsByGpuController.GetByGpu.
        List<ResolutionDto> orderedResolutions = resolutionsTask.Result
            .OrderBy(r => r.Resolution.Width)
            .ThenBy(r => r.Resolution.Height)
            .ThenBy(r => r.Resolution.Chars)
            .ThenBy(r => r.Resolution.Grayscale)
            .ThenBy(r => r.Resolution.Colors)
            .ThenBy(r => r.Resolution.Palette)
            .Select(r => r.Resolution)
            .ToList();

        var description = descriptionTask.Result;

        return new GpuFullDto
        {
            Gpu                     = gpu,
            CompanyLogo             = head.CompanyLogo,
            DescriptionHtml         = description?.Html,
            DescriptionText         = description?.Text,
            DescriptionLanguageCode = description?.LanguageCode,
            Resolutions             = orderedResolutions,
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

        string entityName = item.Name;

        context.Gpus.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        // Mark any pending suggestions for this GPU as Stale and notify the suggesting users.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.Gpu, id, entityName);

        // Cascade: also mark stale every per-language description suggestion for this GPU.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.GpuDescription, id, entityName);

        return Ok();
    }

    [HttpGet("{id:int}/description/text")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetDescriptionTextAsync(int id, [FromQuery] string lang = "eng")
    {
        // Collapse the original two-step lookup (try requested lang, then English
        // fallback) into a single ordered query. Descriptions matching the requested
        // language sort first (key 0); English fallback is key 1; FirstOrDefaultAsync
        // returns the preferred row in one round-trip.
        var description = await context.GpuDescriptions.AsNoTracking()
                                       .Where(d => d.GpuId == id &&
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
    public Task<List<GpuDescriptionDto>> GetDescriptionsAsync(int id) => context.GpuDescriptions.AsNoTracking()
       .Where(d => d.GpuId == id)
       .Select(d => new GpuDescriptionDto
        {
            Id           = d.Id,
            GpuId        = d.GpuId,
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
    public async Task<GpuDescriptionDto> GetDescriptionAsync(int id, [FromQuery] string lang = "eng")
    {
        // Collapse the original two-step lookup (try requested lang, then English
        // fallback) into a single ordered query. Descriptions matching the requested
        // language sort first (key 0); English fallback is key 1; FirstOrDefaultAsync
        // returns the preferred row in one round-trip.
        GpuDescriptionDto description = await context.GpuDescriptions.AsNoTracking()
                                                     .Where(d => d.GpuId == id &&
                                                                 (d.LanguageCode == lang ||
                                                                  d.LanguageCode == "eng"))
                                                     .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
                                                     .Select(d => new GpuDescriptionDto
                                                      {
                                                          Id           = d.Id,
                                                          GpuId        = d.GpuId,
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
        int id, [FromBody] GpuDescriptionDto description)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        GpuDescription current = await context.GpuDescriptions
                                                  .FirstOrDefaultAsync(d => d.GpuId        == id &&
                                                                            d.LanguageCode == description.LanguageCode);

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        string             html     = Markdown.ToHtml(description.Markdown, pipeline);

        if(current is null)
        {
            current = new GpuDescription
            {
                GpuId        = id,
                LanguageCode = description.LanguageCode,
                Html         = html,
                Text         = description.Markdown
            };

            await context.GpuDescriptions.AddAsync(current);
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

        GpuDescription description = await context.GpuDescriptions
                                                      .FirstOrDefaultAsync(d => d.GpuId        == id &&
                                                                                d.LanguageCode == languageCode);

        if(description is null) return NotFound();

        // Capture display data BEFORE the cascade, while the GPU + language rows are still
        // available for the system message body.
        string gpuName = await context.Gpus.AsNoTracking()
                                      .Where(g => g.Id == id)
                                      .Select(g => g.Name)
                                      .FirstOrDefaultAsync();
        string langName = await context.Iso639.AsNoTracking()
                                       .Where(l => l.Id == languageCode)
                                       .Select(l => l.ReferenceName)
                                       .FirstOrDefaultAsync();
        string subkeyLabel = $"({langName ?? languageCode} description)";

        context.GpuDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntitySubkeyAsync(
            context, Marechai.Data.SuggestionEntityType.GpuDescription,
            id, languageCode, gpuName ?? $"#{id}", subkeyLabel);

        return Ok();
    }
}