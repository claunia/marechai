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
///     Suggestion metadata for the <see cref="PersonDto" /> entity. Exposes 12 directly
///     editable scalar fields plus the <c>cover_pending_guid</c> pseudo-field that wraps a
///     collaborator-uploaded pending photo. Person has no in-scope junctions — all five
///     Person junctions (<c>PeopleByCompany</c>, <c>PeopleByBook</c>, <c>PeopleByDocument</c>,
///     <c>PeopleByMagazine</c>, <c>PeopleBySoftware</c>) are owned by the other side per
///     established convention and stay admin-only. Mirrors
///     <see cref="SoundSynthSuggestionMetadata" /> (scalar-only) plus the
///     <c>cover_pending_guid</c> Image-kind field from
///     <see cref="BookSuggestionMetadata" />.
/// </summary>
public sealed class PersonSuggestionMetadata : SuggestionMetadata
{
    // ---- Scalar field names (mirror server-side constants) -----------------------------
    public const string FieldName               = "name";
    public const string FieldSurname            = "surname";
    public const string FieldAlias              = "alias";
    public const string FieldDisplayName        = "display_name";
    public const string FieldCountryOfBirthId   = "country_of_birth_id";
    public const string FieldBirthDate          = "birth_date";
    public const string FieldBirthDatePrecision = "birth_date_precision";
    public const string FieldDeathDate          = "death_date";
    public const string FieldDeathDatePrecision = "death_date_precision";
    public const string FieldWebpage            = "webpage";
    public const string FieldTwitter            = "twitter";
    public const string FieldFacebook           = "facebook";
    public const string FieldCoverPendingGuid   = "cover_pending_guid";

    static readonly IReadOnlyList<SuggestionEnumOption> PrecisionOptions = new[]
    {
        new SuggestionEnumOption(0, "Full date"),
        new SuggestionEnumOption(1, "Month and year only"),
        new SuggestionEnumOption(2, "Year only")
    };

    static readonly IReadOnlyList<SuggestionFieldDescriptor> s_fields = new SuggestionFieldDescriptor[]
    {
        new(FieldName,               "Name",              SuggestionFieldKind.Text, MaxLength: 100),
        new(FieldSurname,            "Surname",           SuggestionFieldKind.Text, MaxLength: 100),
        new(FieldAlias,              "Alias",             SuggestionFieldKind.Text, MaxLength: 100),
        new(FieldDisplayName,        "Display name",      SuggestionFieldKind.Text, MaxLength: 100),
        new(FieldCountryOfBirthId,   "Country of birth",  SuggestionFieldKind.ForeignKeyCountry),
        new(FieldBirthDate,          "Birth date",        SuggestionFieldKind.Date),
        new(FieldBirthDatePrecision, "Birth date precision", SuggestionFieldKind.Enum, EnumOptions: PrecisionOptions),
        new(FieldDeathDate,          "Death date",        SuggestionFieldKind.Date),
        new(FieldDeathDatePrecision, "Death date precision", SuggestionFieldKind.Enum, EnumOptions: PrecisionOptions),
        new(FieldWebpage,            "Webpage",           SuggestionFieldKind.Text, MaxLength: 255),
        new(FieldTwitter,            "Twitter",           SuggestionFieldKind.Text, MaxLength: 50),
        new(FieldFacebook,           "Facebook",          SuggestionFieldKind.Text, MaxLength: 100),
        new(FieldCoverPendingGuid,   "Photo",             SuggestionFieldKind.Image)
    };

    static PersonSuggestionMetadata() => SuggestionMetadataRegistry.Register(new PersonSuggestionMetadata());

    /// <summary>Touch this to make sure the static constructor (and registry self-registration) runs.</summary>
    public static void EnsureRegistered() { /* triggers the static ctor */ }

    public override SuggestionEntityType EntityType => SuggestionEntityType.Person;

    public override IReadOnlyList<SuggestionFieldDescriptor> Fields => s_fields;

    public override Dictionary<string, object> ExtractCurrentValues(object currentDto)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);

        if(currentDto is not PersonDto p) return result;

        result[FieldName]               = p.Name;
        result[FieldSurname]            = p.Surname;
        result[FieldAlias]              = p.Alias;
        result[FieldDisplayName]        = p.DisplayName;
        // Kiota wire DTO exposes country_id as int? — cast to short? to match our wire
        // field semantics (the server applier accepts numeric coercion either way).
        result[FieldCountryOfBirthId]   = p.CountryId;
        result[FieldBirthDate]          = p.Birthdate?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        result[FieldBirthDatePrecision] = p.BirthdatePrecision;
        result[FieldDeathDate]          = p.DeathDate?.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        result[FieldDeathDatePrecision] = p.DeathDatePrecision;
        result[FieldWebpage]            = p.Webpage;
        result[FieldTwitter]            = p.Twitter;
        result[FieldFacebook]           = p.Facebook;
        // Photo guid as canonical current value so the diff panel can render "current photo
        // → suggested pending photo". Guid.Empty / null → empty string => no current photo.
        result[FieldCoverPendingGuid]   = (p.Photo is null || p.Photo == Guid.Empty)
                                              ? null
                                              : p.Photo.Value.ToString();

        return result;
    }

    public override string FormatDisplayValue(string fieldName, object value)
    {
        if(value is null) return string.Empty;

        // Precision enum → label (shared between birth/death precision fields)
        if(fieldName == FieldBirthDatePrecision || fieldName == FieldDeathDatePrecision)
        {
            int? i = ToInt(value);
            return i switch
            {
                0 => "Full date",
                1 => "Month and year only",
                2 => "Year only",
                _ => string.Empty
            };
        }

        // FK ids: fall back to "#{id}" when the server didn't resolve a display label.
        if(fieldName == FieldCountryOfBirthId)
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
