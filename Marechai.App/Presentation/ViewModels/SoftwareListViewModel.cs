#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.Data;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class SoftwareListViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareBrowsingService          _browsingService;
    private readonly ISoftwareListFilterContext        _filterContext;
    private readonly IStringLocalizer                  _localizer;
    private readonly ILogger<SoftwareListViewModel>    _logger;
    private readonly IRegionManager                    _regionManager;

    [ObservableProperty]
    private ObservableCollection<SoftwareListItem> _softwareList = [];

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _filterDescription = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _pageTitle = string.Empty;

    public SoftwareListViewModel(SoftwareBrowsingService          browsingService, IStringLocalizer localizer,
                                 ILogger<SoftwareListViewModel>    logger,          IRegionManager   regionManager,
                                 ISoftwareListFilterContext        filterContext)
    {
        _browsingService          = browsingService;
        _localizer                = localizer;
        _logger                   = logger;
        _regionManager            = regionManager;
        _filterContext            = filterContext;
        LoadData                  = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand             = new AsyncRelayCommand(GoBackAsync);
        NavigateToSoftwareCommand = new AsyncRelayCommand<SoftwareListItem>(NavigateToSoftwareAsync);
    }

    public IAsyncRelayCommand                      LoadData                  { get; }
    public ICommand                                GoBackCommand             { get; }
    public IAsyncRelayCommand<SoftwareListItem>    NavigateToSoftwareCommand { get; }

    public SoftwareListFilterType FilterType
    {
        get => _filterContext.FilterType;
        set => _filterContext.FilterType = value;
    }

    public string FilterValue
    {
        get => _filterContext.FilterValue;
        set => _filterContext.FilterValue = value;
    }

    public SoftwareKind? Kind
    {
        get => _filterContext.Kind;
        set
        {
            if(_filterContext.Kind == value) return;

            _filterContext.Kind = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(KindLabel));
            OnPropertyChanged(nameof(SelectedKindIndex));
            _ = LoadDataAsync();
        }
    }

    public string KindLabel => Kind switch
    {
        SoftwareKind.OperatingSystem     => _localizer["SoftwareIsOSLabel"],
        SoftwareKind.Game                => _localizer["SoftwareIsGameLabel"],
        SoftwareKind.Dlc                 => _localizer["SoftwareIsDlcLabel"],
        SoftwareKind.SystemSoftware      => _localizer["SoftwareIsSystemSoftwareLabel"],
        SoftwareKind.Application         => _localizer["SoftwareIsApplicationLabel"],
        SoftwareKind.DevelopmentSoftware => _localizer["SoftwareIsDevelopmentSoftwareLabel"],
        SoftwareKind.ServerSoftware      => _localizer["SoftwareIsServerSoftwareLabel"],
        SoftwareKind.Middleware          => _localizer["SoftwareIsMiddlewareLabel"],
        SoftwareKind.Firmware            => _localizer["SoftwareIsFirmwareLabel"],
        SoftwareKind.EmbeddedSoftware    => _localizer["SoftwareIsEmbeddedSoftwareLabel"],
        SoftwareKind.Software            => _localizer["SoftwareIsSoftwareLabel"],
        _                                 => _localizer["SoftwareAnyKindLabel"]
    };

    // Index 0 = Any (null). Index 1..11 = SoftwareKind values 0..10 in enum order.
    // Used to bind ComboBox.SelectedIndex from XAML without a custom converter.
    public int SelectedKindIndex
    {
        get => Kind switch
        {
            null                                  => 0,
            SoftwareKind.Software                 => 1,
            SoftwareKind.OperatingSystem          => 2,
            SoftwareKind.Game                     => 3,
            SoftwareKind.Dlc                      => 4,
            SoftwareKind.SystemSoftware           => 5,
            SoftwareKind.Application              => 6,
            SoftwareKind.DevelopmentSoftware      => 7,
            SoftwareKind.ServerSoftware           => 8,
            SoftwareKind.Middleware               => 9,
            SoftwareKind.Firmware                 => 10,
            SoftwareKind.EmbeddedSoftware         => 11,
            _                                      => 0
        };
        set => Kind = value switch
        {
            1  => SoftwareKind.Software,
            2  => SoftwareKind.OperatingSystem,
            3  => SoftwareKind.Game,
            4  => SoftwareKind.Dlc,
            5  => SoftwareKind.SystemSoftware,
            6  => SoftwareKind.Application,
            7  => SoftwareKind.DevelopmentSoftware,
            8  => SoftwareKind.ServerSoftware,
            9  => SoftwareKind.Middleware,
            10 => SoftwareKind.Firmware,
            11 => SoftwareKind.EmbeddedSoftware,
            _  => null
        };
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            SoftwareList.Clear();

            _logger.LogInformation("LoadDataAsync called. FilterType={FilterType}, FilterValue={FilterValue}",
                                   FilterType,
                                   FilterValue);

            UpdateFilterDescription();
            await LoadSoftwareFromApiAsync();

            _logger.LogInformation("LoadSoftwareFromApiAsync completed. SoftwareList.Count={Count}",
                                   SoftwareList.Count);

            if(SoftwareList.Count == 0)
            {
                ErrorMessage = _localizer["No software found for this filter"].Value;
                HasError     = true;

                _logger.LogWarning("No software found for filter: {FilterType} {FilterValue}",
                                   FilterType,
                                   FilterValue);
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load software. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateFilterDescription()
    {
        switch(FilterType)
        {
            case SoftwareListFilterType.All:
                PageTitle         = _localizer["All Software"];
                FilterDescription = _localizer["Browsing all software in the database"];

                break;

            case SoftwareListFilterType.Letter:
                if(!string.IsNullOrEmpty(FilterValue) && FilterValue.Length == 1)
                {
                    PageTitle         = $"{_localizer["Software Starting with"]} {FilterValue}";
                    FilterDescription = $"{_localizer["Showing software that start with"]} {FilterValue}";
                }

                break;

            case SoftwareListFilterType.Year:
                if(!string.IsNullOrEmpty(FilterValue) && int.TryParse(FilterValue, out int year))
                {
                    PageTitle         = $"{_localizer["Software from"]} {year}";
                    FilterDescription = $"{_localizer["Showing software released in"]} {year}";
                }

                break;

            case SoftwareListFilterType.Platform:
                PageTitle         = _localizer["Software by Platform"];
                FilterDescription = _localizer["Showing software for the selected platform"];

                break;

            case SoftwareListFilterType.Spec:
                if(!string.IsNullOrEmpty(FilterValue) && FilterValue.Contains('|'))
                {
                    string[] parts    = FilterValue.Split('|', 2);
                    string   specKey  = parts[0];
                    string   specVal  = parts[1];
                    PageTitle         = $"{_localizer[specKey]}: {_localizer[specVal]}";
                    FilterDescription = string.Format(_localizer["Showing software with {0}: {1}"], _localizer[specKey], _localizer[specVal]);
                }

                break;
        }

        if(_filterContext.Kind.HasValue)
            FilterDescription =
                $"{FilterDescription} \u2014 {string.Format(_localizer["SoftwareFilteredByKindFormat"], KindLabel)}";
    }

    private async Task LoadSoftwareFromApiAsync()
    {
        try
        {
            SoftwareKind? kind = _filterContext.Kind;

            List<SoftwareDto> software = FilterType switch
                                         {
                                             SoftwareListFilterType.Letter when FilterValue.Length == 1 =>
                                                 await _browsingService.GetSoftwareByLetterAsync(FilterValue[0], kind),

                                             SoftwareListFilterType.Year when int.TryParse(FilterValue, out int year) =>
                                                 await _browsingService.GetSoftwareByYearAsync(year, kind),

                                             SoftwareListFilterType.Platform
                                                 when int.TryParse(FilterValue, out int platformId) =>
                                                 await _browsingService.GetSoftwareByPlatformAsync(platformId, kind),

                                             SoftwareListFilterType.Spec
                                                 when FilterValue.Contains('|') =>
                                                 await _browsingService.GetSoftwareBySpecAsync(
                                                     FilterValue.Split('|', 2)[0],
                                                     FilterValue.Split('|', 2)[1],
                                                     kind),

                                             _ => await _browsingService.GetAllSoftwareAsync(kind)
                                         };

            foreach(SoftwareDto sw in software)
            {
                int id = (int)(sw.Id ?? 0);

                var item = new SoftwareListItem
                {
                    Id                = id,
                    Name              = sw.Name ?? string.Empty,
                    Family            = sw.Family,
                    Kind              = (SoftwareKind)(sw.Kind ?? 0),
                    IsCompilation     = sw.IsCompilation ?? false
                };

                SoftwareList.Add(item);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software from API");
        }
    }

    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwarePage));

        return Task.CompletedTask;
    }

    private Task NavigateToSoftwareAsync(SoftwareListItem? sw)
    {
        if(sw is null) return Task.CompletedTask;

        if(sw.IsCompilation)
        {
            var releaseParameters = new NavigationParameters
            {
                { NavParamKeys.SoftwareReleaseId, sw.Id },
                { NavParamKeys.NavigationSource, nameof(SoftwareListViewModel) }
            };

            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareReleaseViewPage), releaseParameters);

            return Task.CompletedTask;
        }

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, sw.Id },
            { NavParamKeys.NavigationSource, nameof(SoftwareListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

        return Task.CompletedTask;
    }
}
