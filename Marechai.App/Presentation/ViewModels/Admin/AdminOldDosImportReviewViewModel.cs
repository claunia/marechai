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
using Marechai.Data.Helpers;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminOldDosImportReviewViewModel : ObservableObject, IRegionAware
{
    private readonly OldDosImportsService _service;
    private readonly CompaniesService _companiesService;
    private readonly SoftwarePlatformsService _platformsService;
    private readonly IJwtService _jwtService;
    private readonly ITokenService _tokenService;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<AdminOldDosImportReviewViewModel> _logger;
    private readonly IRegionManager _regionManager;
    private readonly Client _apiClient;

    private List<SoftwareGenreDto> _allGenres = [];
    private List<SoftwarePlatformDto> _allPlatforms = [];
    private List<CompanyDto> _allCompanies = [];
    private long _currentImportId;

    [ObservableProperty] private OldDosPendingDetailDto? _detail;
    [ObservableProperty] private ObservableCollection<OldDosNameMatchCandidateItem> _nameMatches = [];
    [ObservableProperty] private ObservableCollection<OldDosVersionDecisionItem> _versionDecisions = [];
    [ObservableProperty] private ObservableCollection<SoftwareGenreDto> _genreSuggestions = [];
    [ObservableProperty] private ObservableCollection<CompanyDto> _companySuggestions = [];
    [ObservableProperty] private ObservableCollection<SoftwarePlatformDto> _platformSuggestions = [];
    [ObservableProperty] private ObservableCollection<OldDosKindOption> _kindOptions = [];
    [ObservableProperty] private ObservableCollection<OldDosEnumOption> _releasePrecisionOptions = [];
    [ObservableProperty] private ObservableCollection<SoftwareGenreDto> _selectedGenres = [];
    [ObservableProperty] private CompanyDto? _selectedDeveloper;
    [ObservableProperty] private SoftwareGenreDto? _selectedGenreToAdd;
    [ObservableProperty] private OldDosKindOption? _selectedKindOption;
    [ObservableProperty] private string _genreSearchText = string.Empty;
    [ObservableProperty] private string _companySearchText = string.Empty;
    [ObservableProperty] private string _museumDescription = string.Empty;
    [ObservableProperty] private string _nameOverride = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private int? _mergeTargetId;
    [ObservableProperty] private string _mergeTargetName = string.Empty;
    [ObservableProperty] private bool _showCreateCompany;
    [ObservableProperty] private bool _isCreatingCompany;
    [ObservableProperty] private string _newCompanyName = string.Empty;
    [ObservableProperty] private bool _hasExactMatch;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _statusIsSuccess;

    public AdminOldDosImportReviewViewModel(OldDosImportsService service,
                                            CompaniesService companiesService,
                                            SoftwarePlatformsService platformsService,
                                            Client apiClient,
                                            IJwtService jwtService,
                                            ITokenService tokenService,
                                            IStringLocalizer localizer,
                                            ILogger<AdminOldDosImportReviewViewModel> logger,
                                            IRegionManager regionManager)
    {
        _service          = service;
        _companiesService = companiesService;
        _platformsService = platformsService;
        _apiClient        = apiClient;
        _jwtService       = jwtService;
        _tokenService     = tokenService;
        _localizer        = localizer;
        _logger           = logger;
        _regionManager    = regionManager;

        AcceptCommand = new AsyncRelayCommand(AcceptAsync);
        SkipCommand = new AsyncRelayCommand(SkipAsync);
        DiscardCommand = new AsyncRelayCommand(DiscardAsync);
        GoBackCommand = new RelayCommand(GoBack);
        CreateCompanyCommand = new AsyncRelayCommand(CreateCompanyAsync);
        ExitMergeModeCommand = new RelayCommand(ExitMergeMode);
        EnableAllVersionsCommand = new RelayCommand(() =>
        {
            foreach(OldDosVersionDecisionItem item in VersionDecisions) item.Include = true;
        });
        DisableAllVersionsCommand = new RelayCommand(() =>
        {
            foreach(OldDosVersionDecisionItem item in VersionDecisions) item.Include = false;
        });

        BuildKindOptions();
        BuildReleasePrecisionOptions();
        CheckAdminRole();
    }

    public IAsyncRelayCommand AcceptCommand { get; }
    public IAsyncRelayCommand SkipCommand { get; }
    public IAsyncRelayCommand DiscardCommand { get; }
    public IRelayCommand GoBackCommand { get; }
    public IAsyncRelayCommand CreateCompanyCommand { get; }
    public IRelayCommand ExitMergeModeCommand { get; }
    public IRelayCommand EnableAllVersionsCommand { get; }
    public IRelayCommand DisableAllVersionsCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();
        if(!IsAdmin) return;

        if(navigationContext.Parameters.TryGetValue<long>(NavParamKeys.OldDosImportId, out long importId))
            _currentImportId = importId;
        else if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.OldDosImportId, out int importIdInt))
            _currentImportId = importIdInt;

        await EnsureLookupsLoadedAsync();

        if(_currentImportId > 0)
            await LoadDetailAsync(_currentImportId);
    }

    public string KindLabel(int? kind) => kind.HasValue ? ((SoftwareKind)kind.Value).Humanize() : "—";

    public string AbsoluteOldDosUrl(string? url)
    {
        if(string.IsNullOrWhiteSpace(url)) return string.Empty;
        if(url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return url;

        return "https://old-dos.ru" + (url.StartsWith('/') ? url : "/" + url);
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
        {
            source = source.Where(company =>
                                      (company.Name ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                      JaroWinkler.Similarity(query, company.Name ?? string.Empty) >= 0.6);
        }

        foreach(CompanyDto company in source.OrderBy(c => c.Name).Take(30))
            CompanySuggestions.Add(company);
    }

    public void AddSelectedGenre()
    {
        if(SelectedGenreToAdd?.Id is not int id || SelectedGenres.Any(g => g.Id == id)) return;

        SelectedGenres.Add(SelectedGenreToAdd);
        SelectedGenreToAdd = null;
        GenreSearchText    = string.Empty;
        UpdateGenreSuggestions(string.Empty);
    }

    public void RemoveGenre(SoftwareGenreDto? genre)
    {
        if(genre is null) return;
        SelectedGenres.Remove(genre);
        UpdateGenreSuggestions(GenreSearchText);
    }

    public void OpenCreateCompany()
    {
        ShowCreateCompany = true;
        string? seed = Detail?.DeveloperName;
        if(string.IsNullOrWhiteSpace(seed)) seed = Detail?.Name;
        NewCompanyName = seed ?? string.Empty;
    }

    public async Task EnterMergeModeAsync(OldDosNameMatchCandidateItem? item)
    {
        if(item?.SoftwareId <= 0) return;

        MergeTargetId   = item.SoftwareId;
        MergeTargetName = item.Name;
    }

    public async Task AcceptAsync()
    {
        if(Detail?.Id is not long id) return;
        if(SelectedDeveloper?.Id is not int developerCompanyId)
        {
            ErrorMessage = _localizer["OldDosDeveloperRequired"];
            HasError     = true;

            return;
        }

        try
        {
            IsBusy       = true;
            HasError     = false;
            ErrorMessage = string.Empty;

            var dto = new AcceptOldDosImportDto
            {
                Mode                    = MergeTargetId.HasValue ? (int)OldDosAcceptMode.MergeIntoExisting : (int)OldDosAcceptMode.CreateNew,
                TargetSoftwareId        = MergeTargetId,
                NameOverride            = NameOverride,
                KindOverride            = (byte?)SelectedKindOption?.Value,
                MuseumDescriptionEdited = MuseumDescription,
                DeveloperCompanyId      = developerCompanyId,
                GenreIds                = SelectedGenres.Select(genre => genre.Id).Where(idValue => idValue.HasValue).ToList(),
                Versions = VersionDecisions.Select(version => new AcceptOldDosVersionDecisionDto
                {
                    OldDosVersionId              = version.Id,
                    Include                      = version.Include,
                    VersionStringOverride        = version.VersionStringOverride,
                    ReleaseDateOverride          = version.ReleaseDateOverride,
                    ReleaseDatePrecisionOverride = (byte?)version.ReleasePrecisionOption?.Value,
                    SoftwarePlatformIdOverride   = version.Platform?.Id
                }).ToList()
            };

            (AcceptOldDosImportResultDto? result, string? error) = await _service.AcceptAsync(id, dto);

            if(error != null || result?.Success != true)
            {
                HasError     = true;
                ErrorMessage = error ?? result?.Error ?? _localizer["FailedToAcceptOldDosImport"];

                return;
            }

            string message = string.Format(_localizer["AcceptedOldDosImportMessage"].Value,
                                           result.PromotedSoftwareId ?? 0,
                                           result.InsertedVersionCount ?? 0,
                                           result.InsertedReleaseCount ?? 0,
                                           result.InsertedDescriptionCount ?? 0,
                                           result.InsertedGenreCount ?? 0);
            SetStatus(true, message);
            await AdvanceToNextAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error accepting old-dos import {ImportId}", id);
            HasError     = true;
            ErrorMessage = _localizer["FailedToAcceptOldDosImport"];
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SkipAsync()
    {
        if(Detail?.Id is not long id) return;

        IsBusy = true;

        try
        {
            bool ok = await _service.SkipAsync(id);

            if(ok) SetStatus(true, _localizer["SkippedOldDosImportMessage"]);
            else
            {
                HasError     = true;
                ErrorMessage = _localizer["FailedToSkipOldDosImport"];
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
        if(Detail?.Id is not long id) return;

        IsBusy = true;

        try
        {
            bool ok = await _service.DiscardAsync(id);

            if(ok) SetStatus(true, _localizer["DiscardedOldDosImportMessage"]);
            else
            {
                HasError     = true;
                ErrorMessage = _localizer["FailedToDiscardOldDosImport"];
            }

            await AdvanceToNextAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task CreateCompanyAsync()
    {
        if(string.IsNullOrWhiteSpace(NewCompanyName)) return;

        IsCreatingCompany = true;

        try
        {
            var dto = new CompanyDto
            {
                Name   = NewCompanyName.Trim(),
                Status = (int)CompanyStatus.Unknown
            };

            int? id = await _companiesService.CreateAsync(dto);

            if(!id.HasValue)
            {
                HasError     = true;
                ErrorMessage = _localizer["FailedToCreateOldDosCompany"];

                return;
            }

            dto.Id = id;
            _allCompanies.Add(dto);
            SelectedDeveloper = dto;
            ShowCreateCompany = false;
            NewCompanyName    = string.Empty;
            UpdateCompanySuggestions(CompanySearchText);
            SetStatus(true, string.Format(_localizer["CreatedOldDosCompanyMessage"].Value, id.Value, dto.Name));
        }
        finally
        {
            IsCreatingCompany = false;
        }
    }

    public void GoBack()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminOldDosImportsPage));
    }

    public void ExitMergeMode()
    {
        MergeTargetId   = null;
        MergeTargetName = string.Empty;
    }

    partial void OnGenreSearchTextChanged(string value) => UpdateGenreSuggestions(value);
    partial void OnCompanySearchTextChanged(string value) => UpdateCompanySuggestions(value);
    partial void OnNameOverrideChanged(string value) => _ = RefreshMatchesAsync();

    private async Task EnsureLookupsLoadedAsync()
    {
        if(_allPlatforms.Count > 0 && _allCompanies.Count > 0 && _allGenres.Count > 0) return;

        _allPlatforms = await _platformsService.GetAllAsync();
        _allCompanies = await _companiesService.GetAllCompaniesAsync();

        List<SoftwareGenreDto>? genres = await _apiClient.Software.Genres.GetAsync(config =>
        {
            config.QueryParameters.IncludeUnused = true;
        });

        _allGenres = genres?.Where(genre => genre.Type == (int)SoftwareGenreType.Category)
                           .OrderBy(genre => genre.Name)
                           .ToList() ?? [];

        PlatformSuggestions.Clear();
        foreach(SoftwarePlatformDto platform in _allPlatforms.OrderBy(platform => platform.Name))
            PlatformSuggestions.Add(platform);

        UpdateGenreSuggestions(string.Empty);
        UpdateCompanySuggestions(string.Empty);
    }

    private async Task LoadDetailAsync(long id)
    {
        try
        {
            IsLoading     = true;
            HasError      = false;
            ErrorMessage  = string.Empty;
            ShowCreateCompany = false;
            NewCompanyName = string.Empty;
            ExitMergeMode();

            OldDosPendingDetailDto? detail = await _service.GetByIdAsync(id);

            if(detail is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["FailedToLoadOldDosImportDetail"];

                return;
            }

            _currentImportId = id;
            Detail = detail;
            MuseumDescription = detail.EnglishDescriptionMuseum ?? string.Empty;
            NameOverride = detail.Name ?? string.Empty;

            int kindValue = (int)SoftwareKind.Game;
            SelectedKindOption = KindOptions.FirstOrDefault(option => option.Value == kindValue);

            SelectedGenres.Clear();

            foreach(int genreId in (detail.SuggestedGenreIds ?? []).Where(item => item.HasValue).Select(item => item!.Value))
            {
                SoftwareGenreDto? genre = _allGenres.FirstOrDefault(g => g.Id == genreId);
                if(genre is not null)
                    SelectedGenres.Add(genre);
            }

            SelectedDeveloper = null;
            if(!string.IsNullOrWhiteSpace(detail.DeveloperName))
            {
                SelectedDeveloper = _allCompanies
                                   .Select(company => new
                                   {
                                       Company = company,
                                       Score = JaroWinkler.Similarity(detail.DeveloperName, company.Name ?? string.Empty)
                                   })
                                   .Where(item => item.Score >= 0.92)
                                   .OrderByDescending(item => item.Score)
                                   .Select(item => item.Company)
                                   .FirstOrDefault();
            }

            VersionDecisions.Clear();
            foreach(OldDosVersionDto version in (detail.Versions ?? [])
                                               .OrderBy(v => v.VersionString, NaturalStringComparer.Instance))
            {
                VersionDecisions.Add(new OldDosVersionDecisionItem
                {
                    Id = version.Id ?? 0,
                    Include = version.IsEnabledByDefault ?? true,
                    OriginalVersionString = version.VersionString ?? string.Empty,
                    VersionStringOverride = version.VersionString ?? string.Empty,
                    ReleaseDateOverride = version.ReleaseDate,
                    ReleaseDatePrecision = version.ReleaseDatePrecision ?? (int)DatePrecision.Full,
                    ReleasePrecisionOption = ReleasePrecisionOptions.FirstOrDefault(option =>
                        option.Value == (version.ReleaseDatePrecision ?? (int)DatePrecision.Full)),
                    OsHint = version.OsHint ?? string.Empty,
                    DownloadUrl = version.DownloadUrl ?? string.Empty,
                    FileName = version.FileName ?? string.Empty,
                    Platform = ResolvePlatformByOsHint(version.OsHint)
                });
            }

            await RefreshMatchesAsync();
            UpdateGenreSuggestions(string.Empty);
            UpdateCompanySuggestions(string.Empty);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading old-dos detail {ImportId}", id);
            HasError     = true;
            ErrorMessage = _localizer["FailedToLoadOldDosImportDetail"];
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshMatchesAsync()
    {
        if(Detail?.Id is not long id) return;

        OldDosNameMatchCandidatesDto matches = await _service.GetNameMatchesAsync(id, NameOverride);

        NameMatches.Clear();
        HasExactMatch = false;

        foreach(OldDosNameMatchCandidateDto match in matches.Matches ?? [])
        {
            if(match.MatchKind == (int)OldDosNameMatchKind.ExactNormalized)
                HasExactMatch = true;

            NameMatches.Add(new OldDosNameMatchCandidateItem
            {
                Candidate = match,
                KindLabel = KindLabel(match.Kind)
            });
        }
    }

    private SoftwarePlatformDto? ResolvePlatformByOsHint(string? osHint)
    {
        if(string.IsNullOrWhiteSpace(osHint)) return null;
        string hint = osHint.Trim();

        SoftwarePlatformDto? exact = _allPlatforms.FirstOrDefault(platform =>
            !string.IsNullOrEmpty(platform.Name) &&
            string.Equals(platform.Name, hint, StringComparison.OrdinalIgnoreCase));

        if(exact is not null) return exact;

        const double MinScore = 0.6;

        return _allPlatforms
              .Where(platform => !string.IsNullOrEmpty(platform.Name) &&
                                 (platform.Name.Contains(hint, StringComparison.OrdinalIgnoreCase) ||
                                  hint.Contains(platform.Name, StringComparison.OrdinalIgnoreCase) ||
                                  JaroWinkler.Similarity(hint, platform.Name) >= MinScore))
              .Select(platform => new
              {
                  Platform = platform,
                  Score = JaroWinkler.Similarity(hint, platform.Name ?? string.Empty)
              })
              .OrderByDescending(item => item.Score)
              .ThenBy(item => item.Platform.Name!.Length)
              .ThenBy(item => item.Platform.Name, StringComparer.OrdinalIgnoreCase)
              .Select(item => item.Platform)
              .FirstOrDefault();
    }

    private async Task AdvanceToNextAsync()
    {
        OldDosPendingDetailDto? next = await _service.GetNextAsync(_currentImportId);

        if(next?.Id is not long nextId)
        {
            GoBack();

            return;
        }

        await LoadDetailAsync(nextId);
    }

    private void BuildKindOptions()
    {
        foreach(SoftwareKind kind in Enum.GetValues<SoftwareKind>())
        {
            KindOptions.Add(new OldDosKindOption
            {
                Label = kind.Humanize(),
                Value = (int)kind
            });
        }
    }

    private void BuildReleasePrecisionOptions()
    {
        ReleasePrecisionOptions =
        [
            new OldDosEnumOption { Label = _localizer["OldDosReleasePrecisionDefault"], Value = null },
            new OldDosEnumOption { Label = _localizer["OldDosReleasePrecisionDay"], Value = (int)DatePrecision.Full },
            new OldDosEnumOption { Label = _localizer["OldDosReleasePrecisionMonth"], Value = (int)DatePrecision.MonthYear },
            new OldDosEnumOption { Label = _localizer["OldDosReleasePrecisionYear"], Value = (int)DatePrecision.YearOnly }
        ];
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

            if(!_jwtService.IsTokenValid(token)) { IsAdmin = false; return; }
            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
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
}
