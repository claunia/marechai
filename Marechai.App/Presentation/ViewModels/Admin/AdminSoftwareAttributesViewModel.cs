#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Presentation.Dialogs;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareAttributesViewModel : ObservableObject, IRegionAware
{
    const string SplitSeparator = ", ";

    readonly SoftwareAttributesService                 _attributesService;
    readonly SoftwareService                           _softwareService;
    readonly IJwtService                               _jwtService;
    readonly IStringLocalizer                          _localizer;
    readonly ILogger<AdminSoftwareAttributesViewModel> _logger;
    readonly ITokenService                             _tokenService;

    List<SoftwareDto> _allSoftware = [];
    List<SoftwareReleaseLookupDto> _filterReleases = [];
    List<SoftwareReleaseLookupDto> _editReleases = [];
    long? _editingAttributeId;

    [ObservableProperty] ObservableCollection<SoftwareAttributeDto> _attributes = [];
    [ObservableProperty] ObservableCollection<SoftwareDto> _filterSoftwareSuggestions = [];
    [ObservableProperty] ObservableCollection<SoftwareReleaseLookupDto> _filterReleaseSuggestions = [];
    [ObservableProperty] ObservableCollection<string> _filterCategorySuggestions = [];
    [ObservableProperty] ObservableCollection<string> _filterKeySuggestions = [];
    [ObservableProperty] ObservableCollection<SoftwareDto> _editSoftwareSuggestions = [];
    [ObservableProperty] ObservableCollection<SoftwareReleaseLookupDto> _editReleaseSuggestions = [];
    [ObservableProperty] ObservableCollection<string> _editCategorySuggestions = [];
    [ObservableProperty] ObservableCollection<string> _editKeySuggestions = [];
    [ObservableProperty] ObservableCollection<SplitFragmentResultDto> _splitPreviewFragments = [];
    [ObservableProperty] ObservableCollection<int> _pageSizeOptions = [10, 25, 50, 100];

    [ObservableProperty] string _filterSoftwareText = string.Empty;
    [ObservableProperty] string _filterReleaseText = string.Empty;
    [ObservableProperty] string _filterCategoryText = string.Empty;
    [ObservableProperty] string _filterKeyText = string.Empty;
    [ObservableProperty] SoftwareDto? _selectedFilterSoftware;
    [ObservableProperty] SoftwareReleaseLookupDto? _selectedFilterRelease;
    [ObservableProperty] bool _isLoading;
    [ObservableProperty] bool _isDataLoaded;
    [ObservableProperty] bool _isAdmin;
    [ObservableProperty] bool _isEditing;
    [ObservableProperty] bool _isEditingExisting;
    [ObservableProperty] bool _isSplitBusy;
    [ObservableProperty] string _editPanelTitle = string.Empty;
    [ObservableProperty] string _errorMessage = string.Empty;
    [ObservableProperty] string _successMessage = string.Empty;
    [ObservableProperty] string _editingContextText = string.Empty;
    [ObservableProperty] int _currentPage = 1;
    [ObservableProperty] int _pageSize = 25;
    [ObservableProperty] int _totalCount;

    [ObservableProperty] string _editSoftwareText = string.Empty;
    [ObservableProperty] SoftwareDto? _selectedEditSoftware;
    [ObservableProperty] SoftwareReleaseLookupDto? _selectedEditRelease;
    [ObservableProperty] string _editCategory = string.Empty;
    [ObservableProperty] string _editKey = string.Empty;
    [ObservableProperty] string _attributeValue = string.Empty;

    public AdminSoftwareAttributesViewModel(SoftwareAttributesService                 attributesService,
                                            SoftwareService                           softwareService,
                                            IJwtService                               jwtService,
                                            ITokenService                             tokenService,
                                            ILogger<AdminSoftwareAttributesViewModel> logger,
                                            IStringLocalizer                          localizer)
    {
        _attributesService = attributesService;
        _softwareService   = softwareService;
        _jwtService        = jwtService;
        _tokenService      = tokenService;
        _logger            = logger;
        _localizer         = localizer;

        LoadCommand         = new AsyncRelayCommand(LoadAsync);
        OpenAddCommand      = new AsyncRelayCommand(OpenAddAsync);
        OpenEditCommand     = new AsyncRelayCommand<SoftwareAttributeDto>(OpenEditAsync);
        DeleteCommand       = new AsyncRelayCommand<SoftwareAttributeDto>(DeleteAsync);
        SaveCommand         = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand   = new RelayCommand(CancelEdit);
        NextPageCommand     = new AsyncRelayCommand(NextPageAsync);
        PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
        PreviewSplitCommand = new AsyncRelayCommand(PreviewSplitAsync);
        ApplySplitCommand   = new AsyncRelayCommand(ApplySplitAsync);

        CheckAdminRole();
    }

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand OpenAddCommand { get; }
    public IAsyncRelayCommand<SoftwareAttributeDto> OpenEditCommand { get; }
    public IAsyncRelayCommand<SoftwareAttributeDto> DeleteCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelEditCommand { get; }
    public IAsyncRelayCommand NextPageCommand { get; }
    public IAsyncRelayCommand PreviousPageCommand { get; }
    public IAsyncRelayCommand ClearFiltersCommand { get; }
    public IAsyncRelayCommand PreviewSplitCommand { get; }
    public IAsyncRelayCommand ApplySplitCommand { get; }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage * PageSize < TotalCount;
    public bool IsSplitPreviewVisible => SplitPreviewFragments.Count > 0;
    public bool CanShowSplitSection => IsEditingExisting &&
                                       !string.IsNullOrWhiteSpace(AttributeValue) &&
                                       AttributeValue.Contains(SplitSeparator, StringComparison.Ordinal);
    public bool CanApplySplit => SplitPreviewFragments.Any(f => f.AlreadyExisted != true);
    public string PageSummary => TotalCount == 0
                                     ? _localizer["SoftwareAttributesPaginationEmpty"]
                                     : string.Format(_localizer["SoftwareAttributesPaginationSummary"],
                                         (CurrentPage - 1) * PageSize + 1,
                                         Math.Min(CurrentPage * PageSize, TotalCount),
                                         TotalCount);

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin) _ = InitializeAsync();
    }

    partial void OnCurrentPageChanged(int value)
    {
        NotifyPageStateChanged();
    }

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
    partial void OnSuccessMessageChanged(string value) => OnPropertyChanged(nameof(HasSuccess));

    partial void OnPageSizeChanged(int value)
    {
        NotifyPageStateChanged();
    }

    partial void OnTotalCountChanged(int value)
    {
        NotifyPageStateChanged();
    }

    partial void OnIsEditingExistingChanged(bool value) => OnPropertyChanged(nameof(CanShowSplitSection));

    partial void OnAttributeValueChanged(string value)
    {
        ClearSplitPreview();
        OnPropertyChanged(nameof(CanShowSplitSection));
    }

    partial void OnEditCategoryChanged(string value) => ClearSplitPreview();
    partial void OnEditKeyChanged(string value) => ClearSplitPreview();

    void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(string.IsNullOrWhiteSpace(token))
            {
                IsAdmin = false;

                return;
            }

            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("UberAdmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    async Task InitializeAsync()
    {
        ErrorMessage   = string.Empty;
        SuccessMessage = string.Empty;
        CurrentPage    = 1;
        _allSoftware   = await _softwareService.GetAllAsync();
        UpdateFilterSoftwareSuggestions(FilterSoftwareText);
        UpdateEditSoftwareSuggestions(EditSoftwareText);
        await LoadAsync();
    }

    async Task LoadAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;

            SoftwareAttributePageDto? page = await _attributesService.GetPagedAsync(
                SelectedFilterSoftware?.Id,
                SelectedFilterRelease?.Id,
                Normalize(FilterCategoryText),
                Normalize(FilterKeyText),
                CurrentPage,
                PageSize);

            Replace(Attributes, page?.Items ?? []);
            TotalCount   = page?.TotalCount ?? 0;
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software attributes");
            ErrorMessage = _localizer["SoftwareAttributesLoadFailed"];
            IsDataLoaded = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task OpenAddAsync()
    {
        ErrorMessage        = string.Empty;
        SuccessMessage      = string.Empty;
        _editingAttributeId = null;
        IsEditingExisting   = false;
        EditPanelTitle      = _localizer["AddSoftwareAttributeDialog_Title"];
        EditingContextText  = string.Empty;
        EditCategory        = FilterCategoryText;
        EditKey             = FilterKeyText;
        AttributeValue      = string.Empty;
        ClearSplitPreview();

        SelectedEditSoftware = SelectedFilterSoftware;
        EditSoftwareText     = SelectedEditSoftware?.Name ?? string.Empty;
        UpdateEditSoftwareSuggestions(EditSoftwareText);

        if(SelectedEditSoftware?.Id is int softwareId)
        {
            _editReleases = await _attributesService.LookupReleasesAsync(softwareId);
            Replace(EditReleaseSuggestions, _editReleases);
            SelectedEditRelease = SelectedFilterRelease != null
                                      ? _editReleases.FirstOrDefault(r => r.Id == SelectedFilterRelease.Id)
                                      : null;
        }
        else
        {
            _editReleases = [];
            EditReleaseSuggestions.Clear();
            SelectedEditRelease = null;
        }

        IsEditing = true;
    }

    async Task OpenEditAsync(SoftwareAttributeDto? item)
    {
        if(item?.Id == null) return;

        ErrorMessage        = string.Empty;
        SuccessMessage      = string.Empty;
        _editingAttributeId = item.Id;
        IsEditingExisting   = true;
        IsEditing           = true;
        EditPanelTitle      = _localizer["EditSoftwareAttributeDialog_Title"];
        EditingContextText  = FormatContext(item.SoftwareName, item.SoftwareReleaseTitle, item.PlatformName);
        EditCategory        = item.Category ?? string.Empty;
        EditKey             = item.Key ?? string.Empty;
        AttributeValue      = item.Value ?? string.Empty;
        SelectedEditSoftware = null;
        SelectedEditRelease  = null;
        EditSoftwareText     = string.Empty;
        EditReleaseSuggestions.Clear();
        ClearSplitPreview();

        await UpdateEditCategorySuggestionsAsync(EditCategory);
        await UpdateEditKeySuggestionsAsync(EditKey, EditCategory);
    }

    async Task DeleteAsync(SoftwareAttributeDto? item)
    {
        if(item?.Id == null) return;

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer, $"{item.Category}: {item.Key}"))
            return;

        (bool succeeded, string? error) = await _attributesService.DeleteAsync(item.Id.Value);

        if(!succeeded)
        {
            ErrorMessage = error ?? _localizer["SoftwareAttributesDeleteFailed"];

            return;
        }

        SuccessMessage = _localizer["SoftwareAttributesDeleteSucceeded"];

        if(Attributes.Count == 1 && CurrentPage > 1) CurrentPage--;

        await LoadAsync();
    }

    async Task SaveAsync()
    {
        ErrorMessage   = string.Empty;
        SuccessMessage = string.Empty;

        string? category = Normalize(EditCategory);
        string? key      = Normalize(EditKey);
        string? value    = Normalize(AttributeValue);

        if(string.IsNullOrWhiteSpace(category))
        {
            ErrorMessage = _localizer["SoftwareAttributesCategoryRequired"];

            return;
        }

        if(string.IsNullOrWhiteSpace(key))
        {
            ErrorMessage = _localizer["SoftwareAttributesKeyRequired"];

            return;
        }

        if(string.IsNullOrWhiteSpace(value))
        {
            ErrorMessage = _localizer["SoftwareAttributesValueRequired"];

            return;
        }

        if(!IsEditingExisting && SelectedEditRelease?.Id is null)
        {
            ErrorMessage = _localizer["SoftwareAttributesReleaseRequired"];

            return;
        }

        if(IsEditingExisting)
        {
            (bool succeeded, string? error) = await _attributesService.UpdateAsync(_editingAttributeId ?? 0,
                new UpdateSoftwareAttributeRequest
                {
                    Category = category,
                    Key      = key,
                    Value    = value
                });

            if(!succeeded)
            {
                ErrorMessage = error ?? _localizer["SoftwareAttributesSaveFailed"];

                return;
            }

            SuccessMessage = _localizer["SoftwareAttributesUpdateSucceeded"];
        }
        else
        {
            (long? id, string? error) = await _attributesService.CreateAsync(new CreateSoftwareAttributeRequest
            {
                SoftwareReleaseId = SelectedEditRelease?.Id ?? 0,
                Category          = category,
                Key               = key,
                Value             = value
            });

            if(id == null)
            {
                ErrorMessage = error ?? _localizer["SoftwareAttributesSaveFailed"];

                return;
            }

            SuccessMessage = _localizer["SoftwareAttributesCreateSucceeded"];
        }

        CancelEdit();
        CurrentPage = 1;
        await LoadAsync();
    }

    void CancelEdit()
    {
        _editingAttributeId  = null;
        IsEditing            = false;
        IsEditingExisting    = false;
        EditPanelTitle       = string.Empty;
        EditingContextText   = string.Empty;
        EditSoftwareText     = string.Empty;
        SelectedEditSoftware = null;
        SelectedEditRelease  = null;
        EditCategory         = string.Empty;
        EditKey              = string.Empty;
        AttributeValue       = string.Empty;
        EditReleaseSuggestions.Clear();
        ClearSplitPreview();
    }

    async Task NextPageAsync()
    {
        if(!CanGoNext) return;

        CurrentPage++;
        await LoadAsync();
    }

    async Task PreviousPageAsync()
    {
        if(!CanGoPrevious) return;

        CurrentPage--;
        await LoadAsync();
    }

    async Task ClearFiltersAsync()
    {
        FilterSoftwareText    = string.Empty;
        FilterReleaseText     = string.Empty;
        FilterCategoryText    = string.Empty;
        FilterKeyText         = string.Empty;
        SelectedFilterSoftware = null;
        SelectedFilterRelease  = null;
        _filterReleases       = [];
        FilterReleaseSuggestions.Clear();
        CurrentPage           = 1;
        await LoadAsync();
    }

    async Task PreviewSplitAsync()
    {
        if(!IsEditingExisting || _editingAttributeId == null) return;

        IsSplitBusy  = true;
        ErrorMessage = string.Empty;

        (SplitSoftwareAttributeResultDto? result, string? error) =
            await _attributesService.PreviewSplitAsync(_editingAttributeId.Value, SplitSeparator);

        IsSplitBusy = false;

        if(result == null)
        {
            ErrorMessage = error ?? _localizer["SoftwareAttributesSplitPreviewFailed"];

            return;
        }

        Replace(SplitPreviewFragments, result.Fragments ?? []);
        NotifySplitStateChanged();
    }

    async Task ApplySplitAsync()
    {
        if(!IsEditingExisting || _editingAttributeId == null) return;

        IsSplitBusy  = true;
        ErrorMessage = string.Empty;

        (SplitSoftwareAttributeResultDto? result, string? error) =
            await _attributesService.SplitAsync(_editingAttributeId.Value, SplitSeparator);

        IsSplitBusy = false;

        if(result == null)
        {
            ErrorMessage = error ?? _localizer["SoftwareAttributesSplitApplyFailed"];

            return;
        }

        int created = result.Fragments?.Count(f => f.AlreadyExisted != true) ?? 0;
        int skipped = result.Fragments?.Count(f => f.AlreadyExisted == true) ?? 0;

        SuccessMessage = string.Format(_localizer["SoftwareAttributesSplitSummary"], created, skipped);
        CancelEdit();
        await LoadAsync();
    }

    public void UpdateFilterSoftwareSuggestions(string? text)
    {
        IEnumerable<SoftwareDto> matches = string.IsNullOrWhiteSpace(text)
            ? _allSoftware.Take(25)
            : _allSoftware.Where(s => (s.Name ?? string.Empty).Contains(text, StringComparison.OrdinalIgnoreCase))
                          .Take(25);

        Replace(FilterSoftwareSuggestions, matches);
    }

    public async Task SelectFilterSoftwareAsync(SoftwareDto? software)
    {
        SelectedFilterSoftware = software;
        FilterSoftwareText     = software?.Name ?? string.Empty;
        SelectedFilterRelease  = null;
        FilterReleaseText      = string.Empty;
        _filterReleases        = [];
        FilterReleaseSuggestions.Clear();
        CurrentPage            = 1;

        if(software?.Id is int softwareId)
        {
            _filterReleases = await _attributesService.LookupReleasesAsync(softwareId);
            Replace(FilterReleaseSuggestions, _filterReleases);
        }

        await LoadAsync();
    }

    public void SyncFilterSoftwareSelection()
    {
        if(SelectedFilterSoftware == null) return;

        if(string.Equals(FilterSoftwareText, SelectedFilterSoftware.Name, StringComparison.OrdinalIgnoreCase)) return;

        SelectedFilterSoftware = null;
        SelectedFilterRelease  = null;
        FilterReleaseText      = string.Empty;
        _filterReleases        = [];
        FilterReleaseSuggestions.Clear();
    }

    public async Task ResolveFilterSoftwareQueryAsync(string? text)
    {
        SoftwareDto? match = _allSoftware.FirstOrDefault(s => string.Equals(s.Name, text, StringComparison.OrdinalIgnoreCase));

        await SelectFilterSoftwareAsync(match);
    }

    public void UpdateFilterReleaseSuggestions(string? text)
    {
        IEnumerable<SoftwareReleaseLookupDto> matches = string.IsNullOrWhiteSpace(text)
            ? _filterReleases.Take(25)
            : _filterReleases.Where(r => ReleaseToString(r).Contains(text, StringComparison.OrdinalIgnoreCase))
                             .Take(25);

        Replace(FilterReleaseSuggestions, matches);
    }

    public async Task SelectFilterReleaseAsync(SoftwareReleaseLookupDto? release)
    {
        SelectedFilterRelease = release;
        FilterReleaseText     = release is null ? string.Empty : ReleaseToString(release);
        CurrentPage           = 1;
        await LoadAsync();
    }

    public void SyncFilterReleaseSelection()
    {
        if(SelectedFilterRelease == null) return;

        if(string.Equals(FilterReleaseText, ReleaseToString(SelectedFilterRelease), StringComparison.OrdinalIgnoreCase)) return;

        SelectedFilterRelease = null;
    }

    public async Task ResolveFilterReleaseQueryAsync(string? text)
    {
        SoftwareReleaseLookupDto? match = _filterReleases.FirstOrDefault(r =>
            string.Equals(ReleaseToString(r), text, StringComparison.OrdinalIgnoreCase));

        await SelectFilterReleaseAsync(match);
    }

    public async Task UpdateFilterCategorySuggestionsAsync(string? text)
    {
        List<string> categories = await _attributesService.GetDistinctCategoriesAsync();

        IEnumerable<string> matches = string.IsNullOrWhiteSpace(text)
            ? categories.Take(25)
            : categories.Where(c => c.Contains(text, StringComparison.OrdinalIgnoreCase)).Take(25);

        Replace(FilterCategorySuggestions, matches);
    }

    public async Task CommitFilterCategoryAsync(string? category)
    {
        FilterCategoryText = category ?? string.Empty;
        CurrentPage        = 1;
        await LoadAsync();
    }

    public async Task UpdateFilterKeySuggestionsAsync(string? text)
    {
        List<string> keys = await _attributesService.GetDistinctKeysAsync(Normalize(FilterCategoryText));

        IEnumerable<string> matches = string.IsNullOrWhiteSpace(text)
            ? keys.Take(25)
            : keys.Where(k => k.Contains(text, StringComparison.OrdinalIgnoreCase)).Take(25);

        Replace(FilterKeySuggestions, matches);
    }

    public async Task CommitFilterKeyAsync(string? key)
    {
        FilterKeyText = key ?? string.Empty;
        CurrentPage   = 1;
        await LoadAsync();
    }

    public void UpdateEditSoftwareSuggestions(string? text)
    {
        IEnumerable<SoftwareDto> matches = string.IsNullOrWhiteSpace(text)
            ? _allSoftware.Take(25)
            : _allSoftware.Where(s => (s.Name ?? string.Empty).Contains(text, StringComparison.OrdinalIgnoreCase))
                          .Take(25);

        Replace(EditSoftwareSuggestions, matches);
    }

    public async Task SelectEditSoftwareAsync(SoftwareDto? software)
    {
        SelectedEditSoftware = software;
        EditSoftwareText     = software?.Name ?? string.Empty;
        SelectedEditRelease  = null;
        _editReleases        = [];
        EditReleaseSuggestions.Clear();

        if(software?.Id is int softwareId)
        {
            _editReleases = await _attributesService.LookupReleasesAsync(softwareId);
            Replace(EditReleaseSuggestions, _editReleases);
        }
    }

    public void SyncEditSoftwareSelection()
    {
        if(SelectedEditSoftware == null) return;

        if(string.Equals(EditSoftwareText, SelectedEditSoftware.Name, StringComparison.OrdinalIgnoreCase)) return;

        SelectedEditSoftware = null;
        SelectedEditRelease  = null;
        _editReleases        = [];
        EditReleaseSuggestions.Clear();
    }

    public async Task ResolveEditSoftwareQueryAsync(string? text)
    {
        SoftwareDto? match = _allSoftware.FirstOrDefault(s => string.Equals(s.Name, text, StringComparison.OrdinalIgnoreCase));

        await SelectEditSoftwareAsync(match);
    }

    public void UpdateEditReleaseSuggestions(string? text)
    {
        IEnumerable<SoftwareReleaseLookupDto> matches = string.IsNullOrWhiteSpace(text)
            ? _editReleases.Take(25)
            : _editReleases.Where(r => ReleaseToString(r).Contains(text, StringComparison.OrdinalIgnoreCase))
                           .Take(25);

        Replace(EditReleaseSuggestions, matches);
    }

    public void SelectEditRelease(SoftwareReleaseLookupDto? release) => SelectedEditRelease = release;

    public async Task UpdateEditCategorySuggestionsAsync(string? text)
    {
        List<string> categories = await _attributesService.GetDistinctCategoriesAsync();

        IEnumerable<string> matches = string.IsNullOrWhiteSpace(text)
            ? categories.Take(25)
            : categories.Where(c => c.Contains(text, StringComparison.OrdinalIgnoreCase)).Take(25);

        Replace(EditCategorySuggestions, matches);
    }

    public async Task UpdateEditKeySuggestionsAsync(string? text, string? category = null)
    {
        List<string> keys = await _attributesService.GetDistinctKeysAsync(Normalize(category ?? EditCategory));

        IEnumerable<string> matches = string.IsNullOrWhiteSpace(text)
            ? keys.Take(25)
            : keys.Where(k => k.Contains(text, StringComparison.OrdinalIgnoreCase)).Take(25);

        Replace(EditKeySuggestions, matches);
    }

    static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    static string ReleaseToString(SoftwareReleaseLookupDto release)
    {
        string title = string.IsNullOrWhiteSpace(release.Title) ? "—" : release.Title;

        return string.IsNullOrWhiteSpace(release.PlatformName) ? title : $"{title} — {release.PlatformName}";
    }

    static string FormatContext(string? softwareName, string? releaseTitle, string? platformName)
    {
        string software = string.IsNullOrWhiteSpace(softwareName) ? "—" : softwareName;
        string release  = string.IsNullOrWhiteSpace(releaseTitle) ? "—" : releaseTitle;

        return string.IsNullOrWhiteSpace(platformName)
                   ? $"{software} · {release}"
                   : $"{software} · {release} · {platformName}";
    }

    void NotifyPageStateChanged()
    {
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(PageSummary));
    }

    void ClearSplitPreview()
    {
        SplitPreviewFragments.Clear();
        NotifySplitStateChanged();
    }

    void NotifySplitStateChanged()
    {
        OnPropertyChanged(nameof(IsSplitPreviewVisible));
        OnPropertyChanged(nameof(CanApplySplit));
    }

    static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();

        foreach(T item in items) target.Add(item);
    }
}
