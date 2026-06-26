#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Marechai.ApiClient.Models;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.Data;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareReleasesViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareReleasesService                     _service;
    private readonly SoftwareVersionsService                     _versionsService;
    private readonly SoftwarePlatformsService                    _platformsService;
    private readonly Client                                   _apiClient;
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
    [ObservableProperty] private UnM49Dto? _selectedRegionToAdd;
    [ObservableProperty] private string _regionSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<UnM49Dto> _regionSuggestions = [];
    [ObservableProperty] private ObservableCollection<UnM49BySoftwareReleaseDto> _releaseRegions = [];
    [ObservableProperty] private ObservableCollection<string> _releaseRegionDisplays = [];
    [ObservableProperty] private Iso639Dto? _selectedLanguageToAdd;
    [ObservableProperty] private string _languageSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<Iso639Dto> _languageSuggestions = [];
    [ObservableProperty] private ObservableCollection<LanguageBySoftwareReleaseDto> _releaseLanguages = [];
    [ObservableProperty] private ObservableCollection<string> _releaseLanguageDisplays = [];
    [ObservableProperty] private CompanyDto? _selectedPublisher;
    [ObservableProperty] private string _publisherSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyDto> _publisherSuggestions = [];
    [ObservableProperty] private DateTimeOffset? _releaseDate;
    [ObservableProperty] private int _releaseDatePrecision;

    // Barcodes
    [ObservableProperty] private ObservableCollection<SoftwareBarcodeDto> _barcodes = [];
    [ObservableProperty] private ObservableCollection<string> _barcodeDisplays = [];
    [ObservableProperty] private string _newBarcodeCode = string.Empty;
    [ObservableProperty] private int _newBarcodeType;
    public List<string> BarcodeTypeItems { get; } = Enum.GetValues<BarcodeType>().Select(e => e.Humanize()).ToList();

    // Product Codes
    [ObservableProperty] private ObservableCollection<SoftwareProductCodeDto> _productCodes = [];
    [ObservableProperty] private ObservableCollection<string> _productCodeDisplays = [];
    [ObservableProperty] private string _newProductCodeCode = string.Empty;
    [ObservableProperty] private int _newProductCodeIssuer;
    public List<string> ProductCodeIssuerItems { get; } = Enum.GetValues<ProductCodeIssuer>().Select(e => e.Humanize()).ToList();

    // Minimum GPUs
    [ObservableProperty] private ObservableCollection<GpuBySoftwareReleaseDto> _minimumGpus = [];
    [ObservableProperty] private ObservableCollection<string> _minimumGpuDisplays = [];
    [ObservableProperty] private GpuDto? _selectedMinimumGpu;
    [ObservableProperty] private string _minimumGpuSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<GpuDto> _minimumGpuSuggestions = [];

    // Recommended GPUs
    [ObservableProperty] private ObservableCollection<GpuBySoftwareReleaseDto> _recommendedGpus = [];
    [ObservableProperty] private ObservableCollection<string> _recommendedGpuDisplays = [];
    [ObservableProperty] private GpuDto? _selectedRecommendedGpu;
    [ObservableProperty] private string _recommendedGpuSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<GpuDto> _recommendedGpuSuggestions = [];

    // Sound Synths
    [ObservableProperty] private ObservableCollection<SoundSynthBySoftwareReleaseDto> _soundSynths = [];
    [ObservableProperty] private ObservableCollection<string> _soundSynthDisplays = [];
    [ObservableProperty] private SoundSynthDto? _selectedSoundSynth;
    [ObservableProperty] private string _soundSynthSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoundSynthDto> _soundSynthSuggestions = [];

    // Compilation support: a release may belong to a SoftwareCompilation, but
    // compilation membership (included software/versions) is managed on the
    // compilation itself, not on individual releases.
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private bool _isCompilation;

    // Software picker for single releases
    [ObservableProperty] private SoftwareDto? _selectedSoftware;
    [ObservableProperty] private string _softwareSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareDto> _softwareSuggestions = [];
    private List<SoftwareDto>? _allSoftware;

    private int? _editingId;
    private int? _editingCompilationId;
    private List<SoftwareReleaseDto>? _allReleases;
    private List<SoftwareVersionDto>? _allVersions;
    private List<SoftwarePlatformDto>? _allPlatforms;
    private List<UnM49Dto>? _allRegions;
    private List<Iso639Dto>? _allLanguages;
    private List<CompanyDto>? _allCompanies;
    private List<GpuDto>? _allGpus;
    private List<SoundSynthDto>? _allSoundSynths;

    public AdminSoftwareReleasesViewModel(SoftwareReleasesService                  service,
                                          SoftwareVersionsService                  versionsService,
                                          SoftwarePlatformsService                 platformsService,
                                          Client                                   apiClient,
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

        AddBarcodeCommand          = new AsyncRelayCommand(AddBarcodeAsync);
        RemoveBarcodeCommand       = new AsyncRelayCommand<string>(RemoveBarcodeByDisplayAsync);
        AddProductCodeCommand      = new AsyncRelayCommand(AddProductCodeAsync);
        RemoveProductCodeCommand   = new AsyncRelayCommand<string>(RemoveProductCodeByDisplayAsync);
        AddMinimumGpuCommand       = new AsyncRelayCommand(AddMinimumGpuAsync);
        RemoveMinimumGpuCommand    = new AsyncRelayCommand<string>(RemoveMinimumGpuByDisplayAsync);
        AddRecommendedGpuCommand   = new AsyncRelayCommand(AddRecommendedGpuAsync);
        RemoveRecommendedGpuCommand = new AsyncRelayCommand<string>(RemoveRecommendedGpuByDisplayAsync);
        AddSoundSynthCommand       = new AsyncRelayCommand(AddSoundSynthAsync);
        RemoveSoundSynthCommand    = new AsyncRelayCommand<string>(RemoveSoundSynthByDisplayAsync);
        AddRegionCommand             = new AsyncRelayCommand(AddRegionAsync);
        RemoveRegionCommand          = new AsyncRelayCommand<string>(RemoveRegionByDisplayAsync);
        AddLanguageCommand           = new AsyncRelayCommand(AddLanguageAsync);
        RemoveLanguageCommand        = new AsyncRelayCommand<string>(RemoveLanguageByDisplayAsync);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                        LoadCommand       { get; }
    public IRelayCommand                             OpenAddCommand    { get; }
    public IRelayCommand<SoftwareReleaseDto>         OpenEditCommand   { get; }
    public IAsyncRelayCommand<SoftwareReleaseDto>    DeleteCommand     { get; }
    public IAsyncRelayCommand                        SaveCommand       { get; }
    public IRelayCommand                             CancelEditCommand { get; }

    public IAsyncRelayCommand         AddBarcodeCommand           { get; }
    public IAsyncRelayCommand<string> RemoveBarcodeCommand        { get; }
    public IAsyncRelayCommand         AddProductCodeCommand       { get; }
    public IAsyncRelayCommand<string> RemoveProductCodeCommand    { get; }
    public IAsyncRelayCommand         AddMinimumGpuCommand        { get; }
    public IAsyncRelayCommand<string> RemoveMinimumGpuCommand     { get; }
    public IAsyncRelayCommand         AddRecommendedGpuCommand    { get; }
    public IAsyncRelayCommand<string> RemoveRecommendedGpuCommand { get; }
    public IAsyncRelayCommand         AddSoundSynthCommand        { get; }
    public IAsyncRelayCommand<string> RemoveSoundSynthCommand     { get; }
    public IAsyncRelayCommand         AddRegionCommand             { get; }
    public IAsyncRelayCommand<string> RemoveRegionCommand          { get; }
    public IAsyncRelayCommand         AddLanguageCommand           { get; }
    public IAsyncRelayCommand<string> RemoveLanguageCommand        { get; }

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

        try { _allRegions = await _apiClient.UnM49.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading regions for picker"); }

        try { _allLanguages = await _apiClient.Languages.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading languages for picker"); }

        try { _allCompanies = await _apiClient.Companies.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading companies for picker"); }

        try { _allGpus = await _apiClient.Gpus.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading GPUs for picker"); }

        try { _allSoundSynths = await _apiClient.SoundSynths.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading sound synths for picker"); }

        try { _allSoftware = await _apiClient.Software.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading software for picker"); }
    }

    private void OpenAdd()
    {
        _editingId = null;
        EditPanelTitle = _localizer["AddSoftwareReleaseDialog_Title"];
        IsEditingExisting = false;
        ClearForm();
        UpdateVersionSuggestions(string.Empty);
        UpdateRegionSuggestions(string.Empty);
        UpdateLanguageSuggestions(string.Empty);
        UpdatePublisherSuggestions(string.Empty);
        UpdateMinimumGpuSuggestions(string.Empty);
        UpdateRecommendedGpuSuggestions(string.Empty);
        UpdateSoundSynthSuggestions(string.Empty);
        IsEditing = true;
    }

    private async void OpenEdit(SoftwareReleaseDto? item)
    {
        if(item?.Id == null) return;

        _editingId        = item.Id;
        EditPanelTitle    = _localizer["EditSoftwareReleaseDialog_Title"];
        IsEditingExisting = true;
        ReleaseDate       = item.ReleaseDate;
        ReleaseDatePrecision = item.ReleaseDatePrecision ?? 0;
        Title             = item.Title ?? string.Empty;
        IsCompilation     = item.SoftwareCompilationId is not null;
        _editingCompilationId = item.SoftwareCompilationId;

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

        // Region management - load existing regions
        ReleaseRegions.Clear();
        ReleaseRegionDisplays.Clear();
        UpdateRegionSuggestions(string.Empty);

        // Language management - load existing languages
        ReleaseLanguages.Clear();
        ReleaseLanguageDisplays.Clear();
        UpdateLanguageSuggestions(string.Empty);

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

        // Pre-fill GPU and SoundSynth pickers
        UpdateMinimumGpuSuggestions(string.Empty);
        UpdateRecommendedGpuSuggestions(string.Empty);
        UpdateSoundSynthSuggestions(string.Empty);

        HasError = false; ErrorMessage = string.Empty;
        IsEditing = true;

        // Load junctions
        if(item.Id.HasValue)
        {
            await LoadBarcodesAsync(item.Id.Value);
            await LoadProductCodesAsync(item.Id.Value);
            await LoadMinimumGpusAsync(item.Id.Value);
            await LoadRecommendedGpusAsync(item.Id.Value);
            await LoadSoundSynthsAsync(item.Id.Value);
            await LoadReleaseRegionsAsync(item.Id.Value);
            await LoadReleaseLanguagesAsync(item.Id.Value);
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
            if(SelectedVersion == null && !IsCompilation && SelectedSoftware == null)
            {
                ErrorMessage = _localizer["SoftwareIsRequired"]; HasError = true; return;
            }

            var dto = new SoftwareReleaseDto
            {
                Title             = string.IsNullOrWhiteSpace(Title) ? null : Title,
                SoftwareCompilationId = IsCompilation ? _editingCompilationId : null,
                SoftwareId        = IsCompilation ? null : SelectedSoftware?.Id,
                SoftwareVersionId = IsCompilation ? null : SelectedVersion?.Id,
                PlatformId        = SelectedPlatform?.Id,
                PublisherId       = SelectedPublisher?.Id,
                ReleaseDate       = ReleaseDate,
                ReleaseDatePrecision = ReleaseDatePrecision
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
                (r.Title != null && r.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (r.SoftwareVersion != null && r.SoftwareVersion.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (r.Platform != null && r.Platform.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (r.Regions != null && r.Regions.Any(rg => rg.RegionName != null && rg.RegionName.Contains(FilterText, StringComparison.OrdinalIgnoreCase))) ||
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
        var assignedIds = new HashSet<int?>(ReleaseRegions.Select(r => r.UnM49Id));
        IEnumerable<UnM49Dto> source = _allRegions.Where(r => !assignedIds.Contains(r.Id));
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(r => r.Name != null && r.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(UnM49Dto match in source) RegionSuggestions.Add(match);
    }

    public async Task AddRegionAsync()
    {
        if(SelectedRegionToAdd?.Id == null || _editingId == null) return;

        try
        {
            await _apiClient.Software.Releases[_editingId.Value].Regions.PostAsync(
                new UnM49BySoftwareReleaseDto { UnM49Id = SelectedRegionToAdd.Id });
            await LoadReleaseRegionsAsync(_editingId.Value);
            SelectedRegionToAdd = null;
            RegionSearchText = string.Empty;
            UpdateRegionSuggestions(string.Empty);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding region"); }
    }

    public async Task RemoveRegionByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;
        UnM49BySoftwareReleaseDto? region = ReleaseRegions.FirstOrDefault(r => r.RegionName == display);
        if(region?.UnM49Id == null) return;

        try
        {
            await _apiClient.Software.Releases[_editingId.Value].Regions[region.UnM49Id.Value].DeleteAsync();
            await LoadReleaseRegionsAsync(_editingId.Value);
            UpdateRegionSuggestions(RegionSearchText);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error removing region"); }
    }

    private async Task LoadReleaseRegionsAsync(int releaseId)
    {
        try
        {
            List<UnM49BySoftwareReleaseDto>? regions =
                await _apiClient.Software.Releases[releaseId].Regions.GetAsync();
            ReleaseRegions.Clear();
            ReleaseRegionDisplays.Clear();
            if(regions != null)
                foreach(UnM49BySoftwareReleaseDto r in regions)
                {
                    ReleaseRegions.Add(r);
                    ReleaseRegionDisplays.Add(r.RegionName ?? $"ID: {r.UnM49Id}");
                }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading release regions"); }
    }

    public void UpdateLanguageSuggestions(string query)
    {
        LanguageSuggestions.Clear();
        if(_allLanguages == null) return;
        var assignedCodes = new HashSet<string?>(ReleaseLanguages.Select(l => l.LanguageCode));
        IEnumerable<Iso639Dto> source = _allLanguages.Where(l => !assignedCodes.Contains(l.Id));
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(l => l.ReferenceName != null && l.ReferenceName.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(Iso639Dto match in source) LanguageSuggestions.Add(match);
    }

    public async Task AddLanguageAsync()
    {
        if(SelectedLanguageToAdd?.Id == null || _editingId == null) return;

        try
        {
            await _apiClient.Software.Releases[_editingId.Value].Languages.PostAsync(
                new LanguageBySoftwareReleaseDto { LanguageCode = SelectedLanguageToAdd.Id });
            await LoadReleaseLanguagesAsync(_editingId.Value);
            SelectedLanguageToAdd = null;
            LanguageSearchText = string.Empty;
            UpdateLanguageSuggestions(string.Empty);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding language"); }
    }

    public async Task RemoveLanguageByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;
        LanguageBySoftwareReleaseDto? lang = ReleaseLanguages.FirstOrDefault(l => l.Language == display);
        if(lang?.LanguageCode == null) return;

        try
        {
            await _apiClient.Software.Releases[_editingId.Value].Languages[lang.LanguageCode].DeleteAsync();
            await LoadReleaseLanguagesAsync(_editingId.Value);
            UpdateLanguageSuggestions(LanguageSearchText);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error removing language"); }
    }

    private async Task LoadReleaseLanguagesAsync(int releaseId)
    {
        try
        {
            List<LanguageBySoftwareReleaseDto>? languages =
                await _apiClient.Software.Releases[releaseId].Languages.GetAsync();
            ReleaseLanguages.Clear();
            ReleaseLanguageDisplays.Clear();
            if(languages != null)
                foreach(LanguageBySoftwareReleaseDto l in languages)
                {
                    ReleaseLanguages.Add(l);
                    ReleaseLanguageDisplays.Add(l.Language ?? $"Code: {l.LanguageCode}");
                }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading release languages"); }
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
        BarcodeDisplays.Clear();

        try
        {
            List<SoftwareBarcodeDto> items = await _service.GetBarcodesAsync(releaseId);

            foreach(SoftwareBarcodeDto item in items)
            {
                Barcodes.Add(item);
                BarcodeDisplays.Add($"{item.Code} ({((BarcodeType)(item.Type ?? 0)).Humanize()})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading barcodes for release {Id}", releaseId); }
    }

    private async Task LoadProductCodesAsync(int releaseId)
    {
        ProductCodes.Clear();
        ProductCodeDisplays.Clear();

        try
        {
            List<SoftwareProductCodeDto> items = await _service.GetProductCodesAsync(releaseId);

            foreach(SoftwareProductCodeDto item in items)
            {
                ProductCodes.Add(item);
                ProductCodeDisplays.Add($"{item.Code} ({((ProductCodeIssuer)(item.Issuer ?? 0)).Humanize()})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading product codes for release {Id}", releaseId); }
    }

    private async Task LoadMinimumGpusAsync(int releaseId)
    {
        MinimumGpus.Clear();
        MinimumGpuDisplays.Clear();

        try
        {
            List<GpuBySoftwareReleaseDto> items = await _service.GetMinimumGpusAsync(releaseId);

            foreach(GpuBySoftwareReleaseDto item in items)
            {
                MinimumGpus.Add(item);
                MinimumGpuDisplays.Add(item.Gpu ?? $"GPU #{item.GpuId}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading minimum GPUs for release {Id}", releaseId); }
    }

    private async Task LoadRecommendedGpusAsync(int releaseId)
    {
        RecommendedGpus.Clear();
        RecommendedGpuDisplays.Clear();

        try
        {
            List<GpuBySoftwareReleaseDto> items = await _service.GetRecommendedGpusAsync(releaseId);

            foreach(GpuBySoftwareReleaseDto item in items)
            {
                RecommendedGpus.Add(item);
                RecommendedGpuDisplays.Add(item.Gpu ?? $"GPU #{item.GpuId}");
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading recommended GPUs for release {Id}", releaseId);
        }
    }

    private async Task LoadSoundSynthsAsync(int releaseId)
    {
        SoundSynths.Clear();
        SoundSynthDisplays.Clear();

        try
        {
            List<SoundSynthBySoftwareReleaseDto> items = await _service.GetSoundSynthsAsync(releaseId);

            foreach(SoundSynthBySoftwareReleaseDto item in items)
            {
                SoundSynths.Add(item);
                SoundSynthDisplays.Add(item.SoundSynth ?? $"SoundSynth #{item.SoundSynthId}");
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading sound synths for release {Id}", releaseId);
        }
    }

    // --- Barcode add/remove ---

    private async Task AddBarcodeAsync()
    {
        if(_editingId == null || string.IsNullOrWhiteSpace(NewBarcodeCode)) return;

        try
        {
            var dto = new SoftwareBarcodeDto
            {
                ReleaseId = _editingId,
                Code      = NewBarcodeCode,
                Type      = NewBarcodeType
            };

            await _service.AddBarcodeAsync(dto);
            NewBarcodeCode = string.Empty;
            NewBarcodeType = 0;
            await LoadBarcodesAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding barcode"); }
    }

    private async Task RemoveBarcodeByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;

        int idx = BarcodeDisplays.IndexOf(display);

        if(idx < 0 || idx >= Barcodes.Count || Barcodes[idx].Id == null) return;

        try
        {
            await _service.RemoveBarcodeAsync(Barcodes[idx].Id!.Value);
            await LoadBarcodesAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error removing barcode"); }
    }

    // --- Product Code add/remove ---

    private async Task AddProductCodeAsync()
    {
        if(_editingId == null || string.IsNullOrWhiteSpace(NewProductCodeCode)) return;

        try
        {
            var dto = new SoftwareProductCodeDto
            {
                ReleaseId = _editingId,
                Code      = NewProductCodeCode,
                Issuer    = NewProductCodeIssuer
            };

            await _service.AddProductCodeAsync(dto);
            NewProductCodeCode   = string.Empty;
            NewProductCodeIssuer = 0;
            await LoadProductCodesAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding product code"); }
    }

    private async Task RemoveProductCodeByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;

        int idx = ProductCodeDisplays.IndexOf(display);

        if(idx < 0 || idx >= ProductCodes.Count || ProductCodes[idx].Id == null) return;

        try
        {
            await _service.RemoveProductCodeAsync(ProductCodes[idx].Id!.Value);
            await LoadProductCodesAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error removing product code"); }
    }

    // --- Minimum GPU add/remove ---

    private async Task AddMinimumGpuAsync()
    {
        if(_editingId == null || SelectedMinimumGpu?.Id == null) return;

        try
        {
            var dto = new GpuBySoftwareReleaseDto
            {
                ReleaseId = _editingId,
                GpuId     = SelectedMinimumGpu.Id
            };

            await _service.AddMinimumGpuAsync(dto);
            SelectedMinimumGpu   = null;
            MinimumGpuSearchText = string.Empty;
            await LoadMinimumGpusAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding minimum GPU"); }
    }

    private async Task RemoveMinimumGpuByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;

        int idx = MinimumGpuDisplays.IndexOf(display);

        if(idx < 0 || idx >= MinimumGpus.Count) return;

        GpuBySoftwareReleaseDto item = MinimumGpus[idx];

        try
        {
            await _service.RemoveMinimumGpuAsync(_editingId.Value.ToString(), item.GpuId?.ToString() ?? "0");
            await LoadMinimumGpusAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error removing minimum GPU"); }
    }

    // --- Recommended GPU add/remove ---

    private async Task AddRecommendedGpuAsync()
    {
        if(_editingId == null || SelectedRecommendedGpu?.Id == null) return;

        try
        {
            var dto = new GpuBySoftwareReleaseDto
            {
                ReleaseId = _editingId,
                GpuId     = SelectedRecommendedGpu.Id
            };

            await _service.AddRecommendedGpuAsync(dto);
            SelectedRecommendedGpu   = null;
            RecommendedGpuSearchText = string.Empty;
            await LoadRecommendedGpusAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding recommended GPU"); }
    }

    private async Task RemoveRecommendedGpuByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;

        int idx = RecommendedGpuDisplays.IndexOf(display);

        if(idx < 0 || idx >= RecommendedGpus.Count) return;

        GpuBySoftwareReleaseDto item = RecommendedGpus[idx];

        try
        {
            await _service.RemoveRecommendedGpuAsync(_editingId.Value.ToString(), item.GpuId?.ToString() ?? "0");
            await LoadRecommendedGpusAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error removing recommended GPU"); }
    }

    // --- Sound Synth add/remove ---

    private async Task AddSoundSynthAsync()
    {
        if(_editingId == null || SelectedSoundSynth?.Id == null) return;

        try
        {
            var dto = new SoundSynthBySoftwareReleaseDto
            {
                ReleaseId    = _editingId,
                SoundSynthId = SelectedSoundSynth.Id
            };

            await _service.AddSoundSynthAsync(dto);
            SelectedSoundSynth   = null;
            SoundSynthSearchText = string.Empty;
            await LoadSoundSynthsAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding sound synth"); }
    }

    private async Task RemoveSoundSynthByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;

        int idx = SoundSynthDisplays.IndexOf(display);

        if(idx < 0 || idx >= SoundSynths.Count) return;

        SoundSynthBySoftwareReleaseDto item = SoundSynths[idx];

        try
        {
            await _service.RemoveSoundSynthAsync(_editingId.Value.ToString(),
                                                 item.SoundSynthId?.ToString() ?? "0");

            await LoadSoundSynthsAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error removing sound synth"); }
    }

    // --- Suggestion methods for GPU/Sound Synth pickers ---

    public void UpdateMinimumGpuSuggestions(string query)
    {
        MinimumGpuSuggestions.Clear();

        if(_allGpus == null) return;

        IEnumerable<GpuDto> source = _allGpus;

        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(g =>
                                      (g.Name != null &&
                                       g.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                      (g.Company != null &&
                                       g.Company.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                      (g.ModelCode != null &&
                                       g.ModelCode.Contains(query, StringComparison.OrdinalIgnoreCase)));

        foreach(GpuDto match in source) MinimumGpuSuggestions.Add(match);
    }

    public void UpdateRecommendedGpuSuggestions(string query)
    {
        RecommendedGpuSuggestions.Clear();

        if(_allGpus == null) return;

        IEnumerable<GpuDto> source = _allGpus;

        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(g =>
                                      (g.Name != null &&
                                       g.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                      (g.Company != null &&
                                       g.Company.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                      (g.ModelCode != null &&
                                       g.ModelCode.Contains(query, StringComparison.OrdinalIgnoreCase)));

        foreach(GpuDto match in source) RecommendedGpuSuggestions.Add(match);
    }

    public void UpdateSoundSynthSuggestions(string query)
    {
        SoundSynthSuggestions.Clear();

        if(_allSoundSynths == null) return;

        IEnumerable<SoundSynthDto> source = _allSoundSynths;

        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(s =>
                                      (s.Name != null &&
                                       s.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                      (s.Company != null &&
                                       s.Company.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                      (s.ModelCode != null &&
                                       s.ModelCode.Contains(query, StringComparison.OrdinalIgnoreCase)));

        foreach(SoundSynthDto match in source) SoundSynthSuggestions.Add(match);
    }

    private void ClearForm()
    {
        SelectedVersion    = null;
        VersionSearchText  = string.Empty;
        SelectedPlatform   = null;
        SelectedRegionToAdd = null;
        RegionSearchText   = string.Empty;
        ReleaseRegions.Clear();
        ReleaseRegionDisplays.Clear();
        SelectedPublisher  = null;
        PublisherSearchText = string.Empty;
        ReleaseDate        = null;
        ReleaseDatePrecision = 0;
        Barcodes.Clear();
        BarcodeDisplays.Clear();
        NewBarcodeCode = string.Empty;
        NewBarcodeType = 0;
        ProductCodes.Clear();
        ProductCodeDisplays.Clear();
        NewProductCodeCode   = string.Empty;
        NewProductCodeIssuer = 0;
        MinimumGpus.Clear();
        MinimumGpuDisplays.Clear();
        SelectedMinimumGpu   = null;
        MinimumGpuSearchText = string.Empty;
        RecommendedGpus.Clear();
        RecommendedGpuDisplays.Clear();
        SelectedRecommendedGpu   = null;
        RecommendedGpuSearchText = string.Empty;
        SoundSynths.Clear();
        SoundSynthDisplays.Clear();
        SelectedSoundSynth   = null;
        SoundSynthSearchText = string.Empty;
        Title                = string.Empty;
        IsCompilation        = false;
        _editingCompilationId = null;
        SelectedSoftware           = null;
        SoftwareSearchText         = string.Empty;
        HasError = false; ErrorMessage = string.Empty;
    }

    private void UpdateSoftwareSuggestions(string query)
    {
        SoftwareSuggestions.Clear();
        if(_allSoftware == null) return;
        IEnumerable<SoftwareDto> source = _allSoftware;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(s =>
                s.Name != null && s.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(SoftwareDto match in source) SoftwareSuggestions.Add(match);
    }
}
