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
///     Suggestion metadata for the <see cref="SoundSynthDto" /> entity. Exposes 11 directly
///     editable scalar fields. SoundSynth has no in-scope junctions — both visible
///     junctions (<c>SoundByMachine</c>, <c>SoundSynthBySoftwareRelease</c>) are owned by
///     the other side per established convention and stay admin-only. Mirrors
///     <see cref="GpuSuggestionMetadata" /> minus the resolutions junction overrides.
/// </summary>
public sealed class SoundSynthSuggestionMetadata : SuggestionMetadata
{
    // ---- Scalar field names (mirror server-side constants) -----------------------------
    public const string FieldName                = "name";
    public const string FieldCompanyId           = "company_id";
    public const string FieldModelCode           = "model_code";
    public const string FieldIntroduced          = "introduced";
    public const string FieldIntroducedPrecision = "introduced_precision";
    public const string FieldVoices              = "voices";
    public const string FieldFrequency           = "frequency";
    public const string FieldDepth               = "depth";
    public const string FieldSquareWave          = "square_wave";
    public const string FieldWhiteNoise          = "white_noise";
    public const string FieldType                = "type";

    static readonly IReadOnlyList<SuggestionEnumOption> PrecisionOptions = new[]
    {
        new SuggestionEnumOption(0, "Full date"),
        new SuggestionEnumOption(1, "Month and year only"),
        new SuggestionEnumOption(2, "Year only")
    };

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldName,                "Name",                 SuggestionFieldKind.Text, MaxLength: 50),
        new(FieldCompanyId,           "Manufacturer",         SuggestionFieldKind.ForeignKeyCompany),
        new(FieldModelCode,           "Model code",           SuggestionFieldKind.Text, MaxLength: 45),
        new(FieldIntroduced,          "Introduction date",    SuggestionFieldKind.Date),
        new(FieldIntroducedPrecision, "Date precision",       SuggestionFieldKind.Enum, EnumOptions: PrecisionOptions),
        // Numeric fields are rendered via the base FormatDisplayValue ToString fall-back;
        // using Text avoids needing new SuggestionFieldKind values. The dialog uses
        // MudNumericField<int?>/<double?> directly.
        new(FieldVoices,              "Voices",               SuggestionFieldKind.Text),
        new(FieldFrequency,           "Frequency (Hz)",       SuggestionFieldKind.Text),
        new(FieldDepth,               "Depth (bits)",         SuggestionFieldKind.Text),
        new(FieldSquareWave,          "Square wave channels", SuggestionFieldKind.Text),
        new(FieldWhiteNoise,          "White noise channels", SuggestionFieldKind.Text),
        // Synthesizer type enum: rendered via FormatDisplayValue ToString fall-back; the
        // dialog uses MudSelect<SoundSynthType>.
        new(FieldType,                "Synthesizer type",     SuggestionFieldKind.Text)
    };

    static SoundSynthSuggestionMetadata() => SuggestionMetadataRegistry.Register(new SoundSynthSuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor (and registry self-registration) runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.SoundSynth;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        if(currentDto is not SoundSynthDto s) return result;

        result[FieldName]                = s.Name;
        result[FieldCompanyId]           = s.CompanyId;
        result[FieldModelCode]           = s.ModelCode;
        result[FieldIntroduced]          = s.Introduced?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        result[FieldIntroducedPrecision] = s.IntroducedPrecision;
        result[FieldVoices]              = s.Voices;
        result[FieldFrequency]           = s.Frequency;
        result[FieldDepth]               = s.Depth;
        result[FieldSquareWave]          = s.SquareWave;
        result[FieldWhiteNoise]          = s.WhiteNoise;
        result[FieldType]                = s.Type;

        return result;
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

        // Synthesizer type enum → label (reads via Enum.IsDefined, falls back to "#id").
        if(fieldName == FieldType)
        {
            int? i = ToInt(value);
            if(!i.HasValue) return string.Empty;
            return Enum.IsDefined(typeof(SoundSynthType), i.Value)
                       ? ((SoundSynthType)i.Value).ToString()
                       : $"#{i.Value}";
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
            byte b          => b,
            DatePrecision d => (int)d,
            JsonElement je  => je.ValueKind == JsonValueKind.Number ? je.GetInt32() : null,
            string str      => int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int p) ? p : null,
            _               => null
        };
    }
}
