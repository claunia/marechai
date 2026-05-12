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
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Suggestions;

/// <summary>
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.CompanySuggestionMetadata</c>.
///     The two MUST agree on the canonical field-name set (the strings below MUST mirror the
///     client constants); a client suggestion with an unknown name is rejected by the controller.
/// </summary>
internal static class CompanySuggestionApplier
{
    // Mirror of the client-side constants. KEEP IN SYNC.
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
    public const string FieldCountryId        = "country_id";
    public const string FieldSoldToId         = "sold_to_id";

    public static readonly IReadOnlyCollection<string> KnownFieldNames = new HashSet<string>(StringComparer.Ordinal)
    {
        FieldName, FieldLegalName, FieldStatus, FieldFounded, FieldFoundedPrecision,
        FieldSold, FieldSoldPrecision, FieldAddress, FieldCity, FieldProvince,
        FieldPostalCode, FieldWebsite, FieldTwitter, FieldFacebook,
        FieldCountryId, FieldSoldToId
    };

    /// <summary>Read the current values of every suggestable field from the entity row.</summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        var c = await context.Companies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == (int)entityId);
        if(c is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldName]              = c.Name,
            [FieldLegalName]         = c.LegalName,
            [FieldStatus]            = (byte?)c.Status,
            [FieldFounded]           = c.Founded?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldFoundedPrecision]  = (byte?)c.FoundedPrecision,
            [FieldSold]              = c.Sold?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldSoldPrecision]     = (byte?)c.SoldPrecision,
            [FieldAddress]           = c.Address,
            [FieldCity]              = c.City,
            [FieldProvince]          = c.Province,
            [FieldPostalCode]        = c.PostalCode,
            [FieldWebsite]           = c.Website,
            [FieldTwitter]           = c.Twitter,
            [FieldFacebook]          = c.Facebook,
            [FieldCountryId]         = (int?)c.CountryId,
            [FieldSoldToId]          = c.SoldToId
        };
    }

    /// <summary>
    ///     Apply the accepted fields from <paramref name="suggested" /> onto the Company row,
    ///     persist via the supplied <paramref name="context" />, and return the actually-applied
    ///     field-name set (which can be smaller than <paramref name="accepted" /> if some values
    ///     failed to coerce).
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        Company c = await context.Companies.FirstOrDefaultAsync(x => x.Id == (int)entityId);
        if(c is null) return (applied, true);

        foreach(string fieldName in accepted)
        {
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                switch(fieldName)
                {
                    case FieldName:
                        string n = ToStringValue(value);
                        if(string.IsNullOrWhiteSpace(n)) continue;
                        c.Name = n.Trim();
                        applied.Add(fieldName);
                        break;
                    case FieldLegalName:
                        c.LegalName = ToStringValue(value);
                        applied.Add(fieldName);
                        break;
                    case FieldStatus:
                        int? statusVal = ToInt(value);
                        if(statusVal.HasValue && statusVal.Value is >= 0 and <= 6)
                        {
                            c.Status = (CompanyStatus)statusVal.Value;
                            applied.Add(fieldName);
                        }
                        break;
                    case FieldFounded:
                        c.Founded = ToDate(value);
                        applied.Add(fieldName);
                        break;
                    case FieldFoundedPrecision:
                        int? fpVal = ToInt(value);
                        if(fpVal.HasValue && fpVal.Value is >= 0 and <= 2)
                        {
                            c.FoundedPrecision = (DatePrecision)fpVal.Value;
                            applied.Add(fieldName);
                        }
                        break;
                    case FieldSold:
                        c.Sold = ToDate(value);
                        applied.Add(fieldName);
                        break;
                    case FieldSoldPrecision:
                        int? spVal = ToInt(value);
                        if(spVal.HasValue && spVal.Value is >= 0 and <= 2)
                        {
                            c.SoldPrecision = (DatePrecision)spVal.Value;
                            applied.Add(fieldName);
                        }
                        break;
                    case FieldAddress:
                        c.Address = TruncString(ToStringValue(value), 80);
                        applied.Add(fieldName);
                        break;
                    case FieldCity:
                        c.City = TruncString(ToStringValue(value), 80);
                        applied.Add(fieldName);
                        break;
                    case FieldProvince:
                        c.Province = TruncString(ToStringValue(value), 80);
                        applied.Add(fieldName);
                        break;
                    case FieldPostalCode:
                        c.PostalCode = TruncString(ToStringValue(value), 25);
                        applied.Add(fieldName);
                        break;
                    case FieldWebsite:
                        c.Website = TruncString(ToStringValue(value), 255);
                        applied.Add(fieldName);
                        break;
                    case FieldTwitter:
                        c.Twitter = TruncString(ToStringValue(value), 45);
                        applied.Add(fieldName);
                        break;
                    case FieldFacebook:
                        c.Facebook = TruncString(ToStringValue(value), 45);
                        applied.Add(fieldName);
                        break;
                    case FieldCountryId:
                        int? countryVal = ToInt(value);
                        if(!countryVal.HasValue)
                        {
                            // Explicit null clears the FK.
                            c.CountryId = null;
                            applied.Add(fieldName);
                        }
                        else if(await context.Iso31661Numeric.AsNoTracking()
                                             .AnyAsync(co => co.Id == (short)countryVal.Value))
                        {
                            c.CountryId = (short)countryVal.Value;
                            applied.Add(fieldName);
                        }
                        // else: silently skip — FK does not exist.
                        break;
                    case FieldSoldToId:
                        int? soldToVal = ToInt(value);
                        if(!soldToVal.HasValue)
                        {
                            c.SoldToId = null;
                            applied.Add(fieldName);
                        }
                        else if(soldToVal.Value != c.Id &&
                                await context.Companies.AsNoTracking()
                                             .AnyAsync(co => co.Id == soldToVal.Value))
                        {
                            c.SoldToId = soldToVal.Value;
                            applied.Add(fieldName);
                        }
                        // else: silently skip — FK does not exist or self-reference.
                        break;
                }
            }
            catch
            {
                // Coerce failure: silently skip this field.
            }
        }

        if(applied.Count > 0) await context.SaveChangesAsync();

        return (applied, false);
    }

    /// <summary>
    ///     Create a brand-new <see cref="Company" /> row from an accepted suggestion. Only
    ///     fields whose names appear in <paramref name="accepted" /> are populated. <c>name</c>
    ///     is mandatory: if the admin didn't accept it, returns <c>(null, empty)</c> so the
    ///     controller treats the whole review as a Rejection.
    /// </summary>
    /// <param name="creditedUserId">
    ///     The Identity user id to attribute the row to in audit history (the suggesting user,
    ///     NOT the reviewing admin). Forwarded to <c>SaveChangesWithUserAsync</c>.
    /// </param>
    public static async Task<(int? newId, HashSet<string> applied)> CreateAsync(
        MarechaiContext context,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string creditedUserId)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        // Name is mandatory at creation time. If the admin didn't tick it, abort.
        if(!accepted.Contains(FieldName)) return (null, applied);
        if(!suggested.TryGetValue(FieldName, out object nameVal)) return (null, applied);

        string name = ToStringValue(nameVal);
        if(string.IsNullOrWhiteSpace(name)) return (null, applied);

        var c = new Company { Name = name.Trim() };
        applied.Add(FieldName);

        foreach(string fieldName in accepted)
        {
            if(fieldName == FieldName) continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                switch(fieldName)
                {
                    case FieldLegalName:
                        c.LegalName = ToStringValue(value);
                        applied.Add(fieldName);
                        break;
                    case FieldStatus:
                        int? statusVal = ToInt(value);
                        if(statusVal.HasValue && statusVal.Value is >= 0 and <= 6)
                        {
                            c.Status = (CompanyStatus)statusVal.Value;
                            applied.Add(fieldName);
                        }
                        break;
                    case FieldFounded:
                        c.Founded = ToDate(value);
                        applied.Add(fieldName);
                        break;
                    case FieldFoundedPrecision:
                        int? fpVal = ToInt(value);
                        if(fpVal.HasValue && fpVal.Value is >= 0 and <= 2)
                        {
                            c.FoundedPrecision = (DatePrecision)fpVal.Value;
                            applied.Add(fieldName);
                        }
                        break;
                    case FieldSold:
                        c.Sold = ToDate(value);
                        applied.Add(fieldName);
                        break;
                    case FieldSoldPrecision:
                        int? spVal = ToInt(value);
                        if(spVal.HasValue && spVal.Value is >= 0 and <= 2)
                        {
                            c.SoldPrecision = (DatePrecision)spVal.Value;
                            applied.Add(fieldName);
                        }
                        break;
                    case FieldAddress:
                        c.Address = TruncString(ToStringValue(value), 80);
                        applied.Add(fieldName);
                        break;
                    case FieldCity:
                        c.City = TruncString(ToStringValue(value), 80);
                        applied.Add(fieldName);
                        break;
                    case FieldProvince:
                        c.Province = TruncString(ToStringValue(value), 80);
                        applied.Add(fieldName);
                        break;
                    case FieldPostalCode:
                        c.PostalCode = TruncString(ToStringValue(value), 25);
                        applied.Add(fieldName);
                        break;
                    case FieldWebsite:
                        c.Website = TruncString(ToStringValue(value), 255);
                        applied.Add(fieldName);
                        break;
                    case FieldTwitter:
                        c.Twitter = TruncString(ToStringValue(value), 45);
                        applied.Add(fieldName);
                        break;
                    case FieldFacebook:
                        c.Facebook = TruncString(ToStringValue(value), 45);
                        applied.Add(fieldName);
                        break;
                    case FieldCountryId:
                        int? countryVal = ToInt(value);
                        if(countryVal.HasValue &&
                           await context.Iso31661Numeric.AsNoTracking()
                                        .AnyAsync(co => co.Id == (short)countryVal.Value))
                        {
                            c.CountryId = (short)countryVal.Value;
                            applied.Add(fieldName);
                        }
                        break;
                    case FieldSoldToId:
                        int? soldToVal = ToInt(value);
                        if(soldToVal.HasValue &&
                           await context.Companies.AsNoTracking()
                                        .AnyAsync(co => co.Id == soldToVal.Value))
                        {
                            c.SoldToId = soldToVal.Value;
                            applied.Add(fieldName);
                        }
                        break;
                }
            }
            catch
            {
                // Coerce failure: silently skip this field.
            }
        }

        await context.Companies.AddAsync(c);
        if(string.IsNullOrEmpty(creditedUserId))
            await context.SaveChangesAsync();
        else
            await context.SaveChangesWithUserAsync(creditedUserId);

        return (c.Id, applied);
    }

    static string ToStringValue(object v)
    {
        return v switch
        {
            null            => null,
            JsonElement je  => je.ValueKind switch
            {
                JsonValueKind.Null   => null,
                JsonValueKind.String => je.GetString(),
                _                    => je.ToString()
            },
            string s        => s,
            _               => v.ToString()
        };
    }

    static int? ToInt(object v)
    {
        return v switch
        {
            null            => null,
            int i           => i,
            short s         => s,
            long l          => (int?)l,
            byte b          => b,
            JsonElement je  => je.ValueKind == JsonValueKind.Number ? je.GetInt32() : null,
            string str      => int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int p) ? p : null,
            _               => null
        };
    }

    static DateTime? ToDate(object v)
    {
        return v switch
        {
            null            => null,
            DateTime dt     => DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc),
            JsonElement je when je.ValueKind == JsonValueKind.Null => null,
            JsonElement je when je.ValueKind == JsonValueKind.String =>
                DateTime.TryParse(je.GetString(), CultureInfo.InvariantCulture,
                                  DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                  out DateTime p) ? DateTime.SpecifyKind(p.Date, DateTimeKind.Utc) : null,
            string str      =>
                DateTime.TryParse(str, CultureInfo.InvariantCulture,
                                  DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                  out DateTime p) ? DateTime.SpecifyKind(p.Date, DateTimeKind.Utc) : null,
            _               => null
        };
    }

    static string TruncString(string s, int max)
    {
        if(string.IsNullOrEmpty(s)) return s;
        return s.Length <= max ? s : s.Substring(0, max);
    }
}
