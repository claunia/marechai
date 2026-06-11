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
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.GpuSuggestionMetadata</c>.
///     Handles BOTH scalar Gpu fields AND junction add/remove operations on the single
///     suggestable junction table (Resolutions). Each junction operation is atomic — its
///     full row payload is accepted or rejected as a single unit. Mirrors the design of
///     <see cref="BookSuggestionApplier" /> minus the cover-pending infrastructure (Gpu has
///     no <c>CoverGuid</c> column) and minus People/Companies/Machines/MachineFamilies
///     junctions. <see cref="Gpu.Id" /> is <c>int</c> so the applier casts the incoming
///     <c>long entityId</c> on every EF query.
/// </summary>
internal static class GpuSuggestionApplier
{
    // ---- Scalar field names (MUST mirror the client-side metadata constants) -----------
    public const string FieldName                 = "name";
    public const string FieldCompanyId            = "company_id";
    public const string FieldModelCode            = "model_code";
    public const string FieldIntroduced           = "introduced";
    public const string FieldIntroducedPrecision  = "introduced_precision";
    public const string FieldPackage              = "package";
    public const string FieldProcess              = "process";
    public const string FieldProcessNm            = "process_nm";
    public const string FieldDieSize              = "die_size";
    public const string FieldTransistors          = "transistors";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldName, FieldCompanyId, FieldModelCode, FieldIntroduced, FieldIntroducedPrecision,
        FieldPackage, FieldProcess, FieldProcessNm, FieldDieSize, FieldTransistors
    };

    // ---- Junction group identifiers (lowercase snake_case prefix in field-name keys) ---
    public const string GroupResolutions = "resolutions";

    public static readonly IReadOnlyCollection<string> JunctionGroups = new[]
    {
        GroupResolutions
    };

    /// <summary>
    ///     Returns <c>true</c> when the field-name is a recognised Gpu field name. Accepts
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
    ///     out parameters with the group identifier (e.g. <c>"resolutions"</c>), the
    ///     operation (<c>"add"</c> or <c>"remove"</c>), and the token (a client-generated
    ///     GUID for adds, the existing row id stringified for removes).
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
    ///     Returns the current scalar values of the targeted Gpu row. Junction operation
    ///     keys are not seeded in the current snapshot — the diff panel renders junction ops
    ///     on their own rows with the readable label resolved server-side.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        Gpu g = await context.Gpus.AsNoTracking().FirstOrDefaultAsync(x => x.Id == (int)entityId);
        if(g is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldName]                = g.Name,
            [FieldCompanyId]           = g.CompanyId,
            [FieldModelCode]           = g.ModelCode,
            [FieldIntroduced]          = g.Introduced?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldIntroducedPrecision] = (byte)g.IntroducedPrecision,
            [FieldPackage]             = g.Package,
            [FieldProcess]             = g.Process,
            [FieldProcessNm]           = g.ProcessNm,
            [FieldDieSize]             = g.DieSize,
            [FieldTransistors]         = g.Transistors
        };
    }

    /// <summary>
    ///     Create a brand-new <see cref="Gpu" /> row from an accepted addition suggestion.
    ///     Mirrors <see cref="MagazineSuggestionApplier.CreateAsync" /> shape but returns
    ///     <c>(int? newId, …)</c> because <see cref="Gpu.Id" /> is <c>int</c>. Validates the
    ///     mandatory <see cref="FieldName" /> first; rejects the addition outright when the
    ///     admin didn't tick it. After persistence, accepted <c>resolutions.add.*</c> keys
    ///     are applied with the freshly-minted <c>GpuId</c>; <c>*.remove.*</c> keys are
    ///     silently skipped (a brand-new entity has nothing to remove from).
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

        // Name is the only mandatory field; everything else is optional. Reject the addition
        // outright if the admin didn't tick Name.
        if(!accepted.Contains(FieldName) || !suggested.TryGetValue(FieldName, out object nameVal))
            return (null, applied);

        string name = ToStringValue(nameVal);
        if(string.IsNullOrWhiteSpace(name)) return (null, applied);
        if(name.Trim().Length > 128) return (null, applied);

        var g = new Gpu { Name = name.Trim() };
        applied.Add(FieldName);

        // Apply remaining accepted scalar fields via the same coercion+validation table the
        // edit path uses. Junction operations are handled in a second pass after persistence.
        foreach(string fieldName in accepted)
        {
            if(fieldName == FieldName) continue;
            if(!s_scalarFieldNames.Contains(fieldName)) continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyScalar(context, g, fieldName, value)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this field.
            }
        }

        await context.Gpus.AddAsync(g);

        await context.SaveChangesWithUserAsync(creditedUserId);

        // Now apply junction adds with the freshly-minted gpu id. Remove keys are silently
        // ignored — a brand-new entity has nothing to remove from.
        foreach(string fieldName in accepted)
        {
            if(!TryParseJunctionKey(fieldName, out string group, out string op, out string _)) continue;
            if(op != "add") continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyJunctionAdd(context, g.Id, group, value, creditedUserId)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this junction add.
            }
        }

        return (g.Id, applied);
    }

    /// <summary>
    ///     Apply the accepted fields onto the Gpu row + junction tables. Each junction
    ///     operation is atomic — failure to coerce one entry skips it without affecting the
    ///     others.
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string creditedUserId)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        Gpu g = await context.Gpus.FirstOrDefaultAsync(x => x.Id == (int)entityId);
        if(g is null) return (applied, true);

        bool scalarChanged = false;

        foreach(string fieldName in accepted)
        {
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(s_scalarFieldNames.Contains(fieldName))
                {
                    if(await ApplyScalar(context, g, fieldName, value))
                    {
                        applied.Add(fieldName);
                        scalarChanged = true;
                    }
                    continue;
                }

                if(TryParseJunctionKey(fieldName, out string group, out string op, out string token))
                {
                    bool ok = op == "add"
                                  ? await ApplyJunctionAdd(context, (int)entityId, group, value, creditedUserId)
                                  : await ApplyJunctionRemove(context, (int)entityId, group, token, creditedUserId);
                    if(ok) applied.Add(fieldName);
                }
            }
            catch
            {
                // Coerce failure: silently skip this field/op.
            }
        }

        if(scalarChanged) await context.SaveChangesWithUserAsync(creditedUserId);

        return (applied, false);
    }

    // ───────────────────────────── Scalar-field application ─────────────────────────────

    static async Task<bool> ApplyScalar(MarechaiContext context, Gpu g, string fieldName, object value)
    {
        switch(fieldName)
        {
            case FieldName:
                string n = ToStringValue(value);
                if(string.IsNullOrWhiteSpace(n)) return false;
                if(n.Length > 128) return false;
                g.Name = n.Trim();
                return true;
            case FieldCompanyId:
            {
                int? cid = ToInt(value);
                if(!cid.HasValue) { g.CompanyId = null; return true; }
                if(!await context.Companies.AsNoTracking().AnyAsync(c => c.Id == cid.Value)) return false;
                g.CompanyId = cid.Value;
                return true;
            }
            case FieldModelCode:
            {
                string m = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(m) && m.Length > 45) return false;
                g.ModelCode = string.IsNullOrEmpty(m) ? null : m;
                return true;
            }
            case FieldIntroduced:
                g.Introduced = ToDate(value);
                return true;
            case FieldIntroducedPrecision:
            {
                int? pv = ToInt(value);
                if(pv.HasValue && pv.Value is >= 0 and <= 2)
                {
                    g.IntroducedPrecision = (DatePrecision)pv.Value;
                    return true;
                }
                return false;
            }
            case FieldPackage:
            {
                string p = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(p) && p.Length > 45) return false;
                g.Package = string.IsNullOrEmpty(p) ? null : p;
                return true;
            }
            case FieldProcess:
            {
                string p = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(p) && p.Length > 45) return false;
                g.Process = string.IsNullOrEmpty(p) ? null : p;
                return true;
            }
            case FieldProcessNm:
            {
                float? f = ToFloat(value);
                if(f.HasValue && f.Value <= 0) return false;
                g.ProcessNm = f;
                return true;
            }
            case FieldDieSize:
            {
                float? f = ToFloat(value);
                if(f.HasValue && f.Value <= 0) return false;
                g.DieSize = f;
                return true;
            }
            case FieldTransistors:
            {
                long? t = ToLong(value);
                if(t.HasValue && t.Value < 1) return false;
                g.Transistors = t;
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction add ─────────────────────────────

    static async Task<bool> ApplyJunctionAdd(MarechaiContext context, int gpuId, string group, object value,
        string creditedUserId)
    {
        Dictionary<string, object> payload = ExtractObject(value);
        if(payload is null) return false;

        switch(group)
        {
            case GroupResolutions:
            {
                int? rid = GetInt(payload, "resolution_id");
                if(!rid.HasValue) return false;
                if(!await context.Resolutions.AsNoTracking().AnyAsync(r => r.Id == rid.Value)) return false;
                // Dedup composite key (GpuId, ResolutionId): same resolution cannot be linked twice.
                if(await context.ResolutionsByGpu.AsNoTracking()
                                .AnyAsync(r => r.GpuId == gpuId && r.ResolutionId == rid.Value))
                    return false;
                await context.ResolutionsByGpu.AddAsync(new ResolutionsByGpu
                {
                    GpuId        = gpuId,
                    ResolutionId = rid.Value
                });
                await context.SaveChangesWithUserAsync(creditedUserId);
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction remove ─────────────────────────────

    static async Task<bool> ApplyJunctionRemove(MarechaiContext context, int gpuId, string group, string token,
        string creditedUserId)
    {
        if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) return false;

        switch(group)
        {
            case GroupResolutions:
                return await context.ResolutionsByGpu
                                    .Where(r => r.Id == rowId && r.GpuId == gpuId)
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

    static long? ToLong(object v)
    {
        return v switch
        {
            null            => null,
            long l          => l,
            int i           => i,
            short s         => s,
            byte b          => b,
            JsonElement je  => je.ValueKind switch
            {
                JsonValueKind.Number => je.TryGetInt64(out long p) ? p : null,
                JsonValueKind.String => long.TryParse(je.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long q) ? q : null,
                _                    => null
            },
            string str      => long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out long p) ? p : null,
            _               => null
        };
    }

    static float? ToFloat(object v)
    {
        return v switch
        {
            null            => null,
            float f         => f,
            double d        => (float)d,
            int i           => i,
            long l          => l,
            JsonElement je  => je.ValueKind switch
            {
                JsonValueKind.Number => je.TryGetSingle(out float f) ? f : null,
                JsonValueKind.String => float.TryParse(je.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float p) ? p : null,
                _                    => null
            },
            string str      => float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float p) ? p : null,
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
