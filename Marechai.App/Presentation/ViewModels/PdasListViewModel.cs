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
///     ViewModel for displaying a filtered list of PDAs
/// </summary>
public partial class PdasListViewModel : ObservableObject
{
    private readonly PdasService                _pdasService;
    private readonly IPdasListFilterContext     _filterContext;
    private readonly IStringLocalizer           _localizer;
    private readonly ILogger<PdasListViewModel> _logger;
    private readonly IRegionManager             _regionManager;

    [ObservableProperty]
    private ObservableCollection<PdaListItem> _pdasList = [];

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

    public PdasListViewModel(PdasService                pdasService, IStringLocalizer localizer,
                             ILogger<PdasListViewModel> logger,      IRegionManager   regionManager,
                             IPdasListFilterContext     filterContext)
    {
        _pdasService          = pdasService;
        _localizer            = localizer;
        _logger               = logger;
        _regionManager        = regionManager;
        _filterContext        = filterContext;
        LoadData              = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand         = new AsyncRelayCommand(GoBackAsync);
        NavigateToPdaCommand  = new AsyncRelayCommand<PdaListItem>(NavigateToPdaAsync);
    }

    public IAsyncRelayCommand              LoadData             { get; }
    public ICommand                        GoBackCommand        { get; }
    public IAsyncRelayCommand<PdaListItem> NavigateToPdaCommand { get; }

    /// <summary>
    ///     Gets or sets the filter type
    /// </summary>
    public PdaListFilterType FilterType
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
    ///     Loads PDAs based on the current filter
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            PdasList.Clear();

            _logger.LogInformation("LoadDataAsync called. FilterType={FilterType}, FilterValue={FilterValue}",
                                   FilterType,
                                   FilterValue);

            // Update title and filter description based on filter type
            UpdateFilterDescription();

            // Load PDAs from the API based on the current filter
            await LoadPdasFromApiAsync();

            _logger.LogInformation("LoadPdasFromApiAsync completed. PdasList.Count={Count}",
                                   PdasList.Count);

            if(PdasList.Count == 0)
            {
                ErrorMessage = _localizer["No pdas found for this filter"].Value;
                HasError     = true;

                _logger.LogWarning("No pdas found for filter: {FilterType} {FilterValue}", FilterType, FilterValue);
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading pdas: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load pdas. Please try again later."].Value;
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
            case PdaListFilterType.All:
                PageTitle         = _localizer["All Pdas"];
                FilterDescription = _localizer["Browsing all pdas in the database"];

                break;

            case PdaListFilterType.Letter:
                if(!string.IsNullOrEmpty(FilterValue) && FilterValue.Length == 1)
                {
                    PageTitle         = $"{_localizer["Pdas Starting with"]} {FilterValue}";
                    FilterDescription = $"{_localizer["Showing pdas that start with"]} {FilterValue}";
                }

                break;

            case PdaListFilterType.Year:
                if(!string.IsNullOrEmpty(FilterValue) && int.TryParse(FilterValue, out int year))
                {
                    PageTitle         = $"{_localizer["Pdas from"]} {year}";
                    FilterDescription = $"{_localizer["Showing pdas released in"]} {year}";
                }

                break;

            case PdaListFilterType.Prototype:
                PageTitle         = _localizer["Prototype Pdas"];
                FilterDescription = _localizer["Showing prototype pdas"];

                break;
        }
    }

    /// <summary>
    ///     Loads PDAs from the API based on the current filter
    /// </summary>
    private async Task LoadPdasFromApiAsync()
    {
        try
        {
            List<MachineDto> pdas = FilterType switch
                                     {
                                         PdaListFilterType.Letter when FilterValue.Length == 1 =>
                                             await _pdasService.GetPdasByLetterAsync(FilterValue[0]),

                                         PdaListFilterType.Year when int.TryParse(FilterValue, out int year) =>
                                             await _pdasService.GetPdasByYearAsync(year),

                                         PdaListFilterType.Prototype =>
                                             await _pdasService.GetPrototypesAsync(),

                                         _ => await _pdasService.GetAllPdasAsync()
                                     };

            // Add pdas to the list sorted by name
            foreach(MachineDto pda in pdas.OrderBy(p => p.Name))
            {
                int year = pda.Introduced?.Year ?? 0;
                int id   = pda.Id               ?? 0;

                _logger.LogInformation("Pda: {Name}, Introduced: {Introduced}, Year: {Year}, Company: {Company}, ID: {Id}",
                                       pda.Name,
                                       pda.Introduced,
                                       year,
                                       pda.Company,
                                       id);

                PdasList.Add(new PdaListItem
                {
                    Id           = id,
                    Name         = pda.Name ?? string.Empty,
                    Year         = year,
                    Manufacturer = pda.Company ?? string.Empty
                });
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading pdas from API");
        }
    }

    /// <summary>
    ///     Navigates back to the PDAs main view
    /// </summary>
    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(PdasPage));

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to the PDA detail view
    /// </summary>
    private Task NavigateToPdaAsync(PdaListItem? pda)
    {
        if(pda is null) return Task.CompletedTask;

        _logger.LogInformation("Navigating to pda detail: {PdaName} (ID: {PdaId})",
                               pda.Name,
                               pda.Id);

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineId, pda.Id },
            { NavParamKeys.NavigationSource, nameof(PdasListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);

        return Task.CompletedTask;
    }
}
