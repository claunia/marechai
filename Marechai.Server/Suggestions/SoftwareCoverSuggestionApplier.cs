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
using Marechai.Server.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Suggestions;

/// <summary>
///     Applier for collaborative Software cover batch-upload suggestions
///     (<see cref="SuggestionEntityType.SoftwareCover" />).
///     <para>
///         A single suggestion row carries a batch of 1-30 pending cover images for the same
///         parent <see cref="SoftwareRelease" />. Unlike SoftwarePromoArt this entity has NO
///         suggestion-level field at all (no group_name, no license_id, no source_url) — every
///         per-image descriptor stands on its own. Each image carries a MANDATORY
///         <c>type</c> (<see cref="SoftwareCoverType" /> byte enum, picked from a fixed
///         dropdown — no free input) and an optional <c>caption</c> (max 500 chars).
///     </para>
///     <para>
///         The dialog stages each image via
///         <c>POST /software/covers/pending?releaseId={ulong}</c> first (writes file +
///         sidecar to disk under <c>photos/software-covers/pending/</c>) then submits ONE
///         <c>/suggestions</c> POST referencing the per-image guids.
///     </para>
///     <para>
///         At admin review time, the admin per-image accepts/rejects via the field-name keys
///         <c>cover.&lt;guid&gt;</c>. Accepted images are promoted to
///         <c>photos/software-covers/originals/</c> and a <see cref="SoftwareCover" /> row is
///         created where the row Id IS the pending guid (matching the admin upload convention
///         where <c>model.Id</c> IS the original-file basename). The same fire-and-forget
///         <c>Photos.ConversionWorker</c> used by the admin upload path materialises all
///         JPEG/WebP/AVIF variants. Rejected images have their pending files deleted.
///     </para>
/// </summary>
public static class SoftwareCoverSuggestionApplier
{
    /// <summary>Wire field name carrying the JSON array of per-image descriptors.</summary>
    public const string FieldPhotos = "photos";

    /// <summary>Hard upper bound on the number of images a single batch may carry.</summary>
    public const int MaxPhotosPerBatch = 30;

    /// <summary>Maximum length for the per-image caption.</summary>
    public const int MaxCaptionLength = 500;

    /// <summary>Allowed lower-case file extensions (matching the pending upload endpoint).</summary>
    static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "jpg", "jpeg", "png", "webp"
    };

    /// <summary>
    ///     Build the field-name key the admin SuggestionDiffPanel emits when accepting a
    ///     specific cover image (e.g. <c>cover.123e4567-e89b-12d3-a456-426614174000</c>).
    /// </summary>
    public static string CoverAcceptKey(Guid guid) =>
        "cover." + guid.ToString("D", CultureInfo.InvariantCulture);

    /// <summary>True when the field name is one we recognise on the wire.</summary>
    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;

        if(fieldName == FieldPhotos) return true;

        // cover.<guid> — per-image accept toggle
        if(fieldName.StartsWith("cover.", StringComparison.Ordinal))
        {
            string rest = fieldName.Substring("cover.".Length);
            return Guid.TryParse(rest, out _);
        }

        return false;
    }

    /// <summary>
    ///     "Current values" for a SoftwareCover batch suggestion. There are no comparable
    ///     existing values to diff against — the suggestion is a brand-new batch of images
    ///     the admin either accepts (creates new SoftwareCover rows) or rejects. The diff
    ///     endpoint still calls this to detect whether the parent entity has been deleted:
    ///     returning a non-null (possibly empty) dictionary signals "parent SoftwareRelease
    ///     exists"; returning null signals "parent missing — close the suggestion as stale".
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));

        ulong releaseId = (ulong)entityId;
        bool exists     = await context.SoftwareReleases.AsNoTracking().AnyAsync(r => r.Id == releaseId);
        return exists ? new Dictionary<string, object>(StringComparer.Ordinal) : null;
    }

    /// <summary>
    ///     Validate the submitted payload before the suggestion row is persisted. Verifies the
    ///     parent SoftwareRelease exists, the photos array is well-formed (1-30 entries), and
    ///     EVERY referenced pending sidecar exists, belongs to the caller, and is scoped to
    ///     the parent SoftwareRelease. Per-image: <c>guid</c> parseable, <c>extension</c> in
    ///     {jpg,jpeg,png,webp}, <c>type</c> present and <see cref="Enum.IsDefined(Type,object)" />
    ///     for <see cref="SoftwareCoverType" />, optional <c>caption</c> ≤500 chars. Returns
    ///     <c>(true, null)</c> on success, <c>(false, errorDetail)</c> otherwise.
    /// </summary>
    public static async Task<(bool ok, string error)> ValidateAsync(MarechaiContext context, long? entityId,
                                                                    Dictionary<string, object> suggested,
                                                                    string assetRootPath, string uploaderUserId)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));
        if(suggested is null) return (false, "Missing suggestion payload.");

        if(!entityId.HasValue || entityId.Value <= 0)
            return (false, "Software cover suggestions must reference an existing software release via entity_id.");

        ulong releaseId = (ulong)entityId.Value;
        bool releaseExists = await context.SoftwareReleases.AsNoTracking().AnyAsync(r => r.Id == releaseId);
        if(!releaseExists) return (false, $"Software release #{releaseId} not found.");

        // ── Photos array (mandatory) ─────────────────────────────────────────────
        if(!suggested.TryGetValue(FieldPhotos, out object photosRaw) || photosRaw is null)
            return (false, "A software cover suggestion must include a non-empty 'photos' array.");

        List<CoverEntry> entries = ParsePhotosArray(photosRaw);
        if(entries is null) return (false, "'photos' must be a JSON array of objects.");
        if(entries.Count == 0) return (false, "A software cover suggestion must include at least one image.");

        if(entries.Count > MaxPhotosPerBatch)
            return (false, $"A software cover suggestion cannot include more than {MaxPhotosPerBatch} images.");

        // ── Per-image validation ─────────────────────────────────────────────────
        var seenGuids = new HashSet<Guid>();

        for(int i = 0; i < entries.Count; i++)
        {
            CoverEntry e = entries[i];

            if(e.Guid == Guid.Empty) return (false, $"Image #{i + 1}: missing or invalid 'guid'.");

            if(!seenGuids.Add(e.Guid))
                return (false, $"Image #{i + 1}: duplicate guid {e.Guid:D}.");

            if(string.IsNullOrEmpty(e.Extension) ||
               !_allowedExtensions.Contains(e.Extension.TrimStart('.')))
                return (false,
                        $"Image #{i + 1}: unsupported extension '{e.Extension}'. Accepted: jpg, jpeg, png, webp.");

            if(!e.Type.HasValue)
                return (false, $"Image #{i + 1}: missing 'type'. Each cover must specify a type.");

            if(!Enum.IsDefined(typeof(SoftwareCoverType), e.Type.Value))
                return (false, $"Image #{i + 1}: invalid 'type' value {e.Type.Value}.");

            if(!string.IsNullOrEmpty(e.Caption) && e.Caption.Length > MaxCaptionLength)
                return (false, $"Image #{i + 1}: caption cannot exceed {MaxCaptionLength} characters.");

            // Sidecar must exist, belong to the uploader, be scoped to this release.
            PendingImageStore.PendingMetadata meta =
                await PendingImageStore.GetMetadataAsync(assetRootPath, "software-covers", e.Guid);

            if(meta is null) return (false, $"Image #{i + 1}: pending upload {e.Guid:D} not found.");

            if(meta.EntityType != (byte)SuggestionEntityType.SoftwareCover)
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} is not a software cover image.");

            if(!string.Equals(meta.UploadedById, uploaderUserId, StringComparison.Ordinal))
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} belongs to a different user.");

            if(meta.ParentEntityId != entityId.Value)
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} is scoped to a different release.");
        }

        return (true, null);
    }

    /// <summary>
    ///     Apply an admin's accept/reject decisions. For each image in the suggested array:
    ///     if its <c>cover.&lt;guid&gt;</c> key is in <paramref name="accepted" />, promote
    ///     the pending file to <c>originals/</c>, create a <see cref="SoftwareCover" /> row
    ///     where the Id IS the pending guid, and fire-and-forget the conversion worker.
    ///     Otherwise, delete the pending file. Returns the set of accept-keys actually applied
    ///     (subset of <paramref name="accepted" />) plus a flag indicating the parent
    ///     SoftwareRelease was missing (so the controller can mark the suggestion stale).
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(MarechaiContext context,
        long entityId, Dictionary<string, object> suggested, HashSet<string> accepted, string creditedUserId,
        string assetRootPath)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);
        if(suggested is null) return (applied, false);

        ulong releaseId = (ulong)entityId;
        bool releaseExists = await context.SoftwareReleases.AnyAsync(r => r.Id == releaseId);
        if(!releaseExists) return (applied, true);

        if(!suggested.TryGetValue(FieldPhotos, out object photosRaw) || photosRaw is null) return (applied, false);

        List<CoverEntry> entries = ParsePhotosArray(photosRaw);
        if(entries is null) return (applied, false);

        foreach(CoverEntry e in entries)
        {
            if(e.Guid == Guid.Empty) continue;

            string acceptKey = CoverAcceptKey(e.Guid);

            if(accepted.Contains(acceptKey) && e.Type.HasValue &&
               Enum.IsDefined(typeof(SoftwareCoverType), e.Type.Value))
            {
                bool ok = await PromoteAndPersistAsync(context, releaseId, creditedUserId, assetRootPath, e);

                if(ok) applied.Add(acceptKey);
            }
            else
            {
                // Rejected (or admin left it unchecked, or somehow missing the type field):
                // clean up the pending file.
                try { PendingImageStore.Delete(assetRootPath, "software-covers", e.Guid); } catch { /* best-effort */ }
            }
        }

        return (applied, false);
    }

    static async Task<bool> PromoteAndPersistAsync(MarechaiContext context, ulong releaseId, string creditedUserId,
                                                    string assetRootPath, CoverEntry entry)
    {
        try
        {
            (string movedPath, string ext) =
                await PendingImageStore.PromoteToOriginalsAsync(assetRootPath, "software-covers", entry.Guid);

            if(movedPath is null || string.IsNullOrEmpty(ext)) return false;

            var cover = new SoftwareCover
            {
                Id                = entry.Guid,
                SoftwareReleaseId = releaseId,
                Type              = (SoftwareCoverType)entry.Type!.Value,
                Caption           = string.IsNullOrEmpty(entry.Caption) ? null : entry.Caption,
                OriginalExtension = ext.TrimStart('.')
            };

            await context.SoftwareCovers.AddAsync(cover);
            await context.SaveChangesWithUserAsync(creditedUserId);

            // Fire-and-forget conversion worker — same pattern as admin
            // SoftwareCoversController.UploadAsync.
            string sourceFormat = ext.TrimStart('.');
            Guid   newId        = cover.Id;
            string capturedPath = movedPath;
            string capturedRoot = assetRootPath;

            _ = Task.Run(() =>
            {
                try
                {
                    Marechai.Helpers.Photos.EnsureCreated(capturedRoot, false, "software-covers");
                    var photos = new Marechai.Helpers.Photos();
                    photos.ConversionWorker(capturedRoot, newId, capturedPath, sourceFormat, false, "software-covers");
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

    // ───────────────────────────── helpers ─────────────────────────────

    sealed class CoverEntry
    {
        public Guid   Guid      { get; init; }
        public string Extension { get; init; }
        public byte?  Type      { get; init; }
        public string Caption   { get; init; }
    }

    static List<CoverEntry> ParsePhotosArray(object raw)
    {
        var result = new List<CoverEntry>();

        if(raw is JsonElement je)
        {
            if(je.ValueKind != JsonValueKind.Array) return null;

            foreach(JsonElement item in je.EnumerateArray())
            {
                if(item.ValueKind != JsonValueKind.Object) return null;
                CoverEntry e = ParseCoverFromJson(item);
                if(e is null) return null;
                result.Add(e);
            }

            return result;
        }

        if(raw is System.Collections.IEnumerable enumerable)
        {
            foreach(object item in enumerable)
            {
                CoverEntry e = ParseCoverFromObject(item);
                if(e is null) return null;
                result.Add(e);
            }

            return result;
        }

        return null;
    }

    static CoverEntry ParseCoverFromJson(JsonElement obj)
    {
        if(!obj.TryGetProperty("guid", out JsonElement guidEl) || guidEl.ValueKind != JsonValueKind.String) return null;
        if(!Guid.TryParse(guidEl.GetString(), out Guid guid)) return null;

        string extension = obj.TryGetProperty("extension", out JsonElement extEl) &&
                           extEl.ValueKind == JsonValueKind.String
                               ? extEl.GetString()
                               : null;

        byte? type = null;

        if(obj.TryGetProperty("type", out JsonElement typeEl))
        {
            type = typeEl.ValueKind switch
            {
                JsonValueKind.Number when typeEl.TryGetByte(out byte b) => b,
                JsonValueKind.String when byte.TryParse(typeEl.GetString(), NumberStyles.Integer,
                                                         CultureInfo.InvariantCulture, out byte sb) => sb,
                _ => null
            };
        }

        string caption = obj.TryGetProperty("caption", out JsonElement capEl) &&
                         capEl.ValueKind == JsonValueKind.String
                             ? capEl.GetString()
                             : null;

        return new CoverEntry { Guid = guid, Extension = extension, Type = type, Caption = caption };
    }

    static CoverEntry ParseCoverFromObject(object item)
    {
        if(item is null) return null;

        if(item is JsonElement je) return ParseCoverFromJson(je);

        if(item is IDictionary<string, object> dict)
        {
            if(!dict.TryGetValue("guid", out object g) || g is null) return null;
            if(!Guid.TryParse(g.ToString(), out Guid guid)) return null;

            string extension = dict.TryGetValue("extension", out object x) ? CoerceString(x) : null;
            string caption   = dict.TryGetValue("caption", out object c) ? CoerceString(c) : null;

            byte? type = null;

            if(dict.TryGetValue("type", out object t) && t is not null)
            {
                if(t is byte tb) type                                                                  = tb;
                else if(t is int ti && ti >= 0 && ti <= 255) type                                      = (byte)ti;
                else if(t is long tl && tl >= 0 && tl <= 255) type                                     = (byte)tl;
                else if(byte.TryParse(t.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                                       out byte tp)) type = tp;
            }

            return new CoverEntry { Guid = guid, Extension = extension, Type = type, Caption = caption };
        }

        return null;
    }

    static string CoerceString(object value)
    {
        return value switch
        {
            null                                                     => null,
            string s                                                 => s,
            JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
            JsonElement je when je.ValueKind == JsonValueKind.Null   => null,
            _                                                        => value.ToString()
        };
    }

    /// <summary>
    ///     Total image count carried by the suggestion's <c>photos</c> array, used by the
    ///     SuggestionsController status-determination logic to decide between Accepted /
    ///     PartiallyAccepted / Rejected based on how many cover accept-keys were checked.
    /// </summary>
    public static int CountSuggestedItems(Dictionary<string, object> suggested)
    {
        if(suggested is null) return 0;
        if(!suggested.TryGetValue(FieldPhotos, out object raw) || raw is null) return 0;
        List<CoverEntry> entries = ParsePhotosArray(raw);
        return entries?.Count ?? 0;
    }
}
