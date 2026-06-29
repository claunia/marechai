#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Uno.Extensions.Authentication;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class BookViewViewModel : ObservableObject, IRegionAware
{
    private readonly BookCoverCache                _coverCache;
    private readonly BooksService                  _booksService;
    private readonly ImageSourceFactory            _imageSourceFactory;
    private readonly IAuthenticationService        _authService;
    private readonly IStringLocalizer              _localizer;
    private readonly ILogger<BookViewViewModel>    _logger;
    private readonly IRegionManager                _regionManager;

    private string? _navigationSource;
    private long    _currentBookId;
    private long?   _sourceNavigationBookId;

    [ObservableProperty]
    private string _bookTitle = string.Empty;

    [ObservableProperty]
    private string? _nativeTitle;

    [ObservableProperty]
    private string? _publishedDisplay;

    [ObservableProperty]
    private string? _country;

    [ObservableProperty]
    private string? _isbn;

    [ObservableProperty]
    private short? _pages;

    [ObservableProperty]
    private int? _edition;

    [ObservableProperty]
    private ImageSource? _coverImageSource;

    [ObservableProperty]
    private bool _hasCover;

    [ObservableProperty]
    private string? _synopsisText;

    [ObservableProperty]
    private string? _previousBookTitle;

    [ObservableProperty]
    private long? _previousBookId;

    [ObservableProperty]
    private string? _sourceBookTitle;

    [ObservableProperty]
    private long? _sourceBookId;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isCollected;

    [ObservableProperty]
    private bool _isTogglingCollection;

    [ObservableProperty]
    private string _collectionButtonText = string.Empty;

    // Visibility flags
    [ObservableProperty]
    private Visibility _showNativeTitle = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPublished = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCountry = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showIsbn = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPages = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showEdition = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSynopsis = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPeople = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCompanies = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showMachines = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showMachineFamilies = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPreviousBook = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSourceBook = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCollectionButton = Visibility.Collapsed;

    public BookViewViewModel(ILogger<BookViewViewModel> logger,           IRegionManager     regionManager,
                             BooksService              booksService,     BookCoverCache     coverCache,
                             IStringLocalizer          localizer,        ImageSourceFactory imageSourceFactory,
                             IAuthenticationService    authService)
    {
        _logger             = logger;
        _regionManager      = regionManager;
        _booksService       = booksService;
        _coverCache         = coverCache;
        _localizer          = localizer;
        _imageSourceFactory = imageSourceFactory;
        _authService        = authService;
    }

    partial void OnIsCollectedChanged(bool value) =>
        CollectionButtonText = value ? _localizer["In Collection"] : _localizer["Add to Collection"];

    public ObservableCollection<string> People          { get; } = [];
    public ObservableCollection<string> Companies       { get; } = [];
    public ObservableCollection<string> Machines        { get; } = [];
    public ObservableCollection<string> MachineFamilies { get; } = [];

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.NavigationSource, out string? source))
            _navigationSource = source;

        if(navigationContext.Parameters.TryGetValue<long>("SourceNavigationBookId", out long sourceBookNavId))
            _sourceNavigationBookId = sourceBookNavId;

        if(navigationContext.Parameters.TryGetValue<long>(NavParamKeys.BookId, out long bookId))
        {
            _currentBookId = bookId;
            _ = LoadBookAsync(bookId);
        }
    }

    [RelayCommand]
    public Task GoBack()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        switch(_navigationSource)
        {
            case nameof(BooksListViewModel):
                _regionManager.RequestNavigate(RegionNames.Content, nameof(BooksListPage));

                break;

            case nameof(BookViewViewModel):
            {
                // Going back from a book-to-book navigation — return to the source book
                if(_sourceNavigationBookId.HasValue)
                {
                    var parameters = new NavigationParameters
                    {
                        { NavParamKeys.BookId, _sourceNavigationBookId.Value },
                        { NavParamKeys.NavigationSource, nameof(BooksListViewModel) }
                    };

                    _regionManager.RequestNavigate(RegionNames.Content, nameof(BookViewPage), parameters);
                }
                else
                    _regionManager.RequestNavigate(RegionNames.Content, nameof(BooksListPage));

                break;
            }

            default:
                _regionManager.RequestNavigate(RegionNames.Content, nameof(BooksPage));

                break;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToPreviousBook()
    {
        if(PreviousBookId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.BookId, PreviousBookId.Value },
            { NavParamKeys.NavigationSource, nameof(BookViewViewModel) },
            { "SourceNavigationBookId", _currentBookId }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(BookViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToSourceBook()
    {
        if(SourceBookId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.BookId, SourceBookId.Value },
            { NavParamKeys.NavigationSource, nameof(BookViewViewModel) },
            { "SourceNavigationBookId", _currentBookId }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(BookViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task LoadData()
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task ToggleCollection()
    {
        if(IsTogglingCollection || _currentBookId == 0) return;

        try
        {
            IsTogglingCollection = true;

            bool success = IsCollected
                               ? await _booksService.RemoveBookFromCollectionAsync(_currentBookId)
                               : await _booksService.AddBookToCollectionAsync(_currentBookId);

            if(success)
                IsCollected = !IsCollected;
        }
        finally
        {
            IsTogglingCollection = false;
        }
    }

    public async Task LoadBookAsync(long bookId)
    {
        try
        {
            IsLoading    = true;
            IsDataLoaded = false;
            HasError     = false;
            ErrorMessage = string.Empty;
            People.Clear();
            Companies.Clear();
            Machines.Clear();
            MachineFamilies.Clear();

            BookDto? book = await _booksService.GetBookAsync(bookId);

            if(book is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["Book not found"];
                IsLoading    = false;

                return;
            }

            // Populate basic information (no SortTitle)
            BookTitle   = book.Title ?? string.Empty;
            NativeTitle = book.NativeTitle;
            Country     = book.Country;
            Isbn        = book.Isbn;
            Pages       = (short?)book.Pages;
            Edition     = book.Edition;

            if(book.Published.HasValue)
                PublishedDisplay = DatePrecisionFormatter.Format(book.Published, book.PublishedPrecision);

            // Load cover image
            HasCover = book.CoverGuid.HasValue;

            if(book.CoverGuid.HasValue)
                _ = LoadCoverAsync(book.CoverGuid.Value);

            // Load synopsis
            DocumentSynopsisDto? synopsis = await _booksService.GetBookSynopsisAsync(bookId);

            if(synopsis is not null)
                SynopsisText = synopsis.Text;

            // Load people (by role)
            List<PersonByBookDto> people = await _booksService.GetPeopleByBookAsync(bookId);

            foreach(PersonByBookDto person in people)
            {
                string name     = person.DisplayName ?? person.Alias ?? $"{person.Name} {person.Surname}".Trim();
                string? roleStr = person.Role;
                string display  = !string.IsNullOrEmpty(roleStr) ? $"{name} ({roleStr})" : name;
                People.Add(display);
            }

            // Load companies (by role)
            List<CompanyByBookDto> companies = await _booksService.GetCompaniesByBookAsync(bookId);

            foreach(CompanyByBookDto company in companies)
            {
                string name     = company.Company ?? string.Empty;
                string? roleStr = company.Role;
                string display  = !string.IsNullOrEmpty(roleStr) ? $"{name} ({roleStr})" : name;
                Companies.Add(display);
            }

            // Load machines
            List<BookByMachineDto> machines = await _booksService.GetMachinesByBookAsync(bookId);

            foreach(BookByMachineDto machine in machines)
                Machines.Add(machine.Machine ?? string.Empty);

            // Load machine families
            List<BookByMachineFamilyDto> families = await _booksService.GetMachineFamiliesByBookAsync(bookId);

            foreach(BookByMachineFamilyDto family in families)
                MachineFamilies.Add(family.MachineFamily ?? string.Empty);

            // Load previous book title
            PreviousBookId = book.PreviousId;

            if(book.PreviousId.HasValue)
            {
                BookDto? prev = await _booksService.GetBookAsync(book.PreviousId.Value);
                PreviousBookTitle = prev?.Title;
            }

            // Load source book title
            SourceBookId = book.SourceId;

            if(book.SourceId.HasValue)
            {
                BookDto? src = await _booksService.GetBookAsync(book.SourceId.Value);
                SourceBookTitle = src?.Title;
            }

            // Load collection state (authenticated users only)
            bool isAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);
            ShowCollectionButton = isAuthenticated ? Visibility.Visible : Visibility.Collapsed;

            if(isAuthenticated)
                IsCollected = await _booksService.IsBookCollectedAsync(bookId);

            CollectionButtonText = IsCollected ? _localizer["In Collection"] : _localizer["Add to Collection"];

            UpdateVisibilities();
            IsDataLoaded = true;
            IsLoading    = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading book {BookId}", bookId);
            HasError     = true;
            ErrorMessage = ex.Message;
            IsLoading    = false;
        }
    }

    private void UpdateVisibilities()
    {
        ShowNativeTitle     = !string.IsNullOrEmpty(NativeTitle) ? Visibility.Visible : Visibility.Collapsed;
        ShowPublished       = !string.IsNullOrEmpty(PublishedDisplay) ? Visibility.Visible : Visibility.Collapsed;
        ShowCountry         = !string.IsNullOrEmpty(Country) ? Visibility.Visible : Visibility.Collapsed;
        ShowIsbn            = !string.IsNullOrEmpty(Isbn) ? Visibility.Visible : Visibility.Collapsed;
        ShowPages           = Pages.HasValue ? Visibility.Visible : Visibility.Collapsed;
        ShowEdition         = Edition.HasValue ? Visibility.Visible : Visibility.Collapsed;
        ShowSynopsis        = !string.IsNullOrEmpty(SynopsisText) ? Visibility.Visible : Visibility.Collapsed;
        ShowPeople          = People.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowCompanies       = Companies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowMachines        = Machines.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowMachineFamilies = MachineFamilies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowPreviousBook    = PreviousBookId.HasValue ? Visibility.Visible : Visibility.Collapsed;
        ShowSourceBook      = SourceBookId.HasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task LoadCoverAsync(Guid coverGuid)
    {
        try
        {
            Stream stream = await _coverCache.GetCoverAsync(coverGuid);
            CoverImageSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading cover for book {BookId}", _currentBookId);
        }
    }
}
