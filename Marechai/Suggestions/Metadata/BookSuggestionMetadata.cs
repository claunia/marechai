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
///     Suggestion metadata for the <see cref="BookDto" /> entity. Exposes 12 directly
///     editable scalar fields PLUS dynamic junction operation keys
///     (<c>&lt;group&gt;.add.&lt;uuid&gt;</c> and <c>&lt;group&gt;.remove.&lt;row_id&gt;</c>)
///     for the 4 suggestable junction tables (People, Companies, Machines, MachineFamilies).
///     Mirrors <see cref="MachineSuggestionMetadata" /> with two key differences: Book.Id is
///     <c>long</c> (the wire id is <c>long?</c>), and People/Companies junctions carry a
///     required <c>RoleId</c> attribute on add.
/// </summary>
public sealed class BookSuggestionMetadata : SuggestionMetadata
{
    // ---- Scalar field names (mirror server-side constants) -----------------------------
    public const string FieldTitle              = "title";
    public const string FieldNativeTitle        = "native_title";
    public const string FieldSortTitle          = "sort_title";
    public const string FieldIsbn               = "isbn";
    public const string FieldPages              = "pages";
    public const string FieldEdition            = "edition";
    public const string FieldPublished          = "published";
    public const string FieldPublishedPrecision = "published_precision";
    public const string FieldCountryId          = "country_id";
    public const string FieldPreviousId         = "previous_id";
    public const string FieldSourceId           = "source_id";
    public const string FieldInternetArchiveUrl    = "internet_archive_url";

    /// <summary>
    ///     Pseudo-field carrying the guid of a pending-cover upload. Mirrors
    ///     <c>BookSuggestionApplier.FieldCoverPendingGuid</c>. The dialog uploads the file
    ///     directly to <c>POST /books/{id}/cover/pending</c> and embeds the returned guid
    ///     in the diff payload. The extension lives in the pending sidecar (no separate
    ///     wire field needed; the applier reads it on accept).
    /// </summary>
    public const string FieldCoverPendingGuid      = "cover_pending_guid";

    // ---- Junction group identifiers (mirror server-side) -------------------------------
    public const string GroupPeople          = "people";
    public const string GroupCompanies       = "companies";
    public const string GroupMachines        = "machines";
    public const string GroupMachineFamilies = "machine_families";

    static readonly HashSet<string> s_junctionGroups = new(StringComparer.Ordinal)
    {
        GroupPeople, GroupCompanies, GroupMachines, GroupMachineFamilies
    };

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldTitle, FieldNativeTitle, FieldSortTitle, FieldIsbn, FieldPages, FieldEdition,
        FieldPublished, FieldPublishedPrecision, FieldCountryId, FieldPreviousId,
        FieldSourceId, FieldInternetArchiveUrl,
        FieldCoverPendingGuid
    };

    static readonly IReadOnlyList<SuggestionEnumOption> PrecisionOptions = new[]
    {
        new SuggestionEnumOption(0, "Full date"),
        new SuggestionEnumOption(1, "Month and year only"),
        new SuggestionEnumOption(2, "Year only")
    };

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldTitle,              "Title",                SuggestionFieldKind.Text, MaxLength: 255),
        new(FieldNativeTitle,        "Native title",         SuggestionFieldKind.Text, MaxLength: 255),
        new(FieldSortTitle,          "Sort title",           SuggestionFieldKind.Text, MaxLength: 255),
        new(FieldIsbn,               "ISBN",                 SuggestionFieldKind.Text, MaxLength: 13),
        new(FieldPages,              "Pages",                SuggestionFieldKind.Short),
        new(FieldEdition,            "Edition",              SuggestionFieldKind.Int),
        new(FieldPublished,          "Published",            SuggestionFieldKind.Date),
        new(FieldPublishedPrecision, "Date precision",       SuggestionFieldKind.Enum, EnumOptions: PrecisionOptions),
        new(FieldCountryId,          "Country",              SuggestionFieldKind.ForeignKeyCountry),
        new(FieldPreviousId,         "Previous book",        SuggestionFieldKind.Int),
        new(FieldSourceId,           "Source book",          SuggestionFieldKind.Int),
        new(FieldInternetArchiveUrl, "Internet Archive URL", SuggestionFieldKind.Url, MaxLength: 2048),
        new(FieldCoverPendingGuid,   "Cover",                SuggestionFieldKind.Image)
    };

    static BookSuggestionMetadata() => SuggestionMetadataRegistry.Register(new BookSuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor (and registry self-registration) runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.Book;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        if(currentDto is not BookDto b) return result;

        result[FieldTitle]              = b.Title;
        result[FieldNativeTitle]        = b.NativeTitle;
        result[FieldSortTitle]          = b.SortTitle;
        result[FieldIsbn]               = b.Isbn;
        result[FieldPages]              = b.Pages;
        result[FieldEdition]            = b.Edition;
        result[FieldPublished]          = b.Published?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        result[FieldPublishedPrecision] = b.PublishedPrecision;
        result[FieldCountryId]          = b.CountryId;
        result[FieldPreviousId]         = b.PreviousId;
        result[FieldSourceId]           = b.SourceId;
        result[FieldInternetArchiveUrl] = b.InternetArchiveUrl;
        // Cover-pending field: write-only on the suggestion side. Seed the current
        // CoverGuid so the diff panel can show "current cover → new pending cover".
        result[FieldCoverPendingGuid] = b.CoverGuid?.ToString();

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
        GroupCompanies       => "Companies",
        GroupMachines        => "Machines",
        GroupMachineFamilies => "Machine families",
        _                    => group
    };

    /// <summary>
    ///     Parse a junction operation field-name. Mirror of the server-side helper in
    ///     <c>Marechai.Server.Suggestions.BookSuggestionApplier.TryParseJunctionKey</c>.
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

        // FK ids: fall back to "#{id}" when the server didn't resolve a display label.
        if(fieldName == FieldCountryId)
        {
            int? i = ToInt(value);
            return i.HasValue ? $"#{i.Value}" : string.Empty;
        }

        if(fieldName == FieldPreviousId || fieldName == FieldSourceId)
        {
            long? i = ToLong(value);
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

    static long? ToLong(object v)
    {
        return v switch
        {
            int i           => i,
            short s         => s,
            long l          => l,
            JsonElement je  => je.ValueKind == JsonValueKind.Number ? je.GetInt64() : null,
            string str      => long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out long p) ? p : null,
            _               => null
        };
    }
}
