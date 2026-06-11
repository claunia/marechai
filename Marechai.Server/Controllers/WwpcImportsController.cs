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
///     Admin-only endpoints driving the WinWorldPC import review queue. The crawler/enrich
///     pipeline lives in the <c>Marechai.WinWorld</c> console tool and writes
///     <see cref="WwpcSoftware" /> rows; this controller exposes the queue + review API
///     consumed by the Blazor admin UI.
/// </summary>
[ApiController]
[Route("/wwpc")]
[Authorize(Roles = "Admin, UberAdmin")]
public class WwpcImportsController(MarechaiContext context, WwpcPromotionService promotion) : ControllerBase
{
    [HttpGet("pending")]
    [ProducesResponseType(typeof(List<WwpcPendingListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WwpcPendingListItemDto>>> GetPendingAsync(
        [FromQuery] int skip                 = 0,
        [FromQuery] int take                 = 25,
        [FromQuery] WwpcSoftwareStatus? status = null,
        [FromQuery] WwpcProductType? productType = null,
        [FromQuery] string search            = null,
        [FromQuery] bool hasError            = false,
        [FromQuery] string sortBy            = null,
        [FromQuery] bool sortDescending      = false)
    {
        if(take is <= 0 or > 500) take = 25;
        if(skip < 0) skip = 0;

        IQueryable<WwpcSoftware> q = BuildFilteredQuery(status, productType, search, hasError);

        IOrderedQueryable<WwpcSoftware> ordered = (sortBy?.ToLowerInvariant()) switch
        {
            "name"        => sortDescending ? q.OrderByDescending(s => s.Name)        : q.OrderBy(s => s.Name),
            "vendor"      => sortDescending ? q.OrderByDescending(s => s.VendorName)  : q.OrderBy(s => s.VendorName),
            "vendorname"  => sortDescending ? q.OrderByDescending(s => s.VendorName)  : q.OrderBy(s => s.VendorName),
            "producttype" => sortDescending ? q.OrderByDescending(s => s.ProductType) : q.OrderBy(s => s.ProductType),
            "versioncount" => sortDescending ? q.OrderByDescending(s => s.Versions.Count) : q.OrderBy(s => s.Versions.Count),
            "versions"    => sortDescending ? q.OrderByDescending(s => s.Versions.Count) : q.OrderBy(s => s.Versions.Count),
            "screenshotcount" => sortDescending ? q.OrderByDescending(s => s.Screenshots.Count) : q.OrderBy(s => s.Screenshots.Count),
            "screenshots" => sortDescending ? q.OrderByDescending(s => s.Screenshots.Count) : q.OrderBy(s => s.Screenshots.Count),
            "status"      => sortDescending ? q.OrderByDescending(s => s.Status)      : q.OrderBy(s => s.Status),
            "crawledon"   => sortDescending ? q.OrderByDescending(s => s.CrawledOn)   : q.OrderBy(s => s.CrawledOn),
            "crawled"     => sortDescending ? q.OrderByDescending(s => s.CrawledOn)   : q.OrderBy(s => s.CrawledOn),
            _ => q.OrderBy(s => s.Status == WwpcSoftwareStatus.ReadyForReview ? 0 :
                                s.Status == WwpcSoftwareStatus.Skipped        ? 1 : 2)
                  .ThenByDescending(s => s.CrawledOn)
        };

        List<WwpcPendingListItemDto> rows = await ordered.Skip(skip).Take(take)
                                                          .Select(s => new WwpcPendingListItemDto
                                                          {
                                                              Id                       = s.Id,
                                                              SourceUrl                = s.SourceUrl,
                                                              Slug                     = s.Slug,
                                                              Status                   = s.Status,
                                                              ProductType              = s.ProductType,
                                                              Name                     = s.Name,
                                                              VendorName               = s.VendorName,
                                                              RawCategoriesCsv         = s.RawCategoriesCsv,
                                                              PlatformsCsv             = s.PlatformsCsv,
                                                              CrawledOn                = s.CrawledOn,
                                                              VersionCount             = s.Versions.Count,
                                                              ScreenshotCount          = s.Screenshots.Count,
                                                              SuggestedVendorCompanyId = s.SuggestedVendorCompanyId,
                                                              SuggestedGenreIds        = WwpcPromotionService.ParseSuggestedGenreIds(s.SuggestedGenreIdsJson),
                                                              PromotedSoftwareId       = s.PromotedSoftwareId,
                                                              LastError                = s.LastError
                                                          })
                                                          .ToListAsync();
        return Ok(rows);
    }

    [HttpGet("pending/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetPendingCountAsync(
        [FromQuery] WwpcSoftwareStatus? status = null,
        [FromQuery] WwpcProductType? productType = null,
        [FromQuery] string search            = null,
        [FromQuery] bool hasError            = false)
    {
        IQueryable<WwpcSoftware> q = BuildFilteredQuery(status, productType, search, hasError);
        int count = await q.CountAsync();
        return Ok(count);
    }

    IQueryable<WwpcSoftware> BuildFilteredQuery(WwpcSoftwareStatus? status, WwpcProductType? productType, string search,
                                                bool hasError)
    {
        IQueryable<WwpcSoftware> q = context.WwpcSoftwares.AsNoTracking();
        if(hasError)
            q = q.Where(s => s.LastError != null && s.LastError != "");
        else if(status.HasValue)
            q = q.Where(s => s.Status == status.Value);
        else
            q = q.Where(s => s.Status == WwpcSoftwareStatus.ReadyForReview ||
                             s.Status == WwpcSoftwareStatus.Skipped);

        if(productType.HasValue) q = q.Where(s => s.ProductType == productType.Value);
        if(!string.IsNullOrWhiteSpace(search)) q = q.Where(s => s.Name.Contains(search));
        return q;
    }

    [HttpGet("pending/next")]
    [ProducesResponseType(typeof(WwpcPendingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<WwpcPendingDetailDto>> GetNextAsync([FromQuery] long afterId = 0)
    {
        WwpcSoftware next = await context.WwpcSoftwares
                                         .Include(s => s.Versions)
                                         .Include(s => s.Screenshots)
                                         .Where(s => s.Status == WwpcSoftwareStatus.ReadyForReview && s.Id != afterId)
                                         .OrderBy(s => s.Id)
                                         .FirstOrDefaultAsync();

        next ??= await context.WwpcSoftwares
                              .Include(s => s.Versions)
                              .Include(s => s.Screenshots)
                              .Where(s => s.Status == WwpcSoftwareStatus.Skipped && s.Id != afterId)
                              .OrderBy(s => s.ReviewedOn ?? s.CrawledOn)
                              .FirstOrDefaultAsync();

        if(next == null) return NoContent();
        return Ok(Map(next));
    }

    [HttpGet("pending/{id:long}")]
    [ProducesResponseType(typeof(WwpcPendingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WwpcPendingDetailDto>> GetByIdAsync(long id)
    {
        WwpcSoftware row = await context.WwpcSoftwares
                                        .Include(s => s.Versions)
                                        .Include(s => s.Screenshots)
                                        .FirstOrDefaultAsync(s => s.Id == id);
        if(row == null) return NotFound();
        return Ok(Map(row));
    }

    [HttpGet("pending/{id:long}/name-matches")]
    [ProducesResponseType(typeof(WwpcNameMatchCandidatesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WwpcNameMatchCandidatesDto>> GetNameMatchesAsync(
        long id, [FromQuery] string name = null)
    {
        string effective = name;
        if(string.IsNullOrWhiteSpace(effective))
        {
            string staged = await context.WwpcSoftwares.Where(s => s.Id == id).Select(s => s.Name).FirstOrDefaultAsync();
            if(staged == null) return NotFound();
            effective = staged;
        }
        return Ok(await promotion.FindNameMatchesAsync(effective));
    }

    [HttpGet("pending/{id:long}/vendor-matches")]
    [ProducesResponseType(typeof(WwpcCompanyMatchCandidatesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WwpcCompanyMatchCandidatesDto>> GetVendorMatchesAsync(
        long id, [FromQuery] string vendor = null)
    {
        string effective = vendor;
        if(string.IsNullOrWhiteSpace(effective))
        {
            string staged = await context.WwpcSoftwares.Where(s => s.Id == id).Select(s => s.VendorName)
                                         .FirstOrDefaultAsync();
            if(staged == null) return NotFound();
            effective = staged;
        }
        return Ok(await promotion.FindCompanyMatchesAsync(effective));
    }

    [HttpPost("pending/{id:long}/accept")]
    [ProducesResponseType(typeof(AcceptWwpcImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AcceptWwpcImportResultDto>> AcceptAsync(long id, [FromBody] AcceptWwpcImportDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        AcceptWwpcImportResultDto result = await promotion.AcceptAsync(id, dto, userId);
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

    [HttpPost("pending/{id:long}/duplicate")]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<long>> DuplicateAsync(long id, [FromBody] DuplicateWwpcImportDto dto)
    {
        if(!ModelState.IsValid)
            return Problem(detail: "A name is required.", statusCode: StatusCodes.Status400BadRequest);

        string userId = User.FindFirstValue(ClaimTypes.Sid) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        long? newId = await promotion.DuplicateAsync(id, dto.NewName, userId);
        if(newId == null) return NotFound();
        return Ok(newId.Value);
    }

    static WwpcPendingDetailDto Map(WwpcSoftware s) => new()
    {
        Id                             = s.Id,
        SourceUrl                      = s.SourceUrl,
        Slug                           = s.Slug,
        Status                         = s.Status,
        ProductType                    = s.ProductType,
        Name                           = s.Name,
        VendorName                     = s.VendorName,
        VendorUrl                      = s.VendorUrl,
        RawCategoriesCsv               = s.RawCategoriesCsv,
        PlatformsCsv                   = s.PlatformsCsv,
        ReleaseDateText                = s.ReleaseDateText,
        UserInterface                  = s.UserInterface,
        RawDescription                 = s.RawDescription,
        EnglishDescriptionMuseum       = s.EnglishDescriptionMuseum,
        MuseumDescriptionPromptVersion = s.MuseumDescriptionPromptVersion,
        SuggestedVendorCompanyId       = s.SuggestedVendorCompanyId,
        SuggestedGenreIds              = WwpcPromotionService.ParseSuggestedGenreIds(s.SuggestedGenreIdsJson),
        CrawledOn                      = s.CrawledOn,
        PromotedSoftwareId             = s.PromotedSoftwareId,
        Versions = s.Versions.OrderBy(v => v.Id).Select(v => new WwpcVersionDto
        {
            Id                        = v.Id,
            MajorRelease              = v.MajorRelease,
            MajorReleaseUrl           = v.MajorReleaseUrl,
            VersionString             = v.VersionString,
            Language                  = v.Language,
            Architecture              = v.Architecture,
            MediaKind                 = v.MediaKind,
            SizeText                  = v.SizeText,
            DownloadUrl               = v.DownloadUrl,
            IsEnabledByDefault        = v.IsEnabledByDefault,
            PromotedSoftwareVersionId = v.PromotedSoftwareVersionId
        }).ToList(),
        Screenshots = s.Screenshots.OrderBy(x => x.Id).Select(x => new WwpcScreenshotDto
        {
            Id                           = x.Id,
            MajorRelease                 = x.MajorRelease,
            SourceUrl                    = x.SourceUrl,
            ImageUrl                     = x.ImageUrl,
            Caption                      = x.Caption,
            SuggestedSoftwarePlatformId  = x.SuggestedSoftwarePlatformId,
            IsEnabledByDefault           = x.IsEnabledByDefault,
            PromotedSoftwareScreenshotId = x.PromotedSoftwareScreenshotId
        }).ToList()
    };
}
