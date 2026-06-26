#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.Data;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminWwpcImportReviewViewModel : ObservableObject, IRegionAware
{
    private readonly WwpcImportsService                         _service;
    private readonly SoftwareService                            _softwareService;
    private readonly CompaniesService                           _companiesService;
    private readonly SoftwarePlatformsService                   _platformsService;
    private readonly SoftwareVersionsService                    _versionsService;
    private readonly Client                                     _apiClient;
    private readonly IJwtService                                _jwtService;
    private readonly ITokenService                              _tokenService;
    private readonly IStringLocalizer                           _localizer;
    private readonly ILogger<AdminWwpcImportReviewViewModel>    _logger;
    private readonly IRegionManager                             _regionManager;

    private List<SoftwareGenreDto> _allGenres = [];
    private List<SoftwarePlatformDto> _allPlatforms = [];
    private List<CompanyDto> _allCompanies = [];
    private List<SoftwareVersionDto> _existingVersions = [];
    private long _currentImportId;

    [ObservableProperty] private WwpcPendingDetailDto? _detail;
    [ObservableProperty] private ObservableCollection<WwpcNameMatchCandidateItem> _nameMatches = [];
    [ObservableProperty] private ObservableCollection<WwpcCompanyMatchCandidateItem> _vendorMatches = [];
    [ObservableProperty] private ObservableCollection<WwpcGenreChipItem> _selectedGenres = [];
    [ObservableProperty] private ObservableCollection<SoftwareGenreDto> _genreSuggestions = [];
    [ObservableProperty] private ObservableCollection<CompanyDto> _companySuggestions = [];
    [ObservableProperty] private ObservableCollection<SoftwarePlatformDto> _platformSuggestions = [];
    [ObservableProperty] private ObservableCollection<WwpcVersionDecisionItem> _versionDecisions = [];
    [ObservableProperty] private ObservableCollection<WwpcScreenshotDecisionItem> _screenshotDecisions = [];
    [ObservableProperty] private ObservableCollection<SoftwareVersionDto> _existingVersionChoices = [];
    [ObservableProperty] private CompanyDto? _selectedVendor;
    [ObservableProperty] private SoftwarePlatformDto? _bulkPlatform;
    [ObservableProperty] private SoftwareGenreDto? _selectedGenreToAdd;
    [ObservableProperty] private string _genreSearchText = string.Empty;
    [ObservableProperty] private string _companySearchText = string.Empty;
    [ObservableProperty] private string _platformSearchText = string.Empty;
    [ObservableProperty] private string _museumDescription = string.Empty;
    [ObservableProperty] private string _nameOverride = string.Empty;
    [ObservableProperty] private int _kindOverride = (int)SoftwareKind.Application;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private ulong? _mergeTargetId;
    [ObservableProperty] private string _mergeTargetName = string.Empty;
    [ObservableProperty] private bool _showCreateCompany;
    [ObservableProperty] private bool _isCreatingCompany;
    [ObservableProperty] private string _newCompanyName = string.Empty;
    [ObservableProperty] private bool _hasExactMatch;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _statusIsSuccess;

    public AdminWwpcImportReviewViewModel(WwpcImportsService service,
                                          SoftwareService softwareService,
                                          CompaniesService companiesService,
                                          SoftwarePlatformsService platformsService,
                                          SoftwareVersionsService versionsService,
                                          Client apiClient,
                                          IJwtService jwtService,
                                          ITokenService tokenService,
                                          IStringLocalizer localizer,
                                          ILogger<AdminWwpcImportReviewViewModel> logger,
                                          IRegionManager regionManager)
    {
        _service          = service;
        _softwareService  = softwareService;
        _companiesService = companiesService;
        _platformsService = platformsService;
        _versionsService  = versionsService;
        _apiClient        = apiClient;
        _jwtService       = jwtService;
        _tokenService     = tokenService;
        _localizer        = localizer;
        _logger           = logger;
        _regionManager    = regionManager;

        AcceptCommand           = new AsyncRelayCommand(AcceptAsync);
        SkipCommand             = new AsyncRelayCommand(SkipAsync);
        DiscardCommand          = new AsyncRelayCommand(DiscardAsync);
        GoBackCommand           = new RelayCommand(GoBack);
        RefreshMatchesCommand   = new AsyncRelayCommand(RefreshMatchesAsync);
        ApplyBulkPlatformCommand = new RelayCommand(ApplyBulkPlatform);
        CreateCompanyCommand    = new AsyncRelayCommand(CreateCompanyAsync);
        ExitMergeModeCommand    = new RelayCommand(ExitMergeMode);
        EnableAllVersionsCommand = new RelayCommand(() =>
        {
            foreach(WwpcVersionDecisionItem item in VersionDecisions) item.Include = true;
        });
        DisableAllVersionsCommand = new RelayCommand(() =>
        {
            foreach(WwpcVersionDecisionItem item in VersionDecisions) item.Include = false;
        });
        EnableAllScreenshotsCommand = new RelayCommand(() =>
        {
            foreach(WwpcScreenshotDecisionItem item in ScreenshotDecisions) item.Include = true;
        });
        DisableAllScreenshotsCommand = new RelayCommand(() =>
        {
            foreach(WwpcScreenshotDecisionItem item in ScreenshotDecisions) item.Include = false;
        });

        CheckAdminRole();
    }

    public IAsyncRelayCommand AcceptCommand { get; }
    public IAsyncRelayCommand SkipCommand { get; }
    public IAsyncRelayCommand DiscardCommand { get; }
    public IRelayCommand GoBackCommand { get; }
    public IAsyncRelayCommand RefreshMatchesCommand { get; }
    public IRelayCommand ApplyBulkPlatformCommand { get; }
    public IAsyncRelayCommand CreateCompanyCommand { get; }
    public IRelayCommand ExitMergeModeCommand { get; }
    public IRelayCommand EnableAllVersionsCommand { get; }
    public IRelayCommand DisableAllVersionsCommand { get; }
    public IRelayCommand EnableAllScreenshotsCommand { get; }
    public IRelayCommand DisableAllScreenshotsCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();
        if(!IsAdmin) return;

        if(navigationContext.Parameters.TryGetValue<long>(NavParamKeys.WwpcImportId, out long importId))
            _currentImportId = importId;
        else if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.WwpcImportId, out int importIdInt))
            _currentImportId = importIdInt;

        await EnsureLookupsLoadedAsync();

        if(_currentImportId > 0)
            await LoadDetailAsync(_currentImportId);
    }

    public string ProductTypeLabel(WwpcProductType type) => type switch
    {
        WwpcProductType.Application => _localizer["WwpcProductTypeApplication"],
        WwpcProductType.DevTool     => _localizer["WwpcProductTypeDevTool"],
        WwpcProductType.System      => _localizer["WwpcProductTypeSystem"],
        _                           => type.ToString()
    };

    public string KindLabel(byte? kind) => kind.HasValue ? ((SoftwareKind)kind.Value).Humanize() : "—";

    public string DetailProductTypeLabel => Detail is null ? string.Empty : ProductTypeLabel((WwpcProductType)(Detail.ProductType ?? 0));

    public string AbsoluteWwpcUrl(string? url)
    {
        if(string.IsNullOrWhiteSpace(url)) return string.Empty;
        if(url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return url;

        return "https://winworldpc.com" + (url.StartsWith('/') ? url : "/" + url);
    }

    public void UpdateGenreSuggestions(string query)
    {
        GenreSuggestions.Clear();

        IEnumerable<SoftwareGenreDto> source = _allGenres.Where(g => g.Type == (int)SoftwareGenreType.Category &&
                                                                      !SelectedGenres.Any(selected => selected.Id == g.Id));
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(g => (g.Name ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(SoftwareGenreDto genre in source.OrderBy(g => g.Name).Take(30))
            GenreSuggestions.Add(genre);
    }

    public void UpdateCompanySuggestions(string query)
    {
        CompanySuggestions.Clear();

        IEnumerable<CompanyDto> source = _allCompanies;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(c => (c.Name ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(CompanyDto company in source.OrderBy(c => c.Name).Take(30))
            CompanySuggestions.Add(company);
    }

    public void UpdatePlatformSuggestions(string query)
    {
        PlatformSuggestions.Clear();

        IEnumerable<SoftwarePlatformDto> source = _allPlatforms;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(p => (p.Name ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(SoftwarePlatformDto platform in source.OrderBy(p => p.Name).Take(30))
            PlatformSuggestions.Add(platform);
    }

    public void AddSelectedGenre()
    {
        if(SelectedGenreToAdd?.Id is not int id || SelectedGenres.Any(g => g.Id == id)) return;

        SelectedGenres.Add(new WwpcGenreChipItem { Genre = SelectedGenreToAdd });
        SelectedGenreToAdd = null;
        GenreSearchText    = string.Empty;
        UpdateGenreSuggestions(string.Empty);
    }

    public void RemoveGenre(WwpcGenreChipItem? genre)
    {
        if(genre is null) return;
        SelectedGenres.Remove(genre);
        UpdateGenreSuggestions(GenreSearchText);
    }

    public async Task EnterMergeModeAsync(WwpcNameMatchCandidateItem? item)
    {
        if(item?.SoftwareId <= 0) return;

        MergeTargetId   = item.SoftwareId;
        MergeTargetName = item.Name;
        _existingVersions = await _versionsService.GetBySoftwareAsync((int)item.SoftwareId);

        ExistingVersionChoices.Clear();
        foreach(SoftwareVersionDto version in _existingVersions)
            ExistingVersionChoices.Add(version);

        foreach(WwpcVersionDecisionItem decision in VersionDecisions)
            decision.LinkedExistingVersion = null;
    }

    public void PickVendor(WwpcCompanyMatchCandidateItem? item)
    {
        if(item?.Candidate.CompanyId is not int companyId) return;
        SelectedVendor = _allCompanies.FirstOrDefault(company => company.Id == companyId);
    }

    public void OpenCreateCompany()
    {
        ShowCreateCompany = true;
        string? seed = Detail?.VendorName;
        if(string.IsNullOrWhiteSpace(seed)) seed = Detail?.Name;
        NewCompanyName = seed ?? string.Empty;
    }

    public async Task CreateCompanyAsync()
    {
        if(string.IsNullOrWhiteSpace(NewCompanyName)) return;

        try
        {
            IsCreatingCompany = true;
            var dto = new CompanyDto
            {
                Name   = NewCompanyName.Trim(),
                Status = (int)CompanyStatus.Unknown
            };

            int? newId = await _companiesService.CreateAsync(dto);

            if(!newId.HasValue)
            {
                SetStatus(false, _localizer["FailedToCreateCompany"]);
                return;
            }

            dto.Id = newId.Value;
            _allCompanies.Add(dto);
            SelectedVendor   = dto;
            ShowCreateCompany = false;
            NewCompanyName    = string.Empty;
            SetStatus(true, string.Format(_localizer["CreatedCompanyMessage"].Value, dto.Name));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating company for wwpc review");
            SetStatus(false, _localizer["FailedToCreateCompany"]);
        }
        finally
        {
            IsCreatingCompany = false;
        }
    }

    public void ExitMergeMode()
    {
        MergeTargetId   = null;
        MergeTargetName = string.Empty;
        _existingVersions = [];
        ExistingVersionChoices.Clear();

        foreach(WwpcVersionDecisionItem decision in VersionDecisions)
            decision.LinkedExistingVersion = null;
    }

    public async Task RefreshMatchesAsync()
    {
        if(Detail is null) return;

        WwpcNameMatchCandidatesDto nameMatches = await _service.GetNameMatchesAsync(Detail.Id ?? 0, NameOverride);
        WwpcCompanyMatchCandidatesDto vendorMatches = await _service.GetVendorMatchesAsync(Detail.Id ?? 0, Detail.VendorName);

        NameMatches.Clear();
        HasExactMatch = false;

        foreach(WwpcNameMatchCandidateDto match in nameMatches.Matches ?? [])
        {
            if(match.MatchKind == (int)WwpcNameMatchKind.ExactNormalized) HasExactMatch = true;

            NameMatches.Add(new WwpcNameMatchCandidateItem
            {
                Candidate = match,
                KindLabel = KindLabel(match.Kind)
            });
        }

        VendorMatches.Clear();
        foreach(WwpcCompanyMatchCandidateDto match in vendorMatches.Matches ?? [])
            VendorMatches.Add(new WwpcCompanyMatchCandidateItem { Candidate = match });
    }

    public async Task AcceptAsync()
    {
        if(Detail is null) return;

        try
        {
            IsBusy       = true;
            HasError     = false;
            ErrorMessage = string.Empty;

            var dto = new AcceptWwpcImportDto
            {
                Mode                    = (int)(MergeTargetId.HasValue ? WwpcAcceptMode.MergeIntoExisting : WwpcAcceptMode.CreateNew),
                TargetSoftwareId        = MergeTargetId.HasValue ? (int?)MergeTargetId.Value : null,
                NameOverride            = NameOverride,
                KindOverride            = (byte?)KindOverride,
                MuseumDescriptionEdited = MuseumDescription,
                VendorCompanyId         = SelectedVendor?.Id,
                GenreIds                = SelectedGenres.Select(genre => (int?)genre.Id).ToList(),
                Versions = VersionDecisions.Select(version => new AcceptWwpcVersionDecisionDto
                {
                    WwpcVersionId           = version.Id,
                    Include                 = version.Include,
                    VersionStringOverride   = version.VersionStringOverride,
                    LinkToExistingVersionId = version.LinkedExistingVersion?.Id
                }).ToList(),
                Screenshots = ScreenshotDecisions.Select(screenshot => new AcceptWwpcScreenshotDecisionDto
                {
                    WwpcScreenshotId = screenshot.Id,
                    Include          = screenshot.Include,
                    CaptionOverride  = screenshot.CaptionOverride,
                    SoftwarePlatformId = screenshot.Platform?.Id,
                    WwpcVersionId    = screenshot.LinkedVersion?.Id > 0 ? screenshot.LinkedVersion.Id : null
                }).ToList()
            };

            (AcceptWwpcImportResultDto? result, string? error) = await _service.AcceptAsync(Detail.Id ?? 0, dto);

            if(error != null || result?.Success != true)
            {
                HasError     = true;
                ErrorMessage = error ?? result?.Error ?? _localizer["FailedToAcceptWwpcImport"];
                return;
            }

            SetStatus(true, string.Format(_localizer["AcceptedWwpcImportMessage"].Value,
                                          result.PromotedSoftwareId, result.InsertedVersionCount,
                                          result.InsertedScreenshotCount, result.InsertedDescriptionCount,
                                          result.InsertedGenreCount));

            await AdvanceToNextAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error accepting wwpc import");
            HasError     = true;
            ErrorMessage = _localizer["FailedToAcceptWwpcImport"];
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SkipAsync()
    {
        if(Detail is null) return;

        IsBusy = true;

        try
        {
            bool ok = await _service.SkipAsync(Detail.Id ?? 0);
            if(ok) SetStatus(true, _localizer["SkippedWwpcImportMessage"]);
            else
            {
                HasError     = true;
                ErrorMessage = _localizer["FailedToSkipWwpcImport"];
                return;
            }

            await AdvanceToNextAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DiscardAsync()
    {
        if(Detail is null) return;

        IsBusy = true;

        try
        {
            bool ok = await _service.DiscardAsync(Detail.Id ?? 0);
            if(ok) SetStatus(true, _localizer["DiscardedWwpcImportMessage"]);
            else
            {
                HasError     = true;
                ErrorMessage = _localizer["FailedToDiscardWwpcImport"];
                return;
            }

            await AdvanceToNextAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void GoBack()
    {
        if(_regionManager.Regions[RegionNames.Content].NavigationService.Journal.CanGoBack)
            _regionManager.Regions[RegionNames.Content].NavigationService.Journal.GoBack();
        else
            _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminWwpcImportsPage));
    }

    private async Task EnsureLookupsLoadedAsync()
    {
        if(_allGenres.Count > 0 && _allPlatforms.Count > 0 && _allCompanies.Count > 0) return;

        try
        {
            List<SoftwareGenreDto>? genres = await _apiClient.Software.Genres.GetAsync(config =>
            {
                config.QueryParameters.IncludeUnused = true;
            });
            _allGenres = genres ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software genres for wwpc review");
        }

        _allPlatforms = await _platformsService.GetAllAsync();
        _allCompanies = await _companiesService.GetAllCompaniesAsync();
    }

    private async Task LoadDetailAsync(long id)
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;

            WwpcPendingDetailDto? detail = await _service.GetByIdAsync(id);

            if(detail is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["FailedToLoadWwpcImportDetail"];
                return;
            }

            Detail             = detail;
            _currentImportId   = detail.Id ?? 0;
            MuseumDescription  = detail.EnglishDescriptionMuseum ?? string.Empty;
            NameOverride       = detail.Name ?? string.Empty;
            KindOverride       = (int)SoftwareKind.Application;
            SelectedVendor     = detail.SuggestedVendorCompanyId.HasValue
                                     ? _allCompanies.FirstOrDefault(c => c.Id == detail.SuggestedVendorCompanyId.Value)
                                     : null;
            ShowCreateCompany  = false;
            NewCompanyName     = string.Empty;
            ExitMergeMode();

            SelectedGenres.Clear();
            foreach(int? genreId in detail.SuggestedGenreIds ?? [])
            {
                if(!genreId.HasValue) continue;
                SoftwareGenreDto? genre = _allGenres.FirstOrDefault(g => g.Id == genreId);
                if(genre is not null)
                    SelectedGenres.Add(new WwpcGenreChipItem { Genre = genre });
            }

            VersionDecisions.Clear();
            foreach(WwpcVersionDto version in (detail.Versions ?? []).OrderBy(v => v.MajorRelease)
                                                             .ThenBy(v => v.VersionString))
            {
                string combined = string.IsNullOrWhiteSpace(version.MajorRelease)
                    ? version.VersionString ?? string.Empty
                    : $"{version.MajorRelease} / {version.VersionString}";

                VersionDecisions.Add(new WwpcVersionDecisionItem
                {
                    Id                    = version.Id ?? 0,
                    Include               = version.IsEnabledByDefault ?? true,
                    MajorRelease          = version.MajorRelease ?? string.Empty,
                    OriginalVersionString = version.VersionString ?? string.Empty,
                    VersionStringOverride = combined,
                    Language              = version.Language ?? string.Empty,
                    Architecture          = version.Architecture ?? string.Empty,
                    MediaKind             = version.MediaKind ?? string.Empty,
                    SizeText              = version.SizeText ?? string.Empty,
                    DownloadUrl           = version.DownloadUrl ?? string.Empty
                });
            }

            ScreenshotDecisions.Clear();
            foreach(WwpcScreenshotDto screenshot in detail.Screenshots ?? [])
            {
                WwpcVersionDecisionItem? linkedVersion = VersionDecisions.FirstOrDefault(v =>
                    string.Equals(v.MajorRelease, screenshot.MajorRelease ?? string.Empty, StringComparison.Ordinal));

                ScreenshotDecisions.Add(new WwpcScreenshotDecisionItem
                {
                    Id              = screenshot.Id ?? 0,
                    Include         = screenshot.IsEnabledByDefault ?? true,
                    MajorRelease    = screenshot.MajorRelease ?? string.Empty,
                    SourceUrl       = screenshot.SourceUrl ?? string.Empty,
                    ImageUrl        = screenshot.ImageUrl ?? string.Empty,
                    OriginalCaption = screenshot.Caption ?? string.Empty,
                    CaptionOverride = screenshot.Caption ?? string.Empty,
                    Platform        = screenshot.SuggestedSoftwarePlatformId.HasValue
                                          ? _allPlatforms.FirstOrDefault(p => p.Id == screenshot.SuggestedSoftwarePlatformId.Value)
                                          : null,
                    LinkedVersion   = linkedVersion
                });
            }

            UpdateGenreSuggestions(string.Empty);
            UpdateCompanySuggestions(string.Empty);
            UpdatePlatformSuggestions(string.Empty);
            await RefreshMatchesAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading wwpc import review detail");
            HasError     = true;
            ErrorMessage = _localizer["FailedToLoadWwpcImportDetail"];
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AdvanceToNextAsync()
    {
        WwpcPendingDetailDto? next = await _service.GetNextAsync(_currentImportId);

        if(next is null)
        {
            _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminWwpcImportsPage));
            return;
        }

        await LoadDetailAsync(next.Id ?? 0);
    }

    private void ApplyBulkPlatform()
    {
        if(BulkPlatform is null) return;

        foreach(WwpcScreenshotDecisionItem screenshot in ScreenshotDecisions)
            screenshot.Platform = BulkPlatform;
    }

    private void CheckAdminRole()
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
                      roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    private void SetStatus(bool success, string message)
    {
        StatusIsSuccess = success;
        StatusMessage   = message;
    }

    partial void OnGenreSearchTextChanged(string value) => UpdateGenreSuggestions(value);
    partial void OnCompanySearchTextChanged(string value) => UpdateCompanySuggestions(value);
    partial void OnPlatformSearchTextChanged(string value) => UpdatePlatformSuggestions(value);
    partial void OnNameOverrideChanged(string value) => _ = RefreshMatchesAsync();
}
