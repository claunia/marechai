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
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.BookSuggestionMetadata</c>.
///     Handles BOTH scalar Book fields AND junction add/remove operations across the 4
///     suggestable junction tables (People, Companies, Machines, MachineFamilies). Each
///     junction operation is atomic — its full row payload is accepted or rejected as a
///     single unit. Edits to existing junction rows are modelled as a <c>remove</c> +
///     <c>add</c> pair which the admin can accept independently. Mirrors the design of
///     <see cref="MachineSuggestionApplier" /> with two key differences: Book.Id is
///     <c>long</c> (not <c>int</c>), and People/Companies junctions carry a required
///     <c>RoleId</c> attribute.
/// </summary>
internal static class BookSuggestionApplier
{
    // ---- Scalar field names (MUST mirror the client-side metadata constants) -----------
    public const string FieldTitle              = "title";
    public const string FieldNativeTitle        = "native_title";
    public const string FieldSortTitle          = "sort_title";
    public const string FieldIsbn               = "isbn";
    public const string FieldPages              = "pages";
    public const string FieldEdition            = "edition";
    public const string FieldPublished          = "published";
    public const string FieldPublishedPrecision = "published_precision";
    public const string FieldCountryId          = "country_id";
    public const string FieldPreviousId         = "previous_id";
    public const string FieldSourceId           = "source_id";
    public const string FieldInternetArchiveUrl    = "internet_archive_url";

    /// <summary>
    ///     Pseudo-field carrying the guid of a pending-cover upload that should replace
    ///     <see cref="Book.CoverGuid" /> when this suggestion is accepted. The corresponding
    ///     image file lives under <c>book-covers/pending/&lt;guid&gt;.&lt;ext&gt;</c> with a
    ///     sidecar JSON file recording the extension and uploader. On accept,
    ///     <see cref="Marechai.Server.Helpers.PendingImageStore.PromoteToOriginalsAsync" /> moves
    ///     the file into <c>book-covers/originals/</c> (extension comes from the sidecar) and
    ///     the existing <see cref="Marechai.Helpers.Photos.ConversionWorker" /> generates the
    ///     AVIF/JXL/WebP/JPEG variants.
    /// </summary>
    public const string FieldCoverPendingGuid      = "cover_pending_guid";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldTitle, FieldNativeTitle, FieldSortTitle, FieldIsbn, FieldPages, FieldEdition,
        FieldPublished, FieldPublishedPrecision, FieldCountryId, FieldPreviousId,
        FieldSourceId, FieldInternetArchiveUrl,
        FieldCoverPendingGuid
    };

    // ---- Junction group identifiers (lowercase snake_case prefix in field-name keys) ---
    public const string GroupPeople          = "people";
    public const string GroupCompanies       = "companies";
    public const string GroupMachines        = "machines";
    public const string GroupMachineFamilies = "machine_families";

    public static readonly IReadOnlyCollection<string> JunctionGroups = new[]
    {
        GroupPeople, GroupCompanies, GroupMachines, GroupMachineFamilies
    };

    /// <summary>
    ///     Returns <c>true</c> when the field-name is a recognised Book field name. Accepts
    ///     scalar field names AND any junction operation key matching
    ///     <c>&lt;group&gt;.{add,remove}.&lt;token&gt;</c>.
    /// </summary>
    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;
        if(s_scalarFieldNames.Contains(fieldName)) return true;
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
    ///     Returns the current scalar values of the targeted Book row. Junction operation
    ///     keys are not seeded in the current snapshot — the diff panel renders junction ops
    ///     on their own rows with the readable label resolved server-side.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        Book b = await context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entityId);
        if(b is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldTitle]              = b.Title,
            [FieldNativeTitle]        = b.NativeTitle,
            [FieldSortTitle]          = b.SortTitle,
            [FieldIsbn]               = b.Isbn,
            [FieldPages]              = b.Pages,
            [FieldEdition]            = b.Edition,
            [FieldPublished]          = b.Published?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldPublishedPrecision] = (byte)b.PublishedPrecision,
            [FieldCountryId]          = b.CountryId,
            [FieldPreviousId]         = b.PreviousId,
            [FieldSourceId]           = b.SourceId,
            [FieldInternetArchiveUrl] = b.InternetArchiveUrl,
            // Cover-pending field: write-only on the suggestion side. Seed the current
            // CoverGuid so the diff panel can show "current cover → new pending cover";
            // the extension lives in the pending sidecar so it isn't echoed here.
            [FieldCoverPendingGuid] = b.CoverGuid?.ToString()
        };
    }

    /// <summary>
    ///     Create a brand-new Book row from an accepted addition-mode suggestion. Returns
    ///     the new entity id (or <c>null</c> on failure to validate the mandatory
    ///     <see cref="FieldTitle" />), plus the actually-applied field set. After scalar
    ///     fields are persisted, accepted <c>*.add.*</c> junction keys are applied with
    ///     the freshly-minted <c>BookId</c>; <c>*.remove.*</c> keys are silently skipped
    ///     (a brand-new entity has nothing to remove from). Cover-pending guid promotion
    ///     happens BEFORE persistence so the cover guid + extension are part of the
    ///     initial row.
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

        // Title is the only mandatory field; everything else (ISBN, dates, country, FK
        // references, junctions, cover) is optional. Reject the addition outright if the
        // admin didn't tick Title.
        if(!accepted.Contains(FieldTitle) || !suggested.TryGetValue(FieldTitle, out object titleVal))
            return (null, applied);

        string title = ToStringValue(titleVal);
        if(string.IsNullOrWhiteSpace(title)) return (null, applied);

        var b = new Book { Title = title.Trim() };
        applied.Add(FieldTitle);

        // Apply remaining accepted scalar fields via the same coercion+validation table the
        // edit path uses. Junction operations and the cover are handled in dedicated passes.
        foreach(string fieldName in accepted)
        {
            if(fieldName == FieldTitle) continue;
            if(fieldName == FieldCoverPendingGuid) continue;
            if(!s_scalarFieldNames.Contains(fieldName)) continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyScalar(context, b, fieldName, value)) applied.Add(fieldName);
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
                if(await PromoteBookCoverAsync(b, assetRootPath, pendingGuid))
                    applied.Add(FieldCoverPendingGuid);
            }
        }

        await context.Books.AddAsync(b);

        if(string.IsNullOrEmpty(creditedUserId))
            await context.SaveChangesAsync();
        else
            await context.SaveChangesWithUserAsync(creditedUserId);

        // Now apply junction adds with the freshly-minted book id. Remove keys are silently
        // ignored — a brand-new entity has nothing to remove from.
        foreach(string fieldName in accepted)
        {
            if(!TryParseJunctionKey(fieldName, out string group, out string op, out string _)) continue;
            if(op != "add") continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyJunctionAdd(context, b.Id, group, value)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this junction add.
            }
        }

        return (b.Id, applied);
    }

    /// <summary>
    ///     Apply the accepted fields onto the Book row + junction tables. Each junction
    ///     operation is atomic — failure to coerce one entry skips it without affecting the
    ///     others.
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string assetRootPath)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        Book b = await context.Books.FirstOrDefaultAsync(x => x.Id == entityId);
        if(b is null) return (applied, true);

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
                bool ok = await PromoteBookCoverAsync(b, assetRootPath, pendingGuid);
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
                    if(await ApplyScalar(context, b, fieldName, value))
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
    ///     Promote a pending cover guid into the book's <c>originals/</c> folder, kick off the
    ///     conversion worker, and update the <see cref="Book.CoverGuid" /> +
    ///     <see cref="Book.OriginalCoverExtension" /> columns. Returns false on any failure
    ///     (missing pending file, IO error) so the caller does not record the field as
    ///     applied. Old book cover files (if any) are deleted via
    ///     <see cref="DeleteBookCoverFiles" /> after the new ones are persisted on disk.
    /// </summary>
    static async Task<bool> PromoteBookCoverAsync(Book b, string assetRootPath, Guid pendingGuid)
    {
        if(string.IsNullOrEmpty(assetRootPath)) return false;

        var (movedPath, extension) = await Marechai.Server.Helpers.PendingImageStore
                                                  .PromoteToOriginalsAsync(assetRootPath, "book-covers", pendingGuid);
        if(movedPath is null || string.IsNullOrEmpty(extension)) return false;

        Guid? oldCoverGuid = b.CoverGuid;
        b.CoverGuid              = pendingGuid;
        b.OriginalCoverExtension = extension;

        // Fire conversion worker in the background — same pattern as the admin upload path
        // (BooksController.UploadCoverAsync). Generates AVIF/JXL/WebP/JPEG variants at HD,
        // 1440p, 4K resolutions plus thumbnails.
        _ = Task.Run(() =>
        {
            try
            {
                Marechai.Helpers.Photos.EnsureCreated(assetRootPath, false, "book-covers");
                var photos = new Marechai.Helpers.Photos();
                photos.ConversionWorker(assetRootPath, pendingGuid, movedPath, extension, false, "book-covers");
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
            DeleteBookCoverFiles(assetRootPath, oldCoverGuid.Value);

        return true;
    }

    /// <summary>
    ///     Mirror of <c>BooksController.DeleteCoverFiles</c> — sweep every book-cover artefact
    ///     for a guid (originals + format/resolution variants + thumbnails). Kept here so the
    ///     applier doesn't take a hard dependency on the controller.
    /// </summary>
    static void DeleteBookCoverFiles(string assetRootPath, Guid coverGuid)
    {
        string photosRoot = System.IO.Path.Combine(assetRootPath, "photos", "book-covers");
        string guidStr    = coverGuid.ToString();

        DeleteFilesByPattern(System.IO.Path.Combine(photosRoot, "originals"), guidStr + ".*");

        string[] formats     = ["jpeg", "webp", "avif", "jxl"];
        string[] resolutions = ["4k"];

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

    static async Task<bool> ApplyScalar(MarechaiContext context, Book b, string fieldName, object value)
    {
        switch(fieldName)
        {
            case FieldTitle:
                string t = ToStringValue(value);
                if(string.IsNullOrWhiteSpace(t)) return false;
                b.Title = t.Trim();
                return true;
            case FieldNativeTitle:
                b.NativeTitle = ToStringValue(value)?.Trim();
                return true;
            case FieldSortTitle:
                b.SortTitle = ToStringValue(value)?.Trim();
                return true;
            case FieldIsbn:
            {
                string isbn = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(isbn) && (isbn.Length < 10 || isbn.Length > 13)) return false;
                b.Isbn = string.IsNullOrEmpty(isbn) ? null : isbn;
                return true;
            }
            case FieldPages:
            {
                int? pv = ToInt(value);
                if(!pv.HasValue) { b.Pages = null; return true; }
                if(pv.Value is < 0 or > short.MaxValue) return false;
                b.Pages = (short)pv.Value;
                return true;
            }
            case FieldEdition:
            {
                int? ev = ToInt(value);
                if(ev.HasValue && ev.Value < 0) return false;
                b.Edition = ev;
                return true;
            }
            case FieldPublished:
                b.Published = ToDate(value);
                return true;
            case FieldPublishedPrecision:
            {
                int? pv = ToInt(value);
                if(pv.HasValue && pv.Value is >= 0 and <= 2)
                {
                    b.PublishedPrecision = (DatePrecision)pv.Value;
                    return true;
                }
                return false;
            }
            case FieldCountryId:
            {
                int? raw = ToInt(value);
                if(!raw.HasValue) { b.CountryId = null; return true; }
                if(raw.Value is < short.MinValue or > short.MaxValue) return false;
                short cid = (short)raw.Value;
                if(!await context.Iso31661Numeric.AsNoTracking().AnyAsync(c => c.Id == cid)) return false;
                b.CountryId = cid;
                return true;
            }
            case FieldPreviousId:
            {
                long? pid = ToLong(value);
                if(!pid.HasValue) { b.PreviousId = null; return true; }
                if(pid.Value == b.Id) return false; // prevent self-reference
                if(!await context.Books.AsNoTracking().AnyAsync(x => x.Id == pid.Value)) return false;
                b.PreviousId = pid.Value;
                return true;
            }
            case FieldSourceId:
            {
                long? sid = ToLong(value);
                if(!sid.HasValue) { b.SourceId = null; return true; }
                if(sid.Value == b.Id) return false; // prevent self-reference
                if(!await context.Books.AsNoTracking().AnyAsync(x => x.Id == sid.Value)) return false;
                b.SourceId = sid.Value;
                return true;
            }
            case FieldInternetArchiveUrl:
            {
                string url = ToStringValue(value)?.Trim();
                if(string.IsNullOrEmpty(url)) { b.InternetArchiveUrl = null; return true; }
                if(url.Length > 2048) return false;
                b.InternetArchiveUrl = url;
                return true;
            }
            case FieldCoverPendingGuid:
                // Cover-pending is handled atomically in ApplyAsync (move file + start
                // conversion + update guid+extension). Should never reach the generic
                // scalar dispatch — return false so a stray accept doesn't half-apply.
                return false;
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction add ─────────────────────────────

    static async Task<bool> ApplyJunctionAdd(MarechaiContext context, long bookId, string group, object value)
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
                // Dedup composite key (BookId, PersonId, RoleId): same person CAN be added under
                // different roles (author + translator) but never twice with the same role.
                if(await context.PeopleByBooks.AsNoTracking()
                                .AnyAsync(r => r.BookId == bookId && r.PersonId == pid.Value && r.RoleId == roleId))
                    return false;
                await context.PeopleByBooks.AddAsync(new PeopleByBook
                {
                    BookId   = bookId,
                    PersonId = pid.Value,
                    RoleId   = roleId
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupCompanies:
            {
                int?   cid    = GetInt(payload, "company_id");
                string roleId = GetString(payload, "role_id");
                if(!cid.HasValue || string.IsNullOrEmpty(roleId)) return false;
                if(!await context.Companies.AsNoTracking().AnyAsync(c => c.Id == cid.Value)) return false;
                if(!await context.DocumentRoles.AsNoTracking().AnyAsync(r => r.Id == roleId)) return false;
                if(await context.CompaniesByBooks.AsNoTracking()
                                .AnyAsync(r => r.BookId == bookId && r.CompanyId == cid.Value && r.RoleId == roleId))
                    return false;
                await context.CompaniesByBooks.AddAsync(new CompaniesByBook
                {
                    BookId    = bookId,
                    CompanyId = cid.Value,
                    RoleId    = roleId
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupMachines:
            {
                int? mid = GetInt(payload, "machine_id");
                if(!mid.HasValue) return false;
                if(!await context.Machines.AsNoTracking().AnyAsync(m => m.Id == mid.Value)) return false;
                if(await context.BooksByMachines.AsNoTracking()
                                .AnyAsync(r => r.BookId == bookId && r.MachineId == mid.Value))
                    return false;
                await context.BooksByMachines.AddAsync(new BooksByMachine
                {
                    BookId    = bookId,
                    MachineId = mid.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            case GroupMachineFamilies:
            {
                int? fid = GetInt(payload, "machine_family_id");
                if(!fid.HasValue) return false;
                if(!await context.MachineFamilies.AsNoTracking().AnyAsync(f => f.Id == fid.Value)) return false;
                if(await context.BooksByMachineFamilies.AsNoTracking()
                                .AnyAsync(r => r.BookId == bookId && r.MachineFamilyId == fid.Value))
                    return false;
                await context.BooksByMachineFamilies.AddAsync(new BooksByMachineFamily
                {
                    BookId          = bookId,
                    MachineFamilyId = fid.Value
                });
                await context.SaveChangesAsync();
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Junction remove ─────────────────────────────

    static async Task<bool> ApplyJunctionRemove(MarechaiContext context, long bookId, string group, string token)
    {
        if(!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long rowId)) return false;

        switch(group)
        {
            case GroupPeople:
                return await context.PeopleByBooks
                                    .Where(r => r.Id == rowId && r.BookId == bookId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupCompanies:
                return await context.CompaniesByBooks
                                    .Where(r => r.Id == rowId && r.BookId == bookId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupMachines:
                return await context.BooksByMachines
                                    .Where(r => r.Id == rowId && r.BookId == bookId)
                                    .ExecuteDeleteAsync() > 0;
            case GroupMachineFamilies:
                return await context.BooksByMachineFamilies
                                    .Where(r => r.Id == rowId && r.BookId == bookId)
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
