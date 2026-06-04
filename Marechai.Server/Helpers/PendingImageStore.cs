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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Marechai.Server.Helpers;

/// <summary>
///     Sidecar-based store for collaborator-uploaded images that are pending admin review
///     (currently book covers, designed for reuse with magazine issue covers and other
///     image kinds in the future).
///     <para>
///         Each upload writes two files under
///         <c>{assetRootPath}/photos/{itemFolder}/pending/</c>:
///         <list type="bullet">
///             <item><description><c>{guid}.{ext}</c> — the raw image (jpg/png/webp).</description></item>
///             <item><description><c>{guid}.json</c> — sidecar metadata (uploader, entity id, timestamp, content type).</description></item>
///         </list>
///     </para>
///     <para>
///         The sidecar lets the controller authorize "is this user the uploader?", scoped
///         cleanup ("delete every pending upload by user X for entity Y") and orphan-sweep
///         queries without touching the database. No EF migration required.
///     </para>
/// </summary>
public static class PendingImageStore
{
    /// <summary>Minimum extension whitelist applied to every pending upload, regardless of item kind.</summary>
    public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    /// <summary>Matching content types for the whitelist above.</summary>
    public static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    public sealed class PendingMetadata
    {
        public Guid     Guid           { get; set; }
        public string   Extension      { get; set; }
        public byte     EntityType     { get; set; }
        public long     EntityId       { get; set; }
        public string   UploadedById   { get; set; }
        public DateTime UploadedOn     { get; set; }
        public string   ContentType    { get; set; }
        public long     SizeBytes      { get; set; }
        /// <summary>
        ///     Optional parent-entity scope used by features where the pending image is
        ///     associated with an existing parent record AND uploaded BEFORE the suggestion
        ///     row exists (e.g. the GPU-photo collaborative-upload flow stores the parent
        ///     <c>GpuId</c> here so the per-uploader cap and cleanup can be scoped per GPU).
        ///     Null for legacy single-image flows (book/issue/person covers).
        /// </summary>
        public long?    ParentEntityId { get; set; }

        /// <summary>
        ///     Marks the pending image as part of an admin batch-upload staging session
        ///     rather than a collaborator suggestion. Admin entries share the same
        ///     <c>pending/</c> folder but bypass the suggestion-review workflow at commit
        ///     time.
        /// </summary>
        public bool     IsAdminStaging { get; set; }

        /// <summary>
        ///     Image dimensions captured at staging time. Null when staging happened before
        ///     this field was added (legacy suggestion uploads), or when identification
        ///     failed for any reason.
        /// </summary>
        public int?     Width          { get; set; }

        public int?     Height         { get; set; }
    }

    /// <summary>
    ///     Returns the absolute path to the pending folder for the given item kind. Creates
    ///     the directory tree if missing. The item folder is the same identifier used by
    ///     <see cref="Marechai.Helpers.Photos.EnsureCreated" /> (e.g. <c>"book-covers"</c>).
    /// </summary>
    public static string EnsurePendingDir(string assetRootPath, string itemFolder)
    {
        string pendingDir = Path.Combine(assetRootPath, "photos", itemFolder, "pending");
        Directory.CreateDirectory(pendingDir);
        return pendingDir;
    }

    /// <summary>
    ///     Persist a freshly-uploaded image plus its sidecar. Returns the new guid.
    /// </summary>
    public static Task<Guid> StoreAsync(string assetRootPath, string itemFolder, string extension,
                                        byte entityType, long entityId, string uploadedById,
                                        string contentType, Stream contents) =>
        StoreAsync(assetRootPath, itemFolder, extension, entityType, entityId, uploadedById, contentType, contents,
                   parentEntityId: null);

    /// <summary>
    ///     Persist a freshly-uploaded image plus its sidecar with an optional parent-entity
    ///     scope. <paramref name="parentEntityId" /> is null for legacy single-image flows
    ///     (book/issue/person covers) and set to the parent record id for batch flows where
    ///     the same uploader can stage multiple in-flight images for the same parent (e.g.
    ///     GPU photos: parentEntityId = GpuId so the per-uploader cap is scoped per GPU).
    /// </summary>
    public static Task<Guid> StoreAsync(string assetRootPath, string itemFolder, string extension,
                                        byte entityType, long entityId, string uploadedById,
                                        string contentType, Stream contents, long? parentEntityId) =>
        StoreAsync(assetRootPath, itemFolder, extension, entityType, entityId, uploadedById, contentType, contents,
                   parentEntityId, AllowedExtensions, isAdminStaging: false, width: null, height: null);

    /// <summary>
    ///     Full-control overload. <paramref name="allowedExtensions" /> overrides the default
    ///     suggestion whitelist (admin batch staging accepts a wider set: JPEG/PNG/WebP/AVIF/BMP/TIFF);
    ///     <paramref name="isAdminStaging" /> marks the sidecar so the admin-vs-suggestion counters
    ///     can distinguish entries sharing the same pending folder.
    /// </summary>
    public static async Task<Guid> StoreAsync(string assetRootPath, string itemFolder, string extension,
                                              byte entityType, long entityId, string uploadedById,
                                              string contentType, Stream contents, long? parentEntityId,
                                              HashSet<string> allowedExtensions, bool isAdminStaging,
                                              int? width, int? height)
    {
        if(string.IsNullOrEmpty(extension)) throw new ArgumentException("Extension required.", nameof(extension));
        HashSet<string> whitelist = allowedExtensions ?? AllowedExtensions;
        if(!whitelist.Contains(extension))
            throw new ArgumentException($"Extension '{extension}' is not in the allowed list.", nameof(extension));
        if(string.IsNullOrEmpty(uploadedById))
            throw new ArgumentException("Uploader id required.", nameof(uploadedById));

        string pendingDir = EnsurePendingDir(assetRootPath, itemFolder);
        Guid   guid       = Guid.NewGuid();
        string ext        = extension.StartsWith('.') ? extension.ToLowerInvariant() : "." + extension.ToLowerInvariant();
        string filePath   = Path.Combine(pendingDir, guid.ToString() + ext);
        string sidecar    = Path.Combine(pendingDir, guid.ToString() + ".json");

        long size;
        await using(var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
        {
            await contents.CopyToAsync(fs);
            size = fs.Length;
        }

        var meta = new PendingMetadata
        {
            Guid           = guid,
            Extension      = ext.TrimStart('.'),
            EntityType     = entityType,
            EntityId       = entityId,
            UploadedById   = uploadedById,
            UploadedOn     = DateTime.UtcNow,
            ContentType    = contentType,
            SizeBytes      = size,
            ParentEntityId = parentEntityId,
            IsAdminStaging = isAdminStaging,
            Width          = width,
            Height         = height
        };
        await File.WriteAllTextAsync(sidecar, JsonSerializer.Serialize(meta));

        return guid;
    }

    /// <summary>
    ///     Persist a JPEG thumbnail next to a previously-stored pending image so the upload
    ///     dialog can render a preview without re-reading the original. The thumbnail file
    ///     uses the suffix <c>.thumb.jpg</c> and is cleaned up alongside the image by
    ///     <see cref="Delete" /> via the glob fallback.
    /// </summary>
    public static async Task StoreThumbnailAsync(string assetRootPath, string itemFolder, Guid guid, byte[] jpegBytes)
    {
        string pendingDir = EnsurePendingDir(assetRootPath, itemFolder);
        string thumbPath  = Path.Combine(pendingDir, guid.ToString() + ".thumb.jpg");
        await File.WriteAllBytesAsync(thumbPath, jpegBytes);
    }

    /// <summary>
    ///     Returns the JPEG thumbnail bytes for a pending image, or null when missing.
    /// </summary>
    public static async Task<byte[]> ReadThumbnailBytesAsync(string assetRootPath, string itemFolder, Guid guid)
    {
        string pendingDir = EnsurePendingDir(assetRootPath, itemFolder);
        string thumbPath  = Path.Combine(pendingDir, guid.ToString() + ".thumb.jpg");
        if(!File.Exists(thumbPath)) return null;
        try { return await File.ReadAllBytesAsync(thumbPath); } catch { return null; }
    }

    /// <summary>Look up the sidecar metadata for a given guid; returns null when no sidecar exists.</summary>
    public static async Task<PendingMetadata> GetMetadataAsync(string assetRootPath, string itemFolder, Guid guid)
    {
        string pendingDir = EnsurePendingDir(assetRootPath, itemFolder);
        string sidecar    = Path.Combine(pendingDir, guid.ToString() + ".json");
        if(!File.Exists(sidecar)) return null;

        try
        {
            string json = await File.ReadAllTextAsync(sidecar);
            return JsonSerializer.Deserialize<PendingMetadata>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Returns the absolute path of the image file for a guid (looking up the extension
    ///     from the sidecar). Returns null if either the sidecar or the file is missing.
    /// </summary>
    public static async Task<string> GetImagePathAsync(string assetRootPath, string itemFolder, Guid guid)
    {
        PendingMetadata meta = await GetMetadataAsync(assetRootPath, itemFolder, guid);
        if(meta is null) return null;

        string pendingDir = EnsurePendingDir(assetRootPath, itemFolder);
        string filePath   = Path.Combine(pendingDir, guid.ToString() + "." + meta.Extension);
        return File.Exists(filePath) ? filePath : null;
    }

    /// <summary>Delete the image+sidecar+thumbnail for a given guid. Silently absorbs missing files.</summary>
    public static void Delete(string assetRootPath, string itemFolder, Guid guid)
    {
        string pendingDir = EnsurePendingDir(assetRootPath, itemFolder);
        string sidecar    = Path.Combine(pendingDir, guid.ToString() + ".json");

        // Try to read extension from sidecar so we delete the matching image; fall back to a
        // glob if the sidecar is unreadable or missing.
        try
        {
            if(File.Exists(sidecar))
            {
                string  json = File.ReadAllText(sidecar);
                var meta = JsonSerializer.Deserialize<PendingMetadata>(json);
                if(meta is not null && !string.IsNullOrEmpty(meta.Extension))
                {
                    string filePath = Path.Combine(pendingDir, guid.ToString() + "." + meta.Extension);
                    if(File.Exists(filePath)) File.Delete(filePath);
                }
            }
        }
        catch
        {
            // ignored
        }

        // Fallback: glob delete in case extension lookup failed. Picks up both the original
        // image and the .thumb.jpg sidecar.
        try
        {
            foreach(string f in Directory.GetFiles(pendingDir, guid.ToString() + ".*"))
            {
                if(!f.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Delete(f); } catch { /* ignored */ }
                }
            }
        }
        catch
        {
            // ignored
        }

        try { if(File.Exists(sidecar)) File.Delete(sidecar); } catch { /* ignored */ }
    }

    /// <summary>
    ///     Delete every pending image+sidecar uploaded by <paramref name="userId" /> for the
    ///     given (entity type, entity id) combination. Used to enforce the "user has at most
    ///     one pending image per entity" rule when they upload a replacement.
    /// </summary>
    public static int DeleteByUploaderForEntity(string assetRootPath, string itemFolder,
                                                string userId, byte entityType, long entityId)
    {
        if(string.IsNullOrEmpty(userId)) return 0;

        string pendingDir = EnsurePendingDir(assetRootPath, itemFolder);
        int    removed    = 0;

        IEnumerable<string> sidecars;
        try { sidecars = Directory.EnumerateFiles(pendingDir, "*.json"); }
        catch { return 0; }

        foreach(string sidecar in sidecars.ToList())
        {
            try
            {
                var meta = JsonSerializer.Deserialize<PendingMetadata>(File.ReadAllText(sidecar));
                if(meta is null) continue;
                if(meta.EntityType != entityType || meta.EntityId != entityId) continue;
                if(!string.Equals(meta.UploadedById, userId, StringComparison.Ordinal)) continue;

                Delete(assetRootPath, itemFolder, meta.Guid);
                removed++;
            }
            catch
            {
                // ignored
            }
        }

        return removed;
    }

    /// <summary>
    ///     Count pending images uploaded by <paramref name="userId" /> and scoped to the
    ///     given (entity type, parent entity id) combination. Used by batch flows (e.g.
    ///     GPU photos) to enforce a per-uploader cap (currently 15 in-flight pending photos
    ///     per parent entity). Admin-staging entries are excluded so they don't double-count
    ///     against the collaborator-suggestion cap.
    /// </summary>
    public static int CountByUploaderForParentEntity(string assetRootPath, string itemFolder,
                                                     string userId, byte entityType, long parentEntityId)
    {
        if(string.IsNullOrEmpty(userId)) return 0;

        string pendingDir = EnsurePendingDir(assetRootPath, itemFolder);
        int    count      = 0;

        IEnumerable<string> sidecars;
        try { sidecars = Directory.EnumerateFiles(pendingDir, "*.json"); }
        catch { return 0; }

        foreach(string sidecar in sidecars)
        {
            try
            {
                var meta = JsonSerializer.Deserialize<PendingMetadata>(File.ReadAllText(sidecar));
                if(meta is null) continue;
                if(meta.IsAdminStaging) continue;
                if(meta.EntityType != entityType) continue;
                if(meta.ParentEntityId != parentEntityId) continue;
                if(!string.Equals(meta.UploadedById, userId, StringComparison.Ordinal)) continue;
                count++;
            }
            catch
            {
                // ignored
            }
        }

        return count;
    }

    /// <summary>
    ///     Delete every pending image+sidecar uploaded by <paramref name="userId" /> and
    ///     scoped to the given (entity type, parent entity id) combination. Used to clean up
    ///     orphan pending uploads when a batch suggestion (e.g. GPU photos) is cancelled,
    ///     rejected, withdrawn or marked stale.
    /// </summary>
    public static int DeleteByUploaderForParentEntity(string assetRootPath, string itemFolder,
                                                      string userId, byte entityType, long parentEntityId)
    {
        if(string.IsNullOrEmpty(userId)) return 0;

        string pendingDir = EnsurePendingDir(assetRootPath, itemFolder);
        int    removed    = 0;

        IEnumerable<string> sidecars;
        try { sidecars = Directory.EnumerateFiles(pendingDir, "*.json"); }
        catch { return 0; }

        foreach(string sidecar in sidecars.ToList())
        {
            try
            {
                var meta = JsonSerializer.Deserialize<PendingMetadata>(File.ReadAllText(sidecar));
                if(meta is null) continue;
                if(meta.EntityType != entityType) continue;
                if(meta.ParentEntityId != parentEntityId) continue;
                if(!string.Equals(meta.UploadedById, userId, StringComparison.Ordinal)) continue;

                Delete(assetRootPath, itemFolder, meta.Guid);
                removed++;
            }
            catch
            {
                // ignored
            }
        }

        return removed;
    }

    /// <summary>
    ///     Returns true if the calling user is allowed to read/delete the pending image. The
    ///     uploader and any admin role are allowed; everyone else is denied.
    /// </summary>
    public static bool CanAccess(PendingMetadata meta, string callerUserId, bool callerIsAdmin)
    {
        if(meta is null) return false;
        if(callerIsAdmin) return true;
        return string.Equals(meta.UploadedById, callerUserId, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Move a pending image into the entity's <c>originals/</c> folder under the SAME
    ///     guid+extension (so the existing conversion worker picks it up unchanged). Deletes
    ///     the sidecar afterward. Returns the absolute path of the moved file or null when
    ///     the source is missing.
    /// </summary>
    public static async Task<(string movedPath, string extension)> PromoteToOriginalsAsync(
        string assetRootPath, string itemFolder, Guid guid)
    {
        PendingMetadata meta = await GetMetadataAsync(assetRootPath, itemFolder, guid);
        if(meta is null) return (null, null);

        string pendingDir   = EnsurePendingDir(assetRootPath, itemFolder);
        string srcPath      = Path.Combine(pendingDir, guid.ToString() + "." + meta.Extension);
        if(!File.Exists(srcPath)) return (null, null);

        string originalsDir = Path.Combine(assetRootPath, "photos", itemFolder, "originals");
        Directory.CreateDirectory(originalsDir);

        string destPath = Path.Combine(originalsDir, guid.ToString() + "." + meta.Extension);
        if(File.Exists(destPath)) File.Delete(destPath);

        File.Move(srcPath, destPath);

        // Sidecar can go now; the image lives in originals.
        string sidecar = Path.Combine(pendingDir, guid.ToString() + ".json");
        try { if(File.Exists(sidecar)) File.Delete(sidecar); } catch { /* ignored */ }

        return (destPath, meta.Extension);
    }

    /// <summary>
    ///     Delete every pending entry (image + sidecar + thumbnail) in
    ///     <paramref name="itemFolder" /> whose recorded <see cref="PendingMetadata.UploadedOn" />
    ///     is older than <paramref name="olderThan" />. When the sidecar is unreadable
    ///     or missing a usable <c>UploadedOn</c>, the file's UTC last-write time is used
    ///     as a fallback so genuinely abandoned files are still cleaned up. Returns the
    ///     number of pending entries removed. Used by the periodic
    ///     <c>PendingImagePurgeService</c> background sweep.
    /// </summary>
    public static int PurgeStale(string assetRootPath, string itemFolder, TimeSpan olderThan)
    {
        string   pendingDir = EnsurePendingDir(assetRootPath, itemFolder);
        DateTime cutoffUtc  = DateTime.UtcNow - olderThan;
        int      removed    = 0;

        IEnumerable<string> sidecars;
        try { sidecars = Directory.EnumerateFiles(pendingDir, "*.json"); }
        catch { return 0; }

        foreach(string sidecar in sidecars)
        {
            Guid     guid;
            DateTime whenUtc;

            try
            {
                string baseName = Path.GetFileNameWithoutExtension(sidecar);
                if(!Guid.TryParse(baseName, out guid)) continue;

                whenUtc = File.GetLastWriteTimeUtc(sidecar);

                try
                {
                    var meta = JsonSerializer.Deserialize<PendingMetadata>(File.ReadAllText(sidecar));
                    if(meta is not null && meta.UploadedOn != default)
                    {
                        whenUtc = meta.UploadedOn.Kind == DateTimeKind.Utc
                                      ? meta.UploadedOn
                                      : meta.UploadedOn.ToUniversalTime();
                    }
                }
                catch
                {
                    // Sidecar present but unreadable / wrong schema — fall through to the
                    // filesystem mtime captured above and let the age check decide.
                }
            }
            catch
            {
                continue;
            }

            if(whenUtc > cutoffUtc) continue;

            Delete(assetRootPath, itemFolder, guid);
            removed++;
        }

        // Also sweep orphan image/thumbnail files that have no matching sidecar at all
        // (e.g. a crash between StoreAsync's image-write and sidecar-write). Use the file
        // mtime as the only signal. Skip anything that still has a sibling .json — Delete
        // above handles those.
        IEnumerable<string> all;
        try { all = Directory.EnumerateFiles(pendingDir); }
        catch { return removed; }

        foreach(string path in all)
        {
            if(path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) continue;

            try
            {
                string baseName = Path.GetFileNameWithoutExtension(path);
                // Strip the secondary ".thumb" extension component so the guid parses cleanly.
                if(baseName.EndsWith(".thumb", StringComparison.OrdinalIgnoreCase))
                    baseName = baseName[..^".thumb".Length];

                if(!Guid.TryParse(baseName, out Guid _)) continue;

                string companionSidecar = Path.Combine(pendingDir, baseName + ".json");
                if(File.Exists(companionSidecar)) continue;

                if(File.GetLastWriteTimeUtc(path) > cutoffUtc) continue;

                try { File.Delete(path); }
                catch { /* ignored */ }
            }
            catch
            {
                // ignored
            }
        }

        return removed;
    }
}
