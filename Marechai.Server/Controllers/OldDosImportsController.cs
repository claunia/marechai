/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

/// <summary>
///     Admin-only endpoints driving the old-dos.ru import review queue. The crawler/enrich
///     pipeline lives in the Marechai.OldDos console tool and writes <see cref="OldDosSoftware" />
///     rows; this controller exposes the queue + review API consumed by the Blazor admin UI.
/// </summary>
[ApiController]
[Route("/old-dos")]
[Authorize(Roles = "Admin, UberAdmin")]
public class OldDosImportsController(MarechaiContext context, OldDosPromotionService promotion) : ControllerBase
{
    [HttpGet("pending")]
    [ProducesResponseType(typeof(List<OldDosPendingListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OldDosPendingListItemDto>>> GetPendingAsync(
        [FromQuery] int skip                = 0,
        [FromQuery] int take                = 25,
        [FromQuery] OldDosSoftwareStatus? status = null,
        [FromQuery] string search           = null,
        [FromQuery] bool hasError           = false)
    {
        if(take is <= 0 or > 200) take = 25;
        if(skip < 0) skip = 0;

        IQueryable<OldDosSoftware> q = context.OldDosSoftwares.AsNoTracking();

        if(hasError)
            q = q.Where(s => s.LastError != null && s.LastError != "");
        else if(status.HasValue)
            q = q.Where(s => s.Status == status.Value);
        else
            q = q.Where(s => s.Status == OldDosSoftwareStatus.ReadyForReview ||
                             s.Status == OldDosSoftwareStatus.Skipped);

        if(!string.IsNullOrWhiteSpace(search))
            q = q.Where(s => s.Name.Contains(search));

        // ReadyForReview first (status=4), then Skipped (status=6), within each by CrawledOn desc.
        // Encode that as a computed sort key so EF can SQL-translate it.
        var rows = await q.OrderBy(s => s.Status == OldDosSoftwareStatus.ReadyForReview ? 0 :
                                       s.Status == OldDosSoftwareStatus.Skipped         ? 1 : 2)
                          .ThenByDescending(s => s.CrawledOn)
                          .Skip(skip).Take(take)
                          .Select(s => new OldDosPendingListItemDto
                          {
                              Id                  = s.Id,
                              SourceId            = s.SourceId,
                              SourceUrl           = s.SourceUrl,
                              Status              = s.Status,
                              Name                = s.Name,
                              DeveloperName       = s.DeveloperName,
                              OsName              = s.OsName,
                              RussianCategoryPath = s.RussianCategoryPath,
                              CrawledOn           = s.CrawledOn,
                              VersionCount        = s.Versions.Count,
                              SuggestedGenreIds   = OldDosPromotionService.ParseSuggestedGenreIds(s.SuggestedGenreIdsJson),
                              PromotedSoftwareId  = s.PromotedSoftwareId,
                              LastError           = s.LastError
                          })
                          .ToListAsync();

        return Ok(rows);
    }

    [HttpGet("pending/next")]
    [ProducesResponseType(typeof(OldDosPendingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<OldDosPendingDetailDto>> GetNextAsync([FromQuery] long afterId = 0)
    {
        OldDosSoftware next = await context.OldDosSoftwares
                                           .Include(s => s.Versions)
                                           .Where(s => s.Status == OldDosSoftwareStatus.ReadyForReview &&
                                                       s.Id != afterId)
                                           .OrderBy(s => s.Id)
                                           .FirstOrDefaultAsync();

        next ??= await context.OldDosSoftwares
                              .Include(s => s.Versions)
                              .Where(s => s.Status == OldDosSoftwareStatus.Skipped && s.Id != afterId)
                              .OrderBy(s => s.ReviewedOn ?? s.CrawledOn)
                              .FirstOrDefaultAsync();

        if(next == null) return NoContent();
        return Ok(Map(next));
    }

    [HttpGet("pending/{id:long}")]
    [ProducesResponseType(typeof(OldDosPendingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OldDosPendingDetailDto>> GetByIdAsync(long id)
    {
        OldDosSoftware row = await context.OldDosSoftwares
                                          .Include(s => s.Versions)
                                          .FirstOrDefaultAsync(s => s.Id == id);
        if(row == null) return NotFound();
        return Ok(Map(row));
    }

    [HttpGet("pending/{id:long}/name-matches")]
    [ProducesResponseType(typeof(OldDosNameMatchCandidatesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OldDosNameMatchCandidatesDto>> GetNameMatchesAsync(
        long id, [FromQuery] string name = null)
    {
        // If admin already typed an override name, score against that; otherwise use the staged name.
        string effective = name;
        if(string.IsNullOrWhiteSpace(effective))
        {
            string staged = await context.OldDosSoftwares.Where(s => s.Id == id)
                                                          .Select(s => s.Name)
                                                          .FirstOrDefaultAsync();
            if(staged == null) return NotFound();
            effective = staged;
        }

        return Ok(await promotion.FindNameMatchesAsync(effective));
    }

    [HttpPost("pending/{id:long}/accept")]
    [ProducesResponseType(typeof(AcceptOldDosImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AcceptOldDosImportResultDto>> AcceptAsync(long id, [FromBody] AcceptOldDosImportDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        AcceptOldDosImportResultDto result = await promotion.AcceptAsync(id, dto, userId);
        if(!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("pending/{id:long}/skip")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SkipAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool ok = await promotion.SkipAsync(id, userId);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("pending/{id:long}/discard")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DiscardAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool ok = await promotion.DiscardAsync(id, userId);
        return ok ? NoContent() : NotFound();
    }

    static OldDosPendingDetailDto Map(OldDosSoftware s) => new()
    {
        Id                        = s.Id,
        SourceId                  = s.SourceId,
        SourceUrl                 = s.SourceUrl,
        Status                    = s.Status,
        Name                      = s.Name,
        DeveloperName             = s.DeveloperName,
        OsName                    = s.OsName,
        RussianCategoryPath       = s.RussianCategoryPath,
        RussianDescription        = s.RussianDescription,
        EnglishDescriptionLiteral = s.EnglishDescriptionLiteral,
        EnglishDescriptionMuseum  = s.EnglishDescriptionMuseum,
        SuggestedGenreIds         = OldDosPromotionService.ParseSuggestedGenreIds(s.SuggestedGenreIdsJson),
        Versions = s.Versions.OrderBy(v => v.Id).Select(v => new OldDosVersionDto
        {
            Id                   = v.Id,
            VersionString        = v.VersionString,
            ReleaseDate          = v.ReleaseDate,
            ReleaseDatePrecision = v.ReleaseDatePrecision,
            OsHint               = v.OsHint,
            DownloadUrl          = v.DownloadUrl,
            FileName             = v.FileName,
            Notes                = v.Notes,
            IsEnabledByDefault   = v.IsEnabledByDefault
        }).ToList()
    };
}
