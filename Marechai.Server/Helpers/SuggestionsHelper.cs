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
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;
using DatabaseDispatcher = Marechai.Database.Helpers.MessageDispatcher;

namespace Marechai.Server.Helpers;

/// <summary>
///     Helpers shared between <c>SuggestionsController</c> and the per-entity controllers
///     that emit stale-on-delete notifications.
/// </summary>
internal static class SuggestionsHelper
{
    static readonly JsonSerializerOptions s_json = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    ///     Marks every Pending suggestion for the given <paramref name="entityType" /> +
    ///     <paramref name="entityId" /> as <see cref="SuggestionStatus.Stale" /> and dispatches
    ///     a system message to each suggesting user. Caller MUST <c>SaveChangesAsync</c> after
    ///     deleting the entity so the FK doesn't dangle, then <see cref="MarkStaleForEntityAsync" />
    ///     does its own SaveChanges through the message dispatcher.
    /// </summary>
    public static async Task MarkStaleForEntityAsync(MarechaiContext context,
                                                     SuggestionEntityType entityType,
                                                     long entityId,
                                                     string entityDisplayName)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));

        List<Suggestion> stale = await context.Suggestions
            .Where(s => s.EntityType == entityType
                     && s.EntityId == entityId
                     && s.Status == SuggestionStatus.Pending)
            .ToListAsync();

        await ApplyStaleAsync(context, stale, entityType, entityDisplayName, subkeyLabel: null);
    }

    /// <summary>
    ///     Mark Pending suggestions stale for a specific entity AND a specific
    ///     <see cref="Suggestion.Subkey" />. Used by per-subkey delete hooks (e.g. removing one
    ///     language of a company description without deleting the company itself).
    /// </summary>
    public static async Task MarkStaleForEntitySubkeyAsync(MarechaiContext context,
                                                           SuggestionEntityType entityType,
                                                           long entityId,
                                                           string subkey,
                                                           string entityDisplayName,
                                                           string subkeyLabel = null)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));
        if(string.IsNullOrEmpty(subkey)) throw new ArgumentException("Subkey is required.", nameof(subkey));

        List<Suggestion> stale = await context.Suggestions
            .Where(s => s.EntityType == entityType
                     && s.EntityId == entityId
                     && s.Subkey == subkey
                     && s.Status == SuggestionStatus.Pending)
            .ToListAsync();

        await ApplyStaleAsync(context, stale, entityType, entityDisplayName, subkeyLabel);
    }

    static async Task ApplyStaleAsync(MarechaiContext context,
                                      List<Suggestion> stale,
                                      SuggestionEntityType entityType,
                                      string entityDisplayName,
                                      string subkeyLabel)
    {
        if(stale.Count == 0) return;

        DateTime now = DateTime.UtcNow;

        foreach(Suggestion s in stale)
        {
            s.Status     = SuggestionStatus.Stale;
            s.ReviewedOn = now;
        }

        await context.SaveChangesAsync();

        // One system message per affected user (a user could conceivably have multiple pending
        // suggestions on the same entity in some edge cases — collapse those into one notification).
        IEnumerable<IGrouping<string, Suggestion>> grouped = stale.GroupBy(s => s.CreatedById);

        foreach(IGrouping<string, Suggestion> g in grouped)
        {
            string typeLabel = EntityLabel(entityType);
            string suffix    = string.IsNullOrEmpty(subkeyLabel) ? string.Empty : " " + subkeyLabel;
            string subject   = $"Your suggestion for {typeLabel} '{entityDisplayName}'{suffix} is no longer applicable";
            string body =
                $"The {typeLabel} **{entityDisplayName}**{suffix} that you suggested changes for has been removed " +
                $"by an administrator, so your suggestion can no longer be reviewed. " +
                $"It has been closed automatically.";

            await DatabaseDispatcher.PostSystemMessageAsync(context,
                                                            new[] { g.Key },
                                                            subject,
                                                            body);
        }
    }

    /// <summary>Friendly lower-case label for the entity type used in message text. Mirrors the controller's helper.</summary>
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
        SuggestionEntityType.ProcessorDescription => "processor description",
        SuggestionEntityType.SoundSynthDescription => "sound synth description",
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

    /// <summary>Serialize a values dictionary to a compact JSON string for the wire.</summary>
    public static string SerializeValues(Dictionary<string, object> values)
    {
        if(values is null || values.Count == 0) return null;
        return JsonSerializer.Serialize(values, s_json);
    }

    /// <summary>Serialize an applied-fields map to a compact JSON string for the wire.</summary>
    public static string SerializeApplied(Dictionary<string, string> applied)
    {
        if(applied is null || applied.Count == 0) return null;
        return JsonSerializer.Serialize(applied, s_json);
    }

    /// <summary>Parse a wire JSON object into a string-keyed dictionary of raw JSON values.</summary>
    public static Dictionary<string, object> DeserializeValues(string json)
    {
        if(string.IsNullOrWhiteSpace(json)) return new Dictionary<string, object>(StringComparer.Ordinal);

        Dictionary<string, JsonElement> raw =
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, s_json) ??
            new Dictionary<string, JsonElement>();

        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        foreach(KeyValuePair<string, JsonElement> kv in raw)
        {
            result[kv.Key] = kv.Value.ValueKind == JsonValueKind.Null
                                 ? null
                                 : (object)kv.Value;
        }

        return result;
    }
}
