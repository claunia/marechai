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
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.SoftwareReleaseSuggestionMetadata</c>.
///     Mirrors the design of <see cref="SoftwareSuggestionApplier" /> (ulong-keyed entity)
///     with FOUR junction groups: regions (composite-key short FK), languages (composite-key
///     string FK — FIRST string-keyed composite junction), barcodes (surrogate-Id with
///     multi-field add payload <c>{code,type}</c>), product_codes (surrogate-Id with
///     multi-field add payload <c>{code,issuer}</c>). Re-parenting fields
///     (<c>software_id</c>, <c>software_version_id</c>, <c>is_compilation</c>) are
///     intentionally admin-only and rejected by <see cref="IsKnownFieldName" />.
/// </summary>
internal static class SoftwareReleaseSuggestionApplier
{
    // ---- Scalar field names (mirror client-side metadata) -----------------------------
    public const string FieldTitle                = "title";
    public const string FieldPlatformId           = "platform_id";
    public const string FieldPublisherId          = "publisher_id";
    public const string FieldReleaseDate          = "release_date";
    public const string FieldReleaseDatePrecision = "release_date_precision";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldTitle, FieldPlatformId, FieldPublisherId, FieldReleaseDate, FieldReleaseDatePrecision
    };

    // ---- Junction group identifiers ---------------------------------------------------
    public const string GroupRegions      = "regions";
    public const string GroupLanguages    = "languages";
    public const string GroupBarcodes     = "barcodes";
    public const string GroupProductCodes = "product_codes";

    public static readonly IReadOnlyCollection<string> JunctionGroups = new[]
    {
        GroupRegions,
        GroupLanguages,
        GroupBarcodes,
        GroupProductCodes
    };

    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;
        if(s_scalarFieldNames.Contains(fieldName)) return true;
        return TryParseJunctionKey(fieldName, out _, out _, out _);
    }

    /// <summary>
    ///     Parse a junction operation field-name. Token shapes vary by group:
    ///     <list type="bullet">
    ///         <item><c>regions</c>: token = unm49_id (short).</item>
    ///         <item><c>languages</c>: token = ISO-639-3 code (3-char string).</item>
    ///         <item><c>barcodes</c>: token = row Id (ulong) — surrogate key.</item>
    ///         <item><c>product_codes</c>: token = row Id (ulong) — surrogate key.</item>
    ///     </list>
    ///     For <c>add</c> ops the token is a client-generated GUID (uniqueness scaffold);
    ///     the actual payload arrives as the field value.
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

    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        SoftwareRelease r = await context.SoftwareReleases.AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.Id == (ulong)entityId);
        if(r is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldTitle]                = r.Title,
            [FieldPlatformId]           = r.PlatformId,
            [FieldPublisherId]          = r.PublisherId,
            [FieldReleaseDate]          = r.ReleaseDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldReleaseDatePrecision] = (int)r.ReleaseDatePrecision
        };
    }

    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        SoftwareRelease r = await context.SoftwareReleases.FirstOrDefaultAsync(x => x.Id == (ulong)entityId);
        if(r is null) return (applied, true);

        bool scalarChanged = false;

        foreach(string fieldName in accepted)
        {
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(s_scalarFieldNames.Contains(fieldName))
                {
                    if(await ApplyScalar(context, r, fieldName, value))
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

    static async Task<bool> ApplyScalar(MarechaiContext context, SoftwareRelease r, string fieldName, object value)
    {
        switch(fieldName)
        {
            case FieldTitle:
            {
                string t = ToStringValue(value);
                // Title is nullable on the entity. Empty / whitespace clears it.
                r.Title = string.IsNullOrWhiteSpace(t) ? null : t.Trim();
                return true;
            }
            case FieldPlatformId:
            {
                ulong? pid = ToUlong(value);
                if(!pid.HasValue) { r.PlatformId = null; return true; }
                if(!await context.SoftwarePlatforms.AsNoTracking().AnyAsync(p => p.Id == pid.Value)) return false;
                r.PlatformId = pid.Value;
                return true;
            }
            case FieldPublisherId:
            {
                int? pid = ToInt(value);
                // PublisherId is [Required] on the entity — reject null/clear.
                if(!pid.HasValue) return false;
                if(!await context.Companies.AsNoTracking().AnyAsync(c => c.Id == pid.Value)) return false;
                r.PublisherId = pid.Value;
                return true;
            }
            case FieldReleaseDate:
            {
                DateTime? d = ToDate(value);
                r.ReleaseDate = d;
                return true;
            }
            case FieldReleaseDatePrecision:
            {
                int? p = ToInt(value);
                if(!p.HasValue) return false;
                if(!Enum.IsDefined(typeof(DatePrecision), p.Value)) return false;
                r.ReleaseDatePrecision = (DatePrecision)p.Value;
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction add ─────────────────────────────

    static async Task<bool> ApplyJunctionAdd(MarechaiContext context, ulong releaseId, string group, object value)
    {
        Dictionary<string, object> payload = ExtractObject(value);
        if(payload is null) return false;

        switch(group)
        {
            case GroupRegions:
            {
                int? rawId = GetInt(payload, "unm49_id");
                if(!rawId.HasValue) return false;
                if(rawId.Value < short.MinValue || rawId.Value > short.MaxValue) return false;
                short id = (short)rawId.Value;
                if(!await context.UnM49.AsNoTracking().AnyAsync(u => u.Id == id)) return false;
                if(await context.UnM49BySoftwareRelease.AsNoTracking()
                                .AnyAsync(x => x.SoftwareReleaseId == releaseId && x.UnM49Id == id))
                    return false;
                await context.UnM49BySoftwareRelease.AddAsync(new UnM49BySoftwareRelease
                {
                    SoftwareReleaseId = releaseId,
                    UnM49Id           = id
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupLanguages:
            {
                string code = GetString(payload, "language_code");
                if(string.IsNullOrWhiteSpace(code)) return false;
                code = code.Trim();
                if(code.Length is < 2 or > 3) return false;
                if(!await context.Iso639.AsNoTracking().AnyAsync(l => l.Id == code)) return false;
                if(await context.LanguageBySoftwareRelease.AsNoTracking()
                                .AnyAsync(x => x.SoftwareReleaseId == releaseId && x.LanguageCode == code))
                    return false;
                await context.LanguageBySoftwareRelease.AddAsync(new LanguageBySoftwareRelease
                {
                    SoftwareReleaseId = releaseId,
                    LanguageCode      = code
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupBarcodes:
            {
                string code = GetString(payload, "code");
                int? typeRaw = GetInt(payload, "type");
                if(string.IsNullOrWhiteSpace(code)) return false;
                if(!typeRaw.HasValue) return false;
                if(!Enum.IsDefined(typeof(BarcodeType), (byte)typeRaw.Value)) return false;
                code = code.Trim();
                // Code is globally unique (HasIndex(Code).IsUnique()).
                if(await context.SoftwareBarcodes.AsNoTracking().AnyAsync(b => b.Code == code)) return false;
                await context.SoftwareBarcodes.AddAsync(new SoftwareBarcode
                {
                    ReleaseId = releaseId,
                    Code      = code,
                    Type      = (BarcodeType)typeRaw.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupProductCodes:
            {
                string code = GetString(payload, "code");
                int? issuerRaw = GetInt(payload, "issuer");
                if(string.IsNullOrWhiteSpace(code)) return false;
                if(!issuerRaw.HasValue) return false;
                if(!Enum.IsDefined(typeof(ProductCodeIssuer), (byte)issuerRaw.Value)) return false;
                code = code.Trim();
                ProductCodeIssuer issuer = (ProductCodeIssuer)issuerRaw.Value;
                // Unique pair (Issuer, Code) — HasIndex(new{Issuer,Code}).IsUnique().
                if(await context.SoftwareProductCodes.AsNoTracking()
                                .AnyAsync(p => p.Issuer == issuer && p.Code == code)) return false;
                await context.SoftwareProductCodes.AddAsync(new SoftwareProductCode
                {
                    ReleaseId = releaseId,
                    Issuer    = issuer,
                    Code      = code
                });
                await context.SaveChangesAsync();
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction remove ─────────────────────────────

    static async Task<bool> ApplyJunctionRemove(MarechaiContext context, ulong releaseId, string group, string token)
    {
        switch(group)
        {
            case GroupRegions:
            {
                if(!short.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out short id))
                    return false;
                return await context.UnM49BySoftwareRelease
                                    .Where(x => x.SoftwareReleaseId == releaseId && x.UnM49Id == id)
                                    .ExecuteDeleteAsync() > 0;
            }
            case GroupLanguages:
            {
                // String-keyed composite junction — token IS the language code.
                string code = token;
                if(code.Length is < 2 or > 3) return false;
                return await context.LanguageBySoftwareRelease
                                    .Where(x => x.SoftwareReleaseId == releaseId && x.LanguageCode == code)
                                    .ExecuteDeleteAsync() > 0;
            }
            case GroupBarcodes:
            {
                if(!ulong.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong rowId))
                    return false;
                // Surrogate-Id: gate delete on parent FK (ReleaseId) to prevent cross-entity
                // tampering with row ids harvested from another release's suggestion list.
                return await context.SoftwareBarcodes
                                    .Where(b => b.Id == rowId && b.ReleaseId == releaseId)
                                    .ExecuteDeleteAsync() > 0;
            }
            case GroupProductCodes:
            {
                if(!ulong.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong rowId))
                    return false;
                return await context.SoftwareProductCodes
                                    .Where(p => p.Id == rowId && p.ReleaseId == releaseId)
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

    static DateTime? ToDate(object v)
    {
        return v switch
        {
            null            => null,
            DateTime dt     => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
            DateTimeOffset dto => dto.UtcDateTime,
            JsonElement je  => je.ValueKind switch
            {
                JsonValueKind.Null   => null,
                JsonValueKind.String => DateTime.TryParse(je.GetString(), CultureInfo.InvariantCulture,
                                          DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                          out DateTime d) ? d : null,
                _                    => null
            },
            string str      => DateTime.TryParse(str, CultureInfo.InvariantCulture,
                                  DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                  out DateTime d) ? d : null,
            _               => null
        };
    }
}
