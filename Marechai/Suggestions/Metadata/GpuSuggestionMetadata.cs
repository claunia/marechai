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
///     Suggestion metadata for the <see cref="GpuDto" /> entity. Exposes 10 directly
///     editable scalar fields PLUS dynamic junction operation keys
///     (<c>&lt;group&gt;.add.&lt;uuid&gt;</c> and <c>&lt;group&gt;.remove.&lt;row_id&gt;</c>)
///     for the single suggestable junction table (Resolutions). Mirrors
///     <see cref="BookSuggestionMetadata" /> minus the cover-pending field (Gpu has no
///     <c>CoverGuid</c> column) and minus People/Companies/Machines/MachineFamilies
///     junctions.
/// </summary>
public sealed class GpuSuggestionMetadata : SuggestionMetadata
{
    // ---- Scalar field names (mirror server-side constants) -----------------------------
    public const string FieldName                = "name";
    public const string FieldCompanyId           = "company_id";
    public const string FieldModelCode           = "model_code";
    public const string FieldIntroduced          = "introduced";
    public const string FieldIntroducedPrecision = "introduced_precision";
    public const string FieldPackage             = "package";
    public const string FieldProcess             = "process";
    public const string FieldProcessNm           = "process_nm";
    public const string FieldDieSize             = "die_size";
    public const string FieldTransistors         = "transistors";

    // ---- Junction group identifiers (mirror server-side) -------------------------------
    public const string GroupResolutions = "resolutions";

    static readonly HashSet<string> s_junctionGroups = new(StringComparer.Ordinal)
    {
        GroupResolutions
    };

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldName, FieldCompanyId, FieldModelCode, FieldIntroduced, FieldIntroducedPrecision,
        FieldPackage, FieldProcess, FieldProcessNm, FieldDieSize, FieldTransistors
    };

    static readonly IReadOnlyList<SuggestionEnumOption> PrecisionOptions = new[]
    {
        new SuggestionEnumOption(0, "Full date"),
        new SuggestionEnumOption(1, "Month and year only"),
        new SuggestionEnumOption(2, "Year only")
    };

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldName,                "Name",              SuggestionFieldKind.Text, MaxLength: 128),
        new(FieldCompanyId,           "Company",           SuggestionFieldKind.ForeignKeyCompany),
        new(FieldModelCode,           "Model code",        SuggestionFieldKind.Text, MaxLength: 45),
        new(FieldIntroduced,          "Introduction date", SuggestionFieldKind.Date),
        new(FieldIntroducedPrecision, "Date precision",    SuggestionFieldKind.Enum, EnumOptions: PrecisionOptions),
        new(FieldPackage,             "Package",           SuggestionFieldKind.Text, MaxLength: 45),
        new(FieldProcess,             "Process",           SuggestionFieldKind.Text, MaxLength: 45),
        // Float fields (process_nm, die_size) and big-int (transistors) are rendered via
        // the base FormatDisplayValue ToString fall-back; using Text avoids needing a new
        // SuggestionFieldKind. The dialog uses MudNumericField<float?>/<long?> directly.
        new(FieldProcessNm,           "Process (nm)",      SuggestionFieldKind.Text),
        new(FieldDieSize,             "Die size (mm²)",    SuggestionFieldKind.Text),
        new(FieldTransistors,         "Transistors",       SuggestionFieldKind.Text)
    };

    static GpuSuggestionMetadata() => SuggestionMetadataRegistry.Register(new GpuSuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor (and registry self-registration) runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.Gpu;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        if(currentDto is not GpuDto g) return result;

        result[FieldName]                = g.Name;
        result[FieldCompanyId]           = g.CompanyId;
        result[FieldModelCode]           = g.ModelCode;
        result[FieldIntroduced]          = g.Introduced?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        result[FieldIntroducedPrecision] = (int?)g.IntroducedPrecision;
        result[FieldPackage]             = g.Package;
        result[FieldProcess]             = g.Process;
        result[FieldProcessNm]           = g.ProcessNm;
        result[FieldDieSize]             = g.DieSize;
        result[FieldTransistors]         = g.Transistors;

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
    ///     Override synthesises a localiser key for dynamic junction keys (e.g.
    ///     <c>"Supported resolutions"</c> for <c>resolutions.{add,remove}.&lt;token&gt;</c>);
    ///     falls back to the static descriptor label for scalar fields.
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
        GroupResolutions => "Supported resolutions",
        _                => group
    };

    /// <summary>
    ///     Parse a junction operation field-name. Mirror of the server-side helper in
    ///     <c>Marechai.Server.Suggestions.GpuSuggestionApplier.TryParseJunctionKey</c>.
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

        // FK ids: fall back to "#{id}" when the server didn't resolve a display label.
        if(fieldName == FieldCompanyId)
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
