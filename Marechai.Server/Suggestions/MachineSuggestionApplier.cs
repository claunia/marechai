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
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.MachineSuggestionMetadata</c>.
///     Handles BOTH scalar Machine fields AND junction add/remove operations across the 7
///     suggestable junction tables (GPUs, Processors, SoundSynths, Screens, Memory, Storage,
///     SoftwarePlatforms). Each junction operation is atomic — its full row payload is accepted
///     or rejected as a single unit. Edits to existing junction rows are modelled as a
///     <c>remove</c> + <c>add</c> pair which the admin can accept independently.
/// </summary>
internal static class MachineSuggestionApplier
{
    // ---- Scalar field names (MUST mirror the client-side metadata constants) -----------
    public const string FieldName                = "name";
    public const string FieldModel               = "model";
    public const string FieldCompanyId           = "company_id";
    public const string FieldType                = "type";
    public const string FieldPrototype           = "prototype";
    public const string FieldIntroduced          = "introduced";
    public const string FieldIntroducedPrecision = "introduced_precision";
    public const string FieldFamilyId            = "family_id";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldName, FieldModel, FieldCompanyId, FieldType, FieldPrototype,
        FieldIntroduced, FieldIntroducedPrecision, FieldFamilyId
    };

    // ---- Junction group identifiers (lowercase snake_case prefix in field-name keys) ---
    public const string GroupGpus              = "gpus";
    public const string GroupProcessors        = "processors";
    public const string GroupSoundSynths       = "sound_synths";
    public const string GroupScreens           = "screens";
    public const string GroupMemory            = "memory";
    public const string GroupStorage           = "storage";
    public const string GroupSoftwarePlatforms = "software_platforms";

    public static readonly IReadOnlyCollection<string> JunctionGroups = new[]
    {
        GroupGpus, GroupProcessors, GroupSoundSynths, GroupScreens,
        GroupMemory, GroupStorage, GroupSoftwarePlatforms
    };

    /// <summary>
    ///     Returns <c>true</c> when the field-name is a recognised Machine field name. Accepts
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
    ///     out parameters with the group identifier (e.g. <c>"gpus"</c>), the operation
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
    ///     Returns the current scalar values of the targeted Machine row. Junction operation
    ///     keys are not seeded in the current snapshot — the diff panel renders junction ops
    ///     on their own rows with the readable label resolved server-side.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        Machine m = await context.Machines.AsNoTracking().FirstOrDefaultAsync(x => x.Id == (int)entityId);
        if(m is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldName]                = m.Name,
            [FieldModel]               = m.Model,
            [FieldCompanyId]           = m.CompanyId,
            [FieldType]                = (byte)m.Type,
            [FieldPrototype]           = m.Prototype,
            [FieldIntroduced]          = m.Introduced?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldIntroducedPrecision] = (byte)m.IntroducedPrecision,
            [FieldFamilyId]            = m.FamilyId
        };
    }

    /// <summary>
    ///     Create a brand-new Machine row from an accepted addition-mode suggestion. <c>name</c>,
    ///     <c>type</c> and <c>company_id</c> are mandatory at creation time (mirroring the
    ///     <c>[Required]</c> attributes on the entity); if the admin didn't tick any of them — or
    ///     if any of them fail coercion / FK validation — the method returns <c>(null, empty)</c>
    ///     so the controller treats the whole review as a rejection. After the Machine row is
    ///     persisted, any accepted junction-add operations are applied with the freshly-minted
    ///     id; <c>*.remove.*</c> keys are silently ignored because there is nothing to remove
    ///     from on a brand-new entity.
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

        // Validate the three mandatory fields up-front so we can build a valid Machine row
        // before invoking ApplyScalar for the optional fields.
        if(!accepted.Contains(FieldName) || !suggested.TryGetValue(FieldName, out object nameVal))
            return (null, applied);

        string name = ToStringValue(nameVal);
        if(string.IsNullOrWhiteSpace(name)) return (null, applied);

        if(!accepted.Contains(FieldType) || !suggested.TryGetValue(FieldType, out object typeVal))
            return (null, applied);

        int? typeInt = ToInt(typeVal);
        if(!typeInt.HasValue || !Enum.IsDefined(typeof(MachineType), typeInt.Value)) return (null, applied);

        if(!accepted.Contains(FieldCompanyId) || !suggested.TryGetValue(FieldCompanyId, out object companyVal))
            return (null, applied);

        int? companyInt = ToInt(companyVal);
        if(!companyInt.HasValue) return (null, applied);
        if(!await context.Companies.AsNoTracking().AnyAsync(c => c.Id == companyInt.Value))
            return (null, applied);

        var m = new Machine
        {
            Name      = name.Trim(),
            Type      = (MachineType)typeInt.Value,
            CompanyId = companyInt.Value
        };

        applied.Add(FieldName);
        applied.Add(FieldType);
        applied.Add(FieldCompanyId);

        // Apply remaining accepted scalar fields via the same coercion+validation table the
        // edit path uses. Junction operations are handled in a second pass after persistence.
        foreach(string fieldName in accepted)
        {
            if(fieldName == FieldName || fieldName == FieldType || fieldName == FieldCompanyId) continue;
            if(!s_scalarFieldNames.Contains(fieldName)) continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyScalar(context, m, fieldName, value)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this field.
            }
        }

        await context.Machines.AddAsync(m);

        if(string.IsNullOrEmpty(creditedUserId))
            await context.SaveChangesAsync();
        else
            await context.SaveChangesWithUserAsync(creditedUserId);

        // Now apply junction adds with the freshly-minted machine id. Remove keys are silently
        // ignored — a brand-new entity has nothing to remove from.
        foreach(string fieldName in accepted)
        {
            if(!TryParseJunctionKey(fieldName, out string group, out string op, out string _)) continue;
            if(op != "add") continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyJunctionAdd(context, m.Id, group, value)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this junction add.
            }
        }

        return (m.Id, applied);
    }

    /// <summary>
    ///     Apply the accepted fields onto the Machine row + junction tables. Each junction
    ///     operation is atomic — failure to coerce one entry skips it without affecting the
    ///     others.
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        Machine m = await context.Machines.FirstOrDefaultAsync(x => x.Id == (int)entityId);
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
                                  ? await ApplyJunctionAdd(context, (int)entityId, group, value)
                                  : await ApplyJunctionRemove(context, (int)entityId, group, token);
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

    static async Task<bool> ApplyScalar(MarechaiContext context, Machine m, string fieldName, object value)
    {
        switch(fieldName)
        {
            case FieldName:
                string n = ToStringValue(value);
                if(string.IsNullOrWhiteSpace(n)) return false;
                m.Name = n.Trim();
                return true;
            case FieldModel:
                m.Model = TruncString(ToStringValue(value), 50);
                return true;
            case FieldCompanyId:
                int? cid = ToInt(value);
                if(!cid.HasValue) return false;
                if(!await context.Companies.AsNoTracking().AnyAsync(c => c.Id == cid.Value)) return false;
                m.CompanyId = cid.Value;
                return true;
            case FieldType:
                int? tv = ToInt(value);
                if(tv.HasValue && Enum.IsDefined(typeof(MachineType), tv.Value))
                {
                    m.Type = (MachineType)tv.Value;
                    return true;
                }
                return false;
            case FieldPrototype:
                bool? bv = ToBool(value);
                if(!bv.HasValue) return false;
                m.Prototype = bv.Value;
                return true;
            case FieldIntroduced:
                m.Introduced = ToDate(value);
                return true;
            case FieldIntroducedPrecision:
                int? pv = ToInt(value);
                if(pv.HasValue && pv.Value is >= 0 and <= 2)
                {
                    m.IntroducedPrecision = (DatePrecision)pv.Value;
                    return true;
                }
                return false;
            case FieldFamilyId:
                int? fid = ToInt(value);
                if(!fid.HasValue)
                {
                    m.FamilyId = null;
                    return true;
                }
                if(!await context.MachineFamilies.AsNoTracking().AnyAsync(f => f.Id == fid.Value)) return false;
                m.FamilyId = fid.Value;
                return true;
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction add ─────────────────────────────

    static async Task<bool> ApplyJunctionAdd(MarechaiContext context, int machineId, string group, object value)
    {
        Dictionary<string, object> payload = ExtractObject(value);
        if(payload is null) return false;

        switch(group)
        {
            case GroupGpus:
            {
                int? gid = GetInt(payload, "gpu_id");
                if(!gid.HasValue) return false;
                if(!await context.Gpus.AsNoTracking().AnyAsync(g => g.Id == gid.Value)) return false;
                if(await context.GpusByMachine.AsNoTracking()
                                .AnyAsync(g => g.MachineId == machineId && g.GpuId == gid.Value))
                    return false; // dedupe — row already exists.
                await context.GpusByMachine.AddAsync(new GpusByMachine
                {
                    MachineId = machineId,
                    GpuId     = gid.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupProcessors:
            {
                int?   pid   = GetInt(payload, "processor_id");
                if(!pid.HasValue) return false;
                if(!await context.Processors.AsNoTracking().AnyAsync(p => p.Id == pid.Value)) return false;
                float? speed = GetFloat(payload, "speed");
                await context.ProcessorsByMachine.AddAsync(new ProcessorsByMachine
                {
                    MachineId   = machineId,
                    ProcessorId = pid.Value,
                    Speed       = speed
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupSoundSynths:
            {
                int? sid = GetInt(payload, "sound_synth_id");
                if(!sid.HasValue) return false;
                if(!await context.SoundSynths.AsNoTracking().AnyAsync(s => s.Id == sid.Value)) return false;
                if(await context.SoundByMachine.AsNoTracking()
                                .AnyAsync(s => s.MachineId == machineId && s.SoundSynthId == sid.Value))
                    return false;
                await context.SoundByMachine.AddAsync(new SoundByMachine
                {
                    MachineId    = machineId,
                    SoundSynthId = sid.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupScreens:
            {
                int? scid = GetInt(payload, "screen_id");
                if(!scid.HasValue) return false;
                if(!await context.Screens.AsNoTracking().AnyAsync(s => s.Id == scid.Value)) return false;
                if(await context.ScreensByMachine.AsNoTracking()
                                .AnyAsync(s => s.MachineId == machineId && s.ScreenId == scid.Value))
                    return false;
                await context.ScreensByMachine.AddAsync(new ScreensByMachine
                {
                    MachineId = machineId,
                    ScreenId  = scid.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupMemory:
            {
                int? typeVal  = GetInt(payload, "type");
                int? usageVal = GetInt(payload, "usage");
                if(!typeVal.HasValue || !usageVal.HasValue) return false;
                if(!Enum.IsDefined(typeof(MemoryType), typeVal.Value)) return false;
                if(!Enum.IsDefined(typeof(MemoryUsage), usageVal.Value)) return false;
                long?   size  = GetLong(payload, "size");
                double? speed = GetDouble(payload, "speed");
                await context.MemoryByMachine.AddAsync(new MemoryByMachine
                {
                    MachineId = machineId,
                    Type      = (MemoryType)typeVal.Value,
                    Usage     = (MemoryUsage)usageVal.Value,
                    Size      = size,
                    Speed     = speed
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupStorage:
            {
                int? typeVal = GetInt(payload, "type");
                int? ifVal   = GetInt(payload, "interface");
                if(!typeVal.HasValue || !ifVal.HasValue) return false;
                if(!Enum.IsDefined(typeof(StorageType), typeVal.Value)) return false;
                if(!Enum.IsDefined(typeof(StorageInterface), ifVal.Value)) return false;
                long? capacity = GetLong(payload, "capacity");
                await context.StorageByMachine.AddAsync(new StorageByMachine
                {
                    MachineId = machineId,
                    Type      = (StorageType)typeVal.Value,
                    Interface = (StorageInterface)ifVal.Value,
                    Capacity  = capacity
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupSoftwarePlatforms:
            {
                long? spIdRaw = GetLong(payload, "software_platform_id");
                if(!spIdRaw.HasValue || spIdRaw.Value < 0) return false;
                ulong spId = (ulong)spIdRaw.Value;
                if(!await context.SoftwarePlatforms.AsNoTracking().AnyAsync(p => p.Id == spId)) return false;
                if(await context.SoftwarePlatformsByMachine.AsNoTracking()
                                .AnyAsync(p => p.MachineId == machineId && p.SoftwarePlatformId == spId))
                    return false;
                await context.SoftwarePlatformsByMachine.AddAsync(new SoftwarePlatformsByMachine
                {
                    MachineId          = machineId,
                    SoftwarePlatformId = spId
                });
                await context.SaveChangesAsync();
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction remove ─────────────────────────────

    static async Task<bool> ApplyJunctionRemove(MarechaiContext context, int machineId, string group, string token)
    {
        if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) return false;

        switch(group)
        {
            case GroupGpus:
                return await context.GpusByMachine
                                    .Where(r => r.Id == rowId && r.MachineId == machineId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupProcessors:
                return await context.ProcessorsByMachine
                                    .Where(r => r.Id == rowId && r.MachineId == machineId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupSoundSynths:
                return await context.SoundByMachine
                                    .Where(r => r.Id == rowId && r.MachineId == machineId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupScreens:
                return await context.ScreensByMachine
                                    .Where(r => r.Id == rowId && r.MachineId == machineId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupMemory:
                return await context.MemoryByMachine
                                    .Where(r => r.Id == rowId && r.MachineId == machineId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupStorage:
                return await context.StorageByMachine
                                    .Where(r => r.Id == rowId && r.MachineId == machineId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupSoftwarePlatforms:
                return await context.SoftwarePlatformsByMachine
                                    .Where(r => r.Id == rowId && r.MachineId == machineId)
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

    static long? GetLong(Dictionary<string, object> payload, string key) =>
        payload.TryGetValue(key, out object v) ? ToLong(v) : null;

    static float? GetFloat(Dictionary<string, object> payload, string key) =>
        payload.TryGetValue(key, out object v) ? ToFloat(v) : null;

    static double? GetDouble(Dictionary<string, object> payload, string key) =>
        payload.TryGetValue(key, out object v) ? ToDouble(v) : null;

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
            ulong ul        => (long)ul,
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
                JsonValueKind.Number => je.TryGetDouble(out double p) ? (float)p : null,
                JsonValueKind.String => float.TryParse(je.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float q) ? q : null,
                _                    => null
            },
            string str      => float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float p) ? p : null,
            _               => null
        };
    }

    static double? ToDouble(object v)
    {
        return v switch
        {
            null            => null,
            double d        => d,
            float f         => f,
            int i           => i,
            long l          => l,
            JsonElement je  => je.ValueKind switch
            {
                JsonValueKind.Number => je.TryGetDouble(out double p) ? p : null,
                JsonValueKind.String => double.TryParse(je.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double q) ? q : null,
                _                    => null
            },
            string str      => double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double p) ? p : null,
            _               => null
        };
    }

    static bool? ToBool(object v)
    {
        return v switch
        {
            null            => null,
            bool b          => b,
            JsonElement je  => je.ValueKind switch
            {
                JsonValueKind.True   => true,
                JsonValueKind.False  => false,
                JsonValueKind.String => bool.TryParse(je.GetString(), out bool p) ? p : null,
                JsonValueKind.Number => je.TryGetInt32(out int n) ? n != 0 : null,
                _                    => null
            },
            string str      => bool.TryParse(str, out bool p) ? p : null,
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
