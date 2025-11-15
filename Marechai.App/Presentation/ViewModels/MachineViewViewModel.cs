/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Humanizer;
using Marechai.App.Helpers;
using Marechai.App.Services;
using Marechai.Data;
using Microsoft.UI.Xaml;
using Uno.Extensions.Navigation;

namespace Marechai.App.Presentation.ViewModels;

public partial class MachineViewViewModel : ObservableObject
{
    private readonly ComputersService              _computersService;
    private readonly ILogger<MachineViewViewModel> _logger;
    private readonly INavigator                    _navigator;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string? _familyName;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string? _introductionDateDisplay;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isPrototype;

    [ObservableProperty]
    private string _machineName = string.Empty;

    [ObservableProperty]
    private string? _modelName;
    private object? _navigationSource;

    [ObservableProperty]
    private Visibility _showFamily = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showFamilyOrModel = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showGpus = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showIntroductionDate = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showMemory = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showModel = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showProcessors = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSoundSynthesizers = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showStorage = Visibility.Collapsed;

    public MachineViewViewModel(ILogger<MachineViewViewModel> logger, INavigator navigator,
                                ComputersService              computersService)
    {
        _logger           = logger;
        _navigator        = navigator;
        _computersService = computersService;
    }

    public ObservableCollection<ProcessorDisplayItem>        Processors        { get; } = [];
    public ObservableCollection<MemoryDisplayItem>           Memory            { get; } = [];
    public ObservableCollection<GpuDisplayItem>              Gpus              { get; } = [];
    public ObservableCollection<SoundSynthesizerDisplayItem> SoundSynthesizers { get; } = [];
    public ObservableCollection<StorageDisplayItem>          Storage           { get; } = [];

    [RelayCommand]
    public async Task GoBack()
    {
        // If we came from News, navigate back to News
        if(_navigationSource is NewsViewModel)
        {
            await _navigator.NavigateViewModelAsync<NewsViewModel>(this);

            return;
        }

        // Otherwise, try to go back in the navigation stack
        await _navigator.GoBack(this);
    }

    /// <summary>
    ///     Sets the navigation source (where we came from).
    /// </summary>
    public void SetNavigationSource(object? source)
    {
        _navigationSource = source;
    }

    [RelayCommand]
    public Task LoadData()
    {
        // Placeholder for retry functionality
        HasError     = false;
        ErrorMessage = string.Empty;

        return Task.CompletedTask;
    }

    public async Task LoadMachineAsync(int machineId)
    {
        try
        {
            IsLoading    = true;
            IsDataLoaded = false;
            HasError     = false;
            ErrorMessage = string.Empty;
            Processors.Clear();
            Memory.Clear();
            Gpus.Clear();
            SoundSynthesizers.Clear();
            Storage.Clear();

            _logger.LogInformation("Loading machine {MachineId}", machineId);

            // Fetch machine data from API
            MachineDto? machine = await _computersService.GetMachineByIdAsync(machineId);

            if(machine is null)
            {
                HasError     = true;
                ErrorMessage = "Machine not found";
                IsLoading    = false;

                return;
            }

            // Populate basic information
            MachineName = machine.Name    ?? string.Empty;
            CompanyName = machine.Company ?? string.Empty;
            FamilyName  = machine.FamilyName;
            ModelName   = machine.Model;

            // Check if this is a prototype (year 1000 is used as placeholder for prototypes)
            IsPrototype = machine.Introduced?.Year == 1000;

            // Set introduction date if available and not a prototype
            if(machine.Introduced.HasValue && machine.Introduced.Value.Year != 1000)
                IntroductionDateDisplay = machine.Introduced.Value.ToString("MMMM d, yyyy");

            // Populate processors
            if(machine.Processors != null)
            {
                foreach(ProcessorDto processor in machine.Processors)
                {
                    var details = new List<string>();
                    int speed   = UntypedNodeExtractor.ExtractInt(processor.Speed);
                    int gprSize = UntypedNodeExtractor.ExtractInt(processor.GprSize);
                    int cores   = UntypedNodeExtractor.ExtractInt(processor.Cores);

                    if(speed   > 0) details.Add($"{speed} MHz");
                    if(gprSize > 0) details.Add($"{gprSize} bits");
                    if(cores   > 1) details.Add($"{cores} cores");

                    Processors.Add(new ProcessorDisplayItem
                    {
                        DisplayName  = processor.Name    ?? string.Empty,
                        Manufacturer = processor.Company ?? string.Empty,
                        HasDetails   = details.Count > 0,
                        DetailsText  = string.Join(", ", details)
                    });
                }
            }

            // Populate memory
            if(machine.Memory != null)
            {
                foreach(MemoryDto mem in machine.Memory)
                {
                    long size = UntypedNodeExtractor.ExtractLong(mem.Size);

                    string sizeStr = size > 0
                                         ? size > 1024 ? $"{size} bytes ({size.Bytes().Humanize()})" : $"{size} bytes"
                                         : "Unknown";

                    // Get humanized memory usage description
                    string usageDescription = mem.Usage.HasValue
                                                  ? ((MemoryUsage)mem.Usage.Value).Humanize()
                                                  : "Unknown";

                    Memory.Add(new MemoryDisplayItem
                    {
                        SizeDisplay = sizeStr,
                        TypeDisplay = usageDescription
                    });
                }
            } // Populate GPUs

            if(machine.Gpus != null)
            {
                foreach(GpuDto gpu in machine.Gpus)
                {
                    Gpus.Add(new GpuDisplayItem
                    {
                        DisplayName     = gpu.Name    ?? string.Empty,
                        Manufacturer    = gpu.Company ?? string.Empty,
                        HasManufacturer = !string.IsNullOrEmpty(gpu.Company)
                    });
                }
            }

            // Populate sound synthesizers
            if(machine.SoundSynthesizers != null)
            {
                foreach(SoundSynthDto synth in machine.SoundSynthesizers)
                {
                    var details = new List<string>();
                    int voices  = UntypedNodeExtractor.ExtractInt(synth.Voices);

                    if(voices > 0) details.Add($"{voices} voices");

                    SoundSynthesizers.Add(new SoundSynthesizerDisplayItem
                    {
                        DisplayName = synth.Name ?? string.Empty,
                        HasDetails  = details.Count > 0,
                        DetailsText = string.Join(", ", details)
                    });
                }
            }

            // Populate storage
            if(machine.Storage != null)
            {
                foreach(StorageDto storage in machine.Storage)
                {
                    long capacity = UntypedNodeExtractor.ExtractLong(storage.Capacity);

                    string displayText = capacity > 0
                                             ? capacity > 1024
                                                   ? $"{capacity} bytes ({capacity.Bytes().Humanize()})"
                                                   : $"{capacity} bytes"
                                             : "Storage";

                    // Get humanized storage type description
                    string typeNote = storage.Type.HasValue ? ((StorageType)storage.Type.Value).Humanize() : "Unknown";

                    Storage.Add(new StorageDisplayItem
                    {
                        DisplayText = displayText,
                        TypeNote    = typeNote
                    });
                }
            }

            UpdateVisibilities();
            IsDataLoaded = true;
            IsLoading    = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading machine {MachineId}", machineId);
            HasError     = true;
            ErrorMessage = ex.Message;
            IsLoading    = false;
        }
    }

    private void UpdateVisibilities()
    {
        ShowIntroductionDate =
            !string.IsNullOrEmpty(IntroductionDateDisplay) ? Visibility.Visible : Visibility.Collapsed;

        ShowFamily = !string.IsNullOrEmpty(FamilyName) ? Visibility.Visible : Visibility.Collapsed;
        ShowModel  = !string.IsNullOrEmpty(ModelName) ? Visibility.Visible : Visibility.Collapsed;

        ShowFamilyOrModel = ShowFamily == Visibility.Visible || ShowModel == Visibility.Visible
                                ? Visibility.Visible
                                : Visibility.Collapsed;

        ShowProcessors        = Processors.Count        > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowMemory            = Memory.Count            > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowGpus              = Gpus.Count              > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowSoundSynthesizers = SoundSynthesizers.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowStorage           = Storage.Count           > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}

/// <summary>
///     Display item for processor information
/// </summary>
public class ProcessorDisplayItem
{
    public string DisplayName  { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public bool   HasDetails   { get; set; }
    public string DetailsText  { get; set; } = string.Empty;
}

/// <summary>
///     Display item for memory information
/// </summary>
public class MemoryDisplayItem
{
    public string SizeDisplay { get; set; } = string.Empty;
    public string TypeDisplay { get; set; } = string.Empty;
}

/// <summary>
///     Display item for GPU information
/// </summary>
public class GpuDisplayItem
{
    public string DisplayName     { get; set; } = string.Empty;
    public string Manufacturer    { get; set; } = string.Empty;
    public bool   HasManufacturer { get; set; }
}

/// <summary>
///     Display item for sound synthesizer information
/// </summary>
public class SoundSynthesizerDisplayItem
{
    public string DisplayName { get; set; } = string.Empty;
    public bool   HasDetails  { get; set; }
    public string DetailsText { get; set; } = string.Empty;
}

/// <summary>
///     Display item for storage information
/// </summary>
public class StorageDisplayItem
{
    public string DisplayText { get; set; } = string.Empty;
    public string TypeNote    { get; set; } = string.Empty;
}