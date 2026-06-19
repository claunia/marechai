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
///     Applier for collaborative Machine promo art batch-upload suggestions
///     (<see cref="SuggestionEntityType.MachinePromoArt" />).
/// </summary>
public static class MachinePromoArtSuggestionApplier
{
    public const string FieldGroupName = "group_name";
    public const string FieldPhotos = "photos";
    public const int MaxPhotosPerBatch = 30;
    public const int MaxGroupNameLength = 256;
    public const int MaxCaptionLength = 500;

    static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "jpg", "jpeg", "png", "webp"
    };

    public static string PromoAcceptKey(Guid guid) =>
        "promo." + guid.ToString("D", CultureInfo.InvariantCulture);

    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;
        if(fieldName == FieldGroupName || fieldName == FieldPhotos) return true;

        if(fieldName.StartsWith("promo.", StringComparison.Ordinal))
        {
            string rest = fieldName.Substring("promo.".Length);
            return Guid.TryParse(rest, out _);
        }

        return false;
    }

    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));

        bool exists = await context.Machines.AsNoTracking().AnyAsync(m => m.Id == (int)entityId);
        return exists ? new Dictionary<string, object>(StringComparer.Ordinal) : null;
    }

    public static async Task<(bool ok, string error)> ValidateAsync(MarechaiContext context, long? entityId,
                                                                    Dictionary<string, object> suggested,
                                                                    string assetRootPath, string uploaderUserId)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));
        if(suggested is null) return (false, "Missing suggestion payload.");

        if(!entityId.HasValue || entityId.Value <= 0)
            return (false, "Machine promo art suggestions must reference an existing machine via entity_id.");

        int machineId = (int)entityId.Value;
        bool machineExists = await context.Machines.AsNoTracking().AnyAsync(m => m.Id == machineId);
        if(!machineExists) return (false, $"Machine #{machineId} not found.");

        if(!suggested.TryGetValue(FieldGroupName, out object groupRaw) || groupRaw is null)
            return (false, "A machine promo art suggestion must include a 'group_name' field.");

        string groupName = CoerceString(groupRaw)?.Trim();
        if(string.IsNullOrEmpty(groupName))
            return (false, "'group_name' cannot be empty.");

        if(groupName.Length > MaxGroupNameLength)
            return (false, $"'group_name' cannot exceed {MaxGroupNameLength} characters.");

        if(!suggested.TryGetValue(FieldPhotos, out object photosRaw) || photosRaw is null)
            return (false, "A machine promo art suggestion must include a non-empty 'photos' array.");

        List<PromoEntry> entries = ParsePhotosArray(photosRaw);
        if(entries is null) return (false, "'photos' must be a JSON array of objects.");
        if(entries.Count == 0) return (false, "A machine promo art suggestion must include at least one image.");
        if(entries.Count > MaxPhotosPerBatch)
            return (false, $"A machine promo art suggestion cannot include more than {MaxPhotosPerBatch} images.");

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

            PendingImageStore.PendingMetadata meta =
                await PendingImageStore.GetMetadataAsync(assetRootPath, "machine-promo-art", e.Guid);

            if(meta is null) return (false, $"Image #{i + 1}: pending upload {e.Guid:D} not found.");
            if(meta.EntityType != (byte)SuggestionEntityType.MachinePromoArt)
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} is not a machine promo art image.");
            if(!string.Equals(meta.UploadedById, uploaderUserId, StringComparison.Ordinal))
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} belongs to a different user.");
            if(meta.ParentEntityId != entityId.Value)
                return (false, $"Image #{i + 1}: pending upload {e.Guid:D} is scoped to a different machine.");
        }

        return (true, null);
    }

    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(MarechaiContext context,
        long entityId, Dictionary<string, object> suggested, HashSet<string> accepted, string creditedUserId,
        string assetRootPath)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);
        if(suggested is null) return (applied, false);

        int machineId = (int)entityId;
        bool machineExists = await context.Machines.AnyAsync(m => m.Id == machineId);
        if(!machineExists) return (applied, true);

        if(!suggested.TryGetValue(FieldGroupName, out object groupRaw) || groupRaw is null) return (applied, false);
        string groupName = CoerceString(groupRaw)?.Trim();
        if(string.IsNullOrEmpty(groupName) || groupName.Length > MaxGroupNameLength) return (applied, false);

        if(!suggested.TryGetValue(FieldPhotos, out object photosRaw) || photosRaw is null) return (applied, false);
        List<PromoEntry> entries = ParsePhotosArray(photosRaw);
        if(entries is null) return (applied, false);

        bool anyAccepted = entries.Any(e => e.Guid != Guid.Empty && accepted.Contains(PromoAcceptKey(e.Guid)));

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
                bool ok = await PromoteAndPersistAsync(context, machineId, group.Id, creditedUserId, assetRootPath, e);
                if(ok) applied.Add(acceptKey);
            }
            else
            {
                try { PendingImageStore.Delete(assetRootPath, "machine-promo-art", e.Guid); } catch { }
            }
        }

        return (applied, false);
    }

    static async Task<bool> PromoteAndPersistAsync(MarechaiContext context, int machineId, int groupId,
                                                   string creditedUserId, string assetRootPath, PromoEntry entry)
    {
        try
        {
            (string movedPath, string ext) =
                await PendingImageStore.PromoteToOriginalsAsync(assetRootPath, "machine-promo-art", entry.Guid);

            if(movedPath is null || string.IsNullOrEmpty(ext)) return false;

            var promo = new MachinePromoArt
            {
                Id = entry.Guid,
                MachineId = machineId,
                GroupId = groupId,
                Caption = string.IsNullOrEmpty(entry.Caption) ? null : entry.Caption,
                OriginalExtension = ext.TrimStart('.')
            };

            await context.MachinePromoArt.AddAsync(promo);
            await context.SaveChangesWithUserAsync(creditedUserId);

            string sourceFormat = ext.TrimStart('.');
            Guid newId = promo.Id;
            string capturedPath = movedPath;
            string capturedRoot = assetRootPath;

            _ = Task.Run(() =>
            {
                try
                {
                    Marechai.Helpers.Photos.EnsureCreated(capturedRoot, false, "machine-promo-art");
                    var photos = new Marechai.Helpers.Photos();
                    photos.ConversionWorker(capturedRoot, newId, capturedPath, sourceFormat, false,
                                            "machine-promo-art");
                }
                catch
                {
                }
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    sealed class PromoEntry
    {
        public Guid Guid { get; init; }
        public string Extension { get; init; }
        public string Caption { get; init; }
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
            string caption = dict.TryGetValue("caption", out object c) ? CoerceString(c) : null;
            return new PromoEntry { Guid = guid, Extension = extension, Caption = caption };
        }

        return null;
    }

    static string CoerceString(object value) =>
        value switch
        {
            null => null,
            string s => s,
            JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
            JsonElement je when je.ValueKind == JsonValueKind.Null => null,
            _ => value.ToString()
        };

    public static int CountSuggestedItems(Dictionary<string, object> suggested)
    {
        if(suggested is null) return 0;
        if(!suggested.TryGetValue(FieldPhotos, out object raw) || raw is null) return 0;
        List<PromoEntry> entries = ParsePhotosArray(raw);
        return entries?.Count ?? 0;
    }
}
