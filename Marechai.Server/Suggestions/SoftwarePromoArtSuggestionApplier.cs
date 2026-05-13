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
///     Applier for collaborative Software promo art batch-upload suggestions
///     (<see cref="SuggestionEntityType.SoftwarePromoArt" />).
///     <para>
///         A single suggestion row carries a batch of 1-30 pending images for the same parent
///         Software, plus a suggestion-level <c>group_name</c> (free-text; the server does
///         get-or-create on <see cref="SoftwarePromoArtGroup" /> at accept time, mirroring
///         the admin upload flow). The dialog stages each image via
///         <c>POST /software/promo-art/pending?softwareId={id}</c> first (writes file +
///         sidecar to disk under <c>photos/software-promo-art/pending/</c>) then submits
///         ONE <c>/suggestions</c> POST referencing the per-image guids.
///     </para>
///     <para>
///         At admin review time, the admin per-image accepts/rejects via the field-name keys
///         <c>promo.&lt;guid&gt;</c>. Accepted images are promoted to
///         <c>photos/software-promo-art/originals/</c> and a <see cref="SoftwarePromoArt" />
///         row is created where the row Id IS the pending guid (matching the admin upload
///         convention where <c>model.Id</c> IS the original-file basename). The same
///         fire-and-forget <c>Photos.ConversionWorker</c> used by the admin upload path
///         materialises all JPEG/WebP variants. Rejected images have their pending files
///         deleted.
///     </para>
///     <para>
///         Unlike the GPU/Processor/SoundSynth/Machine photo ports, this entity has NO
///         <c>license_id</c> and NO <c>source_url</c> because the underlying
///         <see cref="SoftwarePromoArt" /> table has no such columns.
///     </para>
/// </summary>
public static class SoftwarePromoArtSuggestionApplier
{
    /// <summary>
    ///     Wire field name carrying the suggestion-level group name (free-text; server does
    ///     get-or-create on <see cref="SoftwarePromoArtGroup" /> on accept).
    /// </summary>
    public const string FieldGroupName = "group_name";

    /// <summary>Wire field name carrying the JSON array of per-image descriptors.</summary>
    public const string FieldPhotos = "photos";

    /// <summary>Hard upper bound on the number of images a single batch may carry.</summary>
    public const int MaxPhotosPerBatch = 30;

    /// <summary>Maximum length for the suggestion-level group name (matches DB column).</summary>
    public const int MaxGroupNameLength = 256;

    /// <summary>Maximum length for the per-image caption.</summary>
    public const int MaxCaptionLength = 500;

    /// <summary>Allowed lower-case file extensions (matching the pending upload endpoint).</summary>
    static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "jpg", "jpeg", "png", "webp"
    };

    /// <summary>
    ///     Build the field-name key the admin SuggestionDiffPanel emits when accepting a
    ///     specific promo art image (e.g.
    ///     <c>promo.123e4567-e89b-12d3-a456-426614174000</c>).
    /// </summary>
    public static string PromoAcceptKey(Guid guid) =>
        "promo." + guid.ToString("D", CultureInfo.InvariantCulture);

    /// <summary>True when the field name is one we recognise on the wire.</summary>
    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;

        if(fieldName == FieldGroupName || fieldName == FieldPhotos) return true;

        // promo.<guid> — per-image accept toggle
        if(fieldName.StartsWith("promo.", StringComparison.Ordinal))
        {
            string rest = fieldName.Substring("promo.".Length);
            return Guid.TryParse(rest, out _);
        }

        return false;
    }

    /// <summary>
    ///     "Current values" for a SoftwarePromoArt batch suggestion. There are no comparable
    ///     existing values to diff against — the suggestion is a brand-new batch of images
    ///     the admin either accepts (creates new SoftwarePromoArt rows) or rejects. The diff
    ///     endpoint still calls this to detect whether the parent entity has been deleted:
    ///     returning a non-null (possibly empty) dictionary signals "parent Software exists";
    ///     returning null signals "parent missing — close the suggestion as stale".
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));

        ulong softwareId = (ulong)entityId;
        bool exists      = await context.Softwares.AsNoTracking().AnyAsync(s => s.Id == softwareId);
        return exists ? new Dictionary<string, object>(StringComparer.Ordinal) : null;
    }

    /// <summary>
    ///     Validate the submitted payload before the suggestion row is persisted. Verifies the
    ///     parent Software exists, the group_name is non-empty and within length limits, the
    ///     photos array is well-formed (1-30 entries), and EVERY referenced pending sidecar
    ///     exists and belongs to the caller AND is scoped to the parent Software.
    ///     Returns <c>(true, null)</c> on success, <c>(false, errorDetail)</c> otherwise.
    /// </summary>
    public static async Task<(bool ok, string error)> ValidateAsync(MarechaiContext context, long? entityId,
                                                                    Dictionary<string, object> suggested,
                                                                    string assetRootPath, string uploaderUserId)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));
        if(suggested is null) return (false, "Missing suggestion payload.");

        if(!entityId.HasValue || entityId.Value <= 0)
            return (false, "Software promo art suggestions must reference an existing software via entity_id.");

        ulong softwareId = (ulong)entityId.Value;
        bool softwareExists = await context.Softwares.AsNoTracking().AnyAsync(s => s.Id == softwareId);
        if(!softwareExists) return (false, $"Software #{softwareId} not found.");

        // ── Group name (suggestion-level, mandatory) ─────────────────────────────
        if(!suggested.TryGetValue(FieldGroupName, out object groupRaw) || groupRaw is null)
            return (false, "A software promo art suggestion must include a 'group_name' field.");

        string groupName = CoerceString(groupRaw)?.Trim();

        if(string.IsNullOrEmpty(groupName))
            return (false, "'group_name' cannot be empty.");

        if(groupName.Length > MaxGroupNameLength)
            return (false, $"'group_name' cannot exceed {MaxGroupNameLength} characters.");

        // ── Photos array (mandatory) ─────────────────────────────────────────────
        if(!suggested.TryGetValue(FieldPhotos, out object photosRaw) || photosRaw is null)
            return (false, "A software promo art suggestion must include a non-empty 'photos' array.");

        List<PromoEntry> entries = ParsePhotosArray(photosRaw);
        if(entries is null) return (false, "'photos' must be a JSON array of objects.");
        if(entries.Count == 0) return (false, "A software promo art suggestion must include at least one image.");

        if(entries.Count > MaxPhotosPerBatch)
            return (false, $"A software promo art suggestion cannot include more than {MaxPhotosPerBatch} images.");

        // ── Per-image validation ─────────────────────────────────────────────────
        var seenGuids = new HashSet<Guid>();

        for(int i = 0; i < entries.Count; i++)
        {
            PromoEntry e = entries[i];

            if(e.Guid == Guid.Empty) return (false, $"Image #{i + 1}: missing or invalid 'guid'.");

            if(!seenGuids.Add(e.Guid))
                return (false, $"Image #{i + 1}: duplicate guid {e.Guid:D}.");

            if(string.IsNullOrEmpty(e.Extension) ||
               !_allowedExtensions.Contains(e.Extension.TrimStart('.')))
                return (false,
                        $"Image #{i + 1}: unsupported extension '{e.Extension}'. Accepted: jpg, jpeg, png, webp.");

            if(!string.IsNullOrEmpty(e.Caption) && e.Caption.Length > MaxCaptionLength)
                return (false, $"Image #{i + 1}: caption cannot exceed {MaxCaptionLength} characters.");

            // Sidecar must exist, belong to the uploader, be scoped to this Software.
            PendingImageStore.PendingMetadata meta =
                await PendingImageStore.GetMetadataAsync(assetRootPath, "software-promo-art", e.Guid);

            if(meta is null) return (false, $"Image #{i + 1}: pending upload {e.Guid:D} not found.");

            if(meta.EntityType != (byte)SuggestionEntityType.SoftwarePromoArt)
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} is not a software promo art image.");

            if(!string.Equals(meta.UploadedById, uploaderUserId, StringComparison.Ordinal))
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} belongs to a different user.");

            if(meta.ParentEntityId != entityId.Value)
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} is scoped to a different software.");
        }

        return (true, null);
    }

    /// <summary>
    ///     Apply an admin's accept/reject decisions. For each image in the suggested array:
    ///     if its <c>promo.&lt;guid&gt;</c> key is in <paramref name="accepted" />, promote
    ///     the pending file to <c>originals/</c>, create a <see cref="SoftwarePromoArt" /> row
    ///     where the Id IS the pending guid, and fire-and-forget the conversion worker.
    ///     Otherwise, delete the pending file. The suggestion-level <c>group_name</c> is
    ///     resolved via get-or-create on <see cref="SoftwarePromoArtGroup" />. Returns the set
    ///     of accept-keys actually applied (subset of <paramref name="accepted" />) plus a
    ///     flag indicating the parent Software was missing (so the controller can mark the
    ///     suggestion stale).
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(MarechaiContext context,
        long entityId, Dictionary<string, object> suggested, HashSet<string> accepted, string creditedUserId,
        string assetRootPath)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);
        if(suggested is null) return (applied, false);

        ulong softwareId = (ulong)entityId;
        bool softwareExists = await context.Softwares.AnyAsync(s => s.Id == softwareId);
        if(!softwareExists) return (applied, true);

        if(!suggested.TryGetValue(FieldGroupName, out object groupRaw) || groupRaw is null) return (applied, false);
        string groupName = CoerceString(groupRaw)?.Trim();
        if(string.IsNullOrEmpty(groupName) || groupName.Length > MaxGroupNameLength) return (applied, false);

        if(!suggested.TryGetValue(FieldPhotos, out object photosRaw) || photosRaw is null) return (applied, false);

        List<PromoEntry> entries = ParsePhotosArray(photosRaw);
        if(entries is null) return (applied, false);

        // Determine which entries are accepted vs rejected up front so we can avoid creating
        // a brand-new group when the admin rejects every photo in the batch (would leave an
        // orphan group row).
        bool anyAccepted = entries.Any(e => e.Guid != Guid.Empty &&
                                            accepted.Contains(PromoAcceptKey(e.Guid)));

        SoftwarePromoArtGroup group = null;

        if(anyAccepted)
        {
            group = await context.SoftwarePromoArtGroups.FirstOrDefaultAsync(g => g.Name == groupName);

            if(group is null)
            {
                group = new SoftwarePromoArtGroup { Name = groupName };
                await context.SoftwarePromoArtGroups.AddAsync(group);
                await context.SaveChangesWithUserAsync(creditedUserId);
            }
        }

        foreach(PromoEntry e in entries)
        {
            if(e.Guid == Guid.Empty) continue;

            string acceptKey = PromoAcceptKey(e.Guid);

            if(accepted.Contains(acceptKey) && group is not null)
            {
                bool ok = await PromoteAndPersistAsync(context, softwareId, group.Id, creditedUserId, assetRootPath, e);

                if(ok) applied.Add(acceptKey);
            }
            else
            {
                // Rejected (or admin left it unchecked): clean up the pending file.
                try { PendingImageStore.Delete(assetRootPath, "software-promo-art", e.Guid); } catch { /* best-effort */ }
            }
        }

        return (applied, false);
    }

    static async Task<bool> PromoteAndPersistAsync(MarechaiContext context, ulong softwareId, int groupId,
                                                    string creditedUserId, string assetRootPath, PromoEntry entry)
    {
        try
        {
            (string movedPath, string ext) =
                await PendingImageStore.PromoteToOriginalsAsync(assetRootPath, "software-promo-art", entry.Guid);

            if(movedPath is null || string.IsNullOrEmpty(ext)) return false;

            var promo = new SoftwarePromoArt
            {
                Id                = entry.Guid,
                SoftwareId        = softwareId,
                GroupId           = groupId,
                Caption           = string.IsNullOrEmpty(entry.Caption) ? null : entry.Caption,
                OriginalExtension = ext.TrimStart('.')
            };

            await context.SoftwarePromoArt.AddAsync(promo);
            await context.SaveChangesWithUserAsync(creditedUserId);

            // Fire-and-forget conversion worker — same pattern as admin
            // SoftwarePromoArtController.UploadAsync.
            string sourceFormat = ext.TrimStart('.');
            Guid   newId        = promo.Id;
            string capturedPath = movedPath;
            string capturedRoot = assetRootPath;

            _ = Task.Run(() =>
            {
                try
                {
                    Marechai.Helpers.Photos.EnsureCreated(capturedRoot, false, "software-promo-art");
                    var photos = new Marechai.Helpers.Photos();
                    photos.ConversionWorker(capturedRoot, newId, capturedPath, sourceFormat, false,
                                            "software-promo-art");
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

    sealed class PromoEntry
    {
        public Guid   Guid      { get; init; }
        public string Extension { get; init; }
        public string Caption   { get; init; }
    }

    static List<PromoEntry> ParsePhotosArray(object raw)
    {
        var result = new List<PromoEntry>();

        if(raw is JsonElement je)
        {
            if(je.ValueKind != JsonValueKind.Array) return null;

            foreach(JsonElement item in je.EnumerateArray())
            {
                if(item.ValueKind != JsonValueKind.Object) return null;
                PromoEntry e = ParsePromoFromJson(item);
                if(e is null) return null;
                result.Add(e);
            }

            return result;
        }

        if(raw is System.Collections.IEnumerable enumerable)
        {
            foreach(object item in enumerable)
            {
                PromoEntry e = ParsePromoFromObject(item);
                if(e is null) return null;
                result.Add(e);
            }

            return result;
        }

        return null;
    }

    static PromoEntry ParsePromoFromJson(JsonElement obj)
    {
        if(!obj.TryGetProperty("guid", out JsonElement guidEl) || guidEl.ValueKind != JsonValueKind.String) return null;
        if(!Guid.TryParse(guidEl.GetString(), out Guid guid)) return null;

        string extension = obj.TryGetProperty("extension", out JsonElement extEl) &&
                           extEl.ValueKind == JsonValueKind.String
                               ? extEl.GetString()
                               : null;

        string caption = obj.TryGetProperty("caption", out JsonElement capEl) &&
                         capEl.ValueKind == JsonValueKind.String
                             ? capEl.GetString()
                             : null;

        return new PromoEntry { Guid = guid, Extension = extension, Caption = caption };
    }

    static PromoEntry ParsePromoFromObject(object item)
    {
        if(item is null) return null;

        if(item is JsonElement je) return ParsePromoFromJson(je);

        if(item is IDictionary<string, object> dict)
        {
            if(!dict.TryGetValue("guid", out object g) || g is null) return null;
            if(!Guid.TryParse(g.ToString(), out Guid guid)) return null;

            string extension = dict.TryGetValue("extension", out object x) ? CoerceString(x) : null;
            string caption   = dict.TryGetValue("caption", out object c) ? CoerceString(c) : null;
            return new PromoEntry { Guid = guid, Extension = extension, Caption = caption };
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
    ///     PartiallyAccepted / Rejected based on how many photo accept-keys were checked.
    /// </summary>
    public static int CountSuggestedItems(Dictionary<string, object> suggested)
    {
        if(suggested is null) return 0;
        if(!suggested.TryGetValue(FieldPhotos, out object raw) || raw is null) return 0;
        List<PromoEntry> entries = ParsePhotosArray(raw);
        return entries?.Count ?? 0;
    }
}
