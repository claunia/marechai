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
using Marechai.Data;

namespace Marechai.Suggestions;

/// <summary>
///     Describes the input/display kind of a single suggestable field. Drives both the
///     <c>SuggestionDialog</c> form input choice and the <c>SuggestionDiffPanel</c> formatting.
/// </summary>
public enum SuggestionFieldKind
{
    Text              = 0,
    MultilineText     = 1,
    Url               = 2,
    Date              = 3,
    Int               = 4,
    Short             = 5,
    Bool              = 6,
    Enum              = 7,
    ForeignKeyCountry = 8,
    ForeignKeyCompany = 9,
    /// <summary>
    ///     A long-form markdown body. Rendered with <c>&lt;MarkdownEditor&gt;</c> in the
    ///     suggest dialog and as a colored inline line-diff in the review panel.
    /// </summary>
    Markdown          = 10
}

/// <summary>One enum option for fields with <see cref="SuggestionFieldKind.Enum" />.</summary>
public record SuggestionEnumOption(int Value, string LabelKey);

/// <summary>Field descriptor for a single suggestable field on an entity.</summary>
public record SuggestionFieldDescriptor(
    string                                  Name,
    string                                  LabelKey,
    SuggestionFieldKind                     Kind,
    int?                                    MaxLength    = null,
    IReadOnlyList<SuggestionEnumOption>     EnumOptions  = null);

/// <summary>
///     Per-entity metadata describing which fields can be suggested + how to render their
///     current and suggested values. Implementations live in
///     <c>Marechai/Suggestions/Metadata/{Entity}SuggestionMetadata.cs</c>.
/// </summary>
public abstract class SuggestionMetadata
{
    public abstract SuggestionEntityType EntityType { get; }

    public abstract IReadOnlyList<SuggestionFieldDescriptor> Fields { get; }

    /// <summary>
    ///     Optional name of the field that MUST be filled in for a new-entity suggestion to be
    ///     submitted (e.g. <c>"name"</c> for Company). Returns <c>null</c> when the entity type
    ///     does not support new-entity suggestions or has no mandatory field.
    /// </summary>
    public virtual string PrimaryFieldName => null;

    Dictionary<string, SuggestionFieldDescriptor> _byName;

    public IReadOnlyDictionary<string, SuggestionFieldDescriptor> FieldsByName
    {
        get
        {
            if(_byName is not null) return _byName;
            _byName = new Dictionary<string, SuggestionFieldDescriptor>(StringComparer.Ordinal);
            foreach(SuggestionFieldDescriptor f in Fields) _byName[f.Name] = f;
            return _byName;
        }
    }

    /// <summary>Pull the current values of all suggestable fields from a freshly loaded DTO snapshot.</summary>
    public abstract Dictionary<string, object> ExtractCurrentValues(object currentDto);

    /// <summary>
    ///     Format a raw value (string / JsonElement / number / null) into a human-readable string
    ///     for display in the diff panel. Used for both current-side and suggested-side cells.
    /// </summary>
    public virtual string FormatDisplayValue(string fieldName, object value)
    {
        if(value is null) return string.Empty;
        if(value is JsonElement je) return je.ValueKind == JsonValueKind.Null ? string.Empty : je.ToString();
        return value.ToString() ?? string.Empty;
    }
}
