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
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.MagazineIssueSuggestionMetadata</c>.
///     Handles BOTH scalar MagazineIssue fields AND junction add/remove operations across the 4
///     suggestable junction tables (People, Machines, MachineFamilies, Software). Each junction
///     operation is atomic — its full row payload is accepted or rejected as a single unit.
///     Edits to existing junction rows are modelled as a <c>remove</c> + <c>add</c> pair which
///     the admin can accept independently. Mirrors the design of
///     <see cref="BookSuggestionApplier" /> with these differences:
///     <list type="bullet">
///         <item><description><see cref="MagazineIssue.MagazineId" /> is intentionally NOT in the
///             scalar set — re-parenting an issue to a different magazine is admin-only.</description></item>
///         <item><description>Companies are bound to the parent <see cref="Magazine" /> entity, not
///             to individual issues, so the <c>companies</c> junction is out of scope (handled by
///             <see cref="MagazineSuggestionApplier" />).</description></item>
///         <item><description>The <c>software</c> junction replaces Book's <c>companies</c> group.
///             <see cref="MagazinesBySoftware.SoftwareId" /> is <c>ulong</c> so the
///             FK-existence + dedup checks use <c>(ulong)</c> casts.</description></item>
///         <item><description>Note the database naming oddity: all four junction tables use
///             <c>MagazineId</c> as their FK column name, but the FK actually points to
///             <see cref="MagazineIssue.Id" />, NOT <see cref="Magazine.Id" />.</description></item>
///     </list>
/// </summary>
internal static class MagazineIssueSuggestionApplier
{
    // ---- Scalar field names (MUST mirror the client-side metadata constants) -----------
    public const string FieldCaption            = "caption";
    public const string FieldNativeCaption      = "native_caption";
    public const string FieldPublished          = "published";
    public const string FieldPublishedPrecision = "published_precision";
    public const string FieldProductCode        = "product_code";
    public const string FieldPages              = "pages";
    public const string FieldIssueNumber        = "issue_number";
    public const string FieldInternetArchiveUrl = "internet_archive_url";

    /// <summary>
    ///     Pseudo-field carrying the guid of a pending-cover upload that should replace
    ///     <see cref="MagazineIssue.CoverGuid" /> when this suggestion is accepted. The
    ///     corresponding image file lives under
    ///     <c>magazine-issue-covers/pending/&lt;guid&gt;.&lt;ext&gt;</c> with a sidecar JSON
    ///     file recording the extension and uploader. On accept,
    ///     <see cref="Marechai.Server.Helpers.PendingImageStore.PromoteToOriginalsAsync" /> moves
    ///     the file into <c>magazine-issue-covers/originals/</c> (extension comes from the
    ///     sidecar) and the existing <see cref="Marechai.Helpers.Photos.ConversionWorker" />
    ///     generates the AVIF/JXL/WebP/JPEG variants.
    /// </summary>
    public const string FieldCoverPendingGuid = "cover_pending_guid";

    /// <summary>
    ///     Pseudo-scalar carrying the parent magazine id for brand-new-issue creation
    ///     (<see cref="CreateAsync" />). Intentionally NOT in <c>s_scalarFieldNames</c>:
    ///     <see cref="ApplyAsync" /> rejects it because re-parenting an existing issue is
    ///     admin-only. Only the addition path consumes this key.
    /// </summary>
    public const string FieldMagazineId = "magazine_id";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldCaption, FieldNativeCaption,
        FieldPublished, FieldPublishedPrecision,
        FieldProductCode, FieldPages, FieldIssueNumber,
        FieldInternetArchiveUrl, FieldCoverPendingGuid
    };

    // ---- Junction group identifiers (lowercase snake_case prefix in field-name keys) ---
    public const string GroupPeople          = "people";
    public const string GroupMachines        = "machines";
    public const string GroupMachineFamilies = "machine_families";
    public const string GroupSoftware        = "software";

    public static readonly IReadOnlyCollection<string> JunctionGroups = new[]
    {
        GroupPeople, GroupMachines, GroupMachineFamilies, GroupSoftware
    };

    /// <summary>
    ///     Returns <c>true</c> when the field-name is a recognised MagazineIssue field name.
    ///     Accepts scalar field names AND <see cref="FieldMagazineId" /> (consumed only by
    ///     <see cref="CreateAsync" /> in addition mode \u2014 <see cref="ApplyAsync" /> still
    ///     rejects it because re-parenting an existing issue is admin-only) AND any junction
    ///     operation key matching <c>&lt;group&gt;.{add,remove}.&lt;token&gt;</c>.
    /// </summary>
    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;
        if(s_scalarFieldNames.Contains(fieldName)) return true;
        if(fieldName == FieldMagazineId) return true;
        return TryParseJunctionKey(fieldName, out _, out _, out _);
    }

    /// <summary>
    ///     Parse a junction operation field-name. Returns true on success and populates the
    ///     out parameters with the group identifier (e.g. <c>"people"</c>), the operation
    ///     (<c>"add"</c> or <c>"remove"</c>), and the token (a client-generated GUID for adds,
    ///     the existing row id stringified for removes).
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
    ///     Returns the current scalar values of the targeted MagazineIssue row. Junction
    ///     operation keys are not seeded in the current snapshot — the diff panel renders
    ///     junction ops on their own rows with the readable label resolved server-side.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        MagazineIssue mi = await context.MagazineIssues.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entityId);
        if(mi is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldCaption]            = mi.Caption,
            [FieldNativeCaption]      = mi.NativeCaption,
            [FieldPublished]          = mi.Published?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldPublishedPrecision] = (byte)mi.PublishedPrecision,
            [FieldProductCode]        = mi.ProductCode,
            [FieldPages]              = mi.Pages,
            [FieldIssueNumber]        = mi.IssueNumber,
            [FieldInternetArchiveUrl] = mi.InternetArchiveUrl,
            // Cover-pending field: write-only on the suggestion side. Seed the current
            // CoverGuid so the diff panel can show "current cover → new pending cover";
            // the extension lives in the pending sidecar so it isn't echoed here.
            [FieldCoverPendingGuid] = mi.CoverGuid?.ToString()
        };
    }

    /// <summary>
    ///     Create a brand-new MagazineIssue from an accepted suggestion (entity_id == null
    ///     path). Returns the new id + the field-name set that was actually applied. Returns
    ///     <c>(null, empty)</c> when creation can't proceed (e.g. admin didn't tick the
    ///     mandatory <c>caption</c> or <c>magazine_id</c> field, or the parent magazine no
    ///     longer exists). Junction <c>*.add.*</c> keys are applied in a second pass against
    ///     the freshly-minted <c>MagazineIssueId</c>; <c>*.remove.*</c> keys are silently
    ///     skipped (a brand-new entity has nothing to remove from). Cover-pending guid
    ///     promotion happens BEFORE persistence so the cover guid + extension are part of
    ///     the initial row.
    /// </summary>
    /// <param name="creditedUserId">
    ///     The Identity user id to attribute the row to in audit history (the suggesting
    ///     user, NOT the reviewing admin). Forwarded to <c>SaveChangesWithUserAsync</c>.
    /// </param>
    public static async Task<(long? newId, HashSet<string> applied)> CreateAsync(
        MarechaiContext context,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string creditedUserId,
        string assetRootPath)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        // Two mandatory fields: parent magazine_id (FK existence check) and caption (non-
        // whitespace). Reject the addition outright if either is missing or invalid.
        if(!accepted.Contains(FieldMagazineId) || !suggested.TryGetValue(FieldMagazineId, out object magIdRaw))
            return (null, applied);
        long? magIdParsed = ToLong(magIdRaw);
        if(!magIdParsed.HasValue || magIdParsed.Value <= 0) return (null, applied);
        long magazineId = magIdParsed.Value;
        if(!await context.Magazines.AsNoTracking().AnyAsync(m => m.Id == magazineId))
            return (null, applied);

        if(!accepted.Contains(FieldCaption) || !suggested.TryGetValue(FieldCaption, out object captionVal))
            return (null, applied);
        string caption = ToStringValue(captionVal);
        if(string.IsNullOrWhiteSpace(caption)) return (null, applied);

        var mi = new MagazineIssue
        {
            MagazineId = magazineId,
            Caption    = caption.Trim()
        };
        applied.Add(FieldMagazineId);
        applied.Add(FieldCaption);

        // Apply remaining accepted scalar fields via the same coercion+validation table the
        // edit path uses. Junction operations and the cover are handled in dedicated passes.
        foreach(string fieldName in accepted)
        {
            if(fieldName == FieldCaption) continue;
            if(fieldName == FieldMagazineId) continue;
            if(fieldName == FieldCoverPendingGuid) continue;
            if(!s_scalarFieldNames.Contains(fieldName)) continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyScalar(context, mi, fieldName, value)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this field.
            }
        }

        // Cover-pending: promote the file BEFORE saving so the new row carries the cover
        // guid + extension from the moment it's persisted. Failure here just skips the
        // cover; the rest of the entity still gets created.
        if(accepted.Contains(FieldCoverPendingGuid) &&
           suggested.TryGetValue(FieldCoverPendingGuid, out object coverGuidRaw))
        {
            string guidStr = ToStringValue(coverGuidRaw);
            if(!string.IsNullOrEmpty(guidStr) && Guid.TryParse(guidStr, out Guid pendingGuid))
            {
                if(await PromoteMagazineIssueCoverAsync(mi, assetRootPath, pendingGuid))
                    applied.Add(FieldCoverPendingGuid);
            }
        }

        await context.MagazineIssues.AddAsync(mi);

        if(string.IsNullOrEmpty(creditedUserId))
            await context.SaveChangesAsync();
        else
            await context.SaveChangesWithUserAsync(creditedUserId);

        // Now apply junction adds with the freshly-minted issue id. Remove keys are silently
        // ignored — a brand-new entity has nothing to remove from.
        foreach(string fieldName in accepted)
        {
            if(!TryParseJunctionKey(fieldName, out string group, out string op, out string _)) continue;
            if(op != "add") continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyJunctionAdd(context, mi.Id, group, value)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this junction add.
            }
        }

        return (mi.Id, applied);
    }

    /// <summary>
    ///     Apply the accepted fields onto the MagazineIssue row + junction tables. Each
    ///     junction operation is atomic — failure to coerce one entry skips it without
    ///     affecting the others.
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string assetRootPath)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        MagazineIssue mi = await context.MagazineIssues.FirstOrDefaultAsync(x => x.Id == entityId);
        if(mi is null) return (applied, true);

        bool scalarChanged = false;

        // ---- Cover promotion -----------------------------------------------------------
        // The applier reads the extension from the pending-image sidecar (no need for a
        // separate cover_pending_extension wire field). The move/process pipeline runs
        // BEFORE we touch any other scalars so a failure here doesn't half-apply the rest.
        if(accepted.Contains(FieldCoverPendingGuid) &&
           suggested.TryGetValue(FieldCoverPendingGuid, out object coverGuidRaw))
        {
            string guidStr = ToStringValue(coverGuidRaw);
            if(!string.IsNullOrEmpty(guidStr) && Guid.TryParse(guidStr, out Guid pendingGuid))
            {
                bool ok = await PromoteMagazineIssueCoverAsync(mi, assetRootPath, pendingGuid);
                if(ok)
                {
                    applied.Add(FieldCoverPendingGuid);
                    scalarChanged = true;
                }
            }
        }

        foreach(string fieldName in accepted)
        {
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            // Cover-pending field is handled above as an atomic unit; skip here so the
            // generic scalar dispatch doesn't try to apply it as a plain string.
            if(fieldName == FieldCoverPendingGuid)
                continue;

            try
            {
                if(s_scalarFieldNames.Contains(fieldName))
                {
                    if(await ApplyScalar(context, mi, fieldName, value))
                    {
                        applied.Add(fieldName);
                        scalarChanged = true;
                    }
                    continue;
                }

                if(TryParseJunctionKey(fieldName, out string group, out string op, out string token))
                {
                    bool ok = op == "add"
                                  ? await ApplyJunctionAdd(context, entityId, group, value)
                                  : await ApplyJunctionRemove(context, entityId, group, token);
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

    /// <summary>
    ///     Promote a pending cover guid into the issue's <c>originals/</c> folder, kick off
    ///     the conversion worker, and update the <see cref="MagazineIssue.CoverGuid" /> +
    ///     <see cref="MagazineIssue.OriginalCoverExtension" /> columns. Returns false on any
    ///     failure (missing pending file, IO error) so the caller does not record the field
    ///     as applied. Old cover files (if any) are deleted via
    ///     <see cref="DeleteMagazineIssueCoverFiles" /> after the new ones are persisted on
    ///     disk.
    /// </summary>
    static async Task<bool> PromoteMagazineIssueCoverAsync(MagazineIssue mi, string assetRootPath, Guid pendingGuid)
    {
        if(string.IsNullOrEmpty(assetRootPath)) return false;

        var (movedPath, extension) = await Marechai.Server.Helpers.PendingImageStore
                                                  .PromoteToOriginalsAsync(assetRootPath, "magazine-issue-covers",
                                                                           pendingGuid);
        if(movedPath is null || string.IsNullOrEmpty(extension)) return false;

        Guid? oldCoverGuid = mi.CoverGuid;
        mi.CoverGuid              = pendingGuid;
        mi.OriginalCoverExtension = extension;

        // Fire conversion worker in the background — same pattern as the admin upload path
        // (MagazineIssuesController.UploadCoverAsync). Generates AVIF/JXL/WebP/JPEG variants
        // at HD, 1440p, 4K resolutions plus thumbnails.
        _ = Task.Run(() =>
        {
            try
            {
                Marechai.Helpers.Photos.EnsureCreated(assetRootPath, false, "magazine-issue-covers");
                var photos = new Marechai.Helpers.Photos();
                photos.ConversionWorker(assetRootPath, pendingGuid, movedPath, extension, false,
                                        "magazine-issue-covers");
            }
            catch
            {
                // ignored — conversion can be retried later by a maintenance task; the
                // original is safe in originals/ either way.
            }
        });

        // Now that the new originals + (background-running) variants are in place, sweep the
        // old cover artefacts from disk so we don't leak storage.
        if(oldCoverGuid.HasValue && oldCoverGuid.Value != pendingGuid)
            DeleteMagazineIssueCoverFiles(assetRootPath, oldCoverGuid.Value);

        return true;
    }

    /// <summary>
    ///     Mirror of <c>MagazineIssuesController.DeleteCoverFiles</c> — sweep every
    ///     magazine-issue-cover artefact for a guid (originals + format/resolution variants
    ///     + thumbnails). Kept here so the applier doesn't take a hard dependency on the
    ///     controller.
    /// </summary>
    static void DeleteMagazineIssueCoverFiles(string assetRootPath, Guid coverGuid)
    {
        string photosRoot = System.IO.Path.Combine(assetRootPath, "photos", "magazine-issue-covers");
        string guidStr    = coverGuid.ToString();

        DeleteFilesByPattern(System.IO.Path.Combine(photosRoot, "originals"), guidStr + ".*");

        string[] formats     = ["jpeg", "webp", "avif", "jxl"];
        string[] resolutions = ["hd", "1440p", "4k"];

        foreach(string format in formats)
        {
            string ext = format switch
            {
                "jpeg" => ".jpg",
                "webp" => ".webp",
                "avif" => ".avif",
                "jxl"  => ".jxl",
                _      => "." + format
            };

            foreach(string res in resolutions)
            {
                string fullPath  = System.IO.Path.Combine(photosRoot, format,    res, guidStr + ext);
                string thumbPath = System.IO.Path.Combine(photosRoot, "thumbs", format, res, guidStr + ext);
                try { if(System.IO.File.Exists(fullPath))  System.IO.File.Delete(fullPath); }  catch { /* ignored */ }
                try { if(System.IO.File.Exists(thumbPath)) System.IO.File.Delete(thumbPath); } catch { /* ignored */ }
            }
        }
    }

    static void DeleteFilesByPattern(string directory, string pattern)
    {
        if(!System.IO.Directory.Exists(directory)) return;
        foreach(string file in System.IO.Directory.GetFiles(directory, pattern))
        {
            try { System.IO.File.Delete(file); } catch { /* ignored */ }
        }
    }

    // ───────────────────────────── Scalar-field application ─────────────────────────────

    static async Task<bool> ApplyScalar(MarechaiContext context, MagazineIssue mi, string fieldName, object value)
    {
        switch(fieldName)
        {
            case FieldCaption:
                string c = ToStringValue(value);
                if(string.IsNullOrWhiteSpace(c)) return false;
                mi.Caption = c.Trim();
                return true;
            case FieldNativeCaption:
                mi.NativeCaption = ToStringValue(value)?.Trim();
                return true;
            case FieldPublished:
                mi.Published = ToDate(value);
                return true;
            case FieldPublishedPrecision:
            {
                int? pv = ToInt(value);
                if(pv.HasValue && pv.Value is >= 0 and <= 2)
                {
                    mi.PublishedPrecision = (DatePrecision)pv.Value;
                    return true;
                }
                return false;
            }
            case FieldProductCode:
            {
                string pc = ToStringValue(value)?.Trim();
                if(string.IsNullOrEmpty(pc)) { mi.ProductCode = null; return true; }
                if(pc.Length > 18) return false;
                mi.ProductCode = pc;
                return true;
            }
            case FieldPages:
            {
                int? pv = ToInt(value);
                if(!pv.HasValue) { mi.Pages = null; return true; }
                if(pv.Value is < 0 or > short.MaxValue) return false;
                mi.Pages = (short)pv.Value;
                return true;
            }
            case FieldIssueNumber:
            {
                long? iv = ToLong(value);
                if(!iv.HasValue) { mi.IssueNumber = null; return true; }
                if(iv.Value is < 0 or > uint.MaxValue) return false;
                mi.IssueNumber = (uint)iv.Value;
                return true;
            }
            case FieldInternetArchiveUrl:
            {
                string url = ToStringValue(value)?.Trim();
                if(string.IsNullOrEmpty(url)) { mi.InternetArchiveUrl = null; return true; }
                if(url.Length > 2048) return false;
                mi.InternetArchiveUrl = url;
                return true;
            }
            case FieldCoverPendingGuid:
                // Cover-pending is handled atomically in ApplyAsync (move file + start
                // conversion + update guid+extension). Should never reach the generic
                // scalar dispatch — return false so a stray accept doesn't half-apply.
                return false;
            default:
                // magazine_id and any other unrecognised field is rejected.
                return false;
        }
    }

    // ───────────────────────────── Junction add ─────────────────────────────

    static async Task<bool> ApplyJunctionAdd(MarechaiContext context, long issueId, string group, object value)
    {
        Dictionary<string, object> payload = ExtractObject(value);
        if(payload is null) return false;

        switch(group)
        {
            case GroupPeople:
            {
                int?   pid    = GetInt(payload, "person_id");
                string roleId = GetString(payload, "role_id");
                if(!pid.HasValue || string.IsNullOrEmpty(roleId)) return false;
                if(!await context.People.AsNoTracking().AnyAsync(p => p.Id == pid.Value)) return false;
                if(!await context.DocumentRoles.AsNoTracking().AnyAsync(r => r.Id == roleId)) return false;
                // Dedup composite key (MagazineId, PersonId, RoleId): same person CAN be
                // added under different roles (author + translator) but never twice with the
                // same role. (MagazineId here is the FK column name — it actually points to
                // MagazineIssue.Id, NOT Magazine.Id.)
                if(await context.PeopleByMagazines.AsNoTracking()
                                .AnyAsync(r => r.MagazineId == issueId && r.PersonId == pid.Value && r.RoleId == roleId))
                    return false;
                await context.PeopleByMagazines.AddAsync(new PeopleByMagazine
                {
                    MagazineId = issueId,
                    PersonId   = pid.Value,
                    RoleId     = roleId
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupMachines:
            {
                int? mid = GetInt(payload, "machine_id");
                if(!mid.HasValue) return false;
                if(!await context.Machines.AsNoTracking().AnyAsync(m => m.Id == mid.Value)) return false;
                if(await context.MagazinesByMachines.AsNoTracking()
                                .AnyAsync(r => r.MagazineId == issueId && r.MachineId == mid.Value))
                    return false;
                await context.MagazinesByMachines.AddAsync(new MagazinesByMachine
                {
                    MagazineId = issueId,
                    MachineId  = mid.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupMachineFamilies:
            {
                int? fid = GetInt(payload, "machine_family_id");
                if(!fid.HasValue) return false;
                if(!await context.MachineFamilies.AsNoTracking().AnyAsync(f => f.Id == fid.Value)) return false;
                if(await context.MagazinesByMachinesFamilies.AsNoTracking()
                                .AnyAsync(r => r.MagazineId == issueId && r.MachineFamilyId == fid.Value))
                    return false;
                await context.MagazinesByMachinesFamilies.AddAsync(new MagazinesByMachineFamily
                {
                    MagazineId      = issueId,
                    MachineFamilyId = fid.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupSoftware:
            {
                long? sid = ToLong(GetRaw(payload, "software_id"));
                if(!sid.HasValue || sid.Value < 0) return false;
                ulong sidU = (ulong)sid.Value;
                if(!await context.Softwares.AsNoTracking().AnyAsync(s => s.Id == sidU)) return false;
                if(await context.MagazinesBySoftware.AsNoTracking()
                                .AnyAsync(r => r.MagazineId == issueId && r.SoftwareId == sidU))
                    return false;
                await context.MagazinesBySoftware.AddAsync(new MagazinesBySoftware
                {
                    MagazineId = issueId,
                    SoftwareId = sidU
                });
                await context.SaveChangesAsync();
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction remove ─────────────────────────────

    static async Task<bool> ApplyJunctionRemove(MarechaiContext context, long issueId, string group, string token)
    {
        if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) return false;

        switch(group)
        {
            case GroupPeople:
                return await context.PeopleByMagazines
                                    .Where(r => r.Id == rowId && r.MagazineId == issueId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupMachines:
                return await context.MagazinesByMachines
                                    .Where(r => r.Id == rowId && r.MagazineId == issueId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupMachineFamilies:
                return await context.MagazinesByMachinesFamilies
                                    .Where(r => r.Id == rowId && r.MagazineId == issueId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupSoftware:
                return await context.MagazinesBySoftware
                                    .Where(r => r.Id == rowId && r.MagazineId == issueId)
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

    static string GetString(Dictionary<string, object> payload, string key) =>
        payload.TryGetValue(key, out object v) ? ToStringValue(v)?.Trim() : null;

    static object GetRaw(Dictionary<string, object> payload, string key) =>
        payload.TryGetValue(key, out object v) ? v : null;

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
            ulong u when u <= long.MaxValue => (long)u,
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
