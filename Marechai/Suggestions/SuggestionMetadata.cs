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
    Markdown          = 10,
    /// <summary>
    ///     A foreign-key picker for <see cref="Marechai.ApiClient.Models.MachineFamilyDto" />.
    ///     Backed by <see cref="Marechai.Services.MachinesService.GetFamiliesAsync" />.
    /// </summary>
    ForeignKeyMachineFamily = 11,
    /// <summary>
    ///     Marker for a junction-row insertion operation in the diff panel. The field name
    ///     follows the convention <c>&lt;group&gt;.add.&lt;client_uuid&gt;</c> and the value
    ///     carries the row payload (object). Renders as a green "+ Add &lt;Group&gt;" row in the
    ///     review diff with the readable target label resolved server-side via
    ///     <c>SuggestedLabels</c>.
    /// </summary>
    JunctionAdd       = 12,
    /// <summary>
    ///     Marker for a junction-row removal operation in the diff panel. The field name
    ///     follows the convention <c>&lt;group&gt;.remove.&lt;row_id&gt;</c> and the value is
    ///     <c>null</c>. Renders as a red "− Remove &lt;Group&gt;" row in the review diff with
    ///     the readable target label resolved server-side via <c>CurrentLabels</c>.
    /// </summary>
    JunctionRemove    = 13,
    /// <summary>
    ///     Pending image upload (e.g. a collaborator-uploaded book cover). The wire value
    ///     is a guid string referencing a file under
    ///     <c>{itemFolder}/pending/&lt;guid&gt;.&lt;ext&gt;</c>. The diff panel renders a small
    ///     thumbnail (clickable to open a full-size lightbox) using the per-entity
    ///     pending-image GET endpoint. On accept, the applier promotes the file into
    ///     <c>originals/</c> and runs the conversion worker for the usual format/resolution
    ///     variants.
    /// </summary>
    Image             = 14
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
    ///     Returns <c>true</c> when the given field name is acceptable for this entity. Default
    ///     implementation checks the static <see cref="Fields" /> list. Override for entities
    ///     that accept dynamic field-name patterns (e.g. <c>&lt;group&gt;.add.&lt;uuid&gt;</c>
    ///     for junction operations).
    /// </summary>
    public virtual bool IsKnownFieldName(string name) => name is not null && FieldsByName.ContainsKey(name);

    /// <summary>
    ///     Returns the rendering kind for a given field name. Default falls back to the static
    ///     descriptor; override to map dynamic field-name patterns to <see cref="SuggestionFieldKind.JunctionAdd" />
    ///     or <see cref="SuggestionFieldKind.JunctionRemove" />.
    /// </summary>
    public virtual SuggestionFieldKind GetFieldKind(string name) =>
        FieldsByName.TryGetValue(name, out SuggestionFieldDescriptor d) ? d.Kind : SuggestionFieldKind.Text;

    /// <summary>
    ///     Returns the localiser-key for a given field name. Default falls back to the static
    ///     descriptor's <c>LabelKey</c>; override to synthesise labels for dynamic keys (e.g.
    ///     <c>"Add GPU"</c> for <c>gpus.add.&lt;uuid&gt;</c>).
    /// </summary>
    public virtual string GetFieldLabelKey(string name) =>
        FieldsByName.TryGetValue(name, out SuggestionFieldDescriptor d) ? d.LabelKey : name;

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
