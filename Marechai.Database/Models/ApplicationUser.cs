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
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Marechai.Database.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string DisplayName { get; set; }

    [MaxLength(2000)]
    public string Bio { get; set; }

    [MaxLength(500)]
    public string Website { get; set; }

    [MaxLength(200)]
    public string Location { get; set; }

    public bool UseGravatar { get; set; } = true;

    public Guid? AvatarGuid { get; set; }

    [MaxLength(10)]
    public string OriginalAvatarExtension { get; set; }

    [MaxLength(100)]
    public string Twitter { get; set; }

    [MaxLength(100)]
    public string GitHub { get; set; }

    [MaxLength(200)]
    public string Mastodon { get; set; }

    [MaxLength(200)]
    public string Facebook { get; set; }

    [MaxLength(200)]
    public string LinkedIn { get; set; }

    /// <summary>
    ///     True for the seeded built-in `system` account used to author bot messages (e.g. report notifications).
    ///     System users are never loginable and bypass all messaging quotas / rate limits.
    /// </summary>
    public bool IsSystemAccount { get; set; }

    /// <summary>
    ///     Slug of the user's preferred UI theme (e.g. <c>default-dark</c>, <c>default-light</c>). When
    ///     <see langword="null" /> the application falls back to the default theme. The set of valid slugs is the
    ///     intersection of <c>Marechai.Data.Constants.ThemeIds</c> (server allow-list) and the client-side
    ///     <c>ThemeCatalog</c>.
    /// </summary>
    [MaxLength(50)]
    public string PreferredThemeId { get; set; }

    /// <summary>
    ///     True when the user has enabled TOTP authenticator-app two-factor authentication. Independent of
    ///     <see cref="IdentityUser.TwoFactorEnabled" />, which is the global flag set when at least one 2FA method
    ///     is active.
    /// </summary>
    public bool TwoFactorViaAuthenticator { get; set; }

    /// <summary>
    ///     True when the user has enabled email-code two-factor authentication. Independent of
    ///     <see cref="IdentityUser.TwoFactorEnabled" />, which is the global flag set when at least one 2FA method
    ///     is active. Requires a confirmed email.
    /// </summary>
    public bool TwoFactorViaEmail { get; set; }

    /// <summary>
    ///     When <see langword="true" /> the user receives an email each time a new conversation message is
    ///     delivered to their inbox. Defaults to <see langword="true" /> for new accounts and is backfilled to
    ///     <see langword="true" /> for existing accounts by the migration that introduced this column. Users can
    ///     toggle this from their profile's <c>Notifications</c> section. The notification email is dispatched by
    ///     <c>MessageNotificationWorker</c> and rendered in the recipient's <see cref="LastLanguageVisited" />
    ///     culture (falling back to English when null/unsupported).
    /// </summary>
    public bool NotifyOnNewMessage { get; set; } = true;

    /// <summary>
    ///     BCP-47 language code (e.g. <c>en</c>, <c>es</c>, <c>pt-BR</c>) of the most recent UI culture the user
    ///     was browsing the site in. Updated by the Razor host page on every authenticated visit so background
    ///     workers (notably the new-message email composer) can render messages in the recipient's preferred
    ///     language even though the recipient is offline at send time. <see langword="null" /> for accounts that
    ///     have never visited the Blazor UI; consumers must fall back to <c>"en"</c>.
    /// </summary>
    [MaxLength(10)]
    public string LastLanguageVisited { get; set; }

    /// <summary>
    ///     UTC timestamp at which the user's self-service GDPR deletion request was confirmed (via the
    ///     emailed link). Null while the account is active. The background
    ///     <c>AccountDeletionPurgeService</c> hard-deletes accounts whose value is older than 30 days; the
    ///     user can cancel by calling <c>POST /auth/me/delete/cancel</c> at any point during the grace
    ///     window.
    /// </summary>
    public DateTime? DeletionRequestedAt { get; set; }

    public virtual ICollection<MachinePhoto>              Photos                    { get; set; }
    public virtual ICollection<OwnedMachine>              OwnedMachines             { get; set; }
    public virtual ICollection<CollectedBook>             CollectedBooks            { get; set; }
    public virtual ICollection<CollectedDocument>         CollectedDocuments        { get; set; }
    public virtual ICollection<CollectedMagazineIssue>    CollectedMagazineIssues   { get; set; }
    public virtual ICollection<CollectedSoftwareRelease>  CollectedSoftwareReleases { get; set; }
    public virtual ICollection<Dump>                      Dumps                     { get; set; }
    public virtual ICollection<SoftwareUserRating>        SoftwareRatings           { get; set; }
    public virtual ICollection<SoftwareUserReview>        SoftwareReviews           { get; set; }
    public virtual ICollection<SoftwareUserReviewVote>    ReviewVotes               { get; set; }
    public virtual ICollection<ReviewReport>              ReviewReports             { get; set; }
}