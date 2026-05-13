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
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.DocumentSuggestionMetadata</c>.
///     Handles BOTH scalar Document fields AND junction add/remove operations across the 4
///     suggestable junction tables (People, Companies, Machines, MachineFamilies). Each
///     junction operation is atomic — its full row payload is accepted or rejected as a
///     single unit. Edits to existing junction rows are modelled as a <c>remove</c> +
///     <c>add</c> pair which the admin can accept independently. Mirrors the design of
///     <see cref="BookSuggestionApplier" /> minus cover/ISBN/Pages/Edition/Previous/Source
///     fields (Documents have no cover image and a leaner scalar surface).
/// </summary>
internal static class DocumentSuggestionApplier
{
    // ---- Scalar field names (MUST mirror the client-side metadata constants) -----------
    public const string FieldTitle              = "title";
    public const string FieldNativeTitle        = "native_title";
    public const string FieldSortTitle          = "sort_title";
    public const string FieldPublished          = "published";
    public const string FieldPublishedPrecision = "published_precision";
    public const string FieldCountryId          = "country_id";
    public const string FieldInternetArchiveUrl = "internet_archive_url";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldTitle, FieldNativeTitle, FieldSortTitle,
        FieldPublished, FieldPublishedPrecision, FieldCountryId,
        FieldInternetArchiveUrl
    };

    // ---- Junction group identifiers (lowercase snake_case prefix in field-name keys) ---
    public const string GroupPeople          = "people";
    public const string GroupCompanies       = "companies";
    public const string GroupMachines        = "machines";
    public const string GroupMachineFamilies = "machine_families";

    public static readonly IReadOnlyCollection<string> JunctionGroups = new[]
    {
        GroupPeople, GroupCompanies, GroupMachines, GroupMachineFamilies
    };

    /// <summary>
    ///     Returns <c>true</c> when the field-name is a recognised Document field name. Accepts
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
    ///     out parameters with the group identifier (e.g. <c>"people"</c>), the operation
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
    ///     Returns the current scalar values of the targeted Document row. Junction operation
    ///     keys are not seeded in the current snapshot — the diff panel renders junction ops
    ///     on their own rows with the readable label resolved server-side.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        Document d = await context.Documents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entityId);
        if(d is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldTitle]              = d.Title,
            [FieldNativeTitle]        = d.NativeTitle,
            [FieldSortTitle]          = d.SortTitle,
            [FieldPublished]          = d.Published?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldPublishedPrecision] = (byte)d.PublishedPrecision,
            [FieldCountryId]          = d.CountryId,
            [FieldInternetArchiveUrl] = d.InternetArchiveUrl
        };
    }

    /// <summary>
    ///     Create a brand-new Document row from an accepted addition-mode suggestion.
    ///     Returns the new entity id (or <c>null</c> on failure to validate the mandatory
    ///     <see cref="FieldTitle" />), plus the actually-applied field set. After scalar
    ///     fields are persisted, accepted <c>*.add.*</c> junction keys are applied with
    ///     the freshly-minted <c>DocumentId</c>; <c>*.remove.*</c> keys are silently
    ///     skipped (a brand-new entity has nothing to remove from). Mirrors
    ///     <see cref="BookSuggestionApplier.CreateAsync" /> minus the cover-promotion
    ///     block (Documents have no cover image).
    /// </summary>
    /// <param name="creditedUserId">
    ///     The Identity user id to attribute the row to in audit history (the suggesting
    ///     user, NOT the reviewing admin). Forwarded to <c>SaveChangesWithUserAsync</c>.
    /// </param>
    public static async Task<(long? newId, HashSet<string> applied)> CreateAsync(
        MarechaiContext context,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string creditedUserId)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        // Title is the only mandatory field; everything else (dates, country, IA url,
        // junctions) is optional. Reject the addition outright if the admin didn't tick
        // Title.
        if(!accepted.Contains(FieldTitle) || !suggested.TryGetValue(FieldTitle, out object titleVal))
            return (null, applied);

        string title = ToStringValue(titleVal);
        if(string.IsNullOrWhiteSpace(title)) return (null, applied);

        var d = new Document { Title = title.Trim() };
        applied.Add(FieldTitle);

        // Apply remaining accepted scalar fields via the same coercion+validation table the
        // edit path uses. Junction operations are handled in a dedicated pass below.
        foreach(string fieldName in accepted)
        {
            if(fieldName == FieldTitle) continue;
            if(!s_scalarFieldNames.Contains(fieldName)) continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyScalar(context, d, fieldName, value)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this field.
            }
        }

        await context.Documents.AddAsync(d);

        if(string.IsNullOrEmpty(creditedUserId))
            await context.SaveChangesAsync();
        else
            await context.SaveChangesWithUserAsync(creditedUserId);

        // Now apply junction adds with the freshly-minted document id. Remove keys are
        // silently ignored — a brand-new entity has nothing to remove from.
        foreach(string fieldName in accepted)
        {
            if(!TryParseJunctionKey(fieldName, out string group, out string op, out string _)) continue;
            if(op != "add") continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyJunctionAdd(context, d.Id, group, value)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this junction add.
            }
        }

        return (d.Id, applied);
    }

    /// <summary>
    ///     Apply the accepted fields onto the Document row + junction tables. Each junction
    ///     operation is atomic — failure to coerce one entry skips it without affecting the
    ///     others.
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        Document d = await context.Documents.FirstOrDefaultAsync(x => x.Id == entityId);
        if(d is null) return (applied, true);

        bool scalarChanged = false;

        foreach(string fieldName in accepted)
        {
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(s_scalarFieldNames.Contains(fieldName))
                {
                    if(await ApplyScalar(context, d, fieldName, value))
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

    static async Task<bool> ApplyScalar(MarechaiContext context, Document d, string fieldName, object value)
    {
        switch(fieldName)
        {
            case FieldTitle:
                string t = ToStringValue(value);
                if(string.IsNullOrWhiteSpace(t)) return false;
                d.Title = t.Trim();
                return true;
            case FieldNativeTitle:
                d.NativeTitle = ToStringValue(value)?.Trim();
                return true;
            case FieldSortTitle:
                d.SortTitle = ToStringValue(value)?.Trim();
                return true;
            case FieldPublished:
                d.Published = ToDate(value);
                return true;
            case FieldPublishedPrecision:
            {
                int? pv = ToInt(value);
                if(pv.HasValue && pv.Value is >= 0 and <= 2)
                {
                    d.PublishedPrecision = (DatePrecision)pv.Value;
                    return true;
                }
                return false;
            }
            case FieldCountryId:
            {
                int? raw = ToInt(value);
                if(!raw.HasValue) { d.CountryId = null; return true; }
                if(raw.Value is < short.MinValue or > short.MaxValue) return false;
                short cid = (short)raw.Value;
                if(!await context.Iso31661Numeric.AsNoTracking().AnyAsync(c => c.Id == cid)) return false;
                d.CountryId = cid;
                return true;
            }
            case FieldInternetArchiveUrl:
            {
                string url = ToStringValue(value)?.Trim();
                if(string.IsNullOrEmpty(url)) { d.InternetArchiveUrl = null; return true; }
                if(url.Length > 2048) return false;
                d.InternetArchiveUrl = url;
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction add ─────────────────────────────

    static async Task<bool> ApplyJunctionAdd(MarechaiContext context, long documentId, string group, object value)
    {
        Dictionary<string, object> payload = ExtractObject(value);
        if(payload is null) return false;

        switch(group)
        {
            case GroupPeople:
            {
                int?   pid    = GetInt(payload, "person_id");
                string roleId = GetString(payload, "role_id");
                if(!pid.HasValue || string.IsNullOrEmpty(roleId)) return false;
                if(!await context.People.AsNoTracking().AnyAsync(p => p.Id == pid.Value)) return false;
                if(!await context.DocumentRoles.AsNoTracking().AnyAsync(r => r.Id == roleId)) return false;
                // Dedup composite key (DocumentId, PersonId, RoleId): same person CAN be added
                // under different roles (author + translator) but never twice with the same role.
                if(await context.PeopleByDocuments.AsNoTracking()
                                .AnyAsync(r => r.DocumentId == documentId && r.PersonId == pid.Value && r.RoleId == roleId))
                    return false;
                await context.PeopleByDocuments.AddAsync(new PeopleByDocument
                {
                    DocumentId = documentId,
                    PersonId   = pid.Value,
                    RoleId     = roleId
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupCompanies:
            {
                int?   cid    = GetInt(payload, "company_id");
                string roleId = GetString(payload, "role_id");
                if(!cid.HasValue || string.IsNullOrEmpty(roleId)) return false;
                if(!await context.Companies.AsNoTracking().AnyAsync(c => c.Id == cid.Value)) return false;
                if(!await context.DocumentRoles.AsNoTracking().AnyAsync(r => r.Id == roleId)) return false;
                if(await context.CompaniesByDocuments.AsNoTracking()
                                .AnyAsync(r => r.DocumentId == documentId && r.CompanyId == cid.Value && r.RoleId == roleId))
                    return false;
                await context.CompaniesByDocuments.AddAsync(new CompaniesByDocument
                {
                    DocumentId = documentId,
                    CompanyId  = cid.Value,
                    RoleId     = roleId
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupMachines:
            {
                int? mid = GetInt(payload, "machine_id");
                if(!mid.HasValue) return false;
                if(!await context.Machines.AsNoTracking().AnyAsync(m => m.Id == mid.Value)) return false;
                if(await context.DocumentsByMachines.AsNoTracking()
                                .AnyAsync(r => r.DocumentId == documentId && r.MachineId == mid.Value))
                    return false;
                await context.DocumentsByMachines.AddAsync(new DocumentsByMachine
                {
                    DocumentId = documentId,
                    MachineId  = mid.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupMachineFamilies:
            {
                int? fid = GetInt(payload, "machine_family_id");
                if(!fid.HasValue) return false;
                if(!await context.MachineFamilies.AsNoTracking().AnyAsync(f => f.Id == fid.Value)) return false;
                if(await context.DocumentsByMachineFamilies.AsNoTracking()
                                .AnyAsync(r => r.DocumentId == documentId && r.MachineFamilyId == fid.Value))
                    return false;
                await context.DocumentsByMachineFamilies.AddAsync(new DocumentsByMachineFamily
                {
                    DocumentId      = documentId,
                    MachineFamilyId = fid.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction remove ─────────────────────────────

    static async Task<bool> ApplyJunctionRemove(MarechaiContext context, long documentId, string group, string token)
    {
        if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) return false;

        switch(group)
        {
            case GroupPeople:
                return await context.PeopleByDocuments
                                    .Where(r => r.Id == rowId && r.DocumentId == documentId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupCompanies:
                return await context.CompaniesByDocuments
                                    .Where(r => r.Id == rowId && r.DocumentId == documentId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupMachines:
                return await context.DocumentsByMachines
                                    .Where(r => r.Id == rowId && r.DocumentId == documentId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupMachineFamilies:
                return await context.DocumentsByMachineFamilies
                                    .Where(r => r.Id == rowId && r.DocumentId == documentId)
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
