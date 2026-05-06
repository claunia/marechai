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
    }

    private async Task LoadSoftwareFromApiAsync()
    {
        try
        {
            List<SoftwareDto> software = FilterType switch
                                         {
                                             SoftwareListFilterType.Letter when FilterValue.Length == 1 =>
                                                 await _browsingService.GetSoftwareByLetterAsync(FilterValue[0]),

                                             SoftwareListFilterType.Year when int.TryParse(FilterValue, out int year) =>
                                                 await _browsingService.GetSoftwareByYearAsync(year),

                                             SoftwareListFilterType.Platform
                                                 when int.TryParse(FilterValue, out int platformId) =>
                                                 await _browsingService.GetSoftwareByPlatformAsync(platformId),

                                             SoftwareListFilterType.Spec
                                                 when FilterValue.Contains('|') =>
                                                 await _browsingService.GetSoftwareBySpecAsync(
                                                     FilterValue.Split('|', 2)[0],
                                                     FilterValue.Split('|', 2)[1]),

                                             _ => await _browsingService.GetAllSoftwareAsync()
                                         };

            foreach(SoftwareDto sw in software)
            {
                int id = (int)(sw.Id ?? 0);

                var item = new SoftwareListItem
                {
                    Id                = id,
                    Name              = sw.Name ?? string.Empty,
                    Family            = sw.Family,
                    Kind              = (SoftwareKind)(sw.Kind ?? 0)
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

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, sw.Id },
            { NavParamKeys.NavigationSource, nameof(SoftwareListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

        return Task.CompletedTask;
    }
}
