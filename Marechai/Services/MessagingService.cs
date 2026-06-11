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
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

/// <summary>
///     Outcome of a send / reply operation. Allows the UI to dispatch on the specific server-side rejection without
///     parsing strings.
/// </summary>
public enum MessageSendResult
{
    Sent,
    InboxFull,
    RateLimited,
    Failed
}

public class MessagingService(Marechai.ApiClient.Client client)
{
    public const int MaxBodyLength = 5000;

    static string ExtractErrorMessage(ApiException ex)
    {
        if(ex is ProblemDetails pd) return pd.Detail ?? pd.Title ?? ex.Message;

        return ex.Message;
    }

    static string GetProblemTitle(ApiException ex)
    {
        if(ex is ProblemDetails pd) return pd.Title;

        return null;
    }

    // ── Conversations ──

    public async Task<(List<ConversationSummaryDto> conversations, string error)> GetConversationsAsync(
        string folder = "inbox", int page = 1, int pageSize = 25)
    {
        try
        {
            List<ConversationSummaryDto> result = await client.Messages.Conversations.GetAsync(c =>
            {
                c.QueryParameters.Folder   = folder;
                c.QueryParameters.Page     = page;
                c.QueryParameters.PageSize = pageSize;
            });

            return (result ?? new List<ConversationSummaryDto>(), null);
        }
        catch(ApiException ex)
        {
            return (new List<ConversationSummaryDto>(), ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (new List<ConversationSummaryDto>(), ex.Message);
        }
    }

    /// <summary>
    ///     Returns the total number of conversations matching <paramref name="folder"/> for the current user.
    ///     Used by the inbox/sent/reports pager on <c>/messages</c>.
    /// </summary>
    public async Task<(int total, string error)> GetConversationsCountAsync(string folder = "inbox")
    {
        try
        {
            int? result = await client.Messages.Conversations.Count.GetAsync(c =>
            {
                c.QueryParameters.Folder = folder;
            });

            return (result ?? 0, null);
        }
        catch(ApiException ex)
        {
            return (0, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (0, ex.Message);
        }
    }

    public async Task<(ConversationDto conversation, string error)> GetConversationAsync(long id)
    {
        try
        {
            ConversationDto result = await client.Messages.Conversations[id].GetAsync();

            return (result, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(MessageSendResult result, long? conversationId, string error)> StartConversationAsync(
        string recipientId, string subject, string body)
    {
        if(string.IsNullOrWhiteSpace(body))
            return (MessageSendResult.Failed, null, "Message body is required.");

        if(body.Length > MaxBodyLength)
            return (MessageSendResult.Failed, null, $"Message exceeds {MaxBodyLength} character limit.");

        try
        {
            long? id = await client.Messages.Conversations.PostAsync(new CreateConversationRequest
            {
                RecipientId = recipientId,
                Subject     = subject,
                Body        = body
            });

            return (MessageSendResult.Sent, id, null);
        }
        catch(ApiException ex)
        {
            string title = GetProblemTitle(ex);

            return title switch
            {
                "INBOX_FULL"   => (MessageSendResult.InboxFull, null, ExtractErrorMessage(ex)),
                "RATE_LIMITED" => (MessageSendResult.RateLimited, null, ExtractErrorMessage(ex)),
                _              => (MessageSendResult.Failed, null, ExtractErrorMessage(ex))
            };
        }
        catch(Exception ex)
        {
            return (MessageSendResult.Failed, null, ex.Message);
        }
    }

    public async Task<(MessageSendResult result, long? messageId, string error)> ReplyAsync(long conversationId, string body)
    {
        if(string.IsNullOrWhiteSpace(body))
            return (MessageSendResult.Failed, null, "Message body is required.");

        if(body.Length > MaxBodyLength)
            return (MessageSendResult.Failed, null, $"Message exceeds {MaxBodyLength} character limit.");

        try
        {
            long? id = await client.Messages.Conversations[conversationId].Reply.PostAsync(new ReplyRequest
            {
                Body = body
            });

            return (MessageSendResult.Sent, id, null);
        }
        catch(ApiException ex)
        {
            string title = GetProblemTitle(ex);

            return title switch
            {
                "INBOX_FULL"   => (MessageSendResult.InboxFull, null, ExtractErrorMessage(ex)),
                "RATE_LIMITED" => (MessageSendResult.RateLimited, null, ExtractErrorMessage(ex)),
                _              => (MessageSendResult.Failed, null, ExtractErrorMessage(ex))
            };
        }
        catch(Exception ex)
        {
            return (MessageSendResult.Failed, null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteConversationAsync(long id)
    {
        try
        {
            await client.Messages.Conversations[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteMessageAsync(long id)
    {
        try
        {
            await client.Messages.Messages[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<int> GetUnreadCountAsync()
    {
        try
        {
            int? count = await client.Messages.UnreadCount.GetAsync();

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<(bool succeeded, string error)> ReportMessageAsync(long messageId,
                                                                          Marechai.Data.ReviewReportReason reason,
                                                                          string explanation)
    {
        try
        {
            await client.Messages.Messages[messageId].Report.PostAsync(new CreateMessageReportRequest
            {
                Reason      = (int)reason,
                Explanation = explanation
            });

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<List<UserSummaryDto>> SearchUsersAsync(string query, int take = 20)
    {
        if(string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2) return new List<UserSummaryDto>();

        try
        {
            List<UserSummaryDto> result = await client.Messages.Users.Search.GetAsync(c =>
            {
                c.QueryParameters.Q    = query.Trim();
                c.QueryParameters.Take = take;
            });

            return result ?? new List<UserSummaryDto>();
        }
        catch
        {
            return new List<UserSummaryDto>();
        }
    }

    // ── Reports admin ──

    public async Task<(List<MessageReportDto> reports, string error)> GetReportsAsync(
        bool includeResolved = false, int page = 1, int pageSize = 25)
    {
        try
        {
            List<MessageReportDto> result = await client.Messages.Reports.GetAsync(c =>
            {
                c.QueryParameters.IncludeResolved = includeResolved;
                c.QueryParameters.Page            = page;
                c.QueryParameters.PageSize        = pageSize;
            });

            return (result ?? new List<MessageReportDto>(), null);
        }
        catch(ApiException ex)
        {
            return (new List<MessageReportDto>(), ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (new List<MessageReportDto>(), ex.Message);
        }
    }

    /// <summary>
    ///     Returns the total number of reports matching the <paramref name="includeResolved"/> filter.
    ///     Used by the pager on <c>/admin/messages/reports</c>.
    /// </summary>
    public async Task<(int total, string error)> GetReportsCountAsync(bool includeResolved = false)
    {
        try
        {
            int? result = await client.Messages.Reports.Count.GetAsync(c =>
            {
                c.QueryParameters.IncludeResolved = includeResolved;
            });

            return (result ?? 0, null);
        }
        catch(ApiException ex)
        {
            return (0, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (0, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> ResolveReportAsync(long id)
    {
        try
        {
            await client.Messages.Reports[id].Resolve.PutAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    
    }

    static string ExtractDetail(ApiException ex)
    {
        if(ex is ProblemDetails pd)
        {
            if(!string.IsNullOrWhiteSpace(pd.Detail)) return pd.Detail;
            if(!string.IsNullOrWhiteSpace(pd.Title))  return pd.Title;
        }

        if(ex is { ResponseStatusCode: 0 } || string.IsNullOrWhiteSpace(ex.Message)) return "Unknown error";

        return ex.Message;
    }
}
