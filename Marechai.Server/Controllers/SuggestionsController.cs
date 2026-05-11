/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
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
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

/// <summary>
///     Endpoints for the user-driven collaborative-suggestions workflow:
///     <list type="bullet">
///         <item><c>POST /suggestions</c> — any authenticated user creates a suggestion.</item>
///         <item><c>GET /suggestions/queue</c> — admins list pending (UberAdmin: optional history).</item>
///         <item><c>GET /suggestions/{id}/diff</c> — admins fetch a live diff of one suggestion.</item>
///         <item><c>POST /suggestions/{id}/review</c> — admins accept/reject per-field.</item>
///         <item><c>GET /auth/me/suggestions</c> — caller lists their own suggestions.</item>
///         <item><c>DELETE /auth/me/suggestions/{id}</c> — caller withdraws their pending suggestion.</item>
///     </list>
///     Per-entity dispatch lives in the <c>switch</c> inside <see cref="ApplyAcceptedFieldsAsync" />.
///     Phase-1 only wires Company; the other entity cases throw <c>NotImplementedException</c> by design.
/// </summary>
[ApiController]
[Authorize]
public class SuggestionsController(MarechaiContext context,
                                   UserManager<ApplicationUser> userManager) : ControllerBase
{
    public const string CollaboratorRole = "Collaborator";
    public const string CuratorRole      = "Curator";
    public const string AdminRole        = "Admin";
    public const string UberAdminRole    = "UberAdmin";

    public const int MaxSuggestedFields    = 50;
    public const int MaxUserCommentLength  = 2000;
    public const int RateWindowDayLimit    = 10;       // 10 suggestions per 24h
    public const int RateWindowBurstLimit  = 3;        // 3 in any 60-second window

    static readonly TimeSpan s_rateDayWindow   = TimeSpan.FromHours(24);
    static readonly TimeSpan s_rateBurstWindow = TimeSpan.FromMinutes(1);

    static readonly JsonSerializerOptions s_json = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // ───────────────────────────── POST /suggestions ─────────────────────────────

    [HttpPost("/suggestions")]
    [ProducesResponseType(typeof(SuggestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SuggestionDto>> CreateAsync([FromBody] SuggestionDto dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();
        if(dto is null) return BadRequest();

        // EntityId is required for Phase 1 (edits only). Additions reserved for later.
        if(!dto.EntityId.HasValue || dto.EntityId.Value <= 0)
            return Problem(title: "Invalid suggestion",
                           detail: "Suggestions must target an existing entity (EntityId is required).",
                           statusCode: StatusCodes.Status400BadRequest);

        Dictionary<string, object> values;

        try
        {
            values = SuggestionsHelper.DeserializeValues(dto.SuggestedValuesJson);
        }
        catch(JsonException)
        {
            return Problem(title: "Invalid suggestion",
                           detail: "SuggestedValuesJson is not a valid JSON object.",
                           statusCode: StatusCodes.Status400BadRequest);
        }

        // ---- Payload size guard -----------------------------------------------------
        if(values.Count == 0)
            return Problem(title: "Empty suggestion",
                           detail: "At least one field must be suggested for change.",
                           statusCode: StatusCodes.Status400BadRequest);

        if(values.Count > MaxSuggestedFields)
            return Problem(title: "Suggestion too large",
                           detail:
                           $"A suggestion cannot include more than {MaxSuggestedFields} fields at once.",
                           statusCode: StatusCodes.Status400BadRequest);

        if(!string.IsNullOrEmpty(dto.UserComment) && dto.UserComment.Length > MaxUserCommentLength)
            return Problem(title: "Comment too long",
                           detail: $"User comment cannot exceed {MaxUserCommentLength} characters.",
                           statusCode: StatusCodes.Status400BadRequest);

        // ---- Field-name validation: only known field names are accepted -------------
        IReadOnlyCollection<string> knownFields = GetKnownFieldNames(dto.EntityType);

        if(knownFields is null)
            return Problem(title: "Unsupported entity",
                           detail: $"Suggestions for {dto.EntityType} are not yet supported.",
                           statusCode: StatusCodes.Status400BadRequest);

        List<string> unknown = values.Keys.Where(k => !knownFields.Contains(k)).ToList();

        if(unknown.Count > 0)
            return Problem(title: "Invalid suggestion",
                           detail: $"Unknown field name(s): {string.Join(", ", unknown)}",
                           statusCode: StatusCodes.Status400BadRequest);

        // ---- Existence check on target entity ---------------------------------------
        string entityDisplayName = await GetEntityDisplayNameAsync(dto.EntityType, dto.EntityId.Value);

        if(entityDisplayName is null)
            return NotFound();

        // ---- Per-entity dedupe: one Pending per (user, entity) ----------------------
        bool dupe = await context.Suggestions.AnyAsync(s =>
            s.CreatedById == userId
         && s.EntityType == dto.EntityType
         && s.EntityId == dto.EntityId.Value
         && s.Status == SuggestionStatus.Pending);

        if(dupe)
            return Problem(title: "Duplicate pending suggestion",
                           detail:
                           "You already have a pending suggestion for this item. Please withdraw it or wait for it to be reviewed before submitting another.",
                           statusCode: StatusCodes.Status409Conflict);

        // ---- Sliding-window rate caps (skipped for trusted roles) -------------------
        bool trusted = await IsInAnyRoleAsync(userId,
                                              CollaboratorRole, CuratorRole, AdminRole, UberAdminRole);

        if(!trusted)
        {
            DateTime now       = DateTime.UtcNow;
            DateTime dayCutoff = now - s_rateDayWindow;
            DateTime burstCutoff = now - s_rateBurstWindow;

            int dayCount = await context.Suggestions.CountAsync(s =>
                s.CreatedById == userId && s.CreatedOn >= dayCutoff);

            if(dayCount >= RateWindowDayLimit)
            {
                Response.Headers["Retry-After"] = ((int)s_rateDayWindow.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                return Problem(title: "Too many suggestions",
                               detail:
                               $"You can submit at most {RateWindowDayLimit} suggestions per 24 hours. Please wait before submitting another.",
                               statusCode: StatusCodes.Status429TooManyRequests);
            }

            int burstCount = await context.Suggestions.CountAsync(s =>
                s.CreatedById == userId && s.CreatedOn >= burstCutoff);

            if(burstCount >= RateWindowBurstLimit)
            {
                Response.Headers["Retry-After"] = ((int)s_rateBurstWindow.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                return Problem(title: "Too many suggestions",
                               detail:
                               $"You can submit at most {RateWindowBurstLimit} suggestions per minute. Please slow down.",
                               statusCode: StatusCodes.Status429TooManyRequests);
            }
        }

        // ---- Persist ----------------------------------------------------------------
        var suggestion = new Suggestion
        {
            EntityType      = dto.EntityType,
            EntityId        = dto.EntityId.Value,
            Status          = SuggestionStatus.Pending,
            CreatedById     = userId,
            UserComment     = string.IsNullOrWhiteSpace(dto.UserComment) ? null : dto.UserComment.Trim(),
            SuggestedValues = values
        };

        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();

        // ---- Notify all admins/uberadmins -------------------------------------------
        ApplicationUser sender = await userManager.FindByIdAsync(userId);
        string senderTag       = sender?.UserName is null ? "a user" : "@" + sender.UserName;
        string entityLink      = EntityLink(dto.EntityType, dto.EntityId.Value, entityDisplayName);
        string entityLabel     = EntityLabel(dto.EntityType);
        string subject         = $"New suggestion for {entityLabel} #{dto.EntityId.Value}";
        string body            = $"{senderTag} suggested changes to {entityLabel} {entityLink}. " +
                                 $"Open the [suggestions queue](/admin/suggestions-queue) to review.";

        await MessageDispatcher.SendSystemMessageToAdminsAsync(context, userManager, subject, body);

        SuggestionDto outDto = await ProjectAsync(suggestion, entityDisplayName);
        return CreatedAtAction(null, new { id = suggestion.Id }, outDto);
    }

    // ───────────────────────────── GET /suggestions/queue ─────────────────────────────

    [HttpGet("/suggestions/queue/count")]
    [Authorize(Roles = AdminRole + "," + UberAdminRole)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<int>> GetPendingCountAsync()
    {
        int count = await context.Suggestions.CountAsync(s => s.Status == SuggestionStatus.Pending);
        return Ok(count);
    }

    [HttpGet("/suggestions/queue")]
    [Authorize(Roles = AdminRole + "," + UberAdminRole)]
    [ProducesResponseType(typeof(List<SuggestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<SuggestionDto>>> GetQueueAsync(
        [FromQuery] bool includeHistory = false,
        [FromQuery] int  page           = 1,
        [FromQuery] int  pageSize       = 50)
    {
        if(page < 1) page         = 1;
        if(pageSize is < 1 or > 200) pageSize = 50;

        // Only UberAdmin can opt-in to history; for everyone else silently force pending-only.
        bool effectiveHistory = includeHistory && User.IsInRole(UberAdminRole);

        IQueryable<Suggestion> query = context.Suggestions.AsNoTracking();

        if(!effectiveHistory)
            query = query.Where(s => s.Status == SuggestionStatus.Pending);

        List<Suggestion> rows = await query
            .OrderBy(s => s.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(s => s.CreatedBy)
            .Include(s => s.ReviewedBy)
            .ToListAsync();

        var output = new List<SuggestionDto>(rows.Count);

        foreach(Suggestion s in rows)
        {
            string displayName = s.EntityId.HasValue
                                     ? await GetEntityDisplayNameAsync(s.EntityType, s.EntityId.Value)
                                     : null;

            output.Add(await ProjectAsync(s, displayName));
        }

        return Ok(output);
    }

    // ───────────────────────────── GET /suggestions/{id}/diff ─────────────────────────────

    [HttpGet("/suggestions/{id:long}/diff")]
    [Authorize(Roles = AdminRole + "," + UberAdminRole)]
    [ProducesResponseType(typeof(SuggestionDiffDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SuggestionDiffDto>> GetDiffAsync(long id)
    {
        Suggestion s = await context.Suggestions
                                    .Include(x => x.CreatedBy)
                                    .Include(x => x.ReviewedBy)
                                    .FirstOrDefaultAsync(x => x.Id == id);

        if(s is null) return NotFound();

        string displayName = s.EntityId.HasValue
                                 ? await GetEntityDisplayNameAsync(s.EntityType, s.EntityId.Value)
                                 : null;

        Dictionary<string, object> current = s.EntityId.HasValue
                                                 ? await GetCurrentValuesAsync(s.EntityType, s.EntityId.Value)
                                                 : null;

        SuggestionDto suggestionDto = await ProjectAsync(s, displayName);

        return Ok(new SuggestionDiffDto
        {
            Suggestion        = suggestionDto,
            CurrentValuesJson = SuggestionsHelper.SerializeValues(current),
            EntityMissing     = s.EntityId.HasValue && current is null
        });
    }

    // ───────────────────────────── POST /suggestions/{id}/review ─────────────────────────────

    [HttpPost("/suggestions/{id:long}/review")]
    [Authorize(Roles = AdminRole + "," + UberAdminRole)]
    [ProducesResponseType(typeof(SuggestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SuggestionDto>> ReviewAsync(long id, [FromBody] SuggestionReviewDto review)
    {
        string adminId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(adminId)) return Unauthorized();
        if(review is null) return BadRequest();

        Suggestion s = await context.Suggestions.FirstOrDefaultAsync(x => x.Id == id);
        if(s is null) return NotFound();

        if(s.Status != SuggestionStatus.Pending)
            return Problem(title: "Already reviewed",
                           detail: "This suggestion has already been reviewed.",
                           statusCode: StatusCodes.Status409Conflict);

        Dictionary<string, object> suggested = s.SuggestedValues ?? new Dictionary<string, object>();

        // Filter accepted to only those keys actually in the suggestion.
        HashSet<string> accepted = new(review.AcceptedFieldNames ?? new List<string>(), StringComparer.Ordinal);
        accepted.IntersectWith(suggested.Keys);

        // Apply via per-entity dispatch.
        if(s.EntityId.HasValue && accepted.Count > 0)
        {
            ApplyResult result = await ApplyAcceptedFieldsAsync(s.EntityType, s.EntityId.Value, suggested, accepted);

            if(result.EntityMissing)
            {
                s.Status     = SuggestionStatus.Stale;
                s.ReviewedById = adminId;
                s.ReviewedOn = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return Problem(title: "Entity no longer exists",
                               detail: "The targeted entity has been deleted; the suggestion was closed as stale.",
                               statusCode: StatusCodes.Status404NotFound);
            }

            // Whatever the applier actually wrote (it may have skipped invalid values).
            accepted = result.Applied;
        }

        // Compute terminal status.
        SuggestionStatus newStatus;

        if(accepted.Count == 0)
            newStatus = SuggestionStatus.Rejected;
        else if(accepted.Count == suggested.Count)
            newStatus = SuggestionStatus.Accepted;
        else
            newStatus = SuggestionStatus.PartiallyAccepted;

        s.Status        = newStatus;
        s.ReviewedById  = adminId;
        s.ReviewedOn    = DateTime.UtcNow;
        s.AppliedFields = accepted.ToDictionary(k => k, _ => string.Empty, StringComparer.Ordinal);

        await context.SaveChangesAsync();

        // Grant Collaborator role + send appropriate system message.
        bool granted = false;

        if(accepted.Count > 0)
        {
            ApplicationUser suggestingUser = await userManager.FindByIdAsync(s.CreatedById);

            if(suggestingUser is not null)
            {
                bool isAdmin     = await userManager.IsInRoleAsync(suggestingUser, AdminRole);
                bool isUberAdmin = await userManager.IsInRoleAsync(suggestingUser, UberAdminRole);
                bool isCollab    = await userManager.IsInRoleAsync(suggestingUser, CollaboratorRole);

                if(!isAdmin && !isUberAdmin && !isCollab)
                {
                    var roleResult = await userManager.AddToRoleAsync(suggestingUser, CollaboratorRole);
                    granted = roleResult.Succeeded;
                }
            }
        }

        string displayName = s.EntityId.HasValue
                                 ? await GetEntityDisplayNameAsync(s.EntityType, s.EntityId.Value)
                                 : null;

        await DispatchReviewMessageAsync(s, displayName, accepted.Count, suggested.Count, granted);

        return Ok(await ProjectAsync(s, displayName));
    }

    // ───────────────────────────── GET /auth/me/suggestions ─────────────────────────────

    [HttpGet("/auth/me/suggestions")]
    [ProducesResponseType(typeof(List<SuggestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<SuggestionDto>>> GetMyAsync()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        List<Suggestion> rows = await context.Suggestions
                                             .AsNoTracking()
                                             .Where(s => s.CreatedById == userId)
                                             .OrderByDescending(s => s.CreatedOn)
                                             .Take(200)
                                             .Include(s => s.CreatedBy)
                                             .Include(s => s.ReviewedBy)
                                             .ToListAsync();

        var output = new List<SuggestionDto>(rows.Count);

        foreach(Suggestion s in rows)
        {
            string displayName = s.EntityId.HasValue
                                     ? await GetEntityDisplayNameAsync(s.EntityType, s.EntityId.Value)
                                     : null;

            output.Add(await ProjectAsync(s, displayName));
        }

        return Ok(output);
    }

    // ───────────────────────────── DELETE /auth/me/suggestions/{id} ─────────────────────────────

    [HttpDelete("/auth/me/suggestions/{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> WithdrawAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        Suggestion s = await context.Suggestions.FirstOrDefaultAsync(x => x.Id == id);

        if(s is null || s.CreatedById != userId) return NotFound();

        if(s.Status != SuggestionStatus.Pending)
            return Problem(title: "Cannot withdraw",
                           detail: "Only pending suggestions can be withdrawn.",
                           statusCode: StatusCodes.Status409Conflict);

        s.Status     = SuggestionStatus.Withdrawn;
        s.ReviewedOn = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return NoContent();
    }

    // ───────────────────────────── Per-entity dispatch ─────────────────────────────

    record ApplyResult(HashSet<string> Applied, bool EntityMissing);

    /// <summary>
    ///     Returns the canonical field names supported by the suggestion metadata for
    ///     the given entity type. Returns <c>null</c> for entities not yet wired in
    ///     Phase 1 — the controller rejects suggestions for those with HTTP 400.
    /// </summary>
    static IReadOnlyCollection<string> GetKnownFieldNames(SuggestionEntityType type) => type switch
    {
        SuggestionEntityType.Company => Suggestions.CompanySuggestionApplier.KnownFieldNames,
        // Phase 3+ adds more cases here.
        _ => null
    };

    async Task<ApplyResult> ApplyAcceptedFieldsAsync(SuggestionEntityType type,
                                                     long entityId,
                                                     Dictionary<string, object> suggested,
                                                     HashSet<string> accepted)
    {
        switch(type)
        {
            case SuggestionEntityType.Company:
                var (applied, missing) = await Suggestions.CompanySuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted);
                return new ApplyResult(applied, missing);
            default:
                throw new NotImplementedException($"Suggestions for {type} are not implemented yet.");
        }
    }

    async Task<Dictionary<string, object>> GetCurrentValuesAsync(SuggestionEntityType type, long entityId)
    {
        return type switch
        {
            SuggestionEntityType.Company =>
                await Suggestions.CompanySuggestionApplier.GetCurrentValuesAsync(context, entityId),
            _ => null
        };
    }

    async Task<string> GetEntityDisplayNameAsync(SuggestionEntityType type, long entityId)
    {
        switch(type)
        {
            case SuggestionEntityType.Company:
                return await context.Companies.AsNoTracking()
                                    .Where(c => c.Id == (int)entityId)
                                    .Select(c => c.Name)
                                    .FirstOrDefaultAsync();
            default:
                return null;
        }
    }

    // ───────────────────────────── Helpers ─────────────────────────────

    async Task<bool> IsInAnyRoleAsync(string userId, params string[] roles)
    {
        ApplicationUser u = await userManager.FindByIdAsync(userId);
        if(u is null) return false;

        foreach(string role in roles)
            if(await userManager.IsInRoleAsync(u, role)) return true;

        return false;
    }

    async Task DispatchReviewMessageAsync(Suggestion s, string entityDisplayName,
                                          int acceptedCount, int suggestedCount, bool roleGranted)
    {
        string subject;
        string body;
        string entityLink  = s.EntityId.HasValue
                                 ? EntityLink(s.EntityType, s.EntityId.Value, entityDisplayName)
                                 : EntityLabel(s.EntityType);
        string entityLabel = EntityLabel(s.EntityType);

        if(acceptedCount == 0)
        {
            subject = "Your suggestion was not accepted";
            body =
                $"Your suggestion for {entityLabel} {entityLink} was reviewed but no fields were accepted. " +
                $"Thank you for contributing — feel free to refine and try again.";
        }
        else if(acceptedCount == suggestedCount)
        {
            subject = "Your suggestion was accepted";
            body =
                $"Your suggestion for {entityLabel} {entityLink} was accepted. Thank you!";

            if(roleGranted) body += "\n\nYou are now a Collaborator!";
        }
        else
        {
            subject = "Your suggestion was partially accepted";
            body =
                $"Your suggestion for {entityLabel} {entityLink} was reviewed. " +
                $"{acceptedCount} of {suggestedCount} suggested change(s) were applied; the rest were declined.";

            if(roleGranted) body += "\n\nYou are now a Collaborator!";
        }

        await Marechai.Database.Helpers.MessageDispatcher.PostSystemMessageAsync(context,
                                                                                 new[] { s.CreatedById },
                                                                                 subject,
                                                                                 body);
    }

    /// <summary>
    ///     Returns the public-facing URL for an entity (Phase 1: Company only). Future entity
    ///     types add their own case here. Returns <c>null</c> when no public route is known —
    ///     the caller falls back to a non-link tag.
    /// </summary>
    static string GetEntityUrl(SuggestionEntityType type, long entityId) => type switch
    {
        SuggestionEntityType.Company => $"/company/{entityId}",
        _                            => null
    };

    /// <summary>
    ///     Markdown link to the entity's public page (e.g. <c>[Apple](/company/355)</c>) when a
    ///     public route exists, falling back to a parenthesised id when it doesn't.
    /// </summary>
    static string EntityLink(SuggestionEntityType type, long entityId, string displayName)
    {
        string label = string.IsNullOrWhiteSpace(displayName) ? $"#{entityId}" : displayName;
        string url   = GetEntityUrl(type, entityId);
        return url is null ? $"'{label}' (#{entityId})" : $"[{label}]({url})";
    }

    /// <summary>Friendly lower-case label for the entity type used in message text.</summary>
    static string EntityLabel(SuggestionEntityType type) => type switch
    {
        SuggestionEntityType.Company         => "company",
        SuggestionEntityType.Machine         => "machine",
        SuggestionEntityType.MachineFamily   => "machine family",
        SuggestionEntityType.Processor       => "processor",
        SuggestionEntityType.Gpu             => "GPU",
        SuggestionEntityType.SoundSynth      => "sound synth",
        SuggestionEntityType.Software        => "software",
        SuggestionEntityType.SoftwareFamily  => "software family",
        SuggestionEntityType.SoftwareRelease => "software release",
        SuggestionEntityType.SoftwareVersion => "software version",
        SuggestionEntityType.Book            => "book",
        SuggestionEntityType.Document        => "document",
        SuggestionEntityType.Magazine        => "magazine",
        SuggestionEntityType.MagazineIssue   => "magazine issue",
        SuggestionEntityType.Person          => "person",
        SuggestionEntityType.Screen          => "screen",
        _                                    => type.ToString().ToLowerInvariant()
    };

    async Task<SuggestionDto> ProjectAsync(Suggestion s, string entityDisplayName)
    {
        await Task.CompletedTask;

        return new SuggestionDto
        {
            Id                    = s.Id,
            EntityType            = s.EntityType,
            EntityId              = s.EntityId,
            EntityDisplayName     = entityDisplayName,
            Status                = s.Status,
            CreatedById           = s.CreatedById,
            CreatedByUserName     = s.CreatedBy?.UserName,
            CreatedByDisplayName  = s.CreatedBy?.DisplayName,
            CreatedOn             = s.CreatedOn,
            ReviewedById          = s.ReviewedById,
            ReviewedByDisplayName = s.ReviewedBy?.DisplayName,
            ReviewedOn            = s.ReviewedOn,
            UserComment           = s.UserComment,
            SuggestedValuesJson   = SuggestionsHelper.SerializeValues(s.SuggestedValues),
            AppliedFieldsJson     = SuggestionsHelper.SerializeApplied(s.AppliedFields)
        };
    }
}
