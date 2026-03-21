#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Models;
using Marechai.App.Navigation;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminBooksViewModel : ObservableObject, IRegionAware
{
    private readonly ApiClient                      _apiClient;
    private readonly BooksService                   _booksService;
    private readonly IJwtService                    _jwtService;
    private readonly IStringLocalizer               _localizer;
    private readonly ILogger<AdminBooksViewModel>   _logger;
    private readonly ITokenService                  _tokenService;

    // --- List state ---
    [ObservableProperty] private ObservableCollection<BookDto> _books = [];
    [ObservableProperty] private ObservableCollection<BookDto> _filteredBooks = [];
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private BookDto? _selectedBook;
    private List<BookDto>? _allBooks;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;

    // --- Edit panel state ---
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isEditingExisting;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    private long? _editingBookId;

    // --- Form fields ---
    [ObservableProperty] private string _bookTitle = string.Empty;
    [ObservableProperty] private string _nativeTitle = string.Empty;
    [ObservableProperty] private DateTimeOffset? _published;
    [ObservableProperty] private Iso31661NumericDto? _selectedCountry;
    [ObservableProperty] private string _isbn = string.Empty;
    [ObservableProperty] private int? _pages;
    [ObservableProperty] private int? _edition;

    // --- Previous / Source book pickers ---
    [ObservableProperty] private string _previousBookSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<BookDto> _previousBookSuggestions = [];
    [ObservableProperty] private BookDto? _selectedPreviousBook;

    [ObservableProperty] private string _sourceBookSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<BookDto> _sourceBookSuggestions = [];
    [ObservableProperty] private BookDto? _selectedSourceBook;

    // --- Synopsis panel state ---
    [ObservableProperty] private bool _isEditingSynopsis;
    [ObservableProperty] private string _synopsisText = string.Empty;
    [ObservableProperty] private long? _synopsisBookId;
    [ObservableProperty] private ObservableCollection<LanguageItem> _availableLanguages = [];
    [ObservableProperty] private LanguageItem? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<DocumentSynopsisDto> _existingSynopses = [];

    // --- Picker data ---
    [ObservableProperty] private ObservableCollection<Iso31661NumericDto> _countries = [];

    // --- People junction ---
    [ObservableProperty] private ObservableCollection<PersonByBookDto> _bookPeople = [];
    [ObservableProperty] private ObservableCollection<string> _bookPeopleDisplays = [];
    [ObservableProperty] private ObservableCollection<PersonDto> _availablePeople = [];
    [ObservableProperty] private PersonDto? _selectedAvailablePerson;
    [ObservableProperty] private ObservableCollection<DocumentRoleDto> _availableRoles = [];
    [ObservableProperty] private DocumentRoleDto? _selectedPersonRole;
    private List<PersonDto>? _allPeopleList;

    // --- Companies junction ---
    [ObservableProperty] private ObservableCollection<CompanyByBookDto> _bookCompanies = [];
    [ObservableProperty] private ObservableCollection<string> _bookCompanyDisplays = [];
    [ObservableProperty] private ObservableCollection<CompanyDto> _availableCompanies = [];
    [ObservableProperty] private CompanyDto? _selectedAvailableCompany;
    [ObservableProperty] private DocumentRoleDto? _selectedCompanyRole;
    private List<CompanyDto>? _allCompaniesList;

    // --- Machines junction ---
    [ObservableProperty] private ObservableCollection<BookByMachineDto> _bookMachines = [];
    [ObservableProperty] private ObservableCollection<string> _bookMachineDisplays = [];
    [ObservableProperty] private ObservableCollection<MachineDto> _availableMachines = [];
    [ObservableProperty] private MachineDto? _selectedAvailableMachine;
    private List<MachineDto>? _allMachinesList;

    // --- Machine Families junction ---
    [ObservableProperty] private ObservableCollection<BookByMachineFamilyDto> _bookMachineFamilies = [];
    [ObservableProperty] private ObservableCollection<string> _bookMachineFamilyDisplays = [];
    [ObservableProperty] private ObservableCollection<MachineFamilyDto> _availableMachineFamilies = [];
    [ObservableProperty] private MachineFamilyDto? _selectedAvailableMachineFamily;
    private List<MachineFamilyDto>? _allMachineFamiliesList;

    // --- Document roles ---
    private List<DocumentRoleDto>? _allRolesList;

    public AdminBooksViewModel(ApiClient                      apiClient,
                               BooksService                   booksService,
                               IJwtService                    jwtService,
                               ITokenService                  tokenService,
                               ILogger<AdminBooksViewModel>   logger,
                               IStringLocalizer               localizer)
    {
        _apiClient    = apiClient;
        _booksService = booksService;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadBooksCommand         = new AsyncRelayCommand(LoadBooksAsync);
        OpenAddBookCommand       = new RelayCommand(OpenAddBook);
        OpenEditBookCommand      = new RelayCommand<BookDto>(OpenEditBook);
        DeleteBookCommand        = new AsyncRelayCommand<BookDto>(DeleteBookAsync);
        SaveBookCommand          = new AsyncRelayCommand(SaveBookAsync);
        CancelEditCommand        = new RelayCommand(CancelEdit);

        // Synopsis commands
        OpenSynopsisCommand      = new AsyncRelayCommand<BookDto>(OpenSynopsisAsync);
        SaveSynopsisCommand      = new AsyncRelayCommand(SaveSynopsisAsync);
        CancelSynopsisCommand    = new RelayCommand(CancelSynopsis);
        DeleteSynopsisCommand    = new AsyncRelayCommand<DocumentSynopsisDto>(DeleteSynopsisAsync);
        EditSynopsisCommand      = new RelayCommand<DocumentSynopsisDto>(EditSynopsis);

        // Junction commands
        AddPersonCommand             = new AsyncRelayCommand(AddPersonAsync);
        RemovePersonCommand          = new AsyncRelayCommand<string>(RemovePersonByDisplayAsync);
        AddCompanyCommand            = new AsyncRelayCommand(AddCompanyAsync);
        RemoveCompanyCommand         = new AsyncRelayCommand<string>(RemoveCompanyByDisplayAsync);
        AddMachineCommand            = new AsyncRelayCommand(AddMachineAsync);
        RemoveMachineCommand         = new AsyncRelayCommand<string>(RemoveMachineByDisplayAsync);
        AddMachineFamilyCommand      = new AsyncRelayCommand(AddMachineFamilyAsync);
        RemoveMachineFamilyCommand   = new AsyncRelayCommand<string>(RemoveMachineFamilyByDisplayAsync);

        InitializeLanguages();
        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand            LoadBooksCommand         { get; }
    public IRelayCommand                 OpenAddBookCommand       { get; }
    public IRelayCommand<BookDto>        OpenEditBookCommand      { get; }
    public IAsyncRelayCommand<BookDto>   DeleteBookCommand        { get; }
    public IAsyncRelayCommand            SaveBookCommand          { get; }
    public IRelayCommand                 CancelEditCommand        { get; }

    public IAsyncRelayCommand<BookDto>              OpenSynopsisCommand      { get; }
    public IAsyncRelayCommand                       SaveSynopsisCommand      { get; }
    public IRelayCommand                            CancelSynopsisCommand    { get; }
    public IAsyncRelayCommand<DocumentSynopsisDto>  DeleteSynopsisCommand    { get; }
    public IRelayCommand<DocumentSynopsisDto>       EditSynopsisCommand      { get; }

    public IAsyncRelayCommand         AddPersonCommand           { get; }
    public IAsyncRelayCommand<string> RemovePersonCommand        { get; }
    public IAsyncRelayCommand         AddCompanyCommand          { get; }
    public IAsyncRelayCommand<string> RemoveCompanyCommand       { get; }
    public IAsyncRelayCommand         AddMachineCommand          { get; }
    public IAsyncRelayCommand<string> RemoveMachineCommand       { get; }
    public IAsyncRelayCommand         AddMachineFamilyCommand    { get; }
    public IAsyncRelayCommand<string> RemoveMachineFamilyCommand { get; }

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();
        if(IsAdmin) _ = LoadBooksCommand.ExecuteAsync(null);
    }

    // --- Role check ---
    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();
            if(string.IsNullOrWhiteSpace(token)) { IsAdmin = false; return; }
            IEnumerable<string> roles = _jwtService.GetRoles(token);
            IsAdmin = roles.Contains("Uberadmin",      StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
        }
        catch { IsAdmin = false; }
    }

    // --- Load books ---
    private async Task LoadBooksAsync()
    {
        try
        {
            IsLoading = true; HasError = false; ErrorMessage = string.Empty;
            Books.Clear();
            List<BookDto> response = await _booksService.GetAllBooksAsync();
            _allBooks = response;
            foreach(BookDto b in response) Books.Add(b);
            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading books");
            ErrorMessage = _localizer["FailedToLoadBooks"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    // --- Add book ---
    private void OpenAddBook()
    {
        _editingBookId = null;
        EditPanelTitle = _localizer["AddBookDialog_Title"];
        ClearForm();
        IsEditingExisting  = false;
        IsEditingSynopsis  = false;
        IsEditing          = true;
    }

    // --- Edit book ---
    private async void OpenEditBook(BookDto? book)
    {
        if(book?.Id == null) return;

        try
        {
            BookDto? full = await _booksService.GetBookAsync(book.Id.Value);
            if(full == null) return;

            _editingBookId = book.Id;
            EditPanelTitle = _localizer["EditBookDialog_Title"];
            PopulateForm(full);
            await LoadAllJunctionsAsync(book.Id.Value);
            IsEditingExisting  = true;
            IsEditingSynopsis  = false;
            IsEditing          = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading book {Id}", book.Id);
            ErrorMessage = _localizer["FailedToLoadBooks"];
            HasError = true;
        }
    }

    // --- Delete book ---
    private async Task DeleteBookAsync(BookDto? book)
    {
        if(book?.Id == null) return;

        try
        {
            await _booksService.DeleteBookAsync(book.Id.Value);
            await LoadBooksAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting book {Id}", book.Id);
            ErrorMessage = _localizer["FailedToDeleteBook"];
            HasError = true;
        }
    }

    // --- Save book ---
    private async Task SaveBookAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(BookTitle))
            {
                ErrorMessage = _localizer["BookTitleRequired"];
                HasError = true;
                return;
            }

            var dto = new BookDto
            {
                Title      = BookTitle,
                NativeTitle = string.IsNullOrWhiteSpace(NativeTitle) ? null : NativeTitle,
                Published  = Published,
                CountryId  = SelectedCountry?.Id,
                Isbn       = string.IsNullOrWhiteSpace(Isbn) ? null : Isbn,
                Pages      = Pages,
                Edition    = Edition,
                PreviousId = SelectedPreviousBook?.Id,
                SourceId   = SelectedSourceBook?.Id
            };

            if(_editingBookId == null)
                await _booksService.CreateBookAsync(dto);
            else
            {
                dto.Id = _editingBookId;
                await _booksService.UpdateBookAsync(_editingBookId.Value, dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadBooksAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving book");
            ErrorMessage = _localizer["FailedToSaveBook"];
            HasError = true;
        }
    }

    // --- Cancel edit ---
    private void CancelEdit()
    {
        IsEditing = false;
        IsEditingExisting = false;
        _editingBookId = null;
        ClearForm();
        HasError = false;
        ErrorMessage = string.Empty;
    }

    // ======================== SYNOPSIS ========================

    private void InitializeLanguages()
    {
        AvailableLanguages =
        [
            new LanguageItem { Code = "eng", DisplayName = "English" },
            new LanguageItem { Code = "spa", DisplayName = "Español" },
            new LanguageItem { Code = "deu", DisplayName = "Deutsch" },
            new LanguageItem { Code = "fra", DisplayName = "Français" },
            new LanguageItem { Code = "lat", DisplayName = "Latina" },
            new LanguageItem { Code = "por", DisplayName = "Português (Brasil)" }
        ];

        SelectedLanguage = AvailableLanguages[0];
    }

    private async Task OpenSynopsisAsync(BookDto? book)
    {
        if(book?.Id == null) return;

        try
        {
            SynopsisBookId = book.Id;
            SynopsisText   = string.Empty;
            IsEditing      = false;
            ExistingSynopses.Clear();

            List<DocumentSynopsisDto> synopses = await _booksService.GetSynopsesAsync(book.Id.Value);

            foreach(DocumentSynopsisDto s in synopses)
                ExistingSynopses.Add(s);

            SelectedLanguage = AvailableLanguages.FirstOrDefault(l =>
                                   ExistingSynopses.All(s => s.LanguageCode != l.Code)) ??
                               AvailableLanguages[0];

            DocumentSynopsisDto? existing =
                ExistingSynopses.FirstOrDefault(s => s.LanguageCode == SelectedLanguage.Code);

            if(existing != null)
                SynopsisText = existing.Text ?? string.Empty;

            IsEditingSynopsis = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading synopses for book {Id}", book.Id);
            SynopsisText      = string.Empty;
            IsEditingSynopsis = true;
        }
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        if(value == null || ExistingSynopses.Count == 0)
        {
            SynopsisText = string.Empty;
            return;
        }

        DocumentSynopsisDto? existing = ExistingSynopses.FirstOrDefault(s => s.LanguageCode == value.Code);
        SynopsisText = existing?.Text ?? string.Empty;
    }

    private async Task SaveSynopsisAsync()
    {
        if(SynopsisBookId == null || SelectedLanguage == null) return;

        try
        {
            var dto = new DocumentSynopsisDto
            {
                Text         = SynopsisText,
                LanguageCode = SelectedLanguage.Code
            };

            await _booksService.UpsertSynopsisAsync(SynopsisBookId.Value, dto);

            ExistingSynopses.Clear();
            List<DocumentSynopsisDto> synopses = await _booksService.GetSynopsesAsync(SynopsisBookId.Value);
            foreach(DocumentSynopsisDto s in synopses) ExistingSynopses.Add(s);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving synopsis");
            ErrorMessage = _localizer["FailedToSaveBook"];
            HasError = true;
        }
    }

    private void EditSynopsis(DocumentSynopsisDto? synopsis)
    {
        if(synopsis?.LanguageCode == null) return;
        SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == synopsis.LanguageCode);
        SynopsisText     = synopsis.Text ?? string.Empty;
    }

    private async Task DeleteSynopsisAsync(DocumentSynopsisDto? synopsis)
    {
        if(SynopsisBookId == null || synopsis?.LanguageCode == null) return;

        try
        {
            await _booksService.DeleteSynopsisAsync(SynopsisBookId.Value, synopsis.LanguageCode);
            ExistingSynopses.Remove(synopsis);
            SynopsisText = string.Empty;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting synopsis");
            ErrorMessage = _localizer["FailedToDeleteTranslation"];
            HasError = true;
        }
    }

    private void CancelSynopsis()
    {
        IsEditingSynopsis = false;
        SynopsisBookId    = null;
        SynopsisText      = string.Empty;
        ExistingSynopses.Clear();
    }

    // ======================== JUNCTION MANAGEMENT ========================

    private async Task LoadAllJunctionsAsync(long bookId)
    {
        await Task.WhenAll(
            LoadBookPeopleAsync(bookId),
            LoadBookCompaniesAsync(bookId),
            LoadBookMachinesAsync(bookId),
            LoadBookMachineFamiliesAsync(bookId)
        );
    }

    // --- People ---
    private async Task LoadBookPeopleAsync(long bookId)
    {
        BookPeople.Clear(); BookPeopleDisplays.Clear();

        try
        {
            List<PersonByBookDto> items = await _booksService.GetPeopleByBookAsync(bookId);

            foreach(PersonByBookDto p in items)
            {
                BookPeople.Add(p);
                string name = p.DisplayName ?? p.Alias ?? $"{p.Name} {p.Surname}".Trim();
                BookPeopleDisplays.Add($"{name} ({p.Role})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading people for book"); }
    }

    private async Task AddPersonAsync()
    {
        if(_editingBookId == null || SelectedAvailablePerson?.Id == null || SelectedPersonRole?.Id == null) return;

        try
        {
            await _booksService.AddPersonToBookAsync(new PersonByBookDto
            {
                PersonId = SelectedAvailablePerson.Id,
                BookId   = _editingBookId,
                RoleId   = SelectedPersonRole.Id
            });

            SelectedAvailablePerson = null;
            await LoadBookPeopleAsync(_editingBookId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding person to book");
            ErrorMessage = _localizer["FailedToSaveBook"];
            HasError = true;
        }
    }

    private async Task RemovePersonByDisplayAsync(string? display)
    {
        if(display == null || _editingBookId == null) return;

        int idx = BookPeopleDisplays.IndexOf(display);

        if(idx >= 0 && idx < BookPeople.Count && BookPeople[idx].Id.HasValue)
        {
            try
            {
                await _booksService.RemovePersonFromBookAsync(BookPeople[idx].Id!.Value);
                await LoadBookPeopleAsync(_editingBookId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing person from book"); }
        }
    }

    // --- Companies ---
    private async Task LoadBookCompaniesAsync(long bookId)
    {
        BookCompanies.Clear(); BookCompanyDisplays.Clear();

        try
        {
            List<CompanyByBookDto> items = await _booksService.GetCompaniesByBookAsync(bookId);

            foreach(CompanyByBookDto c in items)
            {
                BookCompanies.Add(c);
                BookCompanyDisplays.Add($"{c.Company} ({c.Role})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading companies for book"); }
    }

    private async Task AddCompanyAsync()
    {
        if(_editingBookId == null || SelectedAvailableCompany?.Id == null || SelectedCompanyRole?.Id == null) return;

        try
        {
            await _booksService.AddCompanyToBookAsync(new CompanyByBookDto
            {
                CompanyId = SelectedAvailableCompany.Id,
                BookId    = _editingBookId,
                RoleId    = SelectedCompanyRole.Id
            });

            SelectedAvailableCompany = null;
            await LoadBookCompaniesAsync(_editingBookId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding company to book");
            ErrorMessage = _localizer["FailedToSaveBook"];
            HasError = true;
        }
    }

    private async Task RemoveCompanyByDisplayAsync(string? display)
    {
        if(display == null || _editingBookId == null) return;

        int idx = BookCompanyDisplays.IndexOf(display);

        if(idx >= 0 && idx < BookCompanies.Count && BookCompanies[idx].Id.HasValue)
        {
            try
            {
                await _booksService.RemoveCompanyFromBookAsync(BookCompanies[idx].Id!.Value);
                await LoadBookCompaniesAsync(_editingBookId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing company from book"); }
        }
    }

    // --- Machines ---
    private async Task LoadBookMachinesAsync(long bookId)
    {
        BookMachines.Clear(); BookMachineDisplays.Clear();

        try
        {
            List<BookByMachineDto> items = await _booksService.GetMachinesByBookAsync(bookId);

            foreach(BookByMachineDto m in items)
            {
                BookMachines.Add(m);
                BookMachineDisplays.Add(m.Machine ?? $"Machine #{m.MachineId}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machines for book"); }

        RefreshAvailableMachines();
    }

    private async Task AddMachineAsync()
    {
        if(_editingBookId == null || SelectedAvailableMachine?.Id == null) return;

        try
        {
            await _booksService.AddMachineToBookAsync(new BookByMachineDto
            {
                BookId    = _editingBookId,
                MachineId = SelectedAvailableMachine.Id
            });

            SelectedAvailableMachine = null;
            await LoadBookMachinesAsync(_editingBookId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine to book");
            ErrorMessage = _localizer["FailedToSaveBook"];
            HasError = true;
        }
    }

    private async Task RemoveMachineByDisplayAsync(string? display)
    {
        if(display == null || _editingBookId == null) return;

        int idx = BookMachineDisplays.IndexOf(display);

        if(idx >= 0 && idx < BookMachines.Count && BookMachines[idx].Id.HasValue)
        {
            try
            {
                await _booksService.RemoveMachineFromBookAsync(BookMachines[idx].Id!.Value);
                await LoadBookMachinesAsync(_editingBookId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing machine from book"); }
        }
    }

    // --- Machine Families ---
    private async Task LoadBookMachineFamiliesAsync(long bookId)
    {
        BookMachineFamilies.Clear(); BookMachineFamilyDisplays.Clear();

        try
        {
            List<BookByMachineFamilyDto> items = await _booksService.GetMachineFamiliesByBookAsync(bookId);

            foreach(BookByMachineFamilyDto f in items)
            {
                BookMachineFamilies.Add(f);
                BookMachineFamilyDisplays.Add(f.MachineFamily ?? $"Family #{f.MachineFamilyId}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machine families for book"); }

        RefreshAvailableMachineFamilies();
    }

    private async Task AddMachineFamilyAsync()
    {
        if(_editingBookId == null || SelectedAvailableMachineFamily?.Id == null) return;

        try
        {
            await _booksService.AddMachineFamilyToBookAsync(new BookByMachineFamilyDto
            {
                BookId          = _editingBookId,
                MachineFamilyId = SelectedAvailableMachineFamily.Id
            });

            SelectedAvailableMachineFamily = null;
            await LoadBookMachineFamiliesAsync(_editingBookId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine family to book");
            ErrorMessage = _localizer["FailedToSaveBook"];
            HasError = true;
        }
    }

    private async Task RemoveMachineFamilyByDisplayAsync(string? display)
    {
        if(display == null || _editingBookId == null) return;

        int idx = BookMachineFamilyDisplays.IndexOf(display);

        if(idx >= 0 && idx < BookMachineFamilies.Count && BookMachineFamilies[idx].Id.HasValue)
        {
            try
            {
                await _booksService.RemoveMachineFamilyFromBookAsync(BookMachineFamilies[idx].Id!.Value);
                await LoadBookMachineFamiliesAsync(_editingBookId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing machine family from book"); }
        }
    }

    // ======================== HELPERS ========================

    private void RefreshAvailableMachines()
    {
        AvailableMachines.Clear();

        if(_allMachinesList == null) return;

        HashSet<int> assigned = new(BookMachines.Where(m => m.MachineId.HasValue).Select(m => m.MachineId!.Value));

        foreach(MachineDto m in _allMachinesList)
            if(m.Id.HasValue && !assigned.Contains(m.Id.Value))
                AvailableMachines.Add(m);
    }

    private void RefreshAvailableMachineFamilies()
    {
        AvailableMachineFamilies.Clear();

        if(_allMachineFamiliesList == null) return;

        HashSet<int> assigned = new(BookMachineFamilies.Where(f => f.MachineFamilyId.HasValue).Select(f => f.MachineFamilyId!.Value));

        foreach(MachineFamilyDto f in _allMachineFamiliesList)
            if(f.Id.HasValue && !assigned.Contains(f.Id.Value))
                AvailableMachineFamilies.Add(f);
    }

    // --- Filtering ---
    public void ApplyFilter()
    {
        FilteredBooks.Clear();
        IEnumerable<BookDto> source = (IEnumerable<BookDto>?)_allBooks ?? Books;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(b =>
                (b.Title != null      && b.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (b.NativeTitle != null && b.NativeTitle.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (b.Isbn != null       && b.Isbn.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));

        foreach(BookDto b in source) FilteredBooks.Add(b);
    }

    // --- Book search pickers ---
    public void UpdatePreviousBookSuggestions(string query)
    {
        PreviousBookSuggestions.Clear();
        if(_allBooks == null) return;

        IEnumerable<BookDto> source = _allBooks;
        if(_editingBookId.HasValue)
            source = source.Where(b => b.Id != _editingBookId);
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(b => b.Title != null && b.Title.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(BookDto b in source.Take(50)) PreviousBookSuggestions.Add(b);
    }

    public void UpdateSourceBookSuggestions(string query)
    {
        SourceBookSuggestions.Clear();
        if(_allBooks == null) return;

        IEnumerable<BookDto> source = _allBooks;
        if(_editingBookId.HasValue)
            source = source.Where(b => b.Id != _editingBookId);
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(b => b.Title != null && b.Title.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(BookDto b in source.Take(50)) SourceBookSuggestions.Add(b);
    }

    // --- Load picker data ---
    public async Task LoadPickerDataAsync()
    {
        if(Countries.Count == 0)
        {
            try
            {
                List<Iso31661NumericDto>? countriesResponse = await _apiClient.Iso31661Numeric.GetAsync();
                if(countriesResponse != null)
                    foreach(Iso31661NumericDto c in countriesResponse) Countries.Add(c);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error loading countries"); }
        }

        if(_allRolesList == null)
        {
            try
            {
                _allRolesList = await _booksService.GetDocumentRolesAsync();
                foreach(DocumentRoleDto r in _allRolesList) AvailableRoles.Add(r);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error loading document roles"); }
        }

        if(_allPeopleList == null)
        {
            try { _allPeopleList = await _apiClient.People.GetAsync(); }
            catch(Exception ex) { _logger.LogError(ex, "Error loading people"); }
        }

        if(_allCompaniesList == null)
        {
            try { _allCompaniesList = await _apiClient.Companies.GetAsync(); }
            catch(Exception ex) { _logger.LogError(ex, "Error loading companies"); }
        }

        if(_allMachinesList == null)
        {
            try { _allMachinesList = await _apiClient.Machines.GetAsync(); }
            catch(Exception ex) { _logger.LogError(ex, "Error loading machines"); }
        }

        if(_allMachineFamiliesList == null)
        {
            try { _allMachineFamiliesList = await _apiClient.MachineFamilies.GetAsync(); }
            catch(Exception ex) { _logger.LogError(ex, "Error loading machine families"); }
        }
    }

    // --- People search ---
    public void UpdatePeopleSuggestions(string query)
    {
        AvailablePeople.Clear();
        if(_allPeopleList == null) return;

        IEnumerable<PersonDto> source = _allPeopleList;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(p =>
                (p.Name != null && p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (p.Surname != null && p.Surname.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (p.DisplayName != null && p.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)));

        foreach(PersonDto p in source.Take(50)) AvailablePeople.Add(p);
    }

    // --- Company search ---
    public void UpdateCompanySuggestions(string query)
    {
        AvailableCompanies.Clear();
        if(_allCompaniesList == null) return;

        IEnumerable<CompanyDto> source = _allCompaniesList;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(c => c.Name != null && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(CompanyDto c in source.Take(50)) AvailableCompanies.Add(c);
    }

    // --- Form helpers ---
    private void ClearForm()
    {
        BookTitle        = string.Empty;
        NativeTitle      = string.Empty;
        Published        = null;
        SelectedCountry  = null;
        Isbn             = string.Empty;
        Pages            = null;
        Edition          = null;
        SelectedPreviousBook   = null;
        PreviousBookSearchText = string.Empty;
        SelectedSourceBook     = null;
        SourceBookSearchText   = string.Empty;

        BookPeople.Clear();          BookPeopleDisplays.Clear();
        BookCompanies.Clear();       BookCompanyDisplays.Clear();
        BookMachines.Clear();        BookMachineDisplays.Clear();
        BookMachineFamilies.Clear(); BookMachineFamilyDisplays.Clear();
        AvailableMachines.Clear();   AvailableMachineFamilies.Clear();
        SelectedAvailablePerson    = null;
        SelectedAvailableCompany   = null;
        SelectedAvailableMachine   = null;
        SelectedAvailableMachineFamily = null;
        SelectedPersonRole         = null;
        SelectedCompanyRole        = null;

        HasError     = false;
        ErrorMessage = string.Empty;
    }

    private void PopulateForm(BookDto book)
    {
        BookTitle   = book.Title      ?? string.Empty;
        NativeTitle = book.NativeTitle ?? string.Empty;
        Published   = book.Published;
        Isbn        = book.Isbn       ?? string.Empty;
        Pages       = book.Pages;
        Edition     = book.Edition;

        SelectedCountry = book.CountryId.HasValue
                              ? Countries.FirstOrDefault(c => c.Id == book.CountryId.Value)
                              : null;

        if(book.PreviousId.HasValue && _allBooks != null)
        {
            BookDto? prev = _allBooks.FirstOrDefault(b => b.Id == book.PreviousId.Value);
            if(prev != null)
            {
                PreviousBookSearchText = prev.Title ?? string.Empty;
                UpdatePreviousBookSuggestions(PreviousBookSearchText);
                SelectedPreviousBook = PreviousBookSuggestions.FirstOrDefault(b => b.Id == prev.Id);
            }
            else { PreviousBookSearchText = string.Empty; SelectedPreviousBook = null; }
        }
        else { PreviousBookSearchText = string.Empty; SelectedPreviousBook = null; }

        if(book.SourceId.HasValue && _allBooks != null)
        {
            BookDto? src = _allBooks.FirstOrDefault(b => b.Id == book.SourceId.Value);
            if(src != null)
            {
                SourceBookSearchText = src.Title ?? string.Empty;
                UpdateSourceBookSuggestions(SourceBookSearchText);
                SelectedSourceBook = SourceBookSuggestions.FirstOrDefault(b => b.Id == src.Id);
            }
            else { SourceBookSearchText = string.Empty; SelectedSourceBook = null; }
        }
        else { SourceBookSearchText = string.Empty; SelectedSourceBook = null; }
    }
}
