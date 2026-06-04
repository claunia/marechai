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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.Server.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Suggestions;

/// <summary>
///     Applier for collaborative GPU-photo upload suggestions
///     (<see cref="SuggestionEntityType.GpuPhoto" />).
///     <para>
///         A single suggestion row carries a batch of 1-15 pending photos for the same parent
///         GPU, plus suggestion-level <c>license_id</c> and (optional) <c>source_url</c> that
///         apply to every photo in the batch. The dialog stages each photo via
///         <c>POST /gpus/photos/pending?gpuId={id}</c> first (writes file + sidecar to disk
///         under <c>photos/gpus/pending/</c>) then submits ONE <c>/suggestions</c> POST
///         referencing the per-photo guids.
///     </para>
///     <para>
///         At admin review time, the admin per-photo accepts/rejects via the field-name keys
///         <c>photo.&lt;guid&gt;</c>. Accepted photos are promoted to
///         <c>photos/gpus/originals/</c> and a <see cref="GpuPhoto" /> row is created with
///         the suggestion-level license + source URL + per-photo comment + auto-extracted
///         EXIF metadata. The same fire-and-forget <c>Photos.ConversionWorker</c> used by
///         the admin upload path materialises all JPEG/WebP/AVIF variants. Rejected
///         photos have their pending files deleted.
///     </para>
/// </summary>
public static class GpuPhotoSuggestionApplier
{
    /// <summary>Wire field name carrying the suggestion-level <c>int</c> license id (CC or PD).</summary>
    public const string FieldLicenseId = "license_id";

    /// <summary>
    ///     Wire field name carrying the suggestion-level optional source URL applied to every
    ///     accepted photo.
    /// </summary>
    public const string FieldSourceUrl = "source_url";

    /// <summary>Wire field name carrying the JSON array of per-photo descriptors.</summary>
    public const string FieldPhotos = "photos";

    /// <summary>Hard upper bound on the number of photos a single batch may carry.</summary>
    public const int MaxPhotosPerBatch = 15;

    /// <summary>Maximum length for the suggestion-level source URL.</summary>
    public const int MaxSourceUrlLength = 500;

    /// <summary>Maximum length for the per-photo comment field.</summary>
    public const int MaxCommentLength = 500;

    /// <summary>SPDX ids treated as Public Domain in addition to the CC- prefix family.</summary>
    static readonly HashSet<string> _publicDomainSpdxIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "CC0-1.0", "Unlicense"
    };

    /// <summary>Allowed lower-case file extensions (matching the pending upload endpoint).</summary>
    static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "jpg", "jpeg", "png", "webp"
    };

    /// <summary>
    ///     Build the field-name key the admin SuggestionDiffPanel emits when accepting a
    ///     specific photo (e.g. <c>photo.123e4567-e89b-12d3-a456-426614174000</c>).
    /// </summary>
    public static string PhotoAcceptKey(Guid guid) =>
        "photo." + guid.ToString("D", CultureInfo.InvariantCulture);

    /// <summary>True when the field name is one we recognise on the wire.</summary>
    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;

        if(fieldName == FieldLicenseId || fieldName == FieldSourceUrl || fieldName == FieldPhotos) return true;

        // photo.<guid> — per-photo accept toggle
        if(fieldName.StartsWith("photo.", StringComparison.Ordinal))
        {
            string rest = fieldName.Substring("photo.".Length);
            return Guid.TryParse(rest, out _);
        }

        return false;
    }

    /// <summary>
    ///     "Current values" for a GpuPhoto batch suggestion. There are no comparable existing
    ///     values to diff against — the suggestion is a brand-new batch of photos the admin
    ///     either accepts (creates new GpuPhoto rows) or rejects. The diff endpoint still
    ///     calls this to detect whether the parent entity has been deleted: returning a
    ///     non-null (possibly empty) dictionary signals "parent GPU exists"; returning null
    ///     signals "parent missing — close the suggestion as stale".
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));

        bool gpuExists = await context.Gpus.AsNoTracking().AnyAsync(g => g.Id == (int)entityId);
        return gpuExists ? new Dictionary<string, object>(StringComparer.Ordinal) : null;
    }

    /// <summary>
    ///     Validate the submitted payload before the suggestion row is persisted. Verifies the
    ///     parent GPU exists, the license is CC/PD-compatible, the source URL (if present) is
    ///     within length limits, the photos array is well-formed (1-15 entries), and EVERY
    ///     referenced pending sidecar exists and belongs to the caller AND is scoped to the
    ///     parent GPU. Returns <c>(true, null)</c> on success, <c>(false, errorDetail)</c>
    ///     otherwise.
    /// </summary>
    public static async Task<(bool ok, string error)> ValidateAsync(MarechaiContext context, long? entityId,
                                                                    Dictionary<string, object> suggested,
                                                                    string assetRootPath, string uploaderUserId)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));
        if(suggested is null) return (false, "Missing suggestion payload.");

        if(!entityId.HasValue || entityId.Value <= 0)
            return (false, "GPU photo suggestions must reference an existing GPU via entity_id.");

        int gpuId = (int)entityId.Value;
        bool gpuExists = await context.Gpus.AsNoTracking().AnyAsync(g => g.Id == gpuId);
        if(!gpuExists) return (false, $"GPU #{gpuId} not found.");

        // ── License (suggestion-level, mandatory) ────────────────────────────────
        if(!suggested.TryGetValue(FieldLicenseId, out object licenseRaw) || licenseRaw is null)
            return (false, "A GPU photo suggestion must include a 'license_id' field.");

        if(!TryCoerceInt(licenseRaw, out int licenseId))
            return (false, "'license_id' must be an integer.");

        License license = await context.Licenses.AsNoTracking().FirstOrDefaultAsync(l => l.Id == licenseId);
        if(license is null) return (false, $"License #{licenseId} not found.");

        if(!IsCreativeCommonsOrPublicDomain(license))
            return (false, "Only Creative Commons or Public Domain licenses are allowed for collaborator-uploaded GPU photos.");

        // ── Source URL (suggestion-level, optional) ──────────────────────────────
        if(suggested.TryGetValue(FieldSourceUrl, out object srcRaw) && srcRaw is not null)
        {
            string srcStr = CoerceString(srcRaw);

            if(!string.IsNullOrEmpty(srcStr) && srcStr.Length > MaxSourceUrlLength)
                return (false, $"'source_url' cannot exceed {MaxSourceUrlLength} characters.");

            if(!string.IsNullOrEmpty(srcStr) && !LooksLikeUrl(srcStr))
                return (false, "'source_url' must be a valid http(s) URL.");
        }

        // ── Photos array (mandatory) ─────────────────────────────────────────────
        if(!suggested.TryGetValue(FieldPhotos, out object photosRaw) || photosRaw is null)
            return (false, "A GPU photo suggestion must include a non-empty 'photos' array.");

        List<PhotoEntry> entries = ParsePhotosArray(photosRaw);
        if(entries is null) return (false, "'photos' must be a JSON array of objects.");
        if(entries.Count == 0) return (false, "A GPU photo suggestion must include at least one photo.");

        if(entries.Count > MaxPhotosPerBatch)
            return (false, $"A GPU photo suggestion cannot include more than {MaxPhotosPerBatch} photos.");

        // ── Per-photo validation ─────────────────────────────────────────────────
        var seenGuids = new HashSet<Guid>();

        for(int i = 0; i < entries.Count; i++)
        {
            PhotoEntry e = entries[i];

            if(e.Guid == Guid.Empty) return (false, $"Photo #{i + 1}: missing or invalid 'guid'.");

            if(!seenGuids.Add(e.Guid))
                return (false, $"Photo #{i + 1}: duplicate guid {e.Guid:D}.");

            if(string.IsNullOrEmpty(e.Extension) ||
               !_allowedExtensions.Contains(e.Extension.TrimStart('.')))
                return (false, $"Photo #{i + 1}: unsupported extension '{e.Extension}'. Accepted: jpg, jpeg, png, webp.");

            if(!string.IsNullOrEmpty(e.Comment) && e.Comment.Length > MaxCommentLength)
                return (false, $"Photo #{i + 1}: comment cannot exceed {MaxCommentLength} characters.");

            // Sidecar must exist, belong to the uploader, be scoped to this GPU.
            PendingImageStore.PendingMetadata meta =
                await PendingImageStore.GetMetadataAsync(assetRootPath, "gpus", e.Guid);

            if(meta is null) return (false, $"Photo #{i + 1}: pending upload {e.Guid:D} not found.");

            if(meta.EntityType != (byte)SuggestionEntityType.GpuPhoto)
                return (false, $"Photo #{i + 1}: pending upload {e.Guid:D} is not a GPU photo.");

            if(!string.Equals(meta.UploadedById, uploaderUserId, StringComparison.Ordinal))
                return (false, $"Photo #{i + 1}: pending upload {e.Guid:D} belongs to a different user.");

            if(meta.ParentEntityId != entityId.Value)
                return (false, $"Photo #{i + 1}: pending upload {e.Guid:D} is scoped to a different GPU.");
        }

        return (true, null);
    }

    /// <summary>
    ///     Apply an admin's accept/reject decisions. For each photo in the suggested array:
    ///     if its <c>photo.&lt;guid&gt;</c> key is in <paramref name="accepted" />, promote
    ///     the pending file to <c>originals/</c>, create a <see cref="GpuPhoto" /> row with
    ///     the suggestion-level license/source + per-photo comment + extracted EXIF, and
    ///     fire-and-forget the conversion worker. Otherwise, delete the pending file.
    ///     Returns the set of accept-keys actually applied (subset of <paramref name="accepted" />)
    ///     plus a flag indicating the parent GPU was missing (so the controller can mark the
    ///     suggestion stale).
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(MarechaiContext context,
        long entityId, Dictionary<string, object> suggested, HashSet<string> accepted, string creditedUserId,
        string assetRootPath)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);
        if(suggested is null) return (applied, false);

        int gpuId = (int)entityId;
        bool gpuExists = await context.Gpus.AnyAsync(g => g.Id == gpuId);
        if(!gpuExists) return (applied, true);

        // Suggestion-level license + source URL apply to every accepted photo.
        if(!suggested.TryGetValue(FieldLicenseId, out object licenseRaw) || !TryCoerceInt(licenseRaw, out int licenseId))
            return (applied, false);

        bool licenseExists = await context.Licenses.AnyAsync(l => l.Id == licenseId);
        if(!licenseExists) return (applied, false);

        string sourceUrl = null;
        if(suggested.TryGetValue(FieldSourceUrl, out object srcRaw) && srcRaw is not null)
        {
            string s = CoerceString(srcRaw);
            sourceUrl = string.IsNullOrEmpty(s) ? null : s;
        }

        if(!suggested.TryGetValue(FieldPhotos, out object photosRaw) || photosRaw is null) return (applied, false);

        List<PhotoEntry> entries = ParsePhotosArray(photosRaw);
        if(entries is null) return (applied, false);

        foreach(PhotoEntry e in entries)
        {
            if(e.Guid == Guid.Empty) continue;

            string acceptKey = PhotoAcceptKey(e.Guid);

            if(accepted.Contains(acceptKey))
            {
                bool ok = await PromoteAndPersistAsync(context, gpuId, licenseId, sourceUrl, creditedUserId,
                                                        assetRootPath, e);

                if(ok) applied.Add(acceptKey);
            }
            else
            {
                // Rejected (or admin left it unchecked): clean up the pending file.
                try { PendingImageStore.Delete(assetRootPath, "gpus", e.Guid); } catch { /* best-effort */ }
            }
        }

        return (applied, false);
    }

    static async Task<bool> PromoteAndPersistAsync(MarechaiContext context, int gpuId, int licenseId, string sourceUrl,
                                                    string creditedUserId, string assetRootPath, PhotoEntry entry)
    {
        try
        {
            (string movedPath, string ext) =
                await PendingImageStore.PromoteToOriginalsAsync(assetRootPath, "gpus", entry.Guid);

            if(movedPath is null || string.IsNullOrEmpty(ext)) return false;

            var photo = new GpuPhoto
            {
                Id                = entry.Guid,
                GpuId             = gpuId,
                LicenseId         = licenseId,
                Source            = sourceUrl,
                Comments          = string.IsNullOrEmpty(entry.Comment) ? null : entry.Comment,
                UserId            = creditedUserId,
                UploadDate        = DateTime.UtcNow,
                OriginalExtension = ext
            };

            // EXIF: extract from the promoted file (read-only stream).
            try
            {
                await using var fs = new FileStream(movedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                PhotoExifExtractor.ExtractInto(photo, fs);
            }
            catch
            {
                // EXIF extraction failure is non-fatal — photo still gets created without metadata.
            }

            await context.GpuPhotos.AddAsync(photo);
            await context.SaveChangesWithUserAsync(creditedUserId);

            // Fire-and-forget conversion worker — same pattern as admin GpuPhotosController.UploadAsync.
            string sourceFormat = ext.TrimStart('.');
            Guid   newId        = photo.Id;
            string capturedPath = movedPath;
            string capturedRoot = assetRootPath;

            _ = Task.Run(() =>
            {
                try
                {
                    Marechai.Helpers.Photos.EnsureCreated(capturedRoot, false, "gpus");
                    var photos = new Marechai.Helpers.Photos();
                    photos.ConversionWorker(capturedRoot, newId, capturedPath, sourceFormat, false, "gpus");
                }
                catch
                {
                    // ignored — original is safe in originals/, conversion can be retried later
                }
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     True when the license is allowed for collaborator-uploaded photos: SPDX prefix
    ///     <c>CC-</c>, OR the SPDX is in the curated PD allowlist (<c>CC0-1.0</c>,
    ///     <c>Unlicense</c>), OR the literal Name "Public Domain" (because the seeded
    ///     Public Domain license has null SPDX — Name fallback is required).
    /// </summary>
    public static bool IsCreativeCommonsOrPublicDomain(License license)
    {
        if(license is null) return false;

        string spdx = license.SPDX;

        if(!string.IsNullOrEmpty(spdx))
        {
            if(spdx.StartsWith("CC-", StringComparison.OrdinalIgnoreCase)) return true;
            if(_publicDomainSpdxIds.Contains(spdx)) return true;
        }

        return string.Equals(license.Name, "Public Domain", StringComparison.OrdinalIgnoreCase);
    }

    // ───────────────────────────── helpers ─────────────────────────────

    sealed class PhotoEntry
    {
        public Guid   Guid      { get; init; }
        public string Extension { get; init; }
        public string Comment   { get; init; }
    }

    static List<PhotoEntry> ParsePhotosArray(object raw)
    {
        var result = new List<PhotoEntry>();

        if(raw is JsonElement je)
        {
            if(je.ValueKind != JsonValueKind.Array) return null;

            foreach(JsonElement item in je.EnumerateArray())
            {
                if(item.ValueKind != JsonValueKind.Object) return null;
                PhotoEntry e = ParsePhotoFromJson(item);
                if(e is null) return null;
                result.Add(e);
            }

            return result;
        }

        if(raw is System.Collections.IEnumerable enumerable)
        {
            foreach(object item in enumerable)
            {
                PhotoEntry e = ParsePhotoFromObject(item);
                if(e is null) return null;
                result.Add(e);
            }

            return result;
        }

        return null;
    }

    static PhotoEntry ParsePhotoFromJson(JsonElement obj)
    {
        if(!obj.TryGetProperty("guid", out JsonElement guidEl) || guidEl.ValueKind != JsonValueKind.String) return null;
        if(!Guid.TryParse(guidEl.GetString(), out Guid guid)) return null;

        string extension = obj.TryGetProperty("extension", out JsonElement extEl) &&
                           extEl.ValueKind == JsonValueKind.String
                               ? extEl.GetString()
                               : null;

        string comment = obj.TryGetProperty("comment", out JsonElement cmtEl) &&
                         cmtEl.ValueKind == JsonValueKind.String
                             ? cmtEl.GetString()
                             : null;

        return new PhotoEntry { Guid = guid, Extension = extension, Comment = comment };
    }

    static PhotoEntry ParsePhotoFromObject(object item)
    {
        if(item is null) return null;

        if(item is JsonElement je) return ParsePhotoFromJson(je);

        if(item is IDictionary<string, object> dict)
        {
            if(!dict.TryGetValue("guid", out object g) || g is null) return null;
            if(!Guid.TryParse(g.ToString(), out Guid guid)) return null;

            string extension = dict.TryGetValue("extension", out object x) ? CoerceString(x) : null;
            string comment   = dict.TryGetValue("comment", out object c) ? CoerceString(c) : null;
            return new PhotoEntry { Guid = guid, Extension = extension, Comment = comment };
        }

        return null;
    }

    static bool TryCoerceInt(object value, out int result)
    {
        result = 0;

        switch(value)
        {
            case null: return false;
            case int i:
                result = i;
                return true;
            case long l when l is >= int.MinValue and <= int.MaxValue:
                result = (int)l;
                return true;
            case JsonElement je:
                switch(je.ValueKind)
                {
                    case JsonValueKind.Number:
                        if(je.TryGetInt32(out int n))
                        {
                            result = n;
                            return true;
                        }

                        return false;
                    case JsonValueKind.String:
                        return int.TryParse(je.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                                            out result);
                    default: return false;
                }
            case string s:
                return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
            default: return false;
        }
    }

    static string CoerceString(object value)
    {
        return value switch
        {
            null               => null,
            string s           => s,
            JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
            JsonElement je when je.ValueKind == JsonValueKind.Null   => null,
            _                  => value.ToString()
        };
    }

    static bool LooksLikeUrl(string s) => Uri.TryCreate(s, UriKind.Absolute, out Uri u) &&
                                          (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
}
