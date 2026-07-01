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
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
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
    private readonly MachinePromoArtCache          _machinePromoArtCache;
    private readonly MachinePromoArtService        _machinePromoArtService;
    private readonly IRegionManager                _regionManager;
    private readonly MachinePhotoCache             _photoCache;
    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string _descriptionMarkdown = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private int? _familyId;

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
    private int     _currentMachineId;
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
    private Visibility _showPromoArt = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showProcessors = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSoundSynthesizers = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSoftware = Visibility.Collapsed;

    [ObservableProperty]
    private int _softwareCurrentPage = 1;

    [ObservableProperty]
    private int _softwareTotalCount;

    private const int SoftwarePageSize = 20;

    [ObservableProperty]
    private Visibility _showStorage = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showDescription = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showVideos = Visibility.Collapsed;

    /// <summary>
    ///     Gets whether a description is available
    /// </summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(DescriptionMarkdown);

    partial void OnDescriptionMarkdownChanged(string value)
    {
        OnPropertyChanged(nameof(HasDescription));
    }

    public bool CanGoToPreviousSoftwarePage => SoftwareCurrentPage > 1;
    public bool CanGoToNextSoftwarePage => SoftwareCurrentPage * SoftwarePageSize < SoftwareTotalCount;

    public string SoftwarePageSummary => SoftwareTotalCount == 0
                                              ? _localizer["MachineSoftwarePaginationEmpty"]
                                              : $"{((SoftwareCurrentPage - 1) * SoftwarePageSize) + 1}-" +
                                                $"{Math.Min(SoftwareCurrentPage * SoftwarePageSize, SoftwareTotalCount)} of {SoftwareTotalCount}";

    partial void OnSoftwareCurrentPageChanged(int value)
    {
        OnPropertyChanged(nameof(CanGoToPreviousSoftwarePage));
        OnPropertyChanged(nameof(CanGoToNextSoftwarePage));
        OnPropertyChanged(nameof(SoftwarePageSummary));
    }

    partial void OnSoftwareTotalCountChanged(int value)
    {
        OnPropertyChanged(nameof(CanGoToPreviousSoftwarePage));
        OnPropertyChanged(nameof(CanGoToNextSoftwarePage));
        OnPropertyChanged(nameof(SoftwarePageSummary));
    }

    public MachineViewViewModel(ILogger<MachineViewViewModel> logger,             IRegionManager     regionManager,
                                ComputersService              computersService,   MachinePhotoCache  photoCache,
                                IStringLocalizer              localizer,          ImageSourceFactory imageSourceFactory,
                                IConfiguration                configuration,      MachinePromoArtService machinePromoArtService,
                                MachinePromoArtCache          machinePromoArtCache)
    {
        _logger             = logger;
        _regionManager      = regionManager;
        _computersService   = computersService;
        _photoCache         = photoCache;
        _localizer          = localizer;
        _imageSourceFactory = imageSourceFactory;
        _configuration      = configuration;
        _machinePromoArtService = machinePromoArtService;
        _machinePromoArtCache   = machinePromoArtCache;
    }

    public ObservableCollection<ProcessorDisplayItem>        Processors        { get; } = [];
    public ObservableCollection<MemoryDisplayItem>           Memory            { get; } = [];
    public ObservableCollection<GpuDisplayItem>              Gpus              { get; } = [];
    public ObservableCollection<SoundSynthesizerDisplayItem> SoundSynthesizers { get; } = [];
    public ObservableCollection<StorageDisplayItem>          Storage           { get; } = [];
    public ObservableCollection<SoftwareListItem>            Software          { get; } = [];
    public ObservableCollection<MachineVideoDisplayItem>     Videos            { get; } = [];
    public ObservableCollection<PhotoCarouselDisplayItem>    Photos            { get; } = [];
    public ObservableCollection<MachinePromoArtGroupDisplayItem> PromoArtGroups { get; } = [];

    public int PromoArtCount => PromoArtGroups.Sum(group => group.Items.Count);
    public bool HasPromoArt => PromoArtCount > 0;

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

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
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

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
            case nameof(ProcessorDetailViewModel):
            case nameof(SoundSynthDetailViewModel):
            {
                // When a machine is opened from a component detail page, use the journal
                // so Back returns to the prior page instead of rebuilding a navigation loop.
                _regionManager.Regions[RegionNames.Content].NavigationService.Journal.GoBack();

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
    public Task ViewPromoArtDetails(Guid promoArtId)
    {
        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachinePromoArtId, promoArtId }
        };

        _logger.LogInformation("Navigating to promo art details for {PromoArtId}", promoArtId);
        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachinePromoArtDetailPage), parameters);

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

    [RelayCommand]
    public Task NavigateToFamily()
    {
        if(!FamilyId.HasValue) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineFamilyId, FamilyId.Value },
            { NavParamKeys.MachineId, _currentMachineId },
            { NavParamKeys.NavigationSource, nameof(MachineViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineFamilyViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task PreviousSoftwarePage()
    {
        if(!CanGoToPreviousSoftwarePage) return;

        SoftwareCurrentPage--;
        await LoadSoftwarePageAsync(_currentMachineId);
    }

    [RelayCommand]
    public async Task NextSoftwarePage()
    {
        if(!CanGoToNextSoftwarePage) return;

        SoftwareCurrentPage++;
        await LoadSoftwarePageAsync(_currentMachineId);
    }

    [RelayCommand]
    public Task NavigateToProcessor(ProcessorDisplayItem? processor)
    {
        if(processor is null || processor.Id <= 0) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.ProcessorId, processor.Id },
            { NavParamKeys.MachineId, _currentMachineId },
            { NavParamKeys.NavigationSource, nameof(MachineViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(ProcessorDetailPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToGpu(GpuDisplayItem? gpu)
    {
        if(gpu is null || gpu.Id <= 0) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.GpuId, gpu.Id },
            { NavParamKeys.MachineId, _currentMachineId },
            { NavParamKeys.NavigationSource, nameof(MachineViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(GpuDetailPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToSoundSynth(SoundSynthesizerDisplayItem? soundSynth)
    {
        if(soundSynth is null || soundSynth.Id <= 0) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoundSynthId, soundSynth.Id },
            { NavParamKeys.MachineId, _currentMachineId },
            { NavParamKeys.NavigationSource, nameof(MachineViewViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoundSynthDetailPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task OpenVideo(MachineVideoDisplayItem? video)
    {
        if(video?.LaunchUri is null) return;

        try
        {
            await Launcher.LaunchUriAsync(video.LaunchUri);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error launching video for machine {MachineId}", _currentMachineId);
        }
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
            Videos.Clear();
            Photos.Clear();
            PromoArtGroups.Clear();

            _logger.LogInformation("Loading machine {MachineId}", machineId);
            _currentMachineId = machineId;

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
            FamilyId    = machine.FamilyId;
            FamilyName  = machine.FamilyName;
            ModelName   = machine.Model;

            // Check if this is a prototype
            IsPrototype = machine.Prototype == true;

            // Set introduction date if available and not a prototype
            if(machine.Introduced.HasValue && !IsPrototype)
                IntroductionDateDisplay = DatePrecisionFormatter.Format(machine.Introduced, machine.IntroducedPrecision);

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
                        Id           = processor.Id ?? 0,
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
                        Id              = gpu.Id ?? 0,
                        DisplayName     = LocalizeSpecialGpuName(gpu.Name),
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
                        Id          = synth.Id ?? 0,
                        DisplayName = LocalizeSpecialSoundSynthName(synth.Name),
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
            SoftwareCurrentPage = 1;
            await LoadSoftwarePageAsync(machineId);

            // Populate videos
            List<MachineVideoDto> videoList = await _computersService.GetVideosByMachineAsync(machineId);

            foreach(MachineVideoDto video in videoList)
            {
                if(string.IsNullOrWhiteSpace(video.VideoId)) continue;

                string provider = string.IsNullOrWhiteSpace(video.Provider)
                                      ? _localizer["Video"]
                                      : video.Provider;

                bool isYouTube = string.Equals(provider, "YouTube", StringComparison.OrdinalIgnoreCase);

                Videos.Add(new MachineVideoDisplayItem
                {
                    Title = string.IsNullOrWhiteSpace(video.Title)
                                ? _localizer["Video"]
                                : video.Title,
                    Provider = provider,
                    VideoId = video.VideoId,
                    ThumbnailUrl = isYouTube
                                       ? $"https://img.youtube.com/vi/{video.VideoId}/hqdefault.jpg"
                                       : null,
                    LaunchUri = new Uri($"https://www.youtube.com/watch?v={Uri.EscapeDataString(video.VideoId)}")
                });
            }

            // Populate promo art
            List<MachinePromoArtDto> promoArtList = await _machinePromoArtService.GetPromoArtByMachineAsync(machineId);
            string                   otherGroup   = _localizer["OtherPromoArt"];

            foreach(var group in promoArtList.GroupBy(item => string.IsNullOrWhiteSpace(item.GroupName)
                                                                  ? otherGroup
                                                                  : item.GroupName!)
                                              .OrderBy(group => group.Key, StringComparer.CurrentCultureIgnoreCase))
            {
                var displayGroup = new MachinePromoArtGroupDisplayItem
                {
                    GroupName = group.Key
                };

                foreach(MachinePromoArtDto promoArt in group)
                {
                    if(!promoArt.Id.HasValue) continue;

                    var promoArtItem = new MachinePromoArtDisplayItem
                    {
                        PromoArtId = promoArt.Id.Value,
                        GroupName  = group.Key,
                        Caption    = promoArt.Caption ?? string.Empty
                    };

                    _ = LoadPromoArtThumbnailAsync(promoArtItem);

                    displayGroup.Items.Add(promoArtItem);
                }

                if(displayGroup.Items.Count > 0)
                    PromoArtGroups.Add(displayGroup);
            }

            // Load localized description
            try
            {
                string langCode = GetIso639CodeFromCulture();
                MachineDescriptionDto? desc = await _computersService.GetDescriptionAsync(machineId, langCode);

                DescriptionMarkdown = desc?.Markdown ?? string.Empty;
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

    private async Task LoadSoftwarePageAsync(int machineId)
    {
        Software.Clear();

        SoftwareTotalCount = await _computersService.GetSoftwareByMachineCountAsync(machineId);

        int skip = (SoftwareCurrentPage - 1) * SoftwarePageSize;

        List<SoftwareDto> softwareList =
            await _computersService.GetSoftwareByMachineAsync(machineId, skip, SoftwarePageSize);

        string baseUrl = _configuration.GetSection("ApiClient:Url").Value;

        foreach(SoftwareDto sw in softwareList)
        {
            int id = (int)(sw.Id ?? 0);

            if(id == 0) continue;

            Software.Add(new SoftwareListItem
            {
                Id            = id,
                Name          = sw.Name ?? string.Empty,
                Family        = sw.Family,
                Kind          = (SoftwareKind)(sw.Kind ?? 0),
                FrontCoverId  = sw.FrontCoverId,
                CoverImageUrl = sw.FrontCoverId.HasValue
                                    ? $"{baseUrl}/assets/photos/software-covers/webp/4k/{sw.FrontCoverId}.webp"
                                    : null
            });
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
        ShowVideos            = Videos.Count            > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowPhotos            = Photos.Count            > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowPromoArt          = PromoArtGroups.Count    > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowDescription       = HasDescription ? Visibility.Visible : Visibility.Collapsed;
        OnPropertyChanged(nameof(PromoArtCount));
        OnPropertyChanged(nameof(HasPromoArt));
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

    async Task LoadPromoArtThumbnailAsync(MachinePromoArtDisplayItem promoArtItem)
    {
        try
        {
            Stream stream = await _machinePromoArtCache.GetThumbnailAsync(promoArtItem.PromoArtId);

            promoArtItem.ThumbnailImageSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading promo art thumbnail {PromoArtId}", promoArtItem.PromoArtId);
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

    string LocalizeSpecialGpuName(string? name) => name switch
    {
        "DB_FRAMEBUFFER" => _localizer["Framebuffer"],
        "DB_SOFTWARE"    => _localizer["Software"],
        "DB_NONE"        => _localizer["None_female"],
        _                => name ?? string.Empty
    };

    string LocalizeSpecialSoundSynthName(string? name) => name switch
    {
        "DB_SOFTWARE" => _localizer["Software"],
        _             => name ?? string.Empty
    };
}
