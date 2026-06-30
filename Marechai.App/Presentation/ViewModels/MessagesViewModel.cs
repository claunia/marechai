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
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class MessagesViewModel : ObservableObject, IRegionAware
{
    const int PageSize = 25;

    readonly IJwtService _jwtService;
    readonly IRegionManager _regionManager;
    readonly ILogger<MessagesViewModel> _logger;
    readonly MessagingService _messagingService;
    readonly MessageNotificationStateService _messageNotificationStateService;
    readonly ITokenService _tokenService;

    [ObservableProperty] string _folder = "inbox";
    [ObservableProperty] bool _isLoading;
    [ObservableProperty] bool _hasError;
    [ObservableProperty] bool _isDataLoaded;
    [ObservableProperty] string _errorMessage = string.Empty;
    [ObservableProperty] int _currentPage = 1;
    [ObservableProperty] int _totalCount;
    [ObservableProperty] bool _isAdmin;

    public ObservableCollection<ConversationListItem> Conversations { get; } = [];

    public MessagesViewModel(MessagingService messagingService, IRegionManager regionManager,
        ILogger<MessagesViewModel> logger, ITokenService tokenService, IJwtService jwtService,
        MessageNotificationStateService messageNotificationStateService)
    {
        _messagingService               = messagingService;
        _regionManager                  = regionManager;
        _logger                         = logger;
        _tokenService                   = tokenService;
        _jwtService                     = jwtService;
        _messageNotificationStateService = messageNotificationStateService;
        UpdateAdminStatus();
    }

    public bool IsInboxSelected => Folder == "inbox";
    public bool IsSentSelected => Folder == "sent";
    public bool IsReportsSelected => Folder == "reports";
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage * PageSize < TotalCount;
    public string PageSummary => TotalCount <= 0
        ? "No messages"
        : $"Showing {((CurrentPage - 1) * PageSize) + 1}-{Math.Min(CurrentPage * PageSize, TotalCount)} of {TotalCount}";

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.MessageFolder, out string? folder) &&
           !string.IsNullOrWhiteSpace(folder))
            Folder = folder;

        _ = LoadAsync();
    }

    [RelayCommand]
    Task GoToInbox()
    {
        Folder = "inbox";
        CurrentPage = 1;
        OnPropertyChanged(nameof(IsInboxSelected));
        OnPropertyChanged(nameof(IsSentSelected));
        OnPropertyChanged(nameof(IsReportsSelected));
        return LoadAsync();
    }

    [RelayCommand]
    Task GoToSent()
    {
        Folder = "sent";
        CurrentPage = 1;
        OnPropertyChanged(nameof(IsInboxSelected));
        OnPropertyChanged(nameof(IsSentSelected));
        OnPropertyChanged(nameof(IsReportsSelected));
        return LoadAsync();
    }

    [RelayCommand]
    Task GoToReports()
    {
        Folder = "reports";
        CurrentPage = 1;
        OnPropertyChanged(nameof(IsInboxSelected));
        OnPropertyChanged(nameof(IsSentSelected));
        OnPropertyChanged(nameof(IsReportsSelected));
        return LoadAsync();
    }

    [RelayCommand]
    Task OpenCompose()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(ComposeMessagePage));
        return Task.CompletedTask;
    }

    [RelayCommand]
    Task OpenConversation(ConversationListItem? item)
    {
        if(item is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.ConversationId, item.Id }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MessageThreadPage), parameters);
        return Task.CompletedTask;
    }

    [RelayCommand]
    async Task DeleteConversationAsync(ConversationListItem? item)
    {
        await DeleteConversationCoreAsync(item);
    }

    public async Task<(bool Succeeded, string? Error)> DeleteConversationCoreAsync(ConversationListItem? item)
    {
        if(item is null) return (false, "Conversation not found.");

        (bool succeeded, string? error) = await _messagingService.DeleteConversationAsync(item.Id);
        if(!succeeded)
        {
            ErrorMessage = error ?? "Failed to delete conversation.";
            HasError     = true;
            return (false, ErrorMessage);
        }

        await _messageNotificationStateService.RefreshAsync();
        await LoadAsync();
        return (true, null);
    }

    [RelayCommand]
    Task NextPage()
    {
        if(!CanGoNext) return Task.CompletedTask;
        CurrentPage++;
        return LoadAsync();
    }

    [RelayCommand]
    Task PreviousPage()
    {
        if(!CanGoPrevious) return Task.CompletedTask;
        CurrentPage--;
        return LoadAsync();
    }

    [RelayCommand]
    Task Refresh() => LoadAsync();

    async Task LoadAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            IsDataLoaded = false;
            Conversations.Clear();

            (int total, string? countError) = await _messagingService.GetConversationsCountAsync(Folder);
            if(countError is not null)
            {
                HasError     = true;
                ErrorMessage = countError;
            }

            TotalCount = total;
            if(CurrentPage < 1) CurrentPage = 1;
            if(TotalCount > 0 && (CurrentPage - 1) * PageSize >= TotalCount)
                CurrentPage = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

            (var conversations, string? error) = await _messagingService.GetConversationsAsync(Folder, CurrentPage, PageSize);
            if(error is not null)
            {
                HasError     = true;
                ErrorMessage = error;
            }

            foreach(ConversationSummaryDto conversation in conversations)
                Conversations.Add(MapConversation(conversation));

            IsDataLoaded = !HasError;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading conversations");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(PageSummary));
        }
    }

    void UpdateAdminStatus()
    {
        string token = _tokenService.GetToken();
        if(string.IsNullOrWhiteSpace(token))
        {
            IsAdmin = false;
            return;
        }

        IsAdmin = _jwtService.GetRoles(token).Any(r =>
            string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, "UberAdmin", StringComparison.OrdinalIgnoreCase));
    }

    static ConversationListItem MapConversation(ConversationSummaryDto conversation)
    {
        MessageSummaryDto? latest = conversation.LatestMessage?.MessageSummaryDto;
        int unreadCount = conversation.UnreadCount ?? 0;
        string participantsText = string.Join(", ", (conversation.Participants ?? []).Select(p => p.DisplayName).Where(n => !string.IsNullOrWhiteSpace(n)));
        if(string.IsNullOrWhiteSpace(participantsText)) participantsText = "Conversation";

        return new ConversationListItem
        {
            Id               = conversation.Id ?? 0,
            Subject          = conversation.Subject ?? "Conversation",
            ParticipantsText = participantsText,
            PreviewText      = $"{latest?.SenderDisplayName}: {latest?.Snippet}".Trim(':', ' '),
            TimestampText    = FormatTimestamp(conversation.LastActivityOn),
            UnreadCount      = unreadCount,
            IsUnread         = unreadCount > 0,
            IsSystemThread   = conversation.IsSystemThread == true
        };
    }

    static string FormatTimestamp(DateTimeOffset? when)
    {
        if(!when.HasValue) return string.Empty;
        TimeSpan diff = DateTimeOffset.UtcNow - when.Value;
        if(diff.TotalMinutes < 1) return "now";
        if(diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if(diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        if(diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
        return when.Value.LocalDateTime.ToString("yyyy-MM-dd");
    }
}
