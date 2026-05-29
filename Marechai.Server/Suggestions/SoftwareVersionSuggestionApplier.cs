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
///     Server-side applier for brand-new <see cref="SoftwareVersion" /> creation
///     suggestions. Mirrors the combined parent+child atomic-creation pattern proven by
///     <see cref="SoftwareSuggestionApplier" />: a SoftwareVersion has no useful surface
///     without a SoftwareRelease, so every creation submission MUST carry a first-release
///     half prefixed with <see cref="FirstReleaseScalarPrefix" /> / <see cref="FirstReleaseGroupPrefix" />,
///     and both rows are inserted atomically inside an EF transaction. Acceptance of the
///     mandatory fields of EITHER half is required for the whole suggestion to apply; if
///     the release applier returns null the version is rolled back as well.
///
///     <para>
///         Edit-mode suggestions for an existing SoftwareVersion are out of scope. The
///         <see cref="ApplyAsync" /> stub returns <c>(empty, false)</c> so the dispatch
///         switch in <c>SuggestionsController</c> stays exhaustive while no edit-mode UI
///         exposes this entity type.
///     </para>
///
///     <para>
///         Re-parenting fields (<see cref="FieldSoftwareId" /> / <c>parent_version_id</c>
///         used as a re-parent on edit) stay admin-only: <c>software_id</c> is whitelisted
///         in <see cref="IsKnownFieldName" /> but NOT in <c>s_scalarFieldNames</c>, so the
///         creation path picks it up while the (currently stubbed) edit path would ignore
///         it.
///     </para>
/// </summary>
internal static class SoftwareVersionSuggestionApplier
{
    // ---- Scalar field names (mirror client-side metadata) -----------------------------
    public const string FieldVersionString   = "version_string";
    public const string FieldPublicVersion   = "public_version";
    public const string FieldCodename        = "codename";
    public const string FieldParentVersionId = "parent_version_id";
    public const string FieldLicenseId       = "license_id";

    static readonly HashSet<string> s_scalarFieldNames = new(StringComparer.Ordinal)
    {
        FieldVersionString,
        FieldPublicVersion,
        FieldCodename,
        FieldParentVersionId,
        FieldLicenseId
    };

    /// <summary>
    ///     Pseudo-field carrying the parent <see cref="Software" /> FK at addition time
    ///     only (consumed by <see cref="CreateAsync" />). Intentionally NOT in
    ///     <c>s_scalarFieldNames</c>: edit-mode re-parenting between Softwares stays
    ///     admin-only. Whitelisted in <see cref="IsKnownFieldName" /> so the controller's
    ///     field-name validator accepts it on the wire.
    /// </summary>
    public const string FieldSoftwareId = "software_id";

    // ---- First-release pseudo-prefixes (mirror server-side; creation-mode only) --------
    /// <summary>
    ///     Field-name prefix for first-release SCALAR fields embedded in a SoftwareVersion
    ///     creation payload (e.g. <c>first_release_title</c>). Stripped before delegating
    ///     to <see cref="SoftwareReleaseSuggestionApplier" />.
    /// </summary>
    public const string FirstReleaseScalarPrefix = "first_release_";

    /// <summary>
    ///     Field-name prefix for first-release JUNCTION operation keys embedded in a
    ///     SoftwareVersion creation payload (e.g.
    ///     <c>first_release.regions.add.&lt;uuid&gt;</c>). The dot delimits the original
    ///     <c>&lt;group&gt;.&lt;op&gt;.&lt;token&gt;</c> shape and survives the strip
    ///     verbatim.
    /// </summary>
    public const string FirstReleaseGroupPrefix = "first_release.";

    public static bool IsKnownFieldName(string fieldName)
    {
        if(string.IsNullOrEmpty(fieldName)) return false;
        if(s_scalarFieldNames.Contains(fieldName)) return true;
        if(fieldName == FieldSoftwareId) return true;

        // Delegate first-release-prefixed keys to the release applier. The dot-prefix
        // ("first_release.") must be checked BEFORE the scalar prefix ("first_release_")
        // — they share the leading "first_release" run and the underscore comes second,
        // so a "first_release.regions.add.X" key would also match the underscore-prefix
        // text check if we tried that first. Test dot first.
        if(fieldName.StartsWith(FirstReleaseGroupPrefix, StringComparison.Ordinal))
            return SoftwareReleaseSuggestionApplier.IsKnownFieldName(fieldName.Substring(FirstReleaseGroupPrefix.Length));
        if(fieldName.StartsWith(FirstReleaseScalarPrefix, StringComparison.Ordinal))
            return SoftwareReleaseSuggestionApplier.IsKnownFieldName(fieldName.Substring(FirstReleaseScalarPrefix.Length));

        return false;
    }

    /// <summary>
    ///     Edit-mode is out of scope for SoftwareVersion suggestions (no user-facing UI
    ///     exposes it today). Stub returns <c>(empty, entityMissing=false)</c> so the
    ///     dispatch switch in <c>SuggestionsController.ApplyAcceptedFieldsAsync</c> stays
    ///     exhaustive without throwing <c>NotImplementedException</c> on an unrelated code
    ///     path that might be reached e.g. by an admin manually pushing through an old
    ///     edit suggestion shape.
    /// </summary>
    public static Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(
        MarechaiContext context, long entityId,
        Dictionary<string, object> suggested,
        HashSet<string> accepted)
    {
        _ = context;
        _ = entityId;
        _ = suggested;
        _ = accepted;
        return Task.FromResult((new HashSet<string>(StringComparer.Ordinal), false));
    }

    /// <summary>
    ///     Returns the current scalar snapshot of the targeted SoftwareVersion row. Used
    ///     by the diff endpoint when reviewing an (unsupported) edit-mode suggestion; the
    ///     creation path passes a synthetic snapshot in the SuggestionsController so this
    ///     method is exercised only as a defensive default.
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context,
                                                                               long entityId)
    {
        SoftwareVersion v = await context.SoftwareVersions.AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.Id == (ulong)entityId);
        if(v is null) return null;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [FieldVersionString]   = v.VersionString,
            [FieldPublicVersion]   = v.PublicVersion,
            [FieldCodename]        = v.Codename,
            [FieldParentVersionId] = v.ParentVersionId,
            [FieldLicenseId]       = v.LicenseId
        };
    }

    /// <summary>
    ///     Create a brand-new SoftwareVersion AND its first SoftwareRelease in a single
    ///     atomic EF transaction. Both halves are inserted together; if the release-side
    ///     mandatories fail after the version is saved, the version row is rolled back too.
    ///
    ///     <para>
    ///         The accepted/suggested dictionaries may contain four flavours of keys:
    ///         <list type="bullet">
    ///             <item>Top-level version scalars (<c>version_string</c>, <c>codename</c>, etc.).</item>
    ///             <item>The parent-FK pseudo-field <see cref="FieldSoftwareId" />.</item>
    ///             <item>First-release scalars prefixed with <see cref="FirstReleaseScalarPrefix" />
    ///                   (<c>first_release_title</c>, <c>first_release_publisher_id</c>, etc.).</item>
    ///             <item>First-release junction-add keys prefixed with <see cref="FirstReleaseGroupPrefix" />
    ///                   (<c>first_release.regions.add.&lt;uuid&gt;</c>, etc.).</item>
    ///         </list>
    ///     </para>
    ///
    ///     <para>
    ///         Mandatory: <see cref="FieldSoftwareId" /> (FK existence-checked),
    ///         <see cref="FieldVersionString" /> (non-whitespace),
    ///         <c>first_release_title</c>, <c>first_release_publisher_id</c>,
    ///         <c>first_release_platform_id</c>. The matching <c>software_id</c> and
    ///         <c>software_version_id</c> on the release are injected after the version is
    ///         saved; callers do not (and must not) provide them via the wire.
    ///     </para>
    /// </summary>
    public static async Task<(ulong? newId, HashSet<string> applied)> CreateAsync(
        MarechaiContext context,
        Dictionary<string, object> suggested,
        HashSet<string> accepted,
        string creditedUserId)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        // ── Mandatory: parent Software FK (consumed only here, not on edit-path) ──
        if(!accepted.Contains(FieldSoftwareId) ||
           !suggested.TryGetValue(FieldSoftwareId, out object softwareIdRaw))
            return (null, applied);
        ulong? softwareIdParsed = ToUlong(softwareIdRaw);
        if(!softwareIdParsed.HasValue || softwareIdParsed.Value == 0) return (null, applied);
        ulong softwareId = softwareIdParsed.Value;
        if(!await context.Softwares.AsNoTracking().AnyAsync(s => s.Id == softwareId))
            return (null, applied);

        // ── Mandatory: VersionString ([Required] on the entity, ≤50 chars) ──
        if(!accepted.Contains(FieldVersionString) ||
           !suggested.TryGetValue(FieldVersionString, out object versionStringRaw))
            return (null, applied);
        string versionString = ToStringValue(versionStringRaw);
        if(string.IsNullOrWhiteSpace(versionString)) return (null, applied);
        versionString = versionString.Trim();
        // The model has no explicit StringLength on VersionString; impose a soft cap to
        // protect downstream UI (mirrors the dialog's MudTextField MaxLength).
        if(versionString.Length > 255) return (null, applied);

        // ── Split payload into version-side vs release-side dicts via prefix-strip ──
        var versionSuggested = new Dictionary<string, object>(StringComparer.Ordinal);
        var versionAccepted  = new HashSet<string>(StringComparer.Ordinal);
        var releaseSuggested = new Dictionary<string, object>(StringComparer.Ordinal);
        var releaseAccepted  = new HashSet<string>(StringComparer.Ordinal);

        foreach(string fieldName in accepted)
        {
            if(!suggested.TryGetValue(fieldName, out object value)) continue;

            // Test dot-prefix first (see IsKnownFieldName comment for why).
            if(fieldName.StartsWith(FirstReleaseGroupPrefix, StringComparison.Ordinal))
            {
                string stripped = fieldName.Substring(FirstReleaseGroupPrefix.Length);
                releaseSuggested[stripped] = value;
                releaseAccepted.Add(stripped);
            }
            else if(fieldName.StartsWith(FirstReleaseScalarPrefix, StringComparison.Ordinal))
            {
                string stripped = fieldName.Substring(FirstReleaseScalarPrefix.Length);
                releaseSuggested[stripped] = value;
                releaseAccepted.Add(stripped);
            }
            else
            {
                versionSuggested[fieldName] = value;
                versionAccepted.Add(fieldName);
            }
        }

        // ── Validate first-release mandatories (priority: title → publisher → platform) ──
        if(!releaseAccepted.Contains(SoftwareReleaseSuggestionApplier.FieldTitle) ||
           !releaseSuggested.TryGetValue(SoftwareReleaseSuggestionApplier.FieldTitle, out object titleRaw) ||
           string.IsNullOrWhiteSpace(ToStringValue(titleRaw)))
            return (null, applied);

        if(!releaseAccepted.Contains(SoftwareReleaseSuggestionApplier.FieldPublisherId) ||
           !releaseSuggested.ContainsKey(SoftwareReleaseSuggestionApplier.FieldPublisherId))
            return (null, applied);

        if(!releaseAccepted.Contains(SoftwareReleaseSuggestionApplier.FieldPlatformId) ||
           !releaseSuggested.ContainsKey(SoftwareReleaseSuggestionApplier.FieldPlatformId))
            return (null, applied);

        // ── Atomic transaction: insert SoftwareVersion + its first Release together ──
        await using var tx = await context.Database.BeginTransactionAsync();

        try
        {
            var v = new SoftwareVersion
            {
                SoftwareId    = softwareId,
                VersionString = versionString
            };

            // Apply remaining accepted version scalars (PublicVersion / Codename /
            // ParentVersionId / LicenseId). VersionString is handled above.
            foreach(string fieldName in versionAccepted)
            {
                if(fieldName == FieldVersionString) continue;
                if(fieldName == FieldSoftwareId) continue;
                if(!s_scalarFieldNames.Contains(fieldName)) continue;
                if(!versionSuggested.TryGetValue(fieldName, out object value)) continue;

                try
                {
                    if(await ApplyScalar(context, v, fieldName, value, softwareId)) applied.Add(fieldName);
                }
                catch
                {
                    // Per-field coercion failure: skip.
                }
            }

            await context.SoftwareVersions.AddAsync(v);

            if(string.IsNullOrEmpty(creditedUserId))
                await context.SaveChangesAsync();
            else
                await context.SaveChangesWithUserAsync(creditedUserId);

            applied.Add(FieldSoftwareId);
            applied.Add(FieldVersionString);

            // ── Inject software_id + software_version_id and delegate to release applier ──
            // Cast through long so the release applier's ToUlong helper picks them up
            // cleanly via its long arm. Both FKs are populated on the new release row so
            // the existing Software/View Releases card (which groups by Software) keeps
            // working, and the release also chains to the new Version for downstream
            // version-scoped lookups.
            releaseSuggested[SoftwareReleaseSuggestionApplier.FieldSoftwareId]        = (long)softwareId;
            releaseAccepted.Add(SoftwareReleaseSuggestionApplier.FieldSoftwareId);
            releaseSuggested[SoftwareReleaseSuggestionApplier.FieldSoftwareVersionId] = (long)v.Id;
            releaseAccepted.Add(SoftwareReleaseSuggestionApplier.FieldSoftwareVersionId);

            (ulong? releaseId, HashSet<string> releaseApplied) =
                await SoftwareReleaseSuggestionApplier.CreateAsync(
                    context, releaseSuggested, releaseAccepted, creditedUserId);

            if(!releaseId.HasValue)
            {
                // Mandatory release fields failed server-side validation after passing
                // the controller's defence-in-depth. Roll back the version too — a
                // version without a release is intentionally not allowed via this flow.
                await tx.RollbackAsync();
                return (null, new HashSet<string>(StringComparer.Ordinal));
            }

            // Re-prefix the release-applied keys back to wire shape so the admin diff
            // panel sees the original keys (first_release_title, etc.). Internally
            // injected pseudo-fields (software_id / software_version_id) are NOT
            // exposed — they're an implementation detail of the atomic insert.
            foreach(string releaseKey in releaseApplied)
            {
                if(releaseKey == SoftwareReleaseSuggestionApplier.FieldSoftwareId)        continue;
                if(releaseKey == SoftwareReleaseSuggestionApplier.FieldSoftwareVersionId) continue;

                string wireKey = releaseKey.Contains('.', StringComparison.Ordinal)
                                     ? FirstReleaseGroupPrefix + releaseKey
                                     : FirstReleaseScalarPrefix + releaseKey;
                applied.Add(wireKey);
            }

            await tx.CommitAsync();
            return (v.Id, applied);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ───────────────────────────── Scalar-field application ─────────────────────────────

    static async Task<bool> ApplyScalar(MarechaiContext context, SoftwareVersion v, string fieldName, object value,
                                        ulong softwareId)
    {
        switch(fieldName)
        {
            case FieldPublicVersion:
            {
                string s = ToStringValue(value);
                v.PublicVersion = string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                return true;
            }
            case FieldCodename:
            {
                string s = ToStringValue(value);
                v.Codename = string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                return true;
            }
            case FieldParentVersionId:
            {
                ulong? pid = ToUlong(value);
                if(!pid.HasValue) { v.ParentVersionId = null; return true; }
                // Existence + same-parent-Software check. Disallow cross-Software linking
                // (matches the dialog's same-software autocomplete scope).
                bool ok = await context.SoftwareVersions.AsNoTracking()
                                       .AnyAsync(x => x.Id == pid.Value && x.SoftwareId == softwareId);
                if(!ok) return false;
                v.ParentVersionId = pid.Value;
                return true;
            }
            case FieldLicenseId:
            {
                int? lid = ToInt(value);
                if(!lid.HasValue) { v.LicenseId = null; return true; }
                if(!await context.Licenses.AsNoTracking().AnyAsync(l => l.Id == lid.Value)) return false;
                v.LicenseId = lid.Value;
                return true;
            }
        }
        return false;
    }

    // ───────────────────────────── Value coercion helpers ─────────────────────────────
    // Mirror of the helpers in SoftwareSuggestionApplier / SoftwareReleaseSuggestionApplier.
    // Kept local to avoid cross-applier dependency creep.

    static string ToStringValue(object v)
    {
        return v switch
        {
            null           => null,
            JsonElement je => je.ValueKind switch
                              {
                                  JsonValueKind.Null   => null,
                                  JsonValueKind.String => je.GetString(),
                                  _                    => je.ToString()
                              },
            string s       => s,
            _              => v.ToString()
        };
    }

    static int? ToInt(object v)
    {
        return v switch
        {
            null           => null,
            int i          => i,
            short s        => s,
            long l         => (int?)l,
            byte b         => b,
            JsonElement je => je.ValueKind switch
                              {
                                  JsonValueKind.Number => je.TryGetInt32(out int i) ? i : null,
                                  JsonValueKind.String => int.TryParse(je.GetString(), NumberStyles.Integer,
                                                                       CultureInfo.InvariantCulture, out int p)
                                                              ? p
                                                              : null,
                                  _                    => null
                              },
            string str     => int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int p)
                                  ? p
                                  : null,
            _              => null
        };
    }

    static ulong? ToUlong(object v)
    {
        return v switch
        {
            null           => null,
            ulong u        => u,
            uint u         => u,
            int i          => i >= 0 ? (ulong)i : null,
            short s        => s >= 0 ? (ulong)s : null,
            long l         => l >= 0 ? (ulong)l : null,
            byte b         => b,
            JsonElement je => je.ValueKind switch
                              {
                                  JsonValueKind.Null   => null,
                                  JsonValueKind.Number => je.TryGetUInt64(out ulong p) ? p :
                                                          je.TryGetInt64(out long pl) && pl >= 0 ? (ulong)pl : null,
                                  JsonValueKind.String => ulong.TryParse(je.GetString(), NumberStyles.Integer,
                                                                         CultureInfo.InvariantCulture, out ulong pu)
                                                              ? pu
                                                              : null,
                                  _                    => null
                              },
            string str     => ulong.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong p)
                                  ? p
                                  : null,
            _              => null
        };
    }
}
