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
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.MagazineSuggestionMetadata</c>.
///     Handles BOTH scalar Magazine fields AND junction add/remove operations on the single
///     suggestable junction table (<see cref="CompaniesByMagazine" />). People are NOT exposed
///     here — they belong to individual <c>MagazineIssue</c> rows, not to magazines, so the
///     <c>PeopleByMagazine</c> table is out of scope. Mirrors the design of
///     <see cref="DocumentSuggestionApplier" /> minus People/Machines/MachineFamilies, plus the
///     Magazine-specific <c>Issn</c>/<c>FirstPublication</c>/<c>FirstPublicationPrecision</c>
///     scalars (and minus the Document-only <c>InternetArchiveUrl</c>, which lives on
///     <c>MagazineIssue</c> for magazines).
/// </summary>
internal static class MagazineSuggestionApplier
{
    // ---- Scalar field names (MUST mirror the client-side metadata constants) -----------
    public const string FieldTitle                     = "title";
    public const string FieldNativeTitle               = "native_title";
    public const string FieldSortTitle                 = "sort_title";
    public const string FieldPublished                 = "published";
    public const string FieldPublishedPrecision        = "published_precision";
    public const string FieldCountryId                 = "country_id";
    public const string FieldIssn                      = "issn";
    public const string FieldFirstPublication          = "first_publication";
    public const string FieldFirstPublicationPrecision = "first_publication_precision";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldTitle, FieldNativeTitle, FieldSortTitle,
        FieldPublished, FieldPublishedPrecision, FieldCountryId,
        FieldIssn, FieldFirstPublication, FieldFirstPublicationPrecision
    };

    // ---- Junction group identifiers (lowercase snake_case prefix in field-name keys) ---
    public const string GroupCompanies = "companies";

    public static readonly IReadOnlyCollection<string> JunctionGroups = new[] { GroupCompanies };

    /// <summary>
    ///     Returns <c>true</c> when the field-name is a recognised Magazine field name. Accepts
    ///     scalar field names AND any junction operation key matching
    ///     <c>&lt;group&gt;.{add,remove}.&lt;token&gt;</c>.
    /// </summary>
    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;
        if(s_scalarFieldNames.Contains(fieldName)) return true;
        return TryParseJunctionKey(fieldName, out _, out _, out _);
    }

    /// <summary>
    ///     Parse a junction operation field-name. Returns true on success and populates the
    ///     out parameters with the group identifier (e.g. <c>"companies"</c>), the operation
    ///     (<c>"add"</c> or <c>"remove"</c>), and the token (a client-generated GUID for adds,
    ///     the existing row id stringified for removes).
    /// </summary>
    public static bool TryParseJunctionKey(string fieldName, out string group, out string op, out string token)
    {
        group = null;
        op    = null;
        token = null;

        if(string.IsNullOrEmpty(fieldName)) return false;

        string[] parts = fieldName.Split('.', 3);
        if(parts.Length != 3) return false;
        if(!JunctionGroups.Contains(parts[0])) return false;
        if(parts[1] != "add" && parts[1] != "remove") return false;
        if(string.IsNullOrEmpty(parts[2])) return false;

        group = parts[0];
        op    = parts[1];
        token = parts[2];
        return true;
    }

    /// <summary>
    ///     Returns the current scalar values of the targeted Magazine row. Junction operation
    ///     keys are not seeded in the current snapshot — the diff panel renders junction ops
    ///     on their own rows with the readable label resolved server-side.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        Magazine m = await context.Magazines.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entityId);
        if(m is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldTitle]                     = m.Title,
            [FieldNativeTitle]               = m.NativeTitle,
            [FieldSortTitle]                 = m.SortTitle,
            [FieldPublished]                 = m.Published?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldPublishedPrecision]        = (byte)m.PublishedPrecision,
            [FieldCountryId]                 = m.CountryId,
            [FieldIssn]                      = m.Issn,
            [FieldFirstPublication]          = m.FirstPublication?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldFirstPublicationPrecision] = (byte)m.FirstPublicationPrecision
        };
    }

    /// <summary>
    ///     Apply the accepted fields onto the Magazine row + junction tables. Each junction
    ///     operation is atomic — failure to coerce one entry skips it without affecting the
    ///     others.
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        Magazine m = await context.Magazines.FirstOrDefaultAsync(x => x.Id == entityId);
        if(m is null) return (applied, true);

        bool scalarChanged = false;

        foreach(string fieldName in accepted)
        {
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(s_scalarFieldNames.Contains(fieldName))
                {
                    if(await ApplyScalar(context, m, fieldName, value))
                    {
                        applied.Add(fieldName);
                        scalarChanged = true;
                    }
                    continue;
                }

                if(TryParseJunctionKey(fieldName, out string group, out string op, out string token))
                {
                    bool ok = op == "add"
                                  ? await ApplyJunctionAdd(context, entityId, group, value)
                                  : await ApplyJunctionRemove(context, entityId, group, token);
                    if(ok) applied.Add(fieldName);
                }
            }
            catch
            {
                // Coerce failure: silently skip this field/op.
            }
        }

        if(scalarChanged) await context.SaveChangesAsync();

        return (applied, false);
    }

    // ───────────────────────────── Scalar-field application ─────────────────────────────

    static async Task<bool> ApplyScalar(MarechaiContext context, Magazine m, string fieldName, object value)
    {
        switch(fieldName)
        {
            case FieldTitle:
                string t = ToStringValue(value);
                if(string.IsNullOrWhiteSpace(t)) return false;
                m.Title = t.Trim();
                return true;
            case FieldNativeTitle:
                m.NativeTitle = ToStringValue(value)?.Trim();
                return true;
            case FieldSortTitle:
                m.SortTitle = ToStringValue(value)?.Trim();
                return true;
            case FieldPublished:
                m.Published = ToDate(value);
                return true;
            case FieldPublishedPrecision:
            {
                int? pv = ToInt(value);
                if(pv.HasValue && pv.Value is >= 0 and <= 2)
                {
                    m.PublishedPrecision = (DatePrecision)pv.Value;
                    return true;
                }
                return false;
            }
            case FieldCountryId:
            {
                int? raw = ToInt(value);
                if(!raw.HasValue) { m.CountryId = null; return true; }
                if(raw.Value is < short.MinValue or > short.MaxValue) return false;
                short cid = (short)raw.Value;
                if(!await context.Iso31661Numeric.AsNoTracking().AnyAsync(c => c.Id == cid)) return false;
                m.CountryId = cid;
                return true;
            }
            case FieldIssn:
            {
                string s = ToStringValue(value)?.Trim();
                if(string.IsNullOrEmpty(s)) { m.Issn = null; return true; }
                // ISSN is exactly 8 chars on the model ([StringLength(8, MinimumLength = 8)]).
                if(s.Length != 8) return false;
                m.Issn = s;
                return true;
            }
            case FieldFirstPublication:
                m.FirstPublication = ToDate(value);
                return true;
            case FieldFirstPublicationPrecision:
            {
                int? pv = ToInt(value);
                if(pv.HasValue && pv.Value is >= 0 and <= 2)
                {
                    m.FirstPublicationPrecision = (DatePrecision)pv.Value;
                    return true;
                }
                return false;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction add ─────────────────────────────

    static async Task<bool> ApplyJunctionAdd(MarechaiContext context, long magazineId, string group, object value)
    {
        Dictionary<string, object> payload = ExtractObject(value);
        if(payload is null) return false;

        switch(group)
        {
            case GroupCompanies:
            {
                int?   cid    = GetInt(payload, "company_id");
                string roleId = GetString(payload, "role_id");
                if(!cid.HasValue || string.IsNullOrEmpty(roleId)) return false;
                if(!await context.Companies.AsNoTracking().AnyAsync(c => c.Id == cid.Value)) return false;
                if(!await context.DocumentRoles.AsNoTracking().AnyAsync(r => r.Id == roleId)) return false;
                // Dedup composite key (MagazineId, CompanyId, RoleId): same company CAN be added
                // under different roles (publisher + distributor) but never twice with the same role.
                if(await context.CompaniesByMagazines.AsNoTracking()
                                .AnyAsync(r => r.MagazineId == magazineId && r.CompanyId == cid.Value && r.RoleId == roleId))
                    return false;
                await context.CompaniesByMagazines.AddAsync(new CompaniesByMagazine
                {
                    MagazineId = magazineId,
                    CompanyId  = cid.Value,
                    RoleId     = roleId
                });
                await context.SaveChangesAsync();
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction remove ─────────────────────────────

    static async Task<bool> ApplyJunctionRemove(MarechaiContext context, long magazineId, string group, string token)
    {
        if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) return false;

        switch(group)
        {
            case GroupCompanies:
                return await context.CompaniesByMagazines
                                    .Where(r => r.Id == rowId && r.MagazineId == magazineId)
                                    .ExecuteDeleteAsync() > 0;
            default:
                return false;
        }
    }

    // ───────────────────────────── Coercion helpers ─────────────────────────────

    static Dictionary<string, object> ExtractObject(object v)
    {
        if(v is null) return null;
        if(v is Dictionary<string, object> dict) return dict;
        if(v is JsonElement je && je.ValueKind == JsonValueKind.Object)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach(JsonProperty prop in je.EnumerateObject()) result[prop.Name] = prop.Value;
            return result;
        }
        return null;
    }

    static int? GetInt(Dictionary<string, object> payload, string key) =>
        payload.TryGetValue(key, out object v) ? ToInt(v) : null;

    static string GetString(Dictionary<string, object> payload, string key) =>
        payload.TryGetValue(key, out object v) ? ToStringValue(v)?.Trim() : null;

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
            JsonElement je  => je.ValueKind switch
            {
                JsonValueKind.Number => je.TryGetInt32(out int i) ? i : null,
                JsonValueKind.String => int.TryParse(je.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int p) ? p : null,
                _                    => null
            },
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
}
