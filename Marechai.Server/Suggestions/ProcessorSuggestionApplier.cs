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
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.ProcessorSuggestionMetadata</c>.
///     Handles BOTH scalar Processor fields AND junction add/remove operations on the single
///     suggestable junction table (Instruction Set Extensions). Each junction operation is
///     atomic — its full row payload is accepted or rejected as a single unit. Mirrors
///     <see cref="GpuSuggestionApplier" /> in shape (int-keyed, no cover, single junction);
///     the broader scalar set reflects the Processor entity's richer columns.
///     <see cref="Processor.Id" /> is <c>int</c> so the applier casts the incoming
///     <c>long entityId</c> on every EF query.
/// </summary>
internal static class ProcessorSuggestionApplier
{
    // ---- Scalar field names (MUST mirror the client-side metadata constants) -----------
    public const string FieldName                 = "name";
    public const string FieldCompanyId            = "company_id";
    public const string FieldModelCode            = "model_code";
    public const string FieldIntroduced           = "introduced";
    public const string FieldIntroducedPrecision  = "introduced_precision";
    public const string FieldInstructionSetId     = "instruction_set_id";
    public const string FieldSpeed                = "speed";
    public const string FieldPackage              = "package";
    public const string FieldGprs                 = "gprs";
    public const string FieldGprSize              = "gpr_size";
    public const string FieldFprs                 = "fprs";
    public const string FieldFprSize              = "fpr_size";
    public const string FieldCores                = "cores";
    public const string FieldThreadsPerCore       = "threads_per_core";
    public const string FieldProcess              = "process";
    public const string FieldProcessNm            = "process_nm";
    public const string FieldDieSize              = "die_size";
    public const string FieldTransistors          = "transistors";
    public const string FieldDataBus              = "data_bus";
    public const string FieldAddressBus           = "address_bus";
    public const string FieldSimdRegisters        = "simd_registers";
    public const string FieldSimdSize             = "simd_size";
    public const string FieldL1Instruction        = "l1_instruction";
    public const string FieldL1Data               = "l1_data";
    public const string FieldL2                   = "l2";
    public const string FieldL3                   = "l3";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldName, FieldCompanyId, FieldModelCode, FieldIntroduced, FieldIntroducedPrecision,
        FieldInstructionSetId, FieldSpeed, FieldPackage, FieldGprs, FieldGprSize, FieldFprs,
        FieldFprSize, FieldCores, FieldThreadsPerCore, FieldProcess, FieldProcessNm,
        FieldDieSize, FieldTransistors, FieldDataBus, FieldAddressBus, FieldSimdRegisters,
        FieldSimdSize, FieldL1Instruction, FieldL1Data, FieldL2, FieldL3
    };

    // ---- Junction group identifiers (lowercase snake_case prefix in field-name keys) ---
    public const string GroupInstructionSetExtensions = "instruction_set_extensions";

    public static readonly IReadOnlyCollection<string> JunctionGroups = new[]
    {
        GroupInstructionSetExtensions
    };

    /// <summary>
    ///     Returns <c>true</c> when the field-name is a recognised Processor field name.
    ///     Accepts scalar field names AND any junction operation key matching
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
    ///     out parameters with the group identifier (e.g. <c>"instruction_set_extensions"</c>),
    ///     the operation (<c>"add"</c> or <c>"remove"</c>), and the token (a client-generated
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
    ///     Returns the current scalar values of the targeted Processor row. Junction operation
    ///     keys are not seeded in the current snapshot — the diff panel renders junction ops
    ///     on their own rows with the readable label resolved server-side.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        Processor p = await context.Processors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == (int)entityId);
        if(p is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldName]                = p.Name,
            [FieldCompanyId]           = p.CompanyId,
            [FieldModelCode]           = p.ModelCode,
            [FieldIntroduced]          = p.Introduced?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldIntroducedPrecision] = (byte)p.IntroducedPrecision,
            [FieldInstructionSetId]    = p.InstructionSetId,
            [FieldSpeed]               = p.Speed,
            [FieldPackage]             = p.Package,
            [FieldGprs]                = p.Gprs,
            [FieldGprSize]             = p.GprSize,
            [FieldFprs]                = p.Fprs,
            [FieldFprSize]             = p.FprSize,
            [FieldCores]               = p.Cores,
            [FieldThreadsPerCore]      = p.ThreadsPerCore,
            [FieldProcess]             = p.Process,
            [FieldProcessNm]           = p.ProcessNm,
            [FieldDieSize]             = p.DieSize,
            [FieldTransistors]         = p.Transistors,
            [FieldDataBus]             = p.DataBus,
            [FieldAddressBus]          = p.AddrBus,
            [FieldSimdRegisters]       = p.SimdRegisters,
            [FieldSimdSize]            = p.SimdSize,
            [FieldL1Instruction]       = p.L1Instruction,
            [FieldL1Data]              = p.L1Data,
            [FieldL2]                  = p.L2,
            [FieldL3]                  = p.L3
        };
    }

    /// <summary>
    ///     Apply the accepted fields onto the Processor row + junction tables. Each junction
    ///     operation is atomic — failure to coerce one entry skips it without affecting the
    ///     others.
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        Processor p = await context.Processors.FirstOrDefaultAsync(x => x.Id == (int)entityId);
        if(p is null) return (applied, true);

        bool scalarChanged = false;

        foreach(string fieldName in accepted)
        {
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(s_scalarFieldNames.Contains(fieldName))
                {
                    if(await ApplyScalar(context, p, fieldName, value))
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

    static async Task<bool> ApplyScalar(MarechaiContext context, Processor p, string fieldName, object value)
    {
        switch(fieldName)
        {
            case FieldName:
                string n = ToStringValue(value);
                if(string.IsNullOrWhiteSpace(n)) return false;
                if(n.Length > 50) return false;
                p.Name = n.Trim();
                return true;
            case FieldCompanyId:
            {
                int? cid = ToInt(value);
                if(!cid.HasValue) { p.CompanyId = null; return true; }
                if(!await context.Companies.AsNoTracking().AnyAsync(c => c.Id == cid.Value)) return false;
                p.CompanyId = cid.Value;
                return true;
            }
            case FieldModelCode:
            {
                string m = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(m) && m.Length > 45) return false;
                p.ModelCode = string.IsNullOrEmpty(m) ? null : m;
                return true;
            }
            case FieldIntroduced:
                p.Introduced = ToDate(value);
                return true;
            case FieldIntroducedPrecision:
            {
                int? pv = ToInt(value);
                if(pv.HasValue && pv.Value is >= 0 and <= 2)
                {
                    p.IntroducedPrecision = (DatePrecision)pv.Value;
                    return true;
                }
                return false;
            }
            case FieldInstructionSetId:
            {
                int? id = ToInt(value);
                if(!id.HasValue) { p.InstructionSetId = null; return true; }
                if(!await context.InstructionSets.AsNoTracking().AnyAsync(i => i.Id == id.Value)) return false;
                p.InstructionSetId = id.Value;
                return true;
            }
            case FieldSpeed:
            {
                double? d = ToDouble(value);
                if(d.HasValue && d.Value <= 0) return false;
                p.Speed = d;
                return true;
            }
            case FieldPackage:
            {
                string s = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(s) && s.Length > 45) return false;
                p.Package = string.IsNullOrEmpty(s) ? null : s;
                return true;
            }
            case FieldGprs:
            {
                int? i = ToInt(value);
                if(i.HasValue && i.Value < 0) return false;
                p.Gprs = i;
                return true;
            }
            case FieldGprSize:
            {
                int? i = ToInt(value);
                if(i.HasValue && i.Value < 0) return false;
                p.GprSize = i;
                return true;
            }
            case FieldFprs:
            {
                int? i = ToInt(value);
                if(i.HasValue && i.Value < 0) return false;
                p.Fprs = i;
                return true;
            }
            case FieldFprSize:
            {
                int? i = ToInt(value);
                if(i.HasValue && i.Value < 0) return false;
                p.FprSize = i;
                return true;
            }
            case FieldCores:
            {
                int? i = ToInt(value);
                if(i.HasValue && i.Value < 1) return false;
                p.Cores = i;
                return true;
            }
            case FieldThreadsPerCore:
            {
                int? i = ToInt(value);
                if(i.HasValue && i.Value < 1) return false;
                p.ThreadsPerCore = i;
                return true;
            }
            case FieldProcess:
            {
                string s = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(s) && s.Length > 45) return false;
                p.Process = string.IsNullOrEmpty(s) ? null : s;
                return true;
            }
            case FieldProcessNm:
            {
                float? f = ToFloat(value);
                if(f.HasValue && f.Value <= 0) return false;
                p.ProcessNm = f;
                return true;
            }
            case FieldDieSize:
            {
                float? f = ToFloat(value);
                if(f.HasValue && f.Value <= 0) return false;
                p.DieSize = f;
                return true;
            }
            case FieldTransistors:
            {
                long? t = ToLong(value);
                if(t.HasValue && t.Value < 1) return false;
                p.Transistors = t;
                return true;
            }
            case FieldDataBus:
            {
                int? i = ToInt(value);
                if(i.HasValue && i.Value < 1) return false;
                p.DataBus = i;
                return true;
            }
            case FieldAddressBus:
            {
                int? i = ToInt(value);
                if(i.HasValue && i.Value < 1) return false;
                p.AddrBus = i;
                return true;
            }
            case FieldSimdRegisters:
            {
                int? i = ToInt(value);
                if(i.HasValue && i.Value < 0) return false;
                p.SimdRegisters = i;
                return true;
            }
            case FieldSimdSize:
            {
                int? i = ToInt(value);
                if(i.HasValue && i.Value < 0) return false;
                p.SimdSize = i;
                return true;
            }
            case FieldL1Instruction:
            {
                float? f = ToFloat(value);
                if(f.HasValue && f.Value < 0) return false;
                p.L1Instruction = f;
                return true;
            }
            case FieldL1Data:
            {
                float? f = ToFloat(value);
                if(f.HasValue && f.Value < 0) return false;
                p.L1Data = f;
                return true;
            }
            case FieldL2:
            {
                float? f = ToFloat(value);
                if(f.HasValue && f.Value < 0) return false;
                p.L2 = f;
                return true;
            }
            case FieldL3:
            {
                float? f = ToFloat(value);
                if(f.HasValue && f.Value < 0) return false;
                p.L3 = f;
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction add ─────────────────────────────

    static async Task<bool> ApplyJunctionAdd(MarechaiContext context, int processorId, string group, object value)
    {
        Dictionary<string, object> payload = ExtractObject(value);
        if(payload is null) return false;

        switch(group)
        {
            case GroupInstructionSetExtensions:
            {
                int? eid = GetInt(payload, "extension_id");
                if(!eid.HasValue) return false;
                if(!await context.InstructionSetExtensions.AsNoTracking().AnyAsync(e => e.Id == eid.Value))
                    return false;
                // Dedup composite key (ProcessorId, ExtensionId): same extension cannot be
                // linked twice.
                if(await context.InstructionSetExtensionsByProcessor.AsNoTracking()
                                .AnyAsync(r => r.ProcessorId == processorId && r.ExtensionId == eid.Value))
                    return false;
                await context.InstructionSetExtensionsByProcessor.AddAsync(new InstructionSetExtensionsByProcessor
                {
                    ProcessorId = processorId,
                    ExtensionId = eid.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction remove ─────────────────────────────

    static async Task<bool> ApplyJunctionRemove(MarechaiContext context, int processorId, string group, string token)
    {
        if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) return false;

        switch(group)
        {
            case GroupInstructionSetExtensions:
                return await context.InstructionSetExtensionsByProcessor
                                    .Where(r => r.Id == rowId && r.ProcessorId == processorId)
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
                JsonValueKind.Number => je.TryGetDouble(out double d) ? d : null,
                JsonValueKind.String => double.TryParse(je.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double p) ? p : null,
                _                    => null
            },
            string str      => double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double p) ? p : null,
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
