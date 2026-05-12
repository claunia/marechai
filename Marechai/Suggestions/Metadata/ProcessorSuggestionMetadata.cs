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
///     Suggestion metadata for the <see cref="ProcessorDto" /> entity. Exposes 26 directly
///     editable scalar fields PLUS dynamic junction operation keys
///     (<c>&lt;group&gt;.add.&lt;uuid&gt;</c> and <c>&lt;group&gt;.remove.&lt;row_id&gt;</c>)
///     for the single suggestable junction table (Instruction Set Extensions). Mirrors
///     <see cref="GpuSuggestionMetadata" /> in shape (int-keyed, no cover, single junction).
///     Float fields (process_nm, die_size, l1_instruction, l1_data, l2, l3) and the long
///     transistors field are declared as <see cref="SuggestionFieldKind.Text" /> so the diff
///     panel renders them via the base ToString fallback; the dialog uses typed
///     MudNumericField inputs regardless of the metadata Kind.
/// </summary>
public sealed class ProcessorSuggestionMetadata : SuggestionMetadata
{
    // ---- Scalar field names (mirror server-side constants) -----------------------------
    public const string FieldName                = "name";
    public const string FieldCompanyId           = "company_id";
    public const string FieldModelCode           = "model_code";
    public const string FieldIntroduced          = "introduced";
    public const string FieldIntroducedPrecision = "introduced_precision";
    public const string FieldInstructionSetId    = "instruction_set_id";
    public const string FieldSpeed               = "speed";
    public const string FieldPackage             = "package";
    public const string FieldGprs                = "gprs";
    public const string FieldGprSize             = "gpr_size";
    public const string FieldFprs                = "fprs";
    public const string FieldFprSize             = "fpr_size";
    public const string FieldCores               = "cores";
    public const string FieldThreadsPerCore      = "threads_per_core";
    public const string FieldProcess             = "process";
    public const string FieldProcessNm           = "process_nm";
    public const string FieldDieSize             = "die_size";
    public const string FieldTransistors         = "transistors";
    public const string FieldDataBus             = "data_bus";
    public const string FieldAddressBus          = "address_bus";
    public const string FieldSimdRegisters       = "simd_registers";
    public const string FieldSimdSize            = "simd_size";
    public const string FieldL1Instruction       = "l1_instruction";
    public const string FieldL1Data              = "l1_data";
    public const string FieldL2                  = "l2";
    public const string FieldL3                  = "l3";

    // ---- Junction group identifiers (mirror server-side) -------------------------------
    public const string GroupInstructionSetExtensions = "instruction_set_extensions";

    static readonly HashSet<string> s_junctionGroups = new(StringComparer.Ordinal)
    {
        GroupInstructionSetExtensions
    };

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldName, FieldCompanyId, FieldModelCode, FieldIntroduced, FieldIntroducedPrecision,
        FieldInstructionSetId, FieldSpeed, FieldPackage, FieldGprs, FieldGprSize, FieldFprs,
        FieldFprSize, FieldCores, FieldThreadsPerCore, FieldProcess, FieldProcessNm,
        FieldDieSize, FieldTransistors, FieldDataBus, FieldAddressBus, FieldSimdRegisters,
        FieldSimdSize, FieldL1Instruction, FieldL1Data, FieldL2, FieldL3
    };

    static readonly IReadOnlyList<SuggestionEnumOption> PrecisionOptions = new[]
    {
        new SuggestionEnumOption(0, "Full date"),
        new SuggestionEnumOption(1, "Month and year only"),
        new SuggestionEnumOption(2, "Year only")
    };

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldName,                "Name",              SuggestionFieldKind.Text, MaxLength: 50),
        new(FieldCompanyId,           "Manufacturer",      SuggestionFieldKind.ForeignKeyCompany),
        new(FieldModelCode,           "Model code",        SuggestionFieldKind.Text, MaxLength: 45),
        new(FieldIntroduced,          "Introduction date", SuggestionFieldKind.Date),
        new(FieldIntroducedPrecision, "Date precision",    SuggestionFieldKind.Enum, EnumOptions: PrecisionOptions),
        new(FieldInstructionSetId,    "Instruction set",   SuggestionFieldKind.Text),
        new(FieldSpeed,               "Speed (MHz)",       SuggestionFieldKind.Text),
        new(FieldPackage,             "Package",           SuggestionFieldKind.Text, MaxLength: 45),
        new(FieldGprs,                "GPRs",              SuggestionFieldKind.Text),
        new(FieldGprSize,             "GPR size (bits)",   SuggestionFieldKind.Text),
        new(FieldFprs,                "FPRs",              SuggestionFieldKind.Text),
        new(FieldFprSize,             "FPR size (bits)",   SuggestionFieldKind.Text),
        new(FieldCores,               "Cores",             SuggestionFieldKind.Text),
        new(FieldThreadsPerCore,      "Threads per core",  SuggestionFieldKind.Text),
        new(FieldProcess,             "Process",           SuggestionFieldKind.Text, MaxLength: 45),
        // Float/long scalars use Text + ToString fallback; new SuggestionFieldKind values
        // are unnecessary because the diff panel only special-cases Markdown/JunctionAdd/
        // JunctionRemove/Image — every other kind is rendered via FormatDisplayValue.
        new(FieldProcessNm,           "Process (nm)",      SuggestionFieldKind.Text),
        new(FieldDieSize,             "Die size (mm²)",    SuggestionFieldKind.Text),
        new(FieldTransistors,         "Transistors",       SuggestionFieldKind.Text),
        new(FieldDataBus,             "Data bus (bits)",   SuggestionFieldKind.Text),
        new(FieldAddressBus,          "Address bus (bits)",SuggestionFieldKind.Text),
        new(FieldSimdRegisters,       "SIMD registers",    SuggestionFieldKind.Text),
        new(FieldSimdSize,            "SIMD size (bits)",  SuggestionFieldKind.Text),
        new(FieldL1Instruction,       "L1 instruction (KiB)", SuggestionFieldKind.Text),
        new(FieldL1Data,              "L1 data (KiB)",     SuggestionFieldKind.Text),
        new(FieldL2,                  "L2 (KiB)",          SuggestionFieldKind.Text),
        new(FieldL3,                  "L3 (KiB)",          SuggestionFieldKind.Text)
    };

    static ProcessorSuggestionMetadata() =>
        SuggestionMetadataRegistry.Register(new ProcessorSuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor (and registry self-registration) runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.Processor;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        if(currentDto is not ProcessorDto p) return result;

        result[FieldName]                = p.Name;
        result[FieldCompanyId]           = p.CompanyId;
        result[FieldModelCode]           = p.ModelCode;
        result[FieldIntroduced]          = p.Introduced?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        result[FieldIntroducedPrecision] = (int?)p.IntroducedPrecision;
        result[FieldInstructionSetId]    = p.InstructionSetId;
        result[FieldSpeed]               = p.Speed;
        result[FieldPackage]             = p.Package;
        result[FieldGprs]                = p.Gprs;
        result[FieldGprSize]             = p.GprSize;
        result[FieldFprs]                = p.Fprs;
        result[FieldFprSize]             = p.FprSize;
        result[FieldCores]               = p.Cores;
        result[FieldThreadsPerCore]      = p.ThreadsPerCore;
        result[FieldProcess]             = p.Process;
        result[FieldProcessNm]           = p.ProcessNm;
        result[FieldDieSize]             = p.DieSize;
        result[FieldTransistors]         = p.Transistors;
        result[FieldDataBus]             = p.DataBus;
        result[FieldAddressBus]          = p.AddressBus;
        result[FieldSimdRegisters]       = p.SimdRegisters;
        result[FieldSimdSize]            = p.SimdSize;
        result[FieldL1Instruction]       = p.L1Instruction;
        result[FieldL1Data]              = p.L1Data;
        result[FieldL2]                  = p.L2;
        result[FieldL3]                  = p.L3;

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
    ///     <c>"Instruction set extensions"</c> for
    ///     <c>instruction_set_extensions.{add,remove}.&lt;token&gt;</c>); falls back to the
    ///     static descriptor label for scalar fields.
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
        GroupInstructionSetExtensions => "Instruction set extensions",
        _                              => group
    };

    /// <summary>
    ///     Parse a junction operation field-name. Mirror of the server-side helper in
    ///     <c>Marechai.Server.Suggestions.ProcessorSuggestionApplier.TryParseJunctionKey</c>.
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
        if(fieldName == FieldCompanyId || fieldName == FieldInstructionSetId)
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
