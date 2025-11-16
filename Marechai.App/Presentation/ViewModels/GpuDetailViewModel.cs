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

public partial class GpuDetailViewModel : ObservableObject
{
    private readonly CompaniesService            _companiesService;
    private readonly GpusService                 _gpusService;
    private readonly IStringLocalizer            _localizer;
    private readonly ILogger<GpuDetailViewModel> _logger;
    private readonly INavigator                  _navigator;

    [ObservableProperty]
    private ObservableCollection<MachineItem> _computers = [];

    [ObservableProperty]
    private string _computersFilterText = string.Empty;

    [ObservableProperty]
    private string _consoelsFilterText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MachineItem> _consoles = [];

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MachineItem> _filteredComputers = [];

    [ObservableProperty]
    private ObservableCollection<MachineItem> _filteredConsoles = [];

    [ObservableProperty]
    private GpuDto? _gpu;

    [ObservableProperty]
    private int _gpuId;

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
    private ObservableCollection<ResolutionItem> _resolutions = [];

    public GpuDetailViewModel(GpusService gpusService, CompaniesService companiesService, IStringLocalizer localizer,
                              ILogger<GpuDetailViewModel> logger, INavigator navigator)
    {
        _gpusService           = gpusService;
        _companiesService      = companiesService;
        _localizer             = localizer;
        _logger                = logger;
        _navigator             = navigator;
        LoadData               = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand          = new AsyncRelayCommand(GoBackAsync);
        SelectMachineCommand   = new AsyncRelayCommand<int>(SelectMachineAsync);
        ComputersFilterCommand = new RelayCommand(() => FilterComputers());
        ConsolesFilterCommand  = new RelayCommand(() => FilterConsoles());
    }

    public IAsyncRelayCommand LoadData               { get; }
    public ICommand           GoBackCommand          { get; }
    public IAsyncRelayCommand SelectMachineCommand   { get; }
    public ICommand           ComputersFilterCommand { get; }
    public ICommand           ConsolesFilterCommand  { get; }

    public string Title { get; } = "GPU Details";

    /// <summary>
    ///     Loads GPU details including resolutions, computers, and consoles
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            Resolutions.Clear();
            Computers.Clear();
            Consoles.Clear();

            if(GpuId <= 0)
            {
                ErrorMessage = _localizer["Invalid GPU ID"].Value;
                HasError     = true;

                return;
            }

            _logger.LogInformation("Loading GPU details for ID: {GpuId}", GpuId);

            // Load GPU details
            Gpu = await _gpusService.GetGpuByIdAsync(GpuId);

            if(Gpu is null)
            {
                ErrorMessage = _localizer["Graphics processing unit not found"].Value;
                HasError     = true;

                return;
            }

            // Set manufacturer name (from Company field or fetch by CompanyId if empty)
            ManufacturerName = Gpu.Company ?? string.Empty;

            if(string.IsNullOrEmpty(ManufacturerName) && Gpu.CompanyId.HasValue)
            {
                try
                {
                    CompanyDto? company = await _companiesService.GetCompanyByIdAsync(Gpu.CompanyId.Value);
                    if(company != null) ManufacturerName = company.Name ?? string.Empty;
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load company for GPU {GpuId}", GpuId);
                }
            }

            // Format display name
            string displayName = Gpu.Name ?? string.Empty;

            if(displayName == "DB_FRAMEBUFFER")
                displayName = "Framebuffer";
            else if(displayName == "DB_SOFTWARE")
                displayName                               = "Software";
            else if(displayName == "DB_NONE") displayName = "None";

            _logger.LogInformation("GPU loaded: {Name}, Company: {Company}", displayName, ManufacturerName);

            // Load resolutions
            try
            {
                List<ResolutionByGpuDto>? resolutions = await _gpusService.GetResolutionsByGpuAsync(GpuId);

                if(resolutions != null && resolutions.Count > 0)
                {
                    Resolutions.Clear();

                    foreach(ResolutionByGpuDto res in resolutions)
                    {
                        // Get the full resolution DTO using the resolution ID
                        if(res.ResolutionId.HasValue)
                        {
                            ResolutionDto? resolutionDto =
                                await _gpusService.GetResolutionByIdAsync(res.ResolutionId.Value);

                            if(resolutionDto != null)
                            {
                                Resolutions.Add(new ResolutionItem
                                {
                                    Id        = resolutionDto.Id ?? 0,
                                    Name      = $"{resolutionDto.Width}x{resolutionDto.Height}",
                                    Width     = resolutionDto.Width     ?? 0,
                                    Height    = resolutionDto.Height    ?? 0,
                                    Colors    = resolutionDto.Colors    ?? 0,
                                    Palette   = resolutionDto.Palette   ?? 0,
                                    Chars     = resolutionDto.Chars     ?? false,
                                    Grayscale = resolutionDto.Grayscale ?? false
                                });
                            }
                        }
                    }

                    _logger.LogInformation("Loaded {Count} resolutions for GPU {GpuId}", Resolutions.Count, GpuId);
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load resolutions for GPU {GpuId}", GpuId);
            }

            // Load machines and separate into computers and consoles
            try
            {
                List<MachineDto>? machines = await _gpusService.GetMachinesByGpuAsync(GpuId);

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

                    _logger.LogInformation("Loaded {ComputerCount} computers and {ConsoleCount} consoles for GPU {GpuId}",
                                           Computers.Count,
                                           Consoles.Count,
                                           GpuId);
                }
                else
                {
                    HasComputers = false;
                    HasConsoles  = false;
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load machines for GPU {GpuId}", GpuId);
                HasComputers = false;
                HasConsoles  = false;
            }

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading GPU details: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load graphics processing unit details. Please try again later."].Value;
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
        if(string.IsNullOrWhiteSpace(ConsoelsFilterText))
        {
            FilteredConsoles.Clear();
            foreach(MachineItem console in Consoles) FilteredConsoles.Add(console);
        }
        else
        {
            var filtered = Consoles.Where(c => c.Name.Contains(ConsoelsFilterText, StringComparison.OrdinalIgnoreCase))
                                   .ToList();

            FilteredConsoles.Clear();
            foreach(MachineItem console in filtered) FilteredConsoles.Add(console);
        }
    }

    /// <summary>
    ///     Navigates back to the GPU list
    /// </summary>
    private async Task GoBackAsync()
    {
        // If we came from a machine view, go back to machine view
        if(_navigationSource is MachineViewViewModel machineVm)
        {
            await _navigator.NavigateViewModelAsync<MachineViewViewModel>(this);

            return;
        }

        // Default: go back to GPU list
        await _navigator.NavigateViewModelAsync<GpusListViewModel>(this);
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

/// <summary>
///     Resolution item for displaying GPU supported resolutions
/// </summary>
public class ResolutionItem
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = string.Empty;
    public int    Width     { get; set; }
    public int    Height    { get; set; }
    public long   Colors    { get; set; }
    public long   Palette   { get; set; }
    public bool   Chars     { get; set; }
    public bool   Grayscale { get; set; }

    public string Resolution => $"{Width}x{Height}";

    public string ResolutionType => Chars ? "Text" : "Pixel";

    public string ResolutionDisplay => Chars ? $"{Width}x{Height} characters" : $"{Width}x{Height}";

    public string ColorDisplay => Grayscale
                                      ? $"{Colors} grays"
                                      : Palette > 0
                                          ? $"{Colors} colors from a palette of {Palette} colors"
                                          : $"{Colors} colors";
}

/// <summary>
///     Machine item for displaying computers or consoles that use the GPU
/// </summary>
public class MachineItem
{
    public int    Id           { get; set; }
    public string Name         { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public int    Year         { get; set; }

    public string YearDisplay => Year > 0 ? Year.ToString() : "Unknown";
}