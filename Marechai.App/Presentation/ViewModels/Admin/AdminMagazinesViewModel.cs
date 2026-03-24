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

public partial class AdminMagazinesViewModel : ObservableObject, IRegionAware
{
    private readonly Client                          _apiClient;
    private readonly MagazinesService                   _magazinesService;
    private readonly IJwtService                        _jwtService;
    private readonly IStringLocalizer                   _localizer;
    private readonly ILogger<AdminMagazinesViewModel>   _logger;
    private readonly ITokenService                      _tokenService;

    // --- List state ---
    [ObservableProperty] private ObservableCollection<MagazineDto> _magazines = [];
    [ObservableProperty] private ObservableCollection<MagazineDto> _filteredMagazines = [];
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private MagazineDto? _selectedMagazine;
    private List<MagazineDto>? _allMagazines;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;

    // --- Edit panel state ---
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isEditingExisting;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    private long? _editingMagazineId;

    // --- Form fields ---
    [ObservableProperty] private string _magazineTitle = string.Empty;
    [ObservableProperty] private string _nativeTitle = string.Empty;
    [ObservableProperty] private string _sortTitle = string.Empty;
    [ObservableProperty] private string _issn = string.Empty;
    [ObservableProperty] private DateTimeOffset? _published;
    [ObservableProperty] private DateTimeOffset? _firstPublication;
    [ObservableProperty] private Iso31661NumericDto? _selectedCountry;

    // --- Synopsis panel state ---
    [ObservableProperty] private bool _isEditingSynopsis;
    [ObservableProperty] private string _synopsisText = string.Empty;
    [ObservableProperty] private long? _synopsisMagazineId;
    [ObservableProperty] private ObservableCollection<LanguageItem> _availableLanguages = [];
    [ObservableProperty] private LanguageItem? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<DocumentSynopsisDto> _existingSynopses = [];

    // --- Picker data ---
    [ObservableProperty] private ObservableCollection<Iso31661NumericDto> _countries = [];

    // --- People junction ---
    [ObservableProperty] private ObservableCollection<PersonByMagazineDto> _magazinePeople = [];
    [ObservableProperty] private ObservableCollection<string> _magazinePeopleDisplays = [];
    [ObservableProperty] private ObservableCollection<PersonDto> _availablePeople = [];
    [ObservableProperty] private PersonDto? _selectedAvailablePerson;
    [ObservableProperty] private ObservableCollection<DocumentRoleDto> _availableRoles = [];
    [ObservableProperty] private DocumentRoleDto? _selectedPersonRole;
    private List<PersonDto>? _allPeopleList;

    // --- Companies junction ---
    [ObservableProperty] private ObservableCollection<CompanyByMagazineDto> _magazineCompanies = [];
    [ObservableProperty] private ObservableCollection<string> _magazineCompanyDisplays = [];
    [ObservableProperty] private ObservableCollection<CompanyDto> _availableCompanies = [];
    [ObservableProperty] private CompanyDto? _selectedAvailableCompany;
    [ObservableProperty] private DocumentRoleDto? _selectedCompanyRole;
    private List<CompanyDto>? _allCompaniesList;

    // --- Machines junction ---
    [ObservableProperty] private ObservableCollection<MagazineByMachineDto> _magazineMachines = [];
    [ObservableProperty] private ObservableCollection<string> _magazineMachineDisplays = [];
    [ObservableProperty] private ObservableCollection<MachineDto> _availableMachines = [];
    [ObservableProperty] private MachineDto? _selectedAvailableMachine;
    private List<MachineDto>? _allMachinesList;

    // --- Machine Families junction ---
    [ObservableProperty] private ObservableCollection<MagazineByMachineFamilyDto> _magazineMachineFamilies = [];
    [ObservableProperty] private ObservableCollection<string> _magazineMachineFamilyDisplays = [];
    [ObservableProperty] private ObservableCollection<MachineFamilyDto> _availableMachineFamilies = [];
    [ObservableProperty] private MachineFamilyDto? _selectedAvailableMachineFamily;
    private List<MachineFamilyDto>? _allMachineFamiliesList;

    // --- Document roles ---
    private List<DocumentRoleDto>? _allRolesList;

    public AdminMagazinesViewModel(Client                          apiClient,
                                   MagazinesService                   magazinesService,
                                   IJwtService                        jwtService,
                                   ITokenService                      tokenService,
                                   ILogger<AdminMagazinesViewModel>   logger,
                                   IStringLocalizer                   localizer)
    {
        _apiClient        = apiClient;
        _magazinesService = magazinesService;
        _jwtService       = jwtService;
        _tokenService     = tokenService;
        _logger           = logger;
        _localizer        = localizer;

        LoadMagazinesCommand         = new AsyncRelayCommand(LoadMagazinesAsync);
        OpenAddMagazineCommand       = new RelayCommand(OpenAddMagazine);
        OpenEditMagazineCommand      = new RelayCommand<MagazineDto>(OpenEditMagazine);
        DeleteMagazineCommand        = new AsyncRelayCommand<MagazineDto>(DeleteMagazineAsync);
        SaveMagazineCommand          = new AsyncRelayCommand(SaveMagazineAsync);
        CancelEditCommand            = new RelayCommand(CancelEdit);

        // Synopsis commands
        OpenSynopsisCommand      = new AsyncRelayCommand<MagazineDto>(OpenSynopsisAsync);
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
    public IAsyncRelayCommand                LoadMagazinesCommand         { get; }
    public IRelayCommand                     OpenAddMagazineCommand       { get; }
    public IRelayCommand<MagazineDto>        OpenEditMagazineCommand      { get; }
    public IAsyncRelayCommand<MagazineDto>   DeleteMagazineCommand        { get; }
    public IAsyncRelayCommand                SaveMagazineCommand          { get; }
    public IRelayCommand                     CancelEditCommand            { get; }

    public IAsyncRelayCommand<MagazineDto>              OpenSynopsisCommand      { get; }
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
            _ = LoadMagazinesCommand.ExecuteAsync(null);
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
            IsAdmin = roles.Contains("Uberadmin",      StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
        }
        catch { IsAdmin = false; }
    }

    // --- Load magazines ---
    private async Task LoadMagazinesAsync()
    {
        try
        {
            IsLoading = true; HasError = false; ErrorMessage = string.Empty;
            Magazines.Clear();
            List<MagazineDto> response = await _magazinesService.GetAllMagazinesAsync();
            _allMagazines = response;
            foreach(MagazineDto m in response) Magazines.Add(m);
            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading magazines");
            ErrorMessage = _localizer["FailedToLoadMagazines"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    // --- Add magazine ---
    private void OpenAddMagazine()
    {
        _editingMagazineId = null;
        EditPanelTitle = _localizer["AddMagazineDialog_Title"];
        ClearForm();
        IsEditingExisting  = false;
        IsEditingSynopsis  = false;
        IsEditing          = true;
    }

    // --- Edit magazine ---
    private async void OpenEditMagazine(MagazineDto? magazine)
    {
        if(magazine?.Id == null) return;

        try
        {
            MagazineDto? full = await _magazinesService.GetMagazineAsync(magazine.Id.Value);
            if(full == null) return;

            _editingMagazineId = magazine.Id;
            EditPanelTitle = _localizer["EditMagazineDialog_Title"];
            PopulateForm(full);
            await LoadAllJunctionsAsync(magazine.Id.Value);
            IsEditingExisting  = true;
            IsEditingSynopsis  = false;
            IsEditing          = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading magazine {Id}", magazine.Id);
            ErrorMessage = _localizer["FailedToLoadMagazines"];
            HasError = true;
        }
    }

    // --- Delete magazine ---
    private async Task DeleteMagazineAsync(MagazineDto? magazine)
    {
        if(magazine?.Id == null) return;

        try
        {
            await _magazinesService.DeleteMagazineAsync(magazine.Id.Value);
            await LoadMagazinesAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting magazine {Id}", magazine.Id);
            ErrorMessage = _localizer["FailedToDeleteMagazine"];
            HasError = true;
        }
    }

    // --- Save magazine ---
    private async Task SaveMagazineAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(MagazineTitle))
            {
                ErrorMessage = _localizer["MagazineTitleRequired"];
                HasError = true;
                return;
            }

            var dto = new MagazineDto
            {
                Title            = MagazineTitle,
                NativeTitle      = string.IsNullOrWhiteSpace(NativeTitle) ? null : NativeTitle,
                SortTitle        = string.IsNullOrWhiteSpace(SortTitle) ? null : SortTitle,
                Issn             = string.IsNullOrWhiteSpace(Issn) ? null : Issn,
                Published        = Published,
                FirstPublication = FirstPublication,
                CountryId        = SelectedCountry?.Id
            };

            if(_editingMagazineId == null)
                await _magazinesService.CreateMagazineAsync(dto);
            else
            {
                dto.Id = _editingMagazineId;
                await _magazinesService.UpdateMagazineAsync(_editingMagazineId.Value, dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadMagazinesAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving magazine");
            ErrorMessage = _localizer["FailedToSaveMagazine"];
            HasError = true;
        }
    }

    // --- Cancel edit ---
    private void CancelEdit()
    {
        IsEditing = false;
        IsEditingExisting = false;
        _editingMagazineId = null;
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

    private async Task OpenSynopsisAsync(MagazineDto? magazine)
    {
        if(magazine?.Id == null) return;

        try
        {
            SynopsisMagazineId = magazine.Id;
            SynopsisText       = string.Empty;
            IsEditing          = false;
            ExistingSynopses.Clear();

            List<DocumentSynopsisDto> synopses = await _magazinesService.GetSynopsesAsync(magazine.Id.Value);

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
            _logger.LogError(ex, "Error loading synopses for magazine {Id}", magazine.Id);
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
        if(SynopsisMagazineId == null || SelectedLanguage == null) return;

        try
        {
            var dto = new DocumentSynopsisDto
            {
                Text         = SynopsisText,
                LanguageCode = SelectedLanguage.Code
            };

            await _magazinesService.UpsertSynopsisAsync(SynopsisMagazineId.Value, dto);

            ExistingSynopses.Clear();

            List<DocumentSynopsisDto> synopses =
                await _magazinesService.GetSynopsesAsync(SynopsisMagazineId.Value);

            foreach(DocumentSynopsisDto s in synopses) ExistingSynopses.Add(s);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving synopsis");
            ErrorMessage = _localizer["FailedToSaveMagazine"];
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
        if(SynopsisMagazineId == null || synopsis?.LanguageCode == null) return;

        try
        {
            await _magazinesService.DeleteSynopsisAsync(SynopsisMagazineId.Value, synopsis.LanguageCode);
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
        SynopsisMagazineId = null;
        SynopsisText       = string.Empty;
        ExistingSynopses.Clear();
    }

    // ======================== JUNCTION MANAGEMENT ========================

    private async Task LoadAllJunctionsAsync(long magazineId)
    {
        await Task.WhenAll(
            LoadMagazinePeopleAsync(magazineId),
            LoadMagazineCompaniesAsync(magazineId),
            LoadMagazineMachinesAsync(magazineId),
            LoadMagazineMachineFamiliesAsync(magazineId)
        );
    }

    // --- People ---
    private async Task LoadMagazinePeopleAsync(long magazineId)
    {
        MagazinePeople.Clear(); MagazinePeopleDisplays.Clear();

        try
        {
            List<PersonByMagazineDto> items = await _magazinesService.GetPeopleByMagazineAsync(magazineId);

            foreach(PersonByMagazineDto p in items)
            {
                MagazinePeople.Add(p);
                string name = p.DisplayName ?? p.Alias ?? $"{p.Name} {p.Surname}".Trim();
                MagazinePeopleDisplays.Add($"{name} ({p.Role})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading people for magazine"); }
    }

    private async Task AddPersonAsync()
    {
        if(_editingMagazineId == null || SelectedAvailablePerson?.Id == null || SelectedPersonRole?.Id == null)
            return;

        try
        {
            await _magazinesService.AddPersonToMagazineAsync(new PersonByMagazineDto
            {
                PersonId   = SelectedAvailablePerson.Id,
                MagazineId = _editingMagazineId,
                RoleId     = SelectedPersonRole.Id
            });

            SelectedAvailablePerson = null;
            await LoadMagazinePeopleAsync(_editingMagazineId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding person to magazine");
            ErrorMessage = _localizer["FailedToSaveMagazine"];
            HasError = true;
        }
    }

    private async Task RemovePersonByDisplayAsync(string? display)
    {
        if(display == null || _editingMagazineId == null) return;

        int idx = MagazinePeopleDisplays.IndexOf(display);

        if(idx >= 0 && idx < MagazinePeople.Count && MagazinePeople[idx].Id.HasValue)
        {
            try
            {
                await _magazinesService.RemovePersonFromMagazineAsync(MagazinePeople[idx].Id!.Value);
                await LoadMagazinePeopleAsync(_editingMagazineId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing person from magazine"); }
        }
    }

    // --- Companies ---
    private async Task LoadMagazineCompaniesAsync(long magazineId)
    {
        MagazineCompanies.Clear(); MagazineCompanyDisplays.Clear();

        try
        {
            List<CompanyByMagazineDto> items = await _magazinesService.GetCompaniesByMagazineAsync(magazineId);

            foreach(CompanyByMagazineDto c in items)
            {
                MagazineCompanies.Add(c);
                MagazineCompanyDisplays.Add($"{c.Company} ({c.Role})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading companies for magazine"); }
    }

    private async Task AddCompanyAsync()
    {
        if(_editingMagazineId == null || SelectedAvailableCompany?.Id == null || SelectedCompanyRole?.Id == null)
            return;

        try
        {
            await _magazinesService.AddCompanyToMagazineAsync(new CompanyByMagazineDto
            {
                CompanyId  = SelectedAvailableCompany.Id,
                MagazineId = _editingMagazineId,
                RoleId     = SelectedCompanyRole.Id
            });

            SelectedAvailableCompany = null;
            await LoadMagazineCompaniesAsync(_editingMagazineId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding company to magazine");
            ErrorMessage = _localizer["FailedToSaveMagazine"];
            HasError = true;
        }
    }

    private async Task RemoveCompanyByDisplayAsync(string? display)
    {
        if(display == null || _editingMagazineId == null) return;

        int idx = MagazineCompanyDisplays.IndexOf(display);

        if(idx >= 0 && idx < MagazineCompanies.Count && MagazineCompanies[idx].Id.HasValue)
        {
            try
            {
                await _magazinesService.RemoveCompanyFromMagazineAsync(MagazineCompanies[idx].Id!.Value);
                await LoadMagazineCompaniesAsync(_editingMagazineId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing company from magazine"); }
        }
    }

    // --- Machines ---
    private async Task LoadMagazineMachinesAsync(long magazineId)
    {
        MagazineMachines.Clear(); MagazineMachineDisplays.Clear();

        try
        {
            List<MagazineByMachineDto> items = await _magazinesService.GetMachinesByMagazineAsync(magazineId);

            foreach(MagazineByMachineDto m in items)
            {
                MagazineMachines.Add(m);
                MagazineMachineDisplays.Add(m.Machine ?? $"Machine #{m.MachineId}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machines for magazine"); }

        RefreshAvailableMachines();
    }

    private async Task AddMachineAsync()
    {
        if(_editingMagazineId == null || SelectedAvailableMachine?.Id == null) return;

        try
        {
            await _magazinesService.AddMachineToMagazineAsync(new MagazineByMachineDto
            {
                MagazineId = _editingMagazineId,
                MachineId  = SelectedAvailableMachine.Id
            });

            SelectedAvailableMachine = null;
            await LoadMagazineMachinesAsync(_editingMagazineId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine to magazine");
            ErrorMessage = _localizer["FailedToSaveMagazine"];
            HasError = true;
        }
    }

    private async Task RemoveMachineByDisplayAsync(string? display)
    {
        if(display == null || _editingMagazineId == null) return;

        int idx = MagazineMachineDisplays.IndexOf(display);

        if(idx >= 0 && idx < MagazineMachines.Count && MagazineMachines[idx].Id.HasValue)
        {
            try
            {
                await _magazinesService.RemoveMachineFromMagazineAsync(MagazineMachines[idx].Id!.Value);
                await LoadMagazineMachinesAsync(_editingMagazineId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing machine from magazine"); }
        }
    }

    // --- Machine Families ---
    private async Task LoadMagazineMachineFamiliesAsync(long magazineId)
    {
        MagazineMachineFamilies.Clear(); MagazineMachineFamilyDisplays.Clear();

        try
        {
            List<MagazineByMachineFamilyDto> items =
                await _magazinesService.GetMachineFamiliesByMagazineAsync(magazineId);

            foreach(MagazineByMachineFamilyDto f in items)
            {
                MagazineMachineFamilies.Add(f);
                MagazineMachineFamilyDisplays.Add(f.MachineFamily ?? $"Family #{f.MachineFamilyId}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machine families for magazine"); }

        RefreshAvailableMachineFamilies();
    }

    private async Task AddMachineFamilyAsync()
    {
        if(_editingMagazineId == null || SelectedAvailableMachineFamily?.Id == null) return;

        try
        {
            await _magazinesService.AddMachineFamilyToMagazineAsync(new MagazineByMachineFamilyDto
            {
                MagazineId      = _editingMagazineId,
                MachineFamilyId = SelectedAvailableMachineFamily.Id
            });

            SelectedAvailableMachineFamily = null;
            await LoadMagazineMachineFamiliesAsync(_editingMagazineId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine family to magazine");
            ErrorMessage = _localizer["FailedToSaveMagazine"];
            HasError = true;
        }
    }

    private async Task RemoveMachineFamilyByDisplayAsync(string? display)
    {
        if(display == null || _editingMagazineId == null) return;

        int idx = MagazineMachineFamilyDisplays.IndexOf(display);

        if(idx >= 0 && idx < MagazineMachineFamilies.Count && MagazineMachineFamilies[idx].Id.HasValue)
        {
            try
            {
                await _magazinesService.RemoveMachineFamilyFromMagazineAsync(
                    MagazineMachineFamilies[idx].Id!.Value);

                await LoadMagazineMachineFamiliesAsync(_editingMagazineId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing machine family from magazine"); }
        }
    }

    // ======================== HELPERS ========================

    private void RefreshAvailableMachines()
    {
        AvailableMachines.Clear();

        if(_allMachinesList == null) return;

        HashSet<int> assigned =
            new(MagazineMachines.Where(m => m.MachineId.HasValue).Select(m => m.MachineId!.Value));

        foreach(MachineDto m in _allMachinesList)
            if(m.Id.HasValue && !assigned.Contains(m.Id.Value))
                AvailableMachines.Add(m);
    }

    private void RefreshAvailableMachineFamilies()
    {
        AvailableMachineFamilies.Clear();

        if(_allMachineFamiliesList == null) return;

        HashSet<int> assigned =
            new(MagazineMachineFamilies.Where(f => f.MachineFamilyId.HasValue)
                                       .Select(f => f.MachineFamilyId!.Value));

        foreach(MachineFamilyDto f in _allMachineFamiliesList)
            if(f.Id.HasValue && !assigned.Contains(f.Id.Value))
                AvailableMachineFamilies.Add(f);
    }

    // --- Filtering ---
    public void ApplyFilter()
    {
        FilteredMagazines.Clear();
        IEnumerable<MagazineDto> source = (IEnumerable<MagazineDto>?)_allMagazines ?? Magazines;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(m =>
                (m.Title != null      && m.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (m.NativeTitle != null && m.NativeTitle.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (m.SortTitle != null  && m.SortTitle.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (m.Issn != null       && m.Issn.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));

        foreach(MagazineDto m in source) FilteredMagazines.Add(m);
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
            _allRolesList = await _magazinesService.GetDocumentRolesAsync();
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
        MagazineTitle    = string.Empty;
        NativeTitle      = string.Empty;
        SortTitle        = string.Empty;
        Issn             = string.Empty;
        Published        = null;
        FirstPublication = null;
        SelectedCountry  = null;

        MagazinePeople.Clear();          MagazinePeopleDisplays.Clear();
        MagazineCompanies.Clear();       MagazineCompanyDisplays.Clear();
        MagazineMachines.Clear();        MagazineMachineDisplays.Clear();
        MagazineMachineFamilies.Clear(); MagazineMachineFamilyDisplays.Clear();
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

    private void PopulateForm(MagazineDto magazine)
    {
        MagazineTitle    = magazine.Title       ?? string.Empty;
        NativeTitle      = magazine.NativeTitle  ?? string.Empty;
        SortTitle        = magazine.SortTitle    ?? string.Empty;
        Issn             = magazine.Issn         ?? string.Empty;
        Published        = magazine.Published;
        FirstPublication = magazine.FirstPublication;

        SelectedCountry = magazine.CountryId.HasValue
                              ? Countries.FirstOrDefault(c => c.Id == magazine.CountryId.Value)
                              : null;
    }
}
