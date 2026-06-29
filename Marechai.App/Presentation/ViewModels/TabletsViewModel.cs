using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;

namespace Marechai.App.Presentation.ViewModels;

public partial class TabletsViewModel : ObservableObject
{
    private readonly TabletsService            _tabletsService;
    private readonly ITabletsListFilterContext _filterContext;
    private readonly IStringLocalizer           _localizer;
    private readonly ILogger<TabletsViewModel> _logger;
    private readonly IRegionManager             _regionManager;

    [ObservableProperty]
    private int _tabletCount;

    [ObservableProperty]
    private string _tabletCountText = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<char> _lettersList = [];

    [ObservableProperty]
    private int _maximumYear;

    [ObservableProperty]
    private int _minimumYear;

    [ObservableProperty]
    private string _yearsGridTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<int> _yearsList = [];

    public TabletsViewModel(TabletsService            tabletsService, IStringLocalizer localizer,
                            ILogger<TabletsViewModel> logger,         IRegionManager   regionManager,
                            ITabletsListFilterContext filterContext)
    {
        _tabletsService           = tabletsService;
        _localizer                = localizer;
        _logger                   = logger;
        _regionManager            = regionManager;
        _filterContext            = filterContext;
        LoadData                  = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand             = new AsyncRelayCommand(GoBackAsync);
        NavigateByLetterCommand   = new AsyncRelayCommand<char>(NavigateByLetterAsync);
        NavigateByYearCommand     = new AsyncRelayCommand<int>(NavigateByYearAsync);
        NavigateAllTabletsCommand = new AsyncRelayCommand(NavigateAllTabletsAsync);
        NavigatePrototypesCommand = new AsyncRelayCommand(NavigatePrototypesAsync);
        Title = _localizer["Tablets"];

        InitializeLetters();
    }

    public IAsyncRelayCommand       LoadData                  { get; }
    public ICommand                 GoBackCommand             { get; }
    public IAsyncRelayCommand<char> NavigateByLetterCommand   { get; }
    public IAsyncRelayCommand<int>  NavigateByYearCommand     { get; }
    public IAsyncRelayCommand       NavigateAllTabletsCommand { get; }
    public IAsyncRelayCommand       NavigatePrototypesCommand { get; }
    public string                   Title                     { get; }

    /// <summary>
    ///     Initializes the alphabet list (A-Z)
    /// </summary>
    private void InitializeLetters()
    {
        LettersList.Clear();

        for(var c = 'A'; c <= 'Z'; c++) LettersList.Add(c);
    }

    /// <summary>
    ///     Loads Tablets count, minimum and maximum years from the API
    /// </summary>
    private async Task LoadDataAsync()
    {
        if(IsLoading) return;

        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            YearsList.Clear();

            // Load all data in parallel for better performance
            Task<int> countTask   = _tabletsService.GetTabletsCountAsync();
            Task<int> minYearTask = _tabletsService.GetMinimumYearAsync();
            Task<int> maxYearTask = _tabletsService.GetMaximumYearAsync();
            await Task.WhenAll(countTask, minYearTask, maxYearTask);

            TabletCount = countTask.Result;
            MinimumYear = minYearTask.Result;
            MaximumYear = maxYearTask.Result;

            // Update display text
            TabletCountText = _localizer["Tablets in the database"];

            // Generate years list
            if(MinimumYear > 0 && MaximumYear > 0)
            {
                for(int year = MinimumYear; year <= MaximumYear; year++) YearsList.Add(year);

                YearsGridTitle = string.Format(_localizer["Browse by Year ({0} - {1})"], MinimumYear, MaximumYear);
            }

            if(TabletCount == 0)
            {
                ErrorMessage = _localizer["No tablets found"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading Tablets data: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load tablets data. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Handles back navigation
    /// </summary>
    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to Tablets filtered by letter
    /// </summary>
    private Task NavigateByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Navigating to Tablets by letter: {Letter}", letter);
            _filterContext.FilterType  = TabletListFilterType.Letter;
            _filterContext.FilterValue = letter.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(TabletsListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to letter Tablets: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to Tablets filtered by year
    /// </summary>
    private Task NavigateByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Navigating to Tablets by year: {Year}", year);
            _filterContext.FilterType  = TabletListFilterType.Year;
            _filterContext.FilterValue = year.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(TabletsListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to year Tablets: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to all Tablets view
    /// </summary>
    private Task NavigateAllTabletsAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to all Tablets");
            _filterContext.FilterType  = TabletListFilterType.All;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(TabletsListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to all Tablets: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to prototype Tablets view
    /// </summary>
    private Task NavigatePrototypesAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to prototype Tablets");
            _filterContext.FilterType  = TabletListFilterType.Prototype;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(TabletsListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to prototype Tablets: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }
}
