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
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Helpers;

/// <summary>
///     Helper for posting system-authored messages into the new messaging system. Sender is the seeded
///     <see cref="WellKnownUsers.SystemUserId" /> account; recipients are all current Admin/UberAdmin users.
/// </summary>
internal static class MessageDispatcher
{
    /// <summary>
    ///     Creates a new system-authored conversation with all current Admin/UberAdmin users as participants and
    ///     persists the changes via <paramref name="context" />. The caller is responsible for the surrounding
    ///     transaction and must NOT call <c>SaveChangesAsync</c> on this conversation again — the method already does so.
    /// </summary>
    /// <returns>The created conversation entity (already saved), or <c>null</c> if no admins exist.</returns>
    public static async Task<Conversation> SendSystemMessageToAdminsAsync(MarechaiContext context,
                                                                          UserManager<ApplicationUser> userManager,
                                                                          string subject,
                                                                          string body)
    {
        // Distinct admin/uberadmin user IDs (a user with both roles must only appear once).
        var adminIds = new HashSet<string>(StringComparer.Ordinal);

        foreach(ApplicationUser u in await userManager.GetUsersInRoleAsync("Admin"))
            adminIds.Add(u.Id);

        foreach(ApplicationUser u in await userManager.GetUsersInRoleAsync("UberAdmin"))
            adminIds.Add(u.Id);

        if(adminIds.Count == 0) return null;

        DateTime now = DateTime.UtcNow;

        var conversation = new Conversation
        {
            Subject        = Truncate(subject, 256),
            IsSystemThread = true
        };

        context.Conversations.Add(conversation);

        // Add the system user as a participant too so it appears in the participant list (rendered as "System").
        // Sender's own state row tracks read/delete uniformly.
        var participantIds = new HashSet<string>(adminIds, StringComparer.Ordinal) { WellKnownUsers.SystemUserId };

        foreach(string userId in participantIds)
        {
            context.ConversationParticipants.Add(new ConversationParticipant
            {
                Conversation = conversation,
                UserId       = userId,
                JoinedOn     = now
            });
        }

        var message = new Message
        {
            Conversation     = conversation,
            SenderId         = WellKnownUsers.SystemUserId,
            Body             = body ?? string.Empty,
            IsSystemAuthored = true
        };

        context.Messages.Add(message);

        // Per-user state rows. Sender (system) row is read; others unread.
        foreach(string userId in participantIds)
        {
            context.MessageStates.Add(new MessageState
            {
                Message = message,
                UserId  = userId,
                IsRead  = userId == WellKnownUsers.SystemUserId,
                ReadAt  = userId == WellKnownUsers.SystemUserId ? now : null
            });
        }

        await context.SaveChangesAsync();

        return conversation;
    }

    static string Truncate(string s, int max)
    {
        if(string.IsNullOrEmpty(s)) return s;

        return s.Length <= max ? s : s.Substring(0, max);
    }
}
