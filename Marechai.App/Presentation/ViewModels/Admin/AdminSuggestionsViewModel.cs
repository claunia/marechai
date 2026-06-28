#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.Data;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels.Admin;

[Bindable]
public partial class AdminSuggestionsViewModel : ObservableObject, IRegionAware
{
    readonly IJwtService _jwtService;
    readonly IStringLocalizer _localizer;
    readonly ILogger<AdminSuggestionsViewModel> _logger;
    readonly IRegionManager _regionManager;
    readonly SuggestionsService _suggestionsService;
    readonly ITokenService _tokenService;

    [ObservableProperty] bool _isAdmin;
    [ObservableProperty] bool _isUberAdmin;
    [ObservableProperty] bool _includeHistory;
    [ObservableProperty] bool _isLoading;
    [ObservableProperty] bool _hasError;
    [ObservableProperty] bool _isDataLoaded;
    [ObservableProperty] string _errorMessage = string.Empty;

    public ObservableCollection<SuggestionListItem> Suggestions { get; } = [];

    public AdminSuggestionsViewModel(SuggestionsService suggestionsService,
        ILogger<AdminSuggestionsViewModel> logger, ITokenService tokenService, IJwtService jwtService,
        IStringLocalizer localizer, IRegionManager regionManager)
    {
        _suggestionsService = suggestionsService;
        _logger             = logger;
        _tokenService       = tokenService;
        _jwtService         = jwtService;
        _localizer          = localizer;
        _regionManager      = regionManager;
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
    Task ToggleHistory() => LoadAsync();

    [RelayCommand]
    void OpenReview(SuggestionListItem? item)
    {
        if(item is null) return;

        var parameters = new NavigationParameters { { NavParamKeys.SuggestionId, item.Id } };
        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSuggestionReviewPage), parameters);
    }

    async Task LoadAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            IsDataLoaded = false;
            Suggestions.Clear();

            (var suggestions, string? error) =
                await _suggestionsService.GetQueueAsync(IsUberAdmin && IncludeHistory);

            if(error is not null)
            {
                HasError     = true;
                ErrorMessage = error;
            }

            foreach(SuggestionListItem item in suggestions) Suggestions.Add(item);

            IsDataLoaded = !HasError;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading suggestions queue");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    void UpdateAdminStatus()
    {
        string token = _tokenService.GetToken();
        var roles = _jwtService.GetRoles(token);

        IsAdmin = roles.Any(r =>
            string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, "UberAdmin", StringComparison.OrdinalIgnoreCase));

        IsUberAdmin = roles.Any(r => string.Equals(r, "UberAdmin", StringComparison.OrdinalIgnoreCase));
    }
}
