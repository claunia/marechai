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
using System.Text.Json;
using Marechai.ApiClient.Models;
using Marechai.Data;

namespace Marechai.Suggestions.Metadata;

/// <summary>
///     Suggestion metadata for the <see cref="MachineDto" /> entity. Exposes 8 directly
///     editable scalar fields PLUS dynamic junction operation keys (<c>&lt;group&gt;.add.&lt;uuid&gt;</c>
///     and <c>&lt;group&gt;.remove.&lt;row_id&gt;</c>) for the 7 suggestable junction tables
///     (GPUs, Processors, SoundSynths, Screens, Memory, Storage, SoftwarePlatforms).
/// </summary>
public sealed class MachineSuggestionMetadata : SuggestionMetadata
{
    // ---- Scalar field names (mirror server-side constants) -----------------------------
    public const string FieldName                = "name";
    public const string FieldModel               = "model";
    public const string FieldCompanyId           = "company_id";
    public const string FieldType                = "type";
    public const string FieldPrototype           = "prototype";
    public const string FieldIntroduced          = "introduced";
    public const string FieldIntroducedPrecision = "introduced_precision";
    public const string FieldFamilyId            = "family_id";

    // ---- Junction group identifiers (mirror server-side) -------------------------------
    public const string GroupGpus              = "gpus";
    public const string GroupProcessors        = "processors";
    public const string GroupSoundSynths       = "sound_synths";
    public const string GroupScreens           = "screens";
    public const string GroupMemory            = "memory";
    public const string GroupStorage           = "storage";
    public const string GroupSoftwarePlatforms = "software_platforms";

    static readonly HashSet<string> s_junctionGroups = new(StringComparer.Ordinal)
    {
        GroupGpus, GroupProcessors, GroupSoundSynths, GroupScreens,
        GroupMemory, GroupStorage, GroupSoftwarePlatforms
    };

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldName, FieldModel, FieldCompanyId, FieldType, FieldPrototype,
        FieldIntroduced, FieldIntroducedPrecision, FieldFamilyId
    };

    static readonly IReadOnlyList<SuggestionEnumOption> TypeOptions = new[]
    {
        new SuggestionEnumOption(0, "Unknown"),
        new SuggestionEnumOption(1, "Computer"),
        new SuggestionEnumOption(2, "Console"),
        new SuggestionEnumOption(3, "Smartphone")
    };

    static readonly IReadOnlyList<SuggestionEnumOption> PrecisionOptions = new[]
    {
        new SuggestionEnumOption(0, "Full date"),
        new SuggestionEnumOption(1, "Month and year only"),
        new SuggestionEnumOption(2, "Year only")
    };

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldName,                "Name",                  SuggestionFieldKind.Text, MaxLength: 255),
        new(FieldModel,               "Model",                 SuggestionFieldKind.Text, MaxLength: 50),
        new(FieldCompanyId,           "Company",               SuggestionFieldKind.ForeignKeyCompany),
        new(FieldType,                "Type",                  SuggestionFieldKind.Enum, EnumOptions: TypeOptions),
        new(FieldPrototype,           "Prototype",             SuggestionFieldKind.Bool),
        new(FieldIntroduced,          "Introduction date",     SuggestionFieldKind.Date),
        new(FieldIntroducedPrecision, "Date precision",        SuggestionFieldKind.Enum, EnumOptions: PrecisionOptions),
        new(FieldFamilyId,            "Family",                SuggestionFieldKind.ForeignKeyMachineFamily)
    };

    static MachineSuggestionMetadata() => SuggestionMetadataRegistry.Register(new MachineSuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor (and registry self-registration) runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.Machine;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        if(currentDto is not MachineDto m) return result;

        result[FieldName]                = m.Name;
        result[FieldModel]               = m.Model;
        result[FieldCompanyId]           = m.CompanyId;
        result[FieldType]                = (int)m.Type;
        result[FieldPrototype]           = m.Prototype;
        result[FieldIntroduced]          = m.Introduced?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        result[FieldIntroducedPrecision] = (int)m.IntroducedPrecision;
        result[FieldFamilyId]            = m.FamilyId;

        return result;
    }

    /// <summary>
    ///     Override accepts both static scalar field names AND junction operation keys of the
    ///     form <c>&lt;group&gt;.{add,remove}.&lt;token&gt;</c>.
    /// </summary>
    public override bool IsKnownFieldName(string name)
    {
        if(string.IsNullOrEmpty(name)) return false;
        if(s_scalarFieldNames.Contains(name)) return true;
        return TryParseJunctionKey(name, out _, out _, out _);
    }

    /// <summary>
    ///     Override returns <see cref="SuggestionFieldKind.JunctionAdd" /> /
    ///     <see cref="SuggestionFieldKind.JunctionRemove" /> for dynamic junction keys; falls
    ///     back to the static descriptor lookup for scalar fields.
    /// </summary>
    public override SuggestionFieldKind GetFieldKind(string name)
    {
        if(FieldsByName.TryGetValue(name, out SuggestionFieldDescriptor d)) return d.Kind;

        if(TryParseJunctionKey(name, out _, out string op, out _))
            return op == "add" ? SuggestionFieldKind.JunctionAdd : SuggestionFieldKind.JunctionRemove;

        return SuggestionFieldKind.Text;
    }

    /// <summary>
    ///     Override synthesises a localiser key for dynamic junction keys (e.g. <c>"GPUs"</c>
    ///     for <c>gpus.{add,remove}.&lt;token&gt;</c>); falls back to the static descriptor
    ///     label for scalar fields.
    /// </summary>
    public override string GetFieldLabelKey(string name)
    {
        if(FieldsByName.TryGetValue(name, out SuggestionFieldDescriptor d)) return d.LabelKey;

        if(TryParseJunctionKey(name, out string group, out _, out _))
            return GroupLabelKey(group);

        return name;
    }

    /// <summary>Map a junction group identifier to its localiser key.</summary>
    public static string GroupLabelKey(string group) => group switch
    {
        GroupGpus              => "GPUs",
        GroupProcessors        => "Processors",
        GroupSoundSynths       => "Sound Synthesizers",
        GroupScreens           => "Screens",
        GroupMemory            => "Memory",
        GroupStorage           => "Storage",
        GroupSoftwarePlatforms => "Software Platforms",
        _                      => group
    };

    /// <summary>
    ///     Parse a junction operation field-name. Mirror of the server-side helper in
    ///     <c>Marechai.Server.Suggestions.MachineSuggestionApplier.TryParseJunctionKey</c>.
    /// </summary>
    public static bool TryParseJunctionKey(string fieldName, out string group, out string op, out string token)
    {
        group = null;
        op    = null;
        token = null;

        if(string.IsNullOrEmpty(fieldName)) return false;

        string[] parts = fieldName.Split('.', 3);
        if(parts.Length != 3) return false;
        if(!s_junctionGroups.Contains(parts[0])) return false;
        if(parts[1] != "add" && parts[1] != "remove") return false;
        if(string.IsNullOrEmpty(parts[2])) return false;

        group = parts[0];
        op    = parts[1];
        token = parts[2];
        return true;
    }

    public override string FormatDisplayValue(string fieldName, object value)
    {
        if(value is null) return string.Empty;

        // Type enum → label
        if(fieldName == FieldType)
        {
            int? i = ToInt(value);
            return i switch
            {
                0 => "Unknown",
                1 => "Computer",
                2 => "Console",
                3 => "Smartphone",
                _ => string.Empty
            };
        }

        // Precision enum → label
        if(fieldName == FieldIntroducedPrecision)
        {
            int? i = ToInt(value);
            return i switch
            {
                0 => "Full date",
                1 => "Month and year",
                2 => "Year only",
                _ => string.Empty
            };
        }

        // Bool
        if(fieldName == FieldPrototype)
        {
            if(value is bool b) return b ? "Yes" : "No";
            if(value is JsonElement bje) return bje.ValueKind switch
            {
                JsonValueKind.True  => "Yes",
                JsonValueKind.False => "No",
                _                   => string.Empty
            };
        }

        // FK ids: fall back to "#{id}" when the server didn't resolve a display label.
        if(fieldName == FieldCompanyId || fieldName == FieldFamilyId)
        {
            int? i = ToInt(value);
            return i.HasValue ? $"#{i.Value}" : string.Empty;
        }

        if(value is JsonElement je)
            return je.ValueKind == JsonValueKind.Null ? string.Empty : je.ToString();

        return value.ToString() ?? string.Empty;
    }

    static int? ToInt(object v)
    {
        return v switch
        {
            int i           => i,
            short s         => s,
            long l          => (int?)l,
            JsonElement je  => je.ValueKind == JsonValueKind.Number ? je.GetInt32() : null,
            string str      => int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int p) ? p : null,
            _               => null
        };
    }
}
