#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.Data;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class MessageThreadViewModel : ObservableObject, IRegionAware
{
    readonly IJwtService _jwtService;
    readonly IRegionManager _regionManager;
    readonly ILogger<MessageThreadViewModel> _logger;
    readonly MessagingService _messagingService;
    readonly MessageNotificationStateService _messageNotificationStateService;
    readonly ITokenService _tokenService;

    long _conversationId;
    string? _currentUserId;

    [ObservableProperty] string _subject = "Conversation";
    [ObservableProperty] string _participantsText = string.Empty;
    [ObservableProperty] string _replyBody = string.Empty;
    [ObservableProperty] string _errorMessage = string.Empty;
    [ObservableProperty] string _infoMessage = string.Empty;
    [ObservableProperty] bool _hasError;
    [ObservableProperty] bool _isLoading;
    [ObservableProperty] bool _isDataLoaded;
    [ObservableProperty] bool _isAdmin;
    [ObservableProperty] bool _isSystemThread;
    [ObservableProperty] bool _isSendingReply;
    [ObservableProperty] int _selectedEditorTabIndex;

    public ObservableCollection<MessageThreadItem> Messages { get; } = [];

    public MessageThreadViewModel(MessagingService messagingService, IRegionManager regionManager,
        ILogger<MessageThreadViewModel> logger, ITokenService tokenService, IJwtService jwtService,
        MessageNotificationStateService messageNotificationStateService)
    {
        _messagingService                = messagingService;
        _regionManager                   = regionManager;
        _logger                          = logger;
        _tokenService                    = tokenService;
        _jwtService                      = jwtService;
        _messageNotificationStateService = messageNotificationStateService;
    }

    public int ReplyLength => ReplyBody?.Length ?? 0;
    public bool CanReply => !IsSendingReply && !IsLoading && !IsSystemThread || IsAdmin;
    public bool CanSendReply => CanReply && !string.IsNullOrWhiteSpace(ReplyBody) &&
                                ReplyLength <= MessagingService.MaxBodyLength;

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<long>(NavParamKeys.ConversationId, out long conversationId))
        {
            _conversationId = conversationId;
            UpdateIdentity();
            _ = LoadAsync();
        }
    }

    partial void OnReplyBodyChanged(string value)
    {
        OnPropertyChanged(nameof(ReplyLength));
        OnPropertyChanged(nameof(CanSendReply));
    }

    partial void OnIsSendingReplyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanReply));
        OnPropertyChanged(nameof(CanSendReply));
    }

    partial void OnIsSystemThreadChanged(bool value)
    {
        OnPropertyChanged(nameof(CanReply));
        OnPropertyChanged(nameof(CanSendReply));
    }

    [RelayCommand]
    Task GoBack()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(MessagesPage));
        return Task.CompletedTask;
    }

    [RelayCommand]
    Task Refresh() => LoadAsync();

    [RelayCommand]
    async Task SendReplyAsync()
    {
        await SendReplyCoreAsync();
    }

    public async Task<(MessageSendResult Result, string? Error)> SendReplyCoreAsync()
    {
        if(!CanSendReply) return (MessageSendResult.Failed, "Reply cannot be sent.");

        try
        {
            IsSendingReply = true;
            HasError       = false;
            ErrorMessage   = string.Empty;
            InfoMessage    = string.Empty;

            (MessageSendResult result, _, string? error) = await _messagingService.ReplyAsync(_conversationId, ReplyBody);

            switch(result)
            {
                case MessageSendResult.Sent:
                    ReplyBody    = string.Empty;
                    InfoMessage  = "Reply sent.";
                    await _messageNotificationStateService.RefreshAsync();
                    await LoadAsync();
                    return (result, null);
                case MessageSendResult.InboxFull:
                    HasError     = true;
                    ErrorMessage = error ?? "The recipient inbox is full.";
                    return (result, ErrorMessage);
                case MessageSendResult.RateLimited:
                    HasError     = true;
                    ErrorMessage = error ?? "Rate limited.";
                    return (result, ErrorMessage);
                default:
                    HasError     = true;
                    ErrorMessage = error ?? "Failed to send reply.";
                    return (result, ErrorMessage);
            }
        }
        finally
        {
            IsSendingReply = false;
        }

        return (MessageSendResult.Failed, ErrorMessage);
    }

    [RelayCommand]
    async Task DeleteConversationAsync()
    {
        await DeleteConversationCoreAsync();
    }

    public async Task<(bool Succeeded, string? Error)> DeleteConversationCoreAsync()
    {
        (bool succeeded, string? error) = await _messagingService.DeleteConversationAsync(_conversationId);
        if(!succeeded)
        {
            HasError     = true;
            ErrorMessage = error ?? "Failed to delete conversation.";
            return (false, ErrorMessage);
        }

        await _messageNotificationStateService.RefreshAsync();
        _regionManager.RequestNavigate(RegionNames.Content, nameof(MessagesPage));
        return (true, null);
    }

    [RelayCommand]
    async Task DeleteMessageAsync(MessageThreadItem? item)
    {
        await DeleteMessageCoreAsync(item);
    }

    public async Task<(bool Succeeded, string? Error)> DeleteMessageCoreAsync(MessageThreadItem? item)
    {
        if(item is null) return (false, "Message not found.");

        (bool succeeded, string? error) = await _messagingService.DeleteMessageAsync(item.Id);
        if(!succeeded)
        {
            HasError     = true;
            ErrorMessage = error ?? "Failed to delete message.";
            return (false, ErrorMessage);
        }

        await _messageNotificationStateService.RefreshAsync();
        await LoadAsync();
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> ReportMessageAsync(MessageThreadItem? item,
        ReviewReportReason reason, string? explanation)
    {
        if(item is null) return (false, "Message not found.");

        (bool succeeded, string? error) = await _messagingService.ReportMessageAsync(item.Id, reason, explanation);
        if(succeeded)
            InfoMessage = "Report submitted.";

        return (succeeded, error);
    }

    async Task LoadAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            InfoMessage  = string.Empty;
            IsDataLoaded = false;
            Messages.Clear();

            (ConversationDto? conversation, string? error) = await _messagingService.GetConversationAsync(_conversationId);
            if(conversation is null)
            {
                HasError     = true;
                ErrorMessage = error ?? "Conversation not found.";
                return;
            }

            Subject        = conversation.Subject ?? "Conversation";
            IsSystemThread = conversation.IsSystemThread == true;
            ParticipantsText = string.Join(", ",
                (conversation.Participants ?? []).Select(p => p.DisplayName).Where(n => !string.IsNullOrWhiteSpace(n)));

            foreach(MessageDto message in conversation.Messages ?? [])
            {
                bool isMine = !string.IsNullOrWhiteSpace(_currentUserId) && message.Sender?.Id == _currentUserId;
                Messages.Add(new MessageThreadItem
                {
                    Id               = message.Id ?? 0,
                    SenderDisplayName = message.Sender?.DisplayName ?? "(deleted)",
                    Body             = message.Body ?? string.Empty,
                    TimestampText    = message.CreatedOn?.LocalDateTime.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
                    IsMine           = isMine,
                    IsSystemAuthored = message.IsSystemAuthored == true,
                    IsRead           = message.IsRead == true,
                    CanReport        = !isMine && message.IsSystemAuthored != true
                });
            }

            await _messageNotificationStateService.RefreshAsync();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading conversation {ConversationId}", _conversationId);
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanReply));
            OnPropertyChanged(nameof(CanSendReply));
        }
    }

    void UpdateIdentity()
    {
        string token = _tokenService.GetToken();
        _currentUserId = _jwtService.GetUserId(token);
        IsAdmin = _jwtService.GetRoles(token).Any(r =>
            string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, "UberAdmin", StringComparison.OrdinalIgnoreCase));
    }
}
