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
using Microsoft.Extensions.Configuration;

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
                                   UserManager<ApplicationUser> userManager,
                                   IConfiguration configuration) : ControllerBase
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

    /// <summary>
    ///     Asset root path for media uploads. Captured from configuration so the per-entity
    ///     applier dispatch (<see cref="ApplyAcceptedFieldsAsync" />) can pass it to entities
    ///     that promote pending uploads (e.g. Book covers via
    ///     <see cref="Marechai.Server.Helpers.PendingImageStore.PromoteToOriginalsAsync" />).
    /// </summary>
    readonly string _assetRootPath = configuration["AssetRootPath"]!;

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
        SuggestionEntityType.Company,
        SuggestionEntityType.Machine,
        SuggestionEntityType.Magazine,
        SuggestionEntityType.Book,
        SuggestionEntityType.Document,
        SuggestionEntityType.MagazineIssue,
        SuggestionEntityType.Gpu,
        SuggestionEntityType.Processor,
        SuggestionEntityType.SoundSynth,
        SuggestionEntityType.Person,
        SuggestionEntityType.Software,
        SuggestionEntityType.SoftwareRelease
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
        if(!IsEntityTypeSupported(dto.EntityType))
            return Problem(title: "Unsupported entity",
                           detail: $"Suggestions for {dto.EntityType} are not yet supported.",
                           statusCode: StatusCodes.Status400BadRequest);

        List<string> unknown = values.Keys.Where(k => !IsKnownFieldName(dto.EntityType, k)).ToList();

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
        else if(dto.EntityType == SuggestionEntityType.GpuDescription)
        {
            if(subkey is null ||
               !Suggestions.GpuDescriptionSuggestionApplier.IsAllowedLanguage(subkey))
                return Problem(title: "Invalid language",
                               detail:
                               "Description suggestions must specify a supported ISO-639-3 language code (eng, spa, deu, fra, ita, lat, por).",
                               statusCode: StatusCodes.Status400BadRequest);

            if(!values.TryGetValue(Suggestions.GpuDescriptionSuggestionApplier.FieldMarkdown,
                                   out object mdValue) ||
               string.IsNullOrWhiteSpace(ExtractStringForValidation(mdValue)))
                return Problem(title: "Empty description",
                               detail:
                               "Description cannot be empty. Use the admin delete flow to remove a description.",
                               statusCode: StatusCodes.Status400BadRequest);
        }
        else if(dto.EntityType == SuggestionEntityType.ProcessorDescription)
        {
            if(subkey is null ||
               !Suggestions.ProcessorDescriptionSuggestionApplier.IsAllowedLanguage(subkey))
                return Problem(title: "Invalid language",
                               detail:
                               "Description suggestions must specify a supported ISO-639-3 language code (eng, spa, deu, fra, ita, lat, por).",
                               statusCode: StatusCodes.Status400BadRequest);

            if(!values.TryGetValue(Suggestions.ProcessorDescriptionSuggestionApplier.FieldMarkdown,
                                   out object mdValue) ||
               string.IsNullOrWhiteSpace(ExtractStringForValidation(mdValue)))
                return Problem(title: "Empty description",
                               detail:
                               "Description cannot be empty. Use the admin delete flow to remove a description.",
                               statusCode: StatusCodes.Status400BadRequest);
        }
        else if(dto.EntityType == SuggestionEntityType.SoundSynthDescription)
        {
            if(subkey is null ||
               !Suggestions.SoundSynthDescriptionSuggestionApplier.IsAllowedLanguage(subkey))
                return Problem(title: "Invalid language",
                               detail:
                               "Description suggestions must specify a supported ISO-639-3 language code (eng, spa, deu, fra, ita, lat, por).",
                               statusCode: StatusCodes.Status400BadRequest);

            if(!values.TryGetValue(Suggestions.SoundSynthDescriptionSuggestionApplier.FieldMarkdown,
                                   out object mdValue) ||
               string.IsNullOrWhiteSpace(ExtractStringForValidation(mdValue)))
                return Problem(title: "Empty description",
                               detail:
                               "Description cannot be empty. Use the admin delete flow to remove a description.",
                               statusCode: StatusCodes.Status400BadRequest);
        }
        else if(dto.EntityType == SuggestionEntityType.PersonDescription)
        {
            if(subkey is null ||
               !Suggestions.PersonDescriptionSuggestionApplier.IsAllowedLanguage(subkey))
                return Problem(title: "Invalid language",
                               detail:
                               "Biography suggestions must specify a supported ISO-639-3 language code (eng, spa, deu, fra, ita, lat, por).",
                               statusCode: StatusCodes.Status400BadRequest);

            if(!values.TryGetValue(Suggestions.PersonDescriptionSuggestionApplier.FieldMarkdown,
                                   out object mdValue) ||
               string.IsNullOrWhiteSpace(ExtractStringForValidation(mdValue)))
                return Problem(title: "Empty biography",
                               detail:
                               "Biography cannot be empty. Use the admin delete flow to remove a biography.",
                               statusCode: StatusCodes.Status400BadRequest);
        }
        else if(dto.EntityType == SuggestionEntityType.SoftwareDescription)
        {
            if(subkey is null ||
               !Suggestions.SoftwareDescriptionSuggestionApplier.IsAllowedLanguage(subkey))
                return Problem(title: "Invalid language",
                               detail:
                               "Description suggestions must specify a supported ISO-639-3 language code (eng, spa, deu, fra, ita, lat, por).",
                               statusCode: StatusCodes.Status400BadRequest);

            if(!values.TryGetValue(Suggestions.SoftwareDescriptionSuggestionApplier.FieldMarkdown,
                                   out object mdValue) ||
               string.IsNullOrWhiteSpace(ExtractStringForValidation(mdValue)))
                return Problem(title: "Empty description",
                               detail:
                               "Description cannot be empty. Use the admin delete flow to remove a description.",
                               statusCode: StatusCodes.Status400BadRequest);
        }
        else
        {
            // Subkey is only meaningful for *Description / *Synopsis entity types today and
            // for SoftwareRelease (which uses subkey to discriminate which card the user is
            // editing — `specs`, `ratings`, or null for the general-info dialog — so the
            // per-(user, entity, subkey) duplicate-pending guard treats those three flows
            // as independent). Reject stray values for other entity types so the dedupe
            // index doesn't get polluted with random strings.
            if(subkey is not null)
            {
                bool subkeyAllowed = dto.EntityType == SuggestionEntityType.SoftwareRelease &&
                                     (subkey == Suggestions.SoftwareReleaseSuggestionApplier.GroupSpecs ||
                                      subkey == Suggestions.SoftwareReleaseSuggestionApplier.GroupRatings);
                if(!subkeyAllowed)
                    return Problem(title: "Invalid suggestion",
                                   detail:
                                   $"Subkey is not supported for {dto.EntityType} suggestions.",
                                   statusCode: StatusCodes.Status400BadRequest);
            }
        }

        // ---- GPU photo batch payload validation (suggestion-level + per-photo sidecars) ----
        if(dto.EntityType == SuggestionEntityType.GpuPhoto)
        {
            var (ok, err) = await Suggestions.GpuPhotoSuggestionApplier.ValidateAsync(
                                context, dto.EntityId, values, _assetRootPath, userId);

            if(!ok)
                return Problem(title: "Invalid GPU photo suggestion",
                               detail: err,
                               statusCode: StatusCodes.Status400BadRequest);
        }

        // ---- Processor photo batch payload validation (suggestion-level + per-photo sidecars) ----
        if(dto.EntityType == SuggestionEntityType.ProcessorPhoto)
        {
            var (ok, err) = await Suggestions.ProcessorPhotoSuggestionApplier.ValidateAsync(
                                context, dto.EntityId, values, _assetRootPath, userId);

            if(!ok)
                return Problem(title: "Invalid processor photo suggestion",
                               detail: err,
                               statusCode: StatusCodes.Status400BadRequest);
        }

        // ---- SoundSynth photo batch payload validation (suggestion-level + per-photo sidecars) ----
        if(dto.EntityType == SuggestionEntityType.SoundSynthPhoto)
        {
            var (ok, err) = await Suggestions.SoundSynthPhotoSuggestionApplier.ValidateAsync(
                                context, dto.EntityId, values, _assetRootPath, userId);

            if(!ok)
                return Problem(title: "Invalid sound synth photo suggestion",
                               detail: err,
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
            // GpuPhoto, ProcessorPhoto and SoundSynthPhoto batches are intentionally exempt — a
            // collaborator can have multiple pending photo batches for the same parent at once
            // (the per-uploader cap of 15 in-flight pending PHOTOS per parent is enforced
            // separately by the upload endpoint via
            // PendingImageStore.CountByUploaderForParentEntity).
            if(dto.EntityType != SuggestionEntityType.GpuPhoto &&
               dto.EntityType != SuggestionEntityType.ProcessorPhoto &&
               dto.EntityType != SuggestionEntityType.SoundSynthPhoto)
            {
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
        Dictionary<string, string> currentLabels      = await ResolveLabelsAsync(s.EntityType, s.EntityId, currentDict);
        Dictionary<string, string> suggestedLabels    = await ResolveLabelsAsync(s.EntityType, s.EntityId, suggestedDict);

        // Machine entity-edit suggestions encode junction add/remove ops as dynamic keys in
        // the SUGGESTED payload only. Adds resolve from the suggested payload's id field
        // (already handled by ResolveMachineLabelsAsync via the suggested-side ResolveLabelsAsync
        // pass above). Removes need a DB lookup of the existing junction row, but the label
        // belongs in the CURRENT-side map so the diff panel renders the readable name in the
        // strikethrough cell. The current-side GetCurrentValuesAsync only returns scalar fields,
        // so we make a dedicated pass here. CRITICAL: iterate over the RAW SuggestedValues dict
        // (NOT the parsed suggestedDict) because ParseValuesForLabels skips null-valued entries
        // and junction-remove keys carry a null value by design.
        if(s.EntityType == SuggestionEntityType.Machine && s.EntityId.HasValue && s.SuggestedValues is not null)
            await ResolveMachineRemoveLabelsAsync((int)s.EntityId.Value, s.SuggestedValues, currentLabels);

        // Same pattern for Book entity-edit suggestions: junction-remove labels need a DB
        // lookup constrained to the targeted book and the result lands in the current-side
        // label map so the diff panel renders the readable name in the strikethrough cell.
        // CRITICAL (same lesson as Machine): iterate the RAW SuggestedValues dict because
        // ParseValuesForLabels skips null-valued entries and remove keys carry a null value.
        if(s.EntityType == SuggestionEntityType.Book && s.EntityId.HasValue && s.SuggestedValues is not null)
            await ResolveBookRemoveLabelsAsync(s.EntityId.Value, s.SuggestedValues, currentLabels);

        // Same pattern for Document entity-edit suggestions: junction-remove labels need a DB
        // lookup constrained to the targeted document and the result lands in the current-side
        // label map so the diff panel renders the readable name in the strikethrough cell.
        // CRITICAL (same lesson as Machine/Book): iterate the RAW SuggestedValues dict because
        // ParseValuesForLabels skips null-valued entries and remove keys carry a null value.
        if(s.EntityType == SuggestionEntityType.Document && s.EntityId.HasValue && s.SuggestedValues is not null)
            await ResolveDocumentRemoveLabelsAsync(s.EntityId.Value, s.SuggestedValues, currentLabels);

        // Same pattern for Magazine entity-edit suggestions (single Companies junction). People
        // belong to MagazineIssue, not Magazine, so they're absent here.
        if(s.EntityType == SuggestionEntityType.Magazine && s.EntityId.HasValue && s.SuggestedValues is not null)
            await ResolveMagazineRemoveLabelsAsync(s.EntityId.Value, s.SuggestedValues, currentLabels);

        // Same pattern for MagazineIssue entity-edit suggestions (4 junctions: people,
        // machines, machine_families, software). NB: the People/Machines/MachineFamilies/
        // Software junctions all hang off MagazineIssue, not the parent Magazine — see
        // MagazineIssueSuggestionApplier for the column-name caveat.
        if(s.EntityType == SuggestionEntityType.MagazineIssue && s.EntityId.HasValue && s.SuggestedValues is not null)
            await ResolveMagazineIssueRemoveLabelsAsync(s.EntityId.Value, s.SuggestedValues, currentLabels);

        // Same pattern for Gpu entity-edit suggestions (single Resolutions junction). The
        // junction-remove labels need a DB lookup of the existing ResolutionsByGpu row
        // joined to the underlying Resolution, and the result lands in the current-side
        // label map so the diff panel renders the readable name in the strikethrough cell.
        if(s.EntityType == SuggestionEntityType.Gpu && s.EntityId.HasValue && s.SuggestedValues is not null)
            await ResolveGpuRemoveLabelsAsync(s.EntityId.Value, s.SuggestedValues, currentLabels);

        // Same pattern for Processor entity-edit suggestions (single Instruction Set
        // Extensions junction). Mirrors the Gpu shape: int-keyed, no cover, simple-FK
        // junction without a role column.
        if(s.EntityType == SuggestionEntityType.Processor && s.EntityId.HasValue && s.SuggestedValues is not null)
            await ResolveProcessorRemoveLabelsAsync(s.EntityId.Value, s.SuggestedValues, currentLabels);

        // Same pattern for Software entity-edit suggestions (single Genres junction).
        // Software is ulong-keyed; the junction is a composite-key table (SoftwareId,
        // GenreId) with NO surrogate Id, so the remove token IS the genre id rather than a
        // row id — see SoftwareSuggestionApplier.ApplyJunctionRemove for the lookup.
        if(s.EntityType == SuggestionEntityType.Software && s.EntityId.HasValue && s.SuggestedValues is not null)
            await ResolveSoftwareRemoveLabelsAsync(s.EntityId.Value, s.SuggestedValues, currentLabels);

        // Same pattern for SoftwareRelease entity-edit suggestions (4 junctions: regions,
        // languages, barcodes, product_codes). Mixes 2 composite-key junctions (regions
        // short FK, languages string FK) with 2 surrogate-Id junctions (barcodes/
        // product_codes), so the per-group token parser dispatches by group — see
        // SoftwareReleaseSuggestionApplier.ApplyJunctionRemove for the per-group lookup.
        if(s.EntityType == SuggestionEntityType.SoftwareRelease && s.EntityId.HasValue && s.SuggestedValues is not null)
            await ResolveSoftwareReleaseRemoveLabelsAsync(s.EntityId.Value, s.SuggestedValues, currentLabels);

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

        // Filter accepted to only those keys actually in the suggestion. Dynamic accept-key
        // patterns (e.g. GpuPhoto's `photo.<guid>` per-photo toggles) are NOT literal keys in
        // SuggestedValues — they're synthetic field names derived from a JSON array element
        // by the diff panel — so intersecting against suggested.Keys would drop them. Keep
        // them when the per-entity validator recognises the shape.
        HashSet<string> accepted = new(review.AcceptedFieldNames ?? new List<string>(), StringComparer.Ordinal);
        accepted.RemoveWhere(k => !suggested.ContainsKey(k) && !IsKnownFieldName(s.EntityType, k));

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
            ApplyResult result = await ApplyAcceptedFieldsAsync(s.EntityType, s.EntityId.Value, s.Subkey, suggested, accepted, s.CreatedById);

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

        // Compute terminal status. GpuPhoto, ProcessorPhoto and SoundSynthPhoto batches need
        // entity-specific accounting because the wire payload's scalar key set (`license_id`,
        // `source_url`, `photos`) bears no relation to the per-photo accept toggles
        // (`photo.<guid>`) the admin actually checks: `accepted.Count == suggested.Count` is
        // meaningless there. For these batches we compare the COUNT of accepted `photo.<guid>`
        // keys against the size of the `photos` JSON array.
        SuggestionStatus newStatus;

        if(s.EntityType == SuggestionEntityType.GpuPhoto)
        {
            int totalPhotos = CountSuggestedGpuPhotos(s);
            int acceptedPhotos = accepted.Count(k => k.StartsWith("photo.", StringComparison.Ordinal));

            if(acceptedPhotos == 0)
                newStatus = SuggestionStatus.Rejected;
            else if(totalPhotos > 0 && acceptedPhotos >= totalPhotos)
                newStatus = SuggestionStatus.Accepted;
            else
                newStatus = SuggestionStatus.PartiallyAccepted;
        }
        else if(s.EntityType == SuggestionEntityType.ProcessorPhoto)
        {
            int totalPhotos = CountSuggestedProcessorPhotos(s);
            int acceptedPhotos = accepted.Count(k => k.StartsWith("photo.", StringComparison.Ordinal));

            if(acceptedPhotos == 0)
                newStatus = SuggestionStatus.Rejected;
            else if(totalPhotos > 0 && acceptedPhotos >= totalPhotos)
                newStatus = SuggestionStatus.Accepted;
            else
                newStatus = SuggestionStatus.PartiallyAccepted;
        }
        else if(s.EntityType == SuggestionEntityType.SoundSynthPhoto)
        {
            int totalPhotos = CountSuggestedSoundSynthPhotos(s);
            int acceptedPhotos = accepted.Count(k => k.StartsWith("photo.", StringComparison.Ordinal));

            if(acceptedPhotos == 0)
                newStatus = SuggestionStatus.Rejected;
            else if(totalPhotos > 0 && acceptedPhotos >= totalPhotos)
                newStatus = SuggestionStatus.Accepted;
            else
                newStatus = SuggestionStatus.PartiallyAccepted;
        }
        else if(accepted.Count == 0)
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

        // Pending-image cleanup: every suggestion that carried a cover_pending_guid in its
        // payload but did NOT end up applying the cover (rejected, or partially accepted
        // with the cover unchecked) leaves an orphaned pending file on disk. Sweep them
        // here so the pending folder doesn't grow unbounded. Cleanup is best-effort —
        // failures don't block the review flow.
        CleanupPendingMedia(s, accepted);

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
        SuggestionEntityType.GpuDescription     => Suggestions.GpuDescriptionSuggestionApplier.KnownFieldNames,
        SuggestionEntityType.ProcessorDescription  => Suggestions.ProcessorDescriptionSuggestionApplier.KnownFieldNames,
        SuggestionEntityType.SoundSynthDescription => Suggestions.SoundSynthDescriptionSuggestionApplier.KnownFieldNames,
        SuggestionEntityType.PersonDescription     => Suggestions.PersonDescriptionSuggestionApplier.KnownFieldNames,
        SuggestionEntityType.SoftwareDescription   => Suggestions.SoftwareDescriptionSuggestionApplier.KnownFieldNames,
        // Phase 3+ adds more cases here.
        _ => null
    };

    /// <summary>
    ///     Returns <c>true</c> when the entity type has a registered suggestion applier (either
    ///     a static <c>KnownFieldNames</c> set or a dynamic <c>IsKnownFieldName</c> predicate).
    /// </summary>
    static bool IsEntityTypeSupported(SuggestionEntityType type) => type switch
    {
        SuggestionEntityType.Machine       => true,
        SuggestionEntityType.Book          => true,
        SuggestionEntityType.Document      => true,
        SuggestionEntityType.Magazine      => true,
        SuggestionEntityType.MagazineIssue => true,
        SuggestionEntityType.Gpu           => true,
        SuggestionEntityType.Processor     => true,
        SuggestionEntityType.SoundSynth    => true,
        SuggestionEntityType.Person          => true,
        SuggestionEntityType.Software        => true,
        SuggestionEntityType.SoftwareRelease => true,
        SuggestionEntityType.GpuPhoto        => true,
        SuggestionEntityType.ProcessorPhoto  => true,
        SuggestionEntityType.SoundSynthPhoto => true,
        _                                  => GetKnownFieldNames(type) is not null
    };

    /// <summary>
    ///     Per-entity field-name validator. Wraps the static <c>KnownFieldNames</c> HashSet
    ///     check for entities with a fixed scalar field set, and dispatches to a per-applier
    ///     <c>IsKnownFieldName</c> predicate for entities that accept dynamic field-name
    ///     patterns (e.g. <c>&lt;group&gt;.add.&lt;uuid&gt;</c> for Machine junction operations).
    /// </summary>
    static bool IsKnownFieldName(SuggestionEntityType type, string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;

        if(type == SuggestionEntityType.Machine)
            return Suggestions.MachineSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.Book)
            return Suggestions.BookSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.Document)
            return Suggestions.DocumentSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.Magazine)
            return Suggestions.MagazineSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.MagazineIssue)
            return Suggestions.MagazineIssueSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.Gpu)
            return Suggestions.GpuSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.Processor)
            return Suggestions.ProcessorSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.SoundSynth)
            return Suggestions.SoundSynthSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.Person)
            return Suggestions.PersonSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.Software)
            return Suggestions.SoftwareSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.SoftwareRelease)
            return Suggestions.SoftwareReleaseSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.GpuPhoto)
            return Suggestions.GpuPhotoSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.ProcessorPhoto)
            return Suggestions.ProcessorPhotoSuggestionApplier.IsKnownFieldName(fieldName);

        if(type == SuggestionEntityType.SoundSynthPhoto)
            return Suggestions.SoundSynthPhotoSuggestionApplier.IsKnownFieldName(fieldName);

        IReadOnlyCollection<string> set = GetKnownFieldNames(type);
        return set is not null && set.Contains(fieldName);
    }

    async Task<ApplyResult> ApplyAcceptedFieldsAsync(SuggestionEntityType type,
                                                     long entityId,
                                                     string subkey,
                                                     Dictionary<string, object> suggested,
                                                     HashSet<string> accepted,
                                                     string creditedUserId)
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
            case SuggestionEntityType.GpuDescription:
            {
                var (applied, missing) = await Suggestions.GpuDescriptionSuggestionApplier.ApplyAsync(
                    context, entityId, subkey, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.ProcessorDescription:
            {
                var (applied, missing) = await Suggestions.ProcessorDescriptionSuggestionApplier.ApplyAsync(
                    context, entityId, subkey, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.SoundSynthDescription:
            {
                var (applied, missing) = await Suggestions.SoundSynthDescriptionSuggestionApplier.ApplyAsync(
                    context, entityId, subkey, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.PersonDescription:
            {
                var (applied, missing) = await Suggestions.PersonDescriptionSuggestionApplier.ApplyAsync(
                    context, entityId, subkey, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.SoftwareDescription:
            {
                var (applied, missing) = await Suggestions.SoftwareDescriptionSuggestionApplier.ApplyAsync(
                    context, entityId, subkey, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.Machine:
            {
                var (applied, missing) = await Suggestions.MachineSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.Book:
            {
                var (applied, missing) = await Suggestions.BookSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted, _assetRootPath);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.Document:
            {
                var (applied, missing) = await Suggestions.DocumentSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.Magazine:
            {
                var (applied, missing) = await Suggestions.MagazineSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.MagazineIssue:
            {
                var (applied, missing) = await Suggestions.MagazineIssueSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted, _assetRootPath);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.Gpu:
            {
                var (applied, missing) = await Suggestions.GpuSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.Processor:
            {
                var (applied, missing) = await Suggestions.ProcessorSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.SoundSynth:
            {
                var (applied, missing) = await Suggestions.SoundSynthSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.Person:
            {
                var (applied, missing) = await Suggestions.PersonSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted, _assetRootPath);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.Software:
            {
                var (applied, missing) = await Suggestions.SoftwareSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.SoftwareRelease:
            {
                var (applied, missing) = await Suggestions.SoftwareReleaseSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.GpuPhoto:
            {
                var (applied, missing) = await Suggestions.GpuPhotoSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted, creditedUserId, _assetRootPath);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.ProcessorPhoto:
            {
                var (applied, missing) = await Suggestions.ProcessorPhotoSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted, creditedUserId, _assetRootPath);
                return new ApplyResult(applied, missing);
            }
            case SuggestionEntityType.SoundSynthPhoto:
            {
                var (applied, missing) = await Suggestions.SoundSynthPhotoSuggestionApplier.ApplyAsync(
                    context, entityId, suggested, accepted, creditedUserId, _assetRootPath);
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
            SuggestionEntityType.GpuDescription =>
                await Suggestions.GpuDescriptionSuggestionApplier.GetCurrentValuesAsync(context, entityId, subkey),
            SuggestionEntityType.ProcessorDescription =>
                await Suggestions.ProcessorDescriptionSuggestionApplier.GetCurrentValuesAsync(context, entityId, subkey),
            SuggestionEntityType.SoundSynthDescription =>
                await Suggestions.SoundSynthDescriptionSuggestionApplier.GetCurrentValuesAsync(context, entityId, subkey),
            SuggestionEntityType.PersonDescription =>
                await Suggestions.PersonDescriptionSuggestionApplier.GetCurrentValuesAsync(context, entityId, subkey),
            SuggestionEntityType.SoftwareDescription =>
                await Suggestions.SoftwareDescriptionSuggestionApplier.GetCurrentValuesAsync(context, entityId, subkey),
            SuggestionEntityType.Machine =>
                await Suggestions.MachineSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.Book =>
                await Suggestions.BookSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.Document =>
                await Suggestions.DocumentSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.Magazine =>
                await Suggestions.MagazineSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.MagazineIssue =>
                await Suggestions.MagazineIssueSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.Gpu =>
                await Suggestions.GpuSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.Processor =>
                await Suggestions.ProcessorSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.SoundSynth =>
                await Suggestions.SoundSynthSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.Person =>
                await Suggestions.PersonSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.Software =>
                await Suggestions.SoftwareSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.SoftwareRelease =>
                await Suggestions.SoftwareReleaseSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.GpuPhoto =>
                await Suggestions.GpuPhotoSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.ProcessorPhoto =>
                await Suggestions.ProcessorPhotoSuggestionApplier.GetCurrentValuesAsync(context, entityId),
            SuggestionEntityType.SoundSynthPhoto =>
                await Suggestions.SoundSynthPhotoSuggestionApplier.GetCurrentValuesAsync(context, entityId),
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
            case SuggestionEntityType.MagazineIssue:
                return await context.MagazineIssues.AsNoTracking()
                                    .Where(mi => mi.Id == entityId)
                                    .Select(mi => mi.Caption)
                                    .FirstOrDefaultAsync();
            case SuggestionEntityType.Gpu:
            case SuggestionEntityType.GpuDescription:
            case SuggestionEntityType.GpuPhoto:
                return await context.Gpus.AsNoTracking()
                                    .Where(g => g.Id == (int)entityId)
                                    .Select(g => g.Name)
                                    .FirstOrDefaultAsync();
            case SuggestionEntityType.Processor:
            case SuggestionEntityType.ProcessorDescription:
            case SuggestionEntityType.ProcessorPhoto:
                return await context.Processors.AsNoTracking()
                                    .Where(p => p.Id == (int)entityId)
                                    .Select(p => p.Name)
                                    .FirstOrDefaultAsync();
            case SuggestionEntityType.SoundSynth:
            case SuggestionEntityType.SoundSynthDescription:
            case SuggestionEntityType.SoundSynthPhoto:
                return await context.SoundSynths.AsNoTracking()
                                    .Where(s => s.Id == (int)entityId)
                                    .Select(s => s.Name)
                                    .FirstOrDefaultAsync();
            case SuggestionEntityType.Person:
            case SuggestionEntityType.PersonDescription:
                return await context.People.AsNoTracking()
                                    .Where(p => p.Id == (int)entityId)
                                    .Select(p => p.DisplayName ?? (p.Name + " " + p.Surname))
                                    .FirstOrDefaultAsync();
            case SuggestionEntityType.Software:
            case SuggestionEntityType.SoftwareDescription:
                return await context.Softwares.AsNoTracking()
                                    .Where(s => s.Id == (ulong)entityId)
                                    .Select(s => s.Name)
                                    .FirstOrDefaultAsync();
            case SuggestionEntityType.SoftwareRelease:
            {
                // Surface BOTH the parent software/version AND the release title so the
                // suggestion queue row reads e.g. "Quake 1.01 — Shareware" rather than just
                // one piece. Falls back gracefully when one side is missing.
                var row = await context.SoftwareReleases.AsNoTracking()
                                       .Where(r => r.Id == (ulong)entityId)
                                       .Select(r => new
                                        {
                                            r.Title,
                                            SoftwareName = r.Software.Name,
                                            VersionString = r.SoftwareVersion.VersionString
                                        })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                string parent = !string.IsNullOrWhiteSpace(row.SoftwareName)
                                    ? (string.IsNullOrWhiteSpace(row.VersionString)
                                           ? row.SoftwareName
                                           : $"{row.SoftwareName} {row.VersionString}")
                                    : null;
                if(string.IsNullOrWhiteSpace(parent)) return row.Title;
                if(string.IsNullOrWhiteSpace(row.Title)) return parent;
                return $"{parent} \u2014 {row.Title}";
            }
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
            case SuggestionEntityType.Machine:
            {
                if(!values.TryGetValue(Suggestions.MachineSuggestionApplier.FieldName, out object n) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(n)))
                    return "A new machine suggestion must include a non-empty 'name' field.";
                if(!values.ContainsKey(Suggestions.MachineSuggestionApplier.FieldType))
                    return "A new machine suggestion must include a 'type' field (computer, console or smartphone).";
                if(!values.ContainsKey(Suggestions.MachineSuggestionApplier.FieldCompanyId))
                    return "A new machine suggestion must include a 'company_id' field referencing the manufacturer.";
                return null;
            }
            case SuggestionEntityType.Magazine:
            {
                if(!values.TryGetValue(Suggestions.MagazineSuggestionApplier.FieldTitle, out object n) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(n)))
                    return "A new magazine suggestion must include a non-empty 'title' field.";
                return null;
            }
            case SuggestionEntityType.Book:
            {
                if(!values.TryGetValue(Suggestions.BookSuggestionApplier.FieldTitle, out object n) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(n)))
                    return "A new book suggestion must include a non-empty 'title' field.";
                return null;
            }
            case SuggestionEntityType.Document:
            {
                if(!values.TryGetValue(Suggestions.DocumentSuggestionApplier.FieldTitle, out object n) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(n)))
                    return "A new document suggestion must include a non-empty 'title' field.";
                return null;
            }
            case SuggestionEntityType.MagazineIssue:
            {
                if(!values.TryGetValue(Suggestions.MagazineIssueSuggestionApplier.FieldMagazineId, out object _))
                    return "A new magazine issue suggestion must include a 'magazine_id' field referencing the parent magazine.";
                if(!values.TryGetValue(Suggestions.MagazineIssueSuggestionApplier.FieldCaption, out object n) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(n)))
                    return "A new magazine issue suggestion must include a non-empty 'caption' field.";
                return null;
            }
            case SuggestionEntityType.Gpu:
            {
                if(!values.TryGetValue(Suggestions.GpuSuggestionApplier.FieldName, out object n) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(n)))
                    return "A new GPU suggestion must include a non-empty 'name' field.";
                return null;
            }
            case SuggestionEntityType.Processor:
            {
                if(!values.TryGetValue(Suggestions.ProcessorSuggestionApplier.FieldName, out object n) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(n)))
                    return "A new processor suggestion must include a non-empty 'name' field.";
                return null;
            }
            case SuggestionEntityType.SoundSynth:
            {
                if(!values.TryGetValue(Suggestions.SoundSynthSuggestionApplier.FieldName, out object n) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(n)))
                    return "A new sound synthesizer suggestion must include a non-empty 'name' field.";
                return null;
            }
            case SuggestionEntityType.Person:
            {
                if(!values.TryGetValue(Suggestions.PersonSuggestionApplier.FieldName, out object n) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(n)))
                    return "A new person suggestion must include a non-empty 'name' field.";
                if(!values.TryGetValue(Suggestions.PersonSuggestionApplier.FieldSurname, out object s) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(s)))
                    return "A new person suggestion must include a non-empty 'surname' field.";
                return null;
            }
            case SuggestionEntityType.SoftwareRelease:
            {
                // Per-field validation in priority order so the user gets a specific error
                // for whichever mandatory field is missing first (title → publisher →
                // platform → software). Matches the dialog's own per-field defence-in-depth.
                if(!values.TryGetValue(Suggestions.SoftwareReleaseSuggestionApplier.FieldTitle, out object t) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(t)))
                    return "A new software release suggestion must include a non-empty 'title' field.";
                if(!values.ContainsKey(Suggestions.SoftwareReleaseSuggestionApplier.FieldPublisherId))
                    return "A new software release suggestion must include a 'publisher_id' field referencing the publisher.";
                if(!values.ContainsKey(Suggestions.SoftwareReleaseSuggestionApplier.FieldPlatformId))
                    return "A new software release suggestion must include a 'platform_id' field referencing the platform.";
                if(!values.ContainsKey(Suggestions.SoftwareReleaseSuggestionApplier.FieldSoftwareId))
                    return "A new software release suggestion must include a 'software_id' field referencing the parent software.";
                return null;
            }
            case SuggestionEntityType.Software:
            {
                // Software has no user-facing surface without a release, so a brand-new
                // Software suggestion MUST carry its first-release in the same submission.
                // Validation order: software name → kind → first_release_title →
                // first_release_publisher_id → first_release_platform_id (matches the
                // dialog's per-field defence-in-depth in priority order).
                if(!values.TryGetValue(Suggestions.SoftwareSuggestionApplier.FieldName, out object n) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(n)))
                    return "A new software suggestion must include a non-empty 'name' field.";
                if(!values.TryGetValue(Suggestions.SoftwareSuggestionApplier.FieldKind, out object k) ||
                   !TryExtractIntForValidation(k, out int kindValue) ||
                   !Enum.IsDefined(typeof(Marechai.Data.SoftwareKind), kindValue))
                    return "A new software suggestion must include a valid 'kind' field.";
                string firstReleaseTitleKey = Suggestions.SoftwareSuggestionApplier.FirstReleaseScalarPrefix +
                                              Suggestions.SoftwareReleaseSuggestionApplier.FieldTitle;
                if(!values.TryGetValue(firstReleaseTitleKey, out object frt) ||
                   string.IsNullOrWhiteSpace(ExtractStringForValidation(frt)))
                    return "A new software suggestion must include a non-empty 'first_release_title' field (a software requires its first release at creation time).";
                string firstReleasePublisherKey = Suggestions.SoftwareSuggestionApplier.FirstReleaseScalarPrefix +
                                                  Suggestions.SoftwareReleaseSuggestionApplier.FieldPublisherId;
                if(!values.ContainsKey(firstReleasePublisherKey))
                    return "A new software suggestion must include a 'first_release_publisher_id' field referencing the first release's publisher.";
                string firstReleasePlatformKey = Suggestions.SoftwareSuggestionApplier.FirstReleaseScalarPrefix +
                                                 Suggestions.SoftwareReleaseSuggestionApplier.FieldPlatformId;
                if(!values.ContainsKey(firstReleasePlatformKey))
                    return "A new software suggestion must include a 'first_release_platform_id' field referencing the first release's platform.";
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
            case SuggestionEntityType.Machine:
            {
                if(values.TryGetValue(Suggestions.MachineSuggestionApplier.FieldName, out object n))
                {
                    string s = ExtractStringForValidation(n);
                    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                }
                return null;
            }
            case SuggestionEntityType.Magazine:
            {
                if(values.TryGetValue(Suggestions.MagazineSuggestionApplier.FieldTitle, out object n))
                {
                    string s = ExtractStringForValidation(n);
                    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                }
                return null;
            }
            case SuggestionEntityType.Book:
            {
                if(values.TryGetValue(Suggestions.BookSuggestionApplier.FieldTitle, out object n))
                {
                    string s = ExtractStringForValidation(n);
                    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                }
                return null;
            }
            case SuggestionEntityType.Document:
            {
                if(values.TryGetValue(Suggestions.DocumentSuggestionApplier.FieldTitle, out object n))
                {
                    string s = ExtractStringForValidation(n);
                    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                }
                return null;
            }
            case SuggestionEntityType.MagazineIssue:
            {
                if(values.TryGetValue(Suggestions.MagazineIssueSuggestionApplier.FieldCaption, out object n))
                {
                    string s = ExtractStringForValidation(n);
                    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                }
                return null;
            }
            case SuggestionEntityType.Gpu:
            {
                if(values.TryGetValue(Suggestions.GpuSuggestionApplier.FieldName, out object n))
                {
                    string s = ExtractStringForValidation(n);
                    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                }
                return null;
            }
            case SuggestionEntityType.Processor:
            {
                if(values.TryGetValue(Suggestions.ProcessorSuggestionApplier.FieldName, out object n))
                {
                    string s = ExtractStringForValidation(n);
                    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                }
                return null;
            }
            case SuggestionEntityType.SoundSynth:
            {
                if(values.TryGetValue(Suggestions.SoundSynthSuggestionApplier.FieldName, out object n))
                {
                    string s = ExtractStringForValidation(n);
                    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                }
                return null;
            }
            case SuggestionEntityType.Person:
            {
                // Combine name + surname for the queue display label. Fall back gracefully
                // when only one is present (the validator will reject the suggestion later
                // anyway, but we still want a sensible label in the meantime).
                string namePart    = values.TryGetValue(Suggestions.PersonSuggestionApplier.FieldName,    out object nVal) ? ExtractStringForValidation(nVal)?.Trim() : null;
                string surnamePart = values.TryGetValue(Suggestions.PersonSuggestionApplier.FieldSurname, out object sVal) ? ExtractStringForValidation(sVal)?.Trim() : null;
                string combined    = string.Join(" ", new[] { namePart, surnamePart }.Where(p => !string.IsNullOrEmpty(p)));
                return string.IsNullOrWhiteSpace(combined) ? null : combined;
            }
            case SuggestionEntityType.SoftwareRelease:
            {
                if(values.TryGetValue(Suggestions.SoftwareReleaseSuggestionApplier.FieldTitle, out object n))
                {
                    string s = ExtractStringForValidation(n);
                    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                }
                return null;
            }
            case SuggestionEntityType.Software:
            {
                // Display the software's own name in the queue. The first-release title is
                // shown in the expanded admin diff via the prefixed first_release_title key.
                if(values.TryGetValue(Suggestions.SoftwareSuggestionApplier.FieldName, out object n))
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
            case SuggestionEntityType.Machine:
                // Machine names are not globally unique (same name can legitimately exist under
                // different companies, e.g. "Series 1" by multiple manufacturers). Dedupe on
                // name only would over-block; rely on admin moderation to spot duplicates within
                // a manufacturer instead.
                return false;
            case SuggestionEntityType.Magazine:
                // Magazine titles repeat across decades, regions and languages (e.g. "BYTE",
                // "Computer World"). Dedupe on title only would over-block; rely on admin
                // moderation to spot true duplicates instead.
                return false;
            case SuggestionEntityType.Book:
                // Book titles repeat across editions, languages and reprints (multiple
                // distinct "Programming in C" books exist for different decades/authors).
                // Dedupe on title only would over-block; rely on admin moderation to spot
                // true duplicates instead.
                return false;
            case SuggestionEntityType.Document:
                // Document titles repeat across editions, translations and reprints (e.g.
                // "User's Manual" appears for many machines / decades). Dedupe on title only
                // would over-block; rely on admin moderation to spot true duplicates instead.
                return false;
            case SuggestionEntityType.MagazineIssue:
                // Issue captions repeat across years/months (e.g. "Issue 1", "January",
                // "Annual") and even within a single magazine. Dedupe on caption only would
                // over-block; rely on admin moderation to spot true duplicates instead.
                return false;
            case SuggestionEntityType.Gpu:
                // GPU names legitimately repeat across die revisions, OEM rebrands and
                // manufacturers (e.g. "GeForce 256" exists as multiple distinct die
                // revisions). Dedupe on name only would over-block; rely on admin moderation
                // to spot true duplicates instead.
                return false;
            case SuggestionEntityType.Processor:
                // Processor names legitimately repeat across die revisions, OEM rebrands and
                // manufacturers (e.g. "Z80" by Zilog vs Z80-compatible by NEC). Dedupe on
                // name only would over-block; rely on admin moderation to spot true
                // duplicates instead.
                return false;
            case SuggestionEntityType.SoundSynth:
                // Sound synthesizer names legitimately repeat across die revisions, OEM
                // rebrands and manufacturers (e.g. "AY-3-8910" by GI vs Yamaha as YM2149,
                // "SID 6581" vs "SID 8580"). Dedupe on name only would over-block; rely on
                // admin moderation to spot true duplicates instead.
                return false;
            case SuggestionEntityType.Person:
                // Person names legitimately repeat across history, regions and eras (many
                // distinct historical figures share names like "John Smith"). Dedupe on the
                // first name alone would massively over-block; rely on admin moderation to
                // spot true duplicates by combining name+surname+birth date+context.
                return false;
            case SuggestionEntityType.SoftwareRelease:
                // Release titles legitimately repeat across regions, platforms and re-issues
                // (the SAME software release "Doom" exists for many platforms/regions/years
                // each as a distinct release). Dedupe on title only would massively
                // over-block; rely on admin moderation to spot true duplicates instead.
                return false;
            case SuggestionEntityType.Software:
                // Software names legitimately repeat across vendors, ports and unrelated
                // products of the same name (e.g. "Solitaire" exists as countless distinct
                // games over decades). Dedupe on name alone would massively over-block;
                // rely on admin moderation + the mandatory accompanying first-release
                // (publisher/platform/title) to disambiguate.
                return false;
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
            case SuggestionEntityType.Machine:
            {
                var (id, applied) = await Suggestions.MachineSuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId);
                return (id, applied);
            }
            case SuggestionEntityType.Magazine:
            {
                var (id, applied) = await Suggestions.MagazineSuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId);
                return (id, applied);
            }
            case SuggestionEntityType.Book:
            {
                var (id, applied) = await Suggestions.BookSuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId, _assetRootPath);
                return (id, applied);
            }
            case SuggestionEntityType.Document:
            {
                var (id, applied) = await Suggestions.DocumentSuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId);
                return (id, applied);
            }
            case SuggestionEntityType.MagazineIssue:
            {
                var (id, applied) = await Suggestions.MagazineIssueSuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId, _assetRootPath);
                return (id, applied);
            }
            case SuggestionEntityType.Gpu:
            {
                var (id, applied) = await Suggestions.GpuSuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId);
                return (id, applied);
            }
            case SuggestionEntityType.Processor:
            {
                var (id, applied) = await Suggestions.ProcessorSuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId);
                return (id, applied);
            }
            case SuggestionEntityType.SoundSynth:
            {
                var (id, applied) = await Suggestions.SoundSynthSuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId);
                return (id, applied);
            }
            case SuggestionEntityType.Person:
            {
                var (id, applied) = await Suggestions.PersonSuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId, _assetRootPath);
                return (id, applied);
            }
            case SuggestionEntityType.SoftwareRelease:
            {
                var (id, applied) = await Suggestions.SoftwareReleaseSuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId);
                // SoftwareRelease.Id is ulong; implicit cast to long? for the unified
                // dispatch return tuple matches the Software ulong-PK precedent.
                return ((long?)id, applied);
            }
            case SuggestionEntityType.Software:
            {
                // Software has no user-facing surface without a release, so this CreateAsync
                // orchestrates BOTH a Software AND its first SoftwareRelease in an atomic
                // EF transaction. The release fields travel on the wire prefixed with
                // "first_release_" / "first_release."; the applier strips the prefixes,
                // delegates to SoftwareReleaseSuggestionApplier.CreateAsync, then re-prefixes
                // the applied keys back so the admin diff panel sees the original wire keys.
                var (id, applied) = await Suggestions.SoftwareSuggestionApplier.CreateAsync(
                    context, suggested, accepted, creditedUserId);
                // Software.Id is ulong; explicit cast to long? — implicit conversion from
                // ulong? to long? doesn't exist in C# (per the SoftwareRelease port lesson).
                return ((long?)id, applied);
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
            case SuggestionEntityType.GpuDescription:
            {
                string langName = await context.Iso639.AsNoTracking()
                                               .Where(l => l.Id == subkey)
                                               .Select(l => l.ReferenceName)
                                               .FirstOrDefaultAsync();
                return $"({langName ?? subkey} description)";
            }
            case SuggestionEntityType.ProcessorDescription:
            {
                string langName = await context.Iso639.AsNoTracking()
                                               .Where(l => l.Id == subkey)
                                               .Select(l => l.ReferenceName)
                                               .FirstOrDefaultAsync();
                return $"({langName ?? subkey} description)";
            }
            case SuggestionEntityType.SoundSynthDescription:
            {
                string langName = await context.Iso639.AsNoTracking()
                                               .Where(l => l.Id == subkey)
                                               .Select(l => l.ReferenceName)
                                               .FirstOrDefaultAsync();
                return $"({langName ?? subkey} description)";
            }
            case SuggestionEntityType.PersonDescription:
            {
                string langName = await context.Iso639.AsNoTracking()
                                               .Where(l => l.Id == subkey)
                                               .Select(l => l.ReferenceName)
                                               .FirstOrDefaultAsync();
                return $"({langName ?? subkey} biography)";
            }
            case SuggestionEntityType.SoftwareDescription:
            {
                string langName = await context.Iso639.AsNoTracking()
                                               .Where(l => l.Id == subkey)
                                               .Select(l => l.ReferenceName)
                                               .FirstOrDefaultAsync();
                return $"({langName ?? subkey} description)";
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
    ///     entries. The <paramref name="entityId" /> is used by entity types with junction
    ///     operations (Machine) to scope row lookups; junction-add labels resolve from the
    ///     suggested payload's id field. Junction-remove labels are resolved separately by
    ///     <see cref="ResolveMachineRemoveLabelsAsync" /> so they land in the current-side
    ///     label map.
    /// </summary>
    async Task<Dictionary<string, string>> ResolveLabelsAsync(SuggestionEntityType type,
                                                              long? entityId,
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
            case SuggestionEntityType.Machine:
            {
                await ResolveMachineLabelsAsync(entityId, values, labels);
                break;
            }
            case SuggestionEntityType.Book:
            {
                await ResolveBookLabelsAsync(entityId, values, labels);
                break;
            }
            case SuggestionEntityType.Document:
            {
                await ResolveDocumentLabelsAsync(entityId, values, labels);
                break;
            }
            case SuggestionEntityType.Magazine:
            {
                await ResolveMagazineLabelsAsync(entityId, values, labels);
                break;
            }
            case SuggestionEntityType.MagazineIssue:
            {
                await ResolveMagazineIssueLabelsAsync(entityId, values, labels);
                break;
            }
            case SuggestionEntityType.Gpu:
            {
                await ResolveGpuLabelsAsync(entityId, values, labels);
                break;
            }
            case SuggestionEntityType.Processor:
            {
                await ResolveProcessorLabelsAsync(entityId, values, labels);
                break;
            }
            case SuggestionEntityType.SoundSynth:
            {
                await ResolveSoundSynthLabelsAsync(entityId, values, labels);
                break;
            }
            case SuggestionEntityType.Person:
            {
                await ResolvePersonLabelsAsync(entityId, values, labels);
                break;
            }
            case SuggestionEntityType.Software:
            {
                await ResolveSoftwareLabelsAsync(entityId, values, labels);
                break;
            }
            case SuggestionEntityType.SoftwareRelease:
            {
                await ResolveSoftwareReleaseLabelsAsync(entityId, values, labels);
                break;
            }
            case SuggestionEntityType.GpuPhoto:
            {
                if(values.TryGetValue(Suggestions.GpuPhotoSuggestionApplier.FieldLicenseId, out JsonElement lj))
                {
                    int? id = JsonElementToInt(lj);
                    if(id.HasValue)
                    {
                        string name = await context.Licenses.AsNoTracking()
                                                  .Where(l => l.Id == id.Value)
                                                  .Select(l => l.Name)
                                                  .FirstOrDefaultAsync();
                        if(!string.IsNullOrEmpty(name))
                            labels[Suggestions.GpuPhotoSuggestionApplier.FieldLicenseId] = name;
                    }
                }
                break;
            }
            case SuggestionEntityType.ProcessorPhoto:
            {
                if(values.TryGetValue(Suggestions.ProcessorPhotoSuggestionApplier.FieldLicenseId, out JsonElement lj))
                {
                    int? id = JsonElementToInt(lj);
                    if(id.HasValue)
                    {
                        string name = await context.Licenses.AsNoTracking()
                                                  .Where(l => l.Id == id.Value)
                                                  .Select(l => l.Name)
                                                  .FirstOrDefaultAsync();
                        if(!string.IsNullOrEmpty(name))
                            labels[Suggestions.ProcessorPhotoSuggestionApplier.FieldLicenseId] = name;
                    }
                }
                break;
            }
            case SuggestionEntityType.SoundSynthPhoto:
            {
                if(values.TryGetValue(Suggestions.SoundSynthPhotoSuggestionApplier.FieldLicenseId, out JsonElement lj))
                {
                    int? id = JsonElementToInt(lj);
                    if(id.HasValue)
                    {
                        string name = await context.Licenses.AsNoTracking()
                                                  .Where(l => l.Id == id.Value)
                                                  .Select(l => l.Name)
                                                  .FirstOrDefaultAsync();
                        if(!string.IsNullOrEmpty(name))
                            labels[Suggestions.SoundSynthPhotoSuggestionApplier.FieldLicenseId] = name;
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

    static long? JsonElementToLong(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.Number => e.TryGetInt64(out long i) ? i : null,
        JsonValueKind.String => long.TryParse(e.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long p) ? p : null,
        _                    => null
    };

    static double? JsonElementToDouble(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.Number => e.TryGetDouble(out double i) ? i : null,
        JsonValueKind.String => double.TryParse(e.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double p) ? p : null,
        _                    => null
    };

    static short? JsonElementToShort(JsonElement e)
    {
        int? i = JsonElementToInt(e);
        return i.HasValue && i.Value is >= short.MinValue and <= short.MaxValue ? (short)i.Value : null;
    }

    /// <summary>
    ///     Build readable labels for the heterogeneous payload of a Machine suggestion. Handles
    ///     scalar FK fields (<c>company_id</c>, <c>family_id</c>) plus junction-add operation
    ///     keys (<c>&lt;group&gt;.add.&lt;uuid&gt;</c> resolved from the suggested payload's id
    ///     field). Junction-remove labels are populated separately by
    ///     <see cref="ResolveMachineRemoveLabelsAsync" /> directly into the current-side label
    ///     map so the diff panel renders them in the strikethrough cell.
    /// </summary>
    async Task ResolveMachineLabelsAsync(long? entityId,
                                         Dictionary<string, JsonElement> values,
                                         Dictionary<string, string> labels)
    {
        // ---- Scalar FK labels -----------------------------------------------------
        if(values.TryGetValue(Suggestions.MachineSuggestionApplier.FieldCompanyId, out JsonElement compE))
        {
            int? id = JsonElementToInt(compE);
            if(id.HasValue)
            {
                string name = await context.Companies.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.MachineSuggestionApplier.FieldCompanyId] = name;
            }
        }

        if(values.TryGetValue(Suggestions.MachineSuggestionApplier.FieldFamilyId, out JsonElement famE))
        {
            int? id = JsonElementToInt(famE);
            if(id.HasValue)
            {
                string name = await context.MachineFamilies.AsNoTracking()
                                           .Where(f => f.Id == id.Value)
                                           .Select(f => f.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.MachineSuggestionApplier.FieldFamilyId] = name;
            }
        }

        // ---- Junction-add labels (resolved from the suggested payload's id field) -----
        // These do NOT depend on the parent entity existing — they look up the linked entity
        // (GPU, Processor, etc.) directly from the payload's id field, so they work for both
        // edit-mode (entityId.HasValue) AND addition-mode (entityId is null) suggestions.
        foreach(KeyValuePair<string, JsonElement> kv in values)
        {
            if(!Suggestions.MachineSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out _))
                continue;
            if(op != "add" || kv.Value.ValueKind != JsonValueKind.Object) continue;

            string label = await BuildAddLabelAsync(group, kv.Value);
            if(!string.IsNullOrEmpty(label)) labels[kv.Key] = label;
        }
    }

    /// <summary>
    ///     Resolve readable labels for every Machine junction-remove operation in the suggested
    ///     payload by looking up the existing junction row in the database. The resolved labels
    ///     are written to <paramref name="currentLabels" /> so the diff panel renders them in
    ///     the strikethrough cell of the JunctionRemove row. Iterates the RAW
    ///     <c>SuggestedValues</c> dict (not the JsonElement-parsed view) because remove ops
    ///     carry a null value and <see cref="ParseValuesForLabels" /> drops null entries.
    /// </summary>
    async Task ResolveMachineRemoveLabelsAsync(int machineId,
                                               Dictionary<string, object> rawSuggested,
                                               Dictionary<string, string> currentLabels)
    {
        foreach(KeyValuePair<string, object> kv in rawSuggested)
        {
            if(!Suggestions.MachineSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out string token))
                continue;
            if(op != "remove") continue;
            if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) continue;

            string label = await BuildRemoveLabelAsync(group, machineId, rowId);
            if(!string.IsNullOrEmpty(label)) currentLabels[kv.Key] = label;
        }
    }

    async Task<string> BuildAddLabelAsync(string group, JsonElement payload)
    {
        switch(group)
        {
            case Suggestions.MachineSuggestionApplier.GroupGpus:
            {
                int? id = ReadIntField(payload, "gpu_id");
                if(!id.HasValue) return null;
                var row = await context.Gpus.AsNoTracking()
                                       .Where(g => g.Id == id.Value)
                                       .Select(g => new { g.Name, CompanyName = g.Company.Name })
                                       .FirstOrDefaultAsync();
                return row is null ? $"GPU #{id.Value}" : $"{row.Name} ({row.CompanyName})";
            }
            case Suggestions.MachineSuggestionApplier.GroupProcessors:
            {
                int? id    = ReadIntField(payload, "processor_id");
                if(!id.HasValue) return null;
                double? sp = ReadDoubleField(payload, "speed");
                var row = await context.Processors.AsNoTracking()
                                       .Where(p => p.Id == id.Value)
                                       .Select(p => new { p.Name, CompanyName = p.Company.Name })
                                       .FirstOrDefaultAsync();
                if(row is null) return $"Processor #{id.Value}";
                return sp is > 0 ? $"{row.Name} ({row.CompanyName}) @ {sp} MHz" : $"{row.Name} ({row.CompanyName})";
            }
            case Suggestions.MachineSuggestionApplier.GroupSoundSynths:
            {
                int? id = ReadIntField(payload, "sound_synth_id");
                if(!id.HasValue) return null;
                var row = await context.SoundSynths.AsNoTracking()
                                       .Where(s => s.Id == id.Value)
                                       .Select(s => new { s.Name, CompanyName = s.Company.Name })
                                       .FirstOrDefaultAsync();
                return row is null ? $"Sound synth #{id.Value}" : $"{row.Name} ({row.CompanyName})";
            }
            case Suggestions.MachineSuggestionApplier.GroupScreens:
            {
                int? id = ReadIntField(payload, "screen_id");
                if(!id.HasValue) return null;
                var row = await context.Screens.AsNoTracking()
                                       .Where(s => s.Id == id.Value)
                                       .Select(s => new { s.Diagonal, s.Type })
                                       .FirstOrDefaultAsync();
                if(row is null) return $"Screen #{id.Value}";
                return string.IsNullOrWhiteSpace(row.Type) ? $"{row.Diagonal}\"" : $"{row.Diagonal}\" {row.Type}";
            }
            case Suggestions.MachineSuggestionApplier.GroupMemory:
            {
                int?    type  = ReadIntField(payload, "type");
                int?    usage = ReadIntField(payload, "usage");
                long?   size  = ReadLongField(payload, "size");
                double? speed = ReadDoubleField(payload, "speed");
                string typeName  = type.HasValue && Enum.IsDefined(typeof(MemoryType), type.Value)
                                       ? ((MemoryType)type.Value).ToString()
                                       : "Unknown";
                string usageName = usage.HasValue && Enum.IsDefined(typeof(MemoryUsage), usage.Value)
                                       ? ((MemoryUsage)usage.Value).ToString()
                                       : "Unknown";
                string sizeStr = size.HasValue ? FormatBytesShort(size.Value) : "Unknown";
                string speedStr = speed.HasValue ? $" @ {speed} MHz" : string.Empty;
                return $"{sizeStr} {typeName} ({usageName}){speedStr}";
            }
            case Suggestions.MachineSuggestionApplier.GroupStorage:
            {
                int?  type     = ReadIntField(payload, "type");
                int?  iface    = ReadIntField(payload, "interface");
                long? capacity = ReadLongField(payload, "capacity");
                string typeName  = type.HasValue && Enum.IsDefined(typeof(StorageType), type.Value)
                                       ? ((StorageType)type.Value).ToString()
                                       : "Unknown";
                string ifaceName = iface.HasValue && Enum.IsDefined(typeof(StorageInterface), iface.Value)
                                       ? ((StorageInterface)iface.Value).ToString()
                                       : "Unknown";
                string capStr = capacity.HasValue ? $" - {FormatBytesShort(capacity.Value)}" : string.Empty;
                return $"{typeName} ({ifaceName}){capStr}";
            }
            case Suggestions.MachineSuggestionApplier.GroupSoftwarePlatforms:
            {
                long? rawId = ReadLongField(payload, "software_platform_id");
                if(!rawId.HasValue || rawId.Value < 0) return null;
                ulong id = (ulong)rawId.Value;
                string name = await context.SoftwarePlatforms.AsNoTracking()
                                           .Where(p => p.Id == id)
                                           .Select(p => p.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Software platform #{id}" : name;
            }
            default:
                return null;
        }
    }

    async Task<string> BuildRemoveLabelAsync(string group, int machineId, long rowId)
    {
        switch(group)
        {
            case Suggestions.MachineSuggestionApplier.GroupGpus:
            {
                var row = await context.GpusByMachine.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MachineId == machineId)
                                       .Select(r => new { r.Gpu.Name, CompanyName = r.Gpu.Company.Name })
                                       .FirstOrDefaultAsync();
                return row is null ? null : $"{row.Name} ({row.CompanyName})";
            }
            case Suggestions.MachineSuggestionApplier.GroupProcessors:
            {
                var row = await context.ProcessorsByMachine.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MachineId == machineId)
                                       .Select(r => new { r.Processor.Name, CompanyName = r.Processor.Company.Name, r.Speed })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                return row.Speed is > 0 ? $"{row.Name} ({row.CompanyName}) @ {row.Speed} MHz" : $"{row.Name} ({row.CompanyName})";
            }
            case Suggestions.MachineSuggestionApplier.GroupSoundSynths:
            {
                var row = await context.SoundByMachine.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MachineId == machineId)
                                       .Select(r => new { r.SoundSynth.Name, CompanyName = r.SoundSynth.Company.Name })
                                       .FirstOrDefaultAsync();
                return row is null ? null : $"{row.Name} ({row.CompanyName})";
            }
            case Suggestions.MachineSuggestionApplier.GroupScreens:
            {
                var row = await context.ScreensByMachine.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MachineId == machineId)
                                       .Select(r => new { r.Screen.Diagonal, r.Screen.Type })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                return string.IsNullOrWhiteSpace(row.Type) ? $"{row.Diagonal}\"" : $"{row.Diagonal}\" {row.Type}";
            }
            case Suggestions.MachineSuggestionApplier.GroupMemory:
            {
                var row = await context.MemoryByMachine.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MachineId == machineId)
                                       .Select(r => new { r.Type, r.Usage, r.Size, r.Speed })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                string sizeStr = row.Size.HasValue ? FormatBytesShort(row.Size.Value) : "Unknown";
                string speedStr = row.Speed.HasValue ? $" @ {row.Speed} MHz" : string.Empty;
                return $"{sizeStr} {row.Type} ({row.Usage}){speedStr}";
            }
            case Suggestions.MachineSuggestionApplier.GroupStorage:
            {
                var row = await context.StorageByMachine.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MachineId == machineId)
                                       .Select(r => new { r.Type, r.Interface, r.Capacity })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                string capStr = row.Capacity.HasValue ? $" - {FormatBytesShort(row.Capacity.Value)}" : string.Empty;
                return $"{row.Type} ({row.Interface}){capStr}";
            }
            case Suggestions.MachineSuggestionApplier.GroupSoftwarePlatforms:
            {
                var row = await context.SoftwarePlatformsByMachine.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MachineId == machineId)
                                       .Select(r => new { Name = r.SoftwarePlatform.Name })
                                       .FirstOrDefaultAsync();
                return row is null ? null : row.Name;
            }
            default:
                return null;
        }
    }

    static int? ReadIntField(JsonElement obj, string key) =>
        obj.TryGetProperty(key, out JsonElement v) ? JsonElementToInt(v) : null;

    static long? ReadLongField(JsonElement obj, string key) =>
        obj.TryGetProperty(key, out JsonElement v) ? JsonElementToLong(v) : null;

    static double? ReadDoubleField(JsonElement obj, string key) =>
        obj.TryGetProperty(key, out JsonElement v) ? JsonElementToDouble(v) : null;

    static string FormatBytesShort(long bytes) => bytes switch
    {
        >= 1073741824 => $"{bytes / 1073741824.0:F1} GiB",
        >= 1048576    => $"{bytes / 1048576.0:F1} MiB",
        >= 1024       => $"{bytes / 1024.0:F1} KiB",
        _             => $"{bytes} B"
    };

    static string ReadStringField(JsonElement obj, string key) =>
        obj.TryGetProperty(key, out JsonElement v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    // ─────────────── Book entity-edit label resolution (scalars + junction ops) ───────────────

    /// <summary>
    ///     Build readable labels for the heterogeneous payload of a Book suggestion. Handles
    ///     scalar FK fields (<c>country_id</c>, <c>previous_id</c>, <c>source_id</c>) plus
    ///     junction-add operation keys (<c>&lt;group&gt;.add.&lt;uuid&gt;</c> resolved from the
    ///     suggested payload's id field). Junction-remove labels are populated separately by
    ///     <see cref="ResolveBookRemoveLabelsAsync" /> directly into the current-side label
    ///     map so the diff panel renders them in the strikethrough cell.
    /// </summary>
    async Task ResolveBookLabelsAsync(long? entityId,
                                      Dictionary<string, JsonElement> values,
                                      Dictionary<string, string> labels)
    {
        // ---- Scalar FK labels -----------------------------------------------------
        if(values.TryGetValue(Suggestions.BookSuggestionApplier.FieldCountryId, out JsonElement countryE))
        {
            short? id = JsonElementToShort(countryE);
            if(id.HasValue)
            {
                string name = await context.Iso31661Numeric.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.BookSuggestionApplier.FieldCountryId] = name;
            }
        }

        if(values.TryGetValue(Suggestions.BookSuggestionApplier.FieldPreviousId, out JsonElement prevE))
        {
            long? id = JsonElementToLong(prevE);
            if(id.HasValue)
            {
                string title = await context.Books.AsNoTracking()
                                            .Where(b => b.Id == id.Value)
                                            .Select(b => b.Title)
                                            .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(title))
                    labels[Suggestions.BookSuggestionApplier.FieldPreviousId] = title;
            }
        }

        if(values.TryGetValue(Suggestions.BookSuggestionApplier.FieldSourceId, out JsonElement srcE))
        {
            long? id = JsonElementToLong(srcE);
            if(id.HasValue)
            {
                string title = await context.Books.AsNoTracking()
                                            .Where(b => b.Id == id.Value)
                                            .Select(b => b.Title)
                                            .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(title))
                    labels[Suggestions.BookSuggestionApplier.FieldSourceId] = title;
            }
        }

        // ---- Junction-add labels (resolved from the suggested payload's id field) -----
        // NB: junction-add labels do NOT need the parent entityId — they look up the
        // linked entity (Person, Company, Machine, MachineFamily) directly from the
        // payload's id field. Removing the previous `if(!entityId.HasValue) return;`
        // guard so addition-mode (entityId=null) suggestions also get readable labels in
        // the diff panel.
        foreach(KeyValuePair<string, JsonElement> kv in values)
        {
            if(!Suggestions.BookSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out _))
                continue;
            if(op != "add" || kv.Value.ValueKind != JsonValueKind.Object) continue;

            string label = await BuildBookAddLabelAsync(group, kv.Value);
            if(!string.IsNullOrEmpty(label)) labels[kv.Key] = label;
        }
    }

    /// <summary>
    ///     Resolve readable labels for every Book junction-remove operation in the suggested
    ///     payload by looking up the existing junction row in the database. The resolved labels
    ///     are written to <paramref name="currentLabels" /> so the diff panel renders them in
    ///     the strikethrough cell of the JunctionRemove row. Iterates the RAW
    ///     <c>SuggestedValues</c> dict (not the JsonElement-parsed view) because remove ops
    ///     carry a null value and <see cref="ParseValuesForLabels" /> drops null entries.
    /// </summary>
    async Task ResolveBookRemoveLabelsAsync(long bookId,
                                            Dictionary<string, object> rawSuggested,
                                            Dictionary<string, string> currentLabels)
    {
        foreach(KeyValuePair<string, object> kv in rawSuggested)
        {
            if(!Suggestions.BookSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out string token))
                continue;
            if(op != "remove") continue;
            if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) continue;

            string label = await BuildBookRemoveLabelAsync(group, bookId, rowId);
            if(!string.IsNullOrEmpty(label)) currentLabels[kv.Key] = label;
        }
    }

    async Task<string> BuildBookAddLabelAsync(string group, JsonElement payload)
    {
        switch(group)
        {
            case Suggestions.BookSuggestionApplier.GroupPeople:
            {
                int?   id     = ReadIntField(payload, "person_id");
                string roleId = ReadStringField(payload, "role_id");
                if(!id.HasValue) return null;
                var row = await context.People.AsNoTracking()
                                       .Where(p => p.Id == id.Value)
                                       .Select(p => new { Display = p.DisplayName ?? p.Alias ?? (p.Name + " " + p.Surname) })
                                       .FirstOrDefaultAsync();
                string display = row is null ? $"Person #{id.Value}" : row.Display;
                if(!string.IsNullOrEmpty(roleId))
                {
                    string roleName = await context.DocumentRoles.AsNoTracking()
                                                   .Where(r => r.Id == roleId)
                                                   .Select(r => r.Name)
                                                   .FirstOrDefaultAsync();
                    if(!string.IsNullOrEmpty(roleName))
                        display = $"{display} ({roleName})";
                }
                return display;
            }
            case Suggestions.BookSuggestionApplier.GroupCompanies:
            {
                int?   id     = ReadIntField(payload, "company_id");
                string roleId = ReadStringField(payload, "role_id");
                if(!id.HasValue) return null;
                string name = await context.Companies.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                string display = string.IsNullOrEmpty(name) ? $"Company #{id.Value}" : name;
                if(!string.IsNullOrEmpty(roleId))
                {
                    string roleName = await context.DocumentRoles.AsNoTracking()
                                                   .Where(r => r.Id == roleId)
                                                   .Select(r => r.Name)
                                                   .FirstOrDefaultAsync();
                    if(!string.IsNullOrEmpty(roleName))
                        display = $"{display} ({roleName})";
                }
                return display;
            }
            case Suggestions.BookSuggestionApplier.GroupMachines:
            {
                int? id = ReadIntField(payload, "machine_id");
                if(!id.HasValue) return null;
                string name = await context.Machines.AsNoTracking()
                                           .Where(m => m.Id == id.Value)
                                           .Select(m => m.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Machine #{id.Value}" : name;
            }
            case Suggestions.BookSuggestionApplier.GroupMachineFamilies:
            {
                int? id = ReadIntField(payload, "machine_family_id");
                if(!id.HasValue) return null;
                string name = await context.MachineFamilies.AsNoTracking()
                                           .Where(f => f.Id == id.Value)
                                           .Select(f => f.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Machine family #{id.Value}" : name;
            }
            default:
                return null;
        }
    }

    async Task<string> BuildBookRemoveLabelAsync(string group, long bookId, long rowId)
    {
        switch(group)
        {
            case Suggestions.BookSuggestionApplier.GroupPeople:
            {
                var row = await context.PeopleByBooks.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.BookId == bookId)
                                       .Select(r => new
                                       {
                                           Display  = r.Person.DisplayName ?? r.Person.Alias ?? (r.Person.Name + " " + r.Person.Surname),
                                           RoleName = r.Role.Name
                                       })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                return string.IsNullOrEmpty(row.RoleName) ? row.Display : $"{row.Display} ({row.RoleName})";
            }
            case Suggestions.BookSuggestionApplier.GroupCompanies:
            {
                var row = await context.CompaniesByBooks.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.BookId == bookId)
                                       .Select(r => new { r.Company.Name, RoleName = r.Role.Name })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                return string.IsNullOrEmpty(row.RoleName) ? row.Name : $"{row.Name} ({row.RoleName})";
            }
            case Suggestions.BookSuggestionApplier.GroupMachines:
            {
                var row = await context.BooksByMachines.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.BookId == bookId)
                                       .Select(r => new { r.Machine.Name })
                                       .FirstOrDefaultAsync();
                return row?.Name;
            }
            case Suggestions.BookSuggestionApplier.GroupMachineFamilies:
            {
                var row = await context.BooksByMachineFamilies.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.BookId == bookId)
                                       .Select(r => new { r.MachineFamily.Name })
                                       .FirstOrDefaultAsync();
                return row?.Name;
            }
            default:
                return null;
        }
    }

    /// <summary>
    ///     Build readable labels for the heterogeneous payload of a Document suggestion. Handles
    ///     the scalar FK field (<c>country_id</c>) plus junction-add operation keys
    ///     (<c>&lt;group&gt;.add.&lt;uuid&gt;</c> resolved from the suggested payload's id field).
    ///     Junction-remove labels are populated separately by
    ///     <see cref="ResolveDocumentRemoveLabelsAsync" /> directly into the current-side label
    ///     map so the diff panel renders them in the strikethrough cell.
    /// </summary>
    async Task ResolveDocumentLabelsAsync(long? entityId,
                                          Dictionary<string, JsonElement> values,
                                          Dictionary<string, string> labels)
    {
        // ---- Scalar FK label ----------------------------------------------------------
        if(values.TryGetValue(Suggestions.DocumentSuggestionApplier.FieldCountryId, out JsonElement countryE))
        {
            short? id = JsonElementToShort(countryE);
            if(id.HasValue)
            {
                string name = await context.Iso31661Numeric.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.DocumentSuggestionApplier.FieldCountryId] = name;
            }
        }

        // NOTE: do NOT early-return on `!entityId.HasValue`. Junction-add labels look up
        // the linked entity (Person/Company/Machine/MachineFamily) directly from the
        // suggested payload's id field, so they're resolvable even in addition mode
        // (entityId == null). Junction-remove labels DO need a parent id and are gated
        // separately at the call site of ResolveDocumentRemoveLabelsAsync.

        // ---- Junction-add labels (resolved from the suggested payload's id field) -----
        foreach(KeyValuePair<string, JsonElement> kv in values)
        {
            if(!Suggestions.DocumentSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out _))
                continue;
            if(op != "add" || kv.Value.ValueKind != JsonValueKind.Object) continue;

            string label = await BuildDocumentAddLabelAsync(group, kv.Value);
            if(!string.IsNullOrEmpty(label)) labels[kv.Key] = label;
        }
    }

    /// <summary>
    ///     Resolve readable labels for every Document junction-remove operation in the
    ///     suggested payload by looking up the existing junction row in the database. The
    ///     resolved labels are written to <paramref name="currentLabels" /> so the diff panel
    ///     renders them in the strikethrough cell of the JunctionRemove row. Iterates the RAW
    ///     <c>SuggestedValues</c> dict (not the JsonElement-parsed view) because remove ops
    ///     carry a null value and <see cref="ParseValuesForLabels" /> drops null entries.
    /// </summary>
    async Task ResolveDocumentRemoveLabelsAsync(long documentId,
                                                Dictionary<string, object> rawSuggested,
                                                Dictionary<string, string> currentLabels)
    {
        foreach(KeyValuePair<string, object> kv in rawSuggested)
        {
            if(!Suggestions.DocumentSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out string token))
                continue;
            if(op != "remove") continue;
            if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) continue;

            string label = await BuildDocumentRemoveLabelAsync(group, documentId, rowId);
            if(!string.IsNullOrEmpty(label)) currentLabels[kv.Key] = label;
        }
    }

    async Task<string> BuildDocumentAddLabelAsync(string group, JsonElement payload)
    {
        switch(group)
        {
            case Suggestions.DocumentSuggestionApplier.GroupPeople:
            {
                int?   id     = ReadIntField(payload, "person_id");
                string roleId = ReadStringField(payload, "role_id");
                if(!id.HasValue) return null;
                var row = await context.People.AsNoTracking()
                                       .Where(p => p.Id == id.Value)
                                       .Select(p => new { Display = p.DisplayName ?? p.Alias ?? (p.Name + " " + p.Surname) })
                                       .FirstOrDefaultAsync();
                string display = row is null ? $"Person #{id.Value}" : row.Display;
                if(!string.IsNullOrEmpty(roleId))
                {
                    string roleName = await context.DocumentRoles.AsNoTracking()
                                                   .Where(r => r.Id == roleId)
                                                   .Select(r => r.Name)
                                                   .FirstOrDefaultAsync();
                    if(!string.IsNullOrEmpty(roleName))
                        display = $"{display} ({roleName})";
                }
                return display;
            }
            case Suggestions.DocumentSuggestionApplier.GroupCompanies:
            {
                int?   id     = ReadIntField(payload, "company_id");
                string roleId = ReadStringField(payload, "role_id");
                if(!id.HasValue) return null;
                string name = await context.Companies.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                string display = string.IsNullOrEmpty(name) ? $"Company #{id.Value}" : name;
                if(!string.IsNullOrEmpty(roleId))
                {
                    string roleName = await context.DocumentRoles.AsNoTracking()
                                                   .Where(r => r.Id == roleId)
                                                   .Select(r => r.Name)
                                                   .FirstOrDefaultAsync();
                    if(!string.IsNullOrEmpty(roleName))
                        display = $"{display} ({roleName})";
                }
                return display;
            }
            case Suggestions.DocumentSuggestionApplier.GroupMachines:
            {
                int? id = ReadIntField(payload, "machine_id");
                if(!id.HasValue) return null;
                string name = await context.Machines.AsNoTracking()
                                           .Where(m => m.Id == id.Value)
                                           .Select(m => m.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Machine #{id.Value}" : name;
            }
            case Suggestions.DocumentSuggestionApplier.GroupMachineFamilies:
            {
                int? id = ReadIntField(payload, "machine_family_id");
                if(!id.HasValue) return null;
                string name = await context.MachineFamilies.AsNoTracking()
                                           .Where(f => f.Id == id.Value)
                                           .Select(f => f.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Machine family #{id.Value}" : name;
            }
            default:
                return null;
        }
    }

    async Task<string> BuildDocumentRemoveLabelAsync(string group, long documentId, long rowId)
    {
        switch(group)
        {
            case Suggestions.DocumentSuggestionApplier.GroupPeople:
            {
                var row = await context.PeopleByDocuments.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.DocumentId == documentId)
                                       .Select(r => new
                                       {
                                           Display  = r.Person.DisplayName ?? r.Person.Alias ?? (r.Person.Name + " " + r.Person.Surname),
                                           RoleName = r.Role.Name
                                       })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                return string.IsNullOrEmpty(row.RoleName) ? row.Display : $"{row.Display} ({row.RoleName})";
            }
            case Suggestions.DocumentSuggestionApplier.GroupCompanies:
            {
                var row = await context.CompaniesByDocuments.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.DocumentId == documentId)
                                       .Select(r => new { r.Company.Name, RoleName = r.Role.Name })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                return string.IsNullOrEmpty(row.RoleName) ? row.Name : $"{row.Name} ({row.RoleName})";
            }
            case Suggestions.DocumentSuggestionApplier.GroupMachines:
            {
                var row = await context.DocumentsByMachines.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.DocumentId == documentId)
                                       .Select(r => new { r.Machine.Name })
                                       .FirstOrDefaultAsync();
                return row?.Name;
            }
            case Suggestions.DocumentSuggestionApplier.GroupMachineFamilies:
            {
                var row = await context.DocumentsByMachineFamilies.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.DocumentId == documentId)
                                       .Select(r => new { r.MachineFamily.Name })
                                       .FirstOrDefaultAsync();
                return row?.Name;
            }
            default:
                return null;
        }
    }

    /// <summary>
    ///     Build readable labels for the heterogeneous payload of a Magazine suggestion. Handles
    ///     the scalar FK field (<c>country_id</c>) plus junction-add operation keys
    ///     (<c>companies.add.&lt;uuid&gt;</c> resolved from the suggested payload's id field).
    ///     Junction-remove labels are populated separately by
    ///     <see cref="ResolveMagazineRemoveLabelsAsync" /> directly into the current-side label
    ///     map so the diff panel renders them in the strikethrough cell.
    /// </summary>
    async Task ResolveMagazineLabelsAsync(long? entityId,
                                          Dictionary<string, JsonElement> values,
                                          Dictionary<string, string> labels)
    {
        // ---- Scalar FK label ----------------------------------------------------------
        if(values.TryGetValue(Suggestions.MagazineSuggestionApplier.FieldCountryId, out JsonElement countryE))
        {
            short? id = JsonElementToShort(countryE);
            if(id.HasValue)
            {
                string name = await context.Iso31661Numeric.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.MagazineSuggestionApplier.FieldCountryId] = name;
            }
        }

        // ---- Junction-add labels (resolved from the suggested payload's id field) -----
        // Note: junction-add labels look up the linked entity (Company, Role) directly from
        // the suggested payload's id fields, so they DO NOT require a parent entityId. Don't
        // gate this loop on entityId.HasValue — that breaks creation-mode review where the
        // parent magazine doesn't exist yet but the queued company adds still need labels.
        foreach(KeyValuePair<string, JsonElement> kv in values)
        {
            if(!Suggestions.MagazineSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out _))
                continue;
            if(op != "add" || kv.Value.ValueKind != JsonValueKind.Object) continue;

            string label = await BuildMagazineAddLabelAsync(group, kv.Value);
            if(!string.IsNullOrEmpty(label)) labels[kv.Key] = label;
        }
    }

    /// <summary>
    ///     Resolve readable labels for every Magazine junction-remove operation in the
    ///     suggested payload by looking up the existing junction row in the database. The
    ///     resolved labels are written to <paramref name="currentLabels" /> so the diff panel
    ///     renders them in the strikethrough cell of the JunctionRemove row. Iterates the RAW
    ///     <c>SuggestedValues</c> dict (not the JsonElement-parsed view) because remove ops
    ///     carry a null value and <see cref="ParseValuesForLabels" /> drops null entries.
    /// </summary>
    async Task ResolveMagazineRemoveLabelsAsync(long magazineId,
                                                Dictionary<string, object> rawSuggested,
                                                Dictionary<string, string> currentLabels)
    {
        foreach(KeyValuePair<string, object> kv in rawSuggested)
        {
            if(!Suggestions.MagazineSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out string token))
                continue;
            if(op != "remove") continue;
            if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) continue;

            string label = await BuildMagazineRemoveLabelAsync(group, magazineId, rowId);
            if(!string.IsNullOrEmpty(label)) currentLabels[kv.Key] = label;
        }
    }

    async Task<string> BuildMagazineAddLabelAsync(string group, JsonElement payload)
    {
        switch(group)
        {
            case Suggestions.MagazineSuggestionApplier.GroupCompanies:
            {
                int?   id     = ReadIntField(payload, "company_id");
                string roleId = ReadStringField(payload, "role_id");
                if(!id.HasValue) return null;
                string name = await context.Companies.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                string display = string.IsNullOrEmpty(name) ? $"Company #{id.Value}" : name;
                if(!string.IsNullOrEmpty(roleId))
                {
                    string roleName = await context.DocumentRoles.AsNoTracking()
                                                   .Where(r => r.Id == roleId)
                                                   .Select(r => r.Name)
                                                   .FirstOrDefaultAsync();
                    if(!string.IsNullOrEmpty(roleName))
                        display = $"{display} ({roleName})";
                }
                return display;
            }
            default:
                return null;
        }
    }

    async Task<string> BuildMagazineRemoveLabelAsync(string group, long magazineId, long rowId)
    {
        switch(group)
        {
            case Suggestions.MagazineSuggestionApplier.GroupCompanies:
            {
                var row = await context.CompaniesByMagazines.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MagazineId == magazineId)
                                       .Select(r => new { r.Company.Name, RoleName = r.Role.Name })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                return string.IsNullOrEmpty(row.RoleName) ? row.Name : $"{row.Name} ({row.RoleName})";
            }
            default:
                return null;
        }
    }

    /// <summary>
    ///     Build readable labels for a MagazineIssue suggestion's heterogeneous payload. The
    ///     issue itself has NO scalar FK fields exposed in the suggestion surface (the parent
    ///     <c>magazine_id</c> is admin-only re-parenting), so this only handles the four
    ///     junction-add operations. Junction-remove labels are populated separately by
    ///     <see cref="ResolveMagazineIssueRemoveLabelsAsync" />.
    /// </summary>
    async Task ResolveMagazineIssueLabelsAsync(long? entityId,
                                               Dictionary<string, JsonElement> values,
                                               Dictionary<string, string> labels)
    {
        // NOTE: do NOT early-return on `!entityId.HasValue` — junction-add labels in addition
        // mode (entityId == null, brand-new issue) look up the linked entity (Person, Machine,
        // MachineFamily, Software) directly from the suggested payload's id field, so they
        // don't need the parent issue id. Only the junction-remove label resolver needs an
        // existing issue id (no rows to remove from a brand-new issue) and is gated
        // separately at the call site.
        _ = entityId;

        foreach(KeyValuePair<string, JsonElement> kv in values)
        {
            if(!Suggestions.MagazineIssueSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out _))
                continue;
            if(op != "add" || kv.Value.ValueKind != JsonValueKind.Object) continue;

            string label = await BuildMagazineIssueAddLabelAsync(group, kv.Value);
            if(!string.IsNullOrEmpty(label)) labels[kv.Key] = label;
        }
    }

    /// <summary>
    ///     Resolve readable labels for every MagazineIssue junction-remove operation in the
    ///     suggested payload by looking up the existing junction row in the database. The
    ///     resolved labels are written to <paramref name="currentLabels" /> so the diff panel
    ///     renders them in the strikethrough cell of the JunctionRemove row. Iterates the RAW
    ///     <c>SuggestedValues</c> dict (not the JsonElement-parsed view) because remove ops
    ///     carry a null value and <see cref="ParseValuesForLabels" /> drops null entries.
    /// </summary>
    async Task ResolveMagazineIssueRemoveLabelsAsync(long issueId,
                                                     Dictionary<string, object> rawSuggested,
                                                     Dictionary<string, string> currentLabels)
    {
        foreach(KeyValuePair<string, object> kv in rawSuggested)
        {
            if(!Suggestions.MagazineIssueSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out string token))
                continue;
            if(op != "remove") continue;
            if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) continue;

            string label = await BuildMagazineIssueRemoveLabelAsync(group, issueId, rowId);
            if(!string.IsNullOrEmpty(label)) currentLabels[kv.Key] = label;
        }
    }

    async Task<string> BuildMagazineIssueAddLabelAsync(string group, JsonElement payload)
    {
        switch(group)
        {
            case Suggestions.MagazineIssueSuggestionApplier.GroupPeople:
            {
                int?   id     = ReadIntField(payload, "person_id");
                string roleId = ReadStringField(payload, "role_id");
                if(!id.HasValue) return null;
                var row = await context.People.AsNoTracking()
                                       .Where(p => p.Id == id.Value)
                                       .Select(p => new { Display = p.DisplayName ?? p.Alias ?? (p.Name + " " + p.Surname) })
                                       .FirstOrDefaultAsync();
                string display = row is null ? $"Person #{id.Value}" : row.Display;
                if(!string.IsNullOrEmpty(roleId))
                {
                    string roleName = await context.DocumentRoles.AsNoTracking()
                                                   .Where(r => r.Id == roleId)
                                                   .Select(r => r.Name)
                                                   .FirstOrDefaultAsync();
                    if(!string.IsNullOrEmpty(roleName))
                        display = $"{display} ({roleName})";
                }
                return display;
            }
            case Suggestions.MagazineIssueSuggestionApplier.GroupMachines:
            {
                int? id = ReadIntField(payload, "machine_id");
                if(!id.HasValue) return null;
                string name = await context.Machines.AsNoTracking()
                                           .Where(m => m.Id == id.Value)
                                           .Select(m => m.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Machine #{id.Value}" : name;
            }
            case Suggestions.MagazineIssueSuggestionApplier.GroupMachineFamilies:
            {
                int? id = ReadIntField(payload, "machine_family_id");
                if(!id.HasValue) return null;
                string name = await context.MachineFamilies.AsNoTracking()
                                           .Where(f => f.Id == id.Value)
                                           .Select(f => f.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Machine family #{id.Value}" : name;
            }
            case Suggestions.MagazineIssueSuggestionApplier.GroupSoftware:
            {
                long? id = ReadLongField(payload, "software_id");
                if(!id.HasValue || id.Value < 0) return null;
                ulong sidU = (ulong)id.Value;
                string name = await context.Softwares.AsNoTracking()
                                           .Where(s => s.Id == sidU)
                                           .Select(s => s.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Software #{id.Value}" : name;
            }
            default:
                return null;
        }
    }

    async Task<string> BuildMagazineIssueRemoveLabelAsync(string group, long issueId, long rowId)
    {
        switch(group)
        {
            case Suggestions.MagazineIssueSuggestionApplier.GroupPeople:
            {
                var row = await context.PeopleByMagazines.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MagazineId == issueId)
                                       .Select(r => new
                                       {
                                           Display  = r.Person.DisplayName ?? r.Person.Alias ?? (r.Person.Name + " " + r.Person.Surname),
                                           RoleName = r.Role.Name
                                       })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                return string.IsNullOrEmpty(row.RoleName) ? row.Display : $"{row.Display} ({row.RoleName})";
            }
            case Suggestions.MagazineIssueSuggestionApplier.GroupMachines:
            {
                var row = await context.MagazinesByMachines.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MagazineId == issueId)
                                       .Select(r => new { r.Machine.Name })
                                       .FirstOrDefaultAsync();
                return row?.Name;
            }
            case Suggestions.MagazineIssueSuggestionApplier.GroupMachineFamilies:
            {
                var row = await context.MagazinesByMachinesFamilies.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MagazineId == issueId)
                                       .Select(r => new { r.MachineFamily.Name })
                                       .FirstOrDefaultAsync();
                return row?.Name;
            }
            case Suggestions.MagazineIssueSuggestionApplier.GroupSoftware:
            {
                var row = await context.MagazinesBySoftware.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.MagazineId == issueId)
                                       .Select(r => new { r.Software.Name })
                                       .FirstOrDefaultAsync();
                return row?.Name;
            }
            default:
                return null;
        }
    }

    // ─────────────── Gpu entity-edit label resolution (scalars + junction ops) ───────────────

    /// <summary>
    ///     Build readable labels for the heterogeneous payload of a Gpu suggestion. Handles
    ///     the scalar FK field (<c>company_id</c>) plus junction-add operation keys
    ///     (<c>resolutions.add.&lt;uuid&gt;</c> resolved from the suggested payload's
    ///     <c>resolution_id</c>). Junction-remove labels are populated separately by
    ///     <see cref="ResolveGpuRemoveLabelsAsync" />.
    /// </summary>
    async Task ResolveGpuLabelsAsync(long? entityId,
                                     Dictionary<string, JsonElement> values,
                                     Dictionary<string, string> labels)
    {
        // ---- Scalar FK label ----------------------------------------------------------
        if(values.TryGetValue(Suggestions.GpuSuggestionApplier.FieldCompanyId, out JsonElement companyE))
        {
            int? id = JsonElementToInt(companyE);
            if(id.HasValue)
            {
                string name = await context.Companies.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.GpuSuggestionApplier.FieldCompanyId] = name;
            }
        }

        // NOTE: do NOT early-return on `!entityId.HasValue`. Junction-add labels look up the
        // linked entity (Resolution) directly from the suggested payload's `resolution_id`
        // field — they don't need the parent gpuId. The companion remove-label resolver is
        // gated separately at its call site (`if(s.EntityId.HasValue) await ResolveXxxRemoveLabelsAsync(...)`).
        _ = entityId;

        // ---- Junction-add labels (resolved from the suggested payload's id field) -----
        foreach(KeyValuePair<string, JsonElement> kv in values)
        {
            if(!Suggestions.GpuSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out _))
                continue;
            if(op != "add" || kv.Value.ValueKind != JsonValueKind.Object) continue;

            string label = await BuildGpuAddLabelAsync(group, kv.Value);
            if(!string.IsNullOrEmpty(label)) labels[kv.Key] = label;
        }
    }

    /// <summary>
    ///     Resolve readable labels for every Gpu junction-remove operation in the suggested
    ///     payload by looking up the existing junction row in the database. The resolved
    ///     labels are written to <paramref name="currentLabels" /> so the diff panel renders
    ///     them in the strikethrough cell of the JunctionRemove row. Iterates the RAW
    ///     <c>SuggestedValues</c> dict (not the JsonElement-parsed view) because remove ops
    ///     carry a null value and <see cref="ParseValuesForLabels" /> drops null entries.
    /// </summary>
    async Task ResolveGpuRemoveLabelsAsync(long gpuId,
                                           Dictionary<string, object> rawSuggested,
                                           Dictionary<string, string> currentLabels)
    {
        foreach(KeyValuePair<string, object> kv in rawSuggested)
        {
            if(!Suggestions.GpuSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out string token))
                continue;
            if(op != "remove") continue;
            if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) continue;

            string label = await BuildGpuRemoveLabelAsync(group, (int)gpuId, rowId);
            if(!string.IsNullOrEmpty(label)) currentLabels[kv.Key] = label;
        }
    }

    async Task<string> BuildGpuAddLabelAsync(string group, JsonElement payload)
    {
        switch(group)
        {
            case Suggestions.GpuSuggestionApplier.GroupResolutions:
            {
                int? id = ReadIntField(payload, "resolution_id");
                if(!id.HasValue) return null;
                var res = await context.Resolutions.AsNoTracking()
                                       .Where(r => r.Id == id.Value)
                                       .Select(r => new { r.Width, r.Height, r.Colors, r.Palette, r.Chars, r.Grayscale })
                                       .FirstOrDefaultAsync();
                if(res is null) return $"Resolution #{id.Value}";
                return FormatResolutionLabel(res.Width, res.Height, res.Colors, res.Palette, res.Chars, res.Grayscale);
            }
            default:
                return null;
        }
    }

    async Task<string> BuildGpuRemoveLabelAsync(string group, int gpuId, long rowId)
    {
        switch(group)
        {
            case Suggestions.GpuSuggestionApplier.GroupResolutions:
            {
                var row = await context.ResolutionsByGpu.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.GpuId == gpuId)
                                       .Select(r => new
                                       {
                                           r.Resolution.Width,
                                           r.Resolution.Height,
                                           r.Resolution.Colors,
                                           r.Resolution.Palette,
                                           r.Resolution.Chars,
                                           r.Resolution.Grayscale
                                       })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                return FormatResolutionLabel(row.Width, row.Height, row.Colors, row.Palette, row.Chars, row.Grayscale);
            }
            default:
                return null;
        }
    }

    /// <summary>
    ///     Server-side label format for a Resolution row. Mirrors the broad shape of the
    ///     client's <see cref="Marechai.Data.Dtos.ResolutionDto.ToString" /> without taking a
    ///     hard dependency on the DTO from the controller layer.
    /// </summary>
    static string FormatResolutionLabel(int width, int height, long? colors, long? palette, bool chars, bool grayscale)
    {
        string dims = $"{width}x{height}";
        if(chars)
        {
            if(colors is null) return $"{dims} characters";
            if(palette is not null && colors != palette)
                return grayscale
                           ? $"{dims} characters with {colors} grays from {palette} palette"
                           : $"{dims} characters with {colors} colors from {palette} palette";
            return grayscale ? $"{dims} characters with {colors} grays" : $"{dims} characters with {colors} colors";
        }
        if(colors is null) return dims;
        if(palette is not null && colors != palette)
            return grayscale
                       ? $"{dims} with {colors} grays from {palette} palette"
                       : $"{dims} with {colors} colors from {palette} palette";
        return grayscale ? $"{dims} with {colors} grays" : $"{dims} with {colors} colors";
    }

    /// <summary>
    ///     Build readable labels for the heterogeneous payload of a Processor suggestion.
    ///     Handles scalar FK fields (<c>company_id</c>, <c>instruction_set_id</c>) plus
    ///     junction-add operation keys
    ///     (<c>instruction_set_extensions.add.&lt;uuid&gt;</c> resolved from the suggested
    ///     payload's <c>extension_id</c>). Junction-remove labels are populated separately by
    ///     <see cref="ResolveProcessorRemoveLabelsAsync" />.
    /// </summary>
    async Task ResolveProcessorLabelsAsync(long? entityId,
                                           Dictionary<string, JsonElement> values,
                                           Dictionary<string, string> labels)
    {
        // ---- Scalar FK labels ---------------------------------------------------------
        if(values.TryGetValue(Suggestions.ProcessorSuggestionApplier.FieldCompanyId, out JsonElement companyE))
        {
            int? id = JsonElementToInt(companyE);
            if(id.HasValue)
            {
                string name = await context.Companies.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.ProcessorSuggestionApplier.FieldCompanyId] = name;
            }
        }

        if(values.TryGetValue(Suggestions.ProcessorSuggestionApplier.FieldInstructionSetId, out JsonElement isE))
        {
            int? id = JsonElementToInt(isE);
            if(id.HasValue)
            {
                string name = await context.InstructionSets.AsNoTracking()
                                           .Where(i => i.Id == id.Value)
                                           .Select(i => i.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.ProcessorSuggestionApplier.FieldInstructionSetId] = name;
            }
        }

        // Note: junction-add labels look up the linked InstructionSetExtension directly from
        // the suggested payload's `extension_id` field — they don't need the parent
        // entityId. Junction-remove labels (which DO need the parent id) are handled by the
        // companion ResolveProcessorRemoveLabelsAsync, gated separately at the call site.
        _ = entityId;

        // ---- Junction-add labels (resolved from the suggested payload's id field) -----
        foreach(KeyValuePair<string, JsonElement> kv in values)
        {
            if(!Suggestions.ProcessorSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out _))
                continue;
            if(op != "add" || kv.Value.ValueKind != JsonValueKind.Object) continue;

            string label = await BuildProcessorAddLabelAsync(group, kv.Value);
            if(!string.IsNullOrEmpty(label)) labels[kv.Key] = label;
        }
    }

    /// <summary>
    ///     Resolve readable labels for every Processor junction-remove operation in the
    ///     suggested payload by looking up the existing junction row in the database. The
    ///     resolved labels are written to <paramref name="currentLabels" /> so the diff panel
    ///     renders them in the strikethrough cell of the JunctionRemove row. Iterates the RAW
    ///     <c>SuggestedValues</c> dict (not the JsonElement-parsed view) because remove ops
    ///     carry a null value and <see cref="ParseValuesForLabels" /> drops null entries.
    /// </summary>
    async Task ResolveProcessorRemoveLabelsAsync(long processorId,
                                                 Dictionary<string, object> rawSuggested,
                                                 Dictionary<string, string> currentLabels)
    {
        foreach(KeyValuePair<string, object> kv in rawSuggested)
        {
            if(!Suggestions.ProcessorSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out string token))
                continue;
            if(op != "remove") continue;
            if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) continue;

            string label = await BuildProcessorRemoveLabelAsync(group, (int)processorId, rowId);
            if(!string.IsNullOrEmpty(label)) currentLabels[kv.Key] = label;
        }
    }

    async Task<string> BuildProcessorAddLabelAsync(string group, JsonElement payload)
    {
        switch(group)
        {
            case Suggestions.ProcessorSuggestionApplier.GroupInstructionSetExtensions:
            {
                int? id = ReadIntField(payload, "extension_id");
                if(!id.HasValue) return null;
                string name = await context.InstructionSetExtensions.AsNoTracking()
                                           .Where(e => e.Id == id.Value)
                                           .Select(e => e.Extension)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Extension #{id.Value}" : name;
            }
            default:
                return null;
        }
    }

    async Task<string> BuildProcessorRemoveLabelAsync(string group, int processorId, long rowId)
    {
        switch(group)
        {
            case Suggestions.ProcessorSuggestionApplier.GroupInstructionSetExtensions:
            {
                string name = await context.InstructionSetExtensionsByProcessor.AsNoTracking()
                                           .Where(r => r.Id == rowId && r.ProcessorId == processorId)
                                           .Select(r => r.Extension.Extension)
                                           .FirstOrDefaultAsync();
                return name;
            }
            default:
                return null;
        }
    }

    /// <summary>
    ///     Build readable labels for a SoundSynth suggestion. SoundSynth has no in-scope
    ///     junctions (both visible junctions are owned by the other side per established
    ///     convention), so this only resolves the scalar <c>company_id</c> FK label.
    /// </summary>
    async Task ResolveSoundSynthLabelsAsync(long? entityId,
                                            Dictionary<string, JsonElement> values,
                                            Dictionary<string, string> labels)
    {
        _ = entityId; // unused — no junctions.

        if(values.TryGetValue(Suggestions.SoundSynthSuggestionApplier.FieldCompanyId, out JsonElement companyE))
        {
            int? id = JsonElementToInt(companyE);
            if(id.HasValue)
            {
                string name = await context.Companies.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.SoundSynthSuggestionApplier.FieldCompanyId] = name;
            }
        }
    }

    /// <summary>
    ///     Build readable labels for a Person suggestion. Person has no in-scope junctions
    ///     (the five Person junctions are owned by the other side per the established
    ///     convention), so this only resolves the scalar <c>country_of_birth_id</c> FK
    ///     label.
    /// </summary>
    async Task ResolvePersonLabelsAsync(long? entityId,
                                        Dictionary<string, JsonElement> values,
                                        Dictionary<string, string> labels)
    {
        _ = entityId; // unused — no junctions.

        if(values.TryGetValue(Suggestions.PersonSuggestionApplier.FieldCountryOfBirthId, out JsonElement countryE))
        {
            short? id = countryE.ValueKind == JsonValueKind.Number && countryE.TryGetInt16(out short v)
                            ? v
                            : (short?)null;
            if(id.HasValue)
            {
                string name = await context.Iso31661Numeric.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.PersonSuggestionApplier.FieldCountryOfBirthId] = name;
            }
        }
    }

    // ─────────────── Software entity-edit label resolution (scalars + junction ops) ───────────────

    /// <summary>
    ///     Build readable labels for a Software suggestion. Resolves three scalar FK fields
    ///     (<c>family_id</c>, <c>predecessor_id</c>, <c>base_software_id</c>) plus
    ///     junction-add operation keys (<c>genres.add.&lt;uuid&gt;</c> resolved from the
    ///     suggested payload's <c>genre_id</c>). Junction-remove labels are populated
    ///     separately by <see cref="ResolveSoftwareRemoveLabelsAsync" />. Software is
    ///     <c>ulong</c>-keyed but Kiota widens FK ids to <c>int?</c> on the wire so the
    ///     payload values are coerced as int and re-cast to ulong for the EF queries.
    /// </summary>
    async Task ResolveSoftwareLabelsAsync(long? entityId,
                                          Dictionary<string, JsonElement> values,
                                          Dictionary<string, string> labels)
    {
        // ---- Scalar FK labels ---------------------------------------------------------
        if(values.TryGetValue(Suggestions.SoftwareSuggestionApplier.FieldFamilyId, out JsonElement familyE))
        {
            long? id = JsonElementToLong(familyE);
            if(id.HasValue && id.Value >= 0)
            {
                ulong fidU = (ulong)id.Value;
                string name = await context.SoftwareFamilies.AsNoTracking()
                                           .Where(f => f.Id == fidU)
                                           .Select(f => f.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.SoftwareSuggestionApplier.FieldFamilyId] = name;
            }
        }

        if(values.TryGetValue(Suggestions.SoftwareSuggestionApplier.FieldPredecessorId, out JsonElement predecessorE))
        {
            long? id = JsonElementToLong(predecessorE);
            if(id.HasValue && id.Value >= 0)
            {
                ulong pidU = (ulong)id.Value;
                string name = await context.Softwares.AsNoTracking()
                                           .Where(x => x.Id == pidU)
                                           .Select(x => x.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.SoftwareSuggestionApplier.FieldPredecessorId] = name;
            }
        }

        if(values.TryGetValue(Suggestions.SoftwareSuggestionApplier.FieldBaseSoftwareId, out JsonElement baseE))
        {
            long? id = JsonElementToLong(baseE);
            if(id.HasValue && id.Value >= 0)
            {
                ulong bidU = (ulong)id.Value;
                string name = await context.Softwares.AsNoTracking()
                                           .Where(x => x.Id == bidU)
                                           .Select(x => x.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.SoftwareSuggestionApplier.FieldBaseSoftwareId] = name;
            }
        }

        // Junction-add labels look up the linked entity (genre / company+role / person)
        // directly from the suggested payload's id field and don't need the parent
        // softwareId. Run them in BOTH edit and creation mode (entityId is null in
        // addition mode).
        _ = entityId;

        // ---- Junction-add labels (resolved from the suggested payload's id field) -----
        foreach(KeyValuePair<string, JsonElement> kv in values)
        {
            if(!Suggestions.SoftwareSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out _))
                continue;
            if(op != "add" || kv.Value.ValueKind != JsonValueKind.Object) continue;

            string label = group switch
            {
                Suggestions.SoftwareSuggestionApplier.GroupGenres    => await BuildSoftwareGenreAddLabelAsync(group, kv.Value),
                Suggestions.SoftwareSuggestionApplier.GroupCompanies => await BuildSoftwareCompanyAddLabelAsync(group, kv.Value),
                Suggestions.SoftwareSuggestionApplier.GroupCredits   => await BuildSoftwareCreditAddLabelAsync(group, kv.Value),
                _                                                    => null
            };
            if(!string.IsNullOrEmpty(label)) labels[kv.Key] = label;
        }

        // ---- First-release-prefixed labels (Software creation mode only) -------------
        // Forward "first_release_*" / "first_release.*" keys to the SoftwareRelease label
        // resolver by stripping the prefix into a sub-dict, resolving with releaseId=null
        // (creation mode), and re-prefixing the emitted labels back to the wire shape so
        // the admin diff panel renders them under their original keys.
        var releaseSubValues = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach(KeyValuePair<string, JsonElement> kv in values)
        {
            if(kv.Key.StartsWith(Suggestions.SoftwareSuggestionApplier.FirstReleaseGroupPrefix, StringComparison.Ordinal))
                releaseSubValues[kv.Key.Substring(Suggestions.SoftwareSuggestionApplier.FirstReleaseGroupPrefix.Length)] = kv.Value;
            else if(kv.Key.StartsWith(Suggestions.SoftwareSuggestionApplier.FirstReleaseScalarPrefix, StringComparison.Ordinal))
                releaseSubValues[kv.Key.Substring(Suggestions.SoftwareSuggestionApplier.FirstReleaseScalarPrefix.Length)] = kv.Value;
        }
        if(releaseSubValues.Count > 0)
        {
            var releaseLabels = new Dictionary<string, string>(StringComparer.Ordinal);
            await ResolveSoftwareReleaseLabelsAsync(null, releaseSubValues, releaseLabels);
            foreach(KeyValuePair<string, string> kv in releaseLabels)
            {
                // Junction-op keys contain a dot ("group.op.token"); scalars never do.
                string prefix = kv.Key.Contains('.', StringComparison.Ordinal)
                                    ? Suggestions.SoftwareSuggestionApplier.FirstReleaseGroupPrefix
                                    : Suggestions.SoftwareSuggestionApplier.FirstReleaseScalarPrefix;
                labels[prefix + kv.Key] = kv.Value;
            }
        }
    }
    /// <summary>
    ///     Resolve readable labels for every Software junction-remove operation in the
    ///     suggested payload by looking up the targeted genre row. The resolved labels are
    ///     written to <paramref name="currentLabels" /> so the diff panel renders them in the
    ///     strikethrough cell of the JunctionRemove row. Iterates the RAW
    ///     <c>SuggestedValues</c> dict (not the JsonElement-parsed view) because remove ops
    ///     carry a null value and <see cref="ParseValuesForLabels" /> drops null entries.
    ///     Note: the genres remove token is the genre id itself (composite-key junction has
    ///     no surrogate row id), so the lookup verifies the link exists for THIS software
    ///     before resolving the genre name.
    /// </summary>
    async Task ResolveSoftwareRemoveLabelsAsync(long softwareId,
                                                Dictionary<string, object> rawSuggested,
                                                Dictionary<string, string> currentLabels)
    {
        foreach(KeyValuePair<string, object> kv in rawSuggested)
        {
            if(!Suggestions.SoftwareSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out string token))
                continue;
            if(op != "remove") continue;

            string label = null;
            switch(group)
            {
                case Suggestions.SoftwareSuggestionApplier.GroupGenres:
                {
                    if(!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int genreId)) continue;
                    label = await BuildSoftwareGenreRemoveLabelAsync(group, (ulong)softwareId, genreId);
                    break;
                }
                case Suggestions.SoftwareSuggestionApplier.GroupCompanies:
                {
                    // 2-component composite token "{companyId}_{roleId}".
                    string[] parts = token.Split('_', 2);
                    if(parts.Length != 2 || string.IsNullOrEmpty(parts[1])) continue;
                    if(!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int companyId)) continue;
                    label = await BuildSoftwareCompanyRemoveLabelAsync(group, (ulong)softwareId, companyId, parts[1]);
                    break;
                }
                case Suggestions.SoftwareSuggestionApplier.GroupCredits:
                {
                    // PeopleBySoftware has a surrogate row Id, so the credits remove token
                    // is the row id directly (parsed as long). The lookup is gated on
                    // SoftwareId to keep the displayed label scoped to this entity.
                    if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) continue;
                    label = await BuildSoftwareCreditRemoveLabelAsync(group, (ulong)softwareId, rowId);
                    break;
                }
            }
            if(!string.IsNullOrEmpty(label)) currentLabels[kv.Key] = label;
        }
    }

    async Task<string> BuildSoftwareGenreAddLabelAsync(string group, JsonElement payload)
    {
        switch(group)
        {
            case Suggestions.SoftwareSuggestionApplier.GroupGenres:
            {
                int? id = ReadIntField(payload, "genre_id");
                if(!id.HasValue) return null;
                string name = await context.SoftwareGenres.AsNoTracking()
                                           .Where(g => g.Id == id.Value)
                                           .Select(g => g.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Genre #{id.Value}" : name;
            }
            default:
                return null;
        }
    }

    async Task<string> BuildSoftwareGenreRemoveLabelAsync(string group, ulong softwareId, int genreId)
    {
        switch(group)
        {
            case Suggestions.SoftwareSuggestionApplier.GroupGenres:
            {
                // Verify the link exists for THIS software; only then resolve the name from
                // the genres table. Returning null for a stale remove op leaves the diff
                // panel cell blank rather than showing a misleading label.
                string name = await context.GenresBySoftware.AsNoTracking()
                                           .Where(r => r.SoftwareId == softwareId && r.GenreId == genreId)
                                           .Select(r => r.Genre.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? null : name;
            }
            default:
                return null;
        }
    }

    async Task<string> BuildSoftwareCompanyAddLabelAsync(string group, JsonElement payload)
    {
        switch(group)
        {
            case Suggestions.SoftwareSuggestionApplier.GroupCompanies:
            {
                int?   id     = ReadIntField(payload, "company_id");
                string roleId = ReadStringField(payload, "role_id");
                if(!id.HasValue) return null;
                string name = await context.Companies.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                string display = string.IsNullOrEmpty(name) ? $"Company #{id.Value}" : name;
                if(!string.IsNullOrEmpty(roleId))
                {
                    string roleName = await context.SoftwareRoles.AsNoTracking()
                                                   .Where(r => r.Id == roleId)
                                                   .Select(r => r.Name)
                                                   .FirstOrDefaultAsync();
                    if(!string.IsNullOrEmpty(roleName))
                        display = $"{display} ({roleName})";
                }
                return display;
            }
            default:
                return null;
        }
    }

    async Task<string> BuildSoftwareCompanyRemoveLabelAsync(string group, ulong softwareId, int companyId, string roleId)
    {
        switch(group)
        {
            case Suggestions.SoftwareSuggestionApplier.GroupCompanies:
            {
                // Verify the link exists for THIS software (composite-key guard); only then
                // resolve readable names. Stale remove ops yield null → diff panel cell
                // stays blank rather than showing a misleading label.
                var row = await context.SoftwareCompanyRoles.AsNoTracking()
                                       .Where(r => r.SoftwareId == softwareId &&
                                                   r.CompanyId  == companyId  &&
                                                   r.RoleId     == roleId)
                                       .Select(r => new { CompanyName = r.Company.Name, RoleName = r.Role.Name })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                return string.IsNullOrEmpty(row.RoleName) ? row.CompanyName : $"{row.CompanyName} ({row.RoleName})";
            }
            default:
                return null;
        }
    }

    async Task<string> BuildSoftwareCreditAddLabelAsync(string group, JsonElement payload)
    {
        switch(group)
        {
            case Suggestions.SoftwareSuggestionApplier.GroupCredits:
            {
                int?   id   = ReadIntField(payload, "person_id");
                string role = ReadStringField(payload, "role");
                if(!id.HasValue) return null;
                var p = await context.People.AsNoTracking()
                                     .Where(x => x.Id == id.Value)
                                     .Select(x => new
                                      {
                                          x.Name,
                                          x.Surname,
                                          x.Alias,
                                          x.DisplayName
                                      })
                                     .FirstOrDefaultAsync();
                string display = p is null
                                     ? $"Person #{id.Value}"
                                     : (!string.IsNullOrEmpty(p.DisplayName) ? p.DisplayName
                                        : !string.IsNullOrEmpty(p.Alias)     ? p.Alias
                                        :                                      $"{p.Name} {p.Surname}".Trim());
                if(string.IsNullOrWhiteSpace(display)) display = $"Person #{id.Value}";
                return string.IsNullOrEmpty(role) ? display : $"{display} ({role})";
            }
            default:
                return null;
        }
    }

    async Task<string> BuildSoftwareCreditRemoveLabelAsync(string group, ulong softwareId, long rowId)
    {
        switch(group)
        {
            case Suggestions.SoftwareSuggestionApplier.GroupCredits:
            {
                // Verify the link exists for THIS software (surrogate-Id guard); only then
                // resolve readable names. Stale remove ops yield null → diff panel cell
                // stays blank rather than showing a misleading label.
                var row = await context.PeopleBySoftware.AsNoTracking()
                                       .Where(r => r.Id == rowId && r.SoftwareId == softwareId)
                                       .Select(r => new
                                        {
                                            r.Person.Name,
                                            r.Person.Surname,
                                            r.Person.Alias,
                                            r.Person.DisplayName,
                                            r.Role
                                        })
                                       .FirstOrDefaultAsync();
                if(row is null) return null;
                string name = !string.IsNullOrEmpty(row.DisplayName) ? row.DisplayName
                              : !string.IsNullOrEmpty(row.Alias)     ? row.Alias
                              :                                        $"{row.Name} {row.Surname}".Trim();
                if(string.IsNullOrWhiteSpace(name)) name = $"Person #{rowId}";
                return string.IsNullOrEmpty(row.Role) ? name : $"{name} ({row.Role})";
            }
            default:
                return null;
        }
    }

    // ─────────────── SoftwareRelease entity-edit label resolution (scalars + junction ops) ───────────────

    /// <summary>
    ///     Build readable labels for a SoftwareRelease suggestion. Resolves two scalar FK
    ///     fields (<c>platform_id</c>, <c>publisher_id</c>) plus junction-add operation keys
    ///     for all four in-scope groups. Junction-remove labels are populated separately by
    ///     <see cref="ResolveSoftwareReleaseRemoveLabelsAsync" />.
    /// </summary>
    async Task ResolveSoftwareReleaseLabelsAsync(long? entityId,
                                                 Dictionary<string, JsonElement> values,
                                                 Dictionary<string, string> labels)
    {
        // ---- Scalar FK labels ---------------------------------------------------------
        if(values.TryGetValue(Suggestions.SoftwareReleaseSuggestionApplier.FieldPlatformId, out JsonElement platE))
        {
            long? id = JsonElementToLong(platE);
            if(id.HasValue && id.Value >= 0)
            {
                ulong pidU = (ulong)id.Value;
                string name = await context.SoftwarePlatforms.AsNoTracking()
                                           .Where(p => p.Id == pidU)
                                           .Select(p => p.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.SoftwareReleaseSuggestionApplier.FieldPlatformId] = name;
            }
        }

        if(values.TryGetValue(Suggestions.SoftwareReleaseSuggestionApplier.FieldPublisherId, out JsonElement pubE))
        {
            int? id = JsonElementToInt(pubE);
            if(id.HasValue)
            {
                string name = await context.Companies.AsNoTracking()
                                           .Where(c => c.Id == id.Value)
                                           .Select(c => c.Name)
                                           .FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(name))
                    labels[Suggestions.SoftwareReleaseSuggestionApplier.FieldPublisherId] = name;
            }
        }

        // Junction-add labels look up the linked entity (gpu / sound synth / language /
        // region / barcode payload / etc.) directly from the suggested payload's id /
        // payload fields and don't need the parent releaseId. Run them in BOTH edit and
        // creation mode (entityId is null in addition mode).
        _ = entityId;

        // ---- Junction-add labels (resolved from the suggested payload's id field) -----
        foreach(KeyValuePair<string, JsonElement> kv in values)
        {
            if(!Suggestions.SoftwareReleaseSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out _))
                continue;
            if(op != "add" || kv.Value.ValueKind != JsonValueKind.Object) continue;

            string label = await BuildSoftwareReleaseAddLabelAsync(group, kv.Value);
            if(!string.IsNullOrEmpty(label)) labels[kv.Key] = label;
        }
    }

    /// <summary>
    ///     Resolve readable labels for every SoftwareRelease junction-remove operation in the
    ///     suggested payload. Iterates the RAW <c>SuggestedValues</c> dict because remove ops
    ///     carry a null value and <see cref="ParseValuesForLabels" /> drops null entries.
    /// </summary>
    async Task ResolveSoftwareReleaseRemoveLabelsAsync(long releaseId,
                                                       Dictionary<string, object> rawSuggested,
                                                       Dictionary<string, string> currentLabels)
    {
        ulong releaseIdU = (ulong)releaseId;

        foreach(KeyValuePair<string, object> kv in rawSuggested)
        {
            if(!Suggestions.SoftwareReleaseSuggestionApplier.TryParseJunctionKey(kv.Key, out string group, out string op, out string token))
                continue;
            if(op != "remove") continue;

            string label = group switch
            {
                Suggestions.SoftwareReleaseSuggestionApplier.GroupRegions      => await BuildSoftwareReleaseRegionRemoveLabelAsync(releaseIdU, token),
                Suggestions.SoftwareReleaseSuggestionApplier.GroupLanguages    => await BuildSoftwareReleaseLanguageRemoveLabelAsync(releaseIdU, token),
                Suggestions.SoftwareReleaseSuggestionApplier.GroupBarcodes     => await BuildSoftwareReleaseBarcodeRemoveLabelAsync(releaseIdU, token),
                Suggestions.SoftwareReleaseSuggestionApplier.GroupProductCodes => await BuildSoftwareReleaseProductCodeRemoveLabelAsync(releaseIdU, token),
                Suggestions.SoftwareReleaseSuggestionApplier.GroupSpecs        => await BuildSoftwareReleaseAttributeRemoveLabelAsync(releaseIdU, token, Suggestions.SoftwareReleaseSuggestionApplier.AttributeCategorySpec),
                Suggestions.SoftwareReleaseSuggestionApplier.GroupRatings      => await BuildSoftwareReleaseAttributeRemoveLabelAsync(releaseIdU, token, Suggestions.SoftwareReleaseSuggestionApplier.AttributeCategoryRating),
                Suggestions.SoftwareReleaseSuggestionApplier.GroupMinGpus      => await BuildSoftwareReleaseMinGpuRemoveLabelAsync(releaseIdU, token),
                Suggestions.SoftwareReleaseSuggestionApplier.GroupRecGpus      => await BuildSoftwareReleaseRecGpuRemoveLabelAsync(releaseIdU, token),
                Suggestions.SoftwareReleaseSuggestionApplier.GroupSoundSynths  => await BuildSoftwareReleaseSoundSynthRemoveLabelAsync(releaseIdU, token),
                _                                                              => null
            };
            if(!string.IsNullOrEmpty(label)) currentLabels[kv.Key] = label;
        }
    }

    async Task<string> BuildSoftwareReleaseAddLabelAsync(string group, JsonElement payload)
    {
        switch(group)
        {
            case Suggestions.SoftwareReleaseSuggestionApplier.GroupRegions:
            {
                int? id = ReadIntField(payload, "unm49_id");
                if(!id.HasValue || id.Value < short.MinValue || id.Value > short.MaxValue) return null;
                short sid = (short)id.Value;
                string name = await context.UnM49.AsNoTracking()
                                           .Where(u => u.Id == sid)
                                           .Select(u => u.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Region #{sid}" : name;
            }
            case Suggestions.SoftwareReleaseSuggestionApplier.GroupLanguages:
            {
                string code = ReadStringField(payload, "language_code");
                if(string.IsNullOrWhiteSpace(code)) return null;
                code = code.Trim();
                string name = await context.Iso639.AsNoTracking()
                                           .Where(l => l.Id == code)
                                           .Select(l => l.ReferenceName)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? code : name;
            }
            case Suggestions.SoftwareReleaseSuggestionApplier.GroupBarcodes:
            {
                string code = ReadStringField(payload, "code");
                int? type = ReadIntField(payload, "type");
                if(string.IsNullOrEmpty(code) || !type.HasValue) return null;
                string typeLabel = Enum.IsDefined(typeof(BarcodeType), (byte)type.Value)
                                       ? ((BarcodeType)type.Value).ToString()
                                       : $"#{type.Value}";
                return $"{typeLabel}: {code}";
            }
            case Suggestions.SoftwareReleaseSuggestionApplier.GroupProductCodes:
            {
                string code = ReadStringField(payload, "code");
                int? issuer = ReadIntField(payload, "issuer");
                if(string.IsNullOrEmpty(code) || !issuer.HasValue) return null;
                string issuerLabel = Enum.IsDefined(typeof(ProductCodeIssuer), (byte)issuer.Value)
                                         ? ((ProductCodeIssuer)issuer.Value).ToString()
                                         : $"#{issuer.Value}";
                return $"{issuerLabel}: {code}";
            }
            case Suggestions.SoftwareReleaseSuggestionApplier.GroupSpecs:
            case Suggestions.SoftwareReleaseSuggestionApplier.GroupRatings:
            {
                string key   = ReadStringField(payload, "key");
                string value = ReadStringField(payload, "value");
                if(string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return null;
                return $"{key.Trim()}: {value.Trim()}";
            }
            case Suggestions.SoftwareReleaseSuggestionApplier.GroupMinGpus:
            case Suggestions.SoftwareReleaseSuggestionApplier.GroupRecGpus:
            {
                int? gpuId = ReadIntField(payload, "gpu_id");
                if(!gpuId.HasValue) return null;
                string name = await context.Gpus.AsNoTracking()
                                           .Where(g => g.Id == gpuId.Value)
                                           .Select(g => g.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"GPU #{gpuId.Value}" : name;
            }
            case Suggestions.SoftwareReleaseSuggestionApplier.GroupSoundSynths:
            {
                int? synthId = ReadIntField(payload, "sound_synth_id");
                if(!synthId.HasValue) return null;
                string name = await context.SoundSynths.AsNoTracking()
                                           .Where(s => s.Id == synthId.Value)
                                           .Select(s => s.Name)
                                           .FirstOrDefaultAsync();
                return string.IsNullOrEmpty(name) ? $"Sound synth #{synthId.Value}" : name;
            }
            default:
                return null;
        }
    }

    async Task<string> BuildSoftwareReleaseRegionRemoveLabelAsync(ulong releaseId, string token)
    {
        if(!short.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out short id)) return null;
        // Verify the link exists for THIS release (composite-key guard).
        string name = await context.UnM49BySoftwareRelease.AsNoTracking()
                                   .Where(x => x.SoftwareReleaseId == releaseId && x.UnM49Id == id)
                                   .Select(x => x.UnM49.Name)
                                   .FirstOrDefaultAsync();
        return string.IsNullOrEmpty(name) ? null : name;
    }

    async Task<string> BuildSoftwareReleaseLanguageRemoveLabelAsync(ulong releaseId, string token)
    {
        // String-keyed composite junction \u2014 token IS the language code.
        string code = token;
        if(string.IsNullOrEmpty(code)) return null;
        string name = await context.LanguageBySoftwareRelease.AsNoTracking()
                                   .Where(x => x.SoftwareReleaseId == releaseId && x.LanguageCode == code)
                                   .Select(x => x.Language.ReferenceName)
                                   .FirstOrDefaultAsync();
        return string.IsNullOrEmpty(name) ? null : name;
    }

    async Task<string> BuildSoftwareReleaseBarcodeRemoveLabelAsync(ulong releaseId, string token)
    {
        if(!ulong.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong rowId)) return null;
        var row = await context.SoftwareBarcodes.AsNoTracking()
                               .Where(b => b.Id == rowId && b.ReleaseId == releaseId)
                               .Select(b => new { b.Code, b.Type })
                               .FirstOrDefaultAsync();
        if(row is null) return null;
        return $"{row.Type}: {row.Code}";
    }

    async Task<string> BuildSoftwareReleaseProductCodeRemoveLabelAsync(ulong releaseId, string token)
    {
        if(!ulong.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong rowId)) return null;
        var row = await context.SoftwareProductCodes.AsNoTracking()
                               .Where(p => p.Id == rowId && p.ReleaseId == releaseId)
                               .Select(p => new { p.Code, p.Issuer })
                               .FirstOrDefaultAsync();
        if(row is null) return null;
        return $"{row.Issuer}: {row.Code}";
    }

    /// <summary>
    ///     Resolve the readable label for a SoftwareAttribute (spec or rating) remove
    ///     operation. Gating on both <c>SoftwareReleaseId</c> and <c>Category</c> mirrors
    ///     the applier guard so a Specs dialog cannot resolve a Rating row id (the lookup
    ///     simply returns null in that case).
    /// </summary>
    async Task<string> BuildSoftwareReleaseAttributeRemoveLabelAsync(ulong releaseId, string token, string category)
    {
        if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) return null;
        var row = await context.SoftwareAttributes.AsNoTracking()
                               .Where(a => a.Id == rowId &&
                                           a.SoftwareReleaseId == releaseId &&
                                           a.Category          == category)
                               .Select(a => new { a.Key, a.Value })
                               .FirstOrDefaultAsync();
        if(row is null) return null;
        return $"{row.Key}: {row.Value}";
    }

    async Task<string> BuildSoftwareReleaseMinGpuRemoveLabelAsync(ulong releaseId, string token)
    {
        if(!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int gpuId)) return null;
        string name = await context.MinimumGpuBySoftwareRelease.AsNoTracking()
                                   .Where(x => x.ReleaseId == releaseId && x.GpuId == gpuId)
                                   .Select(x => x.Gpu.Name)
                                   .FirstOrDefaultAsync();
        return string.IsNullOrEmpty(name) ? null : name;
    }

    async Task<string> BuildSoftwareReleaseRecGpuRemoveLabelAsync(ulong releaseId, string token)
    {
        if(!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int gpuId)) return null;
        string name = await context.RecommendedGpuBySoftwareRelease.AsNoTracking()
                                   .Where(x => x.ReleaseId == releaseId && x.GpuId == gpuId)
                                   .Select(x => x.Gpu.Name)
                                   .FirstOrDefaultAsync();
        return string.IsNullOrEmpty(name) ? null : name;
    }

    async Task<string> BuildSoftwareReleaseSoundSynthRemoveLabelAsync(ulong releaseId, string token)
    {
        if(!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int synthId)) return null;
        string name = await context.SoundSynthBySoftwareRelease.AsNoTracking()
                                   .Where(x => x.ReleaseId == releaseId && x.SoundSynthId == synthId)
                                   .Select(x => x.SoundSynth.Name)
                                   .FirstOrDefaultAsync();
        return string.IsNullOrEmpty(name) ? null : name;
    }

    /// <summary>
    ///     Sweep pending image uploads referenced by a now-terminal suggestion. Called from
    ///     <see cref="ReviewAsync" /> after the per-entity applier has run. For every
    ///     <c>cover_pending_guid</c> in the suggested payload that did NOT make it into the
    ///     <paramref name="accepted" /> set, delete the orphaned pending file + sidecar.
    ///     The accepted path is already cleaned up by the applier (the file is moved into
    ///     <c>originals/</c> and the sidecar removed) so we only need to handle rejection /
    ///     partial-accept-without-cover here. Best-effort: failures are swallowed.
    /// </summary>
    void CleanupPendingMedia(Suggestion s, HashSet<string> accepted)
    {
        if(s?.SuggestedValues is null || string.IsNullOrEmpty(_assetRootPath)) return;

        try
        {
            switch(s.EntityType)
            {
                case SuggestionEntityType.Book:
                {
                    if(!s.SuggestedValues.TryGetValue(
                           Suggestions.BookSuggestionApplier.FieldCoverPendingGuid, out object guidRaw))
                        return;

                    string guidStr = guidRaw switch
                    {
                        string str         => str,
                        JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
                        _                  => guidRaw?.ToString()
                    };
                    if(string.IsNullOrEmpty(guidStr) || !Guid.TryParse(guidStr, out Guid pendingGuid)) return;

                    // If the cover field was accepted, the applier already promoted the file
                    // (and removed the sidecar). Only sweep when it wasn't accepted.
                    if(accepted.Contains(Suggestions.BookSuggestionApplier.FieldCoverPendingGuid)) return;

                    Marechai.Server.Helpers.PendingImageStore.Delete(_assetRootPath, "book-covers", pendingGuid);
                    break;
                }
                case SuggestionEntityType.MagazineIssue:
                {
                    if(!s.SuggestedValues.TryGetValue(
                           Suggestions.MagazineIssueSuggestionApplier.FieldCoverPendingGuid, out object guidRaw))
                        return;

                    string guidStr = guidRaw switch
                    {
                        string str         => str,
                        JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
                        _                  => guidRaw?.ToString()
                    };
                    if(string.IsNullOrEmpty(guidStr) || !Guid.TryParse(guidStr, out Guid pendingGuid)) return;

                    // If the cover field was accepted, the applier already promoted the file
                    // (and removed the sidecar). Only sweep when it wasn't accepted.
                    if(accepted.Contains(Suggestions.MagazineIssueSuggestionApplier.FieldCoverPendingGuid)) return;

                    Marechai.Server.Helpers.PendingImageStore.Delete(_assetRootPath, "magazine-issue-covers", pendingGuid);
                    break;
                }
                case SuggestionEntityType.Person:
                {
                    if(!s.SuggestedValues.TryGetValue(
                           Suggestions.PersonSuggestionApplier.FieldCoverPendingGuid, out object guidRaw))
                        return;

                    string guidStr = guidRaw switch
                    {
                        string str         => str,
                        JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
                        _                  => guidRaw?.ToString()
                    };
                    if(string.IsNullOrEmpty(guidStr) || !Guid.TryParse(guidStr, out Guid pendingGuid)) return;

                    // If the photo field was accepted, the applier already promoted the file
                    // (and removed the sidecar). Only sweep when it wasn't accepted.
                    if(accepted.Contains(Suggestions.PersonSuggestionApplier.FieldCoverPendingGuid)) return;

                    Marechai.Server.Helpers.PendingImageStore.Delete(_assetRootPath, "people", pendingGuid);
                    break;
                }
                case SuggestionEntityType.GpuPhoto:
                {
                    // Sweep every per-photo pending file the suggester referenced that did NOT
                    // end up accepted. The applier already deletes rejected pending files when
                    // the admin submits the review (see GpuPhotoSuggestionApplier.ApplyAsync's
                    // "rejected → Delete" branch), so this catch-all only fires for the
                    // never-reviewed paths (withdraw, stale, controller exception fallback).
                    if(!s.SuggestedValues.TryGetValue(
                           Suggestions.GpuPhotoSuggestionApplier.FieldPhotos, out object photosRaw))
                        return;

                    if(photosRaw is not JsonElement arr || arr.ValueKind != JsonValueKind.Array) return;

                    foreach(JsonElement photo in arr.EnumerateArray())
                    {
                        if(photo.ValueKind != JsonValueKind.Object) continue;
                        if(!photo.TryGetProperty("guid", out JsonElement gj) ||
                           gj.ValueKind != JsonValueKind.String) continue;
                        if(!Guid.TryParse(gj.GetString(), out Guid pg)) continue;

                        string acceptKey = Suggestions.GpuPhotoSuggestionApplier.PhotoAcceptKey(pg);

                        // Skip files that were just promoted (the applier deleted the sidecar
                        // already; calling Delete here is harmless thanks to the silent
                        // fallback but we save the IO).
                        if(accepted.Contains(acceptKey)) continue;

                        Marechai.Server.Helpers.PendingImageStore.Delete(_assetRootPath, "gpus", pg);
                    }

                    break;
                }
                case SuggestionEntityType.ProcessorPhoto:
                {
                    // Sweep every per-photo pending file the suggester referenced that did NOT
                    // end up accepted. The applier already deletes rejected pending files when
                    // the admin submits the review (see ProcessorPhotoSuggestionApplier.ApplyAsync's
                    // "rejected → Delete" branch), so this catch-all only fires for the
                    // never-reviewed paths (withdraw, stale, controller exception fallback).
                    if(!s.SuggestedValues.TryGetValue(
                           Suggestions.ProcessorPhotoSuggestionApplier.FieldPhotos, out object photosRaw))
                        return;

                    if(photosRaw is not JsonElement arr || arr.ValueKind != JsonValueKind.Array) return;

                    foreach(JsonElement photo in arr.EnumerateArray())
                    {
                        if(photo.ValueKind != JsonValueKind.Object) continue;
                        if(!photo.TryGetProperty("guid", out JsonElement gj) ||
                           gj.ValueKind != JsonValueKind.String) continue;
                        if(!Guid.TryParse(gj.GetString(), out Guid pg)) continue;

                        string acceptKey = Suggestions.ProcessorPhotoSuggestionApplier.PhotoAcceptKey(pg);

                        // Skip files that were just promoted (the applier deleted the sidecar
                        // already; calling Delete here is harmless thanks to the silent
                        // fallback but we save the IO).
                        if(accepted.Contains(acceptKey)) continue;

                        Marechai.Server.Helpers.PendingImageStore.Delete(_assetRootPath, "processors", pg);
                    }

                    break;
                }
                case SuggestionEntityType.SoundSynthPhoto:
                {
                    // Sweep every per-photo pending file the suggester referenced that did NOT
                    // end up accepted. The applier already deletes rejected pending files when
                    // the admin submits the review (see SoundSynthPhotoSuggestionApplier.ApplyAsync's
                    // "rejected → Delete" branch), so this catch-all only fires for the
                    // never-reviewed paths (withdraw, stale, controller exception fallback).
                    if(!s.SuggestedValues.TryGetValue(
                           Suggestions.SoundSynthPhotoSuggestionApplier.FieldPhotos, out object photosRaw))
                        return;

                    if(photosRaw is not JsonElement arr || arr.ValueKind != JsonValueKind.Array) return;

                    foreach(JsonElement photo in arr.EnumerateArray())
                    {
                        if(photo.ValueKind != JsonValueKind.Object) continue;
                        if(!photo.TryGetProperty("guid", out JsonElement gj) ||
                           gj.ValueKind != JsonValueKind.String) continue;
                        if(!Guid.TryParse(gj.GetString(), out Guid pg)) continue;

                        string acceptKey = Suggestions.SoundSynthPhotoSuggestionApplier.PhotoAcceptKey(pg);

                        // Skip files that were just promoted (the applier deleted the sidecar
                        // already; calling Delete here is harmless thanks to the silent
                        // fallback but we save the IO).
                        if(accepted.Contains(acceptKey)) continue;

                        Marechai.Server.Helpers.PendingImageStore.Delete(_assetRootPath, "sound-synths", pg);
                    }

                    break;
                }
            }
        }
        catch
        {
            // ignored — review must not fail because of pending-media cleanup
        }
    }

    // ───────────────────────────── Helpers ─────────────────────────────

    /// <summary>
    ///     Count the photos referenced by a GPU-photo batch suggestion's
    ///     <c>SuggestedValues["photos"]</c> array. Returns 0 when the array is missing or
    ///     malformed (defensive — the validator at submit time should catch malformed
    ///     payloads before persistence, but this guards against post-hoc DB hand-edits).
    /// </summary>
    static int CountSuggestedGpuPhotos(Suggestion s)
    {
        if(s?.SuggestedValues is null) return 0;
        if(!s.SuggestedValues.TryGetValue(Suggestions.GpuPhotoSuggestionApplier.FieldPhotos, out object raw))
            return 0;

        return raw switch
        {
            JsonElement je when je.ValueKind == JsonValueKind.Array => je.GetArrayLength(),
            System.Collections.ICollection col                      => col.Count,
            System.Collections.IEnumerable enumerable               => enumerable.Cast<object>().Count(),
            _                                                       => 0
        };
    }

    /// <summary>
    ///     Count the photos referenced by a Processor-photo batch suggestion's
    ///     <c>SuggestedValues["photos"]</c> array. Returns 0 when the array is missing or
    ///     malformed (defensive — the validator at submit time should catch malformed
    ///     payloads before persistence, but this guards against post-hoc DB hand-edits).
    /// </summary>
    static int CountSuggestedProcessorPhotos(Suggestion s)
    {
        if(s?.SuggestedValues is null) return 0;
        if(!s.SuggestedValues.TryGetValue(Suggestions.ProcessorPhotoSuggestionApplier.FieldPhotos, out object raw))
            return 0;

        return raw switch
        {
            JsonElement je when je.ValueKind == JsonValueKind.Array => je.GetArrayLength(),
            System.Collections.ICollection col                      => col.Count,
            System.Collections.IEnumerable enumerable               => enumerable.Cast<object>().Count(),
            _                                                       => 0
        };
    }

    /// <summary>
    ///     Count the photos referenced by a SoundSynth-photo batch suggestion's
    ///     <c>SuggestedValues["photos"]</c> array. Returns 0 when the array is missing or
    ///     malformed (defensive — the validator at submit time should catch malformed
    ///     payloads before persistence, but this guards against post-hoc DB hand-edits).
    /// </summary>
    static int CountSuggestedSoundSynthPhotos(Suggestion s)
    {
        if(s?.SuggestedValues is null) return 0;
        if(!s.SuggestedValues.TryGetValue(Suggestions.SoundSynthPhotoSuggestionApplier.FieldPhotos, out object raw))
            return 0;

        return raw switch
        {
            JsonElement je when je.ValueKind == JsonValueKind.Array => je.GetArrayLength(),
            System.Collections.ICollection col                      => col.Count,
            System.Collections.IEnumerable enumerable               => enumerable.Cast<object>().Count(),
            _                                                       => 0
        };
    }

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

    /// <summary>
    ///     Best-effort coercion of a raw JSON-deserialised value to an <see cref="int" />
    ///     for "is this a defined enum value" validation. Returns <c>true</c> when a
    ///     numeric value could be extracted; the caller is responsible for the
    ///     <see cref="Enum.IsDefined(System.Type, object)" /> check on the result.
    /// </summary>
    static bool TryExtractIntForValidation(object v, out int value)
    {
        value = 0;
        switch(v)
        {
            case null: return false;
            case int i: value = i; return true;
            case short s: value = s; return true;
            case long l when l >= int.MinValue && l <= int.MaxValue: value = (int)l; return true;
            case byte b: value = b; return true;
            case JsonElement je:
                if(je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out int p)) { value = p; return true; }
                if(je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int ps)) { value = ps; return true; }
                return false;
            case string str: return int.TryParse(str, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out value);
            default: return false;
        }
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

        // GPU-photo batch suggestions get a per-photo summary instead of the generic
        // "X of Y suggested change(s)" wording. The total photo count comes from the
        // suggested 'photos' array; the accepted count comes from the AppliedFields keys
        // (only photo.<guid> keys count — license_id / source_url are batch-level and
        // applied implicitly with each accepted photo).
        if(s.EntityType == SuggestionEntityType.GpuPhoto)
        {
            int photoTotal    = CountSuggestedGpuPhotos(s);
            int photoAccepted = (s.AppliedFields ?? new Dictionary<string, string>())
                .Count(kv => kv.Key.StartsWith("photo.", StringComparison.Ordinal));

            string plural = photoTotal == 1 ? "photo" : "photos";

            if(photoAccepted == 0)
            {
                subject = "Your GPU photo upload was not accepted";
                body =
                    $"Your suggested upload of {photoTotal} {plural} for {entityRef} was reviewed but no photos were accepted. " +
                    $"Thank you for contributing — feel free to refine and try again.";
            }
            else if(photoAccepted == photoTotal)
            {
                subject = "Your GPU photo upload was accepted";
                body =
                    $"Your suggested upload of {photoTotal} {plural} for {entityRef} was accepted. Thank you!";
                if(roleGranted) body += "\n\nYou are now a Collaborator!";
            }
            else
            {
                subject = "Your GPU photo upload was partially accepted";
                body =
                    $"Your suggested upload for {entityRef} was reviewed. " +
                    $"{photoAccepted} of {photoTotal} {plural} were accepted; the rest were declined.";
                if(roleGranted) body += "\n\nYou are now a Collaborator!";
            }
        }
        else if(s.EntityType == SuggestionEntityType.ProcessorPhoto)
        {
            int photoTotal    = CountSuggestedProcessorPhotos(s);
            int photoAccepted = (s.AppliedFields ?? new Dictionary<string, string>())
                .Count(kv => kv.Key.StartsWith("photo.", StringComparison.Ordinal));

            string plural = photoTotal == 1 ? "photo" : "photos";

            if(photoAccepted == 0)
            {
                subject = "Your processor photo upload was not accepted";
                body =
                    $"Your suggested upload of {photoTotal} {plural} for {entityRef} was reviewed but no photos were accepted. " +
                    $"Thank you for contributing — feel free to refine and try again.";
            }
            else if(photoAccepted == photoTotal)
            {
                subject = "Your processor photo upload was accepted";
                body =
                    $"Your suggested upload of {photoTotal} {plural} for {entityRef} was accepted. Thank you!";
                if(roleGranted) body += "\n\nYou are now a Collaborator!";
            }
            else
            {
                subject = "Your processor photo upload was partially accepted";
                body =
                    $"Your suggested upload for {entityRef} was reviewed. " +
                    $"{photoAccepted} of {photoTotal} {plural} were accepted; the rest were declined.";
                if(roleGranted) body += "\n\nYou are now a Collaborator!";
            }
        }
        else if(s.EntityType == SuggestionEntityType.SoundSynthPhoto)
        {
            int photoTotal    = CountSuggestedSoundSynthPhotos(s);
            int photoAccepted = (s.AppliedFields ?? new Dictionary<string, string>())
                .Count(kv => kv.Key.StartsWith("photo.", StringComparison.Ordinal));

            string plural = photoTotal == 1 ? "photo" : "photos";

            if(photoAccepted == 0)
            {
                subject = "Your sound synth photo upload was not accepted";
                body =
                    $"Your suggested upload of {photoTotal} {plural} for {entityRef} was reviewed but no photos were accepted. " +
                    $"Thank you for contributing — feel free to refine and try again.";
            }
            else if(photoAccepted == photoTotal)
            {
                subject = "Your sound synth photo upload was accepted";
                body =
                    $"Your suggested upload of {photoTotal} {plural} for {entityRef} was accepted. Thank you!";
                if(roleGranted) body += "\n\nYou are now a Collaborator!";
            }
            else
            {
                subject = "Your sound synth photo upload was partially accepted";
                body =
                    $"Your suggested upload for {entityRef} was reviewed. " +
                    $"{photoAccepted} of {photoTotal} {plural} were accepted; the rest were declined.";
                if(roleGranted) body += "\n\nYou are now a Collaborator!";
            }
        }
        else if(wasAddition)
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
        SuggestionEntityType.MagazineIssue      => $"/magazine/issue/{entityId}",
        SuggestionEntityType.Gpu                => $"/gpu/{entityId}",
        SuggestionEntityType.GpuDescription     => $"/gpu/{entityId}",
        SuggestionEntityType.GpuPhoto           => $"/gpu/{entityId}",
        SuggestionEntityType.Processor          => $"/processor/{entityId}",
        SuggestionEntityType.ProcessorDescription => $"/processor/{entityId}",
        SuggestionEntityType.ProcessorPhoto       => $"/processor/{entityId}",
        SuggestionEntityType.SoundSynth           => $"/soundsynth/{entityId}",
        SuggestionEntityType.SoundSynthDescription => $"/soundsynth/{entityId}",
        SuggestionEntityType.SoundSynthPhoto       => $"/soundsynth/{entityId}",
        SuggestionEntityType.Person                => $"/person/{entityId}",
        SuggestionEntityType.PersonDescription     => $"/person/{entityId}",
        SuggestionEntityType.Software              => $"/software/{entityId}",
        SuggestionEntityType.SoftwareDescription   => $"/software/{entityId}",
        SuggestionEntityType.SoftwareRelease       => $"/software/release/{entityId}",
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
        SuggestionEntityType.GpuDescription      => "GPU description",
        SuggestionEntityType.GpuPhoto            => "GPU photo upload",
        SuggestionEntityType.ProcessorPhoto      => "processor photo upload",
        SuggestionEntityType.SoundSynthPhoto     => "sound synth photo upload",
        SuggestionEntityType.ProcessorDescription => "processor description",
        SuggestionEntityType.SoundSynthDescription => "sound synth description",
        SuggestionEntityType.PersonDescription   => "person biography",
        SuggestionEntityType.MachineFamily       => "machine family",
        SuggestionEntityType.Processor           => "processor",
        SuggestionEntityType.Gpu                 => "GPU",
        SuggestionEntityType.SoundSynth          => "sound synth",
        SuggestionEntityType.Software            => "software",
        SuggestionEntityType.SoftwareDescription => "software description",
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
