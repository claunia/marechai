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
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Humanizer;
using Marechai.App.Presentation.Models;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Marechai.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Uno.Extensions.Navigation;

namespace Marechai.App.Presentation.ViewModels;

public partial class MachineViewViewModel : ObservableObject
{
    private readonly ComputersService              _computersService;
    private readonly ILogger<MachineViewViewModel> _logger;
    private readonly INavigator                    _navigator;
    private readonly MachinePhotoCache             _photoCache;
    private IStringLocalizer _localizer;
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
    private Visibility _showPhotos = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showProcessors = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSoundSynthesizers = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showStorage = Visibility.Collapsed;

    public MachineViewViewModel(ILogger<MachineViewViewModel> logger,           INavigator        navigator,
                                ComputersService              computersService, MachinePhotoCache photoCache, IStringLocalizer localizer)
    {
        _logger           = logger;
        _navigator        = navigator;
        _computersService = computersService;
        _photoCache       = photoCache;
        _localizer        = localizer;
    }

    public ObservableCollection<ProcessorDisplayItem>        Processors        { get; } = [];
    public ObservableCollection<MemoryDisplayItem>           Memory            { get; } = [];
    public ObservableCollection<GpuDisplayItem>              Gpus              { get; } = [];
    public ObservableCollection<SoundSynthesizerDisplayItem> SoundSynthesizers { get; } = [];
    public ObservableCollection<StorageDisplayItem>          Storage           { get; } = [];
    public ObservableCollection<PhotoCarouselDisplayItem>    Photos            { get; } = [];

    [RelayCommand]
    public async Task GoBack()
    {
        // If we came from News, navigate back to News
        if(_navigationSource is NewsViewModel)
        {
            await _navigator.NavigateViewModelAsync<NewsViewModel>(this);

            return;
        }

        // If we came from CompanyDetailViewModel, navigate back to company details
        if(_navigationSource is CompanyDetailViewModel companyVm)
        {
            var navParam = new CompanyDetailNavigationParameter
            {
                CompanyId = companyVm.CompanyId
            };

            await _navigator.NavigateViewModelAsync<CompanyDetailViewModel>(this, data: navParam);

            return;
        }

        // If we came from ConsolesListViewModel, navigate back to consoles list
        if(_navigationSource is ConsolesListViewModel)
        {
            await _navigator.NavigateViewModelAsync<ConsolesListViewModel>(this);

            return;
        }

        // If we came from ComputersListViewModel, navigate back to computers list
        if(_navigationSource is ComputersListViewModel)
        {
            await _navigator.NavigateViewModelAsync<ComputersListViewModel>(this);

            return;
        }

        // If we came from GpuDetailViewModel, navigate back to GPU details
        if(_navigationSource is GpuDetailViewModel gpuDetailVm)
        {
            var navParam = new GpuDetailNavigationParameter
            {
                GpuId            = gpuDetailVm.GpuId,
                NavigationSource = this
            };

            await _navigator.NavigateViewModelAsync<GpuDetailViewModel>(this, data: navParam);

            return;
        }

        // If we came from ProcessorDetailViewModel, navigate back to processor details
        if(_navigationSource is ProcessorDetailViewModel processorDetailVm)
        {
            var navParam = new ProcessorDetailNavigationParameter
            {
                ProcessorId      = processorDetailVm.ProcessorId,
                NavigationSource = this
            };

            await _navigator.NavigateViewModelAsync<ProcessorDetailViewModel>(this, data: navParam);

            return;
        }

        // If we came from SoundSynthDetailViewModel, navigate back to sound synth details
        if(_navigationSource is SoundSynthDetailViewModel soundSynthDetailVm)
        {
            var navParam = new SoundSynthDetailNavigationParameter
            {
                SoundSynthId     = soundSynthDetailVm.SoundSynthId,
                NavigationSource = this
            };

            await _navigator.NavigateViewModelAsync<SoundSynthDetailViewModel>(this, data: navParam);

            return;
        }

        // Otherwise, try to go back in the navigation stack
        await _navigator.GoBack(this);
    }

    [RelayCommand]
    public async Task ViewPhotoDetails(Guid photoId)
    {
        var navParam = new PhotoDetailNavigationParameter
        {
            PhotoId = photoId
        };

        _logger.LogInformation("Navigating to photo details for {PhotoId}", photoId);
        await _navigator.NavigateViewModelAsync<PhotoDetailViewModel>(this, data: navParam);
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
            Photos.Clear();

            _logger.LogInformation("Loading machine {MachineId}", machineId);

            // Fetch machine data from API
            MachineDto? machine = await _computersService.GetMachineByIdAsync(machineId);

            if(machine is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["Machine not found"];
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
                    var speed   = (int)(processor.Speed ?? 0);
                    int gprSize = processor.GprSize ?? 0;
                    int cores   = processor.Cores   ?? 0;

                    if(speed   > 0) details.Add(string.Format(_localizer["_0_MHz"], speed));
                    if(gprSize > 0) details.Add(string.Format(_localizer["_0_bits"], gprSize));
                    if(cores   > 1) details.Add(string.Format(_localizer["_0_cores"], cores));

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
                    long size = mem.Size ?? 0;

                    string sizeStr = size > 0
                                         ? size > 1024 ? string.Format(_localizer["_0_bytes_1_"], size,size.Bytes().Humanize()) : string.Format(_localizer["_0_bytes"], size)
                                         : _localizer["Unknown_male"];

                    // Get humanized memory usage description
                    string usageDescription = mem.Usage.HasValue
                                                  ? ((MemoryUsage)mem.Usage.Value).Humanize()
                                                  : _localizer["Unknown_male"];

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
                    int voices  = synth.Voices ?? 0;

                    if(voices > 0) details.Add(string.Format(_localizer["_0_voices"], voices));

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
                    long capacity = storage.Capacity ?? 0;

                    string displayText = capacity > 0
                                         ? capacity > 1024 ? string.Format(_localizer["_0_bytes_1_"], capacity, capacity.Bytes().Humanize()) : string.Format(_localizer["_0_bytes"], capacity)
                                             : _localizer["Storage"];

                    // Get humanized storage type description
                    string typeNote = storage.Type.HasValue ? ((StorageType)storage.Type.Value).Humanize() : _localizer["Unknown_male"];

                    Storage.Add(new StorageDisplayItem
                    {
                        DisplayText = displayText,
                        TypeNote    = typeNote
                    });
                }
            }

            // Populate photos
            List<Guid> photoIds = await _computersService.GetMachinePhotosAsync(machineId);

            if(photoIds.Count > 0)
            {
                foreach(Guid photoId in photoIds)
                {
                    var photoItem = new PhotoCarouselDisplayItem
                    {
                        PhotoId = photoId
                    };

                    // Load thumbnail image asynchronously
                    _ = LoadPhotoThumbnailAsync(photoItem);

                    Photos.Add(photoItem);
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
        ShowPhotos            = Photos.Count            > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task LoadPhotoThumbnailAsync(PhotoCarouselDisplayItem photoItem)
    {
        try
        {
            Stream stream = await _photoCache.GetThumbnailAsync(photoItem.PhotoId);

            var bitmap = new BitmapImage();

            using(IRandomAccessStream randomStream = stream.AsRandomAccessStream())
            {
                await bitmap.SetSourceAsync(randomStream);
            }

            photoItem.ThumbnailImageSource = bitmap;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading photo thumbnail {PhotoId}", photoItem.PhotoId);
        }
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

/// <summary>
///     Display item for photo carousel
/// </summary>
public class PhotoCarouselDisplayItem
{
    // Thumbnail constraints
    public const int          ThumbnailMaxSize = 256;
    public       Guid         PhotoId              { get; set; }
    public       ImageSource? ThumbnailImageSource { get; set; }
}