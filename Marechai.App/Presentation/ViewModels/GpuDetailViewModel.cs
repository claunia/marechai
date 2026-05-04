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

namespace Marechai.App.Presentation.ViewModels;

public partial class GpuDetailViewModel : ObservableObject, IRegionAware
{
    private readonly CompaniesService            _companiesService;
    private readonly GpuPhotoCache               _gpuPhotoCache;
    private readonly GpusService                 _gpusService;
    private readonly ImageSourceFactory          _imageSourceFactory;
    private readonly IStringLocalizer            _localizer;
    private readonly ILogger<GpuDetailViewModel> _logger;
    private readonly IRegionManager              _regionManager;

    [ObservableProperty]
    private ObservableCollection<MachineItem> _computers = [];

    [ObservableProperty]
    private string _computersFilterText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MachineItem> _consoles = [];

    [ObservableProperty]
    private string _consolesFilterText = string.Empty;

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
    private GpuDto? _gpu;

    [ObservableProperty]
    private int _gpuId;

    [ObservableProperty]
    private bool _hasComputers;

    [ObservableProperty]
    private bool _hasConsoles;

    [ObservableProperty]
    private bool _hasSmartphones;

    [ObservableProperty]
    private string _descriptionHtml = string.Empty;

    [ObservableProperty]
    private Visibility _showDescription = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPhotos = Visibility.Collapsed;

    public ObservableCollection<PhotoCarouselDisplayItem> Photos { get; } = [];

    /// <summary>
    ///     Gets whether a description is available
    /// </summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(DescriptionHtml);

    partial void OnDescriptionHtmlChanged(string value)
    {
        OnPropertyChanged(nameof(HasDescription));
    }

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
                              ILogger<GpuDetailViewModel> logger, IRegionManager regionManager,
                              GpuPhotoCache gpuPhotoCache, ImageSourceFactory imageSourceFactory)
    {
        _gpusService           = gpusService;
        _companiesService      = companiesService;
        _localizer             = localizer;
        _logger                = logger;
        _regionManager         = regionManager;
        _gpuPhotoCache         = gpuPhotoCache;
        _imageSourceFactory    = imageSourceFactory;
        LoadData               = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand          = new AsyncRelayCommand(GoBackAsync);
        SelectMachineCommand   = new AsyncRelayCommand<int>(SelectMachineAsync);
        ComputersFilterCommand = new RelayCommand(() => FilterComputers());
        ConsolesFilterCommand  = new RelayCommand(() => FilterConsoles());
        SmartphonesFilterCommand = new RelayCommand(() => FilterSmartphones());
        Title                  = _localizer["GPU Details"];
    }

    public IAsyncRelayCommand LoadData               { get; }
    public ICommand           GoBackCommand          { get; }
    public IAsyncRelayCommand SelectMachineCommand   { get; }
    public ICommand           ComputersFilterCommand { get; }
    public ICommand           ConsolesFilterCommand  { get; }
    public ICommand           SmartphonesFilterCommand { get; }

    public string Title { get; }

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
                displayName = _localizer["Framebuffer"];
            else if(displayName == "DB_SOFTWARE")
                displayName                               = _localizer["Software"];
            else if(displayName == "DB_NONE") displayName = _localizer["None_female"];

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
                    Smartphones.Clear();                    foreach(MachineDto machine in machines)
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
                        else if(machine.Type == 3) // MachineType.Smartphone
                            Smartphones.Add(machineItem);
                        else // MachineType.Computer or Unknown
                            Computers.Add(machineItem);
                    }

                    HasComputers   = Computers.Count   > 0;
                    HasConsoles    = Consoles.Count    > 0;
                    HasSmartphones = Smartphones.Count > 0;

                    // Initialize filtered collections
                    FilterComputers();
                    FilterConsoles();
                    FilterSmartphones();
                }
                else
                {
                    HasComputers   = false;
                    HasConsoles    = false;
                    HasSmartphones = false;
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load machines for GPU {GpuId}", GpuId);
                HasComputers   = false;
                HasConsoles    = false;
                HasSmartphones = false;
            }

            // Load localized description
            try
            {
                string langCode = GetIso639CodeFromCulture();
                GpuDescriptionDto? desc = await _gpusService.GetDescriptionAsync(GpuId, langCode);

                DescriptionHtml = desc?.Html ?? desc?.Markdown ?? string.Empty;
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load GPU description: {Exception}", ex.Message);
            }

            ShowDescription = HasDescription ? Visibility.Visible : Visibility.Collapsed;

            // Load photos
            Photos.Clear();
            List<Guid> photoIds = await _gpusService.GetGpuPhotosAsync(GpuId);

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
            Stream stream = await _gpuPhotoCache.GetThumbnailAsync(photoItem.PhotoId);

            photoItem.ThumbnailImageSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading GPU photo thumbnail {PhotoId}", photoItem.PhotoId);
        }
    }

    /// <summary>
    ///     Navigates back to the GPU list
    /// </summary>
    private Task GoBackAsync()
    {
        if(_navigationSource == nameof(MachineViewViewModel))
            _regionManager.Regions[RegionNames.Content].NavigationService.Journal.GoBack();
        else
            _regionManager.RequestNavigate(RegionNames.Content, nameof(GpuListPage));

        return Task.CompletedTask;
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
            { NavParamKeys.NavigationSource, nameof(GpuDetailViewModel) },
            { NavParamKeys.GpuId, GpuId }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Sets the navigation source (where we came from).
    /// </summary>
    public void SetNavigationSource(string? source)
    {
        _navigationSource = source;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.GpuId, out int gpuId))
        {
            GpuId = gpuId;

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