using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;

namespace Marechai.App.Presentation.ViewModels;

public partial class ComputersViewModel : ObservableObject
{
    private readonly ComputersService            _computersService;
    private readonly IComputersListFilterContext _filterContext;
    private readonly IStringLocalizer            _localizer;
    private readonly ILogger<ComputersViewModel> _logger;
    private readonly IRegionManager              _regionManager;

    [ObservableProperty]
    private int _computerCount;

    [ObservableProperty]
    private string _computerCountText = string.Empty;

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

    public ComputersViewModel(ComputersService            computersService, IStringLocalizer localizer,
                              ILogger<ComputersViewModel> logger,           IRegionManager   regionManager,
                              IComputersListFilterContext filterContext)
    {
        _computersService           = computersService;
        _localizer                  = localizer;
        _logger                     = logger;
        _regionManager              = regionManager;
        _filterContext              = filterContext;
        LoadData                    = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand               = new AsyncRelayCommand(GoBackAsync);
        NavigateByLetterCommand     = new AsyncRelayCommand<char>(NavigateByLetterAsync);
        NavigateByYearCommand       = new AsyncRelayCommand<int>(NavigateByYearAsync);
        NavigateAllComputersCommand = new AsyncRelayCommand(NavigateAllComputersAsync);
        NavigatePrototypesCommand   = new AsyncRelayCommand(NavigatePrototypesAsync);
        Title = _localizer["Computers"];

        InitializeLetters();
    }

    public IAsyncRelayCommand       LoadData                    { get; }
    public ICommand                 GoBackCommand               { get; }
    public IAsyncRelayCommand<char> NavigateByLetterCommand     { get; }
    public IAsyncRelayCommand<int>  NavigateByYearCommand       { get; }
    public IAsyncRelayCommand       NavigateAllComputersCommand { get; }
    public IAsyncRelayCommand       NavigatePrototypesCommand   { get; }
    public string                   Title                       { get; }

    /// <summary>
    ///     Initializes the alphabet list (A-Z)
    /// </summary>
    private void InitializeLetters()
    {
        LettersList.Clear();

        for(var c = 'A'; c <= 'Z'; c++) LettersList.Add(c);
    }

    /// <summary>
    ///     Loads computers count, minimum and maximum years from the API
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            YearsList.Clear();

            // Load all data in parallel for better performance
            Task<int> countTask   = _computersService.GetComputersCountAsync();
            Task<int> minYearTask = _computersService.GetMinimumYearAsync();
            Task<int> maxYearTask = _computersService.GetMaximumYearAsync();
            await Task.WhenAll(countTask, minYearTask, maxYearTask);

            ComputerCount = countTask.Result;
            MinimumYear   = minYearTask.Result;
            MaximumYear   = maxYearTask.Result;

            // Update display text
            ComputerCountText = _localizer["Computers in the database"];

            // Generate years list
            if(MinimumYear > 0 && MaximumYear > 0)
            {
                for(int year = MinimumYear; year <= MaximumYear; year++) YearsList.Add(year);

                YearsGridTitle = string.Format(_localizer["Browse by Year ({0} - {1})"], MinimumYear, MaximumYear);
            }

            if(ComputerCount == 0)
            {
                ErrorMessage = _localizer["No computers found"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading computers data: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load computers data. Please try again later."].Value;
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
    private async Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));
    }

    /// <summary>
    ///     Navigates to computers filtered by letter
    /// </summary>
    private Task NavigateByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Navigating to computers by letter: {Letter}", letter);
            _filterContext.FilterType  = ComputerListFilterType.Letter;
            _filterContext.FilterValue = letter.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(ComputersListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to letter computers: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to computers filtered by year
    /// </summary>
    private Task NavigateByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Navigating to computers by year: {Year}", year);
            _filterContext.FilterType  = ComputerListFilterType.Year;
            _filterContext.FilterValue = year.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(ComputersListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to year computers: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to all computers view
    /// </summary>
    private Task NavigateAllComputersAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to all computers");
            _filterContext.FilterType  = ComputerListFilterType.All;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(ComputersListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to all computers: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to prototype computers view
    /// </summary>
    private Task NavigatePrototypesAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to prototype computers");
            _filterContext.FilterType  = ComputerListFilterType.Prototype;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(ComputersListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to prototype computers: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }
}