#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Models;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareReleasesViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareReleasesService                     _service;
    private readonly SoftwareVersionsService                     _versionsService;
    private readonly SoftwarePlatformsService                    _platformsService;
    private readonly ApiClient                                   _apiClient;
    private readonly IJwtService                                 _jwtService;
    private readonly IStringLocalizer                            _localizer;
    private readonly ILogger<AdminSoftwareReleasesViewModel>     _logger;
    private readonly ITokenService                               _tokenService;

    [ObservableProperty] private ObservableCollection<SoftwareReleaseDto> _releases = [];
    [ObservableProperty] private ObservableCollection<SoftwareReleaseDto> _filteredReleases = [];
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private SoftwareReleaseDto? _selectedRelease;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isEditingExisting;
    [ObservableProperty] private string _editPanelTitle = string.Empty;

    // Form - pickers
    [ObservableProperty] private SoftwareVersionDto? _selectedVersion;
    [ObservableProperty] private string _versionSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareVersionDto> _versionSuggestions = [];
    [ObservableProperty] private SoftwarePlatformDto? _selectedPlatform;
    [ObservableProperty] private ObservableCollection<SoftwarePlatformDto> _platforms = [];
    [ObservableProperty] private Iso31661NumericDto? _selectedRegion;
    [ObservableProperty] private string _regionSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<Iso31661NumericDto> _regionSuggestions = [];
    [ObservableProperty] private CompanyDto? _selectedPublisher;
    [ObservableProperty] private string _publisherSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyDto> _publisherSuggestions = [];
    [ObservableProperty] private DateTimeOffset? _releaseDate;

    // Barcode/ProductCode inline
    [ObservableProperty] private ObservableCollection<SoftwareBarcodeDto> _barcodes = [];
    [ObservableProperty] private ObservableCollection<SoftwareProductCodeDto> _productCodes = [];

    private int? _editingId;
    private List<SoftwareReleaseDto>? _allReleases;
    private List<SoftwareVersionDto>? _allVersions;
    private List<SoftwarePlatformDto>? _allPlatforms;
    private List<Iso31661NumericDto>? _allRegions;
    private List<CompanyDto>? _allCompanies;

    public AdminSoftwareReleasesViewModel(SoftwareReleasesService                  service,
                                          SoftwareVersionsService                  versionsService,
                                          SoftwarePlatformsService                 platformsService,
                                          ApiClient                                apiClient,
                                          IJwtService                              jwtService,
                                          ITokenService                            tokenService,
                                          ILogger<AdminSoftwareReleasesViewModel>  logger,
                                          IStringLocalizer                         localizer)
    {
        _service          = service;
        _versionsService  = versionsService;
        _platformsService = platformsService;
        _apiClient        = apiClient;
        _jwtService       = jwtService;
        _tokenService     = tokenService;
        _logger           = logger;
        _localizer        = localizer;

        LoadCommand       = new AsyncRelayCommand(LoadAsync);
        OpenAddCommand    = new RelayCommand(OpenAdd);
        OpenEditCommand   = new RelayCommand<SoftwareReleaseDto>(OpenEdit);
        DeleteCommand     = new AsyncRelayCommand<SoftwareReleaseDto>(DeleteAsync);
        SaveCommand       = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                        LoadCommand       { get; }
    public IRelayCommand                             OpenAddCommand    { get; }
    public IRelayCommand<SoftwareReleaseDto>         OpenEditCommand   { get; }
    public IAsyncRelayCommand<SoftwareReleaseDto>    DeleteCommand     { get; }
    public IAsyncRelayCommand                        SaveCommand       { get; }
    public IRelayCommand                             CancelEditCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();
        if(IsAdmin)
        {
            await LoadPickerDataAsync();
            _ = LoadCommand.ExecuteAsync(null);
        }
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

    private async Task LoadAsync()
    {
        try
        {
            IsLoading = true; HasError = false;
            Releases.Clear();
            List<SoftwareReleaseDto> response = await _service.GetAllAsync();
            _allReleases = response;

            // Enrich SoftwareVersion display with software name
            if(_allVersions != null)
                foreach(SoftwareReleaseDto r in response)
                    if(r.SoftwareVersionId.HasValue)
                    {
                        SoftwareVersionDto? ver = _allVersions.FirstOrDefault(v => v.Id == r.SoftwareVersionId.Value);
                        if(ver?.Software != null)
                            r.SoftwareVersion = $"{ver.Software} - {r.SoftwareVersion}";
                    }

            foreach(SoftwareReleaseDto item in response) Releases.Add(item);
            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software releases");
            ErrorMessage = _localizer["FailedToLoadSoftwareReleases"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    public async Task LoadPickerDataAsync()
    {
        try { _allVersions = await _versionsService.GetAllAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading versions for picker"); }

        try
        {
            _allPlatforms = await _platformsService.GetAllAsync();
            Platforms.Clear();
            foreach(SoftwarePlatformDto p in _allPlatforms) Platforms.Add(p);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading platforms for picker"); }

        try { _allRegions = await _apiClient.Iso31661Numeric.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading regions for picker"); }

        try { _allCompanies = await _apiClient.Companies.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading companies for picker"); }
    }

    private void OpenAdd()
    {
        _editingId = null;
        EditPanelTitle = _localizer["AddSoftwareReleaseDialog_Title"];
        IsEditingExisting = false;
        ClearForm();
        UpdateVersionSuggestions(string.Empty);
        UpdateRegionSuggestions(string.Empty);
        UpdatePublisherSuggestions(string.Empty);
        IsEditing = true;
    }

    private async void OpenEdit(SoftwareReleaseDto? item)
    {
        if(item?.Id == null) return;

        _editingId        = item.Id;
        EditPanelTitle    = _localizer["EditSoftwareReleaseDialog_Title"];
        IsEditingExisting = true;
        ReleaseDate       = item.ReleaseDate;

        // Version picker
        if(item.SoftwareVersionId.HasValue && _allVersions != null)
        {
            SoftwareVersionDto? ver = _allVersions.FirstOrDefault(v => v.Id == item.SoftwareVersionId.Value);
            if(ver != null)
            {
                VersionSearchText = $"{ver.Software} - {ver.VersionString}";
                UpdateVersionSuggestions(VersionSearchText);
                SelectedVersion = VersionSuggestions.FirstOrDefault(v => v.Id == ver.Id);
            }
        }
        else
        {
            UpdateVersionSuggestions(string.Empty);
        }

        // Region picker
        if(item.RegionId.HasValue && _allRegions != null)
        {
            UpdateRegionSuggestions(string.Empty);
            SelectedRegion = RegionSuggestions.FirstOrDefault(r => r.Id == item.RegionId.Value);
            if(SelectedRegion != null) RegionSearchText = SelectedRegion.Name ?? string.Empty;
        }
        else
        {
            UpdateRegionSuggestions(string.Empty);
        }

        // Publisher picker
        if(item.PublisherId.HasValue && _allCompanies != null)
        {
            UpdatePublisherSuggestions(string.Empty);
            SelectedPublisher = PublisherSuggestions.FirstOrDefault(c => c.Id == item.PublisherId.Value);
            if(SelectedPublisher != null) PublisherSearchText = SelectedPublisher.Name ?? string.Empty;
        }
        else
        {
            UpdatePublisherSuggestions(string.Empty);
        }

        // Platform
        if(item.PlatformId.HasValue && _allPlatforms != null)
            SelectedPlatform = Platforms.FirstOrDefault(p => p.Id == item.PlatformId.Value);

        HasError = false; ErrorMessage = string.Empty;
        IsEditing = true;

        // Load barcodes and product codes
        if(item.Id.HasValue)
        {
            await LoadBarcodesAsync(item.Id.Value);
            await LoadProductCodesAsync(item.Id.Value);
        }
    }

    private async Task DeleteAsync(SoftwareReleaseDto? item)
    {
        if(item?.Id == null) return;
        try { await _service.DeleteAsync(item.Id.Value); await LoadAsync(); }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting release {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteSoftwareRelease"];
            HasError = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(SelectedVersion == null)
            {
                ErrorMessage = _localizer["VersionIsRequired"]; HasError = true; return;
            }

            var dto = new SoftwareReleaseDto
            {
                SoftwareVersionId = SelectedVersion.Id,
                PlatformId        = SelectedPlatform?.Id,
                RegionId          = SelectedRegion?.Id,
                PublisherId       = SelectedPublisher?.Id,
                ReleaseDate       = ReleaseDate
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

            IsEditing = false; ClearForm();
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving release");
            ErrorMessage = _localizer["FailedToSaveSoftwareRelease"];
            HasError = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing = false; _editingId = null;
        ClearForm();
        HasError = false; ErrorMessage = string.Empty;
    }

    public void ApplyFilter()
    {
        FilteredReleases.Clear();
        IEnumerable<SoftwareReleaseDto> source = (IEnumerable<SoftwareReleaseDto>?)_allReleases ?? Releases;
        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(r =>
                (r.SoftwareVersion != null && r.SoftwareVersion.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (r.Platform != null && r.Platform.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (r.Region != null && r.Region.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (r.Publisher != null && r.Publisher.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));
        foreach(SoftwareReleaseDto item in source) FilteredReleases.Add(item);
    }

    public void UpdateVersionSuggestions(string query)
    {
        VersionSuggestions.Clear();
        if(_allVersions == null) return;
        IEnumerable<SoftwareVersionDto> source = _allVersions;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(v =>
                (v.VersionString != null && v.VersionString.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (v.PublicVersion != null && v.PublicVersion.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (v.Software != null && v.Software.Contains(query, StringComparison.OrdinalIgnoreCase)));
        foreach(SoftwareVersionDto match in source) VersionSuggestions.Add(match);
    }

    public void UpdateRegionSuggestions(string query)
    {
        RegionSuggestions.Clear();
        if(_allRegions == null) return;
        IEnumerable<Iso31661NumericDto> source = _allRegions;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(r => r.Name != null && r.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(Iso31661NumericDto match in source) RegionSuggestions.Add(match);
    }

    public void UpdatePublisherSuggestions(string query)
    {
        PublisherSuggestions.Clear();
        if(_allCompanies == null) return;
        IEnumerable<CompanyDto> source = _allCompanies;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(c => c.Name != null && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(CompanyDto match in source) PublisherSuggestions.Add(match);
    }

    private async Task LoadBarcodesAsync(int releaseId)
    {
        Barcodes.Clear();
        try
        {
            List<SoftwareBarcodeDto> items = await _service.GetBarcodesAsync(releaseId);
            foreach(SoftwareBarcodeDto item in items) Barcodes.Add(item);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading barcodes for release {Id}", releaseId); }
    }

    private async Task LoadProductCodesAsync(int releaseId)
    {
        ProductCodes.Clear();
        try
        {
            List<SoftwareProductCodeDto> items = await _service.GetProductCodesAsync(releaseId);
            foreach(SoftwareProductCodeDto item in items) ProductCodes.Add(item);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading product codes for release {Id}", releaseId); }
    }

    private void ClearForm()
    {
        SelectedVersion    = null;
        VersionSearchText  = string.Empty;
        SelectedPlatform   = null;
        SelectedRegion     = null;
        RegionSearchText   = string.Empty;
        SelectedPublisher  = null;
        PublisherSearchText = string.Empty;
        ReleaseDate        = null;
        Barcodes.Clear();
        ProductCodes.Clear();
        HasError = false; ErrorMessage = string.Empty;
    }
}
