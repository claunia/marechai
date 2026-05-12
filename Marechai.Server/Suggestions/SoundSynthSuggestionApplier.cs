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
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Suggestions;

/// <summary>
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.SoundSynthSuggestionMetadata</c>.
///     Handles scalar SoundSynth fields ONLY — the two visible junctions (<c>SoundByMachine</c>,
///     <c>SoundSynthBySoftwareRelease</c>) are owned by the other side per the established
///     convention and therefore stay admin-only. Mirrors <see cref="GpuSuggestionApplier" />
///     minus the resolutions junction. <see cref="SoundSynth.Id" /> is <c>int</c> so the
///     applier casts the incoming <c>long entityId</c> on every EF query.
/// </summary>
internal static class SoundSynthSuggestionApplier
{
    // ---- Scalar field names (MUST mirror the client-side metadata constants) -----------
    public const string FieldName                = "name";
    public const string FieldCompanyId           = "company_id";
    public const string FieldModelCode           = "model_code";
    public const string FieldIntroduced          = "introduced";
    public const string FieldIntroducedPrecision = "introduced_precision";
    public const string FieldVoices              = "voices";
    public const string FieldFrequency           = "frequency";
    public const string FieldDepth               = "depth";
    public const string FieldSquareWave          = "square_wave";
    public const string FieldWhiteNoise          = "white_noise";
    public const string FieldType                = "type";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldName, FieldCompanyId, FieldModelCode, FieldIntroduced, FieldIntroducedPrecision,
        FieldVoices, FieldFrequency, FieldDepth, FieldSquareWave, FieldWhiteNoise, FieldType
    };

    /// <summary>
    ///     Returns <c>true</c> when the field-name is a recognised SoundSynth scalar field
    ///     name. SoundSynth has no in-scope junctions so there is no junction-key parser.
    /// </summary>
    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;
        return s_scalarFieldNames.Contains(fieldName);
    }

    /// <summary>
    ///     Returns the current scalar values of the targeted SoundSynth row.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        SoundSynth s = await context.SoundSynths.AsNoTracking().FirstOrDefaultAsync(x => x.Id == (int)entityId);
        if(s is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldName]                = s.Name,
            [FieldCompanyId]           = s.CompanyId,
            [FieldModelCode]           = s.ModelCode,
            [FieldIntroduced]          = s.Introduced?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldIntroducedPrecision] = (byte)s.IntroducedPrecision,
            [FieldVoices]              = s.Voices,
            [FieldFrequency]           = s.Frequency,
            [FieldDepth]               = s.Depth,
            [FieldSquareWave]          = s.SquareWave,
            [FieldWhiteNoise]          = s.WhiteNoise,
            [FieldType]                = s.Type
        };
    }

    /// <summary>
    ///     Apply the accepted fields onto the SoundSynth row. Scalar-only — no junction
    ///     branches.
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        SoundSynth s = await context.SoundSynths.FirstOrDefaultAsync(x => x.Id == (int)entityId);
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
                }
            }
            catch
            {
                // Coerce failure: silently skip this field.
            }
        }

        if(scalarChanged) await context.SaveChangesAsync();

        return (applied, false);
    }

    // ───────────────────────────── Scalar-field application ─────────────────────────────

    static async Task<bool> ApplyScalar(MarechaiContext context, SoundSynth s, string fieldName, object value)
    {
        switch(fieldName)
        {
            case FieldName:
                string n = ToStringValue(value);
                if(string.IsNullOrWhiteSpace(n)) return false;
                if(n.Length > 50) return false;
                s.Name = n.Trim();
                return true;
            case FieldCompanyId:
            {
                int? cid = ToInt(value);
                if(!cid.HasValue) { s.CompanyId = null; return true; }
                if(!await context.Companies.AsNoTracking().AnyAsync(c => c.Id == cid.Value)) return false;
                s.CompanyId = cid.Value;
                return true;
            }
            case FieldModelCode:
            {
                string m = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(m) && m.Length > 45) return false;
                s.ModelCode = string.IsNullOrEmpty(m) ? null : m;
                return true;
            }
            case FieldIntroduced:
                s.Introduced = ToDate(value);
                return true;
            case FieldIntroducedPrecision:
            {
                int? pv = ToInt(value);
                if(pv.HasValue && pv.Value is >= 0 and <= 2)
                {
                    s.IntroducedPrecision = (DatePrecision)pv.Value;
                    return true;
                }
                return false;
            }
            case FieldVoices:
            {
                int? v = ToInt(value);
                if(v.HasValue && v.Value < 1) return false;
                s.Voices = v;
                return true;
            }
            case FieldFrequency:
            {
                double? f = ToDouble(value);
                if(f.HasValue && f.Value < 0) return false;
                s.Frequency = f;
                return true;
            }
            case FieldDepth:
            {
                int? d = ToInt(value);
                if(d.HasValue && d.Value < 1) return false;
                s.Depth = d;
                return true;
            }
            case FieldSquareWave:
            {
                int? v = ToInt(value);
                if(v.HasValue && v.Value < 1) return false;
                s.SquareWave = v;
                return true;
            }
            case FieldWhiteNoise:
            {
                int? v = ToInt(value);
                if(v.HasValue && v.Value < 1) return false;
                s.WhiteNoise = v;
                return true;
            }
            case FieldType:
            {
                int? t = ToInt(value);
                if(!t.HasValue) { s.Type = null; return true; }
                if(!Enum.IsDefined(typeof(SoundSynthType), t.Value)) return false;
                s.Type = t.Value;
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Coercion helpers ─────────────────────────────

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
