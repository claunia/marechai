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
///     Suggestion metadata for the <see cref="MagazineIssueDto" /> entity. Exposes 9 directly
///     editable scalar fields PLUS dynamic junction operation keys
///     (<c>&lt;group&gt;.add.&lt;uuid&gt;</c> and <c>&lt;group&gt;.remove.&lt;row_id&gt;</c>)
///     for the 4 suggestable junction tables (People, Machines, MachineFamilies, Software).
///     Mirrors <see cref="BookSuggestionMetadata" /> minus the Book-only scalars (title,
///     sort_title, isbn, edition, country_id, previous_id, source_id) and minus the parent
///     <c>magazine_id</c> which is admin-only re-parenting. The <c>software</c> junction
///     replaces Book's <c>companies</c> group.
/// </summary>
public sealed class MagazineIssueSuggestionMetadata : SuggestionMetadata
{
    // ---- Scalar field names (mirror server-side constants) -----------------------------
    public const string FieldCaption            = "caption";
    public const string FieldNativeCaption      = "native_caption";
    public const string FieldPublished          = "published";
    public const string FieldPublishedPrecision = "published_precision";
    public const string FieldProductCode        = "product_code";
    public const string FieldPages              = "pages";
    public const string FieldIssueNumber        = "issue_number";
    public const string FieldInternetArchiveUrl = "internet_archive_url";

    /// <summary>
    ///     Pseudo-field carrying the guid of a pending-cover upload. Mirrors
    ///     <c>MagazineIssueSuggestionApplier.FieldCoverPendingGuid</c>. The dialog uploads
    ///     the file directly to <c>POST /magazines/issues/{id}/cover/pending</c> and embeds
    ///     the returned guid in the diff payload. The extension lives in the pending sidecar
    ///     (no separate wire field needed; the applier reads it on accept).
    /// </summary>
    public const string FieldCoverPendingGuid = "cover_pending_guid";

    /// <summary>
    ///     Pseudo-scalar carrying the parent magazine id for brand-new-issue creation
    ///     (addition mode). Mirrors <c>MagazineIssueSuggestionApplier.FieldMagazineId</c>.
    ///     Intentionally NOT in <c>s_scalarFieldNames</c>: only the addition path consumes
    ///     this key, and the server applier rejects it in <c>ApplyAsync</c> because
    ///     re-parenting an existing issue is admin-only.
    /// </summary>
    public const string FieldMagazineId = "magazine_id";

    // ---- Junction group identifiers (mirror server-side) -------------------------------
    public const string GroupPeople          = "people";
    public const string GroupMachines        = "machines";
    public const string GroupMachineFamilies = "machine_families";
    public const string GroupSoftware        = "software";

    static readonly HashSet<string> s_junctionGroups = new(StringComparer.Ordinal)
    {
        GroupPeople, GroupMachines, GroupMachineFamilies, GroupSoftware
    };

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldCaption, FieldNativeCaption,
        FieldPublished, FieldPublishedPrecision,
        FieldProductCode, FieldPages, FieldIssueNumber,
        FieldInternetArchiveUrl, FieldCoverPendingGuid
    };

    static readonly IReadOnlyList<SuggestionEnumOption> PrecisionOptions = new[]
    {
        new SuggestionEnumOption(0, "Full date"),
        new SuggestionEnumOption(1, "Month and year only"),
        new SuggestionEnumOption(2, "Year only")
    };

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldCaption,            "Caption",              SuggestionFieldKind.Text, MaxLength: 255),
        new(FieldNativeCaption,      "Native caption",       SuggestionFieldKind.Text, MaxLength: 255),
        new(FieldPublished,          "Published",            SuggestionFieldKind.Date),
        new(FieldPublishedPrecision, "Date precision",       SuggestionFieldKind.Enum, EnumOptions: PrecisionOptions),
        new(FieldProductCode,        "Product code",         SuggestionFieldKind.Text, MaxLength: 18),
        new(FieldPages,              "Pages",                SuggestionFieldKind.Short),
        new(FieldIssueNumber,        "Issue number",         SuggestionFieldKind.Int),
        new(FieldInternetArchiveUrl, "Internet Archive URL", SuggestionFieldKind.Url, MaxLength: 2048),
        new(FieldCoverPendingGuid,   "Cover",                SuggestionFieldKind.Image)
    };

    static MagazineIssueSuggestionMetadata() =>
        SuggestionMetadataRegistry.Register(new MagazineIssueSuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor (and registry self-registration) runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.MagazineIssue;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        if(currentDto is not MagazineIssueDto mi) return result;

        result[FieldCaption]            = mi.Caption;
        result[FieldNativeCaption]      = mi.NativeCaption;
        result[FieldPublished]          = mi.Published?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        result[FieldPublishedPrecision] = mi.PublishedPrecision;
        result[FieldProductCode]        = mi.ProductCode;
        result[FieldPages]              = mi.Pages;
        result[FieldIssueNumber]        = mi.IssueNumber;
        result[FieldInternetArchiveUrl] = mi.InternetArchiveUrl;
        // Cover-pending field: write-only on the suggestion side. Seed the current
        // CoverGuid so the diff panel can show "current cover → new pending cover".
        result[FieldCoverPendingGuid] = mi.CoverGuid?.ToString();

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
    ///     Override synthesises a localiser key for dynamic junction keys (e.g. <c>"People"</c>
    ///     for <c>people.{add,remove}.&lt;token&gt;</c>); falls back to the static descriptor
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
        GroupPeople          => "People",
        GroupMachines        => "Machines",
        GroupMachineFamilies => "Machine families",
        GroupSoftware        => "Software",
        _                    => group
    };

    /// <summary>
    ///     Parse a junction operation field-name. Mirror of the server-side helper in
    ///     <c>Marechai.Server.Suggestions.MagazineIssueSuggestionApplier.TryParseJunctionKey</c>.
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
        if(fieldName == FieldPublishedPrecision)
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
