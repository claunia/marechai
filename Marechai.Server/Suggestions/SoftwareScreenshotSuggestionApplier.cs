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
///     Applier for collaborative Software screenshot batch-upload suggestions
///     (<see cref="SuggestionEntityType.SoftwareScreenshot" />).
///     <para>
///         A single suggestion row carries a batch of 1-50 pending images for the same parent
///         Software, plus a mandatory suggestion-level <c>platform_id</c> (FK to
///         <see cref="SoftwarePlatform" />; applied to every accepted screenshot uniformly).
///         The dialog stages each image via
///         <c>POST /software/screenshots/pending?softwareId={id}</c> first (writes file +
///         sidecar to disk under <c>photos/software-screenshots/pending/</c>) then submits
///         ONE <c>/suggestions</c> POST referencing the per-image guids.
///     </para>
///     <para>
///         At admin review time, the admin per-image accepts/rejects via the field-name keys
///         <c>screenshot.&lt;guid&gt;</c>. Accepted images are promoted to
///         <c>photos/software-screenshots/originals/</c> and a
///         <see cref="SoftwareScreenshot" /> row is created where the row Id IS the pending
///         guid (matching the admin upload convention where <c>model.Id</c> IS the
///         original-file basename). The same fire-and-forget <c>Photos.ConversionWorker</c>
///         used by the admin upload path materialises all JPEG/WebP/AVIF/JXL variants.
///         Rejected images have their pending files deleted.
///     </para>
///     <para>
///         <c>SoftwareVersionId</c> is left null on every accepted row — the collaborative
///         path intentionally simplifies vs the admin upload flow which allows a per-row
///         version. Allowed extensions are jpg/jpeg/png/webp ONLY (narrower than the admin
///         upload allow-set).
///     </para>
/// </summary>
public static class SoftwareScreenshotSuggestionApplier
{
    /// <summary>
    ///     Wire field name carrying the suggestion-level platform id (mandatory FK to
    ///     <see cref="SoftwarePlatform" />). Applied to every accepted screenshot.
    /// </summary>
    public const string FieldPlatformId = "platform_id";

    /// <summary>Wire field name carrying the JSON array of per-image descriptors.</summary>
    public const string FieldPhotos = "photos";

    /// <summary>Hard upper bound on the number of images a single batch may carry.</summary>
    public const int MaxPhotosPerBatch = 50;

    /// <summary>Maximum length for the per-image caption.</summary>
    public const int MaxCaptionLength = 500;

    /// <summary>
    ///     Maximum length for the per-image group name (matches the
    ///     <c>SoftwareScreenshotGroups.Name</c> column width).
    /// </summary>
    public const int MaxGroupNameLength = 256;

    /// <summary>Allowed lower-case file extensions (matching the pending upload endpoint).</summary>
    static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "jpg", "jpeg", "png", "webp"
    };

    /// <summary>
    ///     Build the field-name key the admin SuggestionDiffPanel emits when accepting a
    ///     specific screenshot image (e.g.
    ///     <c>screenshot.123e4567-e89b-12d3-a456-426614174000</c>).
    /// </summary>
    public static string ScreenshotAcceptKey(Guid guid) =>
        "screenshot." + guid.ToString("D", CultureInfo.InvariantCulture);

    /// <summary>True when the field name is one we recognise on the wire.</summary>
    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;

        if(fieldName == FieldPlatformId || fieldName == FieldPhotos) return true;

        // screenshot.<guid> — per-image accept toggle
        if(fieldName.StartsWith("screenshot.", StringComparison.Ordinal))
        {
            string rest = fieldName.Substring("screenshot.".Length);
            return Guid.TryParse(rest, out _);
        }

        return false;
    }

    /// <summary>
    ///     "Current values" for a SoftwareScreenshot batch suggestion. There are no comparable
    ///     existing values to diff against — the suggestion is a brand-new batch of images
    ///     the admin either accepts (creates new SoftwareScreenshot rows) or rejects. The
    ///     diff endpoint still calls this to detect whether the parent entity has been
    ///     deleted: returning a non-null (possibly empty) dictionary signals "parent Software
    ///     exists"; returning null signals "parent missing — close the suggestion as stale".
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
    ///     parent Software exists, the platform_id is set and references an existing
    ///     <see cref="SoftwarePlatform" />, the photos array is well-formed (1-50 entries),
    ///     and EVERY referenced pending sidecar exists, belongs to the caller AND is scoped
    ///     to the parent Software. Returns <c>(true, null)</c> on success,
    ///     <c>(false, errorDetail)</c> otherwise.
    /// </summary>
    public static async Task<(bool ok, string error)> ValidateAsync(MarechaiContext context, long? entityId,
                                                                    Dictionary<string, object> suggested,
                                                                    string assetRootPath, string uploaderUserId)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));
        if(suggested is null) return (false, "Missing suggestion payload.");

        if(!entityId.HasValue || entityId.Value <= 0)
            return (false, "Software screenshot suggestions must reference an existing software via entity_id.");

        ulong softwareId = (ulong)entityId.Value;
        bool softwareExists = await context.Softwares.AsNoTracking().AnyAsync(s => s.Id == softwareId);
        if(!softwareExists) return (false, $"Software #{softwareId} not found.");

        // ── Platform id (suggestion-level, mandatory) ────────────────────────────
        if(!suggested.TryGetValue(FieldPlatformId, out object platformRaw) || platformRaw is null)
            return (false, "A software screenshot suggestion must include a 'platform_id' field.");

        if(!TryCoerceUlong(platformRaw, out ulong platformId) || platformId == 0)
            return (false, "'platform_id' must be a positive integer.");

        bool platformExists = await context.SoftwarePlatforms.AsNoTracking().AnyAsync(p => p.Id == platformId);

        if(!platformExists)
            return (false, $"Software platform #{platformId} not found.");

        // ── Photos array (mandatory) ─────────────────────────────────────────────
        if(!suggested.TryGetValue(FieldPhotos, out object photosRaw) || photosRaw is null)
            return (false, "A software screenshot suggestion must include a non-empty 'photos' array.");

        List<ScreenshotEntry> entries = ParsePhotosArray(photosRaw);
        if(entries is null) return (false, "'photos' must be a JSON array of objects.");
        if(entries.Count == 0) return (false, "A software screenshot suggestion must include at least one image.");

        if(entries.Count > MaxPhotosPerBatch)
            return (false, $"A software screenshot suggestion cannot include more than {MaxPhotosPerBatch} images.");

        // ── Per-image validation ─────────────────────────────────────────────────
        var seenGuids = new HashSet<Guid>();

        for(int i = 0; i < entries.Count; i++)
        {
            ScreenshotEntry e = entries[i];

            if(e.Guid == Guid.Empty) return (false, $"Image #{i + 1}: missing or invalid 'guid'.");

            if(!seenGuids.Add(e.Guid))
                return (false, $"Image #{i + 1}: duplicate guid {e.Guid:D}.");

            if(string.IsNullOrEmpty(e.Extension) ||
               !_allowedExtensions.Contains(e.Extension.TrimStart('.')))
                return (false,
                        $"Image #{i + 1}: unsupported extension '{e.Extension}'. Accepted: jpg, jpeg, png, webp.");

            if(!string.IsNullOrEmpty(e.Caption) && e.Caption.Length > MaxCaptionLength)
                return (false, $"Image #{i + 1}: caption cannot exceed {MaxCaptionLength} characters.");

            if(!string.IsNullOrEmpty(e.GroupName) && e.GroupName.Length > MaxGroupNameLength)
                return (false,
                        $"Image #{i + 1}: group name cannot exceed {MaxGroupNameLength} characters.");

            // Sidecar must exist, belong to the uploader, be scoped to this Software.
            PendingImageStore.PendingMetadata meta =
                await PendingImageStore.GetMetadataAsync(assetRootPath, "software-screenshots", e.Guid);

            if(meta is null) return (false, $"Image #{i + 1}: pending upload {e.Guid:D} not found.");

            if(meta.EntityType != (byte)SuggestionEntityType.SoftwareScreenshot)
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} is not a software screenshot image.");

            if(!string.Equals(meta.UploadedById, uploaderUserId, StringComparison.Ordinal))
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} belongs to a different user.");

            if(meta.ParentEntityId != entityId.Value)
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} is scoped to a different software.");
        }

        return (true, null);
    }

    /// <summary>
    ///     Apply an admin's accept/reject decisions. For each image in the suggested array:
    ///     if its <c>screenshot.&lt;guid&gt;</c> key is in <paramref name="accepted" />,
    ///     promote the pending file to <c>originals/</c>, create a
    ///     <see cref="SoftwareScreenshot" /> row where the Id IS the pending guid, and
    ///     fire-and-forget the conversion worker. Otherwise, delete the pending file. The
    ///     suggestion-level <c>platform_id</c> is applied uniformly to every accepted row.
    ///     Returns the set of accept-keys actually applied (subset of
    ///     <paramref name="accepted" />) plus a flag indicating the parent Software was
    ///     missing (so the controller can mark the suggestion stale).
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

        if(!suggested.TryGetValue(FieldPlatformId, out object platformRaw) || platformRaw is null)
            return (applied, false);

        if(!TryCoerceUlong(platformRaw, out ulong platformId) || platformId == 0) return (applied, false);

        bool platformExists = await context.SoftwarePlatforms.AnyAsync(p => p.Id == platformId);
        if(!platformExists) return (applied, false);

        if(!suggested.TryGetValue(FieldPhotos, out object photosRaw) || photosRaw is null) return (applied, false);

        List<ScreenshotEntry> entries = ParsePhotosArray(photosRaw);
        if(entries is null) return (applied, false);

        foreach(ScreenshotEntry e in entries)
        {
            if(e.Guid == Guid.Empty) continue;

            string acceptKey = ScreenshotAcceptKey(e.Guid);

            if(accepted.Contains(acceptKey))
            {
                bool ok = await PromoteAndPersistAsync(context, softwareId, platformId, creditedUserId,
                                                       assetRootPath, e);

                if(ok) applied.Add(acceptKey);
            }
            else
            {
                // Rejected (or admin left it unchecked): clean up the pending file.
                try
                {
                    PendingImageStore.Delete(assetRootPath, "software-screenshots", e.Guid);
                }
                catch
                {
                    /* best-effort */
                }
            }
        }

        return (applied, false);
    }

    static async Task<bool> PromoteAndPersistAsync(MarechaiContext context, ulong softwareId, ulong platformId,
                                                    string creditedUserId, string assetRootPath, ScreenshotEntry entry)
    {
        try
        {
            (string movedPath, string ext) =
                await PendingImageStore.PromoteToOriginalsAsync(assetRootPath, "software-screenshots", entry.Guid);

            if(movedPath is null || string.IsNullOrEmpty(ext)) return false;

            // Optional group resolve-or-create. Mirrors
            // SoftwareScreenshotsController.ResolveOrCreateGroupAsync — case-sensitive lookup
            // on Name, create-if-missing. Empty / whitespace input clears the FK.
            int? groupId = null;
            string trimmedGroup = entry.GroupName?.Trim();

            if(!string.IsNullOrWhiteSpace(trimmedGroup))
            {
                if(trimmedGroup.Length > MaxGroupNameLength) trimmedGroup = trimmedGroup[..MaxGroupNameLength];

                SoftwareScreenshotGroup group =
                    await context.SoftwareScreenshotGroups.FirstOrDefaultAsync(g => g.Name == trimmedGroup);

                if(group is null)
                {
                    group = new SoftwareScreenshotGroup { Name = trimmedGroup };
                    await context.SoftwareScreenshotGroups.AddAsync(group);
                    await context.SaveChangesWithUserAsync(creditedUserId);
                }

                groupId = group.Id;
            }

            var screenshot = new SoftwareScreenshot
            {
                Id                 = entry.Guid,
                SoftwareId         = softwareId,
                SoftwarePlatformId = platformId,
                SoftwareVersionId  = null,
                GroupId            = groupId,
                Caption            = string.IsNullOrEmpty(entry.Caption) ? null : entry.Caption,
                OriginalExtension  = ext.TrimStart('.')
            };

            await context.SoftwareScreenshots.AddAsync(screenshot);
            await context.SaveChangesWithUserAsync(creditedUserId);

            // Fire-and-forget conversion worker — same pattern as admin
            // SoftwareScreenshotsController.UploadAsync.
            string sourceFormat = ext.TrimStart('.');
            Guid   newId        = screenshot.Id;
            string capturedPath = movedPath;
            string capturedRoot = assetRootPath;

            _ = Task.Run(() =>
            {
                try
                {
                    Marechai.Helpers.Photos.EnsureCreated(capturedRoot, false, "software-screenshots");
                    var photos = new Marechai.Helpers.Photos();
                    photos.ConversionWorker(capturedRoot, newId, capturedPath, sourceFormat, false,
                                            "software-screenshots");
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

    sealed class ScreenshotEntry
    {
        public Guid   Guid      { get; init; }
        public string Extension { get; init; }
        public string Caption   { get; init; }

        /// <summary>
        ///     Optional canonical English screenshot-group name. When non-null, the applier
        ///     does a get-or-create against <see cref="SoftwareScreenshotGroup" /> and assigns
        ///     the resulting Id to the new screenshot row. Empty / whitespace clears the FK.
        /// </summary>
        public string GroupName { get; init; }
    }

    static List<ScreenshotEntry> ParsePhotosArray(object raw)
    {
        var result = new List<ScreenshotEntry>();

        if(raw is JsonElement je)
        {
            if(je.ValueKind != JsonValueKind.Array) return null;

            foreach(JsonElement item in je.EnumerateArray())
            {
                if(item.ValueKind != JsonValueKind.Object) return null;
                ScreenshotEntry e = ParseScreenshotFromJson(item);
                if(e is null) return null;
                result.Add(e);
            }

            return result;
        }

        if(raw is System.Collections.IEnumerable enumerable)
        {
            foreach(object item in enumerable)
            {
                ScreenshotEntry e = ParseScreenshotFromObject(item);
                if(e is null) return null;
                result.Add(e);
            }

            return result;
        }

        return null;
    }

    static ScreenshotEntry ParseScreenshotFromJson(JsonElement obj)
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

        string groupName = obj.TryGetProperty("groupName", out JsonElement grpEl) &&
                           grpEl.ValueKind == JsonValueKind.String
                               ? grpEl.GetString()
                               : null;

        return new ScreenshotEntry { Guid = guid, Extension = extension, Caption = caption, GroupName = groupName };
    }

    static ScreenshotEntry ParseScreenshotFromObject(object item)
    {
        if(item is null) return null;

        if(item is JsonElement je) return ParseScreenshotFromJson(je);

        if(item is IDictionary<string, object> dict)
        {
            if(!dict.TryGetValue("guid", out object g) || g is null) return null;
            if(!Guid.TryParse(g.ToString(), out Guid guid)) return null;

            string extension = dict.TryGetValue("extension", out object x) ? CoerceString(x) : null;
            string caption   = dict.TryGetValue("caption", out object c) ? CoerceString(c) : null;
            string groupName = dict.TryGetValue("groupName", out object gn) ? CoerceString(gn) : null;

            return new ScreenshotEntry
            {
                Guid      = guid,
                Extension = extension,
                Caption   = caption,
                GroupName = groupName
            };
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

    static bool TryCoerceUlong(object value, out ulong result)
    {
        result = 0;

        switch(value)
        {
            case null: return false;
            case ulong u:
                result = u;
                return true;
            case long l when l >= 0:
                result = (ulong)l;
                return true;
            case int i when i >= 0:
                result = (ulong)i;
                return true;
            case short s when s >= 0:
                result = (ulong)s;
                return true;
            case byte b:
                result = b;
                return true;
            case string str: return ulong.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
            case JsonElement je:
                switch(je.ValueKind)
                {
                    case JsonValueKind.Number:
                        if(je.TryGetUInt64(out ulong uv))
                        {
                            result = uv;
                            return true;
                        }

                        if(je.TryGetInt64(out long lv) && lv >= 0)
                        {
                            result = (ulong)lv;
                            return true;
                        }

                        return false;
                    case JsonValueKind.String:
                        return ulong.TryParse(je.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                                              out result);
                    default: return false;
                }

            default: return ulong.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                                           out result);
        }
    }

    /// <summary>
    ///     Total image count carried by the suggestion's <c>photos</c> array, used by the
    ///     SuggestionsController status-determination logic to decide between Accepted /
    ///     PartiallyAccepted / Rejected based on how many screenshot accept-keys were
    ///     checked.
    /// </summary>
    public static int CountSuggestedItems(Dictionary<string, object> suggested)
    {
        if(suggested is null) return 0;
        if(!suggested.TryGetValue(FieldPhotos, out object raw) || raw is null) return 0;
        List<ScreenshotEntry> entries = ParsePhotosArray(raw);
        return entries?.Count ?? 0;
    }
}
