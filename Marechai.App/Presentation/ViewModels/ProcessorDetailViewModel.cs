#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml;
using Windows.System;

namespace Marechai.App.Presentation.ViewModels;

public partial class ProcessorDetailViewModel : ObservableObject, IRegionAware
{
    private readonly CompaniesService                  _companiesService;
    private readonly ImageSourceFactory                _imageSourceFactory;
    private readonly IStringLocalizer                  _localizer;
    private readonly ILogger<ProcessorDetailViewModel> _logger;
    private readonly ProcessorPhotoCache               _processorPhotoCache;
    private readonly IRegionManager                    _regionManager;
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
    private string _smartphonesFilterText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MachineItem> _smartphones = [];

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MachineItem> _filteredComputers = [];

    [ObservableProperty]
    private ObservableCollection<MachineItem> _filteredConsoles = [];

    [ObservableProperty]
    private ObservableCollection<MachineItem> _filteredSmartphones = [];

    [ObservableProperty]
    private bool _hasComputers;

    [ObservableProperty]
    private bool _hasConsoles;

    [ObservableProperty]
    private bool _hasSmartphones;

    [ObservableProperty]
    private string _descriptionMarkdown = string.Empty;

    [ObservableProperty]
    private Visibility _showDescription = Visibility.Collapsed;

    /// <summary>
    ///     Gets whether a description is available
    /// </summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(DescriptionMarkdown);

    partial void OnDescriptionMarkdownChanged(string value)
    {
        OnPropertyChanged(nameof(HasDescription));
    }

    [ObservableProperty]
    private bool _hasError;

    public ObservableCollection<PhotoCarouselDisplayItem> Photos { get; } = [];
    public ObservableCollection<MachineVideoDisplayItem>  Videos { get; } = [];

    [ObservableProperty]
    private Visibility _showPhotos = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showVideos = Visibility.Collapsed;

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

    public ProcessorDetailViewModel(ProcessorsService   processorsService, CompaniesService companiesService,
                                    IStringLocalizer    localizer,         ILogger<ProcessorDetailViewModel> logger,
                                    IRegionManager      regionManager,     ProcessorPhotoCache processorPhotoCache,
                                    ImageSourceFactory  imageSourceFactory)
    {
        _processorsService     = processorsService;
        _companiesService      = companiesService;
        _localizer             = localizer;
        _logger                = logger;
        _regionManager         = regionManager;
        _processorPhotoCache   = processorPhotoCache;
        _imageSourceFactory    = imageSourceFactory;
        LoadData               = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand          = new AsyncRelayCommand(GoBackAsync);
        SelectMachineCommand   = new AsyncRelayCommand<int>(SelectMachineAsync);
        SelectPhotoCommand     = new AsyncRelayCommand<Guid>(SelectPhotoAsync);
        OpenVideoCommand       = new AsyncRelayCommand<MachineVideoDisplayItem>(OpenVideoAsync);
        ComputersFilterCommand = new RelayCommand(() => FilterComputers());
        ConsolesFilterCommand  = new RelayCommand(() => FilterConsoles());
        SmartphonesFilterCommand = new RelayCommand(() => FilterSmartphones());
        Title = _localizer["Processor Details"];
    }

    public IAsyncRelayCommand LoadData               { get; }
    public ICommand           GoBackCommand          { get; }
    public IAsyncRelayCommand SelectMachineCommand   { get; }
    public IAsyncRelayCommand SelectPhotoCommand     { get; }
    public IAsyncRelayCommand OpenVideoCommand       { get; }
    public ICommand           ComputersFilterCommand { get; }
    public ICommand           ConsolesFilterCommand  { get; }
    public ICommand           SmartphonesFilterCommand { get; }

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
            Smartphones.Clear();
            FilteredComputers.Clear();
            FilteredConsoles.Clear();
            FilteredSmartphones.Clear();
            Photos.Clear();
            Videos.Clear();
            ShowPhotos = Visibility.Collapsed;
            ShowVideos = Visibility.Collapsed;

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
                    Smartphones.Clear();

                    foreach(MachineDto machine in machines)
                    {
                        var machineItem = new MachineItem
                        {
                            Id           = machine.Id               ?? 0,
                            Name         = machine.Name             ?? string.Empty,
                            Manufacturer = machine.Company          ?? string.Empty,
                            Year         = machine.Introduced?.Year ?? 0
                        };

                        if(machine.Type == 2) // MachineType.Console
                            Consoles.Add(machineItem);
                        else if(machine.Type == 3) // MachineType.Smartphone
                            Smartphones.Add(machineItem);
                        else
                            Computers.Add(machineItem);
                    }

                    HasComputers   = Computers.Count   > 0;
                    HasConsoles    = Consoles.Count    > 0;
                    HasSmartphones = Smartphones.Count > 0;

                    FilterComputers();
                    FilterConsoles();
                    FilterSmartphones();

                    _logger.LogInformation("Loaded {ComputerCount} computers and {ConsoleCount} consoles for Processor {ProcessorId}",
                                           Computers.Count,
                                           Consoles.Count,
                                           ProcessorId);
                }
                else
                {
                    HasComputers = false;
                    HasConsoles  = false;
                    HasSmartphones = false;
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load machines for Processor {ProcessorId}", ProcessorId);
                HasComputers = false;
                HasConsoles  = false;
                HasSmartphones = false;
            }

            // Load localized description
            try
            {
                string langCode = GetIso639CodeFromCulture();
                ProcessorDescriptionDto? desc = await _processorsService.GetDescriptionAsync(ProcessorId, langCode);

                DescriptionMarkdown = desc?.Markdown ?? string.Empty;
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load processor description: {Exception}", ex.Message);
            }

            ShowDescription = HasDescription ? Visibility.Visible : Visibility.Collapsed;

            // Load photos
            Photos.Clear();
            List<Guid> photoIds = await _processorsService.GetProcessorPhotosAsync(ProcessorId);

            if(photoIds.Count > 0)
            {
                foreach(Guid photoId in photoIds)
                {
                    var photoItem = new PhotoCarouselDisplayItem
                    {
                        PhotoId = photoId
                    };

                    _ = LoadPhotoThumbnailAsync(photoItem);
                    Photos.Add(photoItem);
                }
            }

            ShowPhotos = Photos.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            // Load videos
            List<ProcessorVideoDto> videoList = await _processorsService.GetVideosByProcessorAsync(ProcessorId);

            foreach(ProcessorVideoDto video in videoList)
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

            ShowVideos = Videos.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

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
    private Task GoBackAsync()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        _regionManager.RequestNavigate(RegionNames.Content, nameof(ProcessorListPage));

        return Task.CompletedTask;
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

    private void FilterSmartphones()
    {
        if(string.IsNullOrWhiteSpace(SmartphonesFilterText))
        {
            FilteredSmartphones.Clear();
            foreach(MachineItem smartphone in Smartphones) FilteredSmartphones.Add(smartphone);
        }
        else
        {
            var filtered = Smartphones.Where(c => c.Name.Contains(SmartphonesFilterText, StringComparison.OrdinalIgnoreCase))
                                      .ToList();

            FilteredSmartphones.Clear();
            foreach(MachineItem smartphone in filtered) FilteredSmartphones.Add(smartphone);
        }
    }

    private async Task LoadPhotoThumbnailAsync(PhotoCarouselDisplayItem photoItem)
    {
        try
        {
            Stream stream = await _processorPhotoCache.GetThumbnailAsync(photoItem.PhotoId);
            photoItem.ThumbnailImageSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading Processor photo thumbnail {PhotoId}", photoItem.PhotoId);
        }
    }

    /// <summary>
    ///     Navigates to machine detail view
    /// </summary>
    private Task SelectMachineAsync(int machineId)
    {
        if(machineId <= 0) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineId, machineId },
            { NavParamKeys.NavigationSource, nameof(ProcessorDetailViewModel) },
            { NavParamKeys.ProcessorId, ProcessorId }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to the photo detail view
    /// </summary>
    private Task SelectPhotoAsync(Guid photoId)
    {
        if(photoId == Guid.Empty) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.PhotoId, photoId },
            { NavParamKeys.NavigationSource, nameof(ProcessorDetailViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(ProcessorPhotoDetailPage), parameters);

        return Task.CompletedTask;
    }

    private async Task OpenVideoAsync(MachineVideoDisplayItem? video)
    {
        if(video?.LaunchUri is null) return;

        try
        {
            await Launcher.LaunchUriAsync(video.LaunchUri);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error launching video for Processor {ProcessorId}", ProcessorId);
        }
    }

    /// <summary>
    ///     Sets the navigation source (where we came from).
    /// </summary>
    public void SetNavigationSource(string? source)
    {
        _navigationSource = source;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.ProcessorId, out int processorId))
        {
            ProcessorId = processorId;

            if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.NavigationSource, out string source))
                SetNavigationSource(source);

            _ = LoadData.ExecuteAsync(null);
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
