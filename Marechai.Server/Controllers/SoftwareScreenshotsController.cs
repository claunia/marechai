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
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Helpers;
using Marechai.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marechai.Server.Controllers;

[Route("/software/screenshots")]
[ApiController]
public class SoftwareScreenshotsController(MarechaiContext    context, IConfiguration configuration,
                                           BatchUploadJobStore batchJobs) : ControllerBase
{
    /// <summary>
    ///     Per-uploader cap of in-flight pending screenshot images per Software for the
    ///     collaborative suggestion flow.
    /// </summary>
    const int PendingScreenshotsPerUserPerSoftwareCap = 50;

    static readonly HashSet<string> _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"];

    static readonly HashSet<string> _allowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/tiff", "image/bmp"
    ];

    /// <summary>
    ///     Narrower allow-set for collaborator pending uploads (jpg/png/webp ONLY). The
    ///     existing admin upload endpoint keeps the broader <see cref="_allowedExtensions" />
    ///     set including tiff/bmp.
    /// </summary>
    static readonly HashSet<string> _pendingAllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    static readonly HashSet<string> _pendingAllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    readonly string _assetRootPath = configuration["AssetRootPath"]!;

    [HttpGet("/software/{softwareId}/screenshots")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<Guid>> GetGuidsBySoftwareAsync(ulong softwareId) =>
        context.SoftwareScreenshots
               .Where(s => s.SoftwareId == softwareId)
               .OrderBy(s => s.CreatedOn)
               .ThenBy(s => s.Id)
               .Select(s => s.Id)
               .ToListAsync();

    [HttpGet("/software/{softwareId}/platforms/{platformId}/screenshots")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<Guid>> GetGuidsByPlatformAsync(ulong softwareId, ulong platformId) =>
        context.SoftwareScreenshots
               .Where(s => s.SoftwareId == softwareId && s.SoftwarePlatformId == platformId)
               .OrderBy(s => s.CreatedOn)
               .ThenBy(s => s.Id)
               .Select(s => s.Id)
               .ToListAsync();

    [HttpGet("/software/{softwareId}/versions/{versionId}/screenshots")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<List<Guid>> GetGuidsByVersionAsync(ulong softwareId, ulong versionId) =>
        context.SoftwareScreenshots
               .Where(s => s.SoftwareId == softwareId && s.SoftwareVersionId == versionId)
               .OrderBy(s => s.CreatedOn)
               .ThenBy(s => s.Id)
               .Select(s => s.Id)
               .ToListAsync();

    [HttpGet("{id:Guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoftwareScreenshotDto>> GetAsync(Guid id, [FromQuery] string lang = null)
    {
        string langCode  = LanguageResolver.Resolve(HttpContext, lang);
        bool   isEnglish = string.Equals(langCode, "eng", StringComparison.Ordinal);

        SoftwareScreenshotDto dto = await context.SoftwareScreenshots
                                                  .Where(s => s.Id == id)
                                                  .Select(s => new SoftwareScreenshotDto
                                                   {
                                                       Id                 = s.Id,
                                                       SoftwareId         = s.SoftwareId,
                                                       SoftwareName       = s.Software.Name,
                                                       SoftwarePlatformId = s.SoftwarePlatformId,
                                                       PlatformName       = s.Platform != null ? s.Platform.Name : null,
                                                       SoftwareVersionId  = s.SoftwareVersionId,
                                                       VersionString = s.Version != null ? s.Version.VersionString : null,
                                                       Caption            = s.Caption,
                                                       CanonicalCaption   = s.Caption,
                                                       GroupId            = s.GroupId,
                                                       GroupName          = s.Group != null ? s.Group.Name : null,
                                                       CanonicalGroupName = s.Group != null ? s.Group.Name : null,
                                                       OriginalExtension  = s.OriginalExtension
                                                   })
                                                  .FirstOrDefaultAsync();

        if(dto is null) return NotFound();

        // Caption + group translation lookups are separate queries so EF doesn't emit
        // correlated subqueries against the translations tables inside the projection above.
        // Only run when a non-English language was resolved AND the canonical text is non-null.
        if(!isEnglish && dto.Caption != null)
        {
            string translated = await context.SoftwareScreenshotCaptionTranslations
                                              .Where(t => t.ScreenshotId == id && t.LanguageCode == langCode)
                                              .Select(t => t.Caption)
                                              .FirstOrDefaultAsync();

            if(translated != null) dto.Caption = translated;
        }

        if(!isEnglish && dto.GroupId.HasValue)
        {
            string translatedGroup = await context.SoftwareScreenshotGroupTranslations
                                                  .Where(t => t.GroupId == dto.GroupId.Value &&
                                                              t.LanguageCode == langCode)
                                                  .Select(t => t.Name)
                                                  .FirstOrDefaultAsync();

            if(translatedGroup != null) dto.GroupName = translatedGroup;
        }

        return Ok(dto);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<SoftwareScreenshotDto>> GetAllAsync([FromQuery] string lang = null)
    {
        string langCode  = LanguageResolver.Resolve(HttpContext, lang);
        bool   isEnglish = string.Equals(langCode, "eng", StringComparison.Ordinal);

        List<SoftwareScreenshotDto> list = await context.SoftwareScreenshots
                                                         .Select(s => new SoftwareScreenshotDto
                                                          {
                                                              Id                 = s.Id,
                                                              SoftwareId         = s.SoftwareId,
                                                              SoftwareName       = s.Software.Name,
                                                              SoftwarePlatformId = s.SoftwarePlatformId,
                                                              PlatformName = s.Platform != null
                                                                                 ? s.Platform.Name
                                                                                 : null,
                                                              SoftwareVersionId = s.SoftwareVersionId,
                                                              VersionString = s.Version != null
                                                                                  ? s.Version.VersionString
                                                                                  : null,
                                                              Caption           = s.Caption,
                                                              CanonicalCaption  = s.Caption,
                                                              GroupId           = s.GroupId,
                                                              GroupName = s.Group != null ? s.Group.Name : null,
                                                              CanonicalGroupName = s.Group != null
                                                                                       ? s.Group.Name
                                                                                       : null,
                                                              OriginalExtension = s.OriginalExtension
                                                          })
                                                         .ToListAsync();

        if(isEnglish || list.Count == 0) return list;

        // Backfill non-English captions in a single IN-list query (was: per-row correlated
        // subquery inside the projection above, executed once per screenshot row).
        Guid[] ids = list.Where(d => d.Caption != null).Select(d => d.Id).ToArray();

        if(ids.Length > 0)
        {
            Dictionary<Guid, string> translations =
                await context.SoftwareScreenshotCaptionTranslations
                             .Where(t => ids.Contains(t.ScreenshotId) && t.LanguageCode == langCode)
                             .Select(t => new
                              {
                                  t.ScreenshotId,
                                  t.Caption
                              })
                             .ToDictionaryAsync(x => x.ScreenshotId, x => x.Caption);

            if(translations.Count > 0)
            {
                foreach(SoftwareScreenshotDto dto in list)
                {
                    if(dto.Caption == null) continue;

                    if(translations.TryGetValue(dto.Id, out string translated) && translated != null)
                        dto.Caption = translated;
                }
            }
        }

        // Backfill non-English group names with the same IN-list pattern. Distinct groupId set
        // ensures we don't redundantly fetch the same translation row for groups shared by
        // multiple screenshots.
        int[] groupIds = list.Where(d => d.GroupId.HasValue).Select(d => d.GroupId!.Value).Distinct().ToArray();

        if(groupIds.Length == 0) return list;

        Dictionary<int, string> groupTranslations =
            await context.SoftwareScreenshotGroupTranslations
                         .Where(t => groupIds.Contains(t.GroupId) && t.LanguageCode == langCode)
                         .Select(t => new { t.GroupId, t.Name })
                         .ToDictionaryAsync(x => x.GroupId, x => x.Name);

        if(groupTranslations.Count == 0) return list;

        foreach(SoftwareScreenshotDto dto in list)
        {
            if(!dto.GroupId.HasValue) continue;

            if(groupTranslations.TryGetValue(dto.GroupId.Value, out string translated) && translated != null)
                dto.GroupName = translated;
        }

        return list;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SoftwareScreenshotDto>> UploadAsync(IFormFile            file,
                                                                       [FromForm] ulong     softwareId,
                                                                       [FromForm] ulong?    softwarePlatformId,
                                                                       [FromForm] ulong?    softwareVersionId,
                                                                       [FromForm] string   caption,
                                                                       [FromForm] string   canonicalGroupName = null)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        if(file is null || file.Length == 0)
            return BadRequest("No file provided.");

        if(file.Length > 50 * 1024 * 1024)
            return BadRequest("File exceeds 50 MB limit.");

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        if(!_allowedExtensions.Contains(extension))
            return BadRequest("Unsupported file format. Accepted: JPEG, PNG, WebP, TIFF, BMP.");

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Unsupported content type.");

        // Validate that the referenced software exists
        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == softwareId);

        if(!softwareExists)
            return BadRequest("Referenced software does not exist.");

        if(softwarePlatformId.HasValue)
        {
            bool platformExists = await context.SoftwarePlatforms.AnyAsync(p => p.Id == softwarePlatformId.Value);

            if(!platformExists)
                return BadRequest("Referenced software platform does not exist.");
        }

        if(softwareVersionId.HasValue)
        {
            bool versionExists = await context.SoftwareVersions.AnyAsync(v => v.Id == softwareVersionId.Value &&
                                                                              v.SoftwareId == softwareId);

            if(!versionExists)
                return BadRequest("Referenced software version does not exist or does not belong to this software.");
        }

        // Optional group: resolve-or-create by canonical English name. Mirrors the PromoArt
        // controller's pattern but stays optional (screenshots without a group are valid).
        SoftwareScreenshotGroup group = await ResolveOrCreateGroupAsync(canonicalGroupName, userId);

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var model = new SoftwareScreenshot
        {
            Id                 = Guid.NewGuid(),
            SoftwareId         = softwareId,
            SoftwarePlatformId = softwarePlatformId,
            SoftwareVersionId  = softwareVersionId,
            GroupId            = group?.Id,
            Caption            = caption,
            OriginalExtension  = extension.TrimStart('.')
        };

        // Save original file to disk
        Photos.EnsureCreated(_assetRootPath, false, "software-screenshots");

        string originalsDir = Path.Combine(_assetRootPath, "photos", "software-screenshots", "originals");
        string originalPath = Path.Combine(originalsDir, $"{model.Id}{extension}");

        ms.Position = 0;

        await using(var fs = new FileStream(originalPath, FileMode.CreateNew, FileAccess.Write))
        {
            await ms.CopyToAsync(fs);
        }

        // Fire conversion worker (generates all format/resolution variants)
        string sourceFormat = extension.TrimStart('.');

        _ = Task.Run(() =>
        {
            var photos = new Photos();

            photos.ConversionWorker(_assetRootPath, model.Id, originalPath, sourceFormat, false,
                                    "software-screenshots");
        });

        // Save to database
        await context.SoftwareScreenshots.AddAsync(model);
        await context.SaveChangesWithUserAsync(userId);

        return Ok(new SoftwareScreenshotDto
        {
            Id                 = model.Id,
            SoftwareId         = model.SoftwareId,
            SoftwarePlatformId = model.SoftwarePlatformId,
            SoftwareVersionId  = model.SoftwareVersionId,
            Caption            = model.Caption,
            CanonicalCaption   = model.Caption,
            GroupId            = model.GroupId,
            GroupName          = group?.Name,
            CanonicalGroupName = group?.Name,
            OriginalExtension  = model.OriginalExtension
        });
    }

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] SoftwareScreenshotDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareScreenshot model = await context.SoftwareScreenshots.FindAsync(id);

        if(model is null) return NotFound();

        // Admin / suggestion edit may submit the canonical English caption via
        // DTO.CanonicalCaption (preferred) or fall back to DTO.Caption when the dialog
        // doesn't carry a separate canonical field.
        string newCaption = !string.IsNullOrEmpty(dto.CanonicalCaption) ? dto.CanonicalCaption : dto.Caption;
        string oldCaption = model.Caption;

        // Invalidate cached translations whenever the canonical caption text changes,
        // so the background TranslationWorker re-translates on its next sweep. Comparison
        // is char-exact (Ordinal) — translation output is char-exact too, so any
        // case/whitespace edit warrants re-translation.
        if(!string.Equals(oldCaption ?? string.Empty, newCaption ?? string.Empty, StringComparison.Ordinal))
        {
            await context.SoftwareScreenshotCaptionTranslations
                         .Where(t => t.ScreenshotId == id)
                         .ExecuteDeleteAsync();
        }

        model.Caption            = newCaption;
        model.SoftwarePlatformId = dto.SoftwarePlatformId;
        model.SoftwareVersionId  = dto.SoftwareVersionId;

        // Group resolve-or-create: admin / suggestion edit submits the canonical English group
        // name via dto.CanonicalGroupName. Null/empty CLEARS the group assignment (the FK is
        // optional). Non-null gets resolved against SoftwareScreenshotGroups; missing rows are
        // created on the fly so admins don't need to pre-seed groups before assigning them.
        SoftwareScreenshotGroup group = await ResolveOrCreateGroupAsync(dto.CanonicalGroupName, userId);
        model.GroupId = group?.Id;

        await context.SaveChangesWithUserAsync(userId);

        return Ok();
    }

    [HttpDelete("{id:Guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(userId is null) return Unauthorized();

        SoftwareScreenshot model = await context.SoftwareScreenshots.FindAsync(id);

        if(model is null) return NotFound();

        context.SoftwareScreenshots.Remove(model);
        await context.SaveChangesWithUserAsync(userId);

        // Delete all generated files from disk
        string photosRoot = Path.Combine(_assetRootPath, "photos", "software-screenshots");
        string guidStr    = id.ToString();

        // Delete original
        DeleteFilesByPattern(Path.Combine(photosRoot, "originals"), $"{guidStr}.*");

        // Delete all format/resolution variants (full + thumbnails)
        string[] formats     = ["jpeg", "webp", "avif"];
        string[] resolutions = ["4k"];

        foreach(string format in formats)
        {
            string ext = format switch
            {
                "jpeg" => ".jpg",
                "webp" => ".webp",
                "avif" => ".avif",
                _      => $".{format}"
            };

            foreach(string res in resolutions)
            {
                string fullPath  = Path.Combine(photosRoot, format, res, $"{guidStr}{ext}");
                string thumbPath = Path.Combine(photosRoot, "thumbs", format, res, $"{guidStr}{ext}");

                if(System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);

                if(System.IO.File.Exists(thumbPath))
                    System.IO.File.Delete(thumbPath);
            }
        }

        return Ok();
    }

    static void DeleteFilesByPattern(string directory, string pattern)
    {
        if(!System.IO.Directory.Exists(directory)) return;

        foreach(string file in System.IO.Directory.GetFiles(directory, pattern))
            System.IO.File.Delete(file);
    }

    /// <summary>
    ///     Resolve a free-form English group name to an existing
    ///     <see cref="SoftwareScreenshotGroup" /> row, creating one on the fly if no
    ///     case-sensitive match exists. Returns <c>null</c> when the input is null / empty so
    ///     the caller can clear the optional FK. Trims whitespace and rejects names longer than
    ///     256 characters via <see cref="System.ArgumentException" /> at the model boundary —
    ///     that's defensive only; controllers should validate length before calling.
    /// </summary>
    /// <remarks>
    ///     Unlike the PromoArt controller's helper (where groups are mandatory), this returns\n    ///     <c>null</c> for empty input so screenshots without a group keep <c>GroupId == null</c>.\n    ///     The unique index on <c>SoftwareScreenshotGroups.Name</c> guarantees no duplicate row\n    ///     is created under concurrent uploads; the worst case is a transient INSERT failure that\n    ///     the caller can retry once.\n    /// </remarks>
    async Task<SoftwareScreenshotGroup> ResolveOrCreateGroupAsync(string canonicalName, string userId)
    {
        string trimmed = canonicalName?.Trim();
        if(string.IsNullOrWhiteSpace(trimmed)) return null;

        if(trimmed.Length > 256) trimmed = trimmed[..256];

        SoftwareScreenshotGroup group =
            await context.SoftwareScreenshotGroups.FirstOrDefaultAsync(g => g.Name == trimmed);

        if(group is not null) return group;

        group = new SoftwareScreenshotGroup { Name = trimmed };
        await context.SoftwareScreenshotGroups.AddAsync(group);
        await context.SaveChangesWithUserAsync(userId);

        return group;
    }

    /// <summary>
    ///     Autocomplete data source for the admin uploader + suggestion-dialog group pickers.
    ///     Returns up to 25 groups whose canonical English name contains the <paramref name="search" />
    ///     substring (case-insensitive), ordered alphabetically. Localized
    ///     <see cref="SoftwareScreenshotGroupDto.Name" /> is filled with English fallback when no
    ///     translation row exists for the requested language; <see cref="SoftwareScreenshotGroupDto.CanonicalName" />
    ///     is always the English value so the edit-path submits back the canonical row identifier.
    /// </summary>
    [HttpGet("groups")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<SoftwareScreenshotGroupDto>> GetGroupsAsync([FromQuery] string search = null,
                                                                       [FromQuery] string lang   = null)
    {
        string langCode  = LanguageResolver.Resolve(HttpContext, lang);
        bool   isEnglish = string.Equals(langCode, "eng", StringComparison.Ordinal);

        IQueryable<SoftwareScreenshotGroup> query = context.SoftwareScreenshotGroups.AsNoTracking();

        if(!string.IsNullOrWhiteSpace(search))
        {
            string trimmed = search.Trim();
            query = query.Where(g => EF.Functions.Like(g.Name, $"%{trimmed}%"));
        }

        List<SoftwareScreenshotGroupDto> groups = isEnglish
                                                      ? await query.Select(g => new SoftwareScreenshotGroupDto
                                                                    {
                                                                        Id            = g.Id,
                                                                        Name          = g.Name,
                                                                        CanonicalName = g.Name
                                                                    })
                                                                   .Take(25)
                                                                   .ToListAsync()
                                                      : await query.Select(g => new SoftwareScreenshotGroupDto
                                                                    {
                                                                        Id   = g.Id,
                                                                        Name = context.SoftwareScreenshotGroupTranslations
                                                                                      .Where(t => t.GroupId      == g.Id &&
                                                                                                  t.LanguageCode == langCode)
                                                                                      .Select(t => t.Name)
                                                                                      .FirstOrDefault() ?? g.Name,
                                                                        CanonicalName = g.Name
                                                                    })
                                                                   .Take(25)
                                                                   .ToListAsync();

        // Sort in-memory by the localized Name so the displayed list is alphabetical in the
        // requested language (sorting in EF would force the join into ORDER BY and complicate
        // SQL — the result set is bounded to 25 rows).
        groups.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));

        return groups;
    }

    // ─────────────── Collaborative pending-image endpoints ───────────────
    // These endpoints back the SoftwareScreenshotsSuggestionDialog. Each upload writes a
    // file + sidecar to disk under photos/software-screenshots/pending/ and the
    // collaborator references the returned guid when calling POST /suggestions. Promotion
    // to originals/ happens in SoftwareScreenshotSuggestionApplier.ApplyAsync once an admin
    // accepts the per-image accept-key (screenshot.<guid>).

    /// <summary>
    ///     Stage a single pending screenshot image for a brand-new collaborative suggestion.
    ///     The uploader keeps each pending file on the server (sidecar tracks ownership +
    ///     parent <c>softwareId</c>) until they call <c>POST /suggestions</c> referencing the
    ///     returned <c>guid</c>. Per-uploader cap of 50 in-flight pending images per
    ///     Software.
    /// </summary>
    [HttpPost("pending")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PendingImageUploadDto>> UploadPendingAsync(IFormFile         file,
                                                                              [FromQuery] ulong softwareId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        if(file is null || file.Length == 0) return BadRequest("No file provided.");

        if(file.Length > 50 * 1024 * 1024) return BadRequest("File exceeds 50 MB limit.");

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;

        if(!_pendingAllowedExtensions.Contains(extension))
            return BadRequest("Unsupported file format. Accepted: JPEG, PNG, WebP.");

        if(!string.IsNullOrEmpty(file.ContentType) &&
           !_pendingAllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Unsupported content type.");

        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == softwareId);

        if(!softwareExists) return NotFound("Software not found.");

        long parentEntityId = (long)softwareId;

        int currentCount = PendingImageStore.CountByUploaderForParentEntity(_assetRootPath, "software-screenshots",
            userId, (byte)SuggestionEntityType.SoftwareScreenshot, parentEntityId);

        if(currentCount >= PendingScreenshotsPerUserPerSoftwareCap)
            return Conflict($"You already have {currentCount} pending screenshot images for this software. Maximum " +
                            $"is {PendingScreenshotsPerUserPerSoftwareCap} per software. Submit or remove some first.");

        Guid guid;

        await using(Stream stream = file.OpenReadStream())
        {
            // EntityId stays 0 because the suggestion row that will reference these images
            // doesn't exist yet. ParentEntityId carries the softwareId so the per-uploader
            // cap and cleanup-by-parent helpers can scope correctly.
            guid = await PendingImageStore.StoreAsync(_assetRootPath, "software-screenshots", extension,
                (byte)SuggestionEntityType.SoftwareScreenshot, entityId: 0L, userId, file.ContentType, stream,
                parentEntityId: parentEntityId);
        }

        return Ok(new PendingImageUploadDto
        {
            Guid      = guid,
            Extension = extension.TrimStart('.')
        });
    }

    /// <summary>
    ///     Delete a pending screenshot image before it has been submitted as part of a
    ///     suggestion. Only the original uploader (or an admin) may delete.
    /// </summary>
    [HttpDelete("pending/{guid:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePendingAsync(Guid guid)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        PendingImageStore.PendingMetadata meta =
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-screenshots", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.SoftwareScreenshot) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");

        if(!PendingImageStore.CanAccess(meta, userId, isAdmin)) return Forbid();

        PendingImageStore.Delete(_assetRootPath, "software-screenshots", guid);

        return NoContent();
    }

    /// <summary>
    ///     Stream the binary contents of a pending screenshot image. Used by the dialog
    ///     thumbnail preview AND by the admin SuggestionDiffPanel preview. Auth-gated: only
    ///     the uploader and admins can read.
    /// </summary>
    [HttpGet("pending/{guid:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPendingAsync(Guid guid)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        PendingImageStore.PendingMetadata meta =
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-screenshots", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.SoftwareScreenshot) return NotFound();

        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("UberAdmin");

        if(!PendingImageStore.CanAccess(meta, userId, isAdmin)) return Forbid();

        string path = await PendingImageStore.GetImagePathAsync(_assetRootPath, "software-screenshots", guid);

        if(path is null || !System.IO.File.Exists(path)) return NotFound();

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return File(stream, meta.ContentType ?? "application/octet-stream");
    }

    // ────────────────────────────── Admin batch upload ──────────────────────────────

    /// <summary>Maximum images an admin may stage in a single batch (matches dialog UI).</summary>
    const int AdminBatchMaxImages = 50;

    /// <summary>
    ///     Allowed extensions accepted by the admin batch-upload staging endpoint. Wider
    ///     than the legacy <c>/upload</c> set (adds AVIF/JXL) and the collaborator-suggestion
    ///     set (JPEG/PNG/WebP only).
    /// </summary>
    static readonly HashSet<string> _adminBatchAllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".avif", ".jxl", ".bmp", ".tif", ".tiff"
    };

    /// <summary>
    ///     ImageMagick canonical format names (as returned by <c>identify -format %m</c>)
    ///     trusted for admin batch uploads. Checked AFTER the file is written to disk so
    ///     an attacker can't bypass it by lying about the extension or MIME type.
    /// </summary>
    static readonly HashSet<string> _adminBatchAllowedMagickFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        "JPEG", "PNG", "WEBP", "AVIF", "JXL", "BMP", "TIFF"
    };

    /// <summary>
    ///     Stage a single image as part of an admin software-screenshot batch upload. The
    ///     server writes the file to the pending folder, content-sniffs it with ImageMagick
    ///     (rejecting on mismatch regardless of extension/MIME), generates a 256x256
    ///     thumbnail returned inline as a JPEG data URL, and stores both file + sidecar for
    ///     later commit.
    /// </summary>
    [HttpPost("admin/pending")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<AdminPendingSoftwareScreenshotUploadDto>> UploadAdminBatchPendingAsync(
        IFormFile file, [FromQuery] int softwareId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        if(file is null || file.Length == 0) return BadRequest("No file provided.");
        if(file.Length > 50 * 1024 * 1024) return BadRequest("File exceeds 50 MB limit.");

        string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        if(!_adminBatchAllowedExtensions.Contains(extension))
            return BadRequest("Unsupported file format. Accepted: JPEG, PNG, WebP, AVIF, JXL, BMP, TIFF.");

        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == (ulong)softwareId);
        if(!softwareExists) return NotFound("Software not found.");

        int currentCount = PendingImageStore.CountAdminStagedByUploader(_assetRootPath, "software-screenshots", userId,
            (byte)SuggestionEntityType.SoftwareScreenshot);

        if(currentCount >= AdminBatchMaxImages)
            return Conflict($"You already have {currentCount} pending images staged. Maximum is " +
                            $"{AdminBatchMaxImages}. Commit or remove some first.");

        // Persist FIRST (with a temporary placeholder for dimensions), then content-sniff
        // and either re-write the sidecar with real dimensions or reject + delete the file.
        Guid guid;
        await using(Stream stream = file.OpenReadStream())
        {
            guid = await PendingImageStore.StoreAsync(_assetRootPath, "software-screenshots", extension,
                (byte)SuggestionEntityType.SoftwareScreenshot, entityId: 0L, userId, file.ContentType, stream,
                parentEntityId: softwareId, _adminBatchAllowedExtensions, isAdminStaging: true,
                width: null, height: null);
        }

        string imagePath = await PendingImageStore.GetImagePathAsync(_assetRootPath, "software-screenshots", guid);
        if(imagePath is null)
        {
            PendingImageStore.Delete(_assetRootPath, "software-screenshots", guid);
            return BadRequest("Failed to persist uploaded file.");
        }

        (string magickFormat, int width, int height) = Photos.Identify(imagePath);
        if(magickFormat is null || !_adminBatchAllowedMagickFormats.Contains(magickFormat))
        {
            PendingImageStore.Delete(_assetRootPath, "software-screenshots", guid);
            return StatusCode(StatusCodes.Status415UnsupportedMediaType,
                              "File content does not match a supported image format.");
        }

        byte[] thumbBytes = Photos.GenerateThumbnailJpeg(imagePath);
        if(thumbBytes is null)
        {
            PendingImageStore.Delete(_assetRootPath, "software-screenshots", guid);
            return BadRequest("Failed to generate thumbnail for the uploaded image.");
        }

        await PendingImageStore.StoreThumbnailAsync(_assetRootPath, "software-screenshots", guid, thumbBytes);

        // Re-write the sidecar so dimensions are captured for future consumers.
        PendingImageStore.PendingMetadata meta =
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-screenshots", guid);
        if(meta is not null)
        {
            meta.Width  = width;
            meta.Height = height;
            string sidecar = Path.Combine(_assetRootPath, "photos", "software-screenshots", "pending",
                                          guid.ToString() + ".json");
            await System.IO.File.WriteAllTextAsync(sidecar, System.Text.Json.JsonSerializer.Serialize(meta));
        }

        string dataUrl = "data:image/jpeg;base64," + Convert.ToBase64String(thumbBytes);

        return Ok(new AdminPendingSoftwareScreenshotUploadDto
        {
            Id              = guid,
            Extension       = extension.TrimStart('.'),
            ThumbnailBase64 = dataUrl,
            SizeBytes       = file.Length,
            Width           = width,
            Height          = height
        });
    }

    /// <summary>
    ///     Delete a single admin-staged pending software-screenshot image before commit.
    ///     Owner-only (an admin cannot delete another admin's in-flight staging entries).
    /// </summary>
    [HttpDelete("admin/pending/{guid:guid}")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAdminBatchPendingAsync(Guid guid)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        PendingImageStore.PendingMetadata meta =
            await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-screenshots", guid);

        if(meta is null) return NotFound();
        if(meta.EntityType != (byte)SuggestionEntityType.SoftwareScreenshot) return NotFound();
        if(!meta.IsAdminStaging) return NotFound();
        if(!string.Equals(meta.UploadedById, userId, StringComparison.Ordinal)) return Forbid();

        PendingImageStore.Delete(_assetRootPath, "software-screenshots", guid);
        return NoContent();
    }

    /// <summary>
    ///     Commit a batch of admin-staged pending images into permanent
    ///     <c>SoftwareScreenshot</c> rows. The endpoint returns 202 with a <c>jobId</c>
    ///     immediately and processes the batch on a background task; the client polls
    ///     <c>GET /software/screenshots/admin/batch/{jobId}/status</c> for progress and
    ///     per-item results. Each image runs through the standard 8-variant conversion
    ///     sequentially so the progress bar advances one image at a time.
    /// </summary>
    [HttpPost("admin/batch/commit")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(typeof(AdminSoftwareScreenshotBatchJobStatusDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminSoftwareScreenshotBatchJobStatusDto>> CommitAdminBatchAsync(
        [FromBody] AdminSoftwareScreenshotBatchCommitRequestDto request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        if(request is null || request.Items is null || request.Items.Count == 0)
            return BadRequest("No items provided.");

        if(request.Items.Count > AdminBatchMaxImages)
            return BadRequest($"At most {AdminBatchMaxImages} images may be committed per batch.");

        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == (ulong)request.SoftwareId);
        if(!softwareExists) return NotFound("Software not found.");

        if(request.SoftwarePlatformId.HasValue)
        {
            ulong platformId = (ulong)request.SoftwarePlatformId.Value;
            bool platformExists = await context.SoftwarePlatforms.AnyAsync(p => p.Id == platformId);
            if(!platformExists) return NotFound("Software platform not found.");
        }

        // Validate every pending id belongs to the caller and matches software/admin scope.
        foreach(AdminSoftwareScreenshotBatchCommitItemDto item in request.Items)
        {
            PendingImageStore.PendingMetadata meta =
                await PendingImageStore.GetMetadataAsync(_assetRootPath, "software-screenshots", item.PendingId);

            if(meta is null) return NotFound($"Pending image {item.PendingId} not found.");
            if(meta.EntityType != (byte)SuggestionEntityType.SoftwareScreenshot)
                return BadRequest($"Pending image {item.PendingId} is not a software screenshot.");
            if(!meta.IsAdminStaging)
                return BadRequest($"Pending image {item.PendingId} is not an admin-staged image.");
            if(!string.Equals(meta.UploadedById, userId, StringComparison.Ordinal))
                return Forbid();
            if(meta.ParentEntityId != request.SoftwareId)
                return BadRequest($"Pending image {item.PendingId} was staged for a different software.");
        }

        Guid jobId = batchJobs.StartJob(userId, request.SoftwareId, request.Items.Count);

        var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
        string assetRootPath = _assetRootPath;
        List<AdminSoftwareScreenshotBatchCommitItemDto> items = request.Items;
        int softwareId = request.SoftwareId;
        int? platformIdNullable = request.SoftwarePlatformId;
        string canonicalGroupName = request.CanonicalGroupName;

        _ = Task.Run(() => RunBatchJobAsync(jobId, userId, softwareId, platformIdNullable, canonicalGroupName, items,
                                            assetRootPath, scopeFactory));

        AdminSoftwareScreenshotBatchJobStatusDto snapshot = SnapshotJob(batchJobs.GetForOwner(jobId, userId)!);
        return Accepted(snapshot);
    }

    /// <summary>
    ///     Poll the status of an in-flight admin software-screenshot batch-commit job.
    ///     Owner-only.
    /// </summary>
    [HttpGet("admin/batch/{jobId:guid}/status")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AdminSoftwareScreenshotBatchJobStatusDto> GetAdminBatchStatus(Guid jobId)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        BatchUploadJobStore.BatchJob job = batchJobs.GetForOwner(jobId, userId);
        if(job is null) return NotFound();

        return Ok(SnapshotJob(job));
    }

    static AdminSoftwareScreenshotBatchJobStatusDto SnapshotJob(BatchUploadJobStore.BatchJob job)
    {
        lock(job.Lock)
        {
            return new AdminSoftwareScreenshotBatchJobStatusDto
            {
                JobId            = job.JobId,
                State            = (int)job.State,
                Total            = job.Total,
                Processed        = job.Processed,
                CurrentPendingId = job.CurrentPendingId,
                Results = job.Results.Select(r => new AdminSoftwareScreenshotBatchJobItemResultDto
                              {
                                  PendingId    = r.PendingId,
                                  Succeeded    = r.Succeeded,
                                  ScreenshotId = r.AssignedId,
                                  Error        = r.Error
                              })
                             .ToList()
            };
        }
    }

    /// <summary>
    ///     Sequential worker that promotes each pending image into a SoftwareScreenshot
    ///     row, applies the shared platform + group + per-item caption, runs the 8-variant
    ///     conversion synchronously per image (so the progress bar advances one-at-a-time),
    ///     and records per-item success/failure on the job. Group resolve-or-create runs
    ///     once up-front so every image lands in the same group row.
    /// </summary>
    async Task RunBatchJobAsync(Guid jobId, string userId, int softwareId, int? softwarePlatformId,
                                string canonicalGroupName,
                                List<AdminSoftwareScreenshotBatchCommitItemDto> items, string assetRootPath,
                                IServiceScopeFactory scopeFactory)
    {
        BatchUploadJobStore.BatchJob job = batchJobs.GetForOwner(jobId, userId);
        if(job is null) return;

        lock(job.Lock) { job.State = BatchJobState.Processing; }

        Photos.EnsureCreated(assetRootPath, false, "software-screenshots");

        // Resolve-or-create the shared group ONCE up front using a fresh scope so every
        // image in the batch maps to the same SoftwareScreenshotGroup row.
        int? groupId = null;
        try
        {
            using IServiceScope groupScope = scopeFactory.CreateScope();
            var groupCtx = groupScope.ServiceProvider.GetRequiredService<MarechaiContext>();
            string trimmed = canonicalGroupName?.Trim();
            if(!string.IsNullOrWhiteSpace(trimmed))
            {
                if(trimmed.Length > 256) trimmed = trimmed[..256];

                SoftwareScreenshotGroup group =
                    await groupCtx.SoftwareScreenshotGroups.FirstOrDefaultAsync(g => g.Name == trimmed);

                if(group is null)
                {
                    group = new SoftwareScreenshotGroup { Name = trimmed };
                    await groupCtx.SoftwareScreenshotGroups.AddAsync(group);
                    await groupCtx.SaveChangesWithUserAsync(userId);
                }

                groupId = group.Id;
            }
        }
        catch(Exception ex)
        {
            // Failing to resolve the group is fatal for the batch; mark all items failed.
            foreach(AdminSoftwareScreenshotBatchCommitItemDto failedItem in items)
                AppendResult(job, failedItem.PendingId, false, null, $"Group resolve failed: {ex.Message}");

            lock(job.Lock)
            {
                job.Processed        = items.Count;
                job.CurrentPendingId = null;
                job.State            = BatchJobState.Failed;
                job.LastTouchedOn    = DateTime.UtcNow;
            }

            return;
        }

        ulong  softwareIdUlong = (ulong)softwareId;
        ulong? platformIdUlong = softwarePlatformId.HasValue ? (ulong)softwarePlatformId.Value : null;

        for(int i = 0; i < items.Count; i++)
        {
            AdminSoftwareScreenshotBatchCommitItemDto item = items[i];

            lock(job.Lock)
            {
                job.CurrentPendingId = item.PendingId;
                job.Processed        = i;
                job.LastTouchedOn    = DateTime.UtcNow;
            }

            try
            {
                (string movedPath, string ext) = await PendingImageStore.PromoteToOriginalsAsync(assetRootPath,
                                                     "software-screenshots", item.PendingId);

                if(movedPath is null)
                {
                    AppendResult(job, item.PendingId, false, null, "Pending file missing at commit time.");
                    continue;
                }

                // Rename the moved file to use a fresh screenshot guid so its filename
                // matches the DB row id (which is the public identifier).
                Guid   screenshotId = Guid.NewGuid();
                string originalsDir = Path.Combine(assetRootPath, "photos", "software-screenshots", "originals");
                string finalPath    = Path.Combine(originalsDir, screenshotId.ToString() + "." + ext);
                System.IO.File.Move(movedPath, finalPath, overwrite: true);

                string caption = string.IsNullOrWhiteSpace(item.Caption) ? null : item.Caption.Trim();

                var model = new SoftwareScreenshot
                {
                    Id                 = screenshotId,
                    SoftwareId         = softwareIdUlong,
                    SoftwarePlatformId = platformIdUlong,
                    GroupId            = groupId,
                    Caption            = caption,
                    OriginalExtension  = ext
                };

                using(IServiceScope scope = scopeFactory.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<MarechaiContext>();
                    await ctx.SoftwareScreenshots.AddAsync(model);
                    await ctx.SaveChangesWithUserAsync(userId);
                }

                // Run the 8-variant conversion synchronously so the progress bar advances
                // only after every variant for this image has landed.
                var photos = new Photos();
                photos.ConversionWorker(assetRootPath, screenshotId, finalPath, ext, false, "software-screenshots");

                AppendResult(job, item.PendingId, true, screenshotId, null);
            }
            catch(Exception ex)
            {
                AppendResult(job, item.PendingId, false, null, ex.Message);
            }
        }

        lock(job.Lock)
        {
            job.Processed        = items.Count;
            job.CurrentPendingId = null;
            job.State            = BatchJobState.Completed;
            job.LastTouchedOn    = DateTime.UtcNow;
        }
    }

    static void AppendResult(BatchUploadJobStore.BatchJob job, Guid pendingId, bool ok, Guid? screenshotId,
                             string error)
    {
        lock(job.Lock)
        {
            job.Results.Add(new BatchJobItemResult
            {
                PendingId  = pendingId,
                Succeeded  = ok,
                AssignedId = screenshotId,
                Error      = error
            });
            job.LastTouchedOn = DateTime.UtcNow;
        }
    }
}
