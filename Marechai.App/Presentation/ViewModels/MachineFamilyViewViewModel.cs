#nullable enable

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;

namespace Marechai.App.Presentation.ViewModels;

public partial class MachineFamilyViewViewModel : ObservableObject, IRegionAware
{
    private readonly MachineFamiliesService            _machineFamiliesService;
    private readonly IStringLocalizer                  _localizer;
    private readonly ILogger<MachineFamilyViewViewModel> _logger;
    private readonly IRegionManager                    _regionManager;
    private          int                                _familyId;
    private          int?                               _sourceMachineId;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private int? _companyId;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _familyName = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _machines = [];

    public bool ShowCompanyLink   => CompanyId.HasValue && !string.IsNullOrEmpty(CompanyName);
    public bool ShowMachines      => Machines.Count > 0;
    public bool ShowEmptyMachines => IsDataLoaded && !HasError && Machines.Count == 0;

    public string Title { get; }

    public MachineFamilyViewViewModel(MachineFamiliesService            machineFamiliesService,
                                      IStringLocalizer                  localizer,
                                      ILogger<MachineFamilyViewViewModel> logger,
                                      IRegionManager                    regionManager)
    {
        _machineFamiliesService = machineFamiliesService;
        _localizer               = localizer;
        _logger                  = logger;
        _regionManager           = regionManager;
        Title                    = _localizer["Machine Family"];
    }

    partial void OnCompanyNameChanged(string value)
    {
        OnPropertyChanged(nameof(ShowCompanyLink));
    }

    partial void OnCompanyIdChanged(int? value)
    {
        OnPropertyChanged(nameof(ShowCompanyLink));
    }

    partial void OnMachinesChanged(ObservableCollection<CompanyDetailMachine> value)
    {
        OnPropertyChanged(nameof(ShowMachines));
        OnPropertyChanged(nameof(ShowEmptyMachines));
    }

    partial void OnIsDataLoadedChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowEmptyMachines));
    }

    partial void OnHasErrorChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowEmptyMachines));
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _sourceMachineId = navigationContext.Parameters.TryGetValue<int>(NavParamKeys.MachineId, out int sourceMachineId)
                                ? sourceMachineId
                                : null;

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.MachineFamilyId, out int familyId))
        {
            _familyId = familyId;
            _ = LoadFamilyAsync(familyId);
        }
    }

    [RelayCommand]
    public Task NavigateToMachine(CompanyDetailMachine? machine)
    {
        if(machine is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineId, machine.Id },
            { NavParamKeys.NavigationSource, nameof(MachineFamilyViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToCompany()
    {
        if(!CompanyId.HasValue) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.CompanyId, CompanyId.Value }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(CompanyDetailPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task GoBack()
    {
        if(_sourceMachineId.HasValue)
        {
            var parameters = new NavigationParameters
            {
                { NavParamKeys.MachineId, _sourceMachineId.Value },
                { NavParamKeys.NavigationSource, nameof(MachineFamilyViewViewModel) }
            };

            _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);
        }
        else
            _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));

        return Task.CompletedTask;
    }

    private async Task LoadFamilyAsync(int familyId)
    {
        try
        {
            IsLoading    = true;
            IsDataLoaded = false;
            HasError     = false;
            ErrorMessage = string.Empty;
            Machines.Clear();

            _logger.LogInformation("Loading machine family {FamilyId}", familyId);

            MachineFamilyDto? family = await _machineFamiliesService.GetByIdAsync(familyId);

            if(family is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["Machine family not found in database"];
                IsDataLoaded = true;

                return;
            }

            FamilyName  = family.Name;
            CompanyName = family.Company ?? string.Empty;
            CompanyId   = family.CompanyId > 0 ? family.CompanyId : null;

            var machines = new ObservableCollection<CompanyDetailMachine>();

            foreach(MachineDto machine in await _machineFamiliesService.GetMachinesAsync(familyId))
            {
                machines.Add(new CompanyDetailMachine
                {
                    Id   = machine.Id ?? 0,
                    Name = machine.Name ?? string.Empty
                });
            }

            Machines = machines;

            IsDataLoaded = true;
        }
        catch(System.Exception ex)
        {
            _logger.LogError(ex, "Error loading machine family {FamilyId}", familyId);
            HasError     = true;
            ErrorMessage = ex.Message;
            IsDataLoaded = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
