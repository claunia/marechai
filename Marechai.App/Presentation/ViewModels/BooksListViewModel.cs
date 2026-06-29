#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class BooksListViewModel : ObservableObject, IRegionAware
{
    private readonly BookCoverCache                  _coverCache;
    private readonly BooksService                    _booksService;
    private readonly IBooksListFilterContext          _filterContext;
    private readonly ImageSourceFactory              _imageSourceFactory;
    private readonly IStringLocalizer                _localizer;
    private readonly ILogger<BooksListViewModel>     _logger;
    private readonly IRegionManager                  _regionManager;

    [ObservableProperty]
    private ObservableCollection<BookListItem> _booksList = [];

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

    public BooksListViewModel(BooksService                    booksService, IStringLocalizer localizer,
                              ILogger<BooksListViewModel>     logger,       IRegionManager   regionManager,
                              IBooksListFilterContext          filterContext,
                              BookCoverCache                  coverCache,
                              ImageSourceFactory              imageSourceFactory)
    {
        _booksService       = booksService;
        _localizer          = localizer;
        _logger             = logger;
        _regionManager      = regionManager;
        _filterContext      = filterContext;
        _coverCache         = coverCache;
        _imageSourceFactory = imageSourceFactory;
        LoadData            = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand       = new AsyncRelayCommand(GoBackAsync);
        NavigateToBookCommand = new AsyncRelayCommand<BookListItem>(NavigateToBookAsync);
    }

    public IAsyncRelayCommand                LoadData              { get; }
    public ICommand                          GoBackCommand         { get; }
    public IAsyncRelayCommand<BookListItem>  NavigateToBookCommand { get; }

    public BookListFilterType FilterType
    {
        get => _filterContext.FilterType;
        set => _filterContext.FilterType = value;
    }

    public string FilterValue
    {
        get => _filterContext.FilterValue;
        set => _filterContext.FilterValue = value;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            BooksList.Clear();

            UpdateFilterDescription();

            await LoadBooksFromApiAsync();

            if(BooksList.Count == 0)
            {
                ErrorMessage = _localizer["No books found for this filter"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading books: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load books. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateFilterDescription()
    {
        switch(FilterType)
        {
            case BookListFilterType.All:
                PageTitle         = _localizer["All Books"];
                FilterDescription = _localizer["Browsing all books in the database"];

                break;

            case BookListFilterType.Letter:
                if(!string.IsNullOrEmpty(FilterValue) && FilterValue.Length == 1)
                {
                    PageTitle         = $"{_localizer["Books Starting with"]} {FilterValue}";
                    FilterDescription = $"{_localizer["Showing books that start with"]} {FilterValue}";
                }

                break;

            case BookListFilterType.Year:
                if(!string.IsNullOrEmpty(FilterValue) && int.TryParse(FilterValue, out int year))
                {
                    PageTitle         = $"{_localizer["Books from"]} {year}";
                    FilterDescription = $"{_localizer["Showing books published in"]} {year}";
                }

                break;
        }
    }

    private async Task LoadBooksFromApiAsync()
    {
        try
        {
            List<BookDto> books = FilterType switch
                                  {
                                      BookListFilterType.Letter when FilterValue.Length == 1 =>
                                          await _booksService.GetBooksByLetterAsync(FilterValue[0]),

                                      BookListFilterType.Year when int.TryParse(FilterValue, out int year) =>
                                          await _booksService.GetBooksByYearAsync(year),

                                      _ => await _booksService.GetAllBooksAsync()
                                  };

            foreach(BookDto book in books)
            {
                long id   = book.Id   ?? 0;
                int? year = book.Published?.Year;

                var item = new BookListItem
                {
                    Id        = id,
                    Title     = book.Title ?? string.Empty,
                    Year      = year,
                    CoverGuid = book.CoverGuid
                };

                // Fire-and-forget cover thumbnail loading
                if(book.CoverGuid.HasValue)
                    _ = LoadCoverThumbnailAsync(item, book.CoverGuid.Value);

                BooksList.Add(item);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading books from API");
        }
    }

    private async Task LoadCoverThumbnailAsync(BookListItem item, Guid coverGuid)
    {
        try
        {
            Stream stream = await _coverCache.GetThumbnailAsync(coverGuid);
            item.CoverThumbnailSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading cover thumbnail for book {BookId}", item.Id);
        }
    }

    private Task GoBackAsync()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        _regionManager.RequestNavigate(RegionNames.Content, nameof(BooksPage));

        return Task.CompletedTask;
    }

    private Task NavigateToBookAsync(BookListItem? book)
    {
        if(book is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.BookId, book.Id },
            { NavParamKeys.NavigationSource, nameof(BooksListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(BookViewPage), parameters);

        return Task.CompletedTask;
    }
}
