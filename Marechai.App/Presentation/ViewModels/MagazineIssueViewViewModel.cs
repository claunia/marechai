#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Uno.Extensions.Authentication;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class MagazineIssueViewViewModel : ObservableObject, IRegionAware
{
    private readonly MagazinesService                   _magazinesService;
    private readonly MagazineIssueCoverCache             _coverCache;
    private readonly ImageSourceFactory                  _imageSourceFactory;
    private readonly IAuthenticationService              _authService;
    private readonly IStringLocalizer                    _localizer;
    private readonly ILogger<MagazineIssueViewViewModel> _logger;
    private readonly IRegionManager                      _regionManager;

    private string? _navigationSource;
    private long    _currentIssueId;
    private long?   _magazineId;

    [ObservableProperty]
    private string _caption = string.Empty;

    [ObservableProperty]
    private string? _nativeCaption;

    [ObservableProperty]
    private string? _magazineTitle;

    [ObservableProperty]
    private int? _issueNumber;

    [ObservableProperty]
    private string? _publishedDisplay;

    [ObservableProperty]
    private int? _pages;

    [ObservableProperty]
    private string? _productCode;

    [ObservableProperty]
    private string? _internetArchiveUrl;

    [ObservableProperty]
    private ImageSource? _coverImageSource;

    [ObservableProperty]
    private bool _hasCover;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isCollected;

    [ObservableProperty]
    private bool _isTogglingCollection;

    [ObservableProperty]
    private string _collectionButtonText = string.Empty;

    // Visibility flags
    [ObservableProperty]
    private Visibility _showNativeCaption = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showMagazineLink = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showIssueNumber = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPublished = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPages = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showProductCode = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showInternetArchiveLink = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPeople = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showMachines = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showMachineFamilies = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSoftware = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCollectionButton = Visibility.Collapsed;

    public MagazineIssueViewViewModel(ILogger<MagazineIssueViewViewModel> logger,
                                       IRegionManager                     regionManager,
                                       MagazinesService                   magazinesService,
                                       MagazineIssueCoverCache             coverCache,
                                       ImageSourceFactory                  imageSourceFactory,
                                       IAuthenticationService              authService,
                                       IStringLocalizer                   localizer)
    {
        _logger             = logger;
        _regionManager      = regionManager;
        _magazinesService   = magazinesService;
        _coverCache         = coverCache;
        _imageSourceFactory = imageSourceFactory;
        _authService        = authService;
        _localizer          = localizer;
    }

    partial void OnIsCollectedChanged(bool value) =>
        CollectionButtonText = value ? _localizer["In Collection"] : _localizer["Add to Collection"];

    public ObservableCollection<string> People          { get; } = [];
    public ObservableCollection<string> Machines        { get; } = [];
    public ObservableCollection<string> MachineFamilies { get; } = [];
    public ObservableCollection<string> Software        { get; } = [];

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.NavigationSource, out string? source))
            _navigationSource = source;

        if(navigationContext.Parameters.TryGetValue<long>(NavParamKeys.MagazineIssueId, out long issueId))
        {
            _currentIssueId = issueId;
            _ = LoadIssueAsync(issueId);
        }
    }

    [RelayCommand]
    public Task GoBack()
    {
        if(_magazineId.HasValue)
        {
            var parameters = new NavigationParameters
            {
                { NavParamKeys.MagazineId, _magazineId.Value },
                { NavParamKeys.NavigationSource, nameof(MagazineIssueViewViewModel) }
            };

            _regionManager.RequestNavigate(RegionNames.Content, nameof(MagazineViewPage), parameters);
        }
        else
            _regionManager.RequestNavigate(RegionNames.Content, nameof(MagazinesPage));

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToMagazine()
    {
        if(_magazineId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MagazineId, _magazineId.Value },
            { NavParamKeys.NavigationSource, nameof(MagazineIssueViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MagazineViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task LoadData()
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task ToggleCollection()
    {
        if(IsTogglingCollection || _currentIssueId == 0) return;

        try
        {
            IsTogglingCollection = true;

            bool success = IsCollected
                               ? await _magazinesService.RemoveMagazineIssueFromCollectionAsync(_currentIssueId)
                               : await _magazinesService.AddMagazineIssueToCollectionAsync(_currentIssueId);

            if(success)
                IsCollected = !IsCollected;
        }
        finally
        {
            IsTogglingCollection = false;
        }
    }

    public async Task LoadIssueAsync(long issueId)
    {
        try
        {
            IsLoading    = true;
            IsDataLoaded = false;
            HasError     = false;
            ErrorMessage = string.Empty;
            People.Clear();
            Machines.Clear();
            MachineFamilies.Clear();
            Software.Clear();

            MagazineIssueFullDto? full = await _magazinesService.GetIssueFullAsync(issueId);

            if(full?.Issue is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["Magazine issue not found"];
                IsLoading    = false;

                return;
            }

            MagazineIssueDto issue = full.Issue;

            Caption             = issue.Caption ?? string.Empty;
            NativeCaption       = issue.NativeCaption;
            MagazineTitle       = full.MagazineTitle ?? issue.MagazineTitle;
            _magazineId         = issue.MagazineId;
            IssueNumber         = issue.IssueNumber;
            Pages               = issue.Pages;
            ProductCode         = issue.ProductCode;
            InternetArchiveUrl  = issue.InternetArchiveUrl;

            if(issue.Published.HasValue)
                PublishedDisplay = DatePrecisionFormatter.Format(issue.Published, issue.PublishedPrecision);

            // Load cover image
            HasCover = issue.CoverGuid.HasValue;

            if(issue.CoverGuid.HasValue)
                _ = LoadCoverAsync(issue.CoverGuid.Value);

            // Load people (by role)
            if(full.People is not null)
                foreach(PersonByMagazineDto person in full.People)
                {
                    string  name    = person.DisplayName ?? person.Alias ?? $"{person.Name} {person.Surname}".Trim();
                    string? roleStr = person.Role;
                    string  display = !string.IsNullOrEmpty(roleStr) ? $"{name} ({roleStr})" : name;
                    People.Add(display);
                }

            // Load machines
            if(full.Machines is not null)
                foreach(MagazineByMachineDto machine in full.Machines)
                    Machines.Add(machine.Machine ?? string.Empty);

            // Load machine families
            if(full.MachineFamilies is not null)
                foreach(MagazineByMachineFamilyDto family in full.MachineFamilies)
                    MachineFamilies.Add(family.MachineFamily ?? string.Empty);

            // Load software
            if(full.Software is not null)
                foreach(MagazineBySoftwareDto software in full.Software)
                    Software.Add(software.Software ?? string.Empty);

            // Load collection state (authenticated users only)
            bool isAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);
            ShowCollectionButton = isAuthenticated ? Visibility.Visible : Visibility.Collapsed;

            if(isAuthenticated)
                IsCollected = await _magazinesService.IsMagazineIssueCollectedAsync(issueId);

            CollectionButtonText = IsCollected ? _localizer["In Collection"] : _localizer["Add to Collection"];

            UpdateVisibilities();
            IsDataLoaded = true;
            IsLoading    = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading magazine issue {IssueId}", issueId);
            HasError     = true;
            ErrorMessage = ex.Message;
            IsLoading    = false;
        }
    }

    private void UpdateVisibilities()
    {
        ShowNativeCaption       = !string.IsNullOrEmpty(NativeCaption) ? Visibility.Visible : Visibility.Collapsed;
        ShowMagazineLink        = !string.IsNullOrEmpty(MagazineTitle) ? Visibility.Visible : Visibility.Collapsed;
        ShowIssueNumber         = IssueNumber.HasValue ? Visibility.Visible : Visibility.Collapsed;
        ShowPublished           = !string.IsNullOrEmpty(PublishedDisplay) ? Visibility.Visible : Visibility.Collapsed;
        ShowPages               = Pages.HasValue ? Visibility.Visible : Visibility.Collapsed;
        ShowProductCode         = !string.IsNullOrEmpty(ProductCode) ? Visibility.Visible : Visibility.Collapsed;
        ShowInternetArchiveLink = !string.IsNullOrEmpty(InternetArchiveUrl) ? Visibility.Visible : Visibility.Collapsed;
        ShowPeople              = People.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowMachines            = Machines.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowMachineFamilies     = MachineFamilies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowSoftware            = Software.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task LoadCoverAsync(Guid coverGuid)
    {
        try
        {
            Stream stream = await _coverCache.GetCoverAsync(coverGuid);
            CoverImageSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading cover for magazine issue {IssueId}", _currentIssueId);
        }
    }
}
