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
using System.Threading.Tasks;
using Marechai.Database.Models;

namespace Marechai.Database.Helpers;

/// <summary>
///     Low-level helper for inserting a system-authored conversation + message into the messaging
///     tables. Lives in <c>Marechai.Database</c> so it can be used by both the ASP.NET Server
///     (<see cref="Marechai.Database.Models.ApplicationUser" />-aware code) and the standalone
///     console importers that don't have a configured <c>UserManager</c>.
/// </summary>
public static class MessageDispatcher
{
    /// <summary>
    ///     Creates a new system-authored conversation with the given recipient user IDs and posts a single
    ///     system message into it. Persists with <see cref="MarechaiContext.SaveChangesAsync(System.Threading.CancellationToken)" />
    ///     before returning. Returns <c>null</c> when <paramref name="recipientIds" /> is empty.
    /// </summary>
    /// <param name="context">The MarechaiContext to write through.</param>
    /// <param name="recipientIds">User IDs that should receive the message (the system user is added automatically).</param>
    /// <param name="subject">Conversation subject (truncated to 256 chars).</param>
    /// <param name="body">Message body (Markdown, rendered by the UI through <c>SafeMarkdownRenderer</c>).</param>
    public static async Task<Conversation> PostSystemMessageAsync(MarechaiContext context,
                                                                  IReadOnlyCollection<string> recipientIds,
                                                                  string subject, string body)
    {
        if(context      is null) throw new ArgumentNullException(nameof(context));
        if(recipientIds is null) throw new ArgumentNullException(nameof(recipientIds));

        var distinctRecipients = new HashSet<string>(StringComparer.Ordinal);

        foreach(string id in recipientIds)
        {
            if(!string.IsNullOrEmpty(id)) distinctRecipients.Add(id);
        }

        if(distinctRecipients.Count == 0) return null;

        DateTime now = DateTime.UtcNow;

        var conversation = new Conversation
        {
            Subject        = Truncate(subject, 256),
            IsSystemThread = true
        };

        context.Conversations.Add(conversation);

        // Add the system user as a participant too so it shows up in the participant list (rendered as "System")
        // and so its own state row tracks read/delete uniformly with the other participants.
        var participantIds = new HashSet<string>(distinctRecipients, StringComparer.Ordinal)
        {
            WellKnownUsers.SystemUserId
        };

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

        // Per-user state rows. Sender (system) row is read; recipients unread.
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
