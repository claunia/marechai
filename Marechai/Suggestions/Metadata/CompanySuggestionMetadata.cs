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
///     Suggestion metadata for the <see cref="CompanyDto" /> entity. Exposes the 14 directly
///     editable fields (Name, Status, Founded, etc.) as suggestable. FK-picker fields (Country,
///     SoldTo) are intentionally excluded from Phase 1 because the generic SuggestionDialog
///     does not yet have FK picker rendering — they can be added in a later phase.
/// </summary>
public sealed class CompanySuggestionMetadata : SuggestionMetadata
{
    public const string FieldName             = "name";
    public const string FieldLegalName        = "legal_name";
    public const string FieldStatus           = "status";
    public const string FieldFounded          = "founded";
    public const string FieldFoundedPrecision = "founded_precision";
    public const string FieldSold             = "sold";
    public const string FieldSoldPrecision    = "sold_precision";
    public const string FieldAddress          = "address";
    public const string FieldCity             = "city";
    public const string FieldProvince         = "province";
    public const string FieldPostalCode       = "postal_code";
    public const string FieldWebsite          = "website";
    public const string FieldTwitter          = "twitter";
    public const string FieldFacebook         = "facebook";

    static readonly IReadOnlyList<SuggestionEnumOption> StatusOptions = new[]
    {
        new SuggestionEnumOption(0, "Unknown"),
        new SuggestionEnumOption(1, "Active"),
        new SuggestionEnumOption(2, "Sold"),
        new SuggestionEnumOption(3, "Merged"),
        new SuggestionEnumOption(4, "Bankrupt"),
        new SuggestionEnumOption(5, "Defunct"),
        new SuggestionEnumOption(6, "Renamed")
    };

    static readonly IReadOnlyList<SuggestionEnumOption> PrecisionOptions = new[]
    {
        new SuggestionEnumOption(0, "Full date"),
        new SuggestionEnumOption(1, "Month and year only"),
        new SuggestionEnumOption(2, "Year only")
    };

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldName,             "Name",              SuggestionFieldKind.Text),
        new(FieldLegalName,        "Legal name",        SuggestionFieldKind.Text),
        new(FieldStatus,           "Status",            SuggestionFieldKind.Enum, EnumOptions: StatusOptions),
        new(FieldFounded,          "Founded",           SuggestionFieldKind.Date),
        new(FieldFoundedPrecision, "Founded precision", SuggestionFieldKind.Enum, EnumOptions: PrecisionOptions),
        new(FieldSold,             "Sold/closed date",  SuggestionFieldKind.Date),
        new(FieldSoldPrecision,    "Sold precision",    SuggestionFieldKind.Enum, EnumOptions: PrecisionOptions),
        new(FieldAddress,          "Address",           SuggestionFieldKind.Text, MaxLength: 80),
        new(FieldCity,             "City",              SuggestionFieldKind.Text, MaxLength: 80),
        new(FieldProvince,         "Province",          SuggestionFieldKind.Text, MaxLength: 80),
        new(FieldPostalCode,       "Postal code",       SuggestionFieldKind.Text, MaxLength: 25),
        new(FieldWebsite,          "Website",           SuggestionFieldKind.Url,  MaxLength: 255),
        new(FieldTwitter,          "Twitter",           SuggestionFieldKind.Text, MaxLength: 45),
        new(FieldFacebook,         "Facebook",          SuggestionFieldKind.Text, MaxLength: 45)
    };

    static CompanySuggestionMetadata() => SuggestionMetadataRegistry.Register(new CompanySuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor (and registry self-registration) runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.Company;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        if(currentDto is not CompanyDto c) return result;

        result[FieldName]              = c.Name;
        result[FieldLegalName]         = c.LegalName;
        result[FieldStatus]            = c.Status; // int? on Kiota wire
        result[FieldFounded]           = c.Founded?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        result[FieldFoundedPrecision]  = c.FoundedPrecision; // int? on Kiota wire
        result[FieldSold]              = c.Sold?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        result[FieldSoldPrecision]     = c.SoldPrecision; // int? on Kiota wire
        result[FieldAddress]           = c.Address;
        result[FieldCity]              = c.City;
        result[FieldProvince]          = c.Province;
        result[FieldPostalCode]        = c.PostalCode;
        result[FieldWebsite]           = c.Website;
        result[FieldTwitter]           = c.Twitter;
        result[FieldFacebook]          = c.Facebook;

        return result;
    }

    public override string FormatDisplayValue(string fieldName, object value)
    {
        if(value is null) return string.Empty;

        // Status enum → label
        if(fieldName == FieldStatus)
        {
            int? i = ToInt(value);
            return i switch
            {
                0 => "Unknown",
                1 => "Active",
                2 => "Sold",
                3 => "Merged",
                4 => "Bankrupt",
                5 => "Defunct",
                6 => "Renamed",
                _ => string.Empty
            };
        }

        // Precision enum → label
        if(fieldName == FieldFoundedPrecision || fieldName == FieldSoldPrecision)
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
