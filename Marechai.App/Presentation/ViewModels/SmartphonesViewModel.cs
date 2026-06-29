using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;

namespace Marechai.App.Presentation.ViewModels;

public partial class SmartphonesViewModel : ObservableObject
{
    private readonly SmartphonesService            _smartphonesService;
    private readonly ISmartphonesListFilterContext _filterContext;
    private readonly IStringLocalizer              _localizer;
    private readonly ILogger<SmartphonesViewModel> _logger;
    private readonly IRegionManager                _regionManager;

    [ObservableProperty]
    private int _smartphoneCount;

    [ObservableProperty]
    private string _smartphoneCountText = string.Empty;

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

    public SmartphonesViewModel(SmartphonesService            smartphonesService, IStringLocalizer localizer,
                                ILogger<SmartphonesViewModel> logger,             IRegionManager   regionManager,
                                ISmartphonesListFilterContext filterContext)
    {
        _smartphonesService            = smartphonesService;
        _localizer                     = localizer;
        _logger                        = logger;
        _regionManager                 = regionManager;
        _filterContext                 = filterContext;
        LoadData                       = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand                  = new AsyncRelayCommand(GoBackAsync);
        NavigateByLetterCommand        = new AsyncRelayCommand<char>(NavigateByLetterAsync);
        NavigateByYearCommand          = new AsyncRelayCommand<int>(NavigateByYearAsync);
        NavigateAllSmartphonesCommand  = new AsyncRelayCommand(NavigateAllSmartphonesAsync);
        NavigatePrototypesCommand      = new AsyncRelayCommand(NavigatePrototypesAsync);
        Title = _localizer["Smartphones"];

        InitializeLetters();
    }

    public IAsyncRelayCommand       LoadData                      { get; }
    public ICommand                 GoBackCommand                 { get; }
    public IAsyncRelayCommand<char> NavigateByLetterCommand       { get; }
    public IAsyncRelayCommand<int>  NavigateByYearCommand         { get; }
    public IAsyncRelayCommand       NavigateAllSmartphonesCommand { get; }
    public IAsyncRelayCommand       NavigatePrototypesCommand     { get; }
    public string                   Title                         { get; }

    private void InitializeLetters()
    {
        LettersList.Clear();

        for(var c = 'A'; c <= 'Z'; c++) LettersList.Add(c);
    }

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

            Task<int> countTask   = _smartphonesService.GetSmartphonesCountAsync();
            Task<int> minYearTask = _smartphonesService.GetMinimumYearAsync();
            Task<int> maxYearTask = _smartphonesService.GetMaximumYearAsync();
            await Task.WhenAll(countTask, minYearTask, maxYearTask);

            SmartphoneCount = countTask.Result;
            MinimumYear     = minYearTask.Result;
            MaximumYear     = maxYearTask.Result;

            SmartphoneCountText = _localizer["Smartphones in the database"];

            if(MinimumYear > 0 && MaximumYear > 0)
            {
                for(int year = MinimumYear; year <= MaximumYear; year++) YearsList.Add(year);

                YearsGridTitle = string.Format(_localizer["Browse by Year ({0} - {1})"], MinimumYear, MaximumYear);
            }

            if(SmartphoneCount == 0)
            {
                ErrorMessage = _localizer["No smartphones found"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading smartphones data: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load smartphones data. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task GoBackAsync()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));

        return Task.CompletedTask;
    }

    private Task NavigateByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Navigating to smartphones by letter: {Letter}", letter);
            _filterContext.FilterType  = SmartphoneListFilterType.Letter;
            _filterContext.FilterValue = letter.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SmartphonesListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to letter smartphones: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Navigating to smartphones by year: {Year}", year);
            _filterContext.FilterType  = SmartphoneListFilterType.Year;
            _filterContext.FilterValue = year.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SmartphonesListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to year smartphones: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateAllSmartphonesAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to all smartphones");
            _filterContext.FilterType  = SmartphoneListFilterType.All;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SmartphonesListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to all smartphones: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to prototype smartphones view
    /// </summary>
    private Task NavigatePrototypesAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to prototype smartphones");
            _filterContext.FilterType  = SmartphoneListFilterType.Prototype;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SmartphonesListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to prototype smartphones: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }
}
