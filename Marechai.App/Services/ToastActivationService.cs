#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Marechai.App.Services;

public sealed class ToastActivationService
{
    static long? _pendingConversationId;

    public static ToastActivationService? Current { get; private set; }

    public event EventHandler<long>? ConversationActivated;

    public long? PendingConversationId { get; private set; }

    public ToastActivationService()
    {
        Current = this;
    }

    public static void ProcessLaunchArguments(IEnumerable<string>? args)
    {
        if(args is null) return;

        string[] values = args.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
        for(int i = 0; i < values.Length; i++)
        {
            string value = values[i];
            const string prefix = "--open-conversation=";

            if(value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
               long.TryParse(value[prefix.Length..], NumberStyles.Integer, CultureInfo.InvariantCulture,
                   out long inlineConversationId))
            {
                _pendingConversationId = inlineConversationId;
                return;
            }

            if(!value.Equals("--open-conversation", StringComparison.OrdinalIgnoreCase) || i + 1 >= values.Length)
                continue;

            if(long.TryParse(values[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture,
                   out long splitConversationId))
                _pendingConversationId = splitConversationId;

            return;
        }
    }

    public void ReportActivation(long conversationId)
    {
        PendingConversationId = conversationId;
        _pendingConversationId = conversationId;
        ConversationActivated?.Invoke(this, conversationId);
    }

    public bool TryConsumePendingConversation(out long conversationId)
    {
        long? pending = PendingConversationId ?? _pendingConversationId;
        if(!pending.HasValue)
        {
            conversationId = 0;
            return false;
        }

        conversationId = pending.Value;
        ClearPendingConversation();
        return true;
    }

    public void ClearPendingConversation()
    {
        PendingConversationId = null;
        _pendingConversationId = null;
    }
}

public sealed record ToastMessage(long ConversationId, long? MessageId, string Title, string Body, bool IsSummary = false);

public interface INativeToastService
{
    Task InitializeAsync();
    Task<bool> EnsurePermissionAsync();
    Task ShowAsync(ToastMessage toastMessage);
}
