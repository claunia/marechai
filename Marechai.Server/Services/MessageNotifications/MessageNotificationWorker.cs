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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.Email.Composers;
using Marechai.Server.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services.MessageNotifications;

/// <summary>
///     Background drain for <see cref="MessageNotificationQueue" />. For each queued item: opens a DI scope,
///     loads the recipient + message + sender from the database, decides whether the email should be sent
///     (recipient must exist, have a confirmed email, not be a system account, and have <c>NotifyOnNewMessage</c>
///     enabled), then dispatches it via <see cref="NewMessageEmailComposer" /> with
///     <see cref="CultureInfo.CurrentUICulture" /> set to whatever <c>EmailCulture.MapToSupported</c> picks for
///     the recipient's <c>LastLanguageVisited</c> (falling back to English). All exceptions are caught
///     per-item so a single bad email never kills the worker.
/// </summary>
public sealed class MessageNotificationWorker(
    MessageNotificationQueue            queue,
    IServiceScopeFactory                scopeFactory,
    IConfiguration                      configuration,
    ILogger<MessageNotificationWorker>  logger) : BackgroundService
{
    /// <summary>Hard cap on the message body excerpt embedded in the notification email.</summary>
    const int MaxEmailBodyLength = 5000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("MessageNotificationWorker started");

        try
        {
            await foreach(MessageNotificationItem item in queue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessOneAsync(item, stoppingToken);
                }
                catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch(Exception ex)
                {
                    logger.LogError(ex,
                                    "Failed to process new-message email for MessageId={MessageId} RecipientUserId={RecipientUserId}",
                                    item.MessageId, item.RecipientUserId);
                }
            }
        }
        catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
        {
            // Graceful shutdown; nothing to log.
        }

        logger.LogInformation("MessageNotificationWorker stopped");
    }

    async Task ProcessOneAsync(MessageNotificationItem item, CancellationToken cancellationToken)
    {
        using IServiceScope scope    = scopeFactory.CreateScope();
        MarechaiContext     context  = scope.ServiceProvider.GetRequiredService<MarechaiContext>();
        NewMessageEmailComposer composer =
            scope.ServiceProvider.GetRequiredService<NewMessageEmailComposer>();

        ApplicationUser recipient = await context.Users
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync(u => u.Id == item.RecipientUserId,
                                                                       cancellationToken);

        if(recipient is null)
        {
            logger.LogDebug("Skipping notification for missing user {RecipientUserId}", item.RecipientUserId);
            return;
        }

        if(recipient.IsSystemAccount)
        {
            logger.LogDebug("Skipping notification for system account {RecipientUserId}", item.RecipientUserId);
            return;
        }

        if(!recipient.NotifyOnNewMessage)
        {
            logger.LogDebug("Skipping notification: user {RecipientUserId} has notifications disabled",
                            item.RecipientUserId);
            return;
        }

        if(!recipient.EmailConfirmed || string.IsNullOrWhiteSpace(recipient.Email))
        {
            logger.LogDebug("Skipping notification: user {RecipientUserId} has no confirmed email",
                            item.RecipientUserId);
            return;
        }

        var messageRow = await context.Messages
                                       .AsNoTracking()
                                       .Where(m => m.Id == item.MessageId)
                                       .Select(m => new
                                       {
                                           m.Id,
                                           m.ConversationId,
                                           m.Body,
                                           m.SenderId,
                                           m.IsSystemAuthored
                                       })
                                       .FirstOrDefaultAsync(cancellationToken);

        if(messageRow is null)
        {
            logger.LogDebug("Skipping notification: message {MessageId} no longer exists", item.MessageId);
            return;
        }

        // Resolve sender display name. System messages or messages whose sender has been deleted fall back
        // to the literal "Marechai" so the recipient still sees a sensible "from" line.
        string senderDisplayName = "Marechai";

        if(!messageRow.IsSystemAuthored && !string.IsNullOrEmpty(messageRow.SenderId))
        {
            string lookedUp = await context.Users
                                            .AsNoTracking()
                                            .Where(u => u.Id == messageRow.SenderId)
                                            .Select(u => u.DisplayName ?? u.UserName)
                                            .FirstOrDefaultAsync(cancellationToken);

            if(!string.IsNullOrWhiteSpace(lookedUp)) senderDisplayName = lookedUp;
        }

        string body = messageRow.Body ?? string.Empty;

        // Defensive cap; the schema already enforces 5000 but a future bump shouldn't blow the email size.
        if(body.Length > MaxEmailBodyLength) body = body[..MaxEmailBodyLength] + "…";

        string baseUrl    = (configuration["Frontend:BaseUrl"] ?? string.Empty).TrimEnd('/');
        string messageUrl = $"{baseUrl}/messages/{messageRow.ConversationId}";

        // Pick recipient culture for the email; fall back to English when LastLanguageVisited is null/unsupported.
        CultureInfo culture       = EmailCulture.MapToSupported(recipient.LastLanguageVisited);
        CultureInfo previousUi    = CultureInfo.CurrentUICulture;
        CultureInfo previousMain  = CultureInfo.CurrentCulture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture   = culture;

        try
        {
            await composer.SendAsync(recipient.Email, senderDisplayName, body, messageUrl);

            logger.LogInformation(
                "Sent new-message notification for MessageId={MessageId} to {RecipientUserId} ({Culture})",
                item.MessageId, item.RecipientUserId, culture.Name);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousUi;
            CultureInfo.CurrentCulture   = previousMain;
        }
    }
}
