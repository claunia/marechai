#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Uno.Extensions.Authentication;

namespace Marechai.App.Presentation.ViewModels;

/// <summary>
///     One entity-type checkbox in the Advanced Search filter panel.
/// </summary>
public partial class EntityTypeOption : ObservableObject
{
    public EntityTypeOption(int value, string label)
    {
        Value = value;
        Label = label;
    }

    public int    Value { get; }
    public string Label { get; }

    [ObservableProperty]
    private bool _isSelected;
}

/// <summary>
///     A group of search results of the same entity type, for display under a category header.
/// </summary>
public partial class SearchResultGroup : ObservableObject
{
    public SearchResultGroup(int entityType, string displayName)
    {
        EntityType  = entityType;
        DisplayName = displayName;
    }

    public int                                   EntityType  { get; }
    public string                                DisplayName { get; }
    public ObservableCollection<SearchResultDto> Items       { get; } = [];

    [ObservableProperty]
    private int _count;
}

public partial class AdvancedSearchViewModel : ObservableObject, IRegionAware
{
    private const int PageSize = 50;

    private readonly IAuthenticationService           _authService;
    private readonly IStringLocalizer                 _localizer;
    private readonly ILogger<AdvancedSearchViewModel> _logger;
    private readonly IRegionManager                   _regionManager;
    private readonly SearchService                    _searchService;

    private bool _hasMoreItems = true;
    private bool _isLoadingMore;
    private int  _skip;

    [ObservableProperty] private string  _query        = string.Empty;
    [ObservableProperty] private int?    _yearFrom;
    [ObservableProperty] private int?    _yearTo;
    [ObservableProperty] private int     _yearKnownIndex;
    [ObservableProperty] private int     _countryKnownIndex;
    [ObservableProperty] private int     _hasImageIndex;
    [ObservableProperty] private int     _inCollectionIndex;
    [ObservableProperty] private int     _sortIndex;
    [ObservableProperty] private string  _letter       = string.Empty;
    [ObservableProperty] private int     _softwareKindIndex;
    [ObservableProperty] private string  _contains     = string.Empty;
    [ObservableProperty] private string  _notContains  = string.Empty;
    [ObservableProperty] private bool    _exactMatch;
    [ObservableProperty] private bool    _isAuthenticated;

    [ObservableProperty] private Iso31661NumericDto? _selectedCountry;
    [ObservableProperty] private ObservableCollection<Iso31661NumericDto> _countries = [];

    [ObservableProperty] private string                              _companySearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SearchResultDto> _companySuggestions = [];
    [ObservableProperty] private SearchResultDto?                      _selectedCompany;

    [ObservableProperty] private string                              _includesSoftwareSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SearchResultDto> _includesSoftwareSuggestions = [];
    [ObservableProperty] private SearchResultDto?                      _selectedIncludesSoftware;

    [ObservableProperty] private ObservableCollection<SearchResultGroup> _resultGroups = [];
    [ObservableProperty] private bool                                    _hasSearched;
    [ObservableProperty] private bool                                    _isLoading;
    [ObservableProperty] private bool                                    _isLoadingNextPage;
    [ObservableProperty] private bool                                    _hasError;
    [ObservableProperty] private string                                  _errorMessage = string.Empty;

    public AdvancedSearchViewModel(SearchService searchService, Client apiClient, IStringLocalizer localizer,
                                   ILogger<AdvancedSearchViewModel> logger, IRegionManager regionManager,
                                   IAuthenticationService authService)
    {
        _searchService = searchService;
        _localizer     = localizer;
        _logger        = logger;
        _regionManager = regionManager;
        _authService   = authService;

        EntityTypes =
        [
            new EntityTypeOption(1, _localizer["CompaniesButton"]),
            new EntityTypeOption(2, _localizer["ComputersButton"]),
            new EntityTypeOption(3, _localizer["ConsolesButton"]),
            new EntityTypeOption(4, _localizer["SmartphonesButton"]),
            new EntityTypeOption(14, "PDA"),
            new EntityTypeOption(15, "Tablet"),
            new EntityTypeOption(5, _localizer["BooksButton"]),
            new EntityTypeOption(6, _localizer["DocumentsButton"]),
            new EntityTypeOption(7, _localizer["MagazinesButton"]),
            new EntityTypeOption(8, _localizer["GraphicalProcessingUnitsButton"]),
            new EntityTypeOption(9, _localizer["ProcessorsButton"]),
            new EntityTypeOption(10, _localizer["SoundSynthesizersButton"]),
            new EntityTypeOption(11, _localizer["PeopleButton"]),
            new EntityTypeOption(12, _localizer["SoftwareButton"]),
            new EntityTypeOption(13, _localizer["SoftwareButton"] + " (compilation)")
        ];

        SearchCommand               = new AsyncRelayCommand(SearchAsync);
        LoadMoreCommand             = new AsyncRelayCommand(LoadMoreAsync);
        ClearFiltersCommand         = new RelayCommand(ClearFilters);
        NavigateToResultCommand     = new RelayCommand<SearchResultDto>(NavigateToResult);
        GoBackCommand               = new RelayCommand(() =>
                                          _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage)));

        _ = LoadCountriesAsync(apiClient);
        _ = UpdateAuthenticationStateAsync();
    }

    public ObservableCollection<EntityTypeOption> EntityTypes { get; }

    public IAsyncRelayCommand                  SearchCommand           { get; }
    public IAsyncRelayCommand                  LoadMoreCommand         { get; }
    public ICommand                            ClearFiltersCommand     { get; }
    public ICommand                            NavigateToResultCommand { get; }
    public ICommand                            GoBackCommand           { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.SearchQuery, out string searchQuery) &&
           !string.IsNullOrWhiteSpace(searchQuery))
        {
            Query = searchQuery;
            _ = SearchAsync();
        }
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    partial void OnCompanySearchTextChanged(string value) => _ = UpdateCompanySuggestionsAsync(value);

    partial void OnIncludesSoftwareSearchTextChanged(string value) => _ = UpdateIncludesSoftwareSuggestionsAsync(value);

    private async Task UpdateCompanySuggestionsAsync(string text)
    {
        string trimmed = text?.Trim() ?? string.Empty;

        if(trimmed.Length < 3)
        {
            CompanySuggestions = [];

            return;
        }

        List<SearchResultDto> results = await _searchService.AutocompleteAsync(trimmed, entityType: 1);
        CompanySuggestions = new ObservableCollection<SearchResultDto>(results);
    }

    private async Task UpdateIncludesSoftwareSuggestionsAsync(string text)
    {
        string trimmed = text?.Trim() ?? string.Empty;

        if(trimmed.Length < 3)
        {
            IncludesSoftwareSuggestions = [];

            return;
        }

        List<SearchResultDto> results = await _searchService.AutocompleteAsync(trimmed, entityType: 12);
        IncludesSoftwareSuggestions = new ObservableCollection<SearchResultDto>(results);
    }

    private async Task LoadCountriesAsync(Client apiClient)
    {
        try
        {
            List<Iso31661NumericDto>? countriesResponse = await apiClient.Iso31661Numeric.GetAsync();

            if(countriesResponse == null) return;

            Countries = new ObservableCollection<Iso31661NumericDto>(
                countriesResponse.OrderBy(c => c.Name));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching countries for advanced search");
        }
    }

    private async Task UpdateAuthenticationStateAsync() =>
        IsAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);

    private void ClearFilters()
    {
        Query                       = string.Empty;
        YearFrom                    = null;
        YearTo                      = null;
        YearKnownIndex              = 0;
        CountryKnownIndex           = 0;
        HasImageIndex               = 0;
        InCollectionIndex           = 0;
        SortIndex                   = 0;
        Letter                      = string.Empty;
        SoftwareKindIndex           = 0;
        Contains                    = string.Empty;
        NotContains                 = string.Empty;
        ExactMatch                  = false;
        SelectedCountry             = null;
        SelectedCompany             = null;
        SelectedIncludesSoftware    = null;
        CompanySearchText           = string.Empty;
        IncludesSoftwareSearchText  = string.Empty;

        foreach(EntityTypeOption option in EntityTypes) option.IsSelected = false;

        ResultGroups = [];
        HasSearched  = false;
    }

    private static bool? TriStateValue(int index) => index switch
    {
        1 => true,
        2 => false,
        _ => null
    };

    private SearchRequestDto BuildRequest()
    {
        var request = new SearchRequestDto
        {
            Query              = string.IsNullOrWhiteSpace(Query) ? null : Query.Trim(),
            YearFrom           = YearFrom,
            YearTo             = YearTo,
            YearKnown          = TriStateValue(YearKnownIndex),
            CountryKnown       = TriStateValue(CountryKnownIndex),
            HasImage           = TriStateValue(HasImageIndex),
            InCollection       = TriStateValue(InCollectionIndex),
            CountryId          = SelectedCountry?.Id,
            CompanyId          = SelectedCompany is null ? null : (int)SelectedCompany.EntityId!.Value,
            Letter             = string.IsNullOrWhiteSpace(Letter) ? null : Letter,
            SoftwareKind       = SoftwareKindIndex == 0 ? null : SoftwareKindIndex - 1,
            IncludesSoftwareId = SelectedIncludesSoftware?.EntityId,
            Contains           = string.IsNullOrWhiteSpace(Contains) ? null : Contains,
            NotContains        = string.IsNullOrWhiteSpace(NotContains) ? null : NotContains,
            ExactMatch         = ExactMatch,
            Sort = SortIndex switch
            {
                1 => "name_asc",
                2 => "name_desc",
                3 => "year_asc",
                4 => "year_desc",
                _ => "relevance"
            },
            Skip = _skip,
            Take = PageSize
        };

        List<int> selectedTypes = EntityTypes.Where(t => t.IsSelected).Select(t => t.Value).ToList();
        if(selectedTypes.Count > 0) request.EntityTypes = selectedTypes.Select(t => (int?)t).ToList();

        return request;
    }

    private bool HasAnyCriteria() =>
        !string.IsNullOrWhiteSpace(Query) || EntityTypes.Any(t => t.IsSelected) || YearFrom.HasValue ||
        YearTo.HasValue || YearKnownIndex != 0 || CountryKnownIndex != 0 || SelectedCountry is not null ||
        SelectedCompany is not null || !string.IsNullOrWhiteSpace(Letter) || SoftwareKindIndex != 0 ||
        SelectedIncludesSoftware is not null || !string.IsNullOrWhiteSpace(Contains) ||
        !string.IsNullOrWhiteSpace(NotContains) || HasImageIndex != 0 || InCollectionIndex != 0;

    private async Task SearchAsync()
    {
        if(!HasAnyCriteria())
        {
            ResultGroups = [];
            HasSearched  = false;

            return;
        }

        _skip         = 0;
        _hasMoreItems = true;
        ResultGroups  = [];
        IsLoading     = true;
        HasError      = false;
        ErrorMessage  = string.Empty;
        HasSearched   = true;

        await LoadPageAsync();

        IsLoading = false;
    }

    private async Task LoadMoreAsync()
    {
        if(_isLoadingMore || !_hasMoreItems || !HasSearched) return;

        IsLoadingNextPage = true;
        await LoadPageAsync();
        IsLoadingNextPage = false;
    }

    private async Task LoadPageAsync()
    {
        _isLoadingMore = true;

        try
        {
            SearchRequestDto request = BuildRequest();
            SearchResultsPageDto? page = await _searchService.SearchAsync(request);

            if(page?.Results is null || page.Results.Count == 0)
            {
                _hasMoreItems = false;

                if(_skip == 0 && (page?.Results is null || page.Results.Count == 0)) HasError = false;

                return;
            }

            foreach(SearchResultDto result in page.Results)
            {
                int entityType = result.EntityType ?? 0;

                SearchResultGroup? group = ResultGroups.FirstOrDefault(g => g.EntityType == entityType);

                if(group is null)
                {
                    group = new SearchResultGroup(entityType, EntityTypeLabel(entityType));
                    ResultGroups.Add(group);
                }

                group.Items.Add(result);
                group.Count = group.Items.Count;
            }

            _skip        += page.Results.Count;
            _hasMoreItems =  page.Results.Count == PageSize;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error running advanced search");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            _isLoadingMore = false;
        }
    }

    private string EntityTypeLabel(int entityType) => entityType switch
    {
        1  => _localizer["CompaniesButton"],
        2  => _localizer["ComputersButton"],
        3  => _localizer["ConsolesButton"],
        4  => _localizer["SmartphonesButton"],
        5  => _localizer["BooksButton"],
        6  => _localizer["DocumentsButton"],
        7  => _localizer["MagazinesButton"],
        8  => _localizer["GraphicalProcessingUnitsButton"],
        9  => _localizer["ProcessorsButton"],
        10 => _localizer["SoundSynthesizersButton"],
        11 => _localizer["PeopleButton"],
        12 => _localizer["SoftwareButton"],
        13 => _localizer["SoftwareButton"] + " (compilation)",
        14 => "PDA",
        15 => "Tablet",
        _  => "?"
    };

    private void NavigateToResult(SearchResultDto? result)
    {
        if(result is null) return;

        SearchResultNavigator.NavigateTo(_regionManager, result);
    }
}
