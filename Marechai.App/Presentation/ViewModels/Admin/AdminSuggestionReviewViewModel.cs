#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels.Admin;

[Bindable]
public partial class AdminSuggestionReviewViewModel : ObservableObject, IRegionAware
{
    readonly IJwtService _jwtService;
    readonly IStringLocalizer _localizer;
    readonly ILogger<AdminSuggestionReviewViewModel> _logger;
    readonly IRegionManager _regionManager;
    readonly SuggestionsService _suggestionsService;
    readonly ITokenService _tokenService;

    long _suggestionId;

    [ObservableProperty] bool _isAdmin;
    [ObservableProperty] bool _isLoading;
    [ObservableProperty] bool _hasError;
    [ObservableProperty] bool _isDataLoaded;
    [ObservableProperty] bool _isSubmitting;
    [ObservableProperty] string _errorMessage = string.Empty;
    [ObservableProperty] SuggestionListItem? _header;
    [ObservableProperty] string? _userComment;
    [ObservableProperty] bool _entityMissing;
    [ObservableProperty] string _adminComment = string.Empty;

    public ObservableCollection<SuggestionFieldDiffItem> Fields { get; } = [];

    public AdminSuggestionReviewViewModel(SuggestionsService suggestionsService,
        ILogger<AdminSuggestionReviewViewModel> logger, ITokenService tokenService, IJwtService jwtService,
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
        if(!IsAdmin) return;

        if(navigationContext.Parameters.TryGetValue<long>(NavParamKeys.SuggestionId, out long id)) _suggestionId = id;

        _ = LoadAsync();
    }

    [RelayCommand]
    Task Refresh() => LoadAsync();

    [RelayCommand]
    async Task AcceptSelectedAsync()
    {
        var accepted = Fields.Where(f => f.IsAccepted).Select(f => f.FieldName).ToList();
        await SubmitAsync(accepted);
    }

    [RelayCommand]
    Task RejectAllAsync() => SubmitAsync([]);

    [RelayCommand]
    void Cancel() => GoBack();

    async Task SubmitAsync(System.Collections.Generic.List<string> acceptedFieldNames)
    {
        IsSubmitting = true;
        HasError      = false;
        ErrorMessage  = string.Empty;

        (bool succeeded, string? error) =
            await _suggestionsService.ReviewAsync(_suggestionId, acceptedFieldNames, AdminComment);

        IsSubmitting = false;

        if(!succeeded)
        {
            HasError     = true;
            ErrorMessage = error ?? _localizer["SuggestionsSubmitFailed"];

            return;
        }

        GoBack();
    }

    void GoBack()
    {
        if(_regionManager.Regions[RegionNames.Content].NavigationService.Journal.CanGoBack)
            _regionManager.Regions[RegionNames.Content].NavigationService.Journal.GoBack();
        else
            _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSuggestionsPage));
    }

    async Task LoadAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            IsDataLoaded = false;
            Fields.Clear();

            (SuggestionListItem? header, var fields, string? comment, bool entityMissing, string? error) =
                await _suggestionsService.GetDiffAsync(_suggestionId);

            if(error is not null || header is null)
            {
                HasError     = true;
                ErrorMessage = error ?? _localizer["SuggestionsLoadFailed"];

                return;
            }

            Header        = header;
            UserComment   = comment;
            EntityMissing = entityMissing;

            foreach(SuggestionFieldDiffItem field in fields) Fields.Add(field);

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading suggestion diff");
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
        IsAdmin = _jwtService.GetRoles(token).Any(r =>
            string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, "UberAdmin", StringComparison.OrdinalIgnoreCase));
    }
}
