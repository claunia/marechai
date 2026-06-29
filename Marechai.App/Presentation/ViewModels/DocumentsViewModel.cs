using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;

namespace Marechai.App.Presentation.ViewModels;

public partial class DocumentsViewModel : ObservableObject
{
    private readonly DocumentsService              _documentsService;
    private readonly IDocumentsListFilterContext    _filterContext;
    private readonly IStringLocalizer              _localizer;
    private readonly ILogger<DocumentsViewModel>   _logger;
    private readonly IRegionManager                _regionManager;

    [ObservableProperty]
    private int _documentCount;

    [ObservableProperty]
    private string _documentCountText = string.Empty;

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

    public DocumentsViewModel(DocumentsService              documentsService, IStringLocalizer localizer,
                              ILogger<DocumentsViewModel>   logger,           IRegionManager   regionManager,
                              IDocumentsListFilterContext    filterContext)
    {
        _documentsService = documentsService;
        _localizer        = localizer;
        _logger           = logger;
        _regionManager    = regionManager;
        _filterContext    = filterContext;
        LoadData                    = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand               = new AsyncRelayCommand(GoBackAsync);
        NavigateByLetterCommand     = new AsyncRelayCommand<char>(NavigateByLetterAsync);
        NavigateByYearCommand       = new AsyncRelayCommand<int>(NavigateByYearAsync);
        NavigateAllDocumentsCommand = new AsyncRelayCommand(NavigateAllDocumentsAsync);
        Title = _localizer["Documents"];

        InitializeLetters();
    }

    public IAsyncRelayCommand       LoadData                    { get; }
    public ICommand                 GoBackCommand               { get; }
    public IAsyncRelayCommand<char> NavigateByLetterCommand     { get; }
    public IAsyncRelayCommand<int>  NavigateByYearCommand       { get; }
    public IAsyncRelayCommand       NavigateAllDocumentsCommand { get; }
    public string                   Title                       { get; }

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

            Task<int> countTask   = _documentsService.GetDocumentsCountAsync();
            Task<int> minYearTask = _documentsService.GetMinimumYearAsync();
            Task<int> maxYearTask = _documentsService.GetMaximumYearAsync();
            await Task.WhenAll(countTask, minYearTask, maxYearTask);

            DocumentCount = countTask.Result;
            MinimumYear   = minYearTask.Result;
            MaximumYear   = maxYearTask.Result;

            DocumentCountText = _localizer["Documents in the database"];

            if(MinimumYear > 0 && MaximumYear > 0)
            {
                for(int year = MinimumYear; year <= MaximumYear; year++) YearsList.Add(year);

                YearsGridTitle = string.Format(_localizer["Browse by Year ({0} - {1})"], MinimumYear, MaximumYear);
            }

            if(DocumentCount == 0)
            {
                ErrorMessage = _localizer["No documents found"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading documents data: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load documents data. Please try again later."].Value;
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
            _filterContext.FilterType  = DocumentListFilterType.Letter;
            _filterContext.FilterValue = letter.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(DocumentsListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to letter documents: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateByYearAsync(int year)
    {
        try
        {
            _filterContext.FilterType  = DocumentListFilterType.Year;
            _filterContext.FilterValue = year.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(DocumentsListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to year documents: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateAllDocumentsAsync()
    {
        try
        {
            _filterContext.FilterType  = DocumentListFilterType.All;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(DocumentsListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to all documents: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }
}
