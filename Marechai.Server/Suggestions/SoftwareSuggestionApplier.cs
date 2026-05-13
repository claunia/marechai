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
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.SoftwareSuggestionMetadata</c>.
///     Handles BOTH scalar Software fields AND junction add/remove operations on the single
///     in-scope junction table (Genres). Each junction operation is atomic — its full row
///     payload is accepted or rejected as a single unit. Mirrors the design of
///     <see cref="GpuSuggestionApplier" /> with three differences:
///     <list type="bullet">
///         <item>
///             <see cref="Software.Id" /> is <c>ulong</c> so the applier casts the incoming
///             <c>long entityId</c> to <c>ulong</c> on every EF query.
///         </item>
///         <item>
///             <c>Kind</c> is a NON-NULLABLE enum scalar (first port with this shape) — the
///             applier rejects null values for it.
///         </item>
///         <item>
///             <c>GenreBySoftware</c> has a composite primary key
///             <c>(SoftwareId, GenreId)</c> with no surrogate Id column, so the
///             <c>genres.remove.&lt;token&gt;</c> token is the genre id itself rather than a
///             row id.
///         </item>
///     </list>
/// </summary>
internal static class SoftwareSuggestionApplier
{
    // ---- Scalar field names (MUST mirror the client-side metadata constants) -----------
    public const string FieldName           = "name";
    public const string FieldFamilyId       = "family_id";
    public const string FieldPredecessorId  = "predecessor_id";
    public const string FieldKind           = "kind";
    public const string FieldBaseSoftwareId = "base_software_id";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldName, FieldFamilyId, FieldPredecessorId, FieldKind, FieldBaseSoftwareId
    };

    // ---- Junction group identifiers (lowercase snake_case prefix in field-name keys) ---
    public const string GroupGenres    = "genres";
    public const string GroupCompanies = "companies";

    public static readonly IReadOnlyCollection<string> JunctionGroups = new[]
    {
        GroupGenres,
        GroupCompanies
    };

    /// <summary>
    ///     Returns <c>true</c> when the field-name is a recognised Software field name. Accepts
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
    ///     out parameters with the group identifier (e.g. <c>"genres"</c>), the operation
    ///     (<c>"add"</c> or <c>"remove"</c>), and the token (a client-generated GUID for adds,
    ///     the genre id stringified for removes — composite-key junction has no surrogate Id).
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
    ///     Returns the current scalar values of the targeted Software row. Junction operation
    ///     keys are not seeded in the current snapshot — the diff panel renders junction ops
    ///     on their own rows with the readable label resolved server-side.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        Software s = await context.Softwares.AsNoTracking().FirstOrDefaultAsync(x => x.Id == (ulong)entityId);
        if(s is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldName]           = s.Name,
            [FieldFamilyId]       = s.FamilyId,
            [FieldPredecessorId]  = s.PredecessorId,
            [FieldKind]           = (int)s.Kind,
            [FieldBaseSoftwareId] = s.BaseSoftwareId
        };
    }

    /// <summary>
    ///     Apply the accepted fields onto the Software row + Genres junction. Each junction
    ///     operation is atomic — failure to coerce one entry skips it without affecting the
    ///     others.
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        Software s = await context.Softwares.FirstOrDefaultAsync(x => x.Id == (ulong)entityId);
        if(s is null) return (applied, true);

        bool scalarChanged = false;

        foreach(string fieldName in accepted)
        {
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(s_scalarFieldNames.Contains(fieldName))
                {
                    if(await ApplyScalar(context, s, fieldName, value))
                    {
                        applied.Add(fieldName);
                        scalarChanged = true;
                    }
                    continue;
                }

                if(TryParseJunctionKey(fieldName, out string group, out string op, out string token))
                {
                    bool ok = op == "add"
                                  ? await ApplyJunctionAdd(context, (ulong)entityId, group, value)
                                  : await ApplyJunctionRemove(context, (ulong)entityId, group, token);
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

    static async Task<bool> ApplyScalar(MarechaiContext context, Software s, string fieldName, object value)
    {
        switch(fieldName)
        {
            case FieldName:
                string n = ToStringValue(value);
                if(string.IsNullOrWhiteSpace(n)) return false;
                s.Name = n.Trim();
                return true;
            case FieldFamilyId:
            {
                ulong? fid = ToUlong(value);
                if(!fid.HasValue) { s.FamilyId = null; return true; }
                if(!await context.SoftwareFamilies.AsNoTracking().AnyAsync(f => f.Id == fid.Value)) return false;
                s.FamilyId = fid.Value;
                return true;
            }
            case FieldPredecessorId:
            {
                ulong? pid = ToUlong(value);
                if(!pid.HasValue) { s.PredecessorId = null; return true; }
                // Disallow self-reference.
                if(pid.Value == s.Id) return false;
                if(!await context.Softwares.AsNoTracking().AnyAsync(x => x.Id == pid.Value)) return false;
                s.PredecessorId = pid.Value;
                return true;
            }
            case FieldKind:
            {
                // Kind is NON-NULLABLE on the entity. Reject null/missing and only accept
                // values defined in the SoftwareKind enum.
                int? kv = ToInt(value);
                if(!kv.HasValue) return false;
                if(!Enum.IsDefined(typeof(SoftwareKind), kv.Value)) return false;
                s.Kind = (SoftwareKind)kv.Value;
                return true;
            }
            case FieldBaseSoftwareId:
            {
                ulong? bid = ToUlong(value);
                if(!bid.HasValue) { s.BaseSoftwareId = null; return true; }
                if(bid.Value == s.Id) return false;
                if(!await context.Softwares.AsNoTracking().AnyAsync(x => x.Id == bid.Value)) return false;
                s.BaseSoftwareId = bid.Value;
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction add ─────────────────────────────

    static async Task<bool> ApplyJunctionAdd(MarechaiContext context, ulong softwareId, string group, object value)
    {
        Dictionary<string, object> payload = ExtractObject(value);
        if(payload is null) return false;

        switch(group)
        {
            case GroupGenres:
            {
                int? gid = GetInt(payload, "genre_id");
                if(!gid.HasValue) return false;
                if(!await context.SoftwareGenres.AsNoTracking().AnyAsync(g => g.Id == gid.Value)) return false;
                // Dedup composite key (SoftwareId, GenreId): same genre cannot be linked twice.
                if(await context.GenresBySoftware.AsNoTracking()
                                .AnyAsync(r => r.SoftwareId == softwareId && r.GenreId == gid.Value))
                    return false;
                await context.GenresBySoftware.AddAsync(new GenreBySoftware
                {
                    SoftwareId = softwareId,
                    GenreId    = gid.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupCompanies:
            {
                int?   companyId = GetInt(payload, "company_id");
                string roleId    = GetString(payload, "role_id");
                if(!companyId.HasValue) return false;
                if(string.IsNullOrEmpty(roleId)) return false;
                if(!await context.Companies.AsNoTracking().AnyAsync(c => c.Id == companyId.Value)) return false;
                if(!await context.SoftwareRoles.AsNoTracking().AnyAsync(r => r.Id == roleId)) return false;
                // Dedup full triple (SoftwareId, CompanyId, RoleId). The same company CAN be
                // linked under DIFFERENT roles, so the dedup check is on the triple, not the
                // (SoftwareId, CompanyId) pair.
                if(await context.SoftwareCompanyRoles.AsNoTracking()
                                .AnyAsync(r => r.SoftwareId == softwareId &&
                                               r.CompanyId  == companyId.Value &&
                                               r.RoleId     == roleId))
                    return false;
                await context.SoftwareCompanyRoles.AddAsync(new SoftwareCompanyRole
                {
                    SoftwareId = softwareId,
                    CompanyId  = companyId.Value,
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

    static async Task<bool> ApplyJunctionRemove(MarechaiContext context, ulong softwareId, string group, string token)
    {
        switch(group)
        {
            case GroupGenres:
            {
                // Composite key (SoftwareId, GenreId) — the token is the genre id itself
                // because GenreBySoftware has no surrogate Id column.
                if(!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int genreId)) return false;
                return await context.GenresBySoftware
                                    .Where(r => r.SoftwareId == softwareId && r.GenreId == genreId)
                                    .ExecuteDeleteAsync() > 0;
            }
            case GroupCompanies:
            {
                // 2-component composite token "{companyId}_{roleId}". Field-name parser
                // reserves dots for <group>.<op>.<token> so an in-token delimiter is
                // needed; underscore is safe because every seeded SoftwareRole id is 3
                // lowercase ASCII letters with no underscore.
                string[] parts = token.Split('_', 2);
                if(parts.Length != 2) return false;
                if(string.IsNullOrEmpty(parts[1])) return false;
                if(!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int companyId)) return false;
                string roleId = parts[1];
                return await context.SoftwareCompanyRoles
                                    .Where(r => r.SoftwareId == softwareId &&
                                                r.CompanyId  == companyId  &&
                                                r.RoleId     == roleId)
                                    .ExecuteDeleteAsync() > 0;
            }
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
        payload.TryGetValue(key, out object v) ? ToStringValue(v) : null;

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

    /// <summary>
    ///     Coerce a JSON-wire value into <c>ulong?</c>. Software's primary key and three of
    ///     its scalar FKs (FamilyId, PredecessorId, BaseSoftwareId) are <c>ulong?</c> on the
    ///     database side, but Kiota widens them to <c>int?</c> on the OpenAPI wire — so the
    ///     suggested payload may carry either an int, a long, or a JSON number. Safely
    ///     covers all numeric cases plus string fallback.
    /// </summary>
    static ulong? ToUlong(object v)
    {
        return v switch
        {
            null            => null,
            ulong u         => u,
            uint u          => u,
            int i           => i >= 0 ? (ulong)i : null,
            short s         => s >= 0 ? (ulong)s : null,
            long l          => l >= 0 ? (ulong)l : null,
            byte b          => b,
            JsonElement je  => je.ValueKind switch
            {
                JsonValueKind.Null   => null,
                JsonValueKind.Number => je.TryGetUInt64(out ulong p) ? p :
                                       je.TryGetInt64(out long pl) && pl >= 0 ? (ulong)pl : null,
                JsonValueKind.String => ulong.TryParse(je.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong pu) ? pu : null,
                _                    => null
            },
            string str      => ulong.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong p) ? p : null,
            _               => null
        };
    }
}
