using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;

namespace Marechai.App.Presentation.ViewModels;

public partial class PdasViewModel : ObservableObject
{
    private readonly PdasService            _pdasService;
    private readonly IPdasListFilterContext _filterContext;
    private readonly IStringLocalizer       _localizer;
    private readonly ILogger<PdasViewModel> _logger;
    private readonly IRegionManager         _regionManager;

    [ObservableProperty]
    private int _pdaCount;

    [ObservableProperty]
    private string _pdaCountText = string.Empty;

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

    public PdasViewModel(PdasService            pdasService, IStringLocalizer localizer,
                         ILogger<PdasViewModel> logger,      IRegionManager   regionManager,
                         IPdasListFilterContext filterContext)
    {
        _pdasService            = pdasService;
        _localizer              = localizer;
        _logger                 = logger;
        _regionManager          = regionManager;
        _filterContext          = filterContext;
        LoadData                = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand           = new AsyncRelayCommand(GoBackAsync);
        NavigateByLetterCommand = new AsyncRelayCommand<char>(NavigateByLetterAsync);
        NavigateByYearCommand   = new AsyncRelayCommand<int>(NavigateByYearAsync);
        NavigateAllPdasCommand  = new AsyncRelayCommand(NavigateAllPdasAsync);
        NavigatePrototypesCommand = new AsyncRelayCommand(NavigatePrototypesAsync);
        Title = _localizer["Pdas"];

        InitializeLetters();
    }

    public IAsyncRelayCommand       LoadData                  { get; }
    public ICommand                 GoBackCommand             { get; }
    public IAsyncRelayCommand<char> NavigateByLetterCommand   { get; }
    public IAsyncRelayCommand<int>  NavigateByYearCommand     { get; }
    public IAsyncRelayCommand       NavigateAllPdasCommand    { get; }
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
    ///     Loads PDAs count, minimum and maximum years from the API
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
            Task<int> countTask   = _pdasService.GetPdasCountAsync();
            Task<int> minYearTask = _pdasService.GetMinimumYearAsync();
            Task<int> maxYearTask = _pdasService.GetMaximumYearAsync();
            await Task.WhenAll(countTask, minYearTask, maxYearTask);

            PdaCount    = countTask.Result;
            MinimumYear = minYearTask.Result;
            MaximumYear = maxYearTask.Result;

            // Update display text
            PdaCountText = _localizer["Pdas in the database"];

            // Generate years list
            if(MinimumYear > 0 && MaximumYear > 0)
            {
                for(int year = MinimumYear; year <= MaximumYear; year++) YearsList.Add(year);

                YearsGridTitle = string.Format(_localizer["Browse by Year ({0} - {1})"], MinimumYear, MaximumYear);
            }

            if(PdaCount == 0)
            {
                ErrorMessage = _localizer["No pdas found"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading PDAs data: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load pdas data. Please try again later."].Value;
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
    ///     Navigates to PDAs filtered by letter
    /// </summary>
    private Task NavigateByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Navigating to PDAs by letter: {Letter}", letter);
            _filterContext.FilterType  = PdaListFilterType.Letter;
            _filterContext.FilterValue = letter.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(PdasListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to letter PDAs: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to PDAs filtered by year
    /// </summary>
    private Task NavigateByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Navigating to PDAs by year: {Year}", year);
            _filterContext.FilterType  = PdaListFilterType.Year;
            _filterContext.FilterValue = year.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(PdasListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to year PDAs: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to all PDAs view
    /// </summary>
    private Task NavigateAllPdasAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to all PDAs");
            _filterContext.FilterType  = PdaListFilterType.All;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(PdasListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to all PDAs: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to prototype PDAs view
    /// </summary>
    private Task NavigatePrototypesAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to prototype PDAs");
            _filterContext.FilterType  = PdaListFilterType.Prototype;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(PdasListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to prototype PDAs: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }
}
