#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class PeopleListViewModel : ObservableObject, IRegionAware
{
    private readonly IPeopleListFilterContext         _filterContext;
    private readonly IStringLocalizer                _localizer;
    private readonly ILogger<PeopleListViewModel>    _logger;
    private readonly PeopleService                   _peopleService;
    private readonly IRegionManager                  _regionManager;

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

    [ObservableProperty]
    private ObservableCollection<PersonListItem> _peopleList = [];

    public PeopleListViewModel(PeopleService                   peopleService, IStringLocalizer localizer,
                               ILogger<PeopleListViewModel>    logger,        IRegionManager   regionManager,
                               IPeopleListFilterContext         filterContext)
    {
        _peopleService  = peopleService;
        _localizer      = localizer;
        _logger         = logger;
        _regionManager  = regionManager;
        _filterContext  = filterContext;
        LoadData                  = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand             = new AsyncRelayCommand(GoBackAsync);
        NavigateToPersonCommand   = new AsyncRelayCommand<PersonListItem>(NavigateToPersonAsync);
    }

    public IAsyncRelayCommand                  LoadData                { get; }
    public ICommand                            GoBackCommand           { get; }
    public IAsyncRelayCommand<PersonListItem>  NavigateToPersonCommand { get; }

    public PeopleListFilterType FilterType
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
        _ = LoadData.ExecuteAsync(null);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            PeopleList.Clear();

            UpdateFilterDescription();

            await LoadPeopleFromApiAsync();

            if(PeopleList.Count == 0)
            {
                ErrorMessage = _localizer["No people found for this filter"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading people: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load people. Please try again later."].Value;
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
            case PeopleListFilterType.All:
                PageTitle         = _localizer["All People"];
                FilterDescription = _localizer["Browsing all people in the database"];

                break;

            case PeopleListFilterType.Letter:
                if(!string.IsNullOrEmpty(FilterValue) && FilterValue.Length == 1)
                {
                    PageTitle         = $"{_localizer["People Starting with"]} {FilterValue}";
                    FilterDescription = $"{_localizer["Showing people that start with"]} {FilterValue}";
                }

                break;

            case PeopleListFilterType.Year:
                if(!string.IsNullOrEmpty(FilterValue) && int.TryParse(FilterValue, out int year))
                {
                    PageTitle         = $"{_localizer["People born in"]} {year}";
                    FilterDescription = $"{_localizer["Showing people born in"]} {year}";
                }

                break;
        }
    }

    private async Task LoadPeopleFromApiAsync()
    {
        try
        {
            List<PersonDto> people = FilterType switch
                                     {
                                         PeopleListFilterType.Letter when FilterValue.Length == 1 =>
                                             await _peopleService.GetPeopleByLetterAsync(FilterValue[0]),

                                         PeopleListFilterType.Year when int.TryParse(FilterValue, out int year) =>
                                             await _peopleService.GetPeopleByYearAsync(year),

                                         _ => await _peopleService.GetAllPeopleAsync()
                                     };

            foreach(PersonDto person in people)
            {
                int  id       = person.Id ?? 0;
                string fullName = person.DisplayName ?? person.Alias ??
                                  $"{person.Name} {person.Surname}".Trim();

                var item = new PersonListItem
                {
                    Id             = id,
                    FullName       = fullName,
                    BirthYear      = person.Birthdate?.DateTime > DateTime.MinValue
                                         ? person.Birthdate?.DateTime.Year
                                         : null,
                    DeathYear      = person.DeathDate?.DateTime.Year,
                    CountryOfBirth = person.Country,
                    Photo          = person.Photo
                };

                PeopleList.Add(item);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading people from API");
        }
    }

    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(PeoplePage));

        return Task.CompletedTask;
    }

    private Task NavigateToPersonAsync(PersonListItem? person)
    {
        if(person is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.PersonId, person.Id },
            { NavParamKeys.NavigationSource, nameof(PeopleListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(PersonViewPage), parameters);

        return Task.CompletedTask;
    }
}
