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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Marechai.Server.Services.MessageNotifications;

/// <summary>
///     One queued "send a new-message email to this user" instruction. Carries only primitive identifiers; the
///     <see cref="MessageNotificationWorker" /> rehydrates the message, sender and recipient from the database
///     when it dequeues, so no DTOs cross the threading boundary.
/// </summary>
/// <param name="MessageId">Primary key of the freshly-saved <c>Message</c> row.</param>
/// <param name="RecipientUserId">AspNetUsers Id of the user who should receive the notification email.</param>
public sealed record MessageNotificationItem(long MessageId, string RecipientUserId);

/// <summary>
///     Process-singleton in-memory queue of pending message-notification emails. The
///     <c>MessagesController</c> writes after each successful <c>SaveChangesAsync</c>; the
///     <see cref="MessageNotificationWorker" /> drains. Backed by an unbounded
///     <see cref="System.Threading.Channels.Channel{T}" /> so producers never block (acceptable while messages
///     remain rate-limited at one per 60 s per sender).
/// </summary>
public sealed class MessageNotificationQueue
{
    readonly Channel<MessageNotificationItem> _channel =
        Channel.CreateUnbounded<MessageNotificationItem>(new UnboundedChannelOptions
        {
            SingleReader = true, SingleWriter = false
        });

    /// <summary>
    ///     Enqueues a notification request. Non-blocking on the unbounded channel; the returned
    ///     <see cref="ValueTask" /> always completes synchronously today but is exposed as a
    ///     <see cref="ValueTask" /> so the implementation can switch to a bounded channel later without breaking
    ///     callers.
    /// </summary>
    public ValueTask EnqueueAsync(MessageNotificationItem item, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(item, cancellationToken);

    /// <summary>
    ///     Async stream of queued items for the worker. Yields each item as soon as a producer writes one and
    ///     completes when the channel is closed (which never happens in normal operation; the worker stops via
    ///     <paramref name="cancellationToken" />).
    /// </summary>
    public IAsyncEnumerable<MessageNotificationItem> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
