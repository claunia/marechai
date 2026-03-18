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
///     ViewModel for displaying a filtered list of consoles
/// </summary>
public partial class ConsolesListViewModel : ObservableObject
{
    private readonly ConsolesService                _consolesService;
    private readonly IConsolesListFilterContext     _filterContext;
    private readonly IStringLocalizer               _localizer;
    private readonly ILogger<ConsolesListViewModel> _logger;
    private readonly IRegionManager                 _regionManager;

    [ObservableProperty]
    private ObservableCollection<ConsoleListItem> _consolesList = [];

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

    public ConsolesListViewModel(ConsolesService                consolesService, IStringLocalizer localizer,
                                 ILogger<ConsolesListViewModel> logger,          IRegionManager    regionManager,
                                 IConsolesListFilterContext     filterContext)
    {
        _consolesService         = consolesService;
        _localizer               = localizer;
        _logger                  = logger;
        _regionManager           = regionManager;
        _filterContext           = filterContext;
        LoadData                 = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand            = new AsyncRelayCommand(GoBackAsync);
        NavigateToConsoleCommand = new AsyncRelayCommand<ConsoleListItem>(NavigateToConsoleAsync);
    }

    public IAsyncRelayCommand                  LoadData                 { get; }
    public ICommand                            GoBackCommand            { get; }
    public IAsyncRelayCommand<ConsoleListItem> NavigateToConsoleCommand { get; }

    /// <summary>
    ///     Gets or sets the filter type
    /// </summary>
    public ConsoleListFilterType FilterType
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
    ///     Loads consoles based on the current filter
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            ConsolesList.Clear();

            _logger.LogInformation("LoadDataAsync called. FilterType={FilterType}, FilterValue={FilterValue}",
                                   FilterType,
                                   FilterValue);

            // Update title and filter description based on filter type
            UpdateFilterDescription();

            // Load consoles from the API based on the current filter
            await LoadConsolesFromApiAsync();

            _logger.LogInformation("LoadConsolesFromApiAsync completed. ConsolesList.Count={Count}",
                                   ConsolesList.Count);

            if(ConsolesList.Count == 0)
            {
                ErrorMessage = _localizer["No consoles found for this filter"].Value;
                HasError     = true;

                _logger.LogWarning("No consoles found for filter: {FilterType} {FilterValue}", FilterType, FilterValue);
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading consoles: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load consoles. Please try again later."].Value;
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
            case ConsoleListFilterType.All:
                PageTitle         = _localizer["All Consoles"];
                FilterDescription = _localizer["Browsing all consoles in the database"];

                break;

            case ConsoleListFilterType.Letter:
                if(!string.IsNullOrEmpty(FilterValue) && FilterValue.Length == 1)
                {
                    PageTitle         = $"{_localizer["Consoles Starting with"]} {FilterValue}";
                    FilterDescription = $"{_localizer["Showing consoles that start with"]} {FilterValue}";
                }

                break;

            case ConsoleListFilterType.Year:
                if(!string.IsNullOrEmpty(FilterValue) && int.TryParse(FilterValue, out int year))
                {
                    PageTitle         = $"{_localizer["Consoles from"]} {year}";
                    FilterDescription = $"{_localizer["Showing consoles released in"]} {year}";
                }

                break;
        }
    }

    /// <summary>
    ///     Loads consoles from the API based on the current filter
    /// </summary>
    private async Task LoadConsolesFromApiAsync()
    {
        try
        {
            List<MachineDto> consoles = FilterType switch
                                        {
                                            ConsoleListFilterType.Letter when FilterValue.Length == 1 =>
                                                await _consolesService.GetConsolesByLetterAsync(FilterValue[0]),

                                            ConsoleListFilterType.Year when int.TryParse(FilterValue, out int year) =>
                                                await _consolesService.GetConsolesByYearAsync(year),

                                            _ => await _consolesService.GetAllConsolesAsync()
                                        };

            // Add consoles to the list sorted by name
            foreach(MachineDto console in consoles.OrderBy(c => c.Name))
            {
                int year = console.Introduced?.Year ?? 0;
                int id   = console.Id               ?? 0;

                _logger.LogInformation("Console: {Name}, Introduced: {Introduced}, Year: {Year}, Company: {Company}, ID: {Id}",
                                       console.Name,
                                       console.Introduced,
                                       year,
                                       console.Company,
                                       id);

                ConsolesList.Add(new ConsoleListItem
                {
                    Id           = id,
                    Name         = console.Name ?? string.Empty,
                    Year         = year,
                    Manufacturer = console.Company ?? string.Empty
                });
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading consoles from API");
        }
    }

    /// <summary>
    ///     Navigates back to the consoles main view
    /// </summary>
    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(ConsolesPage));

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to the console detail view
    /// </summary>
    private Task NavigateToConsoleAsync(ConsoleListItem? console)
    {
        if(console is null) return Task.CompletedTask;

        _logger.LogInformation("Navigating to console detail: {ConsoleName} (ID: {ConsoleId})",
                               console.Name,
                               console.Id);

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineId, console.Id },
            { NavParamKeys.NavigationSource, nameof(ConsolesListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);

        return Task.CompletedTask;
    }
}