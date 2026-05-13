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
using Microsoft.Extensions.Caching.Memory;

namespace Marechai.Server.Controllers;

[Route("/software/attributes")]
[ApiController]
public class SoftwareAttributesController(MarechaiContext context, IMemoryCache cache) : ControllerBase
{
    const string DEFAULT_SEPARATOR = ", ";

    // Database column limits — kept in sync with SoftwareAttribute model.
    const int MAX_CATEGORY_LENGTH = 64;
    const int MAX_KEY_LENGTH      = 128;
    const int MAX_VALUE_LENGTH    = 512;

    [HttpGet]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SoftwareAttributePageDto>> GetPagedAsync([FromQuery] ulong? softwareId = null,
        [FromQuery] ulong? releaseId = null,
        [FromQuery] string category  = null,
        [FromQuery] string key       = null,
        [FromQuery] int page         = 1,
        [FromQuery] int pageSize     = 25)
    {
        if(page     < 1)   page     = 1;
        if(pageSize < 1)   pageSize = 25;
        if(pageSize > 200) pageSize = 200;

        IQueryable<SoftwareAttribute> query = context.SoftwareAttributes;

        if(releaseId.HasValue) query = query.Where(a => a.SoftwareReleaseId == releaseId.Value);

        if(softwareId.HasValue)
        {
            ulong sid = softwareId.Value;

            query = query.Where(a => a.SoftwareRelease.SoftwareId == sid ||
                                     (a.SoftwareRelease.SoftwareVersion != null &&
                                      a.SoftwareRelease.SoftwareVersion.SoftwareId == sid));
        }

        if(!string.IsNullOrWhiteSpace(category)) query = query.Where(a => a.Category == category);
        if(!string.IsNullOrWhiteSpace(key))      query = query.Where(a => a.Key == key);

        int totalCount = await query.CountAsync();

        List<SoftwareAttributeDto> items = await query
                                                .OrderBy(a => a.SoftwareRelease.Software != null
                                                                  ? a.SoftwareRelease.Software.Name
                                                                  : a.SoftwareRelease.SoftwareVersion != null
                                                                      ? a.SoftwareRelease.SoftwareVersion.Software.Name
                                                                      : a.SoftwareRelease.Title)
                                                .ThenBy(a => a.SoftwareRelease.Title)
                                                .ThenBy(a => a.SoftwareRelease.Platform.Name)
                                                .ThenBy(a => a.Category)
                                                .ThenBy(a => a.Key)
                                                .ThenBy(a => a.Value)
                                                .Skip((page - 1) * pageSize)
                                                .Take(pageSize)
                                                .Select(a => new SoftwareAttributeDto
                                                 {
                                                     Id                = a.Id,
                                                     SoftwareReleaseId = a.SoftwareReleaseId,
                                                     Category          = a.Category,
                                                     Key               = a.Key,
                                                     Value             = a.Value,
                                                     PlatformName      = a.SoftwareRelease.Platform.Name,
                                                     RegionNames =
                                                         string.Join(", ",
                                                             a.SoftwareRelease.Regions.Select(r => r.UnM49.Name)),
                                                     SoftwareName = a.SoftwareRelease.Software != null
                                                         ? a.SoftwareRelease.Software.Name
                                                         : a.SoftwareRelease.SoftwareVersion != null
                                                             ? a.SoftwareRelease.SoftwareVersion.Software.Name
                                                             : null,
                                                     SoftwareReleaseTitle = a.SoftwareRelease.Title
                                                 })
                                                .ToListAsync();

        return Ok(new SoftwareAttributePageDto
        {
            Items      = items,
            TotalCount = totalCount
        });
    }

    [HttpGet("distinct-categories")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<string>> GetDistinctCategoriesAsync() => context.SoftwareAttributes
                                                                     .Select(a => a.Category)
                                                                     .Distinct()
                                                                     .OrderBy(c => c)
                                                                     .ToListAsync();

    [HttpGet("distinct-keys")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<string>> GetDistinctKeysAsync([FromQuery] string category = null)
    {
        IQueryable<SoftwareAttribute> query = context.SoftwareAttributes;

        if(!string.IsNullOrWhiteSpace(category)) query = query.Where(a => a.Category == category);

        return query.Select(a => a.Key).Distinct().OrderBy(k => k).ToListAsync();
    }

    /// <summary>
    ///     Returns the DISTINCT non-empty <c>Value</c> strings recorded across all software
    ///     attributes, optionally filtered by category and/or key. Used by the public
    ///     collaborative suggestions dialogs (specs / ratings) which gate the value picker
    ///     to STRICT mode over this corpus, so the user can only suggest values that
    ///     already exist. The optional <paramref name="key" /> filter narrows the corpus to
    ///     values previously paired with that key (since certain values only make sense
    ///     under certain keys, e.g. "1 MB" under "RAM" but not under "ESRB Rating").
    /// </summary>
    [HttpGet("distinct-values")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<string>> GetDistinctValuesAsync([FromQuery] string category = null,
                                                     [FromQuery] string key      = null)
    {
        IQueryable<SoftwareAttribute> query = context.SoftwareAttributes
                                                     .Where(a => !string.IsNullOrEmpty(a.Value));

        if(!string.IsNullOrWhiteSpace(category)) query = query.Where(a => a.Category == category);
        if(!string.IsNullOrWhiteSpace(key))      query = query.Where(a => a.Key      == key);

        return query.Select(a => a.Value).Distinct().OrderBy(v => v).ToListAsync();
    }

    [HttpGet("lookup-releases")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<List<SoftwareReleaseLookupDto>> LookupReleasesAsync([FromQuery] ulong softwareId)
    {
        IQueryable<SoftwareRelease> query = context.SoftwareReleases.Where(r => r.SoftwareId == softwareId ||
            (r.SoftwareVersion != null && r.SoftwareVersion.SoftwareId == softwareId));

        return query.OrderBy(r => r.Platform != null ? r.Platform.Name : null)
                    .ThenBy(r => r.Title)
                    .Select(r => new SoftwareReleaseLookupDto
                     {
                         Id           = r.Id,
                         Title        = r.Title,
                         PlatformName = r.Platform != null ? r.Platform.Name : null,
                         SoftwareName = r.Software != null
                                            ? r.Software.Name
                                            : r.SoftwareVersion != null
                                                ? r.SoftwareVersion.Software.Name
                                                : null
                     })
                    .ToListAsync();
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoftwareAttributeDto>> GetByIdAsync(long id)
    {
        SoftwareAttributeDto dto = await context.SoftwareAttributes.Where(a => a.Id == id)
                                                .Select(a => new SoftwareAttributeDto
                                                 {
                                                     Id                = a.Id,
                                                     SoftwareReleaseId = a.SoftwareReleaseId,
                                                     Category          = a.Category,
                                                     Key               = a.Key,
                                                     Value             = a.Value,
                                                     PlatformName      = a.SoftwareRelease.Platform.Name,
                                                     RegionNames =
                                                         string.Join(", ",
                                                             a.SoftwareRelease.Regions.Select(r => r.UnM49.Name)),
                                                     SoftwareName = a.SoftwareRelease.Software != null
                                                         ? a.SoftwareRelease.Software.Name
                                                         : a.SoftwareRelease.SoftwareVersion != null
                                                             ? a.SoftwareRelease.SoftwareVersion.Software.Name
                                                             : null,
                                                     SoftwareReleaseTitle = a.SoftwareRelease.Title
                                                 })
                                                .FirstOrDefaultAsync();

        if(dto is null) return NotFound();

        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<long>> CreateAsync([FromBody] CreateSoftwareAttributeRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        ActionResult validation = ValidateFields(request.Category, request.Key, request.Value);

        if(validation is not null) return validation;

        bool releaseExists = await context.SoftwareReleases.AnyAsync(r => r.Id == request.SoftwareReleaseId);

        if(!releaseExists) return Problem("Software release not found.", statusCode: StatusCodes.Status404NotFound);

        string category = request.Category.Trim();
        string key      = request.Key.Trim();
        string value    = request.Value.Trim();

        bool duplicate = await context.SoftwareAttributes
                                      .AnyAsync(a => a.SoftwareReleaseId == request.SoftwareReleaseId &&
                                                     a.Category          == category                 &&
                                                     a.Key               == key                      &&
                                                     a.Value             == value);

        if(duplicate)
            return Problem("An attribute with the same release, category, key and value already exists.",
                           statusCode: StatusCodes.Status409Conflict);

        var model = new SoftwareAttribute
        {
            SoftwareReleaseId = request.SoftwareReleaseId,
            Category          = category,
            Key               = key,
            Value             = value
        };

        context.SoftwareAttributes.Add(model);
        await context.SaveChangesWithUserAsync(userId);
        InvalidateSpecsCache();

        return model.Id;
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> UpdateAsync(long id, [FromBody] UpdateSoftwareAttributeRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        ActionResult validation = ValidateFields(request.Category, request.Key, request.Value);

        if(validation is not null) return validation;

        SoftwareAttribute model = await context.SoftwareAttributes.FindAsync(id);

        if(model is null) return NotFound();

        string category = request.Category.Trim();
        string key      = request.Key.Trim();
        string value    = request.Value.Trim();

        bool duplicate = await context.SoftwareAttributes
                                      .AnyAsync(a => a.Id != id &&
                                                     a.SoftwareReleaseId == model.SoftwareReleaseId &&
                                                     a.Category          == category                &&
                                                     a.Key               == key                     &&
                                                     a.Value             == value);

        if(duplicate)
            return Problem("Another attribute with the same release, category, key and value already exists.",
                           statusCode: StatusCodes.Status409Conflict);

        model.Category = category;
        model.Key      = key;
        model.Value    = value;

        await context.SaveChangesWithUserAsync(userId);
        InvalidateSpecsCache();

        return Ok();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareAttribute model = await context.SoftwareAttributes.FindAsync(id);

        if(model is null) return NotFound();

        context.SoftwareAttributes.Remove(model);
        await context.SaveChangesWithUserAsync(userId);
        InvalidateSpecsCache();

        return Ok();
    }

    [HttpPost("{id:long}/split-preview")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SplitSoftwareAttributeResultDto>> PreviewSplitAsync(
        long id, [FromBody] SplitSoftwareAttributeRequest request)
    {
        SoftwareAttribute model = await context.SoftwareAttributes.FindAsync(id);

        if(model is null) return NotFound();

        string separator = string.IsNullOrEmpty(request?.Separator) ? DEFAULT_SEPARATOR : request.Separator;

        List<string> fragments = SplitValue(model.Value, separator);

        if(fragments.Count < 2)
            return Problem("Splitting requires at least two non-empty fragments.",
                           statusCode: StatusCodes.Status400BadRequest);

        List<string> existing = await context.SoftwareAttributes
                                             .Where(a => a.Id != id &&
                                                         a.SoftwareReleaseId == model.SoftwareReleaseId &&
                                                         a.Category          == model.Category          &&
                                                         a.Key               == model.Key)
                                             .Select(a => a.Value)
                                             .ToListAsync();

        var existingSet = new HashSet<string>(existing);
        var seenInBatch = new HashSet<string>();
        var preview     = new List<SplitFragmentResultDto>(fragments.Count);

        foreach(string fragment in fragments)
        {
            bool already = existingSet.Contains(fragment) || !seenInBatch.Add(fragment);

            preview.Add(new SplitFragmentResultDto
            {
                Value          = fragment,
                AlreadyExisted = already,
                CreatedId      = null
            });
        }

        return Ok(new SplitSoftwareAttributeResultDto
        {
            AttributeId       = model.Id,
            Deleted           = false,
            Category          = model.Category,
            Key               = model.Key,
            SoftwareReleaseId = model.SoftwareReleaseId,
            Fragments         = preview
        });
    }

    [HttpPost("{id:long}/split")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SplitSoftwareAttributeResultDto>> SplitAsync(
        long id, [FromBody] SplitSoftwareAttributeRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareAttribute model = await context.SoftwareAttributes.FindAsync(id);

        if(model is null) return NotFound();

        string separator = string.IsNullOrEmpty(request?.Separator) ? DEFAULT_SEPARATOR : request.Separator;

        List<string> fragments = SplitValue(model.Value, separator);

        if(fragments.Count < 2)
            return Problem("Splitting requires at least two non-empty fragments.",
                           statusCode: StatusCodes.Status400BadRequest);

        List<string> existing = await context.SoftwareAttributes
                                             .Where(a => a.Id != id &&
                                                         a.SoftwareReleaseId == model.SoftwareReleaseId &&
                                                         a.Category          == model.Category          &&
                                                         a.Key               == model.Key)
                                             .Select(a => a.Value)
                                             .ToListAsync();

        var existingSet = new HashSet<string>(existing);
        var seenInBatch = new HashSet<string>();
        var results     = new List<SplitFragmentResultDto>(fragments.Count);
        var toInsert    = new List<SoftwareAttribute>();

        ulong releaseId = model.SoftwareReleaseId;
        string category = model.Category;
        string key      = model.Key;

        foreach(string fragment in fragments)
        {
            if(existingSet.Contains(fragment) || !seenInBatch.Add(fragment))
            {
                results.Add(new SplitFragmentResultDto
                {
                    Value          = fragment,
                    AlreadyExisted = true,
                    CreatedId      = null
                });

                continue;
            }

            var newAttr = new SoftwareAttribute
            {
                SoftwareReleaseId = releaseId,
                Category          = category,
                Key               = key,
                Value             = fragment
            };

            toInsert.Add(newAttr);

            results.Add(new SplitFragmentResultDto
            {
                Value          = fragment,
                AlreadyExisted = false,
                CreatedId      = null
            });
        }

        context.SoftwareAttributes.Remove(model);

        if(toInsert.Count > 0) await context.SoftwareAttributes.AddRangeAsync(toInsert);

        await context.SaveChangesWithUserAsync(userId);
        InvalidateSpecsCache();

        // Backfill the generated IDs into the result DTOs (preserving order).
        int insertIndex = 0;

        foreach(SplitFragmentResultDto result in results)
        {
            if(result.AlreadyExisted) continue;
            if(insertIndex >= toInsert.Count) break;
            result.CreatedId = toInsert[insertIndex++].Id;
        }

        return Ok(new SplitSoftwareAttributeResultDto
        {
            AttributeId       = id,
            Deleted           = true,
            Category          = category,
            Key               = key,
            SoftwareReleaseId = releaseId,
            Fragments         = results
        });
    }

    static List<string> SplitValue(string value, string separator)
    {
        if(string.IsNullOrEmpty(value) || string.IsNullOrEmpty(separator)) return new List<string>();

        string[] raw = value.Split(separator, System.StringSplitOptions.None);
        var      result = new List<string>(raw.Length);

        foreach(string fragment in raw)
        {
            string trimmed = fragment.Trim();

            if(!string.IsNullOrWhiteSpace(trimmed)) result.Add(trimmed);
        }

        return result;
    }

    ActionResult ValidateFields(string category, string key, string value)
    {
        if(string.IsNullOrWhiteSpace(category))
            return Problem("Category is required.", statusCode: StatusCodes.Status400BadRequest);

        if(string.IsNullOrWhiteSpace(key))
            return Problem("Key is required.", statusCode: StatusCodes.Status400BadRequest);

        if(string.IsNullOrWhiteSpace(value))
            return Problem("Value cannot be empty.", statusCode: StatusCodes.Status400BadRequest);

        if(category.Length > MAX_CATEGORY_LENGTH)
            return Problem($"Category exceeds {MAX_CATEGORY_LENGTH} characters.",
                           statusCode: StatusCodes.Status400BadRequest);

        if(key.Length > MAX_KEY_LENGTH)
            return Problem($"Key exceeds {MAX_KEY_LENGTH} characters.", statusCode: StatusCodes.Status400BadRequest);

        if(value.Length > MAX_VALUE_LENGTH)
            return Problem($"Value exceeds {MAX_VALUE_LENGTH} characters.",
                           statusCode: StatusCodes.Status400BadRequest);

        return null;
    }

    void InvalidateSpecsCache() => cache.Remove(SoftwareController.SOFTWARE_SPECS_CACHE_KEY);
}
