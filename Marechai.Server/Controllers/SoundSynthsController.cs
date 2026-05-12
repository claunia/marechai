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

[Route("/sound-synths")]
[ApiController]
public class SoundSynthsController(
    MarechaiContext                  context,
    IDbContextFactory<MarechaiContext> dbFactory) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoundSynthDto>> GetAsync([FromQuery] int? skip = null, [FromQuery] int? take = null,
                                              CancellationToken cancellationToken = default)
    {
        IQueryable<SoundSynth> ordered = context.SoundSynths
                                                .AsNoTracking()
                                                // Pin the special "DB_SOFTWARE" row to the top so the public
                                                // /soundsynths page can display it first across paginated batches.
                                                .OrderBy(s => s.Name == "DB_SOFTWARE" ? 0 : 1)
                                                .ThenBy(s => s.Company.Name)
                                                .ThenBy(s => s.Name)
                                                .ThenBy(s => s.ModelCode);

        if(skip.HasValue) ordered = ordered.Skip(skip.Value);
        if(take.HasValue) ordered = ordered.Take(take.Value);

        return ordered.Select(s => new SoundSynthDto
                       {
                           Id          = s.Id,
                           Name        = s.Name,
                           CompanyId   = s.Company.Id,
                           CompanyName = s.Company.Name,
                           ModelCode   = s.ModelCode,
                           Introduced  = s.Introduced,
                           Voices      = s.Voices,
                           Frequency   = s.Frequency,
                           Depth       = s.Depth,
                           SquareWave  = s.SquareWave,
                           WhiteNoise  = s.WhiteNoise,
                           Type        = s.Type
                       })
                      .ToListAsync(cancellationToken);
    }

    [HttpGet("count")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<int> GetCountAsync(CancellationToken cancellationToken = default) =>
        context.SoundSynths.CountAsync(cancellationToken);

    [HttpGet("/machines/{machineId:int}/sound-synths")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoundSynthDto>> GetByMachineAsync(int machineId) => context.SoundByMachine
       .Where(s => s.MachineId == machineId)
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

    [HttpGet("{soundSynthId:int}/machines")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<MachineDto>> GetMachinesBySoundSynthAsync(int soundSynthId) =>
        context.SoundByMachine.Where(s => s.SoundSynthId == soundSynthId)
               .Select(s => s.Machine)
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

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<SoundSynthDto> GetAsync(int id) => context.SoundSynths.AsNoTracking()
                                                          .Where(s => s.Id == id)
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
                                                          .FirstOrDefaultAsync();

    /// <summary>
    /// Consolidated payload for the public /soundsynth/{Id} view page. Replaces five
    /// sequential HTTP round-trips (head + machines + description + photos + videos)
    /// with a single response. The head + company name + company logo are projected in
    /// one query (logo is an inline correlated subquery so it costs zero extra DB
    /// round-trips); the description language fallback is collapsed into a single
    /// ordered query; the three child collections are fetched in parallel using
    /// independent <see cref="MarechaiContext"/> instances from <c>IDbContextFactory</c>
    /// (DbContext is not thread-safe; sharing the request-scoped context across parallel
    /// branches throws <c>InvalidOperationException</c>).
    /// </summary>
    [HttpGet("{id:int}/full")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoundSynthFullDto>> GetFullAsync(int id, [FromQuery] string lang = "eng")
    {
        // Head + description + the three child collections only depend on `id`
        // (from the URL) and not on any value projected by the head query, so we
        // fire all FIVE queries in parallel using independent DbContext instances
        // from the factory. Compared to running head first and then the children
        // in parallel, this saves one full ~180 ms RTT in the common case. The
        // company-logo lookup is folded into the head query as an inline
        // correlated subquery so it adds zero extra DB round-trips.
        await using var headCtx        = await dbFactory.CreateDbContextAsync();
        await using var descriptionCtx = await dbFactory.CreateDbContextAsync();
        await using var machinesCtx    = await dbFactory.CreateDbContextAsync();
        await using var photosCtx      = await dbFactory.CreateDbContextAsync();
        await using var videosCtx      = await dbFactory.CreateDbContextAsync();

        // Head + company name + company logo in a single projected join.
        // The CompanyLogo lookup uses an inline correlated subquery ordered
        // such that logos issued on or after the synth's introduction year sort
        // first (key 0), then any other logo by ascending year. Returns null
        // when the synth does not exist; we surface that as 404 below.
        var headTask = headCtx.SoundSynths.AsNoTracking()
                              .Where(s => s.Id == id)
                              .Select(s => new
                               {
                                   s.Id,
                                   s.Name,
                                   s.CompanyId,
                                   CompanyName         = s.Company.Name,
                                   s.ModelCode,
                                   s.Introduced,
                                   s.IntroducedPrecision,
                                   s.Voices,
                                   s.Frequency,
                                   s.Depth,
                                   s.SquareWave,
                                   s.WhiteNoise,
                                   s.Type,
                                   CompanyLogo         = s.Company.Logos
                                                          .OrderBy(l => s.Introduced.HasValue && l.Year >= s.Introduced.Value.Year
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
        var descriptionTask = descriptionCtx.SoundSynthDescriptions.AsNoTracking()
            .Where(d => d.SoundSynthId == id && (d.LanguageCode == lang || d.LanguageCode == "eng"))
            .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
            .Select(d => new { d.Html, d.Text, d.LanguageCode })
            .FirstOrDefaultAsync();

        // Mirrors GetMachinesBySoundSynthAsync.
        Task<List<MachineDto>> machinesTask = machinesCtx.SoundByMachine.AsNoTracking()
            .Where(s => s.SoundSynthId == id)
            .Select(s => s.Machine)
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

        // Mirrors SoundSynthPhotosController.GetGuidsBySoundSynthAsync.
        Task<List<Guid>> photosTask = photosCtx.SoundSynthPhotos.AsNoTracking()
                                               .Where(p => p.SoundSynthId == id)
                                               .OrderBy(p => p.CreatedOn)
                                               .ThenBy(p => p.Id)
                                               .Select(p => p.Id)
                                               .ToListAsync();

        // Mirrors SoundSynthVideosController.GetVideosBySoundSynthAsync.
        Task<List<SoundSynthVideoDto>> videosTask = videosCtx.SoundSynthVideos.AsNoTracking()
            .Where(v => v.SoundSynthId == id)
            .OrderBy(v => v.Title)
            .Select(v => new SoundSynthVideoDto
             {
                 Id             = v.Id,
                 SoundSynthId   = v.SoundSynthId,
                 SoundSynthName = v.SoundSynth.Name,
                 Provider       = v.Provider,
                 VideoId        = v.VideoId,
                 Title          = v.Title
             })
            .ToListAsync();

        await Task.WhenAll(headTask, descriptionTask, machinesTask, photosTask, videosTask);

        var head = headTask.Result;

        if(head is null) return NotFound();

        var synth = new SoundSynthDto
        {
            Id                  = head.Id,
            Name                = head.Name,
            CompanyId           = head.CompanyId,
            CompanyName         = head.CompanyName,
            ModelCode           = head.ModelCode,
            Introduced          = head.Introduced,
            IntroducedPrecision = head.IntroducedPrecision,
            Voices              = head.Voices,
            Frequency           = head.Frequency,
            Depth               = head.Depth,
            SquareWave          = head.SquareWave,
            WhiteNoise          = head.WhiteNoise,
            Type                = head.Type
        };

        var description = descriptionTask.Result;

        return new SoundSynthFullDto
        {
            SoundSynth              = synth,
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
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] SoundSynthDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();
        SoundSynth model = await context.SoundSynths.FindAsync(id);

        if(model is null) return NotFound();

        model.Depth      = dto.Depth;
        model.Frequency  = dto.Frequency;
        model.Introduced = dto.Introduced;
        model.IntroducedPrecision = dto.IntroducedPrecision;
        model.Name       = dto.Name;
        model.Type       = dto.Type;
        model.Voices     = dto.Voices;
        model.CompanyId  = dto.CompanyId;
        model.ModelCode  = dto.ModelCode;
        model.SquareWave = dto.SquareWave;
        model.WhiteNoise = dto.WhiteNoise;

        Company company  = dto.CompanyId is not null ? await context.Companies.FindAsync(dto.CompanyId) : null;
        string  newsName = company is not null ? $"{company.Name} {dto.Name}" : dto.Name;

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.UpdatedSoundSynthInDb,
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
    public async Task<ActionResult<long>> CreateAsync([FromBody] SoundSynthDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        var model = new SoundSynth
        {
            Depth      = dto.Depth,
            Frequency  = dto.Frequency,
            Introduced = dto.Introduced,
            IntroducedPrecision = dto.IntroducedPrecision,
            Name       = dto.Name,
            Type       = dto.Type,
            Voices     = dto.Voices,
            CompanyId  = dto.CompanyId,
            ModelCode  = dto.ModelCode,
            SquareWave = dto.SquareWave,
            WhiteNoise = dto.WhiteNoise
        };

        await context.SoundSynths.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        Company company  = dto.CompanyId is not null ? await context.Companies.FindAsync(dto.CompanyId) : null;
        string  newsName = company is not null ? $"{company.Name} {dto.Name}" : dto.Name;

        await context.News.AddAsync(new News
        {
            AddedId = model.Id,
            Date    = DateTime.UtcNow,
            Type    = NewsType.NewSoundSynthInDb,
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
        SoundSynth item = await context.SoundSynths.FindAsync(id);

        if(item is null) return NotFound();

        string entityName = item.Name;

        context.SoundSynths.Remove(item);

        await context.SaveChangesWithUserAsync(userId);

        // Mark any pending suggestions for this SoundSynth as Stale and notify the suggesting users.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.SoundSynth, id, entityName);

        // Cascade: also mark stale every per-language description suggestion for this SoundSynth.
        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntityAsync(
            context, Marechai.Data.SuggestionEntityType.SoundSynthDescription, id, entityName);

        return Ok();
    }

    [HttpGet("{id:int}/description/text")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<string> GetDescriptionTextAsync(int id, [FromQuery] string lang = "eng")
    {
        // Single ordered query: requested-language matches sort first (key 0),
        // English fallback is key 1. Saves one ~RTT vs the original two-step
        // lookup when the fallback fires.
        var description = await context.SoundSynthDescriptions.AsNoTracking()
            .Where(d => d.SoundSynthId == id && (d.LanguageCode == lang || d.LanguageCode == "eng"))
            .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
            .Select(d => new { d.Html, d.Text })
            .FirstOrDefaultAsync();

        return description?.Html ?? description?.Text;
    }

    [HttpGet("{id:int}/descriptions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<List<SoundSynthDescriptionDto>> GetDescriptionsAsync(int id) => context.SoundSynthDescriptions
       .Where(d => d.SoundSynthId == id)
       .Select(d => new SoundSynthDescriptionDto
        {
            Id           = d.Id,
            SoundSynthId = d.SoundSynthId,
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
    public Task<SoundSynthDescriptionDto> GetDescriptionAsync(int id, [FromQuery] string lang = "eng") =>
        // Single ordered query: requested-language matches sort first (key 0),
        // English fallback is key 1. Saves one ~RTT vs the original two-step
        // lookup when the fallback fires.
        context.SoundSynthDescriptions.AsNoTracking()
               .Where(d => d.SoundSynthId == id && (d.LanguageCode == lang || d.LanguageCode == "eng"))
               .OrderBy(d => d.LanguageCode == lang ? 0 : 1)
               .Select(d => new SoundSynthDescriptionDto
                {
                    Id           = d.Id,
                    SoundSynthId = d.SoundSynthId,
                    Html         = d.Html,
                    Markdown     = d.Text,
                    LanguageCode = d.LanguageCode,
                    Language     = d.Language.ReferenceName
                })
               .FirstOrDefaultAsync();

    [HttpPost("{id:int}/description")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> CreateOrUpdateDescriptionAsync(
        int id, [FromBody] SoundSynthDescriptionDto description)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoundSynthDescription current = await context.SoundSynthDescriptions
                                                  .FirstOrDefaultAsync(d => d.SoundSynthId    == id &&
                                                                            d.LanguageCode == description.LanguageCode);

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        string             html     = Markdown.ToHtml(description.Markdown, pipeline);

        if(current is null)
        {
            current = new SoundSynthDescription
            {
                SoundSynthId = id,
                LanguageCode = description.LanguageCode,
                Html         = html,
                Text         = description.Markdown
            };

            await context.SoundSynthDescriptions.AddAsync(current);
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

        SoundSynthDescription description = await context.SoundSynthDescriptions
                                                      .FirstOrDefaultAsync(d => d.SoundSynthId    == id &&
                                                                                d.LanguageCode == languageCode);

        if(description is null) return NotFound();

        // Capture display data BEFORE the cascade, while the SoundSynth + language rows are still
        // available for the system message body.
        string soundSynthName = await context.SoundSynths.AsNoTracking()
                                            .Where(s => s.Id == id)
                                            .Select(s => s.Name)
                                            .FirstOrDefaultAsync();
        string langName = await context.Iso639.AsNoTracking()
                                       .Where(l => l.Id == languageCode)
                                       .Select(l => l.ReferenceName)
                                       .FirstOrDefaultAsync();
        string subkeyLabel = $"({langName ?? languageCode} description)";

        context.SoundSynthDescriptions.Remove(description);

        await context.SaveChangesWithUserAsync(userId);

        await Marechai.Server.Helpers.SuggestionsHelper.MarkStaleForEntitySubkeyAsync(
            context, Marechai.Data.SuggestionEntityType.SoundSynthDescription,
            id, languageCode, soundSynthName ?? $"#{id}", subkeyLabel);

        return Ok();
    }
}