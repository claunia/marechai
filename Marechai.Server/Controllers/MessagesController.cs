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
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Marechai.Server.Helpers;
using Marechai.Server.Services.MessageNotifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[ApiController]
[Route("messages")]
[Authorize]
public class MessagesController(
    MarechaiContext              context,
    UserManager<ApplicationUser> userManager,
    MessageNotificationQueue     notificationQueue) : ControllerBase
{
    public const int InboxQuota          = 50;
    public const int RateLimitSeconds    = 60;
    public const int MaxBodyLength       = Message.MaxBodyLength;
    public const int SnippetLength       = 160;
    public const int UserSearchMinChars  = 2;
    public const int UserSearchMaxResult = 20;

    // ───────────────────────────── Conversations ─────────────────────────────

    /// <summary>
    ///     Builds the conversation list for the given <paramref name="folder"/> already restricted to threads the
    ///     <paramref name="userId"/> still participates in. Shared by <see cref="GetConversationsAsync"/> and
    ///     <see cref="GetConversationsCountAsync"/> so the page and the total stay in sync.
    /// </summary>
    /// <remarks><paramref name="folder"/> is expected to be already lowercased/trimmed by the caller.</remarks>
    IQueryable<Conversation> BuildFolderConversationsQuery(string userId, bool isAdmin, string folder)
    {
        // Conversations the current user is still part of (LeftOn is null) and the only-non-deleted-message condition
        // for inbox/sent.
        IQueryable<Conversation> baseQuery = context.Conversations
            .Where(c => c.Participants.Any(p => p.UserId == userId && p.LeftOn == null));

        IQueryable<Conversation> filtered = folder switch
        {
            "reports" => baseQuery.Where(c => c.IsSystemThread),
            "sent" => baseQuery.Where(c => c.Messages
                                            .Any(m => m.SenderId == userId
                                                   && m.States.Any(s => s.UserId == userId && s.DeletedAt == null))),
            _ /* inbox */ => baseQuery.Where(c => !c.IsSystemThread || isAdmin)
                                      .Where(c => c.Messages
                                                   .Any(m => m.SenderId != userId
                                                          && m.States.Any(s => s.UserId == userId && s.DeletedAt == null)))
        };

        // Inbox already includes system threads when admin; reports tab is the dedicated system-thread view.
        if(folder == "inbox") filtered = filtered.Where(c => !c.IsSystemThread);

        return filtered;
    }

    [HttpGet("conversations")]
    [ProducesResponseType(typeof(List<ConversationSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ConversationSummaryDto>>> GetConversationsAsync(
        [FromQuery] string folder = "inbox",
        [FromQuery] int    page   = 1,
        [FromQuery] int    pageSize = 25)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        if(page < 1) page = 1;
        if(pageSize is < 1 or > 200) pageSize = 25;

        folder = (folder ?? "inbox").Trim().ToLowerInvariant();

        bool isAdmin = await IsAdminAsync(userId);

        if(folder == "reports" && !isAdmin) return Forbid();

        IQueryable<Conversation> filtered = BuildFolderConversationsQuery(userId, isAdmin, folder);

        // Project to summaries with latest visible message + unread count.
        var rows = await filtered
            .Select(c => new
            {
                Conversation = c,
                LatestMessage = c.Messages
                                 .Where(m => m.States.Any(s => s.UserId == userId && s.DeletedAt == null))
                                 .OrderByDescending(m => m.CreatedOn)
                                 .Select(m => new
                                 {
                                     m.Id,
                                     m.SenderId,
                                     SenderDisplayName = m.Sender == null ? "(deleted)" : m.Sender.DisplayName ?? m.Sender.UserName,
                                     m.Body,
                                     m.IsSystemAuthored,
                                     m.CreatedOn
                                 })
                                 .FirstOrDefault(),
                UnreadCount = c.Messages
                               .Count(m => m.SenderId != userId
                                        && m.States.Any(s => s.UserId == userId && !s.IsRead && s.DeletedAt == null)),
                Participants = c.Participants
                                .Where(p => p.User != null)
                                .Select(p => new
                                {
                                    p.User!.Id,
                                    p.User.UserName,
                                    p.User.DisplayName,
                                    p.User.Email,
                                    p.User.UseGravatar,
                                    p.User.AvatarGuid,
                                    p.User.IsSystemAccount
                                })
                                .ToList()
            })
            .OrderByDescending(r => r.LatestMessage!.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var participantIds = rows
            .SelectMany(r => r.Participants.Select(p => p.Id))
            .Distinct()
            .ToList();

        HashSet<string> adminUserIds        = await GetAdminIdsAsync(participantIds);
        HashSet<string> collaboratorUserIds = await GetCollaboratorIdsAsync(participantIds);

        var result = rows.Select(r => new ConversationSummaryDto
        {
            Id             = r.Conversation.Id,
            Subject        = r.Conversation.Subject,
            IsSystemThread = r.Conversation.IsSystemThread,
            UnreadCount    = r.UnreadCount,
            LastActivityOn = r.LatestMessage?.CreatedOn ?? r.Conversation.UpdatedOn,
            LatestMessage = r.LatestMessage == null
                                ? null
                                : new MessageSummaryDto
                                {
                                    Id                = r.LatestMessage.Id,
                                    SenderDisplayName = r.LatestMessage.SenderDisplayName,
                                    Snippet           = MakeSnippet(r.LatestMessage.Body),
                                    IsSystemAuthored  = r.LatestMessage.IsSystemAuthored,
                                    CreatedOn         = r.LatestMessage.CreatedOn
                                },
            Participants = r.Participants.Select(p => new UserSummaryDto
            {
                Id             = p.Id,
                UserName       = p.UserName,
                DisplayName    = p.DisplayName ?? p.UserName,
                AvatarUrl      = BuildAvatarUrl(p.UseGravatar, p.Email, p.AvatarGuid),
                IsSystem       = p.IsSystemAccount,
                IsAdmin        = adminUserIds.Contains(p.Id),
                IsCollaborator = collaboratorUserIds.Contains(p.Id)
            }).ToList()
        }).ToList();

        return Ok(result);
    }

    [HttpGet("conversations/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<int>> GetConversationsCountAsync([FromQuery] string folder = "inbox")
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        folder = (folder ?? "inbox").Trim().ToLowerInvariant();

        bool isAdmin = await IsAdminAsync(userId);

        if(folder == "reports" && !isAdmin) return Forbid();

        int total = await BuildFolderConversationsQuery(userId, isAdmin, folder).CountAsync();

        return Ok(total);
    }

    [HttpGet("conversations/{id:long}")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationDto>> GetConversationAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        Conversation conv = await context.Conversations
                                         .Include(c => c.Participants)
                                         .ThenInclude(p => p.User)
                                         .FirstOrDefaultAsync(c => c.Id == id);

        if(conv is null) return NotFound();

        bool isParticipant = conv.Participants.Any(p => p.UserId == userId && p.LeftOn == null);
        if(!isParticipant) return Forbid();

        var rawMessages = await context.Messages
            .Where(m => m.ConversationId == id)
            .Where(m => m.States.Any(s => s.UserId == userId && s.DeletedAt == null))
            .OrderBy(m => m.CreatedOn)
            .Select(m => new
            {
                m.Id,
                m.ConversationId,
                m.SenderId,
                SenderUserName = m.Sender == null ? null : m.Sender.UserName,
                SenderDisplayName = m.Sender == null ? null : m.Sender.DisplayName,
                SenderEmail = m.Sender == null ? null : m.Sender.Email,
                SenderUseGravatar = m.Sender != null && m.Sender.UseGravatar,
                SenderAvatarGuid = m.Sender == null ? (Guid?)null : m.Sender.AvatarGuid,
                SenderIsSystem = m.Sender != null && m.Sender.IsSystemAccount,
                m.Body,
                m.IsSystemAuthored,
                m.ParentMessageId,
                m.CreatedOn,
                IsRead = m.States.Any(s => s.UserId == userId && s.IsRead)
            })
            .ToListAsync();

        // Mark all unread messages addressed to this user as read in a single update.
        DateTime now = DateTime.UtcNow;
        await context.MessageStates
                     .Where(s => s.UserId == userId
                              && !s.IsRead
                              && s.DeletedAt == null
                              && s.Message.ConversationId == id
                              && s.Message.SenderId != userId)
                     .ExecuteUpdateAsync(setters => setters
                                            .SetProperty(s => s.IsRead, true)
                                            .SetProperty(s => s.ReadAt, now));

        var participantIds = conv.Participants
                                  .Where(p => p.UserId != null)
                                  .Select(p => p.UserId)
                                  .ToList();

        var allRelatedIds = new HashSet<string>(participantIds, StringComparer.Ordinal);
        foreach(var m in rawMessages)
            if(m.SenderId is not null) allRelatedIds.Add(m.SenderId);

        HashSet<string> adminIds        = await GetAdminIdsAsync(allRelatedIds.ToList());
        HashSet<string> collaboratorIds = await GetCollaboratorIdsAsync(allRelatedIds.ToList());

        var dto = new ConversationDto
        {
            Id             = conv.Id,
            Subject        = conv.Subject,
            IsSystemThread = conv.IsSystemThread,
            Participants = conv.Participants
                               .Where(p => p.User != null)
                               .Select(p => new UserSummaryDto
                               {
                                   Id             = p.User!.Id,
                                   UserName       = p.User.UserName,
                                   DisplayName    = p.User.DisplayName ?? p.User.UserName,
                                   AvatarUrl      = BuildAvatarUrl(p.User.UseGravatar, p.User.Email, p.User.AvatarGuid),
                                   IsSystem       = p.User.IsSystemAccount,
                                   IsAdmin        = adminIds.Contains(p.User.Id),
                                   IsCollaborator = collaboratorIds.Contains(p.User.Id)
                               })
                               .ToList(),
            Messages = rawMessages.Select(m => new MessageDto
            {
                Id              = m.Id,
                ConversationId  = m.ConversationId,
                Body            = m.Body,
                IsSystemAuthored = m.IsSystemAuthored,
                ParentMessageId = m.ParentMessageId,
                CreatedOn       = m.CreatedOn,
                IsRead          = m.IsRead || m.SenderId == userId,
                Sender = m.SenderId is null
                             ? null
                             : new UserSummaryDto
                             {
                                 Id             = m.SenderId,
                                 UserName       = m.SenderUserName,
                                 DisplayName    = m.SenderDisplayName ?? m.SenderUserName,
                                 AvatarUrl      = BuildAvatarUrl(m.SenderUseGravatar, m.SenderEmail, m.SenderAvatarGuid),
                                 IsSystem       = m.SenderIsSystem,
                                 IsAdmin        = adminIds.Contains(m.SenderId),
                                 IsCollaborator = collaboratorIds.Contains(m.SenderId)
                             }
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost("conversations")]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateConversationAsync([FromBody] CreateConversationRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        if(request is null || string.IsNullOrWhiteSpace(request.RecipientId) || string.IsNullOrWhiteSpace(request.Body))
            return BadRequest(BuildProblem(StatusCodes.Status400BadRequest, "EMPTY_FIELDS",
                                           "Recipient and body are required."));

        if(string.Equals(request.RecipientId, userId, StringComparison.Ordinal))
            return BadRequest(BuildProblem(StatusCodes.Status400BadRequest, "SELF_RECIPIENT",
                                           "You cannot send a message to yourself."));

        if(request.Body!.Length > MaxBodyLength)
            return BadRequest(BuildProblem(StatusCodes.Status400BadRequest, "MESSAGE_TOO_LONG",
                                           $"Message exceeds {MaxBodyLength} character limit.",
                                           ("maxLength", MaxBodyLength)));

        ApplicationUser recipient = await context.Users
                                                  .FirstOrDefaultAsync(u => u.Id == request.RecipientId);

        if(recipient is null) return NotFound();

        if(!recipient.EmailConfirmed || recipient.IsSystemAccount)
            return BadRequest(BuildProblem(StatusCodes.Status400BadRequest, "INVALID_RECIPIENT",
                                           "Recipient cannot receive messages."));

        bool senderIsAdmin = await IsAdminAsync(userId);

        // Rate limit (admins exempt). 1 user-authored sent message per RateLimitSeconds.
        if(!senderIsAdmin)
        {
            DateTime cutoff = DateTime.UtcNow.AddSeconds(-RateLimitSeconds);

            DateTime? lastSent = await context.Messages
                                              .Where(m => m.SenderId == userId && !m.IsSystemAuthored)
                                              .Select(m => (DateTime?)m.CreatedOn)
                                              .OrderByDescending(t => t)
                                              .FirstOrDefaultAsync();

            if(lastSent is not null && lastSent > cutoff)
            {
                int retry = (int)Math.Ceiling((lastSent.Value - cutoff).TotalSeconds);
                Response.Headers["Retry-After"] = retry.ToString();
                return StatusCode(StatusCodes.Status429TooManyRequests,
                                  BuildProblem(StatusCodes.Status429TooManyRequests, "RATE_LIMITED",
                                               $"You can send another message in {retry}s.",
                                               ("retryAfterSeconds", retry)));
            }
        }

        // Recipient inbox quota (admins/system always bypass; sender-is-admin also bypasses).
        if(!senderIsAdmin)
        {
            bool recipientIsAdmin = await IsAdminAsync(recipient.Id);

            if(!recipientIsAdmin)
            {
                int receivedCount = await context.MessageStates
                                                  .CountAsync(s => s.UserId == recipient.Id
                                                                && s.DeletedAt == null
                                                                && s.Message.SenderId != recipient.Id);

                if(receivedCount >= InboxQuota)
                    return Conflict(BuildProblem(StatusCodes.Status409Conflict, "INBOX_FULL",
                                                  $"{recipient.DisplayName ?? recipient.UserName}'s inbox is full and cannot receive more messages.",
                                                  ("recipientDisplayName", recipient.DisplayName ?? recipient.UserName)));
            }
        }

        DateTime now = DateTime.UtcNow;

        var conversation = new Conversation
        {
            Subject        = string.IsNullOrWhiteSpace(request.Subject) ? null : Truncate(request.Subject!, 256),
            IsSystemThread = false
        };
        context.Conversations.Add(conversation);

        var senderParticipant = new ConversationParticipant
        {
            Conversation = conversation, UserId = userId, JoinedOn = now
        };

        var recipientParticipant = new ConversationParticipant
        {
            Conversation = conversation, UserId = recipient.Id, JoinedOn = now
        };

        context.ConversationParticipants.AddRange(senderParticipant, recipientParticipant);

        var message = new Message
        {
            Conversation     = conversation,
            SenderId         = userId,
            Body             = request.Body!,
            IsSystemAuthored = false
        };
        context.Messages.Add(message);

        context.MessageStates.Add(new MessageState
        {
            Message = message, UserId = userId, IsRead = true, ReadAt = now
        });
        context.MessageStates.Add(new MessageState
        {
            Message = message, UserId = recipient.Id, IsRead = false
        });

        await context.SaveChangesWithUserAsync(userId);

        // Queue the new-message email notification. The worker re-checks the recipient's NotifyOnNewMessage
        // flag, IsSystemAccount, and EmailConfirmed before sending, so we keep the controller's logic minimal.
        await notificationQueue.EnqueueAsync(new MessageNotificationItem(message.Id, recipient.Id),
                                              HttpContext.RequestAborted);

        return Created($"/messages/conversations/{conversation.Id}", conversation.Id);
    }

    [HttpPost("conversations/{id:long}/reply")]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ReplyAsync(long id, [FromBody] ReplyRequest request)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        if(request is null || string.IsNullOrWhiteSpace(request.Body))
            return BadRequest(BuildProblem(StatusCodes.Status400BadRequest, "EMPTY_FIELDS",
                                           "Body is required."));

        if(request.Body!.Length > MaxBodyLength)
            return BadRequest(BuildProblem(StatusCodes.Status400BadRequest, "MESSAGE_TOO_LONG",
                                           $"Message exceeds {MaxBodyLength} character limit.",
                                           ("maxLength", MaxBodyLength)));

        Conversation conv = await context.Conversations
                                         .Include(c => c.Participants)
                                         .FirstOrDefaultAsync(c => c.Id == id);

        if(conv is null) return NotFound();

        ConversationParticipant me = conv.Participants
                                         .FirstOrDefault(p => p.UserId == userId && p.LeftOn == null);
        if(me is null) return Forbid();

        bool senderIsAdmin = await IsAdminAsync(userId);

        // System threads can only be replied to by admin/uberadmin (the reporter is not a participant).
        if(conv.IsSystemThread && !senderIsAdmin) return Forbid();

        // Rate limit (admins exempt).
        if(!senderIsAdmin)
        {
            DateTime cutoff = DateTime.UtcNow.AddSeconds(-RateLimitSeconds);

            DateTime? lastSent = await context.Messages
                                              .Where(m => m.SenderId == userId && !m.IsSystemAuthored)
                                              .Select(m => (DateTime?)m.CreatedOn)
                                              .OrderByDescending(t => t)
                                              .FirstOrDefaultAsync();

            if(lastSent is not null && lastSent > cutoff)
            {
                int retry = (int)Math.Ceiling((lastSent.Value - cutoff).TotalSeconds);
                Response.Headers["Retry-After"] = retry.ToString();
                return StatusCode(StatusCodes.Status429TooManyRequests,
                                  BuildProblem(StatusCodes.Status429TooManyRequests, "RATE_LIMITED",
                                               $"You can send another message in {retry}s.",
                                               ("retryAfterSeconds", retry)));
            }
        }

        // Quota check: every non-sender participant who is not an admin must have room.
        var otherParticipantIds = conv.Participants
                                       .Where(p => p.UserId != userId && p.LeftOn == null)
                                       .Select(p => p.UserId)
                                       .ToList();

        if(!senderIsAdmin && otherParticipantIds.Count > 0)
        {
            HashSet<string> adminIds = await GetAdminIdsAsync(otherParticipantIds);

            foreach(string recipientId in otherParticipantIds)
            {
                if(adminIds.Contains(recipientId)) continue;

                int receivedCount = await context.MessageStates
                                                  .CountAsync(s => s.UserId == recipientId
                                                                && s.DeletedAt == null
                                                                && s.Message.SenderId != recipientId);

                if(receivedCount >= InboxQuota)
                {
                    string recipientName = await context.Users
                                                         .Where(u => u.Id == recipientId)
                                                         .Select(u => u.DisplayName ?? u.UserName)
                                                         .FirstOrDefaultAsync() ?? recipientId;

                    return Conflict(BuildProblem(StatusCodes.Status409Conflict, "INBOX_FULL",
                                                  $"{recipientName}'s inbox is full and cannot receive more messages.",
                                                  ("recipientDisplayName", recipientName)));
                }
            }
        }

        DateTime now = DateTime.UtcNow;

        long? parentMessageId = await context.Messages
                                              .Where(m => m.ConversationId == id)
                                              .OrderByDescending(m => m.CreatedOn)
                                              .Select(m => (long?)m.Id)
                                              .FirstOrDefaultAsync();

        var message = new Message
        {
            ConversationId   = id,
            SenderId         = userId,
            Body             = request.Body!,
            IsSystemAuthored = false,
            ParentMessageId  = parentMessageId
        };
        context.Messages.Add(message);

        // States: sender row read; every other still-active participant gets an unread row.
        context.MessageStates.Add(new MessageState
        {
            Message = message, UserId = userId, IsRead = true, ReadAt = now
        });

        foreach(string recipientId in otherParticipantIds)
        {
            context.MessageStates.Add(new MessageState
            {
                Message = message, UserId = recipientId, IsRead = false
            });
        }

        await context.SaveChangesWithUserAsync(userId);

        // Queue one new-message email notification per non-sender participant. The worker filters out
        // recipients with NotifyOnNewMessage off, system accounts, and unconfirmed emails before sending.
        foreach(string recipientId in otherParticipantIds)
        {
            await notificationQueue.EnqueueAsync(new MessageNotificationItem(message.Id, recipientId),
                                                  HttpContext.RequestAborted);
        }

        return Created($"/messages/conversations/{id}", message.Id);
    }

    [HttpDelete("conversations/{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConversationAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        ConversationParticipant me = await context.ConversationParticipants
                                                   .FirstOrDefaultAsync(p => p.ConversationId == id
                                                                          && p.UserId == userId);

        if(me is null) return NotFound();

        DateTime now = DateTime.UtcNow;
        me.LeftOn = now;

        await context.MessageStates
                     .Where(s => s.UserId == userId
                              && s.DeletedAt == null
                              && s.Message.ConversationId == id)
                     .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.DeletedAt, now));

        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    // ───────────────────────────── Single Message ─────────────────────────────

    [HttpDelete("messages/{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMessageAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        MessageState state = await context.MessageStates
                                           .FirstOrDefaultAsync(s => s.MessageId == id && s.UserId == userId);

        if(state is null) return NotFound();

        state.DeletedAt = DateTime.UtcNow;
        await context.SaveChangesWithUserAsync(userId);

        return NoContent();
    }

    [HttpPost("messages/{id:long}/report")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReportMessageAsync(long id, [FromBody] CreateMessageReportRequest dto)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        if(dto is null) return BadRequest();

        Message message = await context.Messages
                                        .Include(m => m.Sender)
                                        .Include(m => m.Conversation)
                                        .FirstOrDefaultAsync(m => m.Id == id);

        if(message is null) return NotFound();

        // Must be a participant of the conversation containing the message.
        bool isParticipant = await context.ConversationParticipants
                                           .AnyAsync(p => p.ConversationId == message.ConversationId && p.UserId == userId);

        if(!isParticipant) return Forbid();

        if(message.SenderId == userId)
            return Problem("You cannot report your own message.", statusCode: StatusCodes.Status403Forbidden);

        if(message.IsSystemAuthored)
            return Problem("System messages cannot be reported.", statusCode: StatusCodes.Status403Forbidden);

        bool alreadyReported = await context.MessageReports
                                             .AnyAsync(r => r.ReporterId == userId && r.MessageId == id);

        if(alreadyReported) return Conflict(BuildProblem(StatusCodes.Status409Conflict, "ALREADY_REPORTED",
                                                          "You have already reported this message."));

        context.MessageReports.Add(new MessageReport
        {
            ReporterId  = userId,
            MessageId   = id,
            Reason      = dto.Reason,
            Explanation = dto.Explanation
        });

        await context.SaveChangesWithUserAsync(userId);

        // Send a system-authored thread to all admins/uberadmins with the report context and quoted body.
        try
        {
            string explanationBlock = string.IsNullOrWhiteSpace(dto.Explanation)
                                          ? string.Empty
                                          : $"\n\n**Reporter explanation:** {dto.Explanation}";

            string senderLabel = message.Sender is null
                                     ? "(deleted user)"
                                     : message.Sender.DisplayName ?? message.Sender.UserName;

            string quoted = string.Join('\n', (message.Body ?? string.Empty)
                                              .Split('\n', StringSplitOptions.None)
                                              .Select(l => "> " + l));

            await MessageDispatcher.SendSystemMessageToAdminsAsync(context,
                userManager,
                subject: $"Message reported: {dto.Reason}",
                body:
                $"A message from **{senderLabel}** has been reported as **{dto.Reason}**.{explanationBlock}\n\n" +
                $"**Reported message:**\n\n{quoted}\n\n" +
                $"[Open the conversation](/messages/{message.ConversationId})  •  [Open Message Reports](/admin/messages/reports)");
        }
        catch
        {
            // Swallow — the report row exists; admins can still find it on the reports page.
        }

        return Created();
    }

    // ───────────────────────────── Unread count ─────────────────────────────

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> GetUnreadCountAsync()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        int count = await context.MessageStates
                                  .CountAsync(s => s.UserId == userId
                                                && !s.IsRead
                                                && s.DeletedAt == null
                                                && s.Message.SenderId != userId);

        return Ok(count);
    }

    // ───────────────────────────── User search ─────────────────────────────

    [HttpGet("users/search")]
    [ProducesResponseType(typeof(List<UserSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<UserSummaryDto>>> SearchUsersAsync([FromQuery] string q,
                                                                            [FromQuery] int    take = UserSearchMaxResult)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        if(string.IsNullOrWhiteSpace(q) || q.Trim().Length < UserSearchMinChars)
            return Ok(new List<UserSummaryDto>());

        if(take is < 1 or > UserSearchMaxResult) take = UserSearchMaxResult;

        string needle = q.Trim();

        var hits = await context.Users
            .Where(u => u.EmailConfirmed
                     && !u.IsSystemAccount
                     && u.Id != userId
                     && (u.UserName!.StartsWith(needle) || (u.DisplayName != null && u.DisplayName.StartsWith(needle))))
            .OrderBy(u => u.DisplayName ?? u.UserName)
            .Take(take)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.DisplayName,
                u.Email,
                u.UseGravatar,
                u.AvatarGuid,
                u.IsSystemAccount
            })
            .ToListAsync();

        var hitIds          = hits.Select(h => h.Id).ToList();
        var adminIds        = await GetAdminIdsAsync(hitIds);
        var collaboratorIds = await GetCollaboratorIdsAsync(hitIds);

        return Ok(hits.Select(h => new UserSummaryDto
        {
            Id             = h.Id,
            UserName       = h.UserName,
            DisplayName    = h.DisplayName ?? h.UserName,
            AvatarUrl      = BuildAvatarUrl(h.UseGravatar, h.Email, h.AvatarGuid),
            IsSystem       = h.IsSystemAccount,
            IsAdmin        = adminIds.Contains(h.Id),
            IsCollaborator = collaboratorIds.Contains(h.Id)
        }).ToList());
    }

    // ───────────────────────────── Reports admin ─────────────────────────────

    [HttpGet("reports")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(typeof(List<MessageReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<MessageReportDto>>> GetReportsAsync(
        [FromQuery] bool includeResolved = false,
        [FromQuery] int  page            = 1,
        [FromQuery] int  pageSize        = 25)
    {
        if(page < 1) page = 1;
        if(pageSize is < 1 or > 200) pageSize = 25;

        IQueryable<MessageReport> query = context.MessageReports;

        if(!includeResolved) query = query.Where(r => !r.IsResolved);

        var rows = await query
            .OrderByDescending(r => r.CreatedOn)
            .Select(r => new
            {
                r.Id,
                r.MessageId,
                ConversationId = r.Message == null ? (long?)null : (long?)r.Message.ConversationId,
                Reporter = r.Reporter == null
                               ? null
                               : new
                               {
                                   r.Reporter.Id,
                                   r.Reporter.UserName,
                                   r.Reporter.DisplayName,
                                   r.Reporter.Email,
                                   r.Reporter.UseGravatar,
                                   r.Reporter.AvatarGuid,
                                   r.Reporter.IsSystemAccount
                               },
                r.Reason,
                r.Explanation,
                r.IsResolved,
                ResolvedBy = r.ResolvedBy == null
                                 ? null
                                 : new
                                 {
                                     r.ResolvedBy.Id,
                                     r.ResolvedBy.UserName,
                                     r.ResolvedBy.DisplayName,
                                     r.ResolvedBy.Email,
                                     r.ResolvedBy.UseGravatar,
                                     r.ResolvedBy.AvatarGuid,
                                     r.ResolvedBy.IsSystemAccount
                                 },
                r.ResolvedOn,
                r.CreatedOn
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var ids = rows.SelectMany(r => new[] { r.Reporter?.Id, r.ResolvedBy?.Id })
                      .Where(id => id is not null)
                      .Distinct()
                      .ToList();

        HashSet<string> adminIds = await GetAdminIdsAsync(ids!);

        UserSummaryDto Build(string id, string userName, string displayName, string email, bool useGravatar,
                             Guid? avatarGuid, bool isSystem)
            => new()
            {
                Id          = id,
                UserName    = userName,
                DisplayName = displayName ?? userName,
                AvatarUrl   = BuildAvatarUrl(useGravatar, email, avatarGuid),
                IsSystem    = isSystem,
                IsAdmin     = adminIds.Contains(id)
            };

        return Ok(rows.Select(r => new MessageReportDto
        {
            Id             = r.Id,
            MessageId      = r.MessageId,
            ConversationId = r.ConversationId,
            Reason         = r.Reason,
            Explanation    = r.Explanation,
            IsResolved     = r.IsResolved,
            ResolvedOn     = r.ResolvedOn,
            CreatedOn      = r.CreatedOn,
            Reporter = r.Reporter == null
                           ? null
                           : Build(r.Reporter.Id, r.Reporter.UserName, r.Reporter.DisplayName, r.Reporter.Email,
                                   r.Reporter.UseGravatar, r.Reporter.AvatarGuid, r.Reporter.IsSystemAccount),
            ResolvedBy = r.ResolvedBy == null
                             ? null
                             : Build(r.ResolvedBy.Id, r.ResolvedBy.UserName, r.ResolvedBy.DisplayName, r.ResolvedBy.Email,
                                     r.ResolvedBy.UseGravatar, r.ResolvedBy.AvatarGuid, r.ResolvedBy.IsSystemAccount)
        }).ToList());
    }

    [HttpGet("reports/count")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<int>> GetReportsCountAsync([FromQuery] bool includeResolved = false)
    {
        IQueryable<MessageReport> query = context.MessageReports;

        if(!includeResolved) query = query.Where(r => !r.IsResolved);

        int total = await query.CountAsync();

        return Ok(total);
    }

    [HttpPut("reports/{id:long}/resolve")]
    [Authorize(Roles = "Admin,UberAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveReportAsync(long id)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);
        if(userId is null) return Unauthorized();

        MessageReport report = await context.MessageReports.FirstOrDefaultAsync(r => r.Id == id);
        if(report is null) return NotFound();

        if(!report.IsResolved)
        {
            report.IsResolved       = true;
            report.ResolvedByUserId = userId;
            report.ResolvedOn       = DateTime.UtcNow;
            await context.SaveChangesWithUserAsync(userId);
        }

        return NoContent();
    }

    // ───────────────────────────── Helpers ─────────────────────────────

    async Task<bool> IsAdminAsync(string userId)
    {
        ApplicationUser u = await userManager.FindByIdAsync(userId);
        if(u is null) return false;

        return await userManager.IsInRoleAsync(u, "Admin") || await userManager.IsInRoleAsync(u, "UberAdmin");
    }

    /// <summary>Returns the subset of <paramref name="userIds" /> that are in role Admin or UberAdmin.</summary>
    async Task<HashSet<string>> GetAdminIdsAsync(IReadOnlyCollection<string> userIds)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if(userIds.Count == 0) return result;

        foreach(ApplicationUser u in await userManager.GetUsersInRoleAsync("Admin"))
            if(userIds.Contains(u.Id)) result.Add(u.Id);

        foreach(ApplicationUser u in await userManager.GetUsersInRoleAsync("UberAdmin"))
            if(userIds.Contains(u.Id)) result.Add(u.Id);

        return result;
    }

    /// <summary>Returns the subset of <paramref name="userIds" /> that are in role Collaborator.</summary>
    async Task<HashSet<string>> GetCollaboratorIdsAsync(IReadOnlyCollection<string> userIds)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if(userIds.Count == 0) return result;

        foreach(ApplicationUser u in await userManager.GetUsersInRoleAsync("Collaborator"))
            if(userIds.Contains(u.Id)) result.Add(u.Id);

        return result;
    }

    static string BuildAvatarUrl(bool useGravatar, string email, Guid? avatarGuid)
        => UserAvatar.GetUrl(new ApplicationUser
        {
            UseGravatar = useGravatar, Email = email, AvatarGuid = avatarGuid
        });

    static string MakeSnippet(string body)
    {
        if(string.IsNullOrEmpty(body)) return string.Empty;

        // Crude: collapse whitespace, then truncate. UI will render markdown only on full body fetch.
        string flat = System.Text.RegularExpressions.Regex.Replace(body.Trim(), @"\s+", " ");

        return flat.Length <= SnippetLength ? flat : flat.Substring(0, SnippetLength) + "…";
    }

    static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max);

    static ProblemDetails BuildProblem(int status, string code, string detail,
                                        params (string Key, object Value)[] extensions)
    {
        var pd = new ProblemDetails
        {
            Status = status,
            Title  = code,
            Detail = detail
        };

        foreach((string key, object value) in extensions) pd.Extensions[key] = value;

        return pd;
    }
}
