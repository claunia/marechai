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
///     Suggestion metadata for the <see cref="SoftwareReleaseDto" /> entity. Exposes 5
///     directly editable scalar fields PLUS dynamic junction operation keys for FOUR
///     in-scope groups (regions, languages, barcodes, product_codes). Re-parenting fields
///     (<c>software_id</c>, <c>software_version_id</c>, <c>is_compilation</c>) are
///     intentionally admin-only. Mirrors the design of <see cref="SoftwareSuggestionMetadata" />
///     adapted to a ulong-keyed entity with a 4-junction shape.
/// </summary>
public sealed class SoftwareReleaseSuggestionMetadata : SuggestionMetadata
{
    // ---- Scalar field names (mirror server-side constants) -----------------------------
    public const string FieldTitle                = "title";
    public const string FieldPlatformId           = "platform_id";
    public const string FieldPublisherId          = "publisher_id";
    public const string FieldReleaseDate          = "release_date";
    public const string FieldReleaseDatePrecision = "release_date_precision";

    /// <summary>
    ///     Pseudo-field carrying the parent Software FK at addition time only. Mirrors
    ///     the server-side <c>SoftwareReleaseSuggestionApplier.FieldSoftwareId</c>; not in
    ///     <c>s_scalarFieldNames</c> on either side because re-parenting on edit stays
    ///     admin-only. Whitelisted in <see cref="IsKnownFieldName" />.
    /// </summary>
    public const string FieldSoftwareId = "software_id";

    // ---- Junction group identifiers (mirror server-side) -------------------------------
    public const string GroupRegions      = "regions";
    public const string GroupLanguages    = "languages";
    public const string GroupBarcodes     = "barcodes";
    public const string GroupProductCodes = "product_codes";
    public const string GroupSpecs        = "specs";
    public const string GroupRatings      = "ratings";
    public const string GroupMinGpus      = "min_gpus";
    public const string GroupRecGpus      = "rec_gpus";
    public const string GroupSoundSynths  = "sound_synths";

    static readonly HashSet<string> s_junctionGroups = new(StringComparer.Ordinal)
    {
        GroupRegions, GroupLanguages, GroupBarcodes, GroupProductCodes, GroupSpecs, GroupRatings,
        GroupMinGpus, GroupRecGpus, GroupSoundSynths
    };

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldTitle, FieldPlatformId, FieldPublisherId, FieldReleaseDate, FieldReleaseDatePrecision
    };

    static readonly IReadOnlyList<SuggestionEnumOption> DatePrecisionOptions = new[]
    {
        new SuggestionEnumOption(0, "Full date"),
        new SuggestionEnumOption(1, "Month and year only"),
        new SuggestionEnumOption(2, "Year only")
    };

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldTitle,                "Title",                  SuggestionFieldKind.Text),
        new(FieldPlatformId,           "Platform",               SuggestionFieldKind.Text),
        new(FieldPublisherId,          "Publisher",              SuggestionFieldKind.ForeignKeyCompany),
        new(FieldReleaseDate,          "Release date",           SuggestionFieldKind.Date),
        new(FieldReleaseDatePrecision, "Release date precision", SuggestionFieldKind.Enum, EnumOptions: DatePrecisionOptions)
    };

    static SoftwareReleaseSuggestionMetadata() =>
        SuggestionMetadataRegistry.Register(new SoftwareReleaseSuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.SoftwareRelease;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        if(currentDto is not SoftwareReleaseDto r) return result;

        result[FieldTitle]                = r.Title;
        result[FieldPlatformId]           = r.PlatformId;
        result[FieldPublisherId]          = r.PublisherId;
        result[FieldReleaseDate]          = r.ReleaseDate?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        result[FieldReleaseDatePrecision] = r.ReleaseDatePrecision ?? 0;

        return result;
    }

    public override bool IsKnownFieldName(string name)
    {
        if(string.IsNullOrEmpty(name)) return false;
        if(s_scalarFieldNames.Contains(name)) return true;
        if(name == FieldSoftwareId) return true;
        return TryParseJunctionKey(name, out _, out _, out _);
    }

    public override SuggestionFieldKind GetFieldKind(string name)
    {
        if(FieldsByName.TryGetValue(name, out SuggestionFieldDescriptor d)) return d.Kind;

        if(TryParseJunctionKey(name, out _, out string op, out _))
            return op == "add" ? SuggestionFieldKind.JunctionAdd : SuggestionFieldKind.JunctionRemove;

        return SuggestionFieldKind.Text;
    }

    public override string GetFieldLabelKey(string name)
    {
        if(FieldsByName.TryGetValue(name, out SuggestionFieldDescriptor d)) return d.LabelKey;

        if(TryParseJunctionKey(name, out string group, out _, out _))
            return GroupLabelKey(group);

        return name;
    }

    public static string GroupLabelKey(string group) => group switch
    {
        GroupRegions      => "Regions",
        GroupLanguages    => "Languages",
        GroupBarcodes     => "Barcodes",
        GroupProductCodes => "Product codes",
        GroupSpecs        => "Specifications",
        GroupRatings      => "Ratings",
        GroupMinGpus      => "Minimum GPUs",
        GroupRecGpus      => "Recommended GPUs",
        GroupSoundSynths  => "Sound synthesizers",
        _                 => group
    };

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

        // DatePrecision enum int → readable label.
        if(fieldName == FieldReleaseDatePrecision)
        {
            int? i = ToInt(value);
            return i switch
            {
                0    => "Full date",
                1    => "Month and year only",
                2    => "Year only",
                null => string.Empty,
                _    => $"#{i.Value}"
            };
        }

        // FK ids: "#{id}" fallback when server didn't resolve a label.
        if(fieldName == FieldPlatformId || fieldName == FieldPublisherId)
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
            ulong u         => (int?)u,
            JsonElement je  => je.ValueKind == JsonValueKind.Number ? je.GetInt32() : null,
            string str      => int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int p) ? p : null,
            _               => null
        };
    }
}
