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
///     ViewModel for displaying a filtered list of computers
/// </summary>
public partial class ComputersListViewModel : ObservableObject
{
    private readonly ComputersService                _computersService;
    private readonly IComputersListFilterContext     _filterContext;
    private readonly IStringLocalizer                _localizer;
    private readonly ILogger<ComputersListViewModel> _logger;
    private readonly IRegionManager                  _regionManager;

    [ObservableProperty]
    private ObservableCollection<ComputerListItem> _computersList = [];

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

    public ComputersListViewModel(ComputersService                computersService, IStringLocalizer localizer,
                                  ILogger<ComputersListViewModel> logger,           IRegionManager   regionManager,
                                  IComputersListFilterContext     filterContext)
    {
        _computersService         = computersService;
        _localizer                = localizer;
        _logger                   = logger;
        _regionManager            = regionManager;
        _filterContext            = filterContext;
        LoadData                  = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand             = new AsyncRelayCommand(GoBackAsync);
        NavigateToComputerCommand = new AsyncRelayCommand<ComputerListItem>(NavigateToComputerAsync);
    }

    public IAsyncRelayCommand                   LoadData                  { get; }
    public ICommand                             GoBackCommand             { get; }
    public IAsyncRelayCommand<ComputerListItem> NavigateToComputerCommand { get; }

    /// <summary>
    ///     Gets or sets the filter type
    /// </summary>
    public ComputerListFilterType FilterType
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
    ///     Loads computers based on the current filter
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            ComputersList.Clear();

            _logger.LogInformation("LoadDataAsync called. FilterType={FilterType}, FilterValue={FilterValue}",
                                   FilterType,
                                   FilterValue);

            // Update title and filter description based on filter type
            UpdateFilterDescription();

            // Load computers from the API based on the current filter
            await LoadComputersFromApiAsync();

            _logger.LogInformation("LoadComputersFromApiAsync completed. ComputersList.Count={Count}",
                                   ComputersList.Count);

            if(ComputersList.Count == 0)
            {
                ErrorMessage = _localizer["No computers found for this filter"].Value;
                HasError     = true;

                _logger.LogWarning("No computers found for filter: {FilterType} {FilterValue}",
                                   FilterType,
                                   FilterValue);
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading computers: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load computers. Please try again later."].Value;
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
            case ComputerListFilterType.All:
                PageTitle         = _localizer["All Computers"];
                FilterDescription = _localizer["Browsing all computers in the database"];

                break;

            case ComputerListFilterType.Letter:
                if(!string.IsNullOrEmpty(FilterValue) && FilterValue.Length == 1)
                {
                    PageTitle         = $"{_localizer["Computers Starting with"]} {FilterValue}";
                    FilterDescription = $"{_localizer["Showing computers that start with"]} {FilterValue}";
                }

                break;

            case ComputerListFilterType.Year:
                if(!string.IsNullOrEmpty(FilterValue) && int.TryParse(FilterValue, out int year))
                {
                    PageTitle         = $"{_localizer["Computers from"]} {year}";
                    FilterDescription = $"{_localizer["Showing computers released in"]} {year}";
                }

                break;
        }
    }

    /// <summary>
    ///     Loads computers from the API based on the current filter
    /// </summary>
    private async Task LoadComputersFromApiAsync()
    {
        try
        {
            List<MachineDto> computers = FilterType switch
                                         {
                                             ComputerListFilterType.Letter when FilterValue.Length == 1 =>
                                                 await _computersService.GetComputersByLetterAsync(FilterValue[0]),

                                             ComputerListFilterType.Year when int.TryParse(FilterValue, out int year) =>
                                                 await _computersService.GetComputersByYearAsync(year),

                                             _ => await _computersService.GetAllComputersAsync()
                                         };

            // Add computers to the list sorted by name
            foreach(MachineDto computer in computers.OrderBy(c => c.Name))
            {
                int year = computer.Introduced?.Year ?? 0;
                int id   = computer.Id               ?? 0;

                _logger.LogInformation("Computer: {Name}, Introduced: {Introduced}, Year: {Year}, Company: {Company}, ID: {Id}",
                                       computer.Name,
                                       computer.Introduced,
                                       year,
                                       computer.Company,
                                       id);

                ComputersList.Add(new ComputerListItem
                {
                    Id           = id,
                    Name         = computer.Name ?? string.Empty,
                    Year         = year,
                    Manufacturer = computer.Company ?? string.Empty
                });
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading computers from API");
        }
    }

    /// <summary>
    ///     Navigates back to the computers main view
    /// </summary>
    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(ComputersPage));

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to the computer detail view
    /// </summary>
    private Task NavigateToComputerAsync(ComputerListItem? computer)
    {
        if(computer is null) return Task.CompletedTask;

        _logger.LogInformation("Navigating to computer detail: {ComputerName} (ID: {ComputerId})",
                               computer.Name,
                               computer.Id);

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineId, computer.Id },
            { NavParamKeys.NavigationSource, nameof(ComputersListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);

        return Task.CompletedTask;
    }
}