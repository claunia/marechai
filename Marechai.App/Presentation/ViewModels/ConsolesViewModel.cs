using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;

namespace Marechai.App.Presentation.ViewModels;

public partial class ConsolesViewModel : ObservableObject
{
    private readonly ConsolesService            _consolesService;
    private readonly IConsolesListFilterContext _filterContext;
    private readonly IStringLocalizer           _localizer;
    private readonly ILogger<ConsolesViewModel> _logger;
    private readonly IRegionManager             _regionManager;

    [ObservableProperty]
    private int _consoleCount;

    [ObservableProperty]
    private string _consoleCountText = string.Empty;

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

    public ConsolesViewModel(ConsolesService            consolesService, IStringLocalizer localizer,
                             ILogger<ConsolesViewModel> logger,          IRegionManager   regionManager,
                             IConsolesListFilterContext filterContext)
    {
        _consolesService           = consolesService;
        _localizer                 = localizer;
        _logger                    = logger;
        _regionManager             = regionManager;
        _filterContext             = filterContext;
        LoadData                   = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand              = new AsyncRelayCommand(GoBackAsync);
        NavigateByLetterCommand    = new AsyncRelayCommand<char>(NavigateByLetterAsync);
        NavigateByYearCommand      = new AsyncRelayCommand<int>(NavigateByYearAsync);
        NavigateAllConsolesCommand = new AsyncRelayCommand(NavigateAllConsolesAsync);
        NavigatePrototypesCommand  = new AsyncRelayCommand(NavigatePrototypesAsync);
        Title = _localizer["Consoles"];

        InitializeLetters();
    }

    public IAsyncRelayCommand       LoadData                   { get; }
    public ICommand                 GoBackCommand              { get; }
    public IAsyncRelayCommand<char> NavigateByLetterCommand    { get; }
    public IAsyncRelayCommand<int>  NavigateByYearCommand      { get; }
    public IAsyncRelayCommand       NavigateAllConsolesCommand { get; }
    public IAsyncRelayCommand       NavigatePrototypesCommand  { get; }
    public string                   Title                      { get; }

    /// <summary>
    ///     Initializes the alphabet list (A-Z)
    /// </summary>
    private void InitializeLetters()
    {
        LettersList.Clear();

        for(var c = 'A'; c <= 'Z'; c++) LettersList.Add(c);
    }

    /// <summary>
    ///     Loads consoles count, minimum and maximum years from the API
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
            Task<int> countTask   = _consolesService.GetConsolesCountAsync();
            Task<int> minYearTask = _consolesService.GetMinimumYearAsync();
            Task<int> maxYearTask = _consolesService.GetMaximumYearAsync();
            await Task.WhenAll(countTask, minYearTask, maxYearTask);

            ConsoleCount = countTask.Result;
            MinimumYear  = minYearTask.Result;
            MaximumYear  = maxYearTask.Result;

            // Update display text
            ConsoleCountText = _localizer["Consoles in the database"];

            // Generate years list
            if(MinimumYear > 0 && MaximumYear > 0)
            {
                for(int year = MinimumYear; year <= MaximumYear; year++) YearsList.Add(year);

                YearsGridTitle = string.Format(_localizer["Browse by Year ({0} - {1})"], MinimumYear, MaximumYear);
            }

            if(ConsoleCount == 0)
            {
                ErrorMessage = _localizer["No consoles found"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading consoles data: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load consoles data. Please try again later."].Value;
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
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to consoles filtered by letter
    /// </summary>
    private Task NavigateByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Navigating to consoles by letter: {Letter}", letter);
            _filterContext.FilterType  = ConsoleListFilterType.Letter;
            _filterContext.FilterValue = letter.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(ConsolesListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to letter consoles: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to consoles filtered by year
    /// </summary>
    private Task NavigateByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Navigating to consoles by year: {Year}", year);
            _filterContext.FilterType  = ConsoleListFilterType.Year;
            _filterContext.FilterValue = year.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(ConsolesListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to year consoles: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to all consoles view
    /// </summary>
    private Task NavigateAllConsolesAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to all consoles");
            _filterContext.FilterType  = ConsoleListFilterType.All;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(ConsolesListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to all consoles: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to prototype consoles view
    /// </summary>
    private Task NavigatePrototypesAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to prototype consoles");
            _filterContext.FilterType  = ConsoleListFilterType.Prototype;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(ConsolesListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to prototype consoles: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }
}
