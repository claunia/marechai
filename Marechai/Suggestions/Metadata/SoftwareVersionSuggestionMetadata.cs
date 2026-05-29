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
///     Suggestion metadata for the <see cref="SoftwareVersionDto" /> entity. Combined
///     parent+child atomic-creation shape — mirrors <see cref="SoftwareSuggestionMetadata" />
///     verbatim except that the parent here is a SoftwareVersion (rather than a Software)
///     and there are no editable junctions on the version side. Exposes 5 directly editable
///     scalar fields PLUS the dual-prefix delegation surface for first-release scalars and
///     junction-op keys.
///
///     <para>
///         No junction groups are exposed on the SoftwareVersion half (Companies stays
///         admin-only, matching the existing admin <c>SoftwareVersionDialog</c> scope).
///         The release-side junctions surface via the <c>first_release.&lt;group&gt;.add.X</c>
///         shape and are resolved through the lazy <see cref="ReleaseMetadata" /> accessor.
///     </para>
/// </summary>
public sealed class SoftwareVersionSuggestionMetadata : SuggestionMetadata
{
    // ---- Scalar field names (mirror server-side constants) -----------------------------
    public const string FieldVersionString   = "version_string";
    public const string FieldPublicVersion   = "public_version";
    public const string FieldCodename        = "codename";
    public const string FieldParentVersionId = "parent_version_id";
    public const string FieldLicenseId       = "license_id";

    /// <summary>
    ///     Pseudo-field carrying the parent <see cref="SoftwareDto" /> FK at addition time
    ///     only. Mirrors the server-side
    ///     <c>SoftwareVersionSuggestionApplier.FieldSoftwareId</c>. NOT in
    ///     <c>s_scalarFieldNames</c> so the (unused-today) edit path would silently ignore
    ///     it; whitelisted in <see cref="IsKnownFieldName" /> so the wire-side field-name
    ///     validator accepts it.
    /// </summary>
    public const string FieldSoftwareId = "software_id";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldVersionString,
        FieldPublicVersion,
        FieldCodename,
        FieldParentVersionId,
        FieldLicenseId
    };

    // ---- First-release pseudo-prefixes (mirror server-side; creation-mode only) --------
    /// <summary>
    ///     Field-name prefix for first-release SCALAR fields embedded in a SoftwareVersion
    ///     creation payload (e.g. <c>first_release_title</c>). Stripped before delegating
    ///     to <see cref="SoftwareReleaseSuggestionMetadata" /> for value formatting.
    /// </summary>
    public const string FirstReleaseScalarPrefix = "first_release_";

    /// <summary>
    ///     Field-name prefix for first-release JUNCTION operation keys embedded in a
    ///     SoftwareVersion creation payload (e.g.
    ///     <c>first_release.regions.add.&lt;uuid&gt;</c>).
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

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldVersionString,   "Version",        SuggestionFieldKind.Text),
        new(FieldPublicVersion,   "Public version", SuggestionFieldKind.Text),
        new(FieldCodename,        "Codename",       SuggestionFieldKind.Text),
        new(FieldParentVersionId, "Parent version", SuggestionFieldKind.Text),
        new(FieldLicenseId,       "License",        SuggestionFieldKind.Text)
    };

    static SoftwareVersionSuggestionMetadata() =>
        SuggestionMetadataRegistry.Register(new SoftwareVersionSuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.SoftwareVersion;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        if(currentDto is not SoftwareVersionDto v) return result;

        result[FieldVersionString]   = v.VersionString;
        result[FieldPublicVersion]   = v.PublicVersion;
        result[FieldCodename]        = v.Codename;
        result[FieldParentVersionId] = v.ParentVersionId;
        result[FieldLicenseId]       = v.LicenseId;

        return result;
    }

    /// <summary>
    ///     Accepts static scalar field names, the parent-FK pseudo-field, AND first-release-
    ///     prefixed keys (delegated to <see cref="SoftwareReleaseSuggestionMetadata" />).
    ///     No junction-key parsing — Companies is admin-only on the version side.
    /// </summary>
    public override bool IsKnownFieldName(string name)
    {
        if(string.IsNullOrEmpty(name)) return false;
        if(s_scalarFieldNames.Contains(name)) return true;
        if(name == FieldSoftwareId) return true;

        // Test dot-prefix first (see SoftwareSuggestionMetadata comment for ordering).
        if(name.StartsWith(FirstReleaseGroupPrefix, StringComparison.Ordinal))
            return ReleaseMetadata.IsKnownFieldName(name.Substring(FirstReleaseGroupPrefix.Length));
        if(name.StartsWith(FirstReleaseScalarPrefix, StringComparison.Ordinal))
            return ReleaseMetadata.IsKnownFieldName(name.Substring(FirstReleaseScalarPrefix.Length));

        return false;
    }

    /// <summary>
    ///     Falls back to the static descriptor lookup; first-release-prefixed keys delegate
    ///     to the release metadata so junction-add/remove keys are rendered with the same
    ///     row layout as a stand-alone release suggestion.
    /// </summary>
    public override SuggestionFieldKind GetFieldKind(string name)
    {
        if(FieldsByName.TryGetValue(name, out SuggestionFieldDescriptor d)) return d.Kind;

        if(name != null)
        {
            if(name.StartsWith(FirstReleaseGroupPrefix, StringComparison.Ordinal))
                return ReleaseMetadata.GetFieldKind(name.Substring(FirstReleaseGroupPrefix.Length));
            if(name.StartsWith(FirstReleaseScalarPrefix, StringComparison.Ordinal))
                return ReleaseMetadata.GetFieldKind(name.Substring(FirstReleaseScalarPrefix.Length));
        }

        return SuggestionFieldKind.Text;
    }

    /// <summary>
    ///     Returns the descriptor label for static fields; for first-release-prefixed keys
    ///     returns the release-side label prepended with <c>"First release: "</c> so the
    ///     admin diff panel groups the release rows visually.
    /// </summary>
    public override string GetFieldLabelKey(string name)
    {
        if(FieldsByName.TryGetValue(name, out SuggestionFieldDescriptor d)) return d.LabelKey;

        if(name != null)
        {
            if(name.StartsWith(FirstReleaseGroupPrefix, StringComparison.Ordinal))
                return "First release: " + ReleaseMetadata.GetFieldLabelKey(name.Substring(FirstReleaseGroupPrefix.Length));
            if(name.StartsWith(FirstReleaseScalarPrefix, StringComparison.Ordinal))
                return "First release: " + ReleaseMetadata.GetFieldLabelKey(name.Substring(FirstReleaseScalarPrefix.Length));
        }

        return name;
    }

    public override string FormatDisplayValue(string fieldName, object value)
    {
        if(value is null) return string.Empty;

        // First-release-prefixed values: delegate to the release-side formatter so
        // rendering matches a stand-alone release suggestion.
        if(fieldName != null)
        {
            if(fieldName.StartsWith(FirstReleaseGroupPrefix, StringComparison.Ordinal))
                return ReleaseMetadata.FormatDisplayValue(fieldName.Substring(FirstReleaseGroupPrefix.Length), value);
            if(fieldName.StartsWith(FirstReleaseScalarPrefix, StringComparison.Ordinal))
                return ReleaseMetadata.FormatDisplayValue(fieldName.Substring(FirstReleaseScalarPrefix.Length), value);
        }

        // FK ids: "#{id}" fallback when server didn't resolve a readable label.
        if(fieldName == FieldParentVersionId)
        {
            ulong? u = ToUlong(value);
            return u.HasValue ? $"#{u.Value}" : string.Empty;
        }
        if(fieldName == FieldLicenseId)
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
            int i          => i,
            short s        => s,
            long l         => (int?)l,
            ulong u        => (int?)u,
            JsonElement je => je.ValueKind == JsonValueKind.Number ? je.GetInt32() : null,
            string str     => int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int p)
                                  ? p
                                  : null,
            _              => null
        };
    }

    static ulong? ToUlong(object v)
    {
        return v switch
        {
            ulong u        => u,
            uint u         => u,
            int i          => i >= 0 ? (ulong)i : null,
            short s        => s >= 0 ? (ulong)s : null,
            long l         => l >= 0 ? (ulong)l : null,
            byte b         => b,
            JsonElement je => je.ValueKind == JsonValueKind.Number
                                  ? je.TryGetUInt64(out ulong p) ? p :
                                    je.TryGetInt64(out long pl) && pl >= 0 ? (ulong)pl : null
                                  : null,
            string str     => ulong.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong p)
                                  ? p
                                  : null,
            _              => null
        };
    }
}
