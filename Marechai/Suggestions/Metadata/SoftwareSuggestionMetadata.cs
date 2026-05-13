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
///     Suggestion metadata for the <see cref="SoftwareDto" /> entity. Exposes 5 directly
///     editable scalar fields PLUS dynamic junction operation keys
///     (<c>genres.add.&lt;uuid&gt;</c> and <c>genres.remove.&lt;genre_id&gt;</c>) for the
///     single in-scope junction table (Genres). Mirrors the design of
///     <see cref="GpuSuggestionMetadata" /> with three differences:
///     <list type="bullet">
///         <item>
///             <see cref="SoftwareDto" /> is keyed by <c>ulong</c> server-side but Kiota
///             widens to <c>int?</c> on the wire, so all FK extractions box as <c>int?</c>.
///         </item>
///         <item>
///             <c>kind</c> is a NON-NULLABLE enum (<see cref="SoftwareKind" />) — there is no
///             "no value" sentinel; the dialog uses a non-clearable select.
///         </item>
///         <item>
///             The genres remove token is the genre id itself (composite-key junction has no
///             surrogate Id column), so the JunctionRemove diff row is keyed by the picked
///             genre's database id rather than a per-link row id.
///         </item>
///     </list>
///     The People / Companies junctions are deferred (admin-only); only Genres is suggestable.
/// </summary>
public sealed class SoftwareSuggestionMetadata : SuggestionMetadata
{
    // ---- Scalar field names (mirror server-side constants) -----------------------------
    public const string FieldName           = "name";
    public const string FieldFamilyId       = "family_id";
    public const string FieldPredecessorId  = "predecessor_id";
    public const string FieldKind           = "kind";
    public const string FieldBaseSoftwareId = "base_software_id";

    // ---- Junction group identifiers (mirror server-side) -------------------------------
    public const string GroupGenres    = "genres";
    public const string GroupCompanies = "companies";
    public const string GroupCredits   = "credits";

    static readonly HashSet<string> s_junctionGroups = new(StringComparer.Ordinal)
    {
        GroupGenres,
        GroupCompanies,
        GroupCredits
    };

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldName, FieldFamilyId, FieldPredecessorId, FieldKind, FieldBaseSoftwareId
    };

    // ---- First-release pseudo-prefixes (mirror server-side; creation-mode only) --------
    /// <summary>
    ///     Field-name prefix for first-release SCALAR fields embedded in a Software
    ///     creation payload (e.g. <c>first_release_title</c>). Stripped before delegating
    ///     to <see cref="SoftwareReleaseSuggestionMetadata" /> for value formatting.
    /// </summary>
    public const string FirstReleaseScalarPrefix = "first_release_";

    /// <summary>
    ///     Field-name prefix for first-release JUNCTION operation keys embedded in a
    ///     Software creation payload (e.g. <c>first_release.regions.add.&lt;uuid&gt;</c>).
    /// </summary>
    public const string FirstReleaseGroupPrefix = "first_release.";

    /// <summary>
    ///     Lazily-instantiated release-side metadata for delegation. Allocated once on
    ///     first read; the static ctor of <see cref="SoftwareReleaseSuggestionMetadata" />
    ///     registers a separate instance with the registry — that's fine because both
    ///     instances are identical, immutable behaviour bags.
    /// </summary>
    static SoftwareReleaseSuggestionMetadata s_releaseMetadata;
    static SoftwareReleaseSuggestionMetadata ReleaseMetadata =>
        s_releaseMetadata ??= new SoftwareReleaseSuggestionMetadata();

    static readonly IReadOnlyList<SuggestionEnumOption> KindOptions = new[]
    {
        new SuggestionEnumOption(0,  "Generic software"),
        new SuggestionEnumOption(1,  "Operating system"),
        new SuggestionEnumOption(2,  "Videogame"),
        new SuggestionEnumOption(3,  "DLC / Addon"),
        new SuggestionEnumOption(4,  "System software"),
        new SuggestionEnumOption(5,  "Application"),
        new SuggestionEnumOption(6,  "Development software"),
        new SuggestionEnumOption(7,  "Server software"),
        new SuggestionEnumOption(8,  "Middleware"),
        new SuggestionEnumOption(9,  "Firmware"),
        new SuggestionEnumOption(10, "Embedded software")
    };

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldName,           "Name",          SuggestionFieldKind.Text),
        new(FieldFamilyId,       "Family",        SuggestionFieldKind.Text),
        new(FieldPredecessorId,  "Predecessor",   SuggestionFieldKind.Text),
        new(FieldKind,           "Kind",          SuggestionFieldKind.Enum, EnumOptions: KindOptions),
        new(FieldBaseSoftwareId, "Base software", SuggestionFieldKind.Text)
    };

    static SoftwareSuggestionMetadata() => SuggestionMetadataRegistry.Register(new SoftwareSuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor (and registry self-registration) runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.Software;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        if(currentDto is not SoftwareDto s) return result;

        result[FieldName]           = s.Name;
        // Kiota widens ulong? FKs to int? on the wire — fine: the diff panel just renders
        // numbers via FormatDisplayValue and the server-side ResolveSoftwareLabelsAsync
        // populates readable names that override the bare-id rendering.
        result[FieldFamilyId]       = s.FamilyId;
        result[FieldPredecessorId]  = s.PredecessorId;
        result[FieldKind]           = s.Kind;
        result[FieldBaseSoftwareId] = s.BaseSoftwareId;

        return result;
    }

    /// <summary>
    ///     Override accepts both static scalar field names AND junction operation keys of the
    ///     form <c>&lt;group&gt;.{add,remove}.&lt;token&gt;</c>. Also accepts first-release-
    ///     prefixed keys (<c>first_release_*</c> / <c>first_release.*</c>) by delegating
    ///     the stripped key to <see cref="SoftwareReleaseSuggestionMetadata" /> — these
    ///     only appear in brand-new-Software submissions where a first release is required.
    /// </summary>
    public override bool IsKnownFieldName(string name)
    {
        if(string.IsNullOrEmpty(name)) return false;
        if(s_scalarFieldNames.Contains(name)) return true;
        if(TryParseJunctionKey(name, out _, out _, out _)) return true;
        if(name.StartsWith(FirstReleaseGroupPrefix, StringComparison.Ordinal))
            return ReleaseMetadata.IsKnownFieldName(name.Substring(FirstReleaseGroupPrefix.Length));
        if(name.StartsWith(FirstReleaseScalarPrefix, StringComparison.Ordinal))
            return ReleaseMetadata.IsKnownFieldName(name.Substring(FirstReleaseScalarPrefix.Length));
        return false;
    }

    /// <summary>
    ///     Override returns <see cref="SuggestionFieldKind.JunctionAdd" /> /
    ///     <see cref="SuggestionFieldKind.JunctionRemove" /> for dynamic junction keys; falls
    ///     back to the static descriptor lookup for scalar fields. First-release-prefixed
    ///     keys delegate to the release metadata so the diff panel renders them with the
    ///     same kind (and thus the same row layout) as a stand-alone release suggestion.
    /// </summary>
    public override SuggestionFieldKind GetFieldKind(string name)
    {
        if(FieldsByName.TryGetValue(name, out SuggestionFieldDescriptor d)) return d.Kind;

        if(TryParseJunctionKey(name, out _, out string op, out _))
            return op == "add" ? SuggestionFieldKind.JunctionAdd : SuggestionFieldKind.JunctionRemove;

        if(name.StartsWith(FirstReleaseGroupPrefix, StringComparison.Ordinal))
            return ReleaseMetadata.GetFieldKind(name.Substring(FirstReleaseGroupPrefix.Length));
        if(name.StartsWith(FirstReleaseScalarPrefix, StringComparison.Ordinal))
            return ReleaseMetadata.GetFieldKind(name.Substring(FirstReleaseScalarPrefix.Length));

        return SuggestionFieldKind.Text;
    }

    /// <summary>
    ///     Override synthesises a localiser key for dynamic junction keys (e.g.
    ///     <c>"Genres"</c> for <c>genres.{add,remove}.&lt;token&gt;</c>); falls back to the
    ///     static descriptor label for scalar fields. First-release-prefixed keys return
    ///     the release-side label key prepended with <c>"First release: "</c>.
    /// </summary>
    public override string GetFieldLabelKey(string name)
    {
        if(FieldsByName.TryGetValue(name, out SuggestionFieldDescriptor d)) return d.LabelKey;

        if(TryParseJunctionKey(name, out string group, out _, out _))
            return GroupLabelKey(group);

        if(name.StartsWith(FirstReleaseGroupPrefix, StringComparison.Ordinal))
            return "First release: " + ReleaseMetadata.GetFieldLabelKey(name.Substring(FirstReleaseGroupPrefix.Length));
        if(name.StartsWith(FirstReleaseScalarPrefix, StringComparison.Ordinal))
            return "First release: " + ReleaseMetadata.GetFieldLabelKey(name.Substring(FirstReleaseScalarPrefix.Length));

        return name;
    }

    /// <summary>Map a junction group identifier to its localiser key.</summary>
    public static string GroupLabelKey(string group) => group switch
    {
        GroupGenres    => "Genres",
        GroupCompanies => "Companies",
        GroupCredits   => "Credits",
        _              => group
    };

    /// <summary>
    ///     Parse a junction operation field-name. Mirror of the server-side helper in
    ///     <c>Marechai.Server.Suggestions.SoftwareSuggestionApplier.TryParseJunctionKey</c>.
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

        // First-release-prefixed scalar / junction values: delegate to the release
        // metadata's formatter so the rendering matches a stand-alone release suggestion.
        if(fieldName != null)
        {
            if(fieldName.StartsWith(FirstReleaseGroupPrefix, StringComparison.Ordinal))
                return ReleaseMetadata.FormatDisplayValue(fieldName.Substring(FirstReleaseGroupPrefix.Length), value);
            if(fieldName.StartsWith(FirstReleaseScalarPrefix, StringComparison.Ordinal))
                return ReleaseMetadata.FormatDisplayValue(fieldName.Substring(FirstReleaseScalarPrefix.Length), value);
        }

        // SoftwareKind enum int → human-readable label. First port to render a non-nullable
        // enum scalar; mirrors SoundSynth's FormatDisplayValue Type arm.
        if(fieldName == FieldKind)
        {
            int? i = ToInt(value);
            if(!i.HasValue) return string.Empty;
            if(!Enum.IsDefined(typeof(SoftwareKind), i.Value)) return $"#{i.Value}";
            return ((SoftwareKind)i.Value).ToString();
        }

        // FK ids: fall back to "#{id}" when the server didn't resolve a display label.
        if(fieldName == FieldFamilyId || fieldName == FieldPredecessorId || fieldName == FieldBaseSoftwareId)
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
            SoftwareKind k  => (int)k,
            JsonElement je  => je.ValueKind == JsonValueKind.Number ? je.GetInt32() : null,
            string str      => int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int p) ? p : null,
            _               => null
        };
    }
}
