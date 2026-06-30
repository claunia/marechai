#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Models;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Microsoft.UI.Dispatching;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminPeopleViewModel : ObservableObject, IRegionAware
{
    private readonly Client                        _apiClient;
    private readonly IJwtService                      _jwtService;
    private readonly IStringLocalizer                 _localizer;
    private readonly ILogger<AdminPeopleViewModel>    _logger;
    private readonly PeopleService                    _peopleService;
    private readonly ITokenService                    _tokenService;
    private readonly DispatcherQueue?                 _dispatcherQueue;

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
    private int _birthDatePrecision;

    [ObservableProperty]
    private DateTimeOffset? _deathDate;

    [ObservableProperty]
    private int _deathDatePrecision;

    [ObservableProperty]
    private string _webpage = string.Empty;

    [ObservableProperty]
    private string _twitter = string.Empty;

    [ObservableProperty]
    private string _facebook = string.Empty;

    [ObservableProperty]
    private Iso31661NumericDto? _selectedCountry;

    // --- Description panel state ---
    [ObservableProperty]
    private bool _isEditingDescription;

    [ObservableProperty]
    private string _descriptionMarkdown = string.Empty;

    [ObservableProperty]
    private int? _descriptionPersonId;

    [ObservableProperty]
    private ObservableCollection<LanguageItem> _availableLanguages = [];

    [ObservableProperty]
    private LanguageItem? _selectedLanguage;

    [ObservableProperty]
    private ObservableCollection<PersonDescriptionDto> _existingTranslations = [];

    public bool CanSaveDescription =>
        DescriptionPersonId.HasValue &&
        SelectedLanguage is not null &&
        !string.IsNullOrWhiteSpace(DescriptionMarkdown);

    // --- Picker data ---
    [ObservableProperty]
    private ObservableCollection<Iso31661NumericDto> _countries = [];

    public AdminPeopleViewModel(Client                        apiClient,
                                IJwtService                      jwtService,
                                PeopleService                    peopleService,
                                ITokenService                    tokenService,
                                ILogger<AdminPeopleViewModel>    logger,
                                IStringLocalizer                 localizer)
    {
        _apiClient    = apiClient;
        _jwtService   = jwtService;
        _peopleService = peopleService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        LoadPeopleCommand         = new AsyncRelayCommand(LoadPeopleAsync);
        OpenAddCommand            = new RelayCommand(OpenAdd);
        OpenEditCommand           = new RelayCommand<PersonDto>(OpenEdit);
        DeleteCommand             = new AsyncRelayCommand<PersonDto>(DeleteAsync);
        SaveCommand               = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand         = new RelayCommand(CancelEdit);
        OpenDescriptionCommand    = new RelayCommand<PersonDto>(OpenDescription);
        SaveDescriptionCommand    = new AsyncRelayCommand(SaveDescriptionAsync);
        CancelDescriptionCommand  = new RelayCommand(CancelDescription);
        DeleteTranslationCommand  = new AsyncRelayCommand<PersonDescriptionDto>(DeleteTranslationAsync);
        EditTranslationCommand    = new RelayCommand<PersonDescriptionDto>(EditTranslation);

        InitializeLanguages();

        CheckAdminRole();
    }

    public IAsyncRelayCommand                       LoadPeopleCommand        { get; }
    public IRelayCommand                            OpenAddCommand           { get; }
    public IRelayCommand<PersonDto>                 OpenEditCommand          { get; }
    public IAsyncRelayCommand<PersonDto>            DeleteCommand            { get; }
    public IAsyncRelayCommand                       SaveCommand              { get; }
    public IRelayCommand                            CancelEditCommand        { get; }
    public IRelayCommand<PersonDto>                 OpenDescriptionCommand   { get; }
    public IAsyncRelayCommand                       SaveDescriptionCommand   { get; }
    public IRelayCommand                            CancelDescriptionCommand { get; }
    public IAsyncRelayCommand<PersonDescriptionDto> DeleteTranslationCommand { get; }
    public IRelayCommand<PersonDescriptionDto>      EditTranslationCommand   { get; }

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

            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
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
        IsEditingDescription = false;
        IsEditing            = true;
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
        BirthDatePrecision = item.BirthdatePrecision ?? 0;
        DeathDate      = item.DeathDate;
        DeathDatePrecision = item.DeathDatePrecision ?? 0;
        Webpage        = item.Webpage   ?? string.Empty;
        Twitter        = item.Twitter   ?? string.Empty;
        Facebook       = item.Facebook  ?? string.Empty;

        SelectedCountry = item.CountryId.HasValue
                              ? Countries.FirstOrDefault(c => c.Id == item.CountryId.Value)
                              : null;

        HasError             = false;
        ErrorMessage         = string.Empty;
        IsEditingDescription = false;
        IsEditing            = true;
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
                BirthdatePrecision = BirthDatePrecision,
                DeathDate   = DeathDate,
                DeathDatePrecision = DeathDatePrecision,
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
        IsEditing            = false;
        IsEditingDescription = false;
        _editingId           = null;
        ClearForm();
        HasError     = false;
        ErrorMessage = string.Empty;
    }

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

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        if(value == null || ExistingTranslations.Count == 0)
        {
            DescriptionMarkdown = string.Empty;
            OnPropertyChanged(nameof(CanSaveDescription));

            return;
        }

        PersonDescriptionDto? existing = ExistingTranslations.FirstOrDefault(t => t.LanguageCode == value.Code);
        DescriptionMarkdown = existing?.Markdown ?? string.Empty;
        OnPropertyChanged(nameof(CanSaveDescription));
    }

    partial void OnDescriptionMarkdownChanged(string value) => OnPropertyChanged(nameof(CanSaveDescription));

    partial void OnDescriptionPersonIdChanged(int? value) => OnPropertyChanged(nameof(CanSaveDescription));

    private void OpenDescription(PersonDto? person) => _ = OpenDescriptionAsync(person);

    private async Task OpenDescriptionAsync(PersonDto? person)
    {
        if(person?.Id == null) return;

        try
        {
            await RunOnUiThreadAsync(() =>
            {
                HasError             = false;
                ErrorMessage         = string.Empty;
                DescriptionPersonId  = person.Id;
                DescriptionMarkdown  = string.Empty;
                IsEditing            = false;
                ExistingTranslations.Clear();
            });
            await ReloadDescriptionTranslationsAsync(person.Id.Value);
            await RunOnUiThreadAsync(() =>
            {
                SelectedLanguage     = GetDefaultDescriptionLanguage();
                IsEditingDescription = true;
            });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading descriptions for person {Id}", person.Id);
            await RunOnUiThreadAsync(() =>
            {
                DescriptionMarkdown  = string.Empty;
                IsEditingDescription = true;
            });
        }
    }

    private async Task ReloadDescriptionTranslationsAsync(int personId)
    {
        List<PersonDescriptionDto> translations = await _peopleService.GetDescriptionsAsync(personId);

        await RunOnUiThreadAsync(() =>
        {
            ExistingTranslations.Clear();

            foreach(PersonDescriptionDto translation in translations.OrderBy(t => GetLanguageDisplayName(t.LanguageCode)))
            {
                translation.Language ??= GetLanguageDisplayName(translation.LanguageCode);
                ExistingTranslations.Add(translation);
            }
        });
    }

    private Task RunOnUiThreadAsync(Action action)
    {
        if(_dispatcherQueue is null || _dispatcherQueue.HasThreadAccess)
        {
            action();

            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();

        if(!_dispatcherQueue.TryEnqueue(() =>
           {
               try
               {
                   action();
                   tcs.SetResult();
               }
               catch(Exception ex)
               {
                   tcs.SetException(ex);
               }
           }))
            tcs.SetException(new InvalidOperationException("Unable to enqueue work on the UI thread."));

        return tcs.Task;
    }

    LanguageItem GetDefaultDescriptionLanguage()
    {
        return AvailableLanguages.FirstOrDefault(language =>
                   ExistingTranslations.All(translation => translation.LanguageCode != language.Code)) ??
               AvailableLanguages.FirstOrDefault(language => language.Code == "eng") ??
               AvailableLanguages.First();
    }

    string GetLanguageDisplayName(string? languageCode)
    {
        if(string.IsNullOrWhiteSpace(languageCode)) return string.Empty;

        return AvailableLanguages.FirstOrDefault(language => language.Code == languageCode)?.DisplayName ??
               ExistingTranslations.FirstOrDefault(translation => translation.LanguageCode == languageCode)?.Language ??
               languageCode;
    }

    private async Task SaveDescriptionAsync()
    {
        if(!CanSaveDescription || DescriptionPersonId == null || SelectedLanguage == null) return;

        try
        {
            var dto = new PersonDescriptionDto
            {
                PersonId     = DescriptionPersonId.Value,
                Markdown     = DescriptionMarkdown,
                LanguageCode = SelectedLanguage.Code
            };

            (bool succeeded, string? error) =
                await _peopleService.CreateOrUpdateDescriptionAsync(DescriptionPersonId.Value, dto);

            if(!succeeded)
            {
                ErrorMessage = error ?? _localizer["FailedToSaveDescription"];
                HasError     = true;

                return;
            }

            await ReloadDescriptionTranslationsAsync(DescriptionPersonId.Value);
            HasError     = false;
            ErrorMessage = string.Empty;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving description for person {Id}", DescriptionPersonId);
            ErrorMessage = _localizer["FailedToSaveDescription"];
            HasError     = true;
        }
    }

    private void EditTranslation(PersonDescriptionDto? translation)
    {
        if(translation?.LanguageCode == null) return;

        SelectedLanguage    = AvailableLanguages.FirstOrDefault(l => l.Code == translation.LanguageCode);
        DescriptionMarkdown = translation.Markdown ?? string.Empty;
    }

    private async Task DeleteTranslationAsync(PersonDescriptionDto? translation)
    {
        if(DescriptionPersonId == null || translation?.LanguageCode == null) return;

        try
        {
            (bool succeeded, string? error) =
                await _peopleService.DeleteDescriptionAsync(DescriptionPersonId.Value, translation.LanguageCode);

            if(!succeeded)
            {
                ErrorMessage = error ?? _localizer["FailedToDeleteTranslation"];
                HasError     = true;

                return;
            }

            await ReloadDescriptionTranslationsAsync(DescriptionPersonId.Value);
            HasError     = false;
            ErrorMessage = string.Empty;

            if(SelectedLanguage?.Code == translation.LanguageCode)
            {
                DescriptionMarkdown = string.Empty;

                LanguageItem nextLanguage = GetDefaultDescriptionLanguage();

                if(SelectedLanguage?.Code == nextLanguage.Code)
                    OnSelectedLanguageChanged(nextLanguage);
                else
                    SelectedLanguage = nextLanguage;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting description for person {Id}", DescriptionPersonId);
            ErrorMessage = _localizer["FailedToDeleteTranslation"];
            HasError     = true;
        }
    }

    private void CancelDescription()
    {
        IsEditingDescription = false;
        DescriptionPersonId  = null;
        DescriptionMarkdown  = string.Empty;
        ExistingTranslations.Clear();
        SelectedLanguage     = AvailableLanguages.FirstOrDefault(language => language.Code == "eng") ??
                               AvailableLanguages.FirstOrDefault();
        HasError             = false;
        ErrorMessage         = string.Empty;
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
        BirthDatePrecision = 0;
        DeathDate       = null;
        DeathDatePrecision = 0;
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
