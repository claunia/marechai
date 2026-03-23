using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;

namespace Marechai.App.Presentation.ViewModels;

public partial class BooksViewModel : ObservableObject
{
    private readonly BooksService              _booksService;
    private readonly IBooksListFilterContext    _filterContext;
    private readonly IStringLocalizer           _localizer;
    private readonly ILogger<BooksViewModel>    _logger;
    private readonly IRegionManager             _regionManager;

    [ObservableProperty]
    private int _bookCount;

    [ObservableProperty]
    private string _bookCountText = string.Empty;

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

    public BooksViewModel(BooksService              booksService, IStringLocalizer localizer,
                          ILogger<BooksViewModel>    logger,       IRegionManager   regionManager,
                          IBooksListFilterContext    filterContext)
    {
        _booksService   = booksService;
        _localizer      = localizer;
        _logger         = logger;
        _regionManager  = regionManager;
        _filterContext  = filterContext;
        LoadData                = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand           = new AsyncRelayCommand(GoBackAsync);
        NavigateByLetterCommand = new AsyncRelayCommand<char>(NavigateByLetterAsync);
        NavigateByYearCommand   = new AsyncRelayCommand<int>(NavigateByYearAsync);
        NavigateAllBooksCommand = new AsyncRelayCommand(NavigateAllBooksAsync);
        Title = _localizer["Books"];

        InitializeLetters();
    }

    public IAsyncRelayCommand       LoadData                { get; }
    public ICommand                 GoBackCommand           { get; }
    public IAsyncRelayCommand<char> NavigateByLetterCommand { get; }
    public IAsyncRelayCommand<int>  NavigateByYearCommand   { get; }
    public IAsyncRelayCommand       NavigateAllBooksCommand { get; }
    public string                   Title                   { get; }

    private void InitializeLetters()
    {
        LettersList.Clear();

        for(var c = 'A'; c <= 'Z'; c++) LettersList.Add(c);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            YearsList.Clear();

            Task<int> countTask   = _booksService.GetBooksCountAsync();
            Task<int> minYearTask = _booksService.GetMinimumYearAsync();
            Task<int> maxYearTask = _booksService.GetMaximumYearAsync();
            await Task.WhenAll(countTask, minYearTask, maxYearTask);

            BookCount   = countTask.Result;
            MinimumYear = minYearTask.Result;
            MaximumYear = maxYearTask.Result;

            BookCountText = _localizer["Books in the database"];

            if(MinimumYear > 0 && MaximumYear > 0)
            {
                for(int year = MinimumYear; year <= MaximumYear; year++) YearsList.Add(year);

                YearsGridTitle = string.Format(_localizer["Browse by Year ({0} - {1})"], MinimumYear, MaximumYear);
            }

            if(BookCount == 0)
            {
                ErrorMessage = _localizer["No books found"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading books data: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load books data. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));
    }

    private Task NavigateByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Navigating to books by letter: {Letter}", letter);
            _filterContext.FilterType  = BookListFilterType.Letter;
            _filterContext.FilterValue = letter.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(BooksListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to letter books: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Navigating to books by year: {Year}", year);
            _filterContext.FilterType  = BookListFilterType.Year;
            _filterContext.FilterValue = year.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(BooksListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to year books: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateAllBooksAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to all books");
            _filterContext.FilterType  = BookListFilterType.All;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(BooksListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to all books: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }
}
