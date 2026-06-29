#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Services;
using Marechai.App.Presentation.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class SoftwareCompilationViewViewModel : ObservableObject, IRegionAware
{
    private readonly IConfiguration                            _configuration;
    private readonly IStringLocalizer                          _localizer;
    private readonly ILogger<SoftwareCompilationViewViewModel> _logger;
    private readonly IRegionManager                            _regionManager;
    private readonly SoftwareCompilationsService               _softwareCompilationsService;

    private int _currentCompilationId;
    private int _sourceCompilationId;
    private int _sourceSoftwareId;
    private string? _navigationSource;

    [ObservableProperty]
    private string _compilationName = string.Empty;

    [ObservableProperty]
    private string? _coverImageUrl;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _bundledSoftwareName;

    [ObservableProperty]
    private int? _bundledSoftwareId;

    [ObservableProperty]
    private string? _bundledMachineName;

    [ObservableProperty]
    private int? _bundledMachineId;

    [ObservableProperty]
    private Visibility _showBundledWith = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showBundledSoftware = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showBundledMachine = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showReleases = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showIncludedSoftware = Visibility.Collapsed;

    public bool HasCover => !string.IsNullOrWhiteSpace(CoverImageUrl);

    partial void OnCoverImageUrlChanged(string? value) => OnPropertyChanged(nameof(HasCover));

    public SoftwareCompilationViewViewModel(ILogger<SoftwareCompilationViewViewModel> logger,
                                            IRegionManager                            regionManager,
                                            SoftwareCompilationsService               softwareCompilationsService,
                                            IStringLocalizer                          localizer,
                                            IConfiguration                            configuration)
    {
        _logger                      = logger;
        _regionManager               = regionManager;
        _softwareCompilationsService = softwareCompilationsService;
        _localizer                   = localizer;
        _configuration               = configuration;
    }

    public ObservableCollection<CompilationReleaseDisplayItem>          Releases             { get; } = [];
    public ObservableCollection<CompilationIncludedSoftwareDisplayItem> IncludedSoftware     { get; } = [];
    public ObservableCollection<CompilationIncludedVersionDisplayItem>  IncludedVersions     { get; } = [];
    public ObservableCollection<CompilationChildDisplayItem>            IncludedCompilations { get; } = [];

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.NavigationSource, out string? source))
            _navigationSource = source;

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareId, out int softwareId))
            _sourceSoftwareId = softwareId;

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SourceSoftwareCompilationId,
                                                         out int sourceCompilationId))
            _sourceCompilationId = sourceCompilationId;

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareCompilationId, out int compilationId))
        {
            _currentCompilationId = compilationId;
            _ = LoadCompilationAsync(compilationId);
        }
    }

    [RelayCommand]
    public Task GoBack()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        if(_sourceCompilationId > 0)
        {
            var parameters = new NavigationParameters
            {
                { NavParamKeys.SoftwareCompilationId, _sourceCompilationId },
                { NavParamKeys.NavigationSource, nameof(SoftwareCompilationViewViewModel) }
            };

            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareCompilationViewPage), parameters);

            return Task.CompletedTask;
        }

        if(_sourceSoftwareId > 0)
        {
            var parameters = new NavigationParameters
            {
                { NavParamKeys.SoftwareId, _sourceSoftwareId },
                { NavParamKeys.NavigationSource, nameof(SoftwareCompilationViewViewModel) }
            };

            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

            return Task.CompletedTask;
        }

        if(_navigationSource == nameof(SoftwareListViewModel))
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareListPage));
        else
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwarePage));

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task LoadData()
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        return _currentCompilationId > 0 ? LoadCompilationAsync(_currentCompilationId) : Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToBundledSoftware()
    {
        if(BundledSoftwareId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, BundledSoftwareId.Value },
            { NavParamKeys.NavigationSource, nameof(SoftwareCompilationViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToBundledMachine()
    {
        if(BundledMachineId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineId, BundledMachineId.Value },
            { NavParamKeys.NavigationSource, nameof(SoftwareCompilationViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToRelease(CompilationReleaseDisplayItem? release)
    {
        if(release?.Id is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareReleaseId, (int)release.Id.Value },
            { NavParamKeys.SourceSoftwareCompilationId, _currentCompilationId },
            { NavParamKeys.NavigationSource, nameof(SoftwareCompilationViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareReleaseViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToIncludedSoftware(CompilationIncludedSoftwareDisplayItem? software)
    {
        if(software?.Id is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, software.Id.Value },
            { NavParamKeys.NavigationSource, nameof(SoftwareCompilationViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToIncludedCompilation(CompilationChildDisplayItem? compilation)
    {
        if(compilation?.Id is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareCompilationId, (int)compilation.Id.Value },
            { NavParamKeys.SourceSoftwareCompilationId, _currentCompilationId },
            { NavParamKeys.NavigationSource, nameof(SoftwareCompilationViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareCompilationViewPage), parameters);

        return Task.CompletedTask;
    }

    private async Task LoadCompilationAsync(int compilationId)
    {
        try
        {
            IsLoading    = true;
            IsDataLoaded = false;
            HasError     = false;
            ErrorMessage = string.Empty;

            Releases.Clear();
            IncludedSoftware.Clear();
            IncludedVersions.Clear();
            IncludedCompilations.Clear();

            SoftwareCompilationDto? compilation = await _softwareCompilationsService.GetAsync(compilationId);

            if(compilation is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["Compilation not found"];
                IsLoading    = false;

                return;
            }

            CompilationName     = compilation.Name ?? _localizer["Compilation"];
            BundledSoftwareName = compilation.Software;
            BundledSoftwareId   = compilation.SoftwareId;
            BundledMachineName  = compilation.Machine;
            BundledMachineId    = compilation.MachineId;

            string baseUrl = _configuration.GetSection("ApiClient:Url").Value ?? string.Empty;
            CoverImageUrl = compilation.FrontCoverId.HasValue
                                ? $"{baseUrl}/assets/photos/software-covers/webp/4k/{compilation.FrontCoverId}.webp"
                                : null;

            var releasesTask             = _softwareCompilationsService.GetReleasesAsync(compilationId);
            var includedSoftwareTask     = _softwareCompilationsService.GetIncludedSoftwareAsync(compilationId);
            var includedVersionsTask     = _softwareCompilationsService.GetIncludedVersionsAsync(compilationId);
            var includedCompilationsTask = _softwareCompilationsService.GetIncludedCompilationsAsync(compilationId);

            await Task.WhenAll(releasesTask, includedSoftwareTask, includedVersionsTask, includedCompilationsTask);

            foreach(SoftwareReleaseDto release in releasesTask.Result)
            {
                Releases.Add(new CompilationReleaseDisplayItem
                {
                    Id                 = release.Id,
                    PlatformDisplay    = string.IsNullOrWhiteSpace(release.Platform)
                                             ? _localizer["Unknown platform"]
                                             : release.Platform,
                    ReleaseDateDisplay = FormatReleaseDate(release)
                });
            }

            foreach(SoftwareBySoftwareCompilationDto software in includedSoftwareTask.Result)
            {
                IncludedSoftware.Add(new CompilationIncludedSoftwareDisplayItem
                {
                    Id   = software.SoftwareId,
                    Name = software.SoftwareName ?? string.Empty
                });
            }

            foreach(SoftwareVersionBySoftwareCompilationDto version in includedVersionsTask.Result)
            {
                IncludedVersions.Add(new CompilationIncludedVersionDisplayItem
                {
                    DisplayText = string.Join(" - ", new[]
                    {
                        version.SoftwareName,
                        version.SoftwareVersion
                    }.Where(x => !string.IsNullOrWhiteSpace(x)))
                });
            }

            foreach(SoftwareCompilationDto child in includedCompilationsTask.Result)
            {
                IncludedCompilations.Add(new CompilationChildDisplayItem
                {
                    Id   = child.Id,
                    Name = child.Name ?? string.Empty
                });
            }

            UpdateVisibilities();
            IsDataLoaded = true;
            IsLoading    = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software compilation {CompilationId}", compilationId);
            HasError     = true;
            ErrorMessage = ex.Message;
            IsLoading    = false;
        }
    }

    private void UpdateVisibilities()
    {
        ShowBundledSoftware = !string.IsNullOrWhiteSpace(BundledSoftwareName) ? Visibility.Visible : Visibility.Collapsed;
        ShowBundledMachine  = !string.IsNullOrWhiteSpace(BundledMachineName) ? Visibility.Visible : Visibility.Collapsed;
        ShowBundledWith     = ShowBundledSoftware == Visibility.Visible || ShowBundledMachine == Visibility.Visible
                                  ? Visibility.Visible
                                  : Visibility.Collapsed;
        ShowReleases        = Releases.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowIncludedSoftware = IncludedSoftware.Count > 0 || IncludedVersions.Count > 0 ||
                               IncludedCompilations.Count > 0
                                   ? Visibility.Visible
                                   : Visibility.Collapsed;
    }

    private static string? FormatReleaseDate(SoftwareReleaseDto release)
    {
        return DatePrecisionFormatter.Format(release.ReleaseDate, release.ReleaseDatePrecision, null);
    }
}

public sealed class CompilationReleaseDisplayItem
{
    public int? Id                    { get; set; }
    public string PlatformDisplay     { get; set; } = string.Empty;
    public string? ReleaseDateDisplay { get; set; }
}

public sealed class CompilationIncludedSoftwareDisplayItem
{
    public int?   Id   { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class CompilationIncludedVersionDisplayItem
{
    public string DisplayText { get; set; } = string.Empty;
}

public sealed class CompilationChildDisplayItem
{
    public int?   Id   { get; set; }
    public string Name { get; set; } = string.Empty;
}
