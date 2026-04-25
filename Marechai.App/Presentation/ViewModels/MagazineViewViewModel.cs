#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class MagazineViewViewModel : ObservableObject, IRegionAware
{
    private readonly MagazinesService                _magazinesService;
    private readonly IStringLocalizer                _localizer;
    private readonly ILogger<MagazineViewViewModel>  _logger;
    private readonly IRegionManager                  _regionManager;

    private string? _navigationSource;

    [ObservableProperty]
    private string _magazineTitle = string.Empty;

    [ObservableProperty]
    private string? _nativeTitle;

    [ObservableProperty]
    private string? _firstPublicationDisplay;

    [ObservableProperty]
    private string? _country;

    [ObservableProperty]
    private string? _issn;

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
    private Visibility _showFirstPublication = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCountry = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showIssn = Visibility.Collapsed;

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

    public MagazineViewViewModel(ILogger<MagazineViewViewModel> logger,            IRegionManager    regionManager,
                                 MagazinesService              magazinesService,  IStringLocalizer  localizer)
    {
        _logger           = logger;
        _regionManager    = regionManager;
        _magazinesService = magazinesService;
        _localizer        = localizer;
    }

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

        if(navigationContext.Parameters.TryGetValue<long>(NavParamKeys.MagazineId, out long magazineId))
            _ = LoadMagazineAsync(magazineId);
    }

    [RelayCommand]
    public Task GoBack()
    {
        switch(_navigationSource)
        {
            case nameof(MagazinesListViewModel):
                _regionManager.RequestNavigate(RegionNames.Content, nameof(MagazinesListPage));

                break;

            default:
                _regionManager.RequestNavigate(RegionNames.Content, nameof(MagazinesPage));

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

    public async Task LoadMagazineAsync(long magazineId)
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

            MagazineDto? magazine = await _magazinesService.GetMagazineAsync(magazineId);

            if(magazine is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["Magazine not found"];
                IsLoading    = false;

                return;
            }

            MagazineTitle = magazine.Title ?? string.Empty;
            NativeTitle   = magazine.NativeTitle;
            Country       = magazine.Country;
            Issn          = magazine.Issn;

            if(magazine.FirstPublication.HasValue)
                FirstPublicationDisplay = (magazine.FirstPublicationPrecision ?? 0) == 2 ? $"{magazine.FirstPublication.Value.Year}" : (magazine.FirstPublicationPrecision ?? 0) == 1 ? magazine.FirstPublication.Value.ToString("MMMM yyyy") : magazine.FirstPublication.Value.DateTime.ToString("MMMM d, yyyy");

            // Load synopsis
            DocumentSynopsisDto? synopsis = await _magazinesService.GetMagazineSynopsisAsync(magazineId);

            if(synopsis is not null)
                SynopsisText = synopsis.Text;

            // Load people (by role)
            List<PersonByMagazineDto> people = await _magazinesService.GetPeopleByMagazineAsync(magazineId);

            foreach(PersonByMagazineDto person in people)
            {
                string  name    = person.DisplayName ?? person.Alias ?? $"{person.Name} {person.Surname}".Trim();
                string? roleStr = person.Role;
                string  display = !string.IsNullOrEmpty(roleStr) ? $"{name} ({roleStr})" : name;
                People.Add(display);
            }

            // Load companies (by role)
            List<CompanyByMagazineDto> companies = await _magazinesService.GetCompaniesByMagazineAsync(magazineId);

            foreach(CompanyByMagazineDto company in companies)
            {
                string  name    = company.Company ?? string.Empty;
                string? roleStr = company.Role;
                string  display = !string.IsNullOrEmpty(roleStr) ? $"{name} ({roleStr})" : name;
                Companies.Add(display);
            }

            // Load machines
            List<MagazineByMachineDto> machines = await _magazinesService.GetMachinesByMagazineAsync(magazineId);

            foreach(MagazineByMachineDto machine in machines)
                Machines.Add(machine.Machine ?? string.Empty);

            // Load machine families
            List<MagazineByMachineFamilyDto> families =
                await _magazinesService.GetMachineFamiliesByMagazineAsync(magazineId);

            foreach(MagazineByMachineFamilyDto family in families)
                MachineFamilies.Add(family.MachineFamily ?? string.Empty);

            UpdateVisibilities();
            IsDataLoaded = true;
            IsLoading    = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading magazine {MagazineId}", magazineId);
            HasError     = true;
            ErrorMessage = ex.Message;
            IsLoading    = false;
        }
    }

    private void UpdateVisibilities()
    {
        ShowNativeTitle      = !string.IsNullOrEmpty(NativeTitle) ? Visibility.Visible : Visibility.Collapsed;
        ShowFirstPublication = !string.IsNullOrEmpty(FirstPublicationDisplay) ? Visibility.Visible : Visibility.Collapsed;
        ShowCountry          = !string.IsNullOrEmpty(Country) ? Visibility.Visible : Visibility.Collapsed;
        ShowIssn             = !string.IsNullOrEmpty(Issn) ? Visibility.Visible : Visibility.Collapsed;
        ShowSynopsis         = !string.IsNullOrEmpty(SynopsisText) ? Visibility.Visible : Visibility.Collapsed;
        ShowPeople           = People.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowCompanies        = Companies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowMachines         = Machines.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowMachineFamilies  = MachineFamilies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}
