#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Presentation.Models;
using Marechai.App.Services;
using Uno.Extensions.Navigation;

namespace Marechai.App.Presentation.ViewModels;

public partial class SoundSynthDetailViewModel : ObservableObject
{
    private readonly CompaniesService                   _companiesService;
    private readonly IStringLocalizer                   _localizer;
    private readonly ILogger<SoundSynthDetailViewModel> _logger;
    private readonly INavigator                         _navigator;
    private readonly SoundSynthsService                 _soundSynthsService;

    [ObservableProperty]
    private ObservableCollection<MachineItem> _computers = [];

    [ObservableProperty]
    private string _computersFilterText = string.Empty;

    [ObservableProperty]
    private string _consolesFilterText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MachineItem> _consoles = [];

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MachineItem> _filteredComputers = [];

    [ObservableProperty]
    private ObservableCollection<MachineItem> _filteredConsoles = [];

    [ObservableProperty]
    private bool _hasComputers;

    [ObservableProperty]
    private bool _hasConsoles;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _manufacturerName = string.Empty;
    private object? _navigationSource;

    [ObservableProperty]
    private SoundSynthDto? _soundSynth;

    [ObservableProperty]
    private int _soundSynthId;

    public SoundSynthDetailViewModel(SoundSynthsService soundSynthsService, CompaniesService companiesService,
                                     IStringLocalizer   localizer,          ILogger<SoundSynthDetailViewModel> logger,
                                     INavigator         navigator)
    {
        _soundSynthsService    = soundSynthsService;
        _companiesService      = companiesService;
        _localizer             = localizer;
        _logger                = logger;
        _navigator             = navigator;
        LoadData               = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand          = new AsyncRelayCommand(GoBackAsync);
        SelectMachineCommand   = new AsyncRelayCommand<int>(SelectMachineAsync);
        ComputersFilterCommand = new RelayCommand(() => FilterComputers());
        ConsolesFilterCommand  = new RelayCommand(() => FilterConsoles());

        Title = _localizer["Sound Synthesizer Details"];
    }

    public IAsyncRelayCommand LoadData               { get; }
    public ICommand           GoBackCommand          { get; }
    public IAsyncRelayCommand SelectMachineCommand   { get; }
    public ICommand           ComputersFilterCommand { get; }
    public ICommand           ConsolesFilterCommand  { get; }

    public string Title { get; }

    /// <summary>
    ///     Loads Sound Synthesizer details including computers and consoles
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            Computers.Clear();
            Consoles.Clear();

            if(SoundSynthId <= 0)
            {
                ErrorMessage = _localizer["Invalid Sound Synthesizer ID"].Value;
                HasError     = true;

                return;
            }

            _logger.LogInformation("Loading Sound Synthesizer details for ID: {SoundSynthId}", SoundSynthId);

            // Load Sound Synthesizer details
            SoundSynth = await _soundSynthsService.GetSoundSynthByIdAsync(SoundSynthId);

            if(SoundSynth is null)
            {
                ErrorMessage = _localizer["Sound Synthesizer not found"].Value;
                HasError     = true;

                return;
            }

            // Set manufacturer name (from Company field or fetch by CompanyId if empty)
            ManufacturerName = SoundSynth.Company ?? string.Empty;

            if(string.IsNullOrEmpty(ManufacturerName) && SoundSynth.CompanyId.HasValue)
            {
                try
                {
                    CompanyDto? company = await _companiesService.GetCompanyByIdAsync(SoundSynth.CompanyId.Value);
                    if(company != null) ManufacturerName = company.Name ?? string.Empty;
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load company for Sound Synthesizer {SoundSynthId}", SoundSynthId);
                }
            }

            _logger.LogInformation("Sound Synthesizer loaded: {Name}, Company: {Company}",
                                   SoundSynth.Name,
                                   ManufacturerName);

            // Load machines and separate into computers and consoles
            try
            {
                List<MachineDto>? machines = await _soundSynthsService.GetMachinesBySoundSynthAsync(SoundSynthId);

                if(machines != null && machines.Count > 0)
                {
                    Computers.Clear();
                    Consoles.Clear();

                    foreach(MachineDto machine in machines)
                    {
                        var machineItem = new MachineItem
                        {
                            Id           = machine.Id               ?? 0,
                            Name         = machine.Name             ?? string.Empty,
                            Manufacturer = machine.Company          ?? string.Empty,
                            Year         = machine.Introduced?.Year ?? 0
                        };

                        // Distinguish between computers and consoles based on Type
                        if(machine.Type == 2) // MachineType.Console
                            Consoles.Add(machineItem);
                        else // MachineType.Computer or Unknown
                            Computers.Add(machineItem);
                    }

                    HasComputers = Computers.Count > 0;
                    HasConsoles  = Consoles.Count  > 0;

                    // Initialize filtered collections
                    FilterComputers();
                    FilterConsoles();

                    _logger.LogInformation("Loaded {ComputerCount} computers and {ConsoleCount} consoles for Sound Synthesizer {SoundSynthId}",
                                           Computers.Count,
                                           Consoles.Count,
                                           SoundSynthId);
                }
                else
                {
                    HasComputers = false;
                    HasConsoles  = false;
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load machines for Sound Synthesizer {SoundSynthId}", SoundSynthId);
                HasComputers = false;
                HasConsoles  = false;
            }

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading Sound Synthesizer details: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load Sound Synthesizer details. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Filters computers based on search text
    /// </summary>
    private void FilterComputers()
    {
        if(string.IsNullOrWhiteSpace(ComputersFilterText))
        {
            FilteredComputers.Clear();
            foreach(MachineItem computer in Computers) FilteredComputers.Add(computer);
        }
        else
        {
            var filtered = Computers
                          .Where(c => c.Name.Contains(ComputersFilterText, StringComparison.OrdinalIgnoreCase))
                          .ToList();

            FilteredComputers.Clear();
            foreach(MachineItem computer in filtered) FilteredComputers.Add(computer);
        }
    }

    /// <summary>
    ///     Filters consoles based on search text
    /// </summary>
    private void FilterConsoles()
    {
        if(string.IsNullOrWhiteSpace(ConsolesFilterText))
        {
            FilteredConsoles.Clear();
            foreach(MachineItem console in Consoles) FilteredConsoles.Add(console);
        }
        else
        {
            var filtered = Consoles.Where(c => c.Name.Contains(ConsolesFilterText, StringComparison.OrdinalIgnoreCase))
                                   .ToList();

            FilteredConsoles.Clear();
            foreach(MachineItem console in filtered) FilteredConsoles.Add(console);
        }
    }

    /// <summary>
    ///     Navigates back to the Sound Synthesizer list
    /// </summary>
    private async Task GoBackAsync()
    {
        // If we came from a machine view, go back to machine view
        if(_navigationSource is MachineViewViewModel machineVm)
        {
            await _navigator.NavigateViewModelAsync<MachineViewViewModel>(this);

            return;
        }

        // Default: go back to Sound Synthesizer list
        await _navigator.NavigateViewModelAsync<SoundSynthsListViewModel>(this);
    }

    /// <summary>
    ///     Navigates to machine detail view
    /// </summary>
    private async Task SelectMachineAsync(int machineId)
    {
        if(machineId <= 0) return;

        var navParam = new MachineViewNavigationParameter
        {
            MachineId        = machineId,
            NavigationSource = this
        };

        await _navigator.NavigateViewModelAsync<MachineViewViewModel>(this, data: navParam);
    }

    /// <summary>
    ///     Sets the navigation source (where we came from).
    /// </summary>
    public void SetNavigationSource(object? source)
    {
        _navigationSource = source;
    }

    /// <summary>
    ///     Machine item for displaying computers or consoles that use the Sound Synthesizer
    /// </summary>
    public class MachineItem
    {
        public int    Id           { get; set; }
        public string Name         { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int    Year         { get; set; }

        public string YearDisplay => Year > 0 ? Year.ToString() : "Unknown";
    }
}