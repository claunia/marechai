#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;

namespace Marechai.App.Presentation.ViewModels;

/// <summary>
///     ViewModel for displaying a filtered list of smartphones
/// </summary>
public partial class SmartphonesListViewModel : ObservableObject
{
    private readonly SmartphonesService                _smartphonesService;
    private readonly ISmartphonesListFilterContext     _filterContext;
    private readonly IStringLocalizer                  _localizer;
    private readonly ILogger<SmartphonesListViewModel> _logger;
    private readonly IRegionManager                    _regionManager;

    [ObservableProperty]
    private ObservableCollection<SmartphoneListItem> _smartphonesList = [];

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

    public SmartphonesListViewModel(SmartphonesService                smartphonesService, IStringLocalizer localizer,
                                    ILogger<SmartphonesListViewModel> logger,             IRegionManager    regionManager,
                                    ISmartphonesListFilterContext     filterContext)
    {
        _smartphonesService            = smartphonesService;
        _localizer                     = localizer;
        _logger                        = logger;
        _regionManager                 = regionManager;
        _filterContext                 = filterContext;
        LoadData                       = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand                  = new AsyncRelayCommand(GoBackAsync);
        NavigateToSmartphoneCommand    = new AsyncRelayCommand<SmartphoneListItem>(NavigateToSmartphoneAsync);
    }

    public IAsyncRelayCommand                      LoadData                    { get; }
    public ICommand                                GoBackCommand               { get; }
    public IAsyncRelayCommand<SmartphoneListItem>  NavigateToSmartphoneCommand { get; }

    public SmartphoneListFilterType FilterType
    {
        get => _filterContext.FilterType;
        set => _filterContext.FilterType = value;
    }

    public string FilterValue
    {
        get => _filterContext.FilterValue;
        set => _filterContext.FilterValue = value;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            SmartphonesList.Clear();

            _logger.LogInformation("LoadDataAsync called. FilterType={FilterType}, FilterValue={FilterValue}",
                                   FilterType,
                                   FilterValue);

            UpdateFilterDescription();

            await LoadSmartphonesFromApiAsync();

            _logger.LogInformation("LoadSmartphonesFromApiAsync completed. SmartphonesList.Count={Count}",
                                   SmartphonesList.Count);

            if(SmartphonesList.Count == 0)
            {
                ErrorMessage = _localizer["No smartphones found for this filter"].Value;
                HasError     = true;

                _logger.LogWarning("No smartphones found for filter: {FilterType} {FilterValue}", FilterType, FilterValue);
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading smartphones: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load smartphones. Please try again later."].Value;
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
            case SmartphoneListFilterType.All:
                PageTitle         = _localizer["All Smartphones"];
                FilterDescription = _localizer["Browsing all smartphones in the database"];

                break;

            case SmartphoneListFilterType.Letter:
                if(!string.IsNullOrEmpty(FilterValue) && FilterValue.Length == 1)
                {
                    PageTitle         = $"{_localizer["Smartphones Starting with"]} {FilterValue}";
                    FilterDescription = $"{_localizer["Showing smartphones that start with"]} {FilterValue}";
                }

                break;

            case SmartphoneListFilterType.Year:
                if(!string.IsNullOrEmpty(FilterValue) && int.TryParse(FilterValue, out int year))
                {
                    PageTitle         = $"{_localizer["Smartphones from"]} {year}";
                    FilterDescription = $"{_localizer["Showing smartphones released in"]} {year}";
                }

                break;
        }
    }

    private async Task LoadSmartphonesFromApiAsync()
    {
        try
        {
            List<MachineDto> smartphones = FilterType switch
                                           {
                                               SmartphoneListFilterType.Letter when FilterValue.Length == 1 =>
                                                   await _smartphonesService.GetSmartphonesByLetterAsync(FilterValue[0]),

                                               SmartphoneListFilterType.Year when int.TryParse(FilterValue, out int year) =>
                                                   await _smartphonesService.GetSmartphonesByYearAsync(year),

                                               _ => await _smartphonesService.GetAllSmartphonesAsync()
                                           };

            foreach(MachineDto smartphone in smartphones.OrderBy(c => c.Name))
            {
                int year = smartphone.Introduced?.Year ?? 0;
                int id   = smartphone.Id               ?? 0;

                _logger.LogInformation("Smartphone: {Name}, Introduced: {Introduced}, Year: {Year}, Company: {Company}, ID: {Id}",
                                       smartphone.Name,
                                       smartphone.Introduced,
                                       year,
                                       smartphone.Company,
                                       id);

                SmartphonesList.Add(new SmartphoneListItem
                {
                    Id           = id,
                    Name         = smartphone.Name ?? string.Empty,
                    Year         = year,
                    Manufacturer = smartphone.Company ?? string.Empty
                });
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading smartphones from API");
        }
    }

    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(SmartphonesPage));

        return Task.CompletedTask;
    }

    private Task NavigateToSmartphoneAsync(SmartphoneListItem? smartphone)
    {
        if(smartphone is null) return Task.CompletedTask;

        _logger.LogInformation("Navigating to smartphone detail: {SmartphoneName} (ID: {SmartphoneId})",
                               smartphone.Name,
                               smartphone.Id);

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineId, smartphone.Id },
            { NavParamKeys.NavigationSource, nameof(SmartphonesListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);

        return Task.CompletedTask;
    }
}
