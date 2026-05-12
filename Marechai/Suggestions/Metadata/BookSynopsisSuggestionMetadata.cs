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
using System.Text.Json;
using Marechai.ApiClient.Models;
using Marechai.Data;

namespace Marechai.Suggestions.Metadata;

/// <summary>
///     Suggestion metadata for the per-language <c>BookSynopsis</c>. The suggestion's
///     <c>Subkey</c> holds the ISO-639-3 language code; the only suggestable field is the
///     markdown body itself.
/// </summary>
public sealed class BookSynopsisSuggestionMetadata : SuggestionMetadata
{
    /// <summary>Mirror of <c>BookSynopsisSuggestionApplier.FieldMarkdown</c>. KEEP IN SYNC.</summary>
    public const string FieldMarkdown = "markdown";

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldMarkdown, "Synopsis (Markdown)", SuggestionFieldKind.Markdown, MaxLength: 262144)
    };

    static BookSynopsisSuggestionMetadata() =>
        SuggestionMetadataRegistry.Register(new BookSynopsisSuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor (and registry self-registration) runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.BookSynopsis;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        // Book uses the shared DocumentSynopsisDto (no separate BookSynopsisDto). The synopsis
        // markdown is stored in the Text property — there is no separate Html column on
        // DocumentBaseSynopsis, so the Markdown IS the canonical value.
        if(currentDto is DocumentSynopsisDto d)
            result[FieldMarkdown] = d.Text;

        return result;
    }

    public override string FormatDisplayValue(string fieldName, object value)
    {
        if(value is null) return string.Empty;

        if(value is JsonElement je)
            return je.ValueKind == JsonValueKind.Null ? string.Empty : je.ToString();

        return value.ToString() ?? string.Empty;
    }
}
