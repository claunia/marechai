#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class PersonViewViewModel : ObservableObject, IRegionAware
{
    private readonly IStringLocalizer              _localizer;
    private readonly ILogger<PersonViewViewModel>  _logger;
    private readonly PeopleService                 _peopleService;
    private readonly IRegionManager                _regionManager;

    private string? _navigationSource;
    private int     _currentPersonId;

    [ObservableProperty]
    private string _personName = string.Empty;

    [ObservableProperty]
    private string? _alias;

    [ObservableProperty]
    private string? _displayName;

    [ObservableProperty]
    private string? _birthDateDisplay;

    [ObservableProperty]
    private string? _deathDateDisplay;

    [ObservableProperty]
    private string? _countryOfBirth;

    [ObservableProperty]
    private string? _webpage;

    [ObservableProperty]
    private string? _twitter;

    [ObservableProperty]
    private string? _facebook;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    // Visibility flags
    [ObservableProperty]
    private Visibility _showAlias = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showDisplayName = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showBirthDate = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showDeathDate = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCountry = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showWebpage = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showTwitter = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showFacebook = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCompanies = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showBooks = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showDocuments = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showMagazines = Visibility.Collapsed;

    public PersonViewViewModel(ILogger<PersonViewViewModel> logger,         IRegionManager  regionManager,
                               PeopleService               peopleService, IStringLocalizer localizer)
    {
        _logger         = logger;
        _regionManager  = regionManager;
        _peopleService  = peopleService;
        _localizer      = localizer;
    }

    public ObservableCollection<string> Companies { get; } = [];
    public ObservableCollection<string> Books     { get; } = [];
    public ObservableCollection<string> Documents { get; } = [];
    public ObservableCollection<string> Magazines { get; } = [];

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.NavigationSource, out string? source))
            _navigationSource = source;

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.PersonId, out int personId))
        {
            _currentPersonId = personId;
            _ = LoadPersonAsync(personId);
        }
    }

    [RelayCommand]
    public Task GoBack()
    {
        switch(_navigationSource)
        {
            case nameof(PeopleListViewModel):
                _regionManager.RequestNavigate(RegionNames.Content, nameof(PeopleListPage));

                break;

            default:
                _regionManager.RequestNavigate(RegionNames.Content, nameof(PeoplePage));

                break;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task LoadData()
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        return Task.CompletedTask;
    }

    public async Task LoadPersonAsync(int personId)
    {
        try
        {
            IsLoading    = true;
            IsDataLoaded = false;
            HasError     = false;
            ErrorMessage = string.Empty;
            Companies.Clear();
            Books.Clear();
            Documents.Clear();
            Magazines.Clear();

            PersonDto? person = await _peopleService.GetPersonByIdAsync(personId);

            if(person is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["Person not found"];
                IsLoading    = false;

                return;
            }

            // Populate basic information
            PersonName  = person.DisplayName ?? person.Alias ?? $"{person.Name} {person.Surname}".Trim();
            Alias       = person.Alias;
            DisplayName = person.DisplayName;
            CountryOfBirth = person.Country;
            Webpage     = person.Webpage;
            Twitter     = person.Twitter;
            Facebook    = person.Facebook;

            if(person.Birthdate.HasValue && person.Birthdate.Value.Year > 1)
                BirthDateDisplay = person.Birthdate.Value.DateTime.ToString("MMMM d, yyyy");

            if(person.DeathDate.HasValue)
                DeathDateDisplay = person.DeathDate.Value.DateTime.ToString("MMMM d, yyyy");

            // Load related entities in parallel
            Task<List<PersonByCompanyDto>>  companiesTask  = _peopleService.GetCompaniesByPersonAsync(personId);
            Task<List<PersonByBookDto>>     booksTask      = _peopleService.GetBooksByPersonAsync(personId);
            Task<List<PersonByDocumentDto>> documentsTask  = _peopleService.GetDocumentsByPersonAsync(personId);
            Task<List<PersonByMagazineDto>> magazinesTask  = _peopleService.GetMagazinesByPersonAsync(personId);
            await Task.WhenAll(companiesTask, booksTask, documentsTask, magazinesTask);

            // Populate companies
            foreach(PersonByCompanyDto company in companiesTask.Result)
            {
                string name    = company.CompanyName ?? string.Empty;
                string display = name;

                if(!string.IsNullOrWhiteSpace(company.Position))
                    display += $" — {company.Position}";

                if(company.Ongoing == true)
                    display += $" ({_localizer["OngoingText"].Value})";
                else if(company.Start != null || company.End != null)
                {
                    string start = company.Start?.DateTime.ToString("yyyy") ?? "?";
                    string end   = company.End?.DateTime.ToString("yyyy")   ?? "?";
                    display += $" ({start}–{end})";
                }

                Companies.Add(display);
            }

            // Populate books
            foreach(PersonByBookDto book in booksTask.Result)
            {
                string title   = book.BookTitle ?? string.Empty;
                string? role   = book.Role;
                string display = !string.IsNullOrEmpty(role) ? $"{title} ({role})" : title;
                Books.Add(display);
            }

            // Populate documents
            foreach(PersonByDocumentDto document in documentsTask.Result)
            {
                string title   = document.DocumentTitle ?? string.Empty;
                string? role   = document.Role;
                string display = !string.IsNullOrEmpty(role) ? $"{title} ({role})" : title;
                Documents.Add(display);
            }

            // Populate magazines
            foreach(PersonByMagazineDto magazine in magazinesTask.Result)
            {
                string title   = magazine.MagazineTitle ?? string.Empty;
                string? role   = magazine.Role;
                string display = !string.IsNullOrEmpty(role) ? $"{title} ({role})" : title;
                Magazines.Add(display);
            }

            UpdateVisibilities();
            IsDataLoaded = true;
            IsLoading    = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading person {PersonId}", personId);
            HasError     = true;
            ErrorMessage = ex.Message;
            IsLoading    = false;
        }
    }

    private void UpdateVisibilities()
    {
        ShowAlias       = !string.IsNullOrEmpty(Alias) ? Visibility.Visible : Visibility.Collapsed;
        ShowDisplayName = !string.IsNullOrEmpty(DisplayName) ? Visibility.Visible : Visibility.Collapsed;
        ShowBirthDate   = !string.IsNullOrEmpty(BirthDateDisplay) ? Visibility.Visible : Visibility.Collapsed;
        ShowDeathDate   = !string.IsNullOrEmpty(DeathDateDisplay) ? Visibility.Visible : Visibility.Collapsed;
        ShowCountry     = !string.IsNullOrEmpty(CountryOfBirth) ? Visibility.Visible : Visibility.Collapsed;
        ShowWebpage     = !string.IsNullOrEmpty(Webpage) ? Visibility.Visible : Visibility.Collapsed;
        ShowTwitter     = !string.IsNullOrEmpty(Twitter) ? Visibility.Visible : Visibility.Collapsed;
        ShowFacebook    = !string.IsNullOrEmpty(Facebook) ? Visibility.Visible : Visibility.Collapsed;
        ShowCompanies   = Companies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowBooks       = Books.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowDocuments   = Documents.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowMagazines   = Magazines.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}
