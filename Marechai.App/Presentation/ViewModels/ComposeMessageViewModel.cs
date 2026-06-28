#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class ComposeMessageViewModel : ObservableObject, IRegionAware
{
    readonly IRegionManager _regionManager;
    readonly ILogger<ComposeMessageViewModel> _logger;
    readonly MessagingService _messagingService;
    readonly MessageNotificationStateService _messageNotificationStateService;

    [ObservableProperty] string _subject = string.Empty;
    [ObservableProperty] string _body = string.Empty;
    [ObservableProperty] string _errorMessage = string.Empty;
    [ObservableProperty] bool _hasError;
    [ObservableProperty] bool _isSending;
    [ObservableProperty] UserSummaryDto? _selectedRecipient;
    [ObservableProperty] int _selectedEditorTabIndex;

    public ObservableCollection<UserSummaryDto> RecipientSuggestions { get; } = [];

    public ComposeMessageViewModel(MessagingService messagingService, IRegionManager regionManager,
        ILogger<ComposeMessageViewModel> logger, MessageNotificationStateService messageNotificationStateService)
    {
        _messagingService                = messagingService;
        _regionManager                   = regionManager;
        _logger                          = logger;
        _messageNotificationStateService = messageNotificationStateService;
    }

    public int BodyLength => Body?.Length ?? 0;
    public bool CanSend => !IsSending && SelectedRecipient?.Id is not null && !string.IsNullOrWhiteSpace(Body) &&
                           BodyLength <= MessagingService.MaxBodyLength;

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
    public void OnNavigatedTo(NavigationContext navigationContext) { }

    partial void OnBodyChanged(string value)
    {
        OnPropertyChanged(nameof(BodyLength));
        OnPropertyChanged(nameof(CanSend));
    }

    partial void OnSelectedRecipientChanged(UserSummaryDto? value) => OnPropertyChanged(nameof(CanSend));
    partial void OnIsSendingChanged(bool value) => OnPropertyChanged(nameof(CanSend));

    [RelayCommand]
    async Task SearchRecipientsAsync(string? query)
    {
        RecipientSuggestions.Clear();

        if(string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2) return;

        try
        {
            foreach(UserSummaryDto user in await _messagingService.SearchUsersAsync(query))
                RecipientSuggestions.Add(user);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error searching messaging recipients");
        }
    }

    [RelayCommand]
    void SelectRecipient(UserSummaryDto? user)
    {
        SelectedRecipient = user;
    }

    [RelayCommand]
    Task GoBack()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(MessagesPage));
        return Task.CompletedTask;
    }

    [RelayCommand]
    async Task SendAsync()
    {
        await SendMessageAsync();
    }

    public async Task<(MessageSendResult Result, long? ConversationId, string? Error)> SendMessageAsync()
    {
        if(!CanSend || SelectedRecipient?.Id is null)
            return (MessageSendResult.Failed, null, "Recipient and message body are required.");

        try
        {
            IsSending    = true;
            HasError     = false;
            ErrorMessage = string.Empty;

            (MessageSendResult result, long? conversationId, string? error) =
                await _messagingService.StartConversationAsync(SelectedRecipient.Id, Subject, Body);

            switch(result)
            {
                case MessageSendResult.Sent when conversationId.HasValue:
                    await _messageNotificationStateService.RefreshAsync();
                    var parameters = new NavigationParameters { { NavParamKeys.ConversationId, conversationId.Value } };
                    _regionManager.RequestNavigate(RegionNames.Content, nameof(MessageThreadPage), parameters);
                    return (result, conversationId, null);
                case MessageSendResult.InboxFull:
                    HasError     = true;
                    ErrorMessage = error ?? "The recipient inbox is full.";
                    return (result, conversationId, ErrorMessage);
                case MessageSendResult.RateLimited:
                    HasError     = true;
                    ErrorMessage = error ?? "Rate limited.";
                    return (result, conversationId, ErrorMessage);
                default:
                    HasError     = true;
                    ErrorMessage = error ?? "Failed to send message.";
                    return (result, conversationId, ErrorMessage);
            }
        }
        finally
        {
            IsSending = false;
        }

        return (MessageSendResult.Failed, null, ErrorMessage);
    }
}
