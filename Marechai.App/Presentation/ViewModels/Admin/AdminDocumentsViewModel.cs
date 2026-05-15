#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminDocumentsViewModel : ObservableObject, IRegionAware
{
    private readonly Client                          _apiClient;
    private readonly DocumentsService                   _documentsService;
    private readonly IJwtService                        _jwtService;
    private readonly IStringLocalizer                   _localizer;
    private readonly ILogger<AdminDocumentsViewModel>   _logger;
    private readonly ITokenService                      _tokenService;

    // --- List state ---
    [ObservableProperty] private ObservableCollection<DocumentDto> _documents = [];
    [ObservableProperty] private ObservableCollection<DocumentDto> _filteredDocuments = [];
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private DocumentDto? _selectedDocument;
    private List<DocumentDto>? _allDocuments;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;

    // --- Edit panel state ---
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isEditingExisting;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    private long? _editingDocumentId;

    // --- Form fields ---
    [ObservableProperty] private string _documentTitle = string.Empty;
    [ObservableProperty] private string _nativeTitle = string.Empty;
    [ObservableProperty] private string _sortTitle = string.Empty;
    [ObservableProperty] private DateTimeOffset? _published;
    [ObservableProperty] private int _publishedPrecision;
    [ObservableProperty] private Iso31661NumericDto? _selectedCountry;

    // --- Synopsis panel state ---
    [ObservableProperty] private bool _isEditingSynopsis;
    [ObservableProperty] private string _synopsisText = string.Empty;
    [ObservableProperty] private long? _synopsisDocumentId;
    [ObservableProperty] private ObservableCollection<LanguageItem> _availableLanguages = [];
    [ObservableProperty] private LanguageItem? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<DocumentSynopsisDto> _existingSynopses = [];

    // --- Picker data ---
    [ObservableProperty] private ObservableCollection<Iso31661NumericDto> _countries = [];

    // --- People junction ---
    [ObservableProperty] private ObservableCollection<PersonByDocumentDto> _documentPeople = [];
    [ObservableProperty] private ObservableCollection<string> _documentPeopleDisplays = [];
    [ObservableProperty] private ObservableCollection<PersonDto> _availablePeople = [];
    [ObservableProperty] private PersonDto? _selectedAvailablePerson;
    [ObservableProperty] private ObservableCollection<DocumentRoleDto> _availableRoles = [];
    [ObservableProperty] private DocumentRoleDto? _selectedPersonRole;
    private List<PersonDto>? _allPeopleList;

    // --- Companies junction ---
    [ObservableProperty] private ObservableCollection<CompanyByDocumentDto> _documentCompanies = [];
    [ObservableProperty] private ObservableCollection<string> _documentCompanyDisplays = [];
    [ObservableProperty] private ObservableCollection<CompanyDto> _availableCompanies = [];
    [ObservableProperty] private CompanyDto? _selectedAvailableCompany;
    [ObservableProperty] private DocumentRoleDto? _selectedCompanyRole;
    private List<CompanyDto>? _allCompaniesList;

    // --- Machines junction ---
    [ObservableProperty] private ObservableCollection<DocumentByMachineDto> _documentMachines = [];
    [ObservableProperty] private ObservableCollection<string> _documentMachineDisplays = [];
    [ObservableProperty] private ObservableCollection<MachineDto> _availableMachines = [];
    [ObservableProperty] private MachineDto? _selectedAvailableMachine;
    private List<MachineDto>? _allMachinesList;

    // --- Machine Families junction ---
    [ObservableProperty] private ObservableCollection<DocumentByMachineFamilyDto> _documentMachineFamilies = [];
    [ObservableProperty] private ObservableCollection<string> _documentMachineFamilyDisplays = [];
    [ObservableProperty] private ObservableCollection<MachineFamilyDto> _availableMachineFamilies = [];
    [ObservableProperty] private MachineFamilyDto? _selectedAvailableMachineFamily;
    private List<MachineFamilyDto>? _allMachineFamiliesList;

    // --- Document roles ---
    private List<DocumentRoleDto>? _allRolesList;

    public AdminDocumentsViewModel(Client                          apiClient,
                                   DocumentsService                   documentsService,
                                   IJwtService                        jwtService,
                                   ITokenService                      tokenService,
                                   ILogger<AdminDocumentsViewModel>   logger,
                                   IStringLocalizer                   localizer)
    {
        _apiClient        = apiClient;
        _documentsService = documentsService;
        _jwtService       = jwtService;
        _tokenService     = tokenService;
        _logger           = logger;
        _localizer        = localizer;

        LoadDocumentsCommand         = new AsyncRelayCommand(LoadDocumentsAsync);
        OpenAddDocumentCommand       = new RelayCommand(OpenAddDocument);
        OpenEditDocumentCommand      = new RelayCommand<DocumentDto>(OpenEditDocument);
        DeleteDocumentCommand        = new AsyncRelayCommand<DocumentDto>(DeleteDocumentAsync);
        SaveDocumentCommand          = new AsyncRelayCommand(SaveDocumentAsync);
        CancelEditCommand            = new RelayCommand(CancelEdit);

        // Synopsis commands
        OpenSynopsisCommand      = new AsyncRelayCommand<DocumentDto>(OpenSynopsisAsync);
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
    public IAsyncRelayCommand                LoadDocumentsCommand         { get; }
    public IRelayCommand                     OpenAddDocumentCommand       { get; }
    public IRelayCommand<DocumentDto>        OpenEditDocumentCommand      { get; }
    public IAsyncRelayCommand<DocumentDto>   DeleteDocumentCommand        { get; }
    public IAsyncRelayCommand                SaveDocumentCommand          { get; }
    public IRelayCommand                     CancelEditCommand            { get; }

    public IAsyncRelayCommand<DocumentDto>              OpenSynopsisCommand      { get; }
    public IAsyncRelayCommand                           SaveSynopsisCommand      { get; }
    public IRelayCommand                                CancelSynopsisCommand    { get; }
    public IAsyncRelayCommand<DocumentSynopsisDto>      DeleteSynopsisCommand    { get; }
    public IRelayCommand<DocumentSynopsisDto>           EditSynopsisCommand      { get; }

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
        if(IsAdmin)
        {
            _ = LoadDocumentsCommand.ExecuteAsync(null);
            _ = LoadPickerDataAsync();
        }
    }

    // --- Role check ---
    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();
            if(string.IsNullOrWhiteSpace(token)) { IsAdmin = false; return; }
            IEnumerable<string> roles = _jwtService.GetRoles(token);
            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch { IsAdmin = false; }
    }

    // --- Load documents ---
    private async Task LoadDocumentsAsync()
    {
        try
        {
            IsLoading = true; HasError = false; ErrorMessage = string.Empty;
            Documents.Clear();
            List<DocumentDto> response = await _documentsService.GetAllDocumentsAsync();
            _allDocuments = response;
            foreach(DocumentDto d in response) Documents.Add(d);
            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading documents");
            ErrorMessage = _localizer["FailedToLoadDocuments"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    // --- Add document ---
    private void OpenAddDocument()
    {
        _editingDocumentId = null;
        EditPanelTitle = _localizer["AddDocumentDialog_Title"];
        ClearForm();
        IsEditingExisting  = false;
        IsEditingSynopsis  = false;
        IsEditing          = true;
    }

    // --- Edit document ---
    private async void OpenEditDocument(DocumentDto? document)
    {
        if(document?.Id == null) return;

        try
        {
            DocumentDto? full = await _documentsService.GetDocumentAsync(document.Id.Value);
            if(full == null) return;

            _editingDocumentId = document.Id;
            EditPanelTitle = _localizer["EditDocumentDialog_Title"];
            PopulateForm(full);
            await LoadAllJunctionsAsync(document.Id.Value);
            IsEditingExisting  = true;
            IsEditingSynopsis  = false;
            IsEditing          = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading document {Id}", document.Id);
            ErrorMessage = _localizer["FailedToLoadDocuments"];
            HasError = true;
        }
    }

    // --- Delete document ---
    private async Task DeleteDocumentAsync(DocumentDto? document)
    {
        if(document?.Id == null) return;

        try
        {
            await _documentsService.DeleteDocumentAsync(document.Id.Value);
            await LoadDocumentsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {Id}", document.Id);
            ErrorMessage = _localizer["FailedToDeleteDocument"];
            HasError = true;
        }
    }

    // --- Save document ---
    private async Task SaveDocumentAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(DocumentTitle))
            {
                ErrorMessage = _localizer["DocumentTitleRequired"];
                HasError = true;
                return;
            }

            var dto = new DocumentDto
            {
                Title       = DocumentTitle,
                NativeTitle = string.IsNullOrWhiteSpace(NativeTitle) ? null : NativeTitle,
                SortTitle   = string.IsNullOrWhiteSpace(SortTitle) ? null : SortTitle,
                Published   = Published,
                PublishedPrecision = PublishedPrecision,
                CountryId   = SelectedCountry?.Id
            };

            if(_editingDocumentId == null)
                await _documentsService.CreateDocumentAsync(dto);
            else
            {
                dto.Id = _editingDocumentId;
                await _documentsService.UpdateDocumentAsync(_editingDocumentId.Value, dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadDocumentsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving document");
            ErrorMessage = _localizer["FailedToSaveDocument"];
            HasError = true;
        }
    }

    // --- Cancel edit ---
    private void CancelEdit()
    {
        IsEditing = false;
        IsEditingExisting = false;
        _editingDocumentId = null;
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

    private async Task OpenSynopsisAsync(DocumentDto? document)
    {
        if(document?.Id == null) return;

        try
        {
            SynopsisDocumentId = document.Id;
            SynopsisText       = string.Empty;
            IsEditing          = false;
            ExistingSynopses.Clear();

            List<DocumentSynopsisDto> synopses = await _documentsService.GetSynopsesAsync(document.Id.Value);

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
            _logger.LogError(ex, "Error loading synopses for document {Id}", document.Id);
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
        if(SynopsisDocumentId == null || SelectedLanguage == null) return;

        try
        {
            var dto = new DocumentSynopsisDto
            {
                Text         = SynopsisText,
                LanguageCode = SelectedLanguage.Code
            };

            await _documentsService.UpsertSynopsisAsync(SynopsisDocumentId.Value, dto);

            ExistingSynopses.Clear();

            List<DocumentSynopsisDto> synopses =
                await _documentsService.GetSynopsesAsync(SynopsisDocumentId.Value);

            foreach(DocumentSynopsisDto s in synopses) ExistingSynopses.Add(s);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving synopsis");
            ErrorMessage = _localizer["FailedToSaveDocument"];
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
        if(SynopsisDocumentId == null || synopsis?.LanguageCode == null) return;

        try
        {
            await _documentsService.DeleteSynopsisAsync(SynopsisDocumentId.Value, synopsis.LanguageCode);
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
        IsEditingSynopsis  = false;
        SynopsisDocumentId = null;
        SynopsisText       = string.Empty;
        ExistingSynopses.Clear();
    }

    // ======================== JUNCTION MANAGEMENT ========================

    private async Task LoadAllJunctionsAsync(long documentId)
    {
        await Task.WhenAll(
            LoadDocumentPeopleAsync(documentId),
            LoadDocumentCompaniesAsync(documentId),
            LoadDocumentMachinesAsync(documentId),
            LoadDocumentMachineFamiliesAsync(documentId)
        );
    }

    // --- People ---
    private async Task LoadDocumentPeopleAsync(long documentId)
    {
        DocumentPeople.Clear(); DocumentPeopleDisplays.Clear();

        try
        {
            List<PersonByDocumentDto> items = await _documentsService.GetPeopleByDocumentAsync(documentId);

            foreach(PersonByDocumentDto p in items)
            {
                DocumentPeople.Add(p);
                string name = p.DisplayName ?? p.Alias ?? $"{p.Name} {p.Surname}".Trim();
                DocumentPeopleDisplays.Add($"{name} ({p.Role})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading people for document"); }
    }

    private async Task AddPersonAsync()
    {
        if(_editingDocumentId == null || SelectedAvailablePerson?.Id == null || SelectedPersonRole?.Id == null)
            return;

        try
        {
            await _documentsService.AddPersonToDocumentAsync(new PersonByDocumentDto
            {
                PersonId   = SelectedAvailablePerson.Id,
                DocumentId = _editingDocumentId,
                RoleId     = SelectedPersonRole.Id
            });

            SelectedAvailablePerson = null;
            await LoadDocumentPeopleAsync(_editingDocumentId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding person to document");
            ErrorMessage = _localizer["FailedToSaveDocument"];
            HasError = true;
        }
    }

    private async Task RemovePersonByDisplayAsync(string? display)
    {
        if(display == null || _editingDocumentId == null) return;

        int idx = DocumentPeopleDisplays.IndexOf(display);

        if(idx >= 0 && idx < DocumentPeople.Count && DocumentPeople[idx].Id.HasValue)
        {
            try
            {
                await _documentsService.RemovePersonFromDocumentAsync(DocumentPeople[idx].Id!.Value);
                await LoadDocumentPeopleAsync(_editingDocumentId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing person from document"); }
        }
    }

    // --- Companies ---
    private async Task LoadDocumentCompaniesAsync(long documentId)
    {
        DocumentCompanies.Clear(); DocumentCompanyDisplays.Clear();

        try
        {
            List<CompanyByDocumentDto> items = await _documentsService.GetCompaniesByDocumentAsync(documentId);

            foreach(CompanyByDocumentDto c in items)
            {
                DocumentCompanies.Add(c);
                DocumentCompanyDisplays.Add($"{c.Company} ({c.Role})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading companies for document"); }
    }

    private async Task AddCompanyAsync()
    {
        if(_editingDocumentId == null || SelectedAvailableCompany?.Id == null || SelectedCompanyRole?.Id == null)
            return;

        try
        {
            await _documentsService.AddCompanyToDocumentAsync(new CompanyByDocumentDto
            {
                CompanyId  = SelectedAvailableCompany.Id,
                DocumentId = _editingDocumentId,
                RoleId     = SelectedCompanyRole.Id
            });

            SelectedAvailableCompany = null;
            await LoadDocumentCompaniesAsync(_editingDocumentId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding company to document");
            ErrorMessage = _localizer["FailedToSaveDocument"];
            HasError = true;
        }
    }

    private async Task RemoveCompanyByDisplayAsync(string? display)
    {
        if(display == null || _editingDocumentId == null) return;

        int idx = DocumentCompanyDisplays.IndexOf(display);

        if(idx >= 0 && idx < DocumentCompanies.Count && DocumentCompanies[idx].Id.HasValue)
        {
            try
            {
                await _documentsService.RemoveCompanyFromDocumentAsync(DocumentCompanies[idx].Id!.Value);
                await LoadDocumentCompaniesAsync(_editingDocumentId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing company from document"); }
        }
    }

    // --- Machines ---
    private async Task LoadDocumentMachinesAsync(long documentId)
    {
        DocumentMachines.Clear(); DocumentMachineDisplays.Clear();

        try
        {
            List<DocumentByMachineDto> items = await _documentsService.GetMachinesByDocumentAsync(documentId);

            foreach(DocumentByMachineDto m in items)
            {
                DocumentMachines.Add(m);
                DocumentMachineDisplays.Add(m.Machine ?? $"Machine #{m.MachineId}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machines for document"); }

        RefreshAvailableMachines();
    }

    private async Task AddMachineAsync()
    {
        if(_editingDocumentId == null || SelectedAvailableMachine?.Id == null) return;

        try
        {
            await _documentsService.AddMachineToDocumentAsync(new DocumentByMachineDto
            {
                DocumentId = _editingDocumentId,
                MachineId  = SelectedAvailableMachine.Id
            });

            SelectedAvailableMachine = null;
            await LoadDocumentMachinesAsync(_editingDocumentId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine to document");
            ErrorMessage = _localizer["FailedToSaveDocument"];
            HasError = true;
        }
    }

    private async Task RemoveMachineByDisplayAsync(string? display)
    {
        if(display == null || _editingDocumentId == null) return;

        int idx = DocumentMachineDisplays.IndexOf(display);

        if(idx >= 0 && idx < DocumentMachines.Count && DocumentMachines[idx].Id.HasValue)
        {
            try
            {
                await _documentsService.RemoveMachineFromDocumentAsync(DocumentMachines[idx].Id!.Value);
                await LoadDocumentMachinesAsync(_editingDocumentId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing machine from document"); }
        }
    }

    // --- Machine Families ---
    private async Task LoadDocumentMachineFamiliesAsync(long documentId)
    {
        DocumentMachineFamilies.Clear(); DocumentMachineFamilyDisplays.Clear();

        try
        {
            List<DocumentByMachineFamilyDto> items =
                await _documentsService.GetMachineFamiliesByDocumentAsync(documentId);

            foreach(DocumentByMachineFamilyDto f in items)
            {
                DocumentMachineFamilies.Add(f);
                DocumentMachineFamilyDisplays.Add(f.MachineFamily ?? $"Family #{f.MachineFamilyId}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machine families for document"); }

        RefreshAvailableMachineFamilies();
    }

    private async Task AddMachineFamilyAsync()
    {
        if(_editingDocumentId == null || SelectedAvailableMachineFamily?.Id == null) return;

        try
        {
            await _documentsService.AddMachineFamilyToDocumentAsync(new DocumentByMachineFamilyDto
            {
                DocumentId      = _editingDocumentId,
                MachineFamilyId = SelectedAvailableMachineFamily.Id
            });

            SelectedAvailableMachineFamily = null;
            await LoadDocumentMachineFamiliesAsync(_editingDocumentId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine family to document");
            ErrorMessage = _localizer["FailedToSaveDocument"];
            HasError = true;
        }
    }

    private async Task RemoveMachineFamilyByDisplayAsync(string? display)
    {
        if(display == null || _editingDocumentId == null) return;

        int idx = DocumentMachineFamilyDisplays.IndexOf(display);

        if(idx >= 0 && idx < DocumentMachineFamilies.Count && DocumentMachineFamilies[idx].Id.HasValue)
        {
            try
            {
                await _documentsService.RemoveMachineFamilyFromDocumentAsync(
                    DocumentMachineFamilies[idx].Id!.Value);

                await LoadDocumentMachineFamiliesAsync(_editingDocumentId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing machine family from document"); }
        }
    }

    // ======================== HELPERS ========================

    private void RefreshAvailableMachines()
    {
        AvailableMachines.Clear();

        if(_allMachinesList == null) return;

        HashSet<int> assigned =
            new(DocumentMachines.Where(m => m.MachineId.HasValue).Select(m => m.MachineId!.Value));

        foreach(MachineDto m in _allMachinesList)
            if(m.Id.HasValue && !assigned.Contains(m.Id.Value))
                AvailableMachines.Add(m);
    }

    private void RefreshAvailableMachineFamilies()
    {
        AvailableMachineFamilies.Clear();

        if(_allMachineFamiliesList == null) return;

        HashSet<int> assigned =
            new(DocumentMachineFamilies.Where(f => f.MachineFamilyId.HasValue)
                                      .Select(f => f.MachineFamilyId!.Value));

        foreach(MachineFamilyDto f in _allMachineFamiliesList)
            if(f.Id.HasValue && !assigned.Contains(f.Id.Value))
                AvailableMachineFamilies.Add(f);
    }

    // --- Filtering ---
    public void ApplyFilter()
    {
        FilteredDocuments.Clear();
        IEnumerable<DocumentDto> source = (IEnumerable<DocumentDto>?)_allDocuments ?? Documents;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(d =>
                (d.Title != null      && d.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (d.NativeTitle != null && d.NativeTitle.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (d.SortTitle != null  && d.SortTitle.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));

        foreach(DocumentDto d in source) FilteredDocuments.Add(d);
    }

    // --- Load picker data ---
    public async Task LoadPickerDataAsync()
    {
        try
        {
            List<Iso31661NumericDto>? countriesResponse = await _apiClient.Iso31661Numeric.GetAsync();
            Countries.Clear();
            if(countriesResponse != null)
                foreach(Iso31661NumericDto c in countriesResponse) Countries.Add(c);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading countries"); }

        try
        {
            _allRolesList = await _documentsService.GetDocumentRolesAsync();
            AvailableRoles.Clear();
            foreach(DocumentRoleDto r in _allRolesList) AvailableRoles.Add(r);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading document roles"); }

        try { _allPeopleList = await _apiClient.People.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading people"); }

        try { _allCompaniesList = await _apiClient.Companies.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading companies"); }

        try { _allMachinesList = await _apiClient.Machines.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machines"); }

        try { _allMachineFamiliesList = await _apiClient.MachineFamilies.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machine families"); }
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

        foreach(PersonDto p in source) AvailablePeople.Add(p);
    }

    // --- Company search ---
    public void UpdateCompanySuggestions(string query)
    {
        AvailableCompanies.Clear();
        if(_allCompaniesList == null) return;

        IEnumerable<CompanyDto> source = _allCompaniesList;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(c =>
                c.Name != null && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(CompanyDto c in source) AvailableCompanies.Add(c);
    }

    // --- Form helpers ---
    private void ClearForm()
    {
        DocumentTitle    = string.Empty;
        NativeTitle      = string.Empty;
        SortTitle        = string.Empty;
        Published        = null;
        PublishedPrecision = 0;
        SelectedCountry  = null;

        DocumentPeople.Clear();          DocumentPeopleDisplays.Clear();
        DocumentCompanies.Clear();       DocumentCompanyDisplays.Clear();
        DocumentMachines.Clear();        DocumentMachineDisplays.Clear();
        DocumentMachineFamilies.Clear(); DocumentMachineFamilyDisplays.Clear();
        AvailableMachines.Clear();       AvailableMachineFamilies.Clear();
        SelectedAvailablePerson        = null;
        SelectedAvailableCompany       = null;
        SelectedAvailableMachine       = null;
        SelectedAvailableMachineFamily = null;
        SelectedPersonRole             = null;
        SelectedCompanyRole            = null;

        HasError     = false;
        ErrorMessage = string.Empty;
    }

    private void PopulateForm(DocumentDto document)
    {
        DocumentTitle = document.Title      ?? string.Empty;
        NativeTitle   = document.NativeTitle ?? string.Empty;
        SortTitle     = document.SortTitle   ?? string.Empty;
        Published     = document.Published;
        PublishedPrecision = document.PublishedPrecision ?? 0;

        SelectedCountry = document.CountryId.HasValue
                              ? Countries.FirstOrDefault(c => c.Id == document.CountryId.Value)
                              : null;
    }
}
