#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.Data;
using Microsoft.UI.Dispatching;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareService                    _service;
    private readonly Client                              _apiClient;
    private readonly SoftwareFamiliesService            _familiesService;
    private readonly ExternalSitesService               _externalSitesService;
    private readonly IJwtService                        _jwtService;
    private readonly IStringLocalizer                   _localizer;
    private readonly ILogger<AdminSoftwareViewModel>    _logger;
    private readonly ITokenService                      _tokenService;
    private readonly IRegionManager                     _regionManager;
    private readonly DispatcherQueue?                   _dispatcherQueue;

    [ObservableProperty] private ObservableCollection<SoftwareDto> _softwareItems = [];
    [ObservableProperty] private ObservableCollection<SoftwareDto> _filteredSoftware = [];
    [ObservableProperty] private string                            _filterText = string.Empty;
    [ObservableProperty] private SoftwareDto?                      _selectedSoftware;
    [ObservableProperty] private bool                               _isLoading;
    [ObservableProperty] private bool                               _isDataLoaded;
    [ObservableProperty] private bool                               _hasError;
    [ObservableProperty] private string                            _errorMessage = string.Empty;
    [ObservableProperty] private bool                               _isAdmin;
    [ObservableProperty] private ObservableCollection<int>         _pageSizeOptions = [10, 25, 50, 100];
    [ObservableProperty] private int                               _currentPage = 1;
    [ObservableProperty] private int                               _pageSize = 25;
    [ObservableProperty] private int                               _totalCount;
    [ObservableProperty] private bool                               _isEditing;
    [ObservableProperty] private bool                               _isEditingExisting;
    [ObservableProperty] private string                            _editPanelTitle = string.Empty;

    // Form
    [ObservableProperty] private string           _softwareName = string.Empty;
    [ObservableProperty] private int               _kind;
    [ObservableProperty] private int?              _baseSoftwareId;
    [ObservableProperty] private SoftwareFamilyDto? _selectedFamily;
    [ObservableProperty] private string           _familySearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareFamilyDto> _familySuggestions = [];

    // Company roles junction
    [ObservableProperty] private ObservableCollection<SoftwareCompanyRoleDto> _companyRoles = [];
    [ObservableProperty] private ObservableCollection<string>                 _companyRoleDisplays = [];
    [ObservableProperty] private CompanyDto?                                  _selectedCompanyToAdd;
    [ObservableProperty] private string                                      _companySearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyDto>             _companySuggestions = [];
    [ObservableProperty] private SoftwareRoleDto?                            _selectedRoleToAdd;
    [ObservableProperty] private ObservableCollection<SoftwareRoleDto>        _roles = [];

    // External ids
    [ObservableProperty] private ObservableCollection<SoftwareExternalIdDto> _externalIds = [];
    [ObservableProperty] private ObservableCollection<ExternalSiteDto>       _externalSites = [];
    [ObservableProperty] private ExternalSiteDto?                           _selectedExternalSiteToAdd;
    [ObservableProperty] private string                                     _externalIdValueToAdd = string.Empty;

    // Similar software
    [ObservableProperty] private ObservableCollection<SoftwareSimilarToDto> _similarSoftware = [];
    [ObservableProperty] private SoftwareDto?                              _selectedSimilarSoftwareToAdd;
    [ObservableProperty] private string                                    _similarSoftwareSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareDto>          _similarSoftwareSuggestions = [];

    private int?                     _editingId;
    private List<SoftwareFamilyDto>? _allFamilies;
    private List<CompanyDto>?        _allCompanies;
    private List<ExternalSiteDto>?   _allExternalSites;
    private readonly object          _loadSync = new();
    private Task?                    _activeLoadTask;
    private Task?                    _activePickerLoadTask;
    private bool                     _hasNavigatedLoadStarted;
    private bool                     _pickerDataLoaded;

    // Description editing
    [ObservableProperty] private bool                                             _isEditingDescription;
    [ObservableProperty] private string                                          _descriptionMarkdown = string.Empty;
    [ObservableProperty] private int?                                            _descriptionSoftwareId;
    [ObservableProperty] private ObservableCollection<LanguageItem>              _availableLanguages = [];
    [ObservableProperty] private LanguageItem?                                   _selectedLanguage;
    [ObservableProperty] private ObservableCollection<SoftwareDescriptionDto>    _existingTranslations = [];

    public AdminSoftwareViewModel(SoftwareService                   service,
                                  Client                             apiClient,
                                  SoftwareFamiliesService           familiesService,
                                  ExternalSitesService              externalSitesService,
                                  IJwtService                       jwtService,
                                  ITokenService                     tokenService,
                                  ILogger<AdminSoftwareViewModel>   logger,
                                  IStringLocalizer                  localizer,
                                  IRegionManager                    regionManager)
    {
        _service              = service;
        _apiClient             = apiClient;
        _familiesService       = familiesService;
        _externalSitesService  = externalSitesService;
        _jwtService            = jwtService;
        _tokenService          = tokenService;
        _logger                = logger;
        _localizer             = localizer;
        _regionManager         = regionManager;
        _dispatcherQueue       = DispatcherQueue.GetForCurrentThread();

        LoadCommand         = new AsyncRelayCommand(() => EnsureLoadAsync(force: true));
        OpenAddCommand      = new RelayCommand(OpenAdd);
        OpenEditCommand     = new RelayCommand<SoftwareDto>(OpenEdit);
        DeleteCommand       = new AsyncRelayCommand<SoftwareDto>(DeleteAsync);
        SaveCommand         = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand   = new RelayCommand(CancelEdit);
        AddCompanyRoleCommand       = new AsyncRelayCommand(AddCompanyRoleAsync);
        RemoveCompanyRoleByDisplayCommand = new AsyncRelayCommand<string>(RemoveCompanyRoleByDisplayAsync);
        AddExternalIdCommand        = new AsyncRelayCommand(AddExternalIdAsync);
        RemoveExternalIdCommand     = new AsyncRelayCommand<SoftwareExternalIdDto>(RemoveExternalIdAsync);
        AddSimilarSoftwareCommand   = new AsyncRelayCommand(AddSimilarSoftwareAsync);
        RemoveSimilarSoftwareCommand = new AsyncRelayCommand<SoftwareSimilarToDto>(RemoveSimilarSoftwareAsync);
        OpenVersionsCommand = new RelayCommand<SoftwareDto>(OpenVersions);
        OpenReleasesCommand = new RelayCommand<SoftwareDto>(OpenReleases);
        OpenPromoArtCommand = new RelayCommand<SoftwareDto>(OpenPromoArt);
        OpenVideosCommand   = new RelayCommand<SoftwareDto>(OpenVideos);
        OpenDescriptionCommand     = new RelayCommand<SoftwareDto>(OpenDescription);
        SaveDescriptionCommand     = new AsyncRelayCommand(SaveDescriptionAsync);
        CancelDescriptionCommand   = new RelayCommand(CancelDescription);
        DeleteTranslationCommand   = new AsyncRelayCommand<SoftwareDescriptionDto>(DeleteTranslationAsync);
        EditTranslationCommand     = new RelayCommand<SoftwareDescriptionDto>(EditTranslation);
        NextPageCommand            = new AsyncRelayCommand(NextPageAsync);
        PreviousPageCommand        = new AsyncRelayCommand(PreviousPageAsync);

        CheckAdminRole();
        InitializeLanguages();
    }

    public IAsyncRelayCommand                LoadCommand       { get; }
    public IRelayCommand                     OpenAddCommand    { get; }
    public IRelayCommand<SoftwareDto>        OpenEditCommand   { get; }
    public IAsyncRelayCommand<SoftwareDto>   DeleteCommand     { get; }
    public IAsyncRelayCommand                SaveCommand       { get; }
    public IRelayCommand                     CancelEditCommand { get; }
    public IAsyncRelayCommand                AddCompanyRoleCommand { get; }
    public IAsyncRelayCommand<string>        RemoveCompanyRoleByDisplayCommand { get; }
    public IAsyncRelayCommand                              AddExternalIdCommand    { get; }
    public IAsyncRelayCommand<SoftwareExternalIdDto>       RemoveExternalIdCommand { get; }
    public IAsyncRelayCommand                              AddSimilarSoftwareCommand    { get; }
    public IAsyncRelayCommand<SoftwareSimilarToDto>        RemoveSimilarSoftwareCommand { get; }
    public IRelayCommand<SoftwareDto>        OpenVersionsCommand { get; }
    public IRelayCommand<SoftwareDto>        OpenReleasesCommand { get; }
    public IRelayCommand<SoftwareDto>        OpenPromoArtCommand { get; }
    public IRelayCommand<SoftwareDto>        OpenVideosCommand { get; }
    public IRelayCommand<SoftwareDto>                   OpenDescriptionCommand   { get; }
    public IAsyncRelayCommand                           SaveDescriptionCommand   { get; }
    public IRelayCommand                                CancelDescriptionCommand { get; }
    public IAsyncRelayCommand<SoftwareDescriptionDto>   DeleteTranslationCommand { get; }
    public IRelayCommand<SoftwareDescriptionDto>        EditTranslationCommand   { get; }
    public IAsyncRelayCommand                           NextPageCommand          { get; }
    public IAsyncRelayCommand                           PreviousPageCommand      { get; }
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage * PageSize < TotalCount;
    public string PageSummary => TotalCount == 0
                                     ? _localizer["SoftwareAttributesPaginationEmpty"]
                                     : string.Format(_localizer["MessageReportsPaginationFormat"],
                                                     (CurrentPage - 1) * PageSize + 1,
                                                     Math.Min(CurrentPage * PageSize, TotalCount),
                                                     TotalCount);

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();
        if(!IsAdmin) return;

        if(!_hasNavigatedLoadStarted || !IsDataLoaded)
        {
            _hasNavigatedLoadStarted = true;
            _ = EnsureLoadAsync();
        }

        _ = EnsurePickerDataLoadedAsync();
    }

    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();
            if(string.IsNullOrWhiteSpace(token)) { IsAdmin = false; return; }
            IEnumerable<string> roles = _jwtService.GetRoles(token);
            IsAdmin = roles.Contains("UberAdmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch { IsAdmin = false; }
    }

    private Task EnsureLoadAsync(bool force = false)
    {
        lock(_loadSync)
        {
            if(!force && _activeLoadTask is { IsCompleted: false })
                return _activeLoadTask;

            _activeLoadTask = LoadAsyncCore();

            return _activeLoadTask;
        }
    }

    private async Task LoadAsyncCore()
    {
        try
        {
            IsLoading = true; HasError = false;
            SoftwareItems.Clear();
            string? search = NullIfWhiteSpace(FilterText);
            TotalCount = await _service.GetCountAsync(search);

            int maxPage = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

            if(CurrentPage > maxPage)
                CurrentPage = maxPage;

            int skip = (CurrentPage - 1) * PageSize;
            List<SoftwareDto> response = await _service.GetPagedAsync(skip, PageSize, search);
            foreach(SoftwareDto item in response) SoftwareItems.Add(item);
            ReplaceFilteredSoftware(SoftwareItems);
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software");
            ErrorMessage = _localizer["FailedToLoadSoftware"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    private Task EnsurePickerDataLoadedAsync(bool force = false)
    {
        lock(_loadSync)
        {
            if(!force)
            {
                if(_pickerDataLoaded)
                    return Task.CompletedTask;

                if(_activePickerLoadTask is { IsCompleted: false })
                    return _activePickerLoadTask;
            }

            _activePickerLoadTask = LoadPickerDataAsyncCore();

            return _activePickerLoadTask;
        }
    }

    public async Task LoadPickerDataAsync()
    {
        await EnsurePickerDataLoadedAsync(force: true);
    }

    private async Task LoadPickerDataAsyncCore()
    {
        try
        {
            _allFamilies = await _familiesService.GetAllAsync();
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading families for picker"); }

        try
        {
            List<SoftwareRoleDto> rolesResponse = await _service.GetRolesAsync();
            Roles.Clear();
            foreach(SoftwareRoleDto r in rolesResponse) Roles.Add(r);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading software roles"); }

        try
        {
            _allExternalSites = await _externalSitesService.GetAllAsync();
            ExternalSites.Clear();
            foreach(ExternalSiteDto s in _allExternalSites) ExternalSites.Add(s);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading external sites for picker"); }

        _pickerDataLoaded = true;
    }

    private void OpenAdd()
    {
        _editingId = null;
        EditPanelTitle = _localizer["AddSoftwareDialog_Title"];
        IsEditingExisting = false;
        ClearForm();
        IsEditing = true;
    }

    private async void OpenEdit(SoftwareDto? item)
    {
        if(item?.Id == null) return;

        _editingId        = item.Id;
        EditPanelTitle    = _localizer["EditSoftwareDialog_Title"];
        IsEditingExisting = true;
        SoftwareName      = item.Name ?? string.Empty;
        Kind              = item.Kind ?? 0;
        BaseSoftwareId    = item.BaseSoftwareId;

        if(item.FamilyId.HasValue && _allFamilies != null)
        {
            SoftwareFamilyDto? family = _allFamilies.FirstOrDefault(f => f.Id == item.FamilyId.Value);
            if(family != null)
            {
                FamilySearchText = family.Name ?? string.Empty;
                UpdateFamilySuggestions(FamilySearchText);
                SelectedFamily = FamilySuggestions.FirstOrDefault(f => f.Id == family.Id);
            }
        }
        else
        {
            FamilySearchText = string.Empty;
            SelectedFamily   = null;
        }

        HasError = false; ErrorMessage = string.Empty;
        IsEditing = true;

        await LoadCompanyRolesAsync(item.Id.Value);
        await LoadExternalIdsAsync(item.Id.Value);
        await LoadSimilarSoftwareAsync(item.Id.Value);
    }

    private async Task DeleteAsync(SoftwareDto? item)
    {
        if(item?.Id == null) return;
        try
        {
            await _service.DeleteAsync(item.Id.Value);
            await EnsureLoadAsync(force: true);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteSoftware"];
            HasError = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(SoftwareName))
            {
                ErrorMessage = _localizer["NameIsRequired"]; HasError = true; return;
            }

            var dto = new SoftwareDto
            {
                Name              = SoftwareName,
                FamilyId          = SelectedFamily?.Id,
                BaseSoftwareId    = BaseSoftwareId,
                Kind              = Kind
            };

            if(_editingId == null)
            {
                int? newId = await _service.CreateAsync(dto);
                if(newId.HasValue) _editingId = newId.Value;
            }
            else
            {
                dto.Id = _editingId;
                await _service.UpdateAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await EnsureLoadAsync(force: true);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving software");
            ErrorMessage = _localizer["FailedToSaveSoftware"];
            HasError = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing = false; _editingId = null;
        ClearForm();
        HasError = false; ErrorMessage = string.Empty;
    }

    // --- Drill-down navigation ---
    private void OpenVersions(SoftwareDto? item)
    {
        if(item?.Id == null) return;
        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, item.Id.Value },
            { NavParamKeys.SoftwareName, item.Name ?? string.Empty }
        };
        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoftwareVersionsPage), parameters);
    }

    private void OpenReleases(SoftwareDto? item)
    {
        if(item?.Id == null) return;
        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, item.Id.Value },
            { NavParamKeys.SoftwareName, item.Name ?? string.Empty }
        };
        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoftwareReleasesPage), parameters);
    }

    private void OpenPromoArt(SoftwareDto? item)
    {
        if(item?.Id == null) return;
        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, item.Id.Value },
            { NavParamKeys.SoftwareName, item.Name ?? string.Empty }
        };
        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoftwarePromoArtPage), parameters);
    }

    private void OpenVideos(SoftwareDto? item)
    {
        if(item?.Id == null) return;
        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, item.Id.Value },
            { NavParamKeys.SoftwareName, item.Name ?? string.Empty }
        };
        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoftwareVideosPage), parameters);
    }

    public void ApplyFilter()
    {
        _ = ReloadFromFirstPageAsync();
    }

    public void UpdateFamilySuggestions(string query)
    {
        FamilySuggestions.Clear();
        if(_allFamilies == null) return;
        IEnumerable<SoftwareFamilyDto> source = _allFamilies;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(f => f.Name != null && f.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(SoftwareFamilyDto match in source) FamilySuggestions.Add(match);
    }

    public void UpdateCompanySuggestions(string query)
    {
        CompanySuggestions.Clear();
        if(_allCompanies == null) return;
        IEnumerable<CompanyDto> source = _allCompanies;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(c => c.Name != null && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(CompanyDto match in source) CompanySuggestions.Add(match);
    }

    // --- Company roles ---
    private async Task LoadCompanyRolesAsync(int softwareId)
    {
        CompanyRoles.Clear(); CompanyRoleDisplays.Clear();
        try
        {
            List<SoftwareCompanyRoleDto> items = await _service.GetCompanyRolesAsync(softwareId);
            foreach(SoftwareCompanyRoleDto item in items)
            {
                CompanyRoles.Add(item);
                CompanyRoleDisplays.Add($"{item.Company} ({item.Role})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading company roles for software {Id}", softwareId); }
    }

    private async Task AddCompanyRoleAsync()
    {
        if(_editingId == null || SelectedCompanyToAdd == null || SelectedRoleToAdd == null) return;
        try
        {
            var dto = new SoftwareCompanyRoleDto
            {
                SoftwareId = _editingId.Value,
                CompanyId  = SelectedCompanyToAdd.Id.GetValueOrDefault(),
                RoleId     = SelectedRoleToAdd.Id ?? string.Empty
            };
            await _service.AddCompanyRoleAsync(dto);
            await LoadCompanyRolesAsync(_editingId.Value);
            SelectedCompanyToAdd = null;
            CompanySearchText = string.Empty;
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding company role to software"); }
    }

    private async Task RemoveCompanyRoleByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;
        int index = CompanyRoleDisplays.IndexOf(display);
        if(index >= 0 && index < CompanyRoles.Count)
        {
            SoftwareCompanyRoleDto item = CompanyRoles[index];
            await _service.RemoveCompanyRoleAsync(
                item.SoftwareId?.ToString() ?? string.Empty,
                item.CompanyId?.ToString() ?? string.Empty,
                item.RoleId ?? string.Empty);
            await LoadCompanyRolesAsync(_editingId.Value);
        }
    }

    // --- External ids ---
    private async Task LoadExternalIdsAsync(int softwareId)
    {
        ExternalIds.Clear();
        try
        {
            List<SoftwareExternalIdDto> items = await _service.GetExternalIdsAsync(softwareId);
            foreach(SoftwareExternalIdDto item in items) ExternalIds.Add(item);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading external ids for software {Id}", softwareId); }
    }

    private async Task AddExternalIdAsync()
    {
        if(_editingId == null || SelectedExternalSiteToAdd?.Id == null ||
           string.IsNullOrWhiteSpace(ExternalIdValueToAdd)) return;

        try
        {
            var dto = new SoftwareExternalIdDto
            {
                SoftwareId     = _editingId.Value,
                ExternalSiteId = SelectedExternalSiteToAdd.Id.Value,
                ExternalId     = ExternalIdValueToAdd.Trim()
            };
            await _service.AddExternalIdAsync(dto);
            await LoadExternalIdsAsync(_editingId.Value);
            SelectedExternalSiteToAdd = null;
            ExternalIdValueToAdd      = string.Empty;
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding external id to software"); }
    }

    private async Task RemoveExternalIdAsync(SoftwareExternalIdDto? item)
    {
        if(item?.Id == null || _editingId == null) return;

        await _service.RemoveExternalIdAsync(item.Id.Value);
        await LoadExternalIdsAsync(_editingId.Value);
    }

    // --- Similar software ---
    private async Task LoadSimilarSoftwareAsync(int softwareId)
    {
        SimilarSoftware.Clear();
        try
        {
            List<SoftwareSimilarToDto> items = await _service.GetSimilarSoftwareAsync(softwareId);
            foreach(SoftwareSimilarToDto item in items) SimilarSoftware.Add(item);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading similar software for software {Id}", softwareId); }
    }

    public async Task UpdateSimilarSoftwareSuggestionsAsync(string query)
    {
        SimilarSoftwareSuggestions.Clear();

        List<SoftwareDto> source = await _service.SearchForPickerAsync(query);

        IEnumerable<SoftwareDto> matches = source.Where(
            s => s.Id != _editingId &&
                 SimilarSoftware.All(r => r.SimilarSoftwareId != s.Id));

        foreach(SoftwareDto match in matches.Take(50))
            SimilarSoftwareSuggestions.Add(match);
    }

    private async Task AddSimilarSoftwareAsync()
    {
        if(_editingId == null || SelectedSimilarSoftwareToAdd?.Id == null) return;

        try
        {
            var dto = new SoftwareSimilarToDto
            {
                SoftwareId        = _editingId.Value,
                SimilarSoftwareId = SelectedSimilarSoftwareToAdd.Id.Value
            };
            await _service.AddSimilarSoftwareAsync(dto);
            await LoadSimilarSoftwareAsync(_editingId.Value);
            SelectedSimilarSoftwareToAdd = null;
            SimilarSoftwareSearchText    = string.Empty;
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding similar software link"); }
    }

    private async Task RemoveSimilarSoftwareAsync(SoftwareSimilarToDto? item)
    {
        if(item?.SimilarSoftwareId == null || _editingId == null) return;

        await _service.RemoveSimilarSoftwareAsync(_editingId.Value, item.SimilarSoftwareId.Value);
        await LoadSimilarSoftwareAsync(_editingId.Value);
    }

    private void ClearForm()
    {
        SoftwareName      = string.Empty;
        Kind              = 0;
        BaseSoftwareId    = null;
        SelectedFamily    = null;
        FamilySearchText  = string.Empty;
        CompanyRoles.Clear();
        CompanyRoleDisplays.Clear();
        SelectedCompanyToAdd = null;
        CompanySearchText = string.Empty;
        SelectedRoleToAdd = null;
        ExternalIds.Clear();
        SelectedExternalSiteToAdd = null;
        ExternalIdValueToAdd      = string.Empty;
        SimilarSoftware.Clear();
        SelectedSimilarSoftwareToAdd = null;
        SimilarSoftwareSearchText    = string.Empty;
        SimilarSoftwareSuggestions.Clear();
        HasError = false; ErrorMessage = string.Empty;
    }

    // --- Description management ---

    private void InitializeLanguages()
    {
        AvailableLanguages =
        [
            new LanguageItem { Code = "eng", DisplayName = "English" },
            new LanguageItem { Code = "spa", DisplayName = "Español" },
            new LanguageItem { Code = "deu", DisplayName = "Deutsch" },
            new LanguageItem { Code = "fra", DisplayName = "Français" },
            new LanguageItem { Code = "lat", DisplayName = "Latina" },
            new LanguageItem { Code = "por", DisplayName = "Português (Brasil)" }
        ];

        SelectedLanguage = AvailableLanguages[0];
    }

    private void OpenDescription(SoftwareDto? software) => _ = OpenDescriptionAsync(software);

    private async Task OpenDescriptionAsync(SoftwareDto? software)
    {
        if(software?.Id == null) return;

        try
        {
            List<SoftwareDescriptionDto>? translations =
                await _apiClient.Software[software.Id.Value].Descriptions.GetAsync();

            await RunOnUiThreadAsync(() =>
            {
                DescriptionSoftwareId = software.Id;
                DescriptionMarkdown   = string.Empty;
                IsEditing             = false;
                ExistingTranslations.Clear();

                if(translations != null)
                    foreach(SoftwareDescriptionDto t in translations)
                        ExistingTranslations.Add(t);

                SelectedLanguage = AvailableLanguages.FirstOrDefault(l =>
                                       ExistingTranslations.All(t => t.LanguageCode != l.Code)) ??
                                   AvailableLanguages[0];

                SoftwareDescriptionDto? existing =
                    ExistingTranslations.FirstOrDefault(t => t.LanguageCode == SelectedLanguage.Code);

                if(existing != null)
                    DescriptionMarkdown = existing.Markdown ?? string.Empty;

                IsEditingDescription = true;
            });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading description for software {Id}", software.Id);
            await RunOnUiThreadAsync(() =>
            {
                DescriptionMarkdown  = string.Empty;
                IsEditingDescription = true;
            });
        }
    }

    private Task RunOnUiThreadAsync(Action action)
    {
        if(_dispatcherQueue is null || _dispatcherQueue.HasThreadAccess)
        {
            action();

            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();

        if(!_dispatcherQueue.TryEnqueue(() =>
           {
               try
               {
                   action();
                   tcs.SetResult();
               }
               catch(Exception ex)
               {
                   tcs.SetException(ex);
               }
           }))
            tcs.SetException(new InvalidOperationException("Unable to enqueue work on the UI thread."));

        return tcs.Task;
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        if(value == null || ExistingTranslations.Count == 0)
        {
            DescriptionMarkdown = string.Empty;

            return;
        }

        SoftwareDescriptionDto? existing = ExistingTranslations.FirstOrDefault(t => t.LanguageCode == value.Code);
        DescriptionMarkdown = existing?.Markdown ?? string.Empty;
    }

    private async Task SaveDescriptionAsync()
    {
        if(DescriptionSoftwareId == null || SelectedLanguage == null) return;

        try
        {
            var dto = new SoftwareDescriptionDto
            {
                SoftwareId   = DescriptionSoftwareId.Value,
                Markdown     = DescriptionMarkdown,
                LanguageCode = SelectedLanguage.Code
            };

            await _apiClient.Software[DescriptionSoftwareId.Value].Description.PostAsync(dto);

            ExistingTranslations.Clear();

            List<SoftwareDescriptionDto>? translations =
                await _apiClient.Software[DescriptionSoftwareId.Value].Descriptions.GetAsync();

            if(translations != null)
                foreach(SoftwareDescriptionDto t in translations)
                    ExistingTranslations.Add(t);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving description");
            ErrorMessage = _localizer["FailedToSaveDescription"];
            HasError     = true;
        }
    }

    private void EditTranslation(SoftwareDescriptionDto? translation)
    {
        if(translation?.LanguageCode == null) return;

        SelectedLanguage    = AvailableLanguages.FirstOrDefault(l => l.Code == translation.LanguageCode);
        DescriptionMarkdown = translation.Markdown ?? string.Empty;
    }

    private async Task DeleteTranslationAsync(SoftwareDescriptionDto? translation)
    {
        if(DescriptionSoftwareId == null || translation?.LanguageCode == null) return;

        try
        {
            await _apiClient.Software[DescriptionSoftwareId.Value].Description[translation.LanguageCode].DeleteAsync();

            ExistingTranslations.Remove(translation);
            DescriptionMarkdown = string.Empty;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting translation");
            ErrorMessage = _localizer["FailedToDeleteTranslation"];
            HasError     = true;
        }
    }

    private void CancelDescription()
    {
        IsEditingDescription  = false;
        DescriptionSoftwareId = null;
        DescriptionMarkdown   = string.Empty;
        ExistingTranslations.Clear();
    }

    partial void OnCurrentPageChanged(int value) => NotifyPaginationStateChanged();
    partial void OnPageSizeChanged(int value) => NotifyPaginationStateChanged();
    partial void OnTotalCountChanged(int value) => NotifyPaginationStateChanged();

    private Task NextPageAsync()
    {
        if(!CanGoNext) return Task.CompletedTask;

        CurrentPage++;

        return EnsureLoadAsync(force: true);
    }

    private Task PreviousPageAsync()
    {
        if(!CanGoPrevious) return Task.CompletedTask;

        CurrentPage--;

        return EnsureLoadAsync(force: true);
    }

    private async Task ReloadFromFirstPageAsync()
    {
        CurrentPage = 1;
        await EnsureLoadAsync(force: true);
    }

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private void ReplaceFilteredSoftware(IEnumerable<SoftwareDto> items)
    {
        FilteredSoftware.Clear();

        foreach(SoftwareDto item in items)
            FilteredSoftware.Add(item);
    }

    private void NotifyPaginationStateChanged()
    {
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(PageSummary));
    }
}
