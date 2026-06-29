using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;

namespace Marechai.App.Presentation.ViewModels;

public partial class PeopleViewModel : ObservableObject
{
    private readonly IPeopleListFilterContext    _filterContext;
    private readonly IStringLocalizer           _localizer;
    private readonly ILogger<PeopleViewModel>   _logger;
    private readonly PeopleService              _peopleService;
    private readonly IRegionManager             _regionManager;

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
    private int _peopleCount;

    [ObservableProperty]
    private string _peopleCountText = string.Empty;

    [ObservableProperty]
    private string _yearsGridTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<int> _yearsList = [];

    public PeopleViewModel(PeopleService              peopleService, IStringLocalizer localizer,
                           ILogger<PeopleViewModel>   logger,        IRegionManager   regionManager,
                           IPeopleListFilterContext    filterContext)
    {
        _peopleService  = peopleService;
        _localizer      = localizer;
        _logger         = logger;
        _regionManager  = regionManager;
        _filterContext  = filterContext;
        LoadData                 = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand            = new AsyncRelayCommand(GoBackAsync);
        NavigateByLetterCommand  = new AsyncRelayCommand<char>(NavigateByLetterAsync);
        NavigateByYearCommand    = new AsyncRelayCommand<int>(NavigateByYearAsync);
        NavigateAllPeopleCommand = new AsyncRelayCommand(NavigateAllPeopleAsync);
        Title = _localizer["People"];

        InitializeLetters();
    }

    public IAsyncRelayCommand       LoadData                 { get; }
    public ICommand                 GoBackCommand            { get; }
    public IAsyncRelayCommand<char> NavigateByLetterCommand  { get; }
    public IAsyncRelayCommand<int>  NavigateByYearCommand    { get; }
    public IAsyncRelayCommand       NavigateAllPeopleCommand { get; }
    public string                   Title                    { get; }

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

            Task<int> countTask   = _peopleService.GetPeopleCountAsync();
            Task<int> minYearTask = _peopleService.GetMinimumBirthYearAsync();
            Task<int> maxYearTask = _peopleService.GetMaximumBirthYearAsync();
            await Task.WhenAll(countTask, minYearTask, maxYearTask);

            PeopleCount = countTask.Result;
            MinimumYear = minYearTask.Result;
            MaximumYear = maxYearTask.Result;

            PeopleCountText = _localizer["People in the database"];

            if(MinimumYear > 0 && MaximumYear > 0)
            {
                for(int year = MinimumYear; year <= MaximumYear; year++) YearsList.Add(year);

                YearsGridTitle = string.Format(_localizer["Browse by Year ({0} - {1})"], MinimumYear, MaximumYear);
            }

            if(PeopleCount == 0)
            {
                ErrorMessage = _localizer["No people found"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading people data: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load people data. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task GoBackAsync()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return;

        _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));
    }

    private Task NavigateByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Navigating to people by letter: {Letter}", letter);
            _filterContext.FilterType  = PeopleListFilterType.Letter;
            _filterContext.FilterValue = letter.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(PeopleListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to letter people: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Navigating to people by year: {Year}", year);
            _filterContext.FilterType  = PeopleListFilterType.Year;
            _filterContext.FilterValue = year.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(PeopleListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to year people: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateAllPeopleAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to all people");
            _filterContext.FilterType  = PeopleListFilterType.All;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(PeopleListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to all people: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }
}
