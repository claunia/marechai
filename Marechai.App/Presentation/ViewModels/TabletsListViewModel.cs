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
///     ViewModel for displaying a filtered list of Tablets
/// </summary>
public partial class TabletsListViewModel : ObservableObject
{
    private readonly TabletsService                _tabletsService;
    private readonly ITabletsListFilterContext     _filterContext;
    private readonly IStringLocalizer               _localizer;
    private readonly ILogger<TabletsListViewModel> _logger;
    private readonly IRegionManager                 _regionManager;

    [ObservableProperty]
    private ObservableCollection<TabletListItem> _tabletsList = [];

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

    public TabletsListViewModel(TabletsService                tabletsService, IStringLocalizer localizer,
                                ILogger<TabletsListViewModel> logger,         IRegionManager   regionManager,
                                ITabletsListFilterContext     filterContext)
    {
        _tabletsService         = tabletsService;
        _localizer              = localizer;
        _logger                 = logger;
        _regionManager          = regionManager;
        _filterContext          = filterContext;
        LoadData                = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand           = new AsyncRelayCommand(GoBackAsync);
        NavigateToTabletCommand = new AsyncRelayCommand<TabletListItem>(NavigateToTabletAsync);
    }

    public IAsyncRelayCommand                 LoadData                { get; }
    public ICommand                           GoBackCommand           { get; }
    public IAsyncRelayCommand<TabletListItem> NavigateToTabletCommand { get; }

    /// <summary>
    ///     Gets or sets the filter type
    /// </summary>
    public TabletListFilterType FilterType
    {
        get => _filterContext.FilterType;
        set => _filterContext.FilterType = value;
    }

    /// <summary>
    ///     Gets or sets the filter value
    /// </summary>
    public string FilterValue
    {
        get => _filterContext.FilterValue;
        set => _filterContext.FilterValue = value;
    }

    /// <summary>
    ///     Loads Tablets based on the current filter
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            TabletsList.Clear();

            _logger.LogInformation("LoadDataAsync called. FilterType={FilterType}, FilterValue={FilterValue}",
                                   FilterType,
                                   FilterValue);

            // Update title and filter description based on filter type
            UpdateFilterDescription();

            // Load Tablets from the API based on the current filter
            await LoadTabletsFromApiAsync();

            _logger.LogInformation("LoadTabletsFromApiAsync completed. TabletsList.Count={Count}",
                                   TabletsList.Count);

            if(TabletsList.Count == 0)
            {
                ErrorMessage = _localizer["No tablets found for this filter"].Value;
                HasError     = true;

                _logger.LogWarning("No tablets found for filter: {FilterType} {FilterValue}", FilterType, FilterValue);
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading tablets: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load tablets. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Updates the title and filter description based on the current filter
    /// </summary>
    private void UpdateFilterDescription()
    {
        switch(FilterType)
        {
            case TabletListFilterType.All:
                PageTitle         = _localizer["All Tablets"];
                FilterDescription = _localizer["Browsing all tablets in the database"];

                break;

            case TabletListFilterType.Letter:
                if(!string.IsNullOrEmpty(FilterValue) && FilterValue.Length == 1)
                {
                    PageTitle         = $"{_localizer["Tablets Starting with"]} {FilterValue}";
                    FilterDescription = $"{_localizer["Showing tablets that start with"]} {FilterValue}";
                }

                break;

            case TabletListFilterType.Year:
                if(!string.IsNullOrEmpty(FilterValue) && int.TryParse(FilterValue, out int year))
                {
                    PageTitle         = $"{_localizer["Tablets from"]} {year}";
                    FilterDescription = $"{_localizer["Showing tablets released in"]} {year}";
                }

                break;

            case TabletListFilterType.Prototype:
                PageTitle         = _localizer["Prototype Tablets"];
                FilterDescription = _localizer["Showing prototype tablets"];

                break;
        }
    }

    /// <summary>
    ///     Loads Tablets from the API based on the current filter
    /// </summary>
    private async Task LoadTabletsFromApiAsync()
    {
        try
        {
            List<MachineDto> tablets = FilterType switch
                                        {
                                            TabletListFilterType.Letter when FilterValue.Length == 1 =>
                                                await _tabletsService.GetTabletsByLetterAsync(FilterValue[0]),

                                            TabletListFilterType.Year when int.TryParse(FilterValue, out int year) =>
                                                await _tabletsService.GetTabletsByYearAsync(year),

                                            TabletListFilterType.Prototype =>
                                                await _tabletsService.GetPrototypesAsync(),

                                            _ => await _tabletsService.GetAllTabletsAsync()
                                        };

            // Add tablets to the list sorted by name
            foreach(MachineDto tablet in tablets.OrderBy(t => t.Name))
            {
                int year = tablet.Introduced?.Year ?? 0;
                int id   = tablet.Id               ?? 0;

                _logger.LogInformation("Tablet: {Name}, Introduced: {Introduced}, Year: {Year}, Company: {Company}, ID: {Id}",
                                       tablet.Name,
                                       tablet.Introduced,
                                       year,
                                       tablet.Company,
                                       id);

                TabletsList.Add(new TabletListItem
                {
                    Id           = id,
                    Name         = tablet.Name ?? string.Empty,
                    Year         = year,
                    Manufacturer = tablet.Company ?? string.Empty
                });
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading tablets from API");
        }
    }

    /// <summary>
    ///     Navigates back to the Tablets main view
    /// </summary>
    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(TabletsPage));

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to the Tablet detail view
    /// </summary>
    private Task NavigateToTabletAsync(TabletListItem? tablet)
    {
        if(tablet is null) return Task.CompletedTask;

        _logger.LogInformation("Navigating to tablet detail: {TabletName} (ID: {TabletId})",
                               tablet.Name,
                               tablet.Id);

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineId, tablet.Id },
            { NavParamKeys.NavigationSource, nameof(TabletsListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);

        return Task.CompletedTask;
    }
}
