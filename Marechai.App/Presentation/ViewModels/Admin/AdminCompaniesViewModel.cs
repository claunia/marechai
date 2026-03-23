#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminCompaniesViewModel : ObservableObject, IRegionAware
{
    private readonly ApiClient                        _apiClient;
    private readonly IJwtService                      _jwtService;
    private readonly IStringLocalizer                 _localizer;
    private readonly ILogger<AdminCompaniesViewModel> _logger;
    private readonly IRegionManager                   _regionManager;
    private readonly ITokenService                    _tokenService;

    // --- List state ---
    [ObservableProperty]
    private ObservableCollection<CompanyDto> _companies = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDto> _filteredCompanies = [];

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private CompanyDto? _selectedCompany;

    private List<CompanyDto>? _allCompanies;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    // --- Edit panel state ---
    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editPanelTitle = string.Empty;

    private int? _editingCompanyId;

    // --- Form fields ---
    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string _legalName = string.Empty;

    [ObservableProperty]
    private int _statusIndex;

    [ObservableProperty]
    private DateTimeOffset? _founded;

    [ObservableProperty]
    private bool _foundedDayIsUnknown;

    [ObservableProperty]
    private bool _foundedMonthIsUnknown;

    [ObservableProperty]
    private DateTimeOffset? _sold;

    [ObservableProperty]
    private bool _soldDayIsUnknown;

    [ObservableProperty]
    private bool _soldMonthIsUnknown;

    [ObservableProperty]
    private string _website = string.Empty;

    [ObservableProperty]
    private string _twitter = string.Empty;

    [ObservableProperty]
    private string _facebook = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _city = string.Empty;

    [ObservableProperty]
    private string _province = string.Empty;

    [ObservableProperty]
    private string _postalCode = string.Empty;

    [ObservableProperty]
    private Iso31661NumericDto? _selectedCountry;

    [ObservableProperty]
    private CompanyDto? _selectedSoldToCompany;

    [ObservableProperty]
    private string _soldToSearchText = string.Empty;

    // --- Description panel state ---
    [ObservableProperty]
    private bool _isEditingDescription;

    [ObservableProperty]
    private string _descriptionMarkdown = string.Empty;

    [ObservableProperty]
    private int? _descriptionCompanyId;

    [ObservableProperty]
    private ObservableCollection<LanguageItem> _availableLanguages = [];

    [ObservableProperty]
    private LanguageItem? _selectedLanguage;

    [ObservableProperty]
    private ObservableCollection<CompanyDescriptionDto> _existingTranslations = [];

    // --- Picker data ---
    [ObservableProperty]
    private ObservableCollection<Iso31661NumericDto> _countries = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDto> _soldToSuggestions = [];

    private List<CompanyDto>? _allCompaniesForSearch;

    // --- Status items for ComboBox (index must match CompanyStatus enum) ---
    [ObservableProperty]
    private List<string> _statusItems = [];

    // --- People junction state ---
    [ObservableProperty]
    private ObservableCollection<PersonByCompanyDto> _companyPeople = [];

    [ObservableProperty]
    private ObservableCollection<string> _companyPeopleDisplays = [];

    [ObservableProperty]
    private ObservableCollection<PersonDto> _availablePeople = [];

    [ObservableProperty]
    private PersonDto? _selectedAvailablePerson;

    [ObservableProperty]
    private string _personSearchText = string.Empty;

    [ObservableProperty]
    private string _newPersonPosition = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _newPersonStart;

    [ObservableProperty]
    private DateTimeOffset? _newPersonEnd;

    [ObservableProperty]
    private bool _newPersonOngoing;

    [ObservableProperty]
    private bool _isEditingExisting;

    // When non-null, we are editing an existing person-company record
    private long? _editingPersonCompanyId;

    private List<PersonDto>? _allPeopleList;

    public AdminCompaniesViewModel(ApiClient                        apiClient,
                                   IJwtService                      jwtService,
                                   ITokenService                    tokenService,
                                   ILogger<AdminCompaniesViewModel> logger,
                                   IStringLocalizer                 localizer,
                                   IRegionManager                   regionManager)
    {
        _apiClient    = apiClient;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;
        _regionManager = regionManager;

        StatusItems =
        [
            localizer["StatusUnknown"],
            localizer["StatusActive"],
            localizer["StatusSold"],
            localizer["StatusMerged"],
            localizer["StatusBankrupt"],
            localizer["StatusDefunct"],
            localizer["StatusRenamed"]
        ];

        LoadCompaniesCommand       = new AsyncRelayCommand(LoadCompaniesAsync);
        OpenAddCompanyCommand      = new RelayCommand(OpenAddCompany);
        OpenEditCompanyCommand     = new RelayCommand<CompanyDto>(OpenEditCompany);
        DeleteCompanyCommand       = new AsyncRelayCommand<CompanyDto>(DeleteCompanyAsync);
        SaveCompanyCommand         = new AsyncRelayCommand(SaveCompanyAsync);
        CancelEditCommand          = new RelayCommand(CancelEdit);
        OpenDescriptionCommand     = new AsyncRelayCommand<CompanyDto>(OpenDescriptionAsync);
        SaveDescriptionCommand     = new AsyncRelayCommand(SaveDescriptionAsync);
        CancelDescriptionCommand   = new RelayCommand(CancelDescription);
        DeleteTranslationCommand   = new AsyncRelayCommand<CompanyDescriptionDto>(DeleteTranslationAsync);
        EditTranslationCommand     = new RelayCommand<CompanyDescriptionDto>(EditTranslation);
        OpenLogosCommand           = new RelayCommand<CompanyDto>(OpenLogos);
        AddPersonCommand           = new AsyncRelayCommand(AddPersonAsync);
        RemovePersonCommand        = new AsyncRelayCommand<string>(RemovePersonByDisplayAsync);
        EditPersonCommand          = new RelayCommand<string>(EditPerson);
        SavePersonEditCommand      = new AsyncRelayCommand(SavePersonEditAsync);
        CancelPersonEditCommand    = new RelayCommand(CancelPersonEdit);

        InitializeLanguages();

        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand             LoadCompaniesCommand     { get; }
    public IRelayCommand                  OpenAddCompanyCommand    { get; }
    public IRelayCommand<CompanyDto>      OpenEditCompanyCommand   { get; }
    public IAsyncRelayCommand<CompanyDto> DeleteCompanyCommand     { get; }
    public IAsyncRelayCommand             SaveCompanyCommand       { get; }
    public IRelayCommand                  CancelEditCommand        { get; }
    public IAsyncRelayCommand<CompanyDto>             OpenDescriptionCommand   { get; }
    public IAsyncRelayCommand                        SaveDescriptionCommand   { get; }
    public IRelayCommand                             CancelDescriptionCommand { get; }
    public IAsyncRelayCommand<CompanyDescriptionDto> DeleteTranslationCommand { get; }
    public IRelayCommand<CompanyDescriptionDto>      EditTranslationCommand   { get; }
    public IRelayCommand<CompanyDto>                 OpenLogosCommand         { get; }
    public IAsyncRelayCommand                        AddPersonCommand         { get; }
    public IAsyncRelayCommand<string>                RemovePersonCommand      { get; }
    public IRelayCommand<string>                     EditPersonCommand        { get; }
    public IAsyncRelayCommand                        SavePersonEditCommand    { get; }
    public IRelayCommand                             CancelPersonEditCommand  { get; }

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
            _ = LoadCompaniesCommand.ExecuteAsync(null);
    }

    // --- Role check ---
    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(string.IsNullOrWhiteSpace(token))
            {
                IsAdmin = false;

                return;
            }

            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("Uberadmin",      StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    // --- Load companies ---
    private async Task LoadCompaniesAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Companies.Clear();

            List<CompanyDto>? response = await _apiClient.Companies.GetAsync();
            _allCompanies = response;

            // Also use as search source so we don't need a second API call
            _allCompaniesForSearch ??= response;

            if(response != null)
            {
                // Resolve SoldTo names client-side (server only returns SoldToId)
                foreach(CompanyDto company in response)
                {
                    if(company.SoldToId.HasValue && company.SoldTo == null)
                        company.SoldTo = response.FirstOrDefault(c => c.Id == company.SoldToId.Value)?.Name;

                    Companies.Add(company);
                }
            }

            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading companies");
            ErrorMessage = _localizer["FailedToLoadCompanies"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- Add company ---
    private void OpenAddCompany()
    {
        _editingCompanyId = null;
        EditPanelTitle    = _localizer["AddCompanyDialog_Title"];
        ClearForm();
        IsEditingExisting    = false;
        IsEditingDescription = false;
        IsEditing            = true;
    }

    // --- Edit company ---
    private void OpenEditCompany(CompanyDto? company)
    {
        if(company == null) return;

        _editingCompanyId = company.Id;
        EditPanelTitle    = _localizer["EditCompanyDialog_Title"];
        PopulateForm(company);
        IsEditingExisting    = true;
        IsEditingDescription = false;
        IsEditing            = true;

        if(company.Id != null)
            _ = LoadCompanyPeopleAsync(company.Id.Value);
    }

    // --- Delete company ---
    private async Task DeleteCompanyAsync(CompanyDto? company)
    {
        if(company?.Id == null) return;

        try
        {
            await _apiClient.Companies[company.Id.Value].DeleteAsync();
            await LoadCompaniesAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting company {Id}", company.Id);
            ErrorMessage = _localizer["FailedToDeleteCompany"];
            HasError     = true;
        }
    }

    // --- Save company ---
    private async Task SaveCompanyAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(CompanyName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError     = true;

                return;
            }

            var dto = new CompanyDto
            {
                Name                 = CompanyName,
                LegalName            = string.IsNullOrWhiteSpace(LegalName) ? null : LegalName,
                Status               = StatusIndex,
                Founded              = Founded,
                FoundedDayIsUnknown  = FoundedDayIsUnknown,
                FoundedMonthIsUnknown= FoundedMonthIsUnknown,
                Sold                 = Sold,
                SoldDayIsUnknown     = SoldDayIsUnknown,
                SoldMonthIsUnknown   = SoldMonthIsUnknown,
                SoldToId             = SelectedSoldToCompany?.Id,
                Website              = string.IsNullOrWhiteSpace(Website)    ? null : Website,
                Twitter              = string.IsNullOrWhiteSpace(Twitter)    ? null : Twitter,
                Facebook             = string.IsNullOrWhiteSpace(Facebook)   ? null : Facebook,
                Address              = string.IsNullOrWhiteSpace(Address)    ? null : Address,
                City                 = string.IsNullOrWhiteSpace(City)       ? null : City,
                Province             = string.IsNullOrWhiteSpace(Province)   ? null : Province,
                PostalCode           = string.IsNullOrWhiteSpace(PostalCode) ? null : PostalCode,
                CountryId            = SelectedCountry?.Id
            };

            if(_editingCompanyId == null)
                await _apiClient.Companies.PostAsync(dto);
            else
            {
                dto.Id = _editingCompanyId;
                await _apiClient.Companies[_editingCompanyId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadCompaniesAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving company");
            ErrorMessage = _localizer["FailedToSaveCompany"];
            HasError     = true;
        }
    }

    // --- Cancel edit ---
    private void CancelEdit()
    {
        IsEditing         = false;
        _editingCompanyId = null;
        ClearForm();
        HasError     = false;
        ErrorMessage = string.Empty;
    }

    // --- Description ---
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

        SelectedLanguage = AvailableLanguages[0]; // Default to English
    }

    private async Task OpenDescriptionAsync(CompanyDto? company)
    {
        if(company?.Id == null) return;

        try
        {
            DescriptionCompanyId = company.Id;
            DescriptionMarkdown  = string.Empty;
            IsEditing            = false;
            ExistingTranslations.Clear();

            // Load all existing translations
            List<CompanyDescriptionDto>? translations =
                await _apiClient.Companies[company.Id.Value].Descriptions.GetAsync();

            if(translations != null)
                foreach(CompanyDescriptionDto t in translations)
                    ExistingTranslations.Add(t);

            // Default to English or first untranslated language
            SelectedLanguage = AvailableLanguages.FirstOrDefault(l =>
                                   ExistingTranslations.All(t => t.LanguageCode != l.Code)) ??
                               AvailableLanguages[0];

            // If selected language already has content, load it
            CompanyDescriptionDto? existing =
                ExistingTranslations.FirstOrDefault(t => t.LanguageCode == SelectedLanguage.Code);

            if(existing != null)
                DescriptionMarkdown = existing.Markdown ?? string.Empty;

            IsEditingDescription = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading description for company {Id}", company.Id);
            DescriptionMarkdown  = string.Empty;
            IsEditingDescription = true;
        }
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        if(value == null || ExistingTranslations.Count == 0)
        {
            DescriptionMarkdown = string.Empty;

            return;
        }

        CompanyDescriptionDto? existing = ExistingTranslations.FirstOrDefault(t => t.LanguageCode == value.Code);
        DescriptionMarkdown = existing?.Markdown ?? string.Empty;
    }

    private async Task SaveDescriptionAsync()
    {
        if(DescriptionCompanyId == null || SelectedLanguage == null) return;

        try
        {
            var dto = new CompanyDescriptionDto
            {
                CompanyId    = DescriptionCompanyId.Value,
                Markdown     = DescriptionMarkdown,
                LanguageCode = SelectedLanguage.Code
            };

            await _apiClient.Companies[DescriptionCompanyId.Value].Description.PostAsync(dto);

            // Refresh translations list
            ExistingTranslations.Clear();

            List<CompanyDescriptionDto>? translations =
                await _apiClient.Companies[DescriptionCompanyId.Value].Descriptions.GetAsync();

            if(translations != null)
                foreach(CompanyDescriptionDto t in translations)
                    ExistingTranslations.Add(t);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving description");
            ErrorMessage = _localizer["FailedToSaveDescription"];
            HasError     = true;
        }
    }

    private void EditTranslation(CompanyDescriptionDto? translation)
    {
        if(translation?.LanguageCode == null) return;

        SelectedLanguage    = AvailableLanguages.FirstOrDefault(l => l.Code == translation.LanguageCode);
        DescriptionMarkdown = translation.Markdown ?? string.Empty;
    }

    private async Task DeleteTranslationAsync(CompanyDescriptionDto? translation)
    {
        if(DescriptionCompanyId == null || translation?.LanguageCode == null) return;

        try
        {
            await _apiClient.Companies[DescriptionCompanyId.Value].Description[translation.LanguageCode].DeleteAsync();

            ExistingTranslations.Remove(translation);
            DescriptionMarkdown = string.Empty;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting translation");
            ErrorMessage = _localizer["FailedToDeleteTranslation"];
            HasError     = true;
        }
    }

    private void CancelDescription()
    {
        IsEditingDescription = false;
        DescriptionCompanyId = null;
        DescriptionMarkdown  = string.Empty;
        ExistingTranslations.Clear();
    }

    // --- Logos navigation ---
    private void OpenLogos(CompanyDto? company)
    {
        if(company?.Id == null) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.CompanyId, company.Id.Value },
            { NavParamKeys.CompanyName, company.Name ?? string.Empty }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminCompanyLogosPage), parameters);
    }

    // --- Filtering ---
    public void ApplyFilter()
    {
        FilteredCompanies.Clear();

        IEnumerable<CompanyDto> source = (IEnumerable<CompanyDto>?)_allCompanies ?? Companies;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(c => (c.Name != null &&
                                        c.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (c.LegalName != null &&
                                        c.LegalName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (c.Country != null &&
                                        c.Country.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));

        foreach(CompanyDto company in source)
            FilteredCompanies.Add(company);
    }

    // --- SoldTo search ---
    public void UpdateSoldToSuggestions(string query)
    {
        SoldToSuggestions.Clear();

        if(_allCompaniesForSearch == null) return;

        IEnumerable<CompanyDto> source = _allCompaniesForSearch;

        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(c => c.Name != null &&
                                       c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(CompanyDto match in source)
            SoldToSuggestions.Add(match);
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
                    foreach(Iso31661NumericDto c in countriesResponse)
                        Countries.Add(c);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error loading countries");
            }
        }

        if(_allCompaniesForSearch == null)
        {
            try
            {
                _allCompaniesForSearch = await _apiClient.Companies.GetAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error loading companies for search");
            }
        }

        if(_allPeopleList == null)
        {
            try
            {
                _allPeopleList = await _apiClient.People.GetAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error loading people");
            }
        }
    }

    // --- Helpers ---
    private void ClearForm()
    {
        CompanyName           = string.Empty;
        LegalName             = string.Empty;
        StatusIndex           = 0;
        Founded               = null;
        FoundedDayIsUnknown   = false;
        FoundedMonthIsUnknown = false;
        Sold                  = null;
        SoldDayIsUnknown      = false;
        SoldMonthIsUnknown    = false;
        Website               = string.Empty;
        Twitter               = string.Empty;
        Facebook              = string.Empty;
        Address               = string.Empty;
        City                  = string.Empty;
        Province              = string.Empty;
        PostalCode            = string.Empty;
        SelectedCountry       = null;
        SelectedSoldToCompany = null;
        SoldToSearchText      = string.Empty;
        HasError              = false;
        ErrorMessage          = string.Empty;
        CompanyPeople.Clear();
        CompanyPeopleDisplays.Clear();
        ClearPersonForm();
    }

    private void PopulateForm(CompanyDto company)
    {
        CompanyName           = company.Name           ?? string.Empty;
        LegalName             = company.LegalName      ?? string.Empty;
        StatusIndex           = company.Status         ?? 0;
        Founded               = company.Founded;
        FoundedDayIsUnknown   = company.FoundedDayIsUnknown   ?? false;
        FoundedMonthIsUnknown = company.FoundedMonthIsUnknown ?? false;
        Sold                  = company.Sold;
        SoldDayIsUnknown      = company.SoldDayIsUnknown      ?? false;
        SoldMonthIsUnknown    = company.SoldMonthIsUnknown    ?? false;
        Website               = company.Website        ?? string.Empty;
        Twitter               = company.Twitter        ?? string.Empty;
        Facebook              = company.Facebook       ?? string.Empty;
        Address               = company.Address        ?? string.Empty;
        City                  = company.City           ?? string.Empty;
        Province              = company.Province       ?? string.Empty;
        PostalCode            = company.PostalCode     ?? string.Empty;
        // Find matching country
        SelectedCountry = company.CountryId.HasValue
                              ? Countries.FirstOrDefault(c => c.Id == company.CountryId.Value)
                              : null;

        // Set SoldTo: populate the ComboBox source first, then select the matching item
        if(company.SoldToId.HasValue && _allCompaniesForSearch != null)
        {
            CompanyDto? soldToCompany = _allCompaniesForSearch.FirstOrDefault(c => c.Id == company.SoldToId.Value);

            if(soldToCompany != null)
            {
                SoldToSearchText = soldToCompany.Name ?? string.Empty;
                UpdateSoldToSuggestions(SoldToSearchText);
                SelectedSoldToCompany = SoldToSuggestions.FirstOrDefault(c => c.Id == soldToCompany.Id);
            }
            else
            {
                SoldToSearchText      = string.Empty;
                SelectedSoldToCompany = null;
            }
        }
        else
        {
            SoldToSearchText      = string.Empty;
            SelectedSoldToCompany = null;
        }
    }

    public string GetStatusDisplay(int? status)
    {
        return status switch
        {
            0 => _localizer["StatusUnknown"],
            1 => _localizer["StatusActive"],
            2 => _localizer["StatusSold"],
            3 => _localizer["StatusMerged"],
            4 => _localizer["StatusBankrupt"],
            5 => _localizer["StatusDefunct"],
            6 => _localizer["StatusRenamed"],
            _ => _localizer["StatusUnknown"]
        };
    }

    // --- People junction methods ---

    private async Task LoadCompanyPeopleAsync(int companyId)
    {
        CompanyPeople.Clear();
        CompanyPeopleDisplays.Clear();

        try
        {
            List<PersonByCompanyDto>? items = await _apiClient.Companies[companyId].People.GetAsync();

            if(items != null)
            {
                foreach(PersonByCompanyDto p in items)
                {
                    CompanyPeople.Add(p);
                    CompanyPeopleDisplays.Add(FormatPersonDisplay(p));
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading people for company {Id}", companyId);
        }
    }

    private static string FormatPersonDisplay(PersonByCompanyDto p)
    {
        string name = p.DisplayName ?? p.Alias ?? $"{p.Name} {p.Surname}".Trim();
        string display = name;

        if(!string.IsNullOrWhiteSpace(p.Position))
            display += $" — {p.Position}";

        if(p.Ongoing == true)
            display += " (Ongoing)";
        else if(p.Start != null || p.End != null)
        {
            string start = p.Start?.ToString("yyyy") ?? "?";
            string end   = p.End?.ToString("yyyy")   ?? "?";
            display += $" ({start}–{end})";
        }

        return display;
    }

    private async Task AddPersonAsync()
    {
        if(_editingCompanyId == null || SelectedAvailablePerson?.Id == null) return;

        try
        {
            var dto = new PersonByCompanyDto
            {
                PersonId  = SelectedAvailablePerson.Id,
                CompanyId = _editingCompanyId,
                Position  = string.IsNullOrWhiteSpace(NewPersonPosition) ? null : NewPersonPosition,
                Start     = NewPersonStart,
                End       = NewPersonEnd,
                Ongoing   = NewPersonOngoing
            };

            await _apiClient.PeopleByCompany.PostAsync(dto);

            ClearPersonForm();
            await LoadCompanyPeopleAsync(_editingCompanyId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding person to company");
        }
    }

    private void EditPerson(string? display)
    {
        if(display == null) return;

        int idx = CompanyPeopleDisplays.IndexOf(display);

        if(idx < 0 || idx >= CompanyPeople.Count) return;

        PersonByCompanyDto p = CompanyPeople[idx];

        _editingPersonCompanyId = p.Id;

        // Find and select the person in available people
        if(p.PersonId != null && _allPeopleList != null)
        {
            PersonDto? person = _allPeopleList.FirstOrDefault(pp => pp.Id == p.PersonId);

            if(person != null)
            {
                PersonSearchText = person.DisplayName ?? person.Alias ?? $"{person.Name} {person.Surname}".Trim();
                UpdatePeopleSuggestions(PersonSearchText);
                SelectedAvailablePerson = AvailablePeople.FirstOrDefault(pp => pp.Id == person.Id);
            }
        }

        NewPersonPosition = p.Position ?? string.Empty;
        NewPersonStart    = p.Start;
        NewPersonEnd      = p.End;
        NewPersonOngoing  = p.Ongoing ?? false;
    }

    private async Task SavePersonEditAsync()
    {
        if(_editingPersonCompanyId == null || _editingCompanyId == null) return;

        try
        {
            var dto = new PersonByCompanyDto
            {
                Position = string.IsNullOrWhiteSpace(NewPersonPosition) ? null : NewPersonPosition,
                Start    = NewPersonStart,
                End      = NewPersonEnd,
                Ongoing  = NewPersonOngoing
            };

            await _apiClient.PeopleByCompany[_editingPersonCompanyId.Value].PutAsync(dto);

            CancelPersonEdit();
            await LoadCompanyPeopleAsync(_editingCompanyId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating person-company association");
        }
    }

    private void CancelPersonEdit()
    {
        _editingPersonCompanyId = null;
        ClearPersonForm();
    }

    private async Task RemovePersonByDisplayAsync(string? display)
    {
        if(display == null || _editingCompanyId == null) return;

        int idx = CompanyPeopleDisplays.IndexOf(display);

        if(idx < 0 || idx >= CompanyPeople.Count || CompanyPeople[idx].Id == null) return;

        try
        {
            await _apiClient.PeopleByCompany[CompanyPeople[idx].Id!.Value].DeleteAsync();
            await LoadCompanyPeopleAsync(_editingCompanyId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing person from company");
        }
    }

    public void UpdatePeopleSuggestions(string query)
    {
        AvailablePeople.Clear();

        if(_allPeopleList == null) return;

        IEnumerable<PersonDto> source = _allPeopleList;

        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(p =>
                                      (p.Name != null &&
                                       p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                      (p.Surname != null &&
                                       p.Surname.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                      (p.DisplayName != null &&
                                       p.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                      (p.Alias != null &&
                                       p.Alias.Contains(query, StringComparison.OrdinalIgnoreCase)));

        foreach(PersonDto match in source)
            AvailablePeople.Add(match);
    }

    private void ClearPersonForm()
    {
        SelectedAvailablePerson = null;
        PersonSearchText        = string.Empty;
        NewPersonPosition       = string.Empty;
        NewPersonStart          = null;
        NewPersonEnd            = null;
        NewPersonOngoing        = false;
        _editingPersonCompanyId = null;
    }
}
