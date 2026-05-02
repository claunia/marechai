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
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Humanizer;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Marechai.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Marechai.App.Presentation.ViewModels;

public partial class MachineViewViewModel : ObservableObject, IRegionAware
{
    private readonly ComputersService              _computersService;
    private readonly IConfiguration                _configuration;
    private readonly ImageSourceFactory            _imageSourceFactory;
    private readonly IStringLocalizer              _localizer;
    private readonly ILogger<MachineViewViewModel> _logger;
    private readonly IRegionManager                _regionManager;
    private readonly MachinePhotoCache             _photoCache;
    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string _descriptionHtml = string.Empty;

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
    private string? _navigationSource;
    private int     _sourceCompanyId;
    private int     _sourceGpuId;
    private int     _sourceProcessorId;
    private int     _sourceSoundSynthId;

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
    private Visibility _showSoftware = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showStorage = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showDescription = Visibility.Collapsed;

    /// <summary>
    ///     Gets whether a description is available
    /// </summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(DescriptionHtml);

    partial void OnDescriptionHtmlChanged(string value)
    {
        OnPropertyChanged(nameof(HasDescription));
    }

    public MachineViewViewModel(ILogger<MachineViewViewModel> logger,             IRegionManager     regionManager,
                                ComputersService              computersService,   MachinePhotoCache  photoCache,
                                IStringLocalizer              localizer,          ImageSourceFactory imageSourceFactory,
                                IConfiguration                configuration)
    {
        _logger             = logger;
        _regionManager      = regionManager;
        _computersService   = computersService;
        _photoCache         = photoCache;
        _localizer          = localizer;
        _imageSourceFactory = imageSourceFactory;
        _configuration      = configuration;
    }

    public ObservableCollection<ProcessorDisplayItem>        Processors        { get; } = [];
    public ObservableCollection<MemoryDisplayItem>           Memory            { get; } = [];
    public ObservableCollection<GpuDisplayItem>              Gpus              { get; } = [];
    public ObservableCollection<SoundSynthesizerDisplayItem> SoundSynthesizers { get; } = [];
    public ObservableCollection<StorageDisplayItem>          Storage           { get; } = [];
    public ObservableCollection<SoftwareListItem>            Software          { get; } = [];
    public ObservableCollection<PhotoCarouselDisplayItem>    Photos            { get; } = [];

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        SetNavigationSource(navigationContext.Parameters);

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.MachineId, out int machineId))
            _ = LoadMachineAsync(machineId);
    }

    [RelayCommand]
    public Task GoBack()
    {
        switch(_navigationSource)
        {
            case nameof(NewsViewModel):
                _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));

                break;

            case nameof(CompanyDetailViewModel):
            {
                var parameters = new NavigationParameters
                {
                    { NavParamKeys.CompanyId, _sourceCompanyId }
                };

                _regionManager.RequestNavigate(RegionNames.Content, nameof(CompanyDetailPage), parameters);

                break;
            }

            case nameof(ConsolesListViewModel):
                _regionManager.RequestNavigate(RegionNames.Content, nameof(ConsolesListPage));

                break;

            case nameof(SmartphonesListViewModel):
                _regionManager.RequestNavigate(RegionNames.Content, nameof(SmartphonesListPage));

                break;

            case nameof(ComputersListViewModel):
                _regionManager.RequestNavigate(RegionNames.Content, nameof(ComputersListPage));

                break;

            case nameof(GpuDetailViewModel):
            {
                var parameters = new NavigationParameters
                {
                    { NavParamKeys.GpuId, _sourceGpuId },
                    { NavParamKeys.NavigationSource, nameof(MachineViewViewModel) }
                };

                _regionManager.RequestNavigate(RegionNames.Content, nameof(GpuDetailPage), parameters);

                break;
            }

            case nameof(ProcessorDetailViewModel):
            {
                var parameters = new NavigationParameters
                {
                    { NavParamKeys.ProcessorId, _sourceProcessorId },
                    { NavParamKeys.NavigationSource, nameof(MachineViewViewModel) }
                };

                _regionManager.RequestNavigate(RegionNames.Content, nameof(ProcessorDetailPage), parameters);

                break;
            }

            case nameof(SoundSynthDetailViewModel):
            {
                var parameters = new NavigationParameters
                {
                    { NavParamKeys.SoundSynthId, _sourceSoundSynthId },
                    { NavParamKeys.NavigationSource, nameof(MachineViewViewModel) }
                };

                _regionManager.RequestNavigate(RegionNames.Content, nameof(SoundSynthDetailPage), parameters);

                break;
            }

            default:
                _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));

                break;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task ViewPhotoDetails(Guid photoId)
    {
        var parameters = new NavigationParameters
        {
            { NavParamKeys.PhotoId, photoId }
        };

        _logger.LogInformation("Navigating to photo details for {PhotoId}", photoId);
        _regionManager.RequestNavigate(RegionNames.Content, nameof(PhotoDetailPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToSoftware(SoftwareListItem? sw)
    {
        if(sw is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, sw.Id },
            { NavParamKeys.NavigationSource, nameof(MachineViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Sets the navigation source context from navigation parameters.
    /// </summary>
    public void SetNavigationSource(INavigationParameters parameters)
    {
        if(parameters.TryGetValue<string>(NavParamKeys.NavigationSource, out string source))
            _navigationSource = source;

        if(parameters.TryGetValue<int>(NavParamKeys.CompanyId, out int companyId))
            _sourceCompanyId = companyId;

        if(parameters.TryGetValue<int>(NavParamKeys.GpuId, out int gpuId))
            _sourceGpuId = gpuId;

        if(parameters.TryGetValue<int>(NavParamKeys.ProcessorId, out int processorId))
            _sourceProcessorId = processorId;

        if(parameters.TryGetValue<int>(NavParamKeys.SoundSynthId, out int soundSynthId))
            _sourceSoundSynthId = soundSynthId;
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
            Software.Clear();
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
                IntroductionDateDisplay = (machine.IntroducedPrecision ?? 0) == 2 ? $"{machine.Introduced.Value.Year}" : (machine.IntroducedPrecision ?? 0) == 1 ? machine.Introduced.Value.ToString("MMMM yyyy") : machine.Introduced.Value.DateTime.ToString("MMMM d, yyyy");

            // Populate processors
            if(machine.Processors != null)
            {
                foreach(ProcessorDto processor in machine.Processors)
                {
                    var details = new List<string>();
                    var speed   = (int)(processor.Speed ?? 0);
                    int gprSize = processor.GprSize ?? 0;
                    int cores   = processor.Cores   ?? 0;

                    if(speed   > 0) details.Add(string.Format(_localizer["_0_MHz"],   speed));
                    if(gprSize > 0) details.Add(string.Format(_localizer["_0_bits"],  gprSize));
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
                                         ? size > 1024
                                               ? string.Format(_localizer["_0_bytes_1_"], size, size.Bytes().Humanize())
                                               : string.Format(_localizer["_0_bytes"],    size)
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
                                             ? capacity > 1024
                                                   ? string.Format(_localizer["_0_bytes_1_"],
                                                                   capacity,
                                                                   capacity.Bytes().Humanize())
                                                   : string.Format(_localizer["_0_bytes"], capacity)
                                             : _localizer["Storage"];

                    // Get humanized storage type description
                    string typeNote = storage.Type.HasValue
                                          ? ((StorageType)storage.Type.Value).Humanize()
                                          : _localizer["Unknown_male"];

                    Storage.Add(new StorageDisplayItem
                    {
                        DisplayText = displayText,
                        TypeNote    = typeNote
                    });
                }
            }

            // Populate software
            List<SoftwareDto> softwareList = await _computersService.GetSoftwareByMachineAsync(machineId);
            string            baseUrl      = _configuration.GetSection("ApiClient:Url").Value;

            foreach(SoftwareDto sw in softwareList)
            {
                int id = (int)(sw.Id ?? 0);

                if(id == 0) continue;

                Software.Add(new SoftwareListItem
                {
                    Id                = id,
                    Name              = sw.Name ?? string.Empty,
                    Family            = sw.Family,
                    IsOperatingSystem = sw.IsOperatingSystem ?? false,
                    IsGame            = sw.IsGame ?? false,
                    FrontCoverId      = sw.FrontCoverId,
                    CoverImageUrl     = sw.FrontCoverId.HasValue
                                            ? $"{baseUrl}/assets/photos/software-covers/webp/hd/{sw.FrontCoverId}.webp"
                                            : null
                });
            }

            // Load localized description
            try
            {
                string langCode = GetIso639CodeFromCulture();
                MachineDescriptionDto? desc = await _computersService.GetDescriptionAsync(machineId, langCode);

                DescriptionHtml = desc?.Html ?? desc?.Markdown ?? string.Empty;
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load machine description: {Exception}", ex.Message);
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
        ShowSoftware          = Software.Count          > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowPhotos            = Photos.Count            > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowDescription       = HasDescription ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task LoadPhotoThumbnailAsync(PhotoCarouselDisplayItem photoItem)
    {
        try
        {
            Stream stream = await _photoCache.GetThumbnailAsync(photoItem.PhotoId);

            photoItem.ThumbnailImageSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading photo thumbnail {PhotoId}", photoItem.PhotoId);
        }
    }

    private static string GetIso639CodeFromCulture()
    {
        string twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        return twoLetter switch
        {
            "en" => "eng",
            "es" => "spa",
            "de" => "deu",
            "fr" => "fra",
            "la" => "lat",
            "pt" => "por",
            _    => "eng"
        };
    }
}