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
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels.Admin;

[Bindable]
public partial class AdminMessageReportsViewModel : ObservableObject, IRegionAware
{
    const int PageSize = 25;

    readonly IJwtService _jwtService;
    readonly IStringLocalizer _localizer;
    readonly IRegionManager _regionManager;
    readonly ILogger<AdminMessageReportsViewModel> _logger;
    readonly MessagingService _messagingService;
    readonly ITokenService _tokenService;

    [ObservableProperty] bool _isAdmin;
    [ObservableProperty] bool _includeResolved;
    [ObservableProperty] bool _isLoading;
    [ObservableProperty] bool _hasError;
    [ObservableProperty] bool _isDataLoaded;
    [ObservableProperty] string _errorMessage = string.Empty;
    [ObservableProperty] int _currentPage = 1;
    [ObservableProperty] int _totalCount;

    public ObservableCollection<MessageReportListItem> Reports { get; } = [];

    public AdminMessageReportsViewModel(MessagingService messagingService, IRegionManager regionManager,
        ILogger<AdminMessageReportsViewModel> logger, ITokenService tokenService, IJwtService jwtService,
        IStringLocalizer localizer)
    {
        _messagingService = messagingService;
        _regionManager    = regionManager;
        _logger           = logger;
        _tokenService     = tokenService;
        _jwtService       = jwtService;
        _localizer        = localizer;
    }

    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage * PageSize < TotalCount;

    public string PaginationText
    {
        get
        {
            if(TotalCount == 0) return string.Empty;

            int rangeStart = (CurrentPage - 1) * PageSize + 1;
            int rangeEnd   = Math.Min(CurrentPage * PageSize, TotalCount);

            return string.Format(_localizer["MessageReportsPaginationFormat"], rangeStart, rangeEnd, TotalCount);
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        UpdateAdminStatus();
        if(IsAdmin) _ = LoadAsync();
    }

    [RelayCommand]
    Task Refresh() => LoadAsync();

    [RelayCommand]
    Task ToggleResolved()
    {
        CurrentPage = 1;
        return LoadAsync();
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
    Task OpenConversation(MessageReportListItem? item)
    {
        if(item?.ConversationId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters { { NavParamKeys.ConversationId, item.ConversationId.Value } };
        _regionManager.RequestNavigate(RegionNames.Content, nameof(MessageThreadPage), parameters);
        return Task.CompletedTask;
    }

    [RelayCommand]
    async Task ResolveReportAsync(MessageReportListItem? item)
    {
        if(item is null) return;

        (bool succeeded, string? error) = await _messagingService.ResolveReportAsync(item.Id);
        if(!succeeded)
        {
            HasError     = true;
            ErrorMessage = error ?? _localizer["MessageReportsFailedToResolve"];
            return;
        }

        await LoadAsync();
    }

    async Task LoadAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            IsDataLoaded = false;
            Reports.Clear();

            (int total, string? countError) = await _messagingService.GetReportsCountAsync(IncludeResolved);
            if(countError is not null)
            {
                HasError     = true;
                ErrorMessage = countError;
            }

            TotalCount = total;

            (var reports, string? error) = await _messagingService.GetReportsAsync(IncludeResolved, CurrentPage, PageSize);
            if(error is not null)
            {
                HasError     = true;
                ErrorMessage = error;
            }

            foreach(MessageReportDto report in reports)
                Reports.Add(new MessageReportListItem
                {
                    Id              = report.Id ?? 0,
                    ConversationId  = report.ConversationId,
                    HasConversation = report.ConversationId.HasValue,
                    Reporter        = report.Reporter?.DisplayName ?? "(deleted)",
                    ReasonText      = FormatReason(report.Reason),
                    Explanation     = report.Explanation ?? string.Empty,
                    CreatedOnText   = report.CreatedOn?.LocalDateTime.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
                    StatusText = report.IsResolved == true
                        ? _localizer["MessageReportsStatusResolved"]
                        : _localizer["MessageReportsStatusPending"],
                    IsResolved = report.IsResolved == true
                });

            IsDataLoaded = !HasError;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading message reports");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(PaginationText));
        }
    }

    void UpdateAdminStatus()
    {
        string token = _tokenService.GetToken();
        IsAdmin = _jwtService.GetRoles(token).Any(r =>
            string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, "UberAdmin", StringComparison.OrdinalIgnoreCase));
    }

    string FormatReason(int? code) => code switch
    {
        (int)ReviewReportReason.Spam       => _localizer["MessageReportsReasonSpam"],
        (int)ReviewReportReason.Offensive  => _localizer["MessageReportsReasonOffensive"],
        (int)ReviewReportReason.Misleading => _localizer["MessageReportsReasonMisleading"],
        (int)ReviewReportReason.OffTopic   => _localizer["MessageReportsReasonOffTopic"],
        _                                  => _localizer["MessageReportsReasonOther"]
    };
}
