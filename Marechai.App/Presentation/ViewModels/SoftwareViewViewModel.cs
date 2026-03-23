#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class SoftwareViewViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareBrowsingService        _browsingService;
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
    private bool _isOperatingSystem;

    [ObservableProperty]
    private bool _isGame;

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
    private Visibility _showCompanies = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showVersions = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showOsBadge = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showGameBadge = Visibility.Collapsed;

    public SoftwareViewViewModel(ILogger<SoftwareViewViewModel> logger,          IRegionManager regionManager,
                                 SoftwareBrowsingService        browsingService, IStringLocalizer localizer)
    {
        _logger          = logger;
        _regionManager   = regionManager;
        _browsingService = browsingService;
        _localizer       = localizer;
    }

    public ObservableCollection<string>             Companies { get; } = [];
    public ObservableCollection<VersionDisplayItem> Versions  { get; } = [];

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
            IsOperatingSystem = software.IsOperatingSystem ?? false;
            IsGame            = software.IsGame ?? false;

            // Load companies
            List<SoftwareCompanyRoleDto> companies = await _browsingService.GetCompaniesAsync(softwareId);

            foreach(SoftwareCompanyRoleDto company in companies)
            {
                string name    = company.Company ?? string.Empty;
                string? role   = company.Role;
                string display = !string.IsNullOrEmpty(role) ? $"{name} ({role})" : name;
                Companies.Add(display);
            }

            // Load versions with their releases
            List<SoftwareVersionDto> versions = await _browsingService.GetVersionsAsync(softwareId);

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

                // Load releases for this version
                List<SoftwareReleaseDto> releases = await _browsingService.GetReleasesByVersionAsync(versionId);

                foreach(SoftwareReleaseDto release in releases)
                {
                    string dateDisplay = release.ReleaseDate?.DateTime.ToString("MMMM d, yyyy") ?? string.Empty;

                    var releaseItem = new ReleaseDisplayItem
                    {
                        Id          = (int)(release.Id ?? 0),
                        Platform    = release.Platform,
                        Region      = release.Region,
                        Publisher   = release.Publisher,
                        ReleaseDate = dateDisplay,
                        Variant     = release.Variant
                    };

                    versionItem.Releases.Add(releaseItem);
                }

                Versions.Add(versionItem);
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

    private void UpdateVisibilities()
    {
        ShowFamily    = !string.IsNullOrEmpty(Family) ? Visibility.Visible : Visibility.Collapsed;
        ShowCompanies = Companies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowVersions  = Versions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowOsBadge   = IsOperatingSystem ? Visibility.Visible : Visibility.Collapsed;
        ShowGameBadge = IsGame ? Visibility.Visible : Visibility.Collapsed;
    }
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
    public int     Id          { get; set; }
    public string? Platform    { get; set; }
    public string? Region      { get; set; }
    public string? Publisher   { get; set; }
    public string? ReleaseDate { get; set; }
    public string? Variant     { get; set; }
}
