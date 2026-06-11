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

    /// <summary>
    ///     Pseudo-field carrying the parent Software FK at addition time only (consumed by
    ///     <see cref="CreateAsync" />). Intentionally NOT in <c>s_scalarFieldNames</c>:
    ///     <see cref="ApplyAsync" /> rejects it as an unknown scalar to keep edit-mode
    ///     re-parenting protection (changing the parent Software of an existing release
    ///     stays admin-only). It IS whitelisted in <see cref="IsKnownFieldName" /> so the
    ///     controller's field-name validator accepts it on the wire.
    /// </summary>
    public const string FieldSoftwareId = "software_id";

    /// <summary>
    ///     Pseudo-field carrying the parent SoftwareVersion FK at addition time only
    ///     (consumed by <see cref="CreateAsync" />). Same semantics as
    ///     <see cref="FieldSoftwareId" />: NOT in <c>s_scalarFieldNames</c> (edit-mode
    ///     re-parenting between versions stays admin-only), but whitelisted in
    ///     <see cref="IsKnownFieldName" /> so the controller's field-name validator accepts
    ///     it on the wire. Used by the combined SoftwareVersion + first-Release atomic
    ///     creation flow (<c>Marechai.Server.Suggestions.SoftwareVersionSuggestionApplier</c>)
    ///     to link the freshly-minted release to the freshly-minted version. Optional on
    ///     stand-alone release creation — omitted when the release isn't version-scoped.
    /// </summary>
    public const string FieldSoftwareVersionId = "software_version_id";

    // ---- Junction group identifiers ---------------------------------------------------
    public const string GroupRegions      = "regions";
    public const string GroupLanguages    = "languages";
    public const string GroupBarcodes     = "barcodes";
    public const string GroupProductCodes = "product_codes";
    public const string GroupSpecs        = "specs";
    public const string GroupRatings      = "ratings";
    public const string GroupMinGpus      = "min_gpus";
    public const string GroupRecGpus      = "rec_gpus";
    public const string GroupSoundSynths  = "sound_synths";

    // SoftwareAttribute.Category discriminator literals (database-bound strings).
    public const string AttributeCategorySpec   = "Spec";
    public const string AttributeCategoryRating = "Rating";

    // Database column limits — kept in sync with SoftwareAttribute model.
    const int MAX_ATTRIBUTE_KEY_LENGTH   = 128;
    const int MAX_ATTRIBUTE_VALUE_LENGTH = 512;

    public static readonly IReadOnlyCollection<string> JunctionGroups = new[]
    {
        GroupRegions,
        GroupLanguages,
        GroupBarcodes,
        GroupProductCodes,
        GroupSpecs,
        GroupRatings,
        GroupMinGpus,
        GroupRecGpus,
        GroupSoundSynths
    };

    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;
        if(s_scalarFieldNames.Contains(fieldName)) return true;
        if(fieldName == FieldSoftwareId) return true;
        if(fieldName == FieldSoftwareVersionId) return true;
        return TryParseJunctionKey(fieldName, out _, out _, out _);
    }

    /// <summary>
    ///     Parse a junction operation field-name. Token shapes vary by group:
    ///     <list type="bullet">
    ///         <item><c>regions</c>: token = unm49_id (short).</item>
    ///         <item><c>languages</c>: token = ISO-639-3 code (3-char string).</item>
    ///         <item><c>barcodes</c>: token = row Id (ulong) — surrogate key.</item>
    ///         <item><c>product_codes</c>: token = row Id (ulong) — surrogate key.</item>
    ///         <item><c>specs</c>: token = row Id (long) — surrogate key on SoftwareAttribute.</item>
    ///         <item><c>ratings</c>: token = row Id (long) — surrogate key on SoftwareAttribute.</item>
    ///         <item><c>min_gpus</c>: token = gpu_id (int) — composite-key junction.</item>
    ///         <item><c>rec_gpus</c>: token = gpu_id (int) — composite-key junction.</item>
    ///         <item><c>sound_synths</c>: token = sound_synth_id (int) — composite-key junction.</item>
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

    /// <summary>
    ///     Create a brand-new SoftwareRelease from an accepted suggestion (entity_id ==
    ///     null path). Returns the new id (ulong) + the field-name set that was actually
    ///     applied. Returns <c>(null, empty)</c> when creation can't proceed (mandatory
    ///     fields missing or FK validation fails). Junction <c>*.add.*</c> keys are
    ///     applied in a second pass against the freshly-minted release Id; <c>*.remove.*</c>
    ///     keys are silently skipped (a brand-new entity has nothing to remove from).
    ///
    ///     <para>
    ///         Mandatory fields: <c>software_id</c> (parent Software FK existence-checked),
    ///         <c>publisher_id</c> ([Required] FK existence-checked), <c>title</c> (non-
    ///         whitespace), <c>platform_id</c> (FK existence-checked). The dialog gates the
    ///         submit button on the same four fields; this server check is defence-in-depth.
    ///     </para>
    /// </summary>
    /// <param name="creditedUserId">
    ///     The Identity user id to attribute the row to in audit history (the suggesting
    ///     user, NOT the reviewing admin). Forwarded to <c>SaveChangesWithUserAsync</c>.
    /// </param>
    public static async Task<(ulong? newId, HashSet<string> applied)> CreateAsync(
        MarechaiContext context,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string creditedUserId)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        // Mandatory: parent Software FK (consumed only here, not in ApplyAsync).
        if(!accepted.Contains(FieldSoftwareId) || !suggested.TryGetValue(FieldSoftwareId, out object softwareIdRaw))
            return (null, applied);
        ulong? softwareIdParsed = ToUlong(softwareIdRaw);
        if(!softwareIdParsed.HasValue || softwareIdParsed.Value == 0) return (null, applied);
        ulong softwareId = softwareIdParsed.Value;
        if(!await context.Softwares.AsNoTracking().AnyAsync(s => s.Id == softwareId))
            return (null, applied);

        // Mandatory: PublisherId ([Required] on the entity, FK existence-checked).
        if(!accepted.Contains(FieldPublisherId) || !suggested.TryGetValue(FieldPublisherId, out object pubIdRaw))
            return (null, applied);
        int? publisherId = ToInt(pubIdRaw);
        if(!publisherId.HasValue) return (null, applied);
        if(!await context.Companies.AsNoTracking().AnyAsync(c => c.Id == publisherId.Value))
            return (null, applied);

        // Mandatory: Title (non-whitespace per the dialog gate; nullable on the entity but
        // we require it here for queue-display + reviewer sanity).
        if(!accepted.Contains(FieldTitle) || !suggested.TryGetValue(FieldTitle, out object titleVal))
            return (null, applied);
        string title = ToStringValue(titleVal);
        if(string.IsNullOrWhiteSpace(title)) return (null, applied);

        // Mandatory: Platform FK existence-checked (nullable on the entity but the dialog
        // gates submit on it being present).
        if(!accepted.Contains(FieldPlatformId) || !suggested.TryGetValue(FieldPlatformId, out object platIdRaw))
            return (null, applied);
        ulong? platformId = ToUlong(platIdRaw);
        if(!platformId.HasValue) return (null, applied);
        if(!await context.SoftwarePlatforms.AsNoTracking().AnyAsync(p => p.Id == platformId.Value))
            return (null, applied);

        var r = new SoftwareRelease
        {
            SoftwareId    = softwareId,
            PublisherId   = publisherId.Value,
            Title         = title.Trim(),
            PlatformId    = platformId.Value,
            IsCompilation = false
        };
        applied.Add(FieldSoftwareId);
        applied.Add(FieldPublisherId);
        applied.Add(FieldTitle);
        applied.Add(FieldPlatformId);

        // Optional: SoftwareVersionId pseudo-field (creation-time only). Used by the
        // combined SoftwareVersion + first-Release atomic creation flow to link the
        // freshly-minted release back to the freshly-minted version. Silently skipped
        // when not accepted or when the FK row doesn't exist (defence-in-depth — the
        // caller injects this id from the just-saved SoftwareVersion.Id so it WILL
        // exist in practice).
        if(accepted.Contains(FieldSoftwareVersionId) &&
           suggested.TryGetValue(FieldSoftwareVersionId, out object versionIdRaw))
        {
            ulong? versionIdParsed = ToUlong(versionIdRaw);
            if(versionIdParsed.HasValue                                    &&
               versionIdParsed.Value != 0                                  &&
               await context.SoftwareVersions.AsNoTracking()
                            .AnyAsync(v => v.Id == versionIdParsed.Value))
            {
                r.SoftwareVersionId = versionIdParsed.Value;
                applied.Add(FieldSoftwareVersionId);
            }
        }

        // Apply remaining accepted scalar fields (release_date, release_date_precision).
        // Mandatory fields are handled above; junctions and software_id pseudo-field skip.
        foreach(string fieldName in accepted)
        {
            if(fieldName == FieldSoftwareId) continue;
            if(fieldName == FieldTitle) continue;
            if(fieldName == FieldPublisherId) continue;
            if(fieldName == FieldPlatformId) continue;
            if(!s_scalarFieldNames.Contains(fieldName)) continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyScalar(context, r, fieldName, value)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this field.
            }
        }

        await context.SoftwareReleases.AddAsync(r);

        await context.SaveChangesWithUserAsync(creditedUserId);

        // Now apply junction adds with the freshly-minted release id. Remove keys are
        // silently ignored — a brand-new entity has nothing to remove from.
        foreach(string fieldName in accepted)
        {
            if(!TryParseJunctionKey(fieldName, out string group, out string op, out string _)) continue;
            if(op != "add") continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyJunctionAdd(context, r.Id, group, value, creditedUserId)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this junction add.
            }
        }

        return (r.Id, applied);
    }

    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string creditedUserId)
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
                                  ? await ApplyJunctionAdd(context, (ulong)entityId, group, value, creditedUserId)
                                  : await ApplyJunctionRemove(context, (ulong)entityId, group, token, creditedUserId);
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

    static async Task<bool> ApplyJunctionAdd(MarechaiContext context, ulong releaseId, string group, object value,
        string creditedUserId)
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
                await context.SaveChangesWithUserAsync(creditedUserId);
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
                await context.SaveChangesWithUserAsync(creditedUserId);
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
                await context.SaveChangesWithUserAsync(creditedUserId);
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
                await context.SaveChangesWithUserAsync(creditedUserId);
                return true;
            }
            case GroupSpecs:
                return await AddSoftwareAttributeAsync(context, releaseId, payload, AttributeCategorySpec, creditedUserId);
            case GroupRatings:
                return await AddSoftwareAttributeAsync(context, releaseId, payload, AttributeCategoryRating, creditedUserId);
            case GroupMinGpus:
            {
                int? gpuId = GetInt(payload, "gpu_id");
                if(!gpuId.HasValue) return false;
                if(!await context.Gpus.AsNoTracking().AnyAsync(g => g.Id == gpuId.Value)) return false;
                if(await context.MinimumGpuBySoftwareRelease.AsNoTracking()
                                .AnyAsync(x => x.ReleaseId == releaseId && x.GpuId == gpuId.Value))
                    return false;
                await context.MinimumGpuBySoftwareRelease.AddAsync(new MinimumGpuBySoftwareRelease
                {
                    ReleaseId = releaseId,
                    GpuId     = gpuId.Value
                });
                await context.SaveChangesWithUserAsync(creditedUserId);
                return true;
            }
            case GroupRecGpus:
            {
                int? gpuId = GetInt(payload, "gpu_id");
                if(!gpuId.HasValue) return false;
                if(!await context.Gpus.AsNoTracking().AnyAsync(g => g.Id == gpuId.Value)) return false;
                if(await context.RecommendedGpuBySoftwareRelease.AsNoTracking()
                                .AnyAsync(x => x.ReleaseId == releaseId && x.GpuId == gpuId.Value))
                    return false;
                await context.RecommendedGpuBySoftwareRelease.AddAsync(new RecommendedGpuBySoftwareRelease
                {
                    ReleaseId = releaseId,
                    GpuId     = gpuId.Value
                });
                await context.SaveChangesWithUserAsync(creditedUserId);
                return true;
            }
            case GroupSoundSynths:
            {
                int? synthId = GetInt(payload, "sound_synth_id");
                if(!synthId.HasValue) return false;
                if(!await context.SoundSynths.AsNoTracking().AnyAsync(s => s.Id == synthId.Value)) return false;
                if(await context.SoundSynthBySoftwareRelease.AsNoTracking()
                                .AnyAsync(x => x.ReleaseId == releaseId && x.SoundSynthId == synthId.Value))
                    return false;
                await context.SoundSynthBySoftwareRelease.AddAsync(new SoundSynthBySoftwareRelease
                {
                    ReleaseId    = releaseId,
                    SoundSynthId = synthId.Value
                });
                await context.SaveChangesWithUserAsync(creditedUserId);
                return true;
            }
            default:
                return false;
        }
    }

    /// <summary>
    ///     Validate and insert a new <see cref="SoftwareAttribute" /> row for the given
    ///     release. Payload must carry <c>key</c> and <c>value</c> string fields. Dedup
    ///     matches the unique index <c>(SoftwareReleaseId, Category, Key)</c>: only ONE
    ///     value is allowed per (release, category, key) so a duplicate-key add is
    ///     rejected. Both length limits mirror the database column constraints.
    /// </summary>
    static async Task<bool> AddSoftwareAttributeAsync(MarechaiContext context, ulong releaseId,
                                                      Dictionary<string, object> payload, string category,
                                                      string creditedUserId)
    {
        string key   = GetString(payload, "key");
        string value = GetString(payload, "value");
        if(string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return false;
        key   = key.Trim();
        value = value.Trim();
        if(key.Length   > MAX_ATTRIBUTE_KEY_LENGTH)   return false;
        if(value.Length > MAX_ATTRIBUTE_VALUE_LENGTH) return false;
        if(await context.SoftwareAttributes.AsNoTracking()
                        .AnyAsync(a => a.SoftwareReleaseId == releaseId &&
                                       a.Category          == category   &&
                                       a.Key               == key))
            return false;
        await context.SoftwareAttributes.AddAsync(new SoftwareAttribute
        {
            SoftwareReleaseId = releaseId,
            Category          = category,
            Key               = key,
            Value             = value
        });
        await context.SaveChangesWithUserAsync(creditedUserId);
        return true;
    }

    // ───────────────────────────── Junction remove ─────────────────────────────

    static async Task<bool> ApplyJunctionRemove(MarechaiContext context, ulong releaseId, string group, string token,
        string creditedUserId)
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
            case GroupSpecs:
                return await RemoveSoftwareAttributeAsync(context, releaseId, creditedUserId, token, AttributeCategorySpec);
            case GroupRatings:
                return await RemoveSoftwareAttributeAsync(context, releaseId, creditedUserId, token, AttributeCategoryRating);
            case GroupMinGpus:
            {
                if(!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int gpuId))
                    return false;
                return await context.MinimumGpuBySoftwareRelease
                                    .Where(x => x.ReleaseId == releaseId && x.GpuId == gpuId)
                                    .ExecuteDeleteAsync() > 0;
            }
            case GroupRecGpus:
            {
                if(!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int gpuId))
                    return false;
                return await context.RecommendedGpuBySoftwareRelease
                                    .Where(x => x.ReleaseId == releaseId && x.GpuId == gpuId)
                                    .ExecuteDeleteAsync() > 0;
            }
            case GroupSoundSynths:
            {
                if(!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int synthId))
                    return false;
                return await context.SoundSynthBySoftwareRelease
                                    .Where(x => x.ReleaseId == releaseId && x.SoundSynthId == synthId)
                                    .ExecuteDeleteAsync() > 0;
            }
            default:
                return false;
        }
    }

    /// <summary>
    ///     Remove a <see cref="SoftwareAttribute" /> row by surrogate Id. Gating on BOTH
    ///     <see cref="SoftwareAttribute.SoftwareReleaseId" /> and <see cref="SoftwareAttribute.Category" />
    ///     prevents cross-entity tampering: a Specs dialog cannot remove a Rating row even
    ///     if the user knows its id, and an attacker cannot delete attributes belonging to
    ///     another release by guessing a row id.
    /// </summary>
    static async Task<bool> RemoveSoftwareAttributeAsync(MarechaiContext context, ulong releaseId, string creditedUserId,
                                                         string token, string category)
    {
        if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) return false;
        return await context.SoftwareAttributes
                            .Where(a => a.Id == rowId &&
                                        a.SoftwareReleaseId == releaseId &&
                                        a.Category          == category)
                            .ExecuteDeleteAsync() > 0;
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
