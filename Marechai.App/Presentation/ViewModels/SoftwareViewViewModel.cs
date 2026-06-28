#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.ApiClient.Models;
using Marechai.App.Models;
using Marechai.App.Presentation.Models;
using Marechai.App.Navigation;
using Marechai.Data;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Windows.System;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class SoftwareViewViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareBrowsingService         _browsingService;
    private readonly AuthService                     _authService;
    private readonly ITokenService                   _tokenService;
    private readonly IJwtService                     _jwtService;
    private readonly SoftwareScreenshotCache         _screenshotCache;
    private readonly SoftwareCoverCache              _coverCache;
    private readonly SoftwarePromoArtCache           _promoArtCache;
    private readonly SoftwarePromoArtService         _promoArtService;
    private readonly ImageSourceFactory              _imageSourceFactory;
    private readonly IStringLocalizer               _localizer;
    private readonly ILogger<SoftwareViewViewModel> _logger;
    private readonly IRegionManager                 _regionManager;

    private string? _navigationSource;
    private int     _currentSoftwareId;
    private string? _currentUserId;
    private SoftwareUserReviewDto? _myReview;

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
    private Visibility _showCovers = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPromoArt = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showVideos = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCriticReviews = Visibility.Collapsed;

    [ObservableProperty]
    private string? _criticReviewsOverallText;

    [ObservableProperty]
    private Visibility _showUserReviews = Visibility.Collapsed;

    [ObservableProperty]
    private string? _userReviewsOverallText;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private float _currentUserRating;

    [ObservableProperty]
    private string? _reviewFeedbackMessage;

    [ObservableProperty]
    private InfoBarSeverity _reviewFeedbackSeverity = InfoBarSeverity.Informational;

    [ObservableProperty]
    private string _reviewDraftTheGood = string.Empty;

    [ObservableProperty]
    private string _reviewDraftTheBad = string.Empty;

    [ObservableProperty]
    private string _reviewDraftTheUgly = string.Empty;

    [ObservableProperty]
    private bool _reviewDraftIsAnonymous;

    [ObservableProperty]
    private double _reviewDraftRating;

    [ObservableProperty]
    private bool _isSubmittingReviewDraft;

    [ObservableProperty]
    private string? _reviewDraftErrorMessage;

    [ObservableProperty]
    private ReviewReportReason _reviewReportReason = ReviewReportReason.Spam;

    [ObservableProperty]
    private string _reviewReportExplanation = string.Empty;

    [ObservableProperty]
    private bool _isSubmittingReviewReport;

    [ObservableProperty]
    private string? _reviewReportDraftErrorMessage;

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
                                 SoftwareBrowsingService         browsingService, AuthService authService,
                                 ITokenService                   tokenService,    IJwtService jwtService,
                                 IStringLocalizer                localizer,       SoftwareScreenshotCache screenshotCache,
                                 SoftwareCoverCache              coverCache,      SoftwarePromoArtCache promoArtCache,
                                 SoftwarePromoArtService         promoArtService,
                                 ImageSourceFactory              imageSourceFactory)
    {
        _logger             = logger;
        _regionManager      = regionManager;
        _browsingService    = browsingService;
        _authService        = authService;
        _tokenService       = tokenService;
        _jwtService         = jwtService;
        _localizer          = localizer;
        _screenshotCache    = screenshotCache;
        _coverCache         = coverCache;
        _promoArtCache      = promoArtCache;
        _promoArtService    = promoArtService;
        _imageSourceFactory = imageSourceFactory;

        ReviewReportReasons.Add(new ReviewReportReasonOption
        {
            Value = ReviewReportReason.Spam,
            Label = _localizer["Spam"]
        });
        ReviewReportReasons.Add(new ReviewReportReasonOption
        {
            Value = ReviewReportReason.Offensive,
            Label = _localizer["Offensive"]
        });
        ReviewReportReasons.Add(new ReviewReportReasonOption
        {
            Value = ReviewReportReason.Misleading,
            Label = _localizer["Misleading"]
        });
        ReviewReportReasons.Add(new ReviewReportReasonOption
        {
            Value = ReviewReportReason.OffTopic,
            Label = _localizer["OffTopic"]
        });
        ReviewReportReasons.Add(new ReviewReportReasonOption
        {
            Value = ReviewReportReason.Other,
            Label = _localizer["Other"]
        });
    }

    public ObservableCollection<string>                   Companies           { get; } = [];
    public ObservableCollection<VersionDisplayItem>       Versions            { get; } = [];
    public ObservableCollection<ReleaseDisplayItem>       Releases            { get; } = [];
    public ObservableCollection<ScreenshotPlatformGroup>  ScreenshotGroups    { get; } = [];
    public ObservableCollection<CoverGroupItem>           CoverGroups         { get; } = [];
    public ObservableCollection<SoftwarePromoArtGroupDisplayItem> PromoArtGroups { get; } = [];
    public ObservableCollection<MachineVideoDisplayItem>  Videos              { get; } = [];
    public ObservableCollection<CriticReviewDisplayItem>  CriticReviews       { get; } = [];
    public ObservableCollection<string>                   CriticReviewsByPlatform { get; } = [];
    public ObservableCollection<UserReviewDisplayItem>    UserReviews         { get; } = [];
    public ObservableCollection<ReviewRatingStarItem>     ReviewRatingStars   { get; } = [];

    public int  PromoArtCount => PromoArtGroups.Sum(group => group.Items.Count);
    public bool HasPromoArt   => PromoArtCount > 0;
    public ObservableCollection<CreditGroupDisplayItem>   CreditGroups        { get; } = [];
    public ObservableCollection<GenreTypeGroupItem>       GenreGroups         { get; } = [];
    public ObservableCollection<SpecPlatformGroupItem>    SpecGroups          { get; } = [];
    public ObservableCollection<RatingItem>               Ratings             { get; } = [];
    public ObservableCollection<SoftwareSimilarToDto>     SimilarSoftware     { get; } = [];

    [ObservableProperty]
    private Visibility _showSimilarSoftware = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showGenres = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSpecs = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showRatings = Visibility.Collapsed;

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public bool HasReviewFeedbackMessage => !string.IsNullOrWhiteSpace(ReviewFeedbackMessage);
    public bool HasReviewDraftError      => !string.IsNullOrWhiteSpace(ReviewDraftErrorMessage);
    public bool HasReviewReportDraftError => !string.IsNullOrWhiteSpace(ReviewReportDraftErrorMessage);
    public bool HasCurrentUserReview     => _myReview is not null;
    public string ReviewActionLabel      => _myReview is not null ? _localizer["EditReviewButton"] : _localizer["WriteReviewButton"];
    public string ReviewDialogTitle      => _myReview is not null ? _localizer["EditReviewDialogTitle"] : _localizer["WriteReviewDialogTitle"];
    public string ReviewSaveButtonText   => _localizer["SaveButton"];
    public string ReviewCancelButtonText => _localizer["CancelButton"];
    public string ReviewDraftRatingText  => ReviewDraftRating > 0 ? $"{ReviewDraftRating:F1}/5" : _localizer["ReviewDraftNoRating"];
    public bool CanSubmitReviewDraft     => !IsSubmittingReviewDraft;
    public string ReportReviewDialogTitle => _localizer["ReportReviewDialogTitle"];
    public string ReportReviewSubmitButtonText => _localizer["SubmitReportButton"];
    public bool CanSubmitReviewReportDraft => !IsSubmittingReviewReport;
    public ObservableCollection<ReviewReportReasonOption> ReviewReportReasons { get; } = [];

    private UserReviewDisplayItem? _selectedReviewForReport;

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
    public Task NavigateToSimilarSoftware(SoftwareSimilarToDto? similar)
    {
        if(similar?.SimilarSoftwareId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, similar.SimilarSoftwareId.Value },
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
            SuccessorId       = software.Successors?.FirstOrDefault()?.Id;
            Successor         = software.Successors?.FirstOrDefault()?.Name;
            Kind              = (SoftwareKind)(software.Kind ?? 0);
            BaseSoftwareId    = software.BaseSoftwareId;
            BaseSoftware      = software.BaseSoftware;

            // Load similar software.
            SimilarSoftware.Clear();
            List<SoftwareSimilarToDto> similar = await _browsingService.GetSimilarSoftwareAsync(softwareId);

            foreach(SoftwareSimilarToDto s in similar) SimilarSoftware.Add(s);

            ShowSimilarSoftware = SimilarSoftware.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

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

            // Load covers
            await LoadCoversAsync(softwareId);

            // Load promo art
            await LoadPromoArtAsync(softwareId);

            // Load screenshots
            await LoadScreenshotsAsync(softwareId);

            // Load videos
            await LoadVideosAsync(softwareId);

            // Load critic reviews
            await LoadCriticReviewsAsync(softwareId);

            // Load user reviews
            await RefreshReviewVotingContextAsync();
            await RefreshCurrentUserRatingAsync(softwareId);
            await LoadUserReviewsAsync(softwareId);

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

    [RelayCommand]
    public Task ViewCover(CoverDisplayItem? item)
    {
        if(item is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.CoverId, item.Id },
            { NavParamKeys.SoftwareName, SoftwareName }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(CoverDetailPage), parameters);

        return Task.CompletedTask;
    }

    private async Task LoadCoversAsync(int softwareId)
    {
        try
        {
            CoverGroups.Clear();

            List<SoftwareCoverDto> covers = await _browsingService.GetCoversAsync(softwareId);

            if(covers.Count == 0) return;

            var byGroup = new Dictionary<string, List<CoverDisplayItem>>();

            foreach(SoftwareCoverDto cover in covers)
            {
                if(cover.Id is null) continue;

                string groupKey = GetCoverGroupLabel(cover);

                if(!byGroup.ContainsKey(groupKey))
                    byGroup[groupKey] = [];

                var item = new CoverDisplayItem
                {
                    Id       = cover.Id.Value,
                    TypeName = cover.TypeName,
                    Caption  = cover.Caption
                };

                byGroup[groupKey].Add(item);
                _ = LoadCoverThumbnailAsync(item);
            }

            foreach(KeyValuePair<string, List<CoverDisplayItem>> kvp in byGroup.OrderBy(k => k.Key))
            {
                var group = new CoverGroupItem
                {
                    GroupLabel = kvp.Key,
                    Covers     = new ObservableCollection<CoverDisplayItem>(kvp.Value)
                };

                CoverGroups.Add(group);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading covers for software {SoftwareId}", softwareId);
        }
    }

    private async Task LoadCoverThumbnailAsync(CoverDisplayItem item)
    {
        try
        {
            Stream stream = await _coverCache.GetThumbnailAsync(item.Id);
            item.ThumbnailSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading cover thumbnail {Id}", item.Id);
        }
    }

    [RelayCommand]
    public Task ViewPromoArtDetails(Guid promoArtId)
    {
        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwarePromoArtId, promoArtId }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwarePromoArtDetailPage), parameters);

        return Task.CompletedTask;
    }

    private async Task LoadPromoArtAsync(int softwareId)
    {
        try
        {
            PromoArtGroups.Clear();

            List<SoftwarePromoArtDto> promoArtList = await _promoArtService.GetPromoArtBySoftwareAsync(softwareId);
            string                    otherGroup   = _localizer["OtherPromoArt"];

            foreach(var group in promoArtList.GroupBy(item => string.IsNullOrWhiteSpace(item.GroupName)
                                                                  ? otherGroup
                                                                  : item.GroupName!)
                                              .OrderBy(group => group.Key, StringComparer.CurrentCultureIgnoreCase))
            {
                var displayGroup = new SoftwarePromoArtGroupDisplayItem
                {
                    GroupName = group.Key
                };

                foreach(SoftwarePromoArtDto promoArt in group)
                {
                    if(!promoArt.Id.HasValue) continue;

                    var promoArtItem = new SoftwarePromoArtDisplayItem
                    {
                        PromoArtId = promoArt.Id.Value,
                        GroupName  = group.Key,
                        Caption    = promoArt.Caption ?? string.Empty
                    };

                    _ = LoadPromoArtThumbnailAsync(promoArtItem);

                    displayGroup.Items.Add(promoArtItem);
                }

                if(displayGroup.Items.Count > 0)
                    PromoArtGroups.Add(displayGroup);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading promo art for software {SoftwareId}", softwareId);
        }
    }

    private async Task LoadPromoArtThumbnailAsync(SoftwarePromoArtDisplayItem promoArtItem)
    {
        try
        {
            Stream stream = await _promoArtCache.GetThumbnailAsync(promoArtItem.PromoArtId);
            promoArtItem.ThumbnailImageSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading promo art thumbnail {PromoArtId}", promoArtItem.PromoArtId);
        }
    }

    private async Task LoadVideosAsync(int softwareId)
    {
        try
        {
            Videos.Clear();

            List<SoftwareVideoDto> videoList = await _browsingService.GetVideosBySoftwareAsync(softwareId);

            foreach(SoftwareVideoDto video in videoList)
            {
                if(string.IsNullOrWhiteSpace(video.VideoId)) continue;

                string provider = string.IsNullOrWhiteSpace(video.Provider)
                                      ? _localizer["Video"]
                                      : video.Provider;

                bool isYouTube = string.Equals(provider, "YouTube", StringComparison.OrdinalIgnoreCase);

                Videos.Add(new MachineVideoDisplayItem
                {
                    Title = string.IsNullOrWhiteSpace(video.Title)
                                ? _localizer["Video"]
                                : video.Title,
                    Provider = provider,
                    VideoId = video.VideoId,
                    ThumbnailUrl = isYouTube
                                       ? $"https://img.youtube.com/vi/{video.VideoId}/hqdefault.jpg"
                                       : null,
                    LaunchUri = new Uri($"https://www.youtube.com/watch?v={Uri.EscapeDataString(video.VideoId)}")
                });
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading videos for software {SoftwareId}", softwareId);
        }
    }

    [RelayCommand]
    public async Task OpenVideo(MachineVideoDisplayItem? video)
    {
        if(video?.LaunchUri is null) return;

        try
        {
            await Launcher.LaunchUriAsync(video.LaunchUri);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error launching video {VideoId}", video.VideoId);
        }
    }

    private async Task LoadCriticReviewsAsync(int softwareId)
    {
        try
        {
            CriticReviews.Clear();
            CriticReviewsByPlatform.Clear();
            CriticReviewsOverallText = null;

            List<SoftwareCriticReviewDto> reviewList = await _browsingService.GetCriticReviewsBySoftwareAsync(softwareId);

            foreach(SoftwareCriticReviewDto review in reviewList)
            {
                CriticReviews.Add(new CriticReviewDisplayItem
                {
                    MagazineTitle        = review.MagazineTitle ?? string.Empty,
                    PlatformName         = review.PlatformName,
                    NormalizedScore      = review.NormalizedScore,
                    OriginalScore        = review.OriginalScore,
                    OriginalScoreMaximum = review.OriginalScoreMaximum,
                    ReviewText           = review.ReviewText,
                    FormattedDate        = FormatReviewDate(review),
                    ReviewUrl            = review.ReviewUrl
                });
            }

            CriticReviewSummaryDto? summary = await _browsingService.GetCriticReviewSummaryBySoftwareAsync(softwareId);

            if(summary?.AverageScore is not null)
            {
                CriticReviewsOverallText = string.Format(_localizer["CriticReviewsOverallFormat"],
                                                           summary.AverageScore.Value,
                                                           summary.TotalReviews ?? 0);
            }

            if(summary?.ByPlatform is not null)
            {
                foreach(PlatformReviewSummaryDto plat in summary.ByPlatform)
                {
                    if(!plat.AverageScore.HasValue) continue;

                    CriticReviewsByPlatform.Add(
                        $"{plat.PlatformName ?? "?"}: {plat.AverageScore:F0}% ({plat.ReviewCount})");
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading critic reviews for software {SoftwareId}", softwareId);
        }
    }

    private async Task LoadUserReviewsAsync(int softwareId)
    {
        try
        {
            UserReviews.Clear();
            UserReviewsOverallText = null;

            List<SoftwareUserReviewDto> reviewList = await _browsingService.GetUserReviewsBySoftwareAsync(softwareId);

            foreach(SoftwareUserReviewDto review in reviewList)
            {
                UserReviews.Add(new UserReviewDisplayItem
                {
                    ReviewId        = review.Id ?? 0,
                    UserId          = review.UserId,
                    DisplayName     = review.DisplayName,
                    UserName        = review.UserName,
                    AvatarUrl       = review.AvatarUrl,
                    IsAnonymous     = review.IsAnonymous ?? false,
                    Rating          = review.Rating,
                    TheGood         = review.TheGood,
                    TheBad          = review.TheBad,
                    TheUgly         = review.TheUgly,
                    ThumbsUp        = review.ThumbsUp ?? 0,
                    ThumbsDown      = review.ThumbsDown ?? 0,
                    CurrentUserVote = review.CurrentUserVote,
                    CanVote         = IsAuthenticated &&
                                      !string.IsNullOrWhiteSpace(_currentUserId) &&
                                      !string.Equals(review.UserId, _currentUserId, StringComparison.Ordinal),
                    CanReport       = IsAuthenticated &&
                                      !string.IsNullOrWhiteSpace(_currentUserId) &&
                                      !string.Equals(review.UserId, _currentUserId, StringComparison.Ordinal),
                    FormattedDate   = review.CreatedOn?.DateTime.ToString("yyyy-MM-dd") ?? string.Empty
                });
            }

            _myReview = reviewList.FirstOrDefault(review => !string.IsNullOrWhiteSpace(_currentUserId) &&
                                                            string.Equals(review.UserId, _currentUserId,
                                                                          StringComparison.Ordinal));

            UserReviewSummaryDto? summary = await _browsingService.GetUserReviewSummaryBySoftwareAsync(softwareId);

            if(summary?.AverageRating is not null)
            {
                UserReviewsOverallText = string.Format(_localizer["UserReviewsOverallFormat"],
                                                         summary.AverageRating.Value,
                                                         summary.TotalReviews ?? 0);
            }

            OnPropertyChanged(nameof(HasCurrentUserReview));
            OnPropertyChanged(nameof(ReviewActionLabel));
            OnPropertyChanged(nameof(ReviewDialogTitle));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading user reviews for software {SoftwareId}", softwareId);
        }
    }

    async Task RefreshReviewVotingContextAsync()
    {
        try
        {
            IsAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);

            if(!IsAuthenticated)
            {
                _currentUserId = null;

                return;
            }

            string token = _tokenService.GetToken();
            _currentUserId = string.IsNullOrWhiteSpace(token) ? null : _jwtService.GetUserId(token);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error refreshing review voting context");
            IsAuthenticated = false;
            _currentUserId  = null;
        }
    }

    async Task RefreshCurrentUserRatingAsync(int softwareId)
    {
        if(!IsAuthenticated)
        {
            CurrentUserRating = 0;

            return;
        }

        try
        {
            SoftwareUserRatingDto? myRating = await _browsingService.GetMyUserRatingAsync(softwareId);
            CurrentUserRating = myRating?.Rating ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error refreshing current user rating for software {SoftwareId}", softwareId);
            CurrentUserRating = 0;
        }
    }

    public void PrepareReviewDraft()
    {
        ReviewDraftTheGood      = _myReview?.TheGood ?? string.Empty;
        ReviewDraftTheBad       = _myReview?.TheBad ?? string.Empty;
        ReviewDraftTheUgly      = _myReview?.TheUgly ?? string.Empty;
        ReviewDraftIsAnonymous  = _myReview?.IsAnonymous ?? false;
        ReviewDraftRating       = _myReview?.Rating ?? CurrentUserRating;
        ReviewDraftErrorMessage = null;
        RebuildReviewRatingStars();
    }

    public async Task<bool> SubmitReviewDraftAsync()
    {
        ReviewDraftErrorMessage = null;
        IsSubmittingReviewDraft = true;

        try
        {
            var dto = new SoftwareUserReviewDto
            {
                TheGood     = string.IsNullOrWhiteSpace(ReviewDraftTheGood) ? null : ReviewDraftTheGood.Trim(),
                TheBad      = string.IsNullOrWhiteSpace(ReviewDraftTheBad) ? null : ReviewDraftTheBad.Trim(),
                TheUgly     = string.IsNullOrWhiteSpace(ReviewDraftTheUgly) ? null : ReviewDraftTheUgly.Trim(),
                IsAnonymous = ReviewDraftIsAnonymous,
                Rating      = ReviewDraftRating > 0 ? (float?)ReviewDraftRating : null
            };

            if(_myReview is not null && _myReview.Id is > 0)
            {
                var update = await _browsingService.UpdateUserReviewAsync(_currentSoftwareId, _myReview.Id.Value, dto);

                if(!update.Succeeded)
                {
                    ReviewDraftErrorMessage = update.ErrorMessage ?? _localizer["ReviewSaveFailed"];

                    return false;
                }

                SetReviewFeedback(_localizer["ReviewUpdatedSuccess"], InfoBarSeverity.Success);
            }
            else
            {
                var create = await _browsingService.CreateUserReviewAsync(_currentSoftwareId, dto);

                if(create.Review is null)
                {
                    ReviewDraftErrorMessage = create.ErrorMessage ?? _localizer["ReviewSaveFailed"];

                    return false;
                }

                SetReviewFeedback(_localizer["ReviewCreatedSuccess"], InfoBarSeverity.Success);
            }

            await RefreshReviewAuthoringStateAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error submitting review draft for software {SoftwareId}", _currentSoftwareId);
            ReviewDraftErrorMessage = ex.Message;

            return false;
        }
        finally
        {
            IsSubmittingReviewDraft = false;
        }
    }

    public async Task RefreshReviewAuthoringStateAsync()
    {
        await RefreshReviewVotingContextAsync();
        await RefreshCurrentUserRatingAsync(_currentSoftwareId);
        await LoadUserReviewsAsync(_currentSoftwareId);
        UpdateVisibilities();
    }

    public void PrepareReviewReportDraft(UserReviewDisplayItem? review)
    {
        _selectedReviewForReport     = review;
        ReviewReportReason           = ReviewReportReason.Spam;
        ReviewReportExplanation      = string.Empty;
        ReviewReportDraftErrorMessage = null;
    }

    public async Task<bool> SubmitReviewReportDraftAsync()
    {
        ReviewReportDraftErrorMessage = null;
        IsSubmittingReviewReport      = true;

        try
        {
            if(_selectedReviewForReport is null || _selectedReviewForReport.ReviewId <= 0)
            {
                ReviewReportDraftErrorMessage = _localizer["ReviewReportSubmissionFailed"];

                return false;
            }

            var request = new CreateReviewReportRequest
            {
                Reason = (int?)ReviewReportReason,
                Explanation = string.IsNullOrWhiteSpace(ReviewReportExplanation)
                                  ? null
                                  : ReviewReportExplanation.Trim()
            };

            var result = await _browsingService.ReportUserReviewAsync(_currentSoftwareId,
                                                                      _selectedReviewForReport.ReviewId,
                                                                      request);

            if(!result.Succeeded)
            {
                ReviewReportDraftErrorMessage = result.ErrorMessage ?? _localizer["ReviewReportSubmissionFailed"];

                return false;
            }

            SetReviewFeedback(_localizer["ReportSubmittedSuccess"], InfoBarSeverity.Success);
            await RefreshReviewAuthoringStateAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error reporting review for software {SoftwareId}", _currentSoftwareId);
            ReviewReportDraftErrorMessage = ex.Message;

            return false;
        }
        finally
        {
            IsSubmittingReviewReport = false;
        }
    }

    void SetReviewFeedback(string message, InfoBarSeverity severity)
    {
        ReviewFeedbackMessage  = message;
        ReviewFeedbackSeverity = severity;
    }

    [RelayCommand]
    public void ClearReviewFeedback() => ReviewFeedbackMessage = null;

    [RelayCommand]
    public void SetReviewDraftRating(double rating)
    {
        ReviewDraftRating = rating;
        ReviewDraftErrorMessage = null;
    }

    [RelayCommand]
    public void ClearReviewDraftRating()
    {
        ReviewDraftRating = 0;
        ReviewDraftErrorMessage = null;
    }

    [RelayCommand]
    public Task UpvoteReview(UserReviewDisplayItem? review) => VoteReviewAsync(review, true);

    [RelayCommand]
    public Task DownvoteReview(UserReviewDisplayItem? review) => VoteReviewAsync(review, false);

    async Task VoteReviewAsync(UserReviewDisplayItem? review, bool isUpvote)
    {
        if(review is null || review.ReviewId <= 0 || !review.CanVote) return;

        bool succeeded = review.CurrentUserVote == isUpvote
                             ? await _browsingService.RemoveUserReviewVoteAsync(_currentSoftwareId, review.ReviewId)
                             : await _browsingService.VoteUserReviewAsync(_currentSoftwareId, review.ReviewId, isUpvote);

        if(!succeeded) return;

        await RefreshReviewVotingContextAsync();
        await LoadUserReviewsAsync(_currentSoftwareId);
        UpdateVisibilities();
    }

    private static string FormatReviewDate(SoftwareCriticReviewDto review)
    {
        if(!review.ReviewDate.HasValue) return string.Empty;

        return review.ReviewDatePrecision switch
        {
            (int)DatePrecision.Full      => review.ReviewDate.Value.ToString("yyyy-MM-dd"),
            (int)DatePrecision.MonthYear => review.ReviewDate.Value.ToString("yyyy-MM"),
            (int)DatePrecision.YearOnly  => review.ReviewDate.Value.ToString("yyyy"),
            _                            => review.ReviewDate.Value.ToString("yyyy-MM-dd")
        };
    }

    [RelayCommand]
    public async Task OpenCriticReview(CriticReviewDisplayItem? review)
    {
        if(string.IsNullOrWhiteSpace(review?.ReviewUrl)) return;

        try
        {
            await Launcher.LaunchUriAsync(new Uri(review.ReviewUrl));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error launching critic review url {ReviewUrl}", review.ReviewUrl);
        }
    }

    private static string GetCoverGroupLabel(SoftwareCoverDto cover)
    {
        if(!string.IsNullOrWhiteSpace(cover.ReleaseTitle))
            return cover.ReleaseTitle;

        List<string> parts = [];

        if(!string.IsNullOrWhiteSpace(cover.PlatformName))
            parts.Add(cover.PlatformName);

        if(!string.IsNullOrWhiteSpace(cover.RegionNames))
            parts.Add(cover.RegionNames);

        return parts.Count > 0 ? string.Join(" - ", parts) : "Unknown";
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
        ShowCovers      = CoverGroups.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowPromoArt    = PromoArtGroups.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowVideos      = Videos.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowCriticReviews = CriticReviews.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowUserReviews   = Visibility.Visible;
        ShowCredits     = CreditGroups.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowDescription = HasDescription ? Visibility.Visible : Visibility.Collapsed;

        OnPropertyChanged(nameof(PromoArtCount));
        OnPropertyChanged(nameof(HasPromoArt));
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

    partial void OnReviewFeedbackMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasReviewFeedbackMessage));
    }

    partial void OnReviewDraftErrorMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasReviewDraftError));
    }

    partial void OnReviewReportDraftErrorMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasReviewReportDraftError));
    }

    partial void OnReviewDraftRatingChanged(double value)
    {
        RebuildReviewRatingStars();
        OnPropertyChanged(nameof(ReviewDraftRatingText));
    }

    partial void OnIsSubmittingReviewDraftChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSubmitReviewDraft));
    }

    partial void OnIsSubmittingReviewReportChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSubmitReviewReportDraft));
    }

    void RebuildReviewRatingStars()
    {
        ReviewRatingStars.Clear();

        for(int i = 1; i <= 5; i++)
        {
            double fill = Math.Clamp(ReviewDraftRating - (i - 1), 0d, 1d);

            ReviewRatingStars.Add(new ReviewRatingStarItem
            {
                LeftValue = i - 0.5d,
                RightValue = i,
                IsFilled = fill >= 1d,
                IsHalfFilled = fill >= 0.5d && fill < 1d,
                IsEmpty = fill < 0.5d
            });
        }
    }
}

[Bindable]
public class ScreenshotPlatformGroup
{
    public string                                           PlatformName { get; set; } = string.Empty;
    public ObservableCollection<ScreenshotDisplayItem> Screenshots  { get; set; } = [];
}

[Bindable]
public class CoverGroupItem
{
    public string                               GroupLabel { get; set; } = string.Empty;
    public ObservableCollection<CoverDisplayItem> Covers     { get; set; } = [];
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

[Bindable]
public class ReviewRatingStarItem : ObservableObject
{
    public double LeftValue { get; set; }
    public double RightValue { get; set; }
    public bool IsFilled { get; set; }
    public bool IsHalfFilled { get; set; }
    public bool IsEmpty { get; set; }
}

public sealed class ReviewReportReasonOption
{
    public ReviewReportReason Value { get; set; }
    public string Label { get; set; } = string.Empty;
}
