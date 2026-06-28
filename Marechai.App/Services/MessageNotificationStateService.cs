#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Services.Authentication;
using Uno.Extensions.Authentication;

namespace Marechai.App.Services;

public sealed class MessageNotificationStateService : IDisposable
{
    readonly IAuthenticationService _authService;
    readonly IJwtService _jwtService;
    readonly MessagingService _messagingService;
    readonly INativeToastService _nativeToastService;
    readonly ITokenService _tokenService;
    readonly ILogger<MessageNotificationStateService> _logger;

    Timer? _timer;
    bool _initialized;
    bool _baselineSeeded;
    bool _isPolling;
    int _unreadCount;
    readonly HashSet<string> _seenUnreadKeys = [];

    public MessageNotificationStateService(IAuthenticationService authService, IJwtService jwtService,
        MessagingService messagingService, INativeToastService nativeToastService, ITokenService tokenService,
        ILogger<MessageNotificationStateService> logger)
    {
        _authService         = authService;
        _jwtService          = jwtService;
        _messagingService    = messagingService;
        _nativeToastService  = nativeToastService;
        _tokenService        = tokenService;
        _logger              = logger;

        _authService.LoggedOut += OnLoggedOut;

        if(_authService is AuthService concreteAuthService)
            concreteAuthService.LoggedIn += OnLoggedIn;
    }

    public event EventHandler<int>? UnreadCountChanged;

    public int UnreadCount => _unreadCount;

    public async Task InitializeAsync()
    {
        if(_initialized) return;
        _initialized = true;

        await _nativeToastService.InitializeAsync();
        await RefreshAsync(seedBaseline: true);
        _timer = new Timer(async _ => await TimerTickAsync(), null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
    }

    public async Task RefreshAsync(bool seedBaseline = false)
    {
        if(_isPolling) return;

        try
        {
            _isPolling = true;
            bool isAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);
            if(!isAuthenticated)
            {
                UpdateUnreadCount(0);
                _seenUnreadKeys.Clear();
                _baselineSeeded = false;
                return;
            }

            int unreadCount = await _messagingService.GetUnreadCountAsync();
            var (conversations, error) = await _messagingService.GetConversationsAsync("inbox", 1, 25);

            if(error is not null)
            {
                _logger.LogWarning("Could not refresh message notifications: {Error}", error);
                UpdateUnreadCount(unreadCount);
                return;
            }

            var unreadConversations = conversations.Where(c => (c.UnreadCount ?? 0) > 0).ToList();

            if(seedBaseline || !_baselineSeeded)
            {
                SeedBaseline(unreadConversations);
                UpdateUnreadCount(unreadCount);
                _baselineSeeded = true;
                return;
            }

            if(unreadCount > _unreadCount)
                await DispatchToastsAsync(unreadConversations);

            SeedBaseline(unreadConversations);
            UpdateUnreadCount(unreadCount);
        }
        finally
        {
            _isPolling = false;
        }
    }

    void SeedBaseline(IEnumerable<ConversationSummaryDto> unreadConversations)
    {
        _seenUnreadKeys.Clear();

        foreach(ConversationSummaryDto conversation in unreadConversations)
        {
            string? key = BuildUnreadKey(conversation);
            if(key is not null) _seenUnreadKeys.Add(key);
        }
    }

    async Task DispatchToastsAsync(List<ConversationSummaryDto> unreadConversations)
    {
        string? currentUserId = GetCurrentUserId();
        List<ToastMessage> pendingToasts = [];

        foreach(ConversationSummaryDto conversation in unreadConversations)
        {
            string? key = BuildUnreadKey(conversation);
            if(key is null || _seenUnreadKeys.Contains(key)) continue;

            MessageSummaryDto? summary = conversation.LatestMessage?.MessageSummaryDto;
            if(summary?.Id is null) continue;

            string title = summary.IsSystemAuthored == true
                ? "System"
                : summary.SenderDisplayName ?? "New message";

            if(summary is not null && currentUserId is not null)
            {
                // SenderDisplayName is all we have in the summary payload, so suppress only by exact
                // self/system cases that can be inferred here.
                if(summary.IsSystemAuthored != true &&
                   string.Equals(summary.SenderDisplayName, _jwtService.GetUserName(_tokenService.GetToken()),
                       StringComparison.Ordinal))
                    continue;
            }

            pendingToasts.Add(new ToastMessage(
                conversation.Id ?? 0,
                summary.Id,
                title,
                conversation.Subject ?? summary.Snippet ?? "New message"));
        }

        if(pendingToasts.Count == 0) return;

        bool allowed = await _nativeToastService.EnsurePermissionAsync();
        if(!allowed) return;

        if(pendingToasts.Count > 3)
        {
            await _nativeToastService.ShowAsync(new ToastMessage(
                pendingToasts[0].ConversationId,
                pendingToasts[0].MessageId,
                "New messages",
                $"You have {pendingToasts.Count} new unread conversations.",
                true));

            return;
        }

        foreach(ToastMessage toast in pendingToasts)
            await _nativeToastService.ShowAsync(toast);
    }

    string? GetCurrentUserId()
    {
        string token = _tokenService.GetToken();
        return string.IsNullOrWhiteSpace(token) ? null : _jwtService.GetUserId(token);
    }

    static string? BuildUnreadKey(ConversationSummaryDto conversation)
    {
        long? conversationId = conversation.Id;
        long? messageId = conversation.LatestMessage?.MessageSummaryDto?.Id;
        if(conversationId is null || messageId is null) return null;

        return $"{conversationId}:{messageId}";
    }

    void UpdateUnreadCount(int unreadCount)
    {
        if(_unreadCount == unreadCount) return;
        _unreadCount = unreadCount;
        UnreadCountChanged?.Invoke(this, unreadCount);
    }

    async Task TimerTickAsync()
    {
        try
        {
            await RefreshAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while polling message notifications");
        }
    }

    void OnLoggedIn(object? sender, EventArgs e) => _ = RefreshAsync(seedBaseline: true);

    void OnLoggedOut(object? sender, EventArgs e)
    {
        _seenUnreadKeys.Clear();
        _baselineSeeded = false;
        UpdateUnreadCount(0);
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _authService.LoggedOut -= OnLoggedOut;

        if(_authService is AuthService concreteAuthService)
            concreteAuthService.LoggedIn -= OnLoggedIn;
    }
}
