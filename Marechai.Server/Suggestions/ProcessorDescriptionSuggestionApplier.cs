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
using Marechai.Database.Models;
using Markdig;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Suggestions;

/// <summary>
///     Applies a per-language Processor description suggestion. The suggestion's <c>Subkey</c>
///     holds the ISO-639-3 language code; the <c>SuggestedValues</c> payload contains a single
///     <c>markdown</c> field with the proposed description body.
/// </summary>
internal static class ProcessorDescriptionSuggestionApplier
{
    public const int MaxMarkdownLength = 262144;

    /// <summary>Canonical field name. KEEP IN SYNC with the client metadata.</summary>
    public const string FieldMarkdown = "markdown";

    public static readonly IReadOnlyCollection<string> KnownFieldNames = new HashSet<string>(StringComparer.Ordinal)
    {
        FieldMarkdown
    };

    /// <summary>
    ///     Languages allowed as a Subkey for description suggestions. Mirrors the seeded set in
    ///     the application UI; <c>lat</c> / <c>por</c> are accepted because they are present in
    ///     the database table even though the UI is only translated to en/es/de/fr/it.
    /// </summary>
    public static readonly IReadOnlyCollection<string> AllowedLanguageCodes =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "eng", "spa", "deu", "fra", "ita", "lat", "por"
        };

    public static bool IsAllowedLanguage(string code) =>
        !string.IsNullOrEmpty(code) && AllowedLanguageCodes.Contains(code);

    /// <summary>
    ///     Read the current (markdown) description for the given Processor / language. Returns
    ///     <c>null</c> when the Processor itself does not exist (sentinel for "entity missing");
    ///     returns <c>{ "markdown": "" }</c> when the Processor exists but has no description in
    ///     this language yet.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context,
                                                                               long entityId,
                                                                               string languageCode)
    {
        bool exists = await context.Processors.AsNoTracking().AnyAsync(p => p.Id == (int)entityId);
        if(!exists) return null;

        ProcessorDescription d =
            await context.ProcessorDescriptions.AsNoTracking()
                         .FirstOrDefaultAsync(x => x.ProcessorId == (int)entityId &&
                                                   x.LanguageCode == languageCode);

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldMarkdown] = d?.Text ?? string.Empty
        };
    }

    /// <summary>
    ///     Apply the accepted markdown to the per-language <c>ProcessorDescription</c> row. Renders
    ///     the markdown to HTML server-side via Markdig with advanced extensions, mirroring
    ///     <c>ProcessorsController.CreateOrUpdateDescriptionAsync</c>. Empty / whitespace-only
    ///     markdown is rejected upstream by the controller — this applier never deletes.
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId, string languageCode,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string creditedUserId)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        bool exists = await context.Processors.AnyAsync(p => p.Id == (int)entityId);
        if(!exists) return (applied, true);

        if(!accepted.Contains(FieldMarkdown))
        {
            // Nothing accepted: nothing to do, no missing-entity error either.
            return (applied, false);
        }

        if(!suggested.TryGetValue(FieldMarkdown, out object value)) return (applied, false);

        string markdown = ToStringValue(value);

        if(string.IsNullOrWhiteSpace(markdown))
        {
            // Belt & braces — controller already rejects this, but the review path could still
            // try to accept an old payload if validation was bypassed somehow. Skip silently.
            return (applied, false);
        }

        if(markdown.Length > MaxMarkdownLength) markdown = markdown.Substring(0, MaxMarkdownLength);

        // Render to HTML server-side (mirrors ProcessorsController.CreateOrUpdateDescriptionAsync).
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        string html = Markdown.ToHtml(markdown, pipeline) ?? string.Empty;

        // If the rendered HTML somehow exceeds the column cap, drop it but keep the markdown
        // so the description is not lost (public view falls back to rendering markdown on demand).
        if(html.Length > MaxMarkdownLength) html = string.Empty;

        ProcessorDescription current =
            await context.ProcessorDescriptions.FirstOrDefaultAsync(d => d.ProcessorId == (int)entityId &&
                                                                         d.LanguageCode == languageCode);

        if(current is null)
        {
            current = new ProcessorDescription
            {
                ProcessorId  = (int)entityId,
                LanguageCode = languageCode,
                Text         = markdown,
                Html         = html
            };
            await context.ProcessorDescriptions.AddAsync(current);
        }
        else
        {
            current.Text = markdown;
            current.Html = html;
        }

        await context.SaveChangesWithUserAsync(creditedUserId);

        applied.Add(FieldMarkdown);
        return (applied, false);
    }

    static string ToStringValue(object v)
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
}
