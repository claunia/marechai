#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.Data;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class SoftwareViewViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareBrowsingService        _browsingService;
    private readonly SoftwareScreenshotCache         _screenshotCache;
    private readonly ImageSourceFactory              _imageSourceFactory;
    private readonly IStringLocalizer               _localizer;
    private readonly ILogger<SoftwareViewViewModel> _logger;
    private readonly IRegionManager                 _regionManager;

    private string? _navigationSource;
    private int     _currentSoftwareId;

    [ObservableProperty]
    private string _softwareName = string.Empty;

    [ObservableProperty]
    private string? _family;

    [ObservableProperty]
    private string? _predecessor;

    [ObservableProperty]
    private int? _predecessorId;

    [ObservableProperty]
    private string? _successor;

    [ObservableProperty]
    private int? _successorId;

    [ObservableProperty]
    private SoftwareKind _kind;

    [ObservableProperty]
    private string? _baseSoftware;

    [ObservableProperty]
    private int? _baseSoftwareId;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private Visibility _showFamily = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPredecessor = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSuccessor = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCompanies = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showVersions = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showOsBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showGameBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSoftwareBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showDlcBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSystemSoftwareBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showApplicationBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showDevelopmentSoftwareBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showServerSoftwareBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showMiddlewareBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showFirmwareBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showEmbeddedSoftwareBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showBaseSoftware = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showScreenshots = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCredits = Visibility.Collapsed;

    [ObservableProperty]
    private string _descriptionHtml = string.Empty;

    [ObservableProperty]
    private Visibility _showDescription = Visibility.Collapsed;

    public bool HasDescription => !string.IsNullOrWhiteSpace(DescriptionHtml);

    partial void OnDescriptionHtmlChanged(string value)
    {
        OnPropertyChanged(nameof(HasDescription));
    }

    public SoftwareViewViewModel(ILogger<SoftwareViewViewModel> logger,          IRegionManager regionManager,
                                 SoftwareBrowsingService        browsingService, IStringLocalizer localizer,
                                 SoftwareScreenshotCache        screenshotCache, ImageSourceFactory imageSourceFactory)
    {
        _logger             = logger;
        _regionManager      = regionManager;
        _browsingService    = browsingService;
        _localizer          = localizer;
        _screenshotCache    = screenshotCache;
        _imageSourceFactory = imageSourceFactory;
    }

    public ObservableCollection<string>                   Companies           { get; } = [];
    public ObservableCollection<VersionDisplayItem>       Versions            { get; } = [];
    public ObservableCollection<ReleaseDisplayItem>       Releases            { get; } = [];
    public ObservableCollection<ScreenshotPlatformGroup>  ScreenshotGroups    { get; } = [];
    public ObservableCollection<CreditGroupDisplayItem>   CreditGroups        { get; } = [];
    public ObservableCollection<GenreTypeGroupItem>       GenreGroups         { get; } = [];
    public ObservableCollection<SpecPlatformGroupItem>    SpecGroups          { get; } = [];
    public ObservableCollection<RatingItem>               Ratings             { get; } = [];

    [ObservableProperty]
    private Visibility _showGenres = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSpecs = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showRatings = Visibility.Collapsed;

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.NavigationSource, out string? source))
            _navigationSource = source;

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareId, out int softwareId))
        {
            _currentSoftwareId = softwareId;
            _ = LoadSoftwareAsync(softwareId);
        }
    }

    [RelayCommand]
    public Task GoBack()
    {
        if(_navigationSource == nameof(SoftwareListViewModel))
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareListPage));
        else
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwarePage));

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToPredecessor()
    {
        if(PredecessorId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, PredecessorId.Value },
            { NavParamKeys.NavigationSource, nameof(SoftwareViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToSuccessor()
    {
        if(SuccessorId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, SuccessorId.Value },
            { NavParamKeys.NavigationSource, nameof(SoftwareViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToBaseSoftware()
    {
        if(BaseSoftwareId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, BaseSoftwareId.Value },
            { NavParamKeys.NavigationSource, nameof(SoftwareViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToRelease(ReleaseDisplayItem? release)
    {
        if(release is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareReleaseId, release.Id },
            { NavParamKeys.SoftwareId, _currentSoftwareId },
            { NavParamKeys.NavigationSource, nameof(SoftwareViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareReleaseViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task LoadData()
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        return Task.CompletedTask;
    }

    private async Task LoadSoftwareAsync(int softwareId)
    {
        try
        {
            IsLoading    = true;
            IsDataLoaded = false;
            HasError     = false;
            ErrorMessage = string.Empty;
            Companies.Clear();
            Versions.Clear();
            CreditGroups.Clear();

            SoftwareDto? software = await _browsingService.GetSoftwareByIdAsync(softwareId);

            if(software is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["Software not found"];
                IsLoading    = false;

                return;
            }

            SoftwareName      = software.Name ?? string.Empty;
            Family            = software.Family;
            PredecessorId     = software.PredecessorId;
            Predecessor       = software.Predecessor;
            SuccessorId       = software.SuccessorId;
            Successor         = software.Successor;
            Kind              = (SoftwareKind)(software.Kind ?? 0);
            BaseSoftwareId    = software.BaseSoftwareId;
            BaseSoftware      = software.BaseSoftware;

            // Load companies
            List<SoftwareCompanyRoleDto> companies = await _browsingService.GetCompaniesAsync(softwareId);

            foreach(SoftwareCompanyRoleDto company in companies)
            {
                string name    = company.Company ?? string.Empty;
                string? role   = company.Role;
                string localizedRole = !string.IsNullOrEmpty(role) ? _localizer[role] : null;
                string display = !string.IsNullOrEmpty(localizedRole) ? $"{name} ({localizedRole})" : name;
                Companies.Add(display);
            }

            // Load credits — Role is returned localized server-side via the
            // PeopleBySoftwareRoleTranslations table (worker fills missing rows on its hourly
            // sweep using OpenAI/NLLB; English fallback before then).
            List<PersonBySoftwareDto> credits = await _browsingService.GetCreditsAsync(softwareId,
                                                                                       GetIso639CodeFromCulture());

            var creditsByRole = credits
                               .GroupBy(c => c.Role ?? "Other")
                               .OrderBy(g => g.Key);

            foreach(var group in creditsByRole)
            {
                var item = new CreditGroupDisplayItem { Role = group.Key };

                foreach(PersonBySoftwareDto credit in group)
                {
                    string fullName = credit.DisplayName ?? credit.Alias ?? $"{credit.Name} {credit.Surname}".Trim();
                    item.People.Add(fullName);
                }

                CreditGroups.Add(item);
            }

            // Load genres (translated server-side via the SoftwareGenreTranslations table; the
            // worker fills in missing rows on its hourly sweep using OpenAI/NLLB).
            GenreGroups.Clear();
            List<SoftwareGenreDto> genres = await _browsingService.GetGenresAsync(softwareId,
                                                                                  GetIso639CodeFromCulture());

            var genresByType = genres
                              .GroupBy(g => g.TypeName ?? "Genre")
                              .OrderBy(g => g.Key);

            foreach(var group in genresByType)
            {
                var genreGroup = new GenreTypeGroupItem { TypeName = _localizer[group.Key] };

                foreach(SoftwareGenreDto genre in group.OrderBy(g => g.Name))
                    genreGroup.Genres.Add(genre.Name ?? string.Empty);

                GenreGroups.Add(genreGroup);
            }

            ShowGenres = GenreGroups.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            // Load attributes (specs + ratings)
            SpecGroups.Clear();
            Ratings.Clear();
            List<SoftwareAttributeDto> attributes = await _browsingService.GetAttributesAsync(softwareId);

            var specs = attributes
                       .Where(a => a.Category == "Spec")
                       .DistinctBy(a => (a.PlatformName, a.Key, a.Value))
                       .GroupBy(s => s.PlatformName ?? "Unknown")
                       .OrderBy(g => g.Key);

            foreach(var platformGroup in specs)
            {
                var specGroup = new SpecPlatformGroupItem { PlatformName = platformGroup.Key };

                foreach(SoftwareAttributeDto spec in platformGroup)
                {
                    // Server already pre-translates non-Rating attribute keys/values via the
                    // SoftwareAttributeTranslationCache (see /software/{id}/attributes?lang=...).
                    // Use the values verbatim — DO NOT re-localize.
                    specGroup.Specs.Add(new SpecItem
                    {
                        Key   = spec.Key   ?? string.Empty,
                        Value = spec.Value ?? string.Empty
                    });
                }

                SpecGroups.Add(specGroup);
            }

            ShowSpecs = SpecGroups.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            foreach(SoftwareAttributeDto rating in attributes.Where(a => a.Category == "Rating")
                                                                      .DistinctBy(a => (a.PlatformName, a.Key,
                                                                                         a.Value)))
            {
                Ratings.Add(new RatingItem
                {
                    System       = rating.Key ?? string.Empty,
                    Rating       = rating.Value ?? string.Empty,
                    PlatformName = rating.PlatformName ?? string.Empty
                });
            }

            ShowRatings = Ratings.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            // Load versions (without nested releases)
            List<SoftwareVersionDto> versions = await _browsingService.GetVersionsAsync(softwareId);
            versions.Sort((a, b) => NaturalStringComparer.Instance.Compare(a.VersionString, b.VersionString));

            foreach(SoftwareVersionDto version in versions)
            {
                int versionId = (int)(version.Id ?? 0);

                var versionItem = new VersionDisplayItem
                {
                    Id            = versionId,
                    VersionString = version.VersionString ?? string.Empty,
                    PublicVersion = version.PublicVersion,
                    Codename      = version.Codename
                };

                Versions.Add(versionItem);
            }

            // Load all non-compilation releases for this software (flat list)
            List<SoftwareReleaseDto> releases = await _browsingService.GetReleasesBySoftwareAsync(softwareId);

            foreach(SoftwareReleaseDto release in releases)
            {
                string dateDisplay = release.ReleaseDate.HasValue ? ((release.ReleaseDatePrecision ?? 0) == 2 ? $"{release.ReleaseDate.Value.Year}" : (release.ReleaseDatePrecision ?? 0) == 1 ? release.ReleaseDate.Value.ToString("MMMM yyyy") : release.ReleaseDate.Value.DateTime.ToString("MMMM d, yyyy")) : string.Empty;

                var releaseItem = new ReleaseDisplayItem
                {
                    Id              = (int)(release.Id ?? 0),
                    SoftwareVersion = release.SoftwareVersion,
                    Platform        = release.Platform,
                    Regions         = release.Regions is { Count: > 0 }
                        ? string.Join(", ", release.Regions.Select(r => r.RegionName))
                        : null,
                    Publisher       = release.Publisher,
                    ReleaseDate     = dateDisplay
                };

                Releases.Add(releaseItem);
            }

            // Load screenshots
            await LoadScreenshotsAsync(softwareId);

            // Load localized description
            try
            {
                string langCode = GetIso639CodeFromCulture();
                SoftwareDescriptionDto? desc = await _browsingService.GetDescriptionAsync(softwareId, langCode);
                DescriptionHtml = desc?.Html ?? desc?.Markdown ?? string.Empty;
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load software description: {Exception}", ex.Message);
            }

            UpdateVisibilities();
            IsDataLoaded = true;
            IsLoading    = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software {SoftwareId}", softwareId);
            HasError     = true;
            ErrorMessage = ex.Message;
            IsLoading    = false;
        }
    }

    [RelayCommand]
    public Task ViewScreenshot(ScreenshotDisplayItem? item)
    {
        if(item is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.ScreenshotId, item.Id }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(ScreenshotDetailPage), parameters);

        return Task.CompletedTask;
    }

    private async Task LoadScreenshotsAsync(int softwareId)
    {
        try
        {
            ScreenshotGroups.Clear();

            List<Guid> screenshotIds = await _browsingService.GetScreenshotIdsAsync(softwareId);

            if(screenshotIds.Count == 0) return;

            // Fetch metadata for each screenshot and group by platform
            var byPlatform = new Dictionary<string, List<ScreenshotDisplayItem>>();

            foreach(Guid id in screenshotIds)
            {
                SoftwareScreenshotDto? dto = await _browsingService.GetScreenshotDetailsAsync(id);

                if(dto is null) continue;

                string platformKey = dto.PlatformName ?? _localizer["General"];

                if(!byPlatform.ContainsKey(platformKey))
                    byPlatform[platformKey] = [];

                var item = new ScreenshotDisplayItem
                {
                    Id           = dto.Id ?? Guid.Empty,
                    Caption      = dto.Caption,
                    PlatformName = dto.PlatformName
                };

                byPlatform[platformKey].Add(item);
                _ = LoadScreenshotThumbnailAsync(item);
            }

            foreach(KeyValuePair<string, List<ScreenshotDisplayItem>> kvp in byPlatform.OrderBy(k => k.Key))
            {
                var group = new ScreenshotPlatformGroup
                {
                    PlatformName = kvp.Key,
                    Screenshots  = new ObservableCollection<ScreenshotDisplayItem>(kvp.Value)
                };

                ScreenshotGroups.Add(group);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading screenshots for software {SoftwareId}", softwareId);
        }
    }

    private async Task LoadScreenshotThumbnailAsync(ScreenshotDisplayItem item)
    {
        try
        {
            Stream stream = await _screenshotCache.GetThumbnailAsync(item.Id);
            item.ThumbnailSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading screenshot thumbnail {Id}", item.Id);
        }
    }

    private void UpdateVisibilities()
    {
        ShowFamily      = !string.IsNullOrEmpty(Family) ? Visibility.Visible : Visibility.Collapsed;
        ShowPredecessor  = PredecessorId is not null && !string.IsNullOrEmpty(Predecessor) ? Visibility.Visible : Visibility.Collapsed;
        ShowSuccessor    = SuccessorId is not null && !string.IsNullOrEmpty(Successor) ? Visibility.Visible : Visibility.Collapsed;
        ShowCompanies   = Companies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowVersions    = Versions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowOsBadge       = Kind == SoftwareKind.OperatingSystem ? Visibility.Visible : Visibility.Collapsed;
        ShowGameBadge     = Kind == SoftwareKind.Game ? Visibility.Visible : Visibility.Collapsed;
        ShowSoftwareBadge = Kind == SoftwareKind.Software ? Visibility.Visible : Visibility.Collapsed;
        ShowDlcBadge      = Kind == SoftwareKind.Dlc ? Visibility.Visible : Visibility.Collapsed;
        ShowSystemSoftwareBadge      = Kind == SoftwareKind.SystemSoftware ? Visibility.Visible : Visibility.Collapsed;
        ShowApplicationBadge         = Kind == SoftwareKind.Application ? Visibility.Visible : Visibility.Collapsed;
        ShowDevelopmentSoftwareBadge = Kind == SoftwareKind.DevelopmentSoftware ? Visibility.Visible : Visibility.Collapsed;
        ShowServerSoftwareBadge      = Kind == SoftwareKind.ServerSoftware ? Visibility.Visible : Visibility.Collapsed;
        ShowMiddlewareBadge          = Kind == SoftwareKind.Middleware ? Visibility.Visible : Visibility.Collapsed;
        ShowFirmwareBadge            = Kind == SoftwareKind.Firmware ? Visibility.Visible : Visibility.Collapsed;
        ShowEmbeddedSoftwareBadge    = Kind == SoftwareKind.EmbeddedSoftware ? Visibility.Visible : Visibility.Collapsed;
        ShowBaseSoftware  = BaseSoftwareId is not null && !string.IsNullOrEmpty(BaseSoftware) ? Visibility.Visible : Visibility.Collapsed;
        ShowScreenshots = ScreenshotGroups.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowCredits     = CreditGroups.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowDescription = HasDescription ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string GetIso639CodeFromCulture()
    {
        string twoLetter = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        return twoLetter switch
        {
            "en" => "eng",
            "es" => "spa",
            "de" => "deu",
            "fr" => "fra",
            "la" => "lat",
            "pt" => "por",
            _    => "eng"
        };
    }
}

[Bindable]
public class ScreenshotPlatformGroup
{
    public string                                           PlatformName { get; set; } = string.Empty;
    public ObservableCollection<ScreenshotDisplayItem> Screenshots  { get; set; } = [];
}

[Bindable]
public class VersionDisplayItem
{
    public int                                     Id            { get; set; }
    public string                                  VersionString { get; set; } = string.Empty;
    public string?                                 PublicVersion { get; set; }
    public string?                                 Codename      { get; set; }
    public ObservableCollection<ReleaseDisplayItem> Releases     { get; } = [];
}

[Bindable]
public class ReleaseDisplayItem
{
    public int     Id              { get; set; }
    public string? SoftwareVersion { get; set; }
    public string? Platform        { get; set; }
    public string? Regions          { get; set; }
    public string? Publisher        { get; set; }
    public string? ReleaseDate     { get; set; }
}

[Bindable]
public class CreditGroupDisplayItem
{
    public string                      Role   { get; set; } = string.Empty;
    public ObservableCollection<string> People { get; } = [];
}

[Bindable]
public class GenreTypeGroupItem
{
    public string                      TypeName { get; set; } = string.Empty;
    public ObservableCollection<string> Genres  { get; } = [];
}

[Bindable]
public class SpecPlatformGroupItem
{
    public string                          PlatformName { get; set; } = string.Empty;
    public ObservableCollection<SpecItem>  Specs        { get; } = [];
}

[Bindable]
public class SpecItem
{
    public string Key   { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

[Bindable]
public class RatingItem
{
    public string System       { get; set; } = string.Empty;
    public string Rating       { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
}
