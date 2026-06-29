#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class ProfileViewModel : ObservableObject, IRegionAware
{
    private readonly ProfileService              _profileService;
    private readonly IStringLocalizer            _localizer;
    private readonly ILogger<ProfileViewModel>   _logger;
    private readonly IRegionManager              _regionManager;

    private string? _username;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string? _bio;

    [ObservableProperty]
    private string? _website;

    [ObservableProperty]
    private string? _location;

    [ObservableProperty]
    private string? _gitHub;

    [ObservableProperty]
    private string? _twitter;

    [ObservableProperty]
    private string? _mastodon;

    [ObservableProperty]
    private string? _facebook;

    [ObservableProperty]
    private string? _linkedIn;

    [ObservableProperty]
    private string? _avatarUrl;

    [ObservableProperty]
    private bool _isCollaborator;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private Visibility _showBio = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showLinks = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showLocation = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCollaboratorBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showAvatar = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSummaryChips = Visibility.Collapsed;

    [ObservableProperty]
    private int _bookCount;

    [ObservableProperty]
    private int _documentCount;

    [ObservableProperty]
    private int _magazineIssueCount;

    [ObservableProperty]
    private int _computerCount;

    [ObservableProperty]
    private int _consoleCount;

    [ObservableProperty]
    private int _smartphoneCount;

    [ObservableProperty]
    private int _softwareReleaseCount;

    [ObservableProperty]
    private Visibility _showBooks = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showDocuments = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showMagazineIssues = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showComputers = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showConsoles = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSmartphones = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSoftwareReleases = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showReviews = Visibility.Collapsed;

    public ObservableCollection<CollectedBookDto>            Books            { get; } = [];
    public ObservableCollection<CollectedDocumentDto>        Documents        { get; } = [];
    public ObservableCollection<CollectedMagazineIssueDto>   MagazineIssues   { get; } = [];
    public ObservableCollection<CollectedMachineDto>         Computers        { get; } = [];
    public ObservableCollection<CollectedMachineDto>         Consoles         { get; } = [];
    public ObservableCollection<CollectedMachineDto>         Smartphones      { get; } = [];
    public ObservableCollection<CollectedSoftwareReleaseDto> SoftwareReleases { get; } = [];
    public ObservableCollection<SoftwareUserReviewDto>       Reviews          { get; } = [];

    public ProfileViewModel(ILogger<ProfileViewModel> logger, IRegionManager regionManager,
                            ProfileService              profileService, IStringLocalizer localizer)
    {
        _logger         = logger;
        _regionManager  = regionManager;
        _profileService = profileService;
        _localizer      = localizer;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.ProfileUsername, out string? username) &&
           !string.IsNullOrWhiteSpace(username))
        {
            _username = username;
            _ = LoadProfileAsync(username);
        }
    }

    [RelayCommand]
    public Task GoBack()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task LoadData()
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        if(!string.IsNullOrWhiteSpace(_username))
            _ = LoadProfileAsync(_username);

        return Task.CompletedTask;
    }

    private async Task LoadProfileAsync(string username)
    {
        try
        {
            IsLoading    = true;
            IsDataLoaded = false;
            HasError     = false;
            ErrorMessage = string.Empty;

            PublicProfileDto? profile = await _profileService.GetPublicProfileAsync(username);

            if(profile is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["UserNotFoundText"];
                IsLoading    = false;

                return;
            }

            UserName        = profile.UserName ?? username;
            DisplayName     = profile.DisplayName ?? profile.UserName ?? username;
            Bio             = profile.Bio;
            Website         = profile.Website;
            Location        = profile.Location;
            GitHub          = profile.GitHub;
            Twitter         = profile.Twitter;
            Mastodon        = profile.Mastodon;
            Facebook        = profile.Facebook;
            LinkedIn        = profile.LinkedIn;
            AvatarUrl       = profile.AvatarUrl;
            IsCollaborator  = profile.IsCollaborator == true;
            IsAdmin         = profile.IsAdmin == true;

            UserCollectionSummaryDto? summary = await _profileService.GetCollectionSummaryAsync(username);
            BookCount             = summary?.BookCount             ?? 0;
            DocumentCount         = summary?.DocumentCount         ?? 0;
            MagazineIssueCount    = summary?.MagazineIssueCount    ?? 0;
            SoftwareReleaseCount  = summary?.SoftwareReleaseCount  ?? 0;

            Books.Clear();
            foreach(CollectedBookDto book in await _profileService.GetCollectedBooksAsync(username))
                Books.Add(book);

            Documents.Clear();
            foreach(CollectedDocumentDto document in await _profileService.GetCollectedDocumentsAsync(username))
                Documents.Add(document);

            MagazineIssues.Clear();

            foreach(CollectedMagazineIssueDto issue in await _profileService.GetCollectedMagazineIssuesAsync(username))
                MagazineIssues.Add(issue);

            List<CollectedMachineDto> machines = await _profileService.GetCollectedMachinesAsync(username);

            Computers.Clear();
            foreach(CollectedMachineDto machine in machines.Where(m => m.Type == 1)) Computers.Add(machine);

            Consoles.Clear();
            foreach(CollectedMachineDto machine in machines.Where(m => m.Type == 2)) Consoles.Add(machine);

            Smartphones.Clear();
            foreach(CollectedMachineDto machine in machines.Where(m => m.Type == 3)) Smartphones.Add(machine);

            ComputerCount   = Computers.Count;
            ConsoleCount    = Consoles.Count;
            SmartphoneCount = Smartphones.Count;

            SoftwareReleases.Clear();

            foreach(CollectedSoftwareReleaseDto release in
                    await _profileService.GetCollectedSoftwareReleasesAsync(username))
                SoftwareReleases.Add(release);

            Reviews.Clear();
            foreach(SoftwareUserReviewDto review in await _profileService.GetUserReviewsAsync(username))
                Reviews.Add(review);

            UpdateVisibilities();
            IsDataLoaded = true;
            IsLoading    = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading profile for {Username}", username);
            HasError     = true;
            ErrorMessage = ex.Message;
            IsLoading    = false;
        }
    }

    [RelayCommand]
    public Task NavigateToBook(CollectedBookDto? book)
    {
        if(book?.BookId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.BookId, book.BookId.Value }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(BookViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToDocument(CollectedDocumentDto? document)
    {
        if(document?.DocumentId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.DocumentId, document.DocumentId.Value }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(DocumentViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToMagazineIssue(CollectedMagazineIssueDto? issue)
    {
        if(issue?.MagazineIssueId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MagazineIssueId, issue.MagazineIssueId.Value }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MagazineIssueViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToMachine(CollectedMachineDto? machine)
    {
        if(machine?.MachineId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineId, machine.MachineId.Value }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToSoftwareRelease(CollectedSoftwareReleaseDto? release)
    {
        if(release?.SoftwareReleaseId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareReleaseId, release.SoftwareReleaseId.Value }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareReleaseViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToReviewSoftware(SoftwareUserReviewDto? review)
    {
        if(review?.SoftwareId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, review.SoftwareId.Value }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

        return Task.CompletedTask;
    }

    private void UpdateVisibilities()
    {
        ShowAvatar             = !string.IsNullOrWhiteSpace(AvatarUrl) ? Visibility.Visible : Visibility.Collapsed;
        ShowBio                = !string.IsNullOrWhiteSpace(Bio) ? Visibility.Visible : Visibility.Collapsed;
        ShowLocation           = !string.IsNullOrWhiteSpace(Location) ? Visibility.Visible : Visibility.Collapsed;
        ShowCollaboratorBadge  = IsCollaborator && !IsAdmin ? Visibility.Visible : Visibility.Collapsed;

        ShowLinks = !string.IsNullOrWhiteSpace(Website)  ||
                    !string.IsNullOrWhiteSpace(GitHub)   ||
                    !string.IsNullOrWhiteSpace(Twitter)  ||
                    !string.IsNullOrWhiteSpace(Mastodon) ||
                    !string.IsNullOrWhiteSpace(Facebook) ||
                    !string.IsNullOrWhiteSpace(LinkedIn)
                        ? Visibility.Visible
                        : Visibility.Collapsed;

        ShowSummaryChips = BookCount > 0 || DocumentCount > 0 || MagazineIssueCount > 0 || ComputerCount > 0 ||
                           ConsoleCount > 0 || SoftwareReleaseCount > 0
                               ? Visibility.Visible
                               : Visibility.Collapsed;

        ShowBooks            = Books.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowDocuments        = Documents.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowMagazineIssues   = MagazineIssues.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowComputers        = Computers.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowConsoles         = Consoles.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowSmartphones      = Smartphones.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowSoftwareReleases = SoftwareReleases.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowReviews          = Reviews.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}
