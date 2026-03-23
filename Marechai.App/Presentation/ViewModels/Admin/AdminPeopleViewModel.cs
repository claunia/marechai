#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminPeopleViewModel : ObservableObject, IRegionAware
{
    private readonly ApiClient                        _apiClient;
    private readonly IJwtService                      _jwtService;
    private readonly IStringLocalizer                 _localizer;
    private readonly ILogger<AdminPeopleViewModel>    _logger;
    private readonly ITokenService                    _tokenService;

    [ObservableProperty]
    private ObservableCollection<PersonDto> _people = [];

    [ObservableProperty]
    private ObservableCollection<PersonDto> _filteredPeople = [];

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private PersonDto? _selectedPerson;

    private List<PersonDto>? _allPeople;

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

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editPanelTitle = string.Empty;

    private int? _editingId;

    // --- Form fields ---
    [ObservableProperty]
    private string _personName = string.Empty;

    [ObservableProperty]
    private string _surname = string.Empty;

    [ObservableProperty]
    private string _alias = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _birthDate;

    [ObservableProperty]
    private DateTimeOffset? _deathDate;

    [ObservableProperty]
    private string _webpage = string.Empty;

    [ObservableProperty]
    private string _twitter = string.Empty;

    [ObservableProperty]
    private string _facebook = string.Empty;

    [ObservableProperty]
    private Iso31661NumericDto? _selectedCountry;

    // --- Picker data ---
    [ObservableProperty]
    private ObservableCollection<Iso31661NumericDto> _countries = [];

    public AdminPeopleViewModel(ApiClient                        apiClient,
                                IJwtService                      jwtService,
                                ITokenService                    tokenService,
                                ILogger<AdminPeopleViewModel>    logger,
                                IStringLocalizer                 localizer)
    {
        _apiClient    = apiClient;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadPeopleCommand = new AsyncRelayCommand(LoadPeopleAsync);
        OpenAddCommand    = new RelayCommand(OpenAdd);
        OpenEditCommand   = new RelayCommand<PersonDto>(OpenEdit);
        DeleteCommand     = new AsyncRelayCommand<PersonDto>(DeleteAsync);
        SaveCommand       = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);

        CheckAdminRole();
    }

    public IAsyncRelayCommand            LoadPeopleCommand { get; }
    public IRelayCommand                 OpenAddCommand    { get; }
    public IRelayCommand<PersonDto>      OpenEditCommand   { get; }
    public IAsyncRelayCommand<PersonDto> DeleteCommand     { get; }
    public IAsyncRelayCommand            SaveCommand       { get; }
    public IRelayCommand                 CancelEditCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
        {
            _ = LoadPeopleCommand.ExecuteAsync(null);
            _ = LoadPickerDataAsync();
        }
    }

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

    private async Task LoadPeopleAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            People.Clear();

            List<PersonDto>? response = await _apiClient.People.GetAsync();
            _allPeople = response;

            if(response != null)
                foreach(PersonDto item in response)
                    People.Add(item);

            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading people");
            ErrorMessage = _localizer["FailedToLoadPeople"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenAdd()
    {
        _editingId     = null;
        EditPanelTitle = _localizer["AddPersonDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    private void OpenEdit(PersonDto? item)
    {
        if(item == null) return;

        _editingId     = item.Id;
        EditPanelTitle = _localizer["EditPersonDialog_Title"];
        PersonName     = item.Name      ?? string.Empty;
        Surname        = item.Surname   ?? string.Empty;
        Alias          = item.Alias     ?? string.Empty;
        DisplayName    = item.DisplayName ?? string.Empty;
        BirthDate      = item.Birthdate;
        DeathDate      = item.DeathDate;
        Webpage        = item.Webpage   ?? string.Empty;
        Twitter        = item.Twitter   ?? string.Empty;
        Facebook       = item.Facebook  ?? string.Empty;

        SelectedCountry = item.CountryId.HasValue
                              ? Countries.FirstOrDefault(c => c.Id == item.CountryId.Value)
                              : null;

        HasError     = false;
        ErrorMessage = string.Empty;
        IsEditing    = true;
    }

    private async Task DeleteAsync(PersonDto? item)
    {
        if(item?.Id == null) return;

        try
        {
            await _apiClient.People[item.Id.Value].DeleteAsync();
            await LoadPeopleAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting person {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeletePerson"];
            HasError     = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(PersonName))
            {
                ErrorMessage = _localizer["PersonNameRequired"];
                HasError     = true;

                return;
            }

            var dto = new PersonDto
            {
                Name        = PersonName,
                Surname     = string.IsNullOrWhiteSpace(Surname)     ? null : Surname,
                Alias       = string.IsNullOrWhiteSpace(Alias)       ? null : Alias,
                DisplayName = string.IsNullOrWhiteSpace(DisplayName) ? null : DisplayName,
                Birthdate   = BirthDate,
                DeathDate   = DeathDate,
                Webpage     = string.IsNullOrWhiteSpace(Webpage)     ? null : Webpage,
                Twitter     = string.IsNullOrWhiteSpace(Twitter)     ? null : Twitter,
                Facebook    = string.IsNullOrWhiteSpace(Facebook)    ? null : Facebook,
                CountryId   = SelectedCountry?.Id
            };

            if(_editingId == null)
                await _apiClient.People.PostAsync(dto);
            else
            {
                dto.Id = _editingId;
                await _apiClient.People[_editingId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadPeopleAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving person");
            ErrorMessage = _localizer["FailedToSavePerson"];
            HasError     = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing  = false;
        _editingId = null;
        ClearForm();
        HasError     = false;
        ErrorMessage = string.Empty;
    }

    public void ApplyFilter()
    {
        FilteredPeople.Clear();

        IEnumerable<PersonDto> source = (IEnumerable<PersonDto>?)_allPeople ?? People;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(p =>
                (p.Name != null &&
                 p.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (p.Surname != null &&
                 p.Surname.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (p.Alias != null &&
                 p.Alias.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (p.DisplayName != null &&
                 p.DisplayName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (p.Country != null &&
                 p.Country.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));

        foreach(PersonDto item in source)
            FilteredPeople.Add(item);
    }

    public async Task LoadPickerDataAsync()
    {
        try
        {
            List<Iso31661NumericDto>? countriesResponse = await _apiClient.Iso31661Numeric.GetAsync();
            Countries.Clear();

            if(countriesResponse != null)
                foreach(Iso31661NumericDto c in countriesResponse)
                    Countries.Add(c);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading countries");
        }
    }

    private void ClearForm()
    {
        PersonName      = string.Empty;
        Surname         = string.Empty;
        Alias           = string.Empty;
        DisplayName     = string.Empty;
        BirthDate       = null;
        DeathDate       = null;
        Webpage         = string.Empty;
        Twitter         = string.Empty;
        Facebook        = string.Empty;
        SelectedCountry = null;
        HasError        = false;
        ErrorMessage    = string.Empty;
    }

    public static string GetFullName(PersonDto? person) =>
        person?.DisplayName ?? person?.Alias ?? $"{person?.Name} {person?.Surname}";
}
