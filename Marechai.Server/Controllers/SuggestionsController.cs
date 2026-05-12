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

    /// <summary>
    ///     Entity types for which a brand-new-entity suggestion (<c>EntityId == null</c>) is
    ///     accepted. Edit suggestions (<c>EntityId.HasValue</c>) are still gated by the per-entity
    ///     applier dispatch below.
    /// </summary>
    static readonly HashSet<SuggestionEntityType> s_supportsAddition = new()
    {
        SuggestionEntityType.Company
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

        // EntityId == null signals a brand-new-entity suggestion. Only certain entity types
        // allow that today (see s_supportsAddition); edits still require a positive EntityId.
        bool isAddition = !dto.EntityId.HasValue || dto.EntityId.Value <= 0;

        if(isAddition && !s_supportsAddition.Contains(dto.EntityType))
            return Problem(title: "Invalid suggestion",
                           detail:
                           $"Suggesting a brand-new {dto.EntityType} is not supported. Provide an EntityId to suggest edits instead.",
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

        // ---- Per-entity-type Subkey + value validation ------------------------------
        // CompanyDescription / MachineDescription require a Subkey (ISO-639-3 language code) + a non-empty markdown.
        string subkey = string.IsNullOrWhiteSpace(dto.Subkey) ? null : dto.Subkey.Trim();

        if(dto.EntityType == SuggestionEntityType.CompanyDescription)
        {
            if(subkey is null ||
               !Suggestions.CompanyDescriptionSuggestionApplier.IsAllowedLanguage(subkey))
                return Problem(title: "Invalid language",
                               detail:
                               "Description suggestions must specify a supported ISO-639-3 language code (eng, spa, deu, fra, ita, lat, por).",
                               statusCode: StatusCodes.Status400BadRequest);

            if(!values.TryGetValue(Suggestions.CompanyDescriptionSuggestionApplier.FieldMarkdown,
                                   out object mdValue) ||
               string.IsNullOrWhiteSpace(ExtractStringForValidation(mdValue)))
                return Problem(title: "Empty description",
                               detail:
                               "Description cannot be empty. Use the admin delete flow to remove a description.",
                               statusCode: StatusCodes.Status400BadRequest);
        }
        else if(dto.EntityType == SuggestionEntityType.MachineDescription)
        {
            if(subkey is null ||
               !Suggestions.MachineDescriptionSuggestionApplier.IsAllowedLanguage(subkey))
                return Problem(title: "Invalid language",
                               detail:
                               "Description suggestions must specify a supported ISO-639-3 language code (eng, spa, deu, fra, ita, lat, por).",
                               statusCode: StatusCodes.Status400BadRequest);

            if(!values.TryGetValue(Suggestions.MachineDescriptionSuggestionApplier.FieldMarkdown,
                                   out object mdValue) ||
               string.IsNullOrWhiteSpace(ExtractStringForValidation(mdValue)))
                return Problem(title: "Empty description",
                               detail:
                               "Description cannot be empty. Use the admin delete flow to remove a description.",
                               statusCode: StatusCodes.Status400BadRequest);
        }
        else if(dto.EntityType == SuggestionEntityType.BookSynopsis)
        {
            if(subkey is null ||
               !Suggestions.BookSynopsisSuggestionApplier.IsAllowedLanguage(subkey))
                return Problem(title: "Invalid language",
                               detail:
                               "Synopsis suggestions must specify a supported ISO-639-3 language code (eng, spa, deu, fra, ita, lat, por).",
                               statusCode: StatusCodes.Status400BadRequest);

            if(!values.TryGetValue(Suggestions.BookSynopsisSuggestionApplier.FieldMarkdown,
                                   out object mdValue) ||
               string.IsNullOrWhiteSpace(ExtractStringForValidation(mdValue)))
                return Problem(title: "Empty synopsis",
                               detail:
                               "Synopsis cannot be empty. Use the admin delete flow to remove a synopsis.",
                               statusCode: StatusCodes.Status400BadRequest);
        }
        else if(dto.EntityType == SuggestionEntityType.DocumentSynopsis)
        {
            if(subkey is null ||
               !Suggestions.DocumentSynopsisSuggestionApplier.IsAllowedLanguage(subkey))
                return Problem(title: "Invalid language",
                               detail:
                               "Synopsis suggestions must specify a supported ISO-639-3 language code (eng, spa, deu, fra, ita, lat, por).",
                               statusCode: StatusCodes.Status400BadRequest);

            if(!values.TryGetValue(Suggestions.DocumentSynopsisSuggestionApplier.FieldMarkdown,
                                   out object mdValue) ||
               string.IsNullOrWhiteSpace(ExtractStringForValidation(mdValue)))
                return Problem(title: "Empty synopsis",
                               detail:
                               "Synopsis cannot be empty. Use the admin delete flow to remove a synopsis.",
                               statusCode: StatusCodes.Status400BadRequest);
        }
        else if(dto.EntityType == SuggestionEntityType.MagazineSynopsis)
        {
            if(subkey is null ||
               !Suggestions.MagazineSynopsisSuggestionApplier.IsAllowedLanguage(subkey))
                return Problem(title: "Invalid language",
                               detail:
                               "Synopsis suggestions must specify a supported ISO-639-3 language code (eng, spa, deu, fra, ita, lat, por).",
                               statusCode: StatusCodes.Status400BadRequest);

            if(!values.TryGetValue(Suggestions.MagazineSynopsisSuggestionApplier.FieldMarkdown,
                                   out object mdValue) ||
               string.IsNullOrWhiteSpace(ExtractStringForValidation(mdValue)))
                return Problem(title: "Empty synopsis",
                               detail:
                               "Synopsis cannot be empty. Use the admin delete flow to remove a synopsis.",
                               statusCode: StatusCodes.Status400BadRequest);
        }
        else
        {
            // Subkey is only meaningful for *Description / *Synopsis entity types today; reject stray
            // values so the dedupe index doesn't get polluted with random strings.
            if(subkey is not null)
                return Problem(title: "Invalid suggestion",
                               detail:
                               $"Subkey is not supported for {dto.EntityType} suggestions.",
                               statusCode: StatusCodes.Status400BadRequest);
        }

        // ---- Existence check on target entity (edits only) --------------------------
        // For additions, the entity does not exist yet; the display name is derived from the
        // suggested 'name' payload field at projection time.
        string entityDisplayName;

        if(isAddition)
        {
            // Per-entity addition validation (e.g. Company requires a non-empty 'name').
            string addError = ValidateAdditionPayload(dto.EntityType, values);
            if(addError is not null)
                return Problem(title: "Invalid suggestion",
                               detail: addError,
                               statusCode: StatusCodes.Status400BadRequest);

            entityDisplayName = ExtractAdditionDisplayName(dto.EntityType, values);

            // ---- Addition dedupe: by NAME (per-user pending + collision with existing) -----
            string normalised = NormaliseName(entityDisplayName);

            // Cross-check against existing entities of this type — duplicate entity names should
            // be edited, not added a second time.
            if(await EntityWithNameExistsAsync(dto.EntityType, normalised))
                return Problem(title: "Already exists",
                               detail:
                               $"A {EntityLabel(dto.EntityType)} named '{entityDisplayName}' already exists. Suggest an edit on its page instead.",
                               statusCode: StatusCodes.Status409Conflict);

            // Per-user pending dedupe by normalised name (in-memory match on the JSON payload).
            // Project to ONLY the SuggestedValues column to keep the SELECT narrow — this avoids
            // materialising columns that may not exist on the live database (defensive against
            // unapplied migrations) and is also cheaper than fetching the full row.
            List<Dictionary<string, object>> userPendingPayloads = await context.Suggestions
                .Where(s => s.CreatedById == userId
                         && s.EntityType == dto.EntityType
                         && s.EntityId == null
                         && s.Status == SuggestionStatus.Pending)
                .Select(s => s.SuggestedValues)
                .ToListAsync();

            foreach(Dictionary<string, object> existingValues in userPendingPayloads)
            {
                string existingName = ExtractAdditionDisplayName(dto.EntityType, existingValues);
                if(string.Equals(NormaliseName(existingName), normalised, StringComparison.Ordinal))
                    return Problem(title: "Duplicate pending suggestion",
                                   detail:
                                   $"You already have a pending suggestion to add a {EntityLabel(dto.EntityType)} named '{entityDisplayName}'.",
                                   statusCode: StatusCodes.Status409Conflict);
            }
        }
        else
        {
            entityDisplayName = await GetEntityDisplayNameAsync(dto.EntityType, dto.EntityId.Value);

            if(entityDisplayName is null)
                return NotFound();

            // ---- Per-entity dedupe: one Pending per (user, entity, subkey) --------------
            bool dupe = await context.Suggestions.AnyAsync(s =>
                s.CreatedById == userId
             && s.EntityType == dto.EntityType
             && s.EntityId == dto.EntityId.Value
             && s.Subkey == subkey
             && s.Status == SuggestionStatus.Pending);

            if(dupe)
                return Problem(title: "Duplicate pending suggestion",
                               detail:
                               "You already have a pending suggestion for this item. Please withdraw it or wait for it to be reviewed before submitting another.",
                               statusCode: StatusCodes.Status409Conflict);
        }

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
            EntityId        = isAddition ? null : dto.EntityId.Value,
            Subkey          = subkey,
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
        string entityLabel     = EntityLabel(dto.EntityType);
        string secondaryLabel  = await GetEntitySecondaryLabelAsync(dto.EntityType, subkey);
        string entitySuffix    = string.IsNullOrEmpty(secondaryLabel) ? string.Empty : " " + secondaryLabel;

        string subject;
        string body;

        if(isAddition)
        {
            string nameTag = string.IsNullOrEmpty(entityDisplayName) ? string.Empty : $": **{entityDisplayName}**";
            subject        = $"New {entityLabel} suggested";
            body           = $"{senderTag} suggested a new {entityLabel}{nameTag}. " +
                             $"Open the [suggestions queue](/admin/suggestions-queue) to review.";
        }
        else
        {
            string entityLink = EntityLink(dto.EntityType, dto.EntityId.Value, entityDisplayName);
            subject           = $"New suggestion for {entityLabel} #{dto.EntityId.Value}";
            body              = $"{senderTag} suggested changes to {entityLabel} {entityLink}{entitySuffix}. " +
                                $"Open the [suggestions queue](/admin/suggestions-queue) to review.";
        }

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
            string displayName = await ResolveDisplayNameAsync(s);

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

        string displayName = await ResolveDisplayNameAsync(s);

        Dictionary<string, object> current = s.EntityId.HasValue
                                                 ? await GetCurrentValuesAsync(s.EntityType, s.EntityId.Value, s.Subkey)
                                                 : null;

        SuggestionDto suggestionDto    = await ProjectAsync(s, displayName);
        string        secondaryLabel   = await GetEntitySecondaryLabelAsync(s.EntityType, s.Subkey);

        Dictionary<string, JsonElement> currentDict   = ParseValuesForLabels(current);
        Dictionary<string, JsonElement> suggestedDict = ParseValuesForLabels(s.SuggestedValues);
        Dictionary<string, string> currentLabels      = await ResolveLabelsAsync(s.EntityType, currentDict);
        Dictionary<string, string> suggestedLabels    = await ResolveLabelsAsync(s.EntityType, suggestedDict);

        return Ok(new SuggestionDiffDto
        {
            Suggestion           = suggestionDto,
            CurrentValuesJson    = SuggestionsHelper.SerializeValues(current),
            EntityMissing        = s.EntityId.HasValue && current is null,
            EntitySecondaryLabel = secondaryLabel,
            CurrentLabels        = currentLabels.Count > 0 ? currentLabels : null,
            SuggestedLabels      = suggestedLabels.Count > 0 ? suggestedLabels : null
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

        // Capture admin's optional review comment (e.g. reason for rejection). Truncate to
        // the column max so the row never overflows.
        string adminComment = string.IsNullOrWhiteSpace(review.AdminComment) ? null : review.AdminComment.Trim();
        if(adminComment is { Length: > MaxUserCommentLength })
            adminComment = adminComment.Substring(0, MaxUserCommentLength);
        s.AdminReviewComment = adminComment;

        // Capture this BEFORE the addition path mutates s.EntityId, so the message dispatcher
        // can choose addition-flavoured wording afterwards.
        bool wasAddition = !s.EntityId.HasValue;

        // Apply via per-entity dispatch — branch on edit vs. addition.
        if(s.EntityId.HasValue && accepted.Count > 0)
        {
            ApplyResult result = await ApplyAcceptedFieldsAsync(s.EntityType, s.EntityId.Value, s.Subkey, suggested, accepted);

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
        else if(!s.EntityId.HasValue && accepted.Count > 0)
        {
            // Addition path: create a brand-new entity row, then patch s.EntityId so the
            // outgoing message carries a clickable link and the projected DTO has a real id.
            (long? newId, HashSet<string> applied) = await CreateNewEntityAsync(
                s.EntityType, suggested, accepted, s.CreatedById);

            if(newId.HasValue)
                s.EntityId = newId.Value;

            // If creation failed (e.g. admin didn't accept the mandatory 'name' field), the
            // applied set is empty and the suggestion will be marked Rejected below.
            accepted = applied;
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

        string displayName = await ResolveDisplayNameAsync(s);

        await DispatchReviewMessageAsync(s, displayName, accepted.Count, suggested.Count, granted, wasAddition);

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
            string displayName = await ResolveDisplayNameAsync(s);

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
        SuggestionEntityType.Company            => Suggestions.CompanySuggestionApplier.KnownFieldNames,
        SuggestionEntityType.CompanyDescription => Suggestions.CompanyDescriptionSuggestionApplier.KnownFieldNames,
        SuggestionEntityType.MachineDescription => Suggestions.MachineDescriptionSuggestionApplier.KnownFieldNames,
        SuggestionEntityType.BookSynopsis       => Suggestions.BookSynopsisSuggestionApplier.KnownFieldNames,
        SuggestionEntityType.DocumentSynopsis   => Suggestions.DocumentSynopsisSuggestionApplier.KnownFieldNames,
        SuggestionEntityType.MagazineSynopsis   => Suggestions.MagazineSynopsisSuggestionApplier.KnownFieldNames,
        // Phase 3+ adds more cases here.
        _ => null
    };

    async Task<ApplyResult> ApplyAcceptedFieldsAsync(SuggestionEntityType type,
                                                     long entityId,
                                                     string subkey,
                                                     Dictionary<string, object> suggested,
                                                     HashSet<string> accepted)
    {
        switch(type)
        {
            case SuggestionEntityType.Company:
            {
                var (applied, missing) = await Suggestions.CompanySuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.CompanyDescription:
            {
                var (applied, missing) = await Suggestions.CompanyDescriptionSuggestionApplier.ApplyAsync(
                    context, entityId, subkey, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.MachineDescription:
            {
                var (applied, missing) = await Suggestions.MachineDescriptionSuggestionApplier.ApplyAsync(
                    context, entityId, subkey, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.BookSynopsis:
            {
                var (applied, missing) = await Suggestions.BookSynopsisSuggestionApplier.ApplyAsync(
                    context, entityId, subkey, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.DocumentSynopsis:
            {
                var (applied, missing) = await Suggestions.DocumentSynopsisSuggestionApplier.ApplyAsync(
                    context, entityId, subkey, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.MagazineSynopsis:
            {
                var (applied, missing) = await Suggestions.MagazineSynopsisSuggestionApplier.ApplyAsync(
                    context, entityId, subkey, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            default:
                throw new NotImplementedException($"Suggestions for {type} are not implemented yet.");
        }
    }

    async Task<Dictionary<string, object>> GetCurrentValuesAsync(SuggestionEntityType type,
                                                                 long entityId,
                                                                 string subkey)
    {
        return type switch
        {
            SuggestionEntityType.Company =>
                await Suggestions.CompanySuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.CompanyDescription =>
                await Suggestions.CompanyDescriptionSuggestionApplier.GetCurrentValuesAsync(context, entityId, subkey),
            SuggestionEntityType.MachineDescription =>
                await Suggestions.MachineDescriptionSuggestionApplier.GetCurrentValuesAsync(context, entityId, subkey),
            SuggestionEntityType.BookSynopsis =>
                await Suggestions.BookSynopsisSuggestionApplier.GetCurrentValuesAsync(context, entityId, subkey),
            SuggestionEntityType.DocumentSynopsis =>
                await Suggestions.DocumentSynopsisSuggestionApplier.GetCurrentValuesAsync(context, entityId, subkey),
            SuggestionEntityType.MagazineSynopsis =>
                await Suggestions.MagazineSynopsisSuggestionApplier.GetCurrentValuesAsync(context, entityId, subkey),
            _ => null
        };
    }

    async Task<string> GetEntityDisplayNameAsync(SuggestionEntityType type, long entityId)
    {
        switch(type)
        {
            case SuggestionEntityType.Company:
            case SuggestionEntityType.CompanyDescription:
                return await context.Companies.AsNoTracking()
                                    .Where(c => c.Id == (int)entityId)
                                    .Select(c => c.Name)
                                    .FirstOrDefaultAsync();
            case SuggestionEntityType.Machine:
            case SuggestionEntityType.MachineDescription:
                return await context.Machines.AsNoTracking()
                                    .Where(m => m.Id == (int)entityId)
                                    .Select(m => m.Name)
                                    .FirstOrDefaultAsync();
            case SuggestionEntityType.Book:
            case SuggestionEntityType.BookSynopsis:
                return await context.Books.AsNoTracking()
                                    .Where(b => b.Id == entityId)
                                    .Select(b => b.Title)
                                    .FirstOrDefaultAsync();
            case SuggestionEntityType.Document:
            case SuggestionEntityType.DocumentSynopsis:
                return await context.Documents.AsNoTracking()
                                    .Where(d => d.Id == entityId)
                                    .Select(d => d.Title)
                                    .FirstOrDefaultAsync();
            case SuggestionEntityType.Magazine:
            case SuggestionEntityType.MagazineSynopsis:
                return await context.Magazines.AsNoTracking()
                                    .Where(m => m.Id == entityId)
                                    .Select(m => m.Title)
                                    .FirstOrDefaultAsync();
            default:
                return null;
        }
    }

    /// <summary>
    ///     Resolve the display name for a suggestion row. For edits (<c>EntityId</c> set), looks
    ///     up the live name from the targeted entity. For additions (<c>EntityId == null</c>),
    ///     extracts the name from the suggested-values payload so queue / review surfaces still
    ///     have a useful label before the entity is created.
    /// </summary>
    async Task<string> ResolveDisplayNameAsync(Suggestion s)
    {
        if(s.EntityId.HasValue)
            return await GetEntityDisplayNameAsync(s.EntityType, s.EntityId.Value);

        return ExtractAdditionDisplayName(s.EntityType, s.SuggestedValues);
    }

    /// <summary>
    ///     Per-entity validation for brand-new-entity submissions. Returns <c>null</c> when
    ///     the payload passes, or a friendly error string for the 400 response detail.
    /// </summary>
    static string ValidateAdditionPayload(SuggestionEntityType type, Dictionary<string, object> values)
    {
        switch(type)
        {
            case SuggestionEntityType.Company:
            {
                if(!values.TryGetValue(Suggestions.CompanySuggestionApplier.FieldName, out object n) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(n)))
                    return "A new company suggestion must include a non-empty 'name' field.";
                return null;
            }
            default:
                return $"Brand-new {type} suggestions are not supported.";
        }
    }

    /// <summary>
    ///     Pull the per-entity display name from a suggested-values payload (e.g. the
    ///     <c>name</c> field for Company). Returns <c>null</c> when the payload doesn't
    ///     include a usable name.
    /// </summary>
    static string ExtractAdditionDisplayName(SuggestionEntityType type, Dictionary<string, object> values)
    {
        if(values is null) return null;

        switch(type)
        {
            case SuggestionEntityType.Company:
            {
                if(values.TryGetValue(Suggestions.CompanySuggestionApplier.FieldName, out object n))
                {
                    string s = ExtractStringForValidation(n);
                    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                }
                return null;
            }
            default:
                return null;
        }
    }

    /// <summary>
    ///     Cross-check whether an entity of the given type already exists with the given
    ///     normalised name (lower-case, trimmed). Used by addition dedupe to redirect users
    ///     to the edit flow when they try to suggest a brand-new entity that's already in the
    ///     catalogue.
    /// </summary>
    async Task<bool> EntityWithNameExistsAsync(SuggestionEntityType type, string normalisedName)
    {
        if(string.IsNullOrEmpty(normalisedName)) return false;

        switch(type)
        {
            case SuggestionEntityType.Company:
                return await context.Companies.AsNoTracking()
                                    .AnyAsync(c => c.Name != null && c.Name.ToLower() == normalisedName);
            default:
                return false;
        }
    }

    static string NormaliseName(string name) =>
        string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim().ToLowerInvariant();

    /// <summary>
    ///     Per-entity dispatch for creating a brand-new entity from an accepted suggestion.
    ///     Returns the new entity id (so the controller can patch <c>Suggestion.EntityId</c>) and
    ///     the actually-applied field set. Returns <c>(null, empty)</c> when creation can't
    ///     proceed (e.g. admin didn't tick the mandatory <c>name</c> field).
    /// </summary>
    async Task<(long? newId, HashSet<string> applied)> CreateNewEntityAsync(
        SuggestionEntityType type,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string creditedUserId)
    {
        switch(type)
        {
            case SuggestionEntityType.Company:
            {
                var (id, applied) = await Suggestions.CompanySuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId);
                return (id, applied);
            }
            default:
                throw new NotImplementedException($"Creating a new {type} from a suggestion is not implemented yet.");
        }
    }

    /// <summary>
    ///     Optional secondary label for the entity (e.g. <c>"(Spanish description)"</c>) used
    ///     by the queue / review dialog to disambiguate per-subkey suggestions. Returns
    ///     <c>null</c> when the entity type doesn't use a Subkey.
    /// </summary>
    async Task<string> GetEntitySecondaryLabelAsync(SuggestionEntityType type, string subkey)
    {
        if(string.IsNullOrEmpty(subkey)) return null;

        switch(type)
        {
            case SuggestionEntityType.CompanyDescription:
            {
                string langName = await context.Iso639.AsNoTracking()
                                               .Where(l => l.Id == subkey)
                                               .Select(l => l.ReferenceName)
                                               .FirstOrDefaultAsync();
                return $"({langName ?? subkey} description)";
            }
            case SuggestionEntityType.MachineDescription:
            {
                string langName = await context.Iso639.AsNoTracking()
                                               .Where(l => l.Id == subkey)
                                               .Select(l => l.ReferenceName)
                                               .FirstOrDefaultAsync();
                return $"({langName ?? subkey} description)";
            }
            case SuggestionEntityType.BookSynopsis:
            {
                string langName = await context.Iso639.AsNoTracking()
                                               .Where(l => l.Id == subkey)
                                               .Select(l => l.ReferenceName)
                                               .FirstOrDefaultAsync();
                return $"({langName ?? subkey} synopsis)";
            }
            case SuggestionEntityType.DocumentSynopsis:
            {
                string langName = await context.Iso639.AsNoTracking()
                                               .Where(l => l.Id == subkey)
                                               .Select(l => l.ReferenceName)
                                               .FirstOrDefaultAsync();
                return $"({langName ?? subkey} synopsis)";
            }
            case SuggestionEntityType.MagazineSynopsis:
            {
                string langName = await context.Iso639.AsNoTracking()
                                               .Where(l => l.Id == subkey)
                                               .Select(l => l.ReferenceName)
                                               .FirstOrDefaultAsync();
                return $"({langName ?? subkey} synopsis)";
            }
            default:
                return null;
        }
    }

    /// <summary>
    ///     Reproject the raw payload dictionary (whose values are <c>object</c>) into a
    ///     <c>JsonElement</c>-keyed shape so the FK-id extraction in
    ///     <see cref="ResolveLabelsAsync" /> can reuse the same reading code as the client diff
    ///     panel.
    /// </summary>
    static Dictionary<string, JsonElement> ParseValuesForLabels(Dictionary<string, object> values)
    {
        var result = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        if(values is null) return result;

        foreach(KeyValuePair<string, object> kv in values)
        {
            if(kv.Value is null) continue;
            string json = JsonSerializer.Serialize(kv.Value);
            try
            {
                using var doc = JsonDocument.Parse(json);
                result[kv.Key] = doc.RootElement.Clone();
            }
            catch
            {
                // Skip values that don't round-trip through JsonElement.
            }
        }

        return result;
    }

    /// <summary>
    ///     Resolve foreign-key field values to display labels for the given entity type. Returns
    ///     a dictionary mapping field-name → label (e.g. <c>"country_id" → "Spain"</c>) for any
    ///     FK fields with a recognised id; non-FK fields and unresolved ids are omitted. The
    ///     diff panel falls back to <c>SuggestionMetadata.FormatDisplayValue</c> for missing
    ///     entries.
    /// </summary>
    async Task<Dictionary<string, string>> ResolveLabelsAsync(SuggestionEntityType type,
                                                              Dictionary<string, JsonElement> values)
    {
        var labels = new Dictionary<string, string>(StringComparer.Ordinal);
        if(values is null || values.Count == 0) return labels;

        switch(type)
        {
            case SuggestionEntityType.Company:
            {
                if(values.TryGetValue(Suggestions.CompanySuggestionApplier.FieldCountryId, out JsonElement cv))
                {
                    short? id = JsonElementToShort(cv);
                    if(id.HasValue)
                    {
                        string name = await context.Iso31661Numeric.AsNoTracking()
                                                   .Where(c => c.Id == id.Value)
                                                   .Select(c => c.Name)
                                                   .FirstOrDefaultAsync();
                        if(!string.IsNullOrEmpty(name))
                            labels[Suggestions.CompanySuggestionApplier.FieldCountryId] = name;
                    }
                }

                if(values.TryGetValue(Suggestions.CompanySuggestionApplier.FieldSoldToId, out JsonElement sv))
                {
                    int? id = JsonElementToInt(sv);
                    if(id.HasValue)
                    {
                        string name = await context.Companies.AsNoTracking()
                                                   .Where(c => c.Id == id.Value)
                                                   .Select(c => c.Name)
                                                   .FirstOrDefaultAsync();
                        if(!string.IsNullOrEmpty(name))
                            labels[Suggestions.CompanySuggestionApplier.FieldSoldToId] = name;
                    }
                }
                break;
            }
        }

        return labels;
    }

    static int? JsonElementToInt(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.Number => e.TryGetInt32(out int i) ? i : null,
        JsonValueKind.String => int.TryParse(e.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int p) ? p : null,
        _                    => null
    };

    static short? JsonElementToShort(JsonElement e)
    {
        int? i = JsonElementToInt(e);
        return i.HasValue && i.Value is >= short.MinValue and <= short.MaxValue ? (short)i.Value : null;
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

    /// <summary>
    ///     Best-effort coercion of a raw JSON-deserialised value to a string suitable for
    ///     "is this empty" validation. Mirrors the per-applier <c>ToStringValue</c> helpers
    ///     but kept here so the controller stays self-contained for validation.
    /// </summary>
    static string ExtractStringForValidation(object v)
    {
        return v switch
        {
            null            => null,
            JsonElement je  => je.ValueKind switch
            {
                JsonValueKind.Null   => null,
                JsonValueKind.String => je.GetString(),
                _                    => je.ToString()
            },
            string s        => s,
            _               => v.ToString()
        };
    }

    async Task DispatchReviewMessageAsync(Suggestion s, string entityDisplayName,
                                          int acceptedCount, int suggestedCount,
                                          bool roleGranted, bool wasAddition)
    {
        string subject;
        string body;
        string entityLabel = EntityLabel(s.EntityType);

        // Per-subkey suggestions get a parenthesised secondary tag (e.g. "(Spanish description)")
        // so the recipient can tell which language was reviewed.
        string secondaryLabel = await GetEntitySecondaryLabelAsync(s.EntityType, s.Subkey);
        string entitySuffix   = string.IsNullOrEmpty(secondaryLabel) ? string.Empty : " " + secondaryLabel;

        // Reference text — clickable markdown link when the entity exists, fallback to the
        // payload-derived name (or just the entity-type label) when it doesn't.
        string entityRef;

        if(s.EntityId.HasValue)
            entityRef = EntityLink(s.EntityType, s.EntityId.Value, entityDisplayName);
        else if(!string.IsNullOrWhiteSpace(entityDisplayName))
            entityRef = $"**{entityDisplayName}**";
        else
            entityRef = entityLabel;

        if(wasAddition)
        {
            // Addition flow: name-mandatory, so partial acceptance is impossible.
            if(acceptedCount == 0)
            {
                subject = $"Your new {entityLabel} suggestion was not accepted";
                body    = $"Your suggestion to add a new {entityLabel} {entityRef} was not accepted. " +
                          $"Thank you for contributing — feel free to refine and try again.";
            }
            else
            {
                subject = $"Your new {entityLabel} suggestion was accepted";
                body    = $"Your suggestion to add a new {entityLabel} {entityRef} was accepted. Thank you!";

                if(roleGranted) body += "\n\nYou are now a Collaborator!";
            }
        }
        else if(acceptedCount == 0)
        {
            subject = "Your suggestion was not accepted";
            body =
                $"Your suggestion for {entityLabel} {entityRef}{entitySuffix} was reviewed but no fields were accepted. " +
                $"Thank you for contributing — feel free to refine and try again.";
        }
        else if(acceptedCount == suggestedCount)
        {
            subject = "Your suggestion was accepted";
            body =
                $"Your suggestion for {entityLabel} {entityRef}{entitySuffix} was accepted. Thank you!";

            if(roleGranted) body += "\n\nYou are now a Collaborator!";
        }
        else
        {
            subject = "Your suggestion was partially accepted";
            body =
                $"Your suggestion for {entityLabel} {entityRef}{entitySuffix} was reviewed. " +
                $"{acceptedCount} of {suggestedCount} suggested change(s) were applied; the rest were declined.";

            if(roleGranted) body += "\n\nYou are now a Collaborator!";
        }

        // Surface the admin's optional review comment (e.g. reason for rejection) as a
        // markdown blockquote so the recipient sees it inline in their inbox.
        if(!string.IsNullOrWhiteSpace(s.AdminReviewComment))
        {
            string quoted = string.Join("\n", s.AdminReviewComment.Split('\n').Select(l => "> " + l));
            body += $"\n\n**Reviewer comment:**\n\n{quoted}";
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
        SuggestionEntityType.Company            => $"/company/{entityId}",
        SuggestionEntityType.CompanyDescription => $"/company/{entityId}",
        SuggestionEntityType.Machine            => $"/machine/{entityId}",
        SuggestionEntityType.MachineDescription => $"/machine/{entityId}",
        SuggestionEntityType.Book               => $"/book/{entityId}",
        SuggestionEntityType.BookSynopsis       => $"/book/{entityId}",
        SuggestionEntityType.Document           => $"/document/{entityId}",
        SuggestionEntityType.DocumentSynopsis   => $"/document/{entityId}",
        SuggestionEntityType.Magazine           => $"/magazine/{entityId}",
        SuggestionEntityType.MagazineSynopsis   => $"/magazine/{entityId}",
        _                                       => null
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
        SuggestionEntityType.Company             => "company",
        SuggestionEntityType.CompanyDescription  => "company description",
        SuggestionEntityType.Machine             => "machine",
        SuggestionEntityType.MachineDescription  => "machine description",
        SuggestionEntityType.BookSynopsis        => "book synopsis",
        SuggestionEntityType.DocumentSynopsis    => "document synopsis",
        SuggestionEntityType.MagazineSynopsis    => "magazine synopsis",
        SuggestionEntityType.MachineFamily       => "machine family",
        SuggestionEntityType.Processor           => "processor",
        SuggestionEntityType.Gpu                 => "GPU",
        SuggestionEntityType.SoundSynth          => "sound synth",
        SuggestionEntityType.Software            => "software",
        SuggestionEntityType.SoftwareFamily      => "software family",
        SuggestionEntityType.SoftwareRelease     => "software release",
        SuggestionEntityType.SoftwareVersion     => "software version",
        SuggestionEntityType.Book                => "book",
        SuggestionEntityType.Document            => "document",
        SuggestionEntityType.Magazine            => "magazine",
        SuggestionEntityType.MagazineIssue       => "magazine issue",
        SuggestionEntityType.Person              => "person",
        SuggestionEntityType.Screen              => "screen",
        _                                        => type.ToString().ToLowerInvariant()
    };

    async Task<SuggestionDto> ProjectAsync(Suggestion s, string entityDisplayName)
    {
        await Task.CompletedTask;

        return new SuggestionDto
        {
            Id                    = s.Id,
            EntityType            = s.EntityType,
            EntityId              = s.EntityId,
            Subkey                = s.Subkey,
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
            AdminReviewComment    = s.AdminReviewComment,
            SuggestedValuesJson   = SuggestionsHelper.SerializeValues(s.SuggestedValues),
            AppliedFieldsJson     = SuggestionsHelper.SerializeApplied(s.AppliedFields)
        };
    }
}
