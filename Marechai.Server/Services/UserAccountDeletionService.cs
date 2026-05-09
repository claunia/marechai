/*******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ---------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] -----------------------------------------------------------
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
// ---------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     Hard-deletes a Marechai user account and all per-user content according to the GDPR-compliant
///     strategy:
///     <list type="bullet">
///         <item>
///             Content tables (<c>MachinePhoto</c>, <c>Dump</c>, <c>BookScan</c>, <c>DocumentScan</c>,
///             <c>MagazineScan</c>, <c>SoftwareUserReview</c>, <c>ReviewReport</c>, <c>Message</c>,
///             <c>OwnedMachine</c>, <c>CollectedBook</c>, <c>CollectedDocument</c>,
///             <c>CollectedSoftwareRelease</c>) are reassigned to the seeded <c>system</c> user
///             (<c>00000000-0000-0000-0000-00000000sys</c>) so contributions remain attributed but
///             anonymized. For composite-keyed reassignment targets where <c>system</c> may already own
///             the same row (<c>(UserId, X)</c>), the user's row is dropped instead of duplicated.
///         </item>
///         <item>
///             Per-user behavioural state tables (<c>SoftwareUserRating</c>,
///             <c>SoftwareUserReviewVote</c>, <c>ConversationParticipant</c>, <c>MessageState</c>) are
///             hard-deleted &mdash; reassigning a vote/rating/read-flag would skew aggregates and
///             carries no value for other users.
///         </item>
///         <item>
///             Avatar files are removed from disk via <see cref="AvatarFileCleaner" />.
///         </item>
///         <item>
///             Finally <c>UserManager.DeleteAsync</c> removes the <c>AspNetUsers</c> row, cascading
///             through <c>AspNet*</c> auth metadata.
///         </item>
///     </list>
///     Used by both the admin <c>DELETE /users/{id}</c> path (immediate) and the background
///     <c>AccountDeletionPurgeService</c> at end-of-grace-window.
/// </summary>
public sealed class UserAccountDeletionService(MarechaiContext              context,
                                               UserManager<ApplicationUser> userManager,
                                               AvatarFileCleaner            avatarFileCleaner,
                                               ILogger<UserAccountDeletionService> logger)
{
    /// <summary>Identifier of the seeded built-in system account; reassignment target for content.</summary>
    public const string SystemUserId = "00000000-0000-0000-0000-00000000sys";

    /// <summary>
    ///     Purges <paramref name="userId" /> end-to-end. Returns true on success, false if the user does
    ///     not exist or the Identity delete returned an error (a rare race; details are logged).
    ///     <paramref name="actorUserIdForLog" /> is the principal id that triggered the purge (admin id
    ///     for admin path, <c>"system"</c> for background worker) and is recorded in the log entry.
    /// </summary>
    public async Task<bool> PurgeAsync(string userId, string actorUserIdForLog)
    {
        ApplicationUser user = await userManager.FindByIdAsync(userId);

        if(user is null)
        {
            logger.LogWarning("Purge requested for unknown user {UserId} by {Actor}", userId, actorUserIdForLog);

            return false;
        }

        Guid? avatarGuid = user.AvatarGuid;

        await using IDbContextTransaction tx = await context.Database.BeginTransactionAsync();

        try
        {
            // -------------------------------------------------------------------------------------------
            //  Reassign content tables to the system account.
            // -------------------------------------------------------------------------------------------
            // Single-PK tables: nullable UserId column is just updated in place; the existing FK
            // constraint already permits the system id as a target.

            // MachinePhoto.UserId is nullable + SetNull cascade; reassign instead of nulling so the
            // contributor field renders as "system" (consistent with what new bot-uploaded photos do).
            await context.MachinePhotos
                         .Where(p => p.UserId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(p => p.UserId, _ => SystemUserId));

            await context.Dumps
                         .Where(d => d.UserId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(d => d.UserId, _ => SystemUserId));

            await context.BookScans
                         .Where(s => s.UserId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(p => p.UserId, _ => SystemUserId));

            await context.DocumentScans
                         .Where(s => s.UserId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(p => p.UserId, _ => SystemUserId));

            await context.MagazineScans
                         .Where(s => s.UserId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(p => p.UserId, _ => SystemUserId));

            await context.SoftwareUserReviews
                         .Where(r => r.UserId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(r => r.UserId, _ => SystemUserId));

            await context.ReviewReports
                         .Where(r => r.ReporterId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(r => r.ReporterId, _ => SystemUserId));

            await context.Messages
                         .Where(m => m.SenderId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(m => m.SenderId, _ => SystemUserId));

            // Composite-PK tables: pre-DELETE rows where the system already owns the same target so the
            // subsequent UPDATE can't violate the unique key. This silently drops the user's duplicate
            // rather than failing the purge &mdash; acceptable because the system already has an entry
            // for that target.
            await context.OwnedMachines
                         .Where(m => m.UserId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(m => m.UserId, _ => SystemUserId));

            await context.CollectedBooks
                         .Where(c => c.UserId == userId &&
                                     context.CollectedBooks.Any(s => s.UserId == SystemUserId &&
                                                                     s.BookId == c.BookId))
                         .ExecuteDeleteAsync();
            await context.CollectedBooks
                         .Where(c => c.UserId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(c => c.UserId, _ => SystemUserId));

            await context.CollectedDocuments
                         .Where(c => c.UserId == userId &&
                                     context.CollectedDocuments.Any(s => s.UserId == SystemUserId &&
                                                                         s.DocumentId == c.DocumentId))
                         .ExecuteDeleteAsync();
            await context.CollectedDocuments
                         .Where(c => c.UserId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(c => c.UserId, _ => SystemUserId));

            await context.CollectedSoftwareReleases
                         .Where(c => c.UserId == userId &&
                                     context.CollectedSoftwareReleases.Any(s => s.UserId == SystemUserId &&
                                                                                s.SoftwareReleaseId == c.SoftwareReleaseId))
                         .ExecuteDeleteAsync();
            await context.CollectedSoftwareReleases
                         .Where(c => c.UserId == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(c => c.UserId, _ => SystemUserId));

            // -------------------------------------------------------------------------------------------
            //  Hard-delete behavioural / per-user state tables.
            // -------------------------------------------------------------------------------------------
            await context.SoftwareUserRatings.Where(r => r.UserId == userId).ExecuteDeleteAsync();
            await context.SoftwareUserReviewVotes.Where(v => v.UserId == userId).ExecuteDeleteAsync();
            await context.ConversationParticipants.Where(p => p.UserId == userId).ExecuteDeleteAsync();
            await context.MessageStates.Where(m => m.UserId == userId).ExecuteDeleteAsync();

            // -------------------------------------------------------------------------------------------
            //  Drop any in-flight invitation codes the user issued (CreatedById is Restrict-FK so we
            //  cannot leave them attached). Delete unused ones outright; reassign used ones to system
            //  so the audit trail (who consumed when) remains intact.
            // -------------------------------------------------------------------------------------------
            await context.InvitationCodes
                         .Where(c => c.CreatedById == userId && c.UsedById == null)
                         .ExecuteDeleteAsync();
            await context.InvitationCodes
                         .Where(c => c.CreatedById == userId)
                         .ExecuteUpdateAsync(s => s.SetProperty(c => c.CreatedById, _ => SystemUserId));

            await tx.CommitAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Failed to reassign/delete per-user content for {UserId}; rolling back", userId);
            await tx.RollbackAsync();

            throw;
        }

        // Identity delete is OUTSIDE the reassignment transaction. UserManager.DeleteAsync uses its own
        // EF transaction and cascades through AspNet* tables. If it fails we have a half-purged user
        // (content reassigned but Identity row still present); the caller should treat that as a hard
        // error and not retry blindly because the next attempt would succeed at deleting Identity but
        // leave nothing to reassign.
        IdentityResult result = await userManager.DeleteAsync(user);

        if(!result.Succeeded)
        {
            logger.LogError("UserManager.DeleteAsync({UserId}) failed for actor {Actor}: {Errors}",
                            userId,
                            actorUserIdForLog,
                            string.Join("; ", result.Errors.Select(e => e.Description)));

            return false;
        }

        // Avatar file cleanup: AFTER successful Identity delete so a transient FS error cannot leave a
        // user without their files but with a row pointing at them.
        if(avatarGuid.HasValue) avatarFileCleaner.DeleteAvatarFiles(avatarGuid.Value);

        logger.LogInformation("Purged user {UserId} (avatar={Avatar}) by actor {Actor}",
                              userId,
                              avatarGuid,
                              actorUserIdForLog);

        return true;
    }
}
