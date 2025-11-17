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

public partial class ProcessorDetailViewModel : ObservableObject
{
    private readonly CompaniesService                  _companiesService;
    private readonly IStringLocalizer                  _localizer;
    private readonly ILogger<ProcessorDetailViewModel> _logger;
    private readonly INavigator                        _navigator;
    private readonly ProcessorsService                 _processorsService;

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
    private ProcessorDto? _processor;

    [ObservableProperty]
    private int _processorId;

    public ProcessorDetailViewModel(ProcessorsService processorsService, CompaniesService companiesService,
                                    IStringLocalizer  localizer,         ILogger<ProcessorDetailViewModel> logger,
                                    INavigator        navigator)
    {
        _processorsService     = processorsService;
        _companiesService      = companiesService;
        _localizer             = localizer;
        _logger                = logger;
        _navigator             = navigator;
        LoadData               = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand          = new AsyncRelayCommand(GoBackAsync);
        SelectMachineCommand   = new AsyncRelayCommand<int>(SelectMachineAsync);
        ComputersFilterCommand = new RelayCommand(() => FilterComputers());
        ConsolesFilterCommand  = new RelayCommand(() => FilterConsoles());
        Title = _localizer["Processor Details"];
    }

    public IAsyncRelayCommand LoadData               { get; }
    public ICommand           GoBackCommand          { get; }
    public IAsyncRelayCommand SelectMachineCommand   { get; }
    public ICommand           ComputersFilterCommand { get; }
    public ICommand           ConsolesFilterCommand  { get; }

    public string Title { get; }

    /// <summary>
    ///     Loads Processor details
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

            if(ProcessorId <= 0)
            {
                ErrorMessage = _localizer["Invalid Processor ID"].Value;
                HasError     = true;

                return;
            }

            _logger.LogInformation("Loading Processor details for ID: {ProcessorId}", ProcessorId);

            // Load Processor details
            Processor = await _processorsService.GetProcessorByIdAsync(ProcessorId);

            if(Processor is null)
            {
                ErrorMessage = _localizer["Processor not found"].Value;
                HasError     = true;

                return;
            }

            // Set manufacturer name (from Company field or fetch by CompanyId if empty)
            ManufacturerName = Processor.Company ?? string.Empty;

            if(string.IsNullOrEmpty(ManufacturerName) && Processor.CompanyId.HasValue)
            {
                try
                {
                    CompanyDto? company = await _companiesService.GetCompanyByIdAsync(Processor.CompanyId.Value);
                    if(company != null) ManufacturerName = company.Name ?? string.Empty;
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load company for Processor {ProcessorId}", ProcessorId);
                }
            }

            _logger.LogInformation("Processor loaded: {Name}, Company: {Company}", Processor.Name, ManufacturerName);

            // Load machines and separate into computers and consoles
            try
            {
                List<MachineDto>? machines = await _processorsService.GetMachinesByProcessorAsync(ProcessorId);

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

                    _logger.LogInformation("Loaded {ComputerCount} computers and {ConsoleCount} consoles for Processor {ProcessorId}",
                                           Computers.Count,
                                           Consoles.Count,
                                           ProcessorId);
                }
                else
                {
                    HasComputers = false;
                    HasConsoles  = false;
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load machines for Processor {ProcessorId}", ProcessorId);
                HasComputers = false;
                HasConsoles  = false;
            }

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading Processor details: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load processor details. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Navigates back to the Processor list
    /// </summary>
    private async Task GoBackAsync()
    {
        // If we came from a machine view, go back to machine view
        if(_navigationSource is MachineViewViewModel machineVm)
        {
            await _navigator.NavigateViewModelAsync<MachineViewViewModel>(this);

            return;
        }

        // Default: go back to Processor list
        await _navigator.NavigateViewModelAsync<ProcessorsListViewModel>(this);
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
}