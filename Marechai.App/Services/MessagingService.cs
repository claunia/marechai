#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public enum MessageSendResult
{
    Sent,
    InboxFull,
    RateLimited,
    Failed
}

public sealed class MessagingService(Client client)
{
    public const int MaxBodyLength = 5000;

    static string ExtractErrorMessage(ApiException ex)
    {
        if(ex is ProblemDetails pd) return pd.Detail ?? pd.Title ?? ex.Message;

        return ex.Message;
    }

    static string? GetProblemTitle(ApiException ex) => ex is ProblemDetails pd ? pd.Title : null;

    public async Task<(List<ConversationSummaryDto> Conversations, string? Error)> GetConversationsAsync(
        string folder = "inbox", int page = 1, int pageSize = 25)
    {
        try
        {
            List<ConversationSummaryDto>? result = await client.Messages.Conversations.GetAsync(c =>
            {
                c.QueryParameters.Folder   = folder;
                c.QueryParameters.Page     = page;
                c.QueryParameters.PageSize = pageSize;
            });

            return (result ?? [], null);
        }
        catch(ApiException ex)
        {
            return ([], ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return ([], ex.Message);
        }
    }

    public async Task<(int Total, string? Error)> GetConversationsCountAsync(string folder = "inbox")
    {
        try
        {
            int? result = await client.Messages.Conversations.Count.GetAsync(c => c.QueryParameters.Folder = folder);

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

    public async Task<(ConversationDto? Conversation, string? Error)> GetConversationAsync(long id)
    {
        try
        {
            ConversationDto? result = await client.Messages.Conversations[id].GetAsync();

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

    public async Task<(MessageSendResult Result, long? ConversationId, string? Error)> StartConversationAsync(
        string recipientId, string? subject, string body)
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
            return GetProblemTitle(ex) switch
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

    public async Task<(MessageSendResult Result, long? MessageId, string? Error)> ReplyAsync(long conversationId,
        string body)
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
            return GetProblemTitle(ex) switch
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

    public async Task<(bool Succeeded, string? Error)> DeleteConversationAsync(long id)
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

    public async Task<(bool Succeeded, string? Error)> DeleteMessageAsync(long id)
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

    public async Task<(bool Succeeded, string? Error)> ReportMessageAsync(long messageId, ReviewReportReason reason,
        string? explanation)
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
        if(string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2) return [];

        try
        {
            List<UserSummaryDto>? result = await client.Messages.Users.Search.GetAsync(c =>
            {
                c.QueryParameters.Q    = query.Trim();
                c.QueryParameters.Take = take;
            });

            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(List<MessageReportDto> Reports, string? Error)> GetReportsAsync(bool includeResolved = false,
        int page = 1, int pageSize = 25)
    {
        try
        {
            List<MessageReportDto>? result = await client.Messages.Reports.GetAsync(c =>
            {
                c.QueryParameters.IncludeResolved = includeResolved;
                c.QueryParameters.Page            = page;
                c.QueryParameters.PageSize        = pageSize;
            });

            return (result ?? [], null);
        }
        catch(ApiException ex)
        {
            return ([], ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return ([], ex.Message);
        }
    }

    public async Task<(int Total, string? Error)> GetReportsCountAsync(bool includeResolved = false)
    {
        try
        {
            int? result = await client.Messages.Reports.Count.GetAsync(c =>
                c.QueryParameters.IncludeResolved = includeResolved);

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

    public async Task<(bool Succeeded, string? Error)> ResolveReportAsync(long id)
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
}
