#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Uno.Extensions.Authentication;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class DocumentViewViewModel : ObservableObject, IRegionAware
{
    private readonly DocumentsService                _documentsService;
    private readonly IAuthenticationService          _authService;
    private readonly IStringLocalizer                _localizer;
    private readonly ILogger<DocumentViewViewModel>  _logger;
    private readonly IRegionManager                  _regionManager;

    private string? _navigationSource;
    private long    _currentDocumentId;

    [ObservableProperty]
    private string _documentTitle = string.Empty;

    [ObservableProperty]
    private string? _nativeTitle;

    [ObservableProperty]
    private string? _publishedDisplay;

    [ObservableProperty]
    private string? _country;

    [ObservableProperty]
    private string? _synopsisText;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private Visibility _showNativeTitle = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPublished = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCountry = Visibility.Collapsed;

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
    private bool _isCollected;

    [ObservableProperty]
    private bool _isTogglingCollection;

    [ObservableProperty]
    private string _collectionButtonText = string.Empty;

    [ObservableProperty]
    private Visibility _showCollectionButton = Visibility.Collapsed;

    public DocumentViewViewModel(ILogger<DocumentViewViewModel> logger,           IRegionManager    regionManager,
                                 DocumentsService              documentsService, IAuthenticationService authService,
                                 IStringLocalizer              localizer)
    {
        _logger           = logger;
        _regionManager    = regionManager;
        _documentsService = documentsService;
        _authService      = authService;
        _localizer        = localizer;
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

        if(navigationContext.Parameters.TryGetValue<long>(NavParamKeys.DocumentId, out long documentId))
        {
            _currentDocumentId = documentId;
            _ = LoadDocumentAsync(documentId);
        }
    }

    [RelayCommand]
    public Task GoBack()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        switch(_navigationSource)
        {
            case nameof(DocumentsListViewModel):
                _regionManager.RequestNavigate(RegionNames.Content, nameof(DocumentsListPage));

                break;

            default:
                _regionManager.RequestNavigate(RegionNames.Content, nameof(DocumentsPage));

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

    [RelayCommand]
    public async Task ToggleCollection()
    {
        if(IsTogglingCollection || _currentDocumentId == 0) return;

        try
        {
            IsTogglingCollection = true;

            bool success = IsCollected
                               ? await _documentsService.RemoveDocumentFromCollectionAsync(_currentDocumentId)
                               : await _documentsService.AddDocumentToCollectionAsync(_currentDocumentId);

            if(success)
                IsCollected = !IsCollected;
        }
        finally
        {
            IsTogglingCollection = false;
        }
    }

    public async Task LoadDocumentAsync(long documentId)
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

            DocumentDto? document = await _documentsService.GetDocumentAsync(documentId);

            if(document is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["Document not found"];
                IsLoading    = false;

                return;
            }

            DocumentTitle = document.Title ?? string.Empty;
            NativeTitle   = document.NativeTitle;
            Country       = document.Country;

            if(document.Published.HasValue)
                PublishedDisplay = DatePrecisionFormatter.Format(document.Published, document.PublishedPrecision);

            // Load synopsis
            DocumentSynopsisDto? synopsis = await _documentsService.GetDocumentSynopsisAsync(documentId);

            if(synopsis is not null)
                SynopsisText = synopsis.Text;

            // Load people (by role)
            List<PersonByDocumentDto> people = await _documentsService.GetPeopleByDocumentAsync(documentId);

            foreach(PersonByDocumentDto person in people)
            {
                string  name    = person.DisplayName ?? person.Alias ?? $"{person.Name} {person.Surname}".Trim();
                string? roleStr = person.Role;
                string  display = !string.IsNullOrEmpty(roleStr) ? $"{name} ({roleStr})" : name;
                People.Add(display);
            }

            // Load companies (by role)
            List<CompanyByDocumentDto> companies = await _documentsService.GetCompaniesByDocumentAsync(documentId);

            foreach(CompanyByDocumentDto company in companies)
            {
                string  name    = company.Company ?? string.Empty;
                string? roleStr = company.Role;
                string  display = !string.IsNullOrEmpty(roleStr) ? $"{name} ({roleStr})" : name;
                Companies.Add(display);
            }

            // Load machines
            List<DocumentByMachineDto> machines = await _documentsService.GetMachinesByDocumentAsync(documentId);

            foreach(DocumentByMachineDto machine in machines)
                Machines.Add(machine.Machine ?? string.Empty);

            // Load machine families
            List<DocumentByMachineFamilyDto> families =
                await _documentsService.GetMachineFamiliesByDocumentAsync(documentId);

            foreach(DocumentByMachineFamilyDto family in families)
                MachineFamilies.Add(family.MachineFamily ?? string.Empty);

            // Load collection state (authenticated users only)
            bool isAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);
            ShowCollectionButton = isAuthenticated ? Visibility.Visible : Visibility.Collapsed;

            if(isAuthenticated)
                IsCollected = await _documentsService.IsDocumentCollectedAsync(documentId);

            CollectionButtonText = IsCollected ? _localizer["In Collection"] : _localizer["Add to Collection"];

            UpdateVisibilities();
            IsDataLoaded = true;
            IsLoading    = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading document {DocumentId}", documentId);
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
        ShowSynopsis        = !string.IsNullOrEmpty(SynopsisText) ? Visibility.Visible : Visibility.Collapsed;
        ShowPeople          = People.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowCompanies       = Companies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowMachines        = Machines.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowMachineFamilies = MachineFamilies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}
