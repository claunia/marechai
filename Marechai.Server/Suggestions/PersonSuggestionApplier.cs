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
using System.Text.Json;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Suggestions;

/// <summary>
///     Server-side counterpart to <c>Marechai.Suggestions.Metadata.PersonSuggestionMetadata</c>.
///     Handles scalar Person fields plus the pseudo-field <c>cover_pending_guid</c> which
///     promotes a previously-uploaded pending photo into the originals folder. The five
///     Person junctions (<c>PeopleByCompany</c>, <c>PeopleByBook</c>, <c>PeopleByDocument</c>,
///     <c>PeopleByMagazine</c>, <c>PeopleBySoftware</c>) stay admin-only per the established
///     convention — they are edited from the other entity's view when that side ports
///     collaborative entity-edit suggestions. <see cref="Person.Id" /> is <c>int</c> so the
///     applier casts the incoming <c>long entityId</c> on every EF query.
/// </summary>
internal static class PersonSuggestionApplier
{
    // ---- Scalar field names (MUST mirror the client-side metadata constants) -----------
    public const string FieldName               = "name";
    public const string FieldSurname            = "surname";
    public const string FieldAlias              = "alias";
    public const string FieldDisplayName        = "display_name";
    public const string FieldCountryOfBirthId   = "country_of_birth_id";
    public const string FieldBirthDate          = "birth_date";
    public const string FieldBirthDatePrecision = "birth_date_precision";
    public const string FieldDeathDate          = "death_date";
    public const string FieldDeathDatePrecision = "death_date_precision";
    public const string FieldWebpage            = "webpage";
    public const string FieldTwitter            = "twitter";
    public const string FieldFacebook           = "facebook";
    public const string FieldCoverPendingGuid   = "cover_pending_guid";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldName, FieldSurname, FieldAlias, FieldDisplayName, FieldCountryOfBirthId,
        FieldBirthDate, FieldBirthDatePrecision, FieldDeathDate, FieldDeathDatePrecision,
        FieldWebpage, FieldTwitter, FieldFacebook,
        FieldCoverPendingGuid
    };

    /// <summary>
    ///     Returns <c>true</c> when the field-name is a recognised Person scalar field name.
    ///     Person has no in-scope junctions so there is no junction-key parser.
    /// </summary>
    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;
        return s_scalarFieldNames.Contains(fieldName);
    }

    /// <summary>
    ///     Returns the current scalar values of the targeted Person row.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        Person p = await context.People.AsNoTracking().FirstOrDefaultAsync(x => x.Id == (int)entityId);
        if(p is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldName]               = p.Name,
            [FieldSurname]            = p.Surname,
            [FieldAlias]              = p.Alias,
            [FieldDisplayName]        = p.DisplayName,
            [FieldCountryOfBirthId]   = p.CountryOfBirthId,
            [FieldBirthDate]          = p.BirthDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldBirthDatePrecision] = (byte)p.BirthDatePrecision,
            [FieldDeathDate]          = p.DeathDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            [FieldDeathDatePrecision] = (byte)p.DeathDatePrecision,
            [FieldWebpage]            = p.Webpage,
            [FieldTwitter]            = p.Twitter,
            [FieldFacebook]           = p.Facebook,
            // Cover-pending field: write-only on the suggestion side. Seed the current
            // Photo guid so the diff panel can show "current photo → new pending photo";
            // the extension lives in the pending sidecar so it isn't echoed here.
            [FieldCoverPendingGuid] = p.Photo == Guid.Empty ? null : p.Photo.ToString()
        };
    }

    /// <summary>
    ///     Create a brand-new Person row from a suggestion. Both <c>name</c> and
    ///     <c>surname</c> must be present in <paramref name="accepted" /> and non-empty;
    ///     every other accepted scalar is then applied via the same coercion+validation
    ///     table the edit path uses. Person has zero in-scope junctions so there is no
    ///     second-pass junction loop. Photo promotion happens BEFORE the row is persisted
    ///     so the cover guid + extension are part of the initial save.
    /// </summary>
    /// <param name="creditedUserId">
    ///     The Identity user id to attribute the row to in audit history (the suggesting
    ///     user, NOT the reviewing admin). Forwarded to <c>SaveChangesWithUserAsync</c>.
    /// </param>
    public static async Task<(int? newId, HashSet<string> applied)> CreateAsync(
        MarechaiContext context,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string creditedUserId,
        string assetRootPath)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        // Name + Surname are mandatory; everything else (alias, dates, country, contact
        // links, photo) is optional. Reject the addition outright if the admin didn't
        // tick both mandatory fields with non-empty values.
        if(!accepted.Contains(FieldName) || !suggested.TryGetValue(FieldName, out object nameVal))
            return (null, applied);
        string name = ToStringValue(nameVal);
        if(string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100) return (null, applied);

        if(!accepted.Contains(FieldSurname) || !suggested.TryGetValue(FieldSurname, out object surnameVal))
            return (null, applied);
        string surname = ToStringValue(surnameVal);
        if(string.IsNullOrWhiteSpace(surname) || surname.Trim().Length > 100) return (null, applied);

        var p = new Person { Name = name.Trim(), Surname = surname.Trim() };
        applied.Add(FieldName);
        applied.Add(FieldSurname);

        // Apply remaining accepted scalar fields via the same coercion+validation table the
        // edit path uses. The cover-pending field is handled in a dedicated pass below.
        foreach(string fieldName in accepted)
        {
            if(fieldName == FieldName) continue;
            if(fieldName == FieldSurname) continue;
            if(fieldName == FieldCoverPendingGuid) continue;
            if(!s_scalarFieldNames.Contains(fieldName)) continue;
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            try
            {
                if(await ApplyScalar(context, p, fieldName, value)) applied.Add(fieldName);
            }
            catch
            {
                // Coerce failure: silently skip this field.
            }
        }

        // Cover-pending: promote the file BEFORE saving so the new row carries the photo
        // guid + extension from the moment it's persisted. Failure here just skips the
        // photo; the rest of the entity still gets created.
        if(accepted.Contains(FieldCoverPendingGuid) &&
           suggested.TryGetValue(FieldCoverPendingGuid, out object coverGuidRaw))
        {
            string guidStr = ToStringValue(coverGuidRaw);
            if(!string.IsNullOrEmpty(guidStr) && Guid.TryParse(guidStr, out Guid pendingGuid))
            {
                if(await PromotePersonPhotoAsync(p, assetRootPath, pendingGuid))
                    applied.Add(FieldCoverPendingGuid);
            }
        }

        await context.People.AddAsync(p);

        if(string.IsNullOrEmpty(creditedUserId))
            await context.SaveChangesAsync();
        else
            await context.SaveChangesWithUserAsync(creditedUserId);

        // Person has no in-scope junctions — the five junctions (PeopleByCompany,
        // PeopleByBook, PeopleByDocument, PeopleByMagazine, PeopleBySoftware) are owned
        // by the other side per the established convention. No second-pass loop here.
        return (p.Id, applied);
    }

    /// <summary>
    ///     Apply the accepted fields onto the Person row. Cover promotion runs first as an
    ///     atomic unit; scalar fields are batched into a single SaveChanges at the end.
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string assetRootPath)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        Person p = await context.People.FirstOrDefaultAsync(x => x.Id == (int)entityId);
        if(p is null) return (applied, true);

        bool scalarChanged = false;

        // ---- Cover promotion -----------------------------------------------------------
        if(accepted.Contains(FieldCoverPendingGuid) &&
           suggested.TryGetValue(FieldCoverPendingGuid, out object coverGuidRaw))
        {
            string guidStr = ToStringValue(coverGuidRaw);
            if(!string.IsNullOrEmpty(guidStr) && Guid.TryParse(guidStr, out Guid pendingGuid))
            {
                bool ok = await PromotePersonPhotoAsync(p, assetRootPath, pendingGuid);
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
                    if(await ApplyScalar(context, p, fieldName, value))
                    {
                        applied.Add(fieldName);
                        scalarChanged = true;
                    }
                }
            }
            catch
            {
                // Coerce failure: silently skip this field.
            }
        }

        if(scalarChanged) await context.SaveChangesAsync();

        return (applied, false);
    }

    // ───────────────────────────── Photo promotion ─────────────────────────────

    static async Task<bool> PromotePersonPhotoAsync(Person p, string assetRootPath, Guid pendingGuid)
    {
        if(string.IsNullOrEmpty(assetRootPath)) return false;

        var (movedPath, extension) = await Marechai.Server.Helpers.PendingImageStore
                                                  .PromoteToOriginalsAsync(assetRootPath, "people", pendingGuid);
        if(movedPath is null || string.IsNullOrEmpty(extension)) return false;

        Guid oldPhotoGuid = p.Photo;
        p.Photo                  = pendingGuid;
        p.OriginalPhotoExtension = extension;

        // Fire conversion worker in the background — same pattern as the admin upload path.
        _ = Task.Run(() =>
        {
            try
            {
                Marechai.Helpers.Photos.EnsureCreated(assetRootPath, false, "people");
                var photos = new Marechai.Helpers.Photos();
                photos.ConversionWorker(assetRootPath, pendingGuid, movedPath, extension, false, "people");
            }
            catch
            {
                // ignored — conversion can be retried later; original is safe in originals/.
            }
        });

        if(oldPhotoGuid != Guid.Empty && oldPhotoGuid != pendingGuid)
            DeletePersonPhotoFiles(assetRootPath, oldPhotoGuid);

        return true;
    }

    /// <summary>
    ///     Sweep every person-photo artefact for a guid (originals + format/resolution
    ///     variants + thumbnails). Kept here so the applier doesn't take a hard dependency
    ///     on the controller. Exposed as <c>internal</c> so <see cref="Marechai.Server.Controllers.PeopleController"/>
    ///     can reuse it from the admin upload/delete endpoints (single source of truth for
    ///     the variant filename list).
    /// </summary>
    internal static void DeletePersonPhotoFiles(string assetRootPath, Guid photoGuid)
    {
        string photosRoot = System.IO.Path.Combine(assetRootPath, "photos", "people");
        string guidStr    = photoGuid.ToString();

        DeleteFilesByPattern(System.IO.Path.Combine(photosRoot, "originals"), guidStr + ".*");

        string[] formats     = ["jpeg", "webp", "avif"];
        string[] resolutions = ["4k"];

        foreach(string format in formats)
        {
            string ext = format switch
            {
                "jpeg" => ".jpg",
                "webp" => ".webp",
                "avif" => ".avif",
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

    static async Task<bool> ApplyScalar(MarechaiContext context, Person p, string fieldName, object value)
    {
        switch(fieldName)
        {
            case FieldName:
            {
                string n = ToStringValue(value);
                if(string.IsNullOrWhiteSpace(n)) return false;
                if(n.Length > 100) return false;
                p.Name = n.Trim();
                return true;
            }
            case FieldSurname:
            {
                string n = ToStringValue(value);
                if(string.IsNullOrWhiteSpace(n)) return false;
                if(n.Length > 100) return false;
                p.Surname = n.Trim();
                return true;
            }
            case FieldAlias:
            {
                string a = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(a) && a.Length > 100) return false;
                p.Alias = string.IsNullOrEmpty(a) ? null : a;
                return true;
            }
            case FieldDisplayName:
            {
                string d = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(d) && d.Length > 100) return false;
                p.DisplayName = string.IsNullOrEmpty(d) ? null : d;
                return true;
            }
            case FieldCountryOfBirthId:
            {
                short? cid = ToShort(value);
                if(!cid.HasValue) { p.CountryOfBirthId = null; return true; }
                if(!await context.Iso31661Numeric.AsNoTracking().AnyAsync(c => c.Id == cid.Value)) return false;
                p.CountryOfBirthId = cid.Value;
                return true;
            }
            case FieldBirthDate:
            {
                DateTime? d = ToDate(value);
                if(!d.HasValue) return false;
                p.BirthDate = d.Value;
                return true;
            }
            case FieldBirthDatePrecision:
            {
                int? pv = ToInt(value);
                if(pv.HasValue && pv.Value is >= 0 and <= 2)
                {
                    p.BirthDatePrecision = (DatePrecision)pv.Value;
                    return true;
                }
                return false;
            }
            case FieldDeathDate:
                p.DeathDate = ToDate(value);
                return true;
            case FieldDeathDatePrecision:
            {
                int? pv = ToInt(value);
                if(pv.HasValue && pv.Value is >= 0 and <= 2)
                {
                    p.DeathDatePrecision = (DatePrecision)pv.Value;
                    return true;
                }
                return false;
            }
            case FieldWebpage:
            {
                string w = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(w) && w.Length > 255) return false;
                p.Webpage = string.IsNullOrEmpty(w) ? null : w;
                return true;
            }
            case FieldTwitter:
            {
                string t = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(t) && t.Length > 50) return false;
                p.Twitter = string.IsNullOrEmpty(t) ? null : t;
                return true;
            }
            case FieldFacebook:
            {
                string f = ToStringValue(value)?.Trim();
                if(!string.IsNullOrEmpty(f) && f.Length > 100) return false;
                p.Facebook = string.IsNullOrEmpty(f) ? null : f;
                return true;
            }
            default:
                return false;
        }
    }

    // ───────────────────────────── Coercion helpers ─────────────────────────────

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

    static short? ToShort(object v)
    {
        return v switch
        {
            null            => null,
            short s         => s,
            int i           => (short?)i,
            long l          => (short?)l,
            byte b          => b,
            JsonElement je  => je.ValueKind switch
            {
                JsonValueKind.Number => je.TryGetInt16(out short s) ? s : null,
                JsonValueKind.String => short.TryParse(je.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out short p) ? p : null,
                _                    => null
            },
            string str      => short.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out short p) ? p : null,
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
