#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.App.Services.Caching;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminMachinePhotosViewModel : ObservableObject, IRegionAware
{
    readonly MachinePhotosService                        _photosService;
    readonly LicensesService                             _licensesService;
    readonly MachinePhotoCache                           _photoCache;
    readonly ImageSourceFactory                          _imageSourceFactory;
    readonly IJwtService                                 _jwtService;
    readonly ITokenService                               _tokenService;
    readonly IStringLocalizer                             _localizer;
    readonly ILogger<AdminMachinePhotosViewModel>         _logger;
    readonly IRegionManager                              _regionManager;

    int _machineId;

    [ObservableProperty]
    private string _machineName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MachinePhotoDisplayItem> _photos = [];

    [ObservableProperty]
    private ObservableCollection<LicenseDto> _licenses = [];

    [ObservableProperty]
    private LicenseDto? _selectedLicense;

    [ObservableProperty]
    private string _sourceUrl = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _isUploading;

    public AdminMachinePhotosViewModel(MachinePhotosService                        photosService,
                                       LicensesService                             licensesService,
                                       MachinePhotoCache                           photoCache,
                                       ImageSourceFactory                          imageSourceFactory,
                                       IJwtService                                 jwtService,
                                       ITokenService                               tokenService,
                                       IStringLocalizer                             localizer,
                                       ILogger<AdminMachinePhotosViewModel>         logger,
                                       IRegionManager                              regionManager)
    {
        _photosService      = photosService;
        _licensesService    = licensesService;
        _photoCache         = photoCache;
        _imageSourceFactory = imageSourceFactory;
        _jwtService         = jwtService;
        _tokenService       = tokenService;
        _localizer          = localizer;
        _logger             = logger;
        _regionManager      = regionManager;

        LoadPhotosCommand  = new AsyncRelayCommand(LoadPhotosAsync);
        UploadPhotoCommand = new AsyncRelayCommand(UploadPhotoAsync);
        DeletePhotoCommand = new AsyncRelayCommand<MachinePhotoDisplayItem>(DeletePhotoAsync);
        ViewPhotoCommand   = new RelayCommand<MachinePhotoDisplayItem>(ViewPhoto);
        GoBackCommand      = new RelayCommand(GoBack);

        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand                              LoadPhotosCommand  { get; }
    public IAsyncRelayCommand                              UploadPhotoCommand { get; }
    public IAsyncRelayCommand<MachinePhotoDisplayItem>     DeletePhotoCommand { get; }
    public IRelayCommand<MachinePhotoDisplayItem>          ViewPhotoCommand   { get; }
    public IRelayCommand                                   GoBackCommand      { get; }

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.MachineId, out int machineId))
            _machineId = machineId;

        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.MachineName, out string? name))
            MachineName = name ?? string.Empty;

        if(IsAdmin)
        {
            _ = LoadLicensesAsync();
            _ = LoadPhotosCommand.ExecuteAsync(null);
        }
    }

    // --- Role check ---
    void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(string.IsNullOrWhiteSpace(token))
            {
                IsAdmin = false;

                return;
            }

            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("Uberadmin",      StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    // --- Load licenses ---
    async Task LoadLicensesAsync()
    {
        try
        {
            List<LicenseDto> licenseList = await _licensesService.GetLicensesAsync();

            Licenses.Clear();

            foreach(LicenseDto license in licenseList.OrderBy(l => l.Name))
                Licenses.Add(license);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading licenses");
        }
    }

    // --- Load photos ---
    async Task LoadPhotosAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Photos.Clear();

            List<Guid> photoIds = await _photosService.GetPhotoIdsAsync(_machineId);

            foreach(Guid photoId in photoIds)
            {
                var item = new MachinePhotoDisplayItem
                {
                    PhotoId = photoId
                };

                // Load photo details for display metadata
                MachinePhotoDto? details = await _photosService.GetPhotoDetailsAsync(photoId);

                if(details != null)
                {
                    item.LicenseName = details.LicenseName;

                    item.CameraInfo = !string.IsNullOrEmpty(details.CameraManufacturer) ||
                                      !string.IsNullOrEmpty(details.CameraModel)
                                          ? $"{details.CameraManufacturer} {details.CameraModel}".Trim()
                                          : null;

                    item.UploadDate = details.UploadDate?.ToString("d");
                }

                Photos.Add(item);

                // Fire-and-forget thumbnail loading
                _ = LoadThumbnailAsync(item);
            }

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading photos for machine {MachineId}", _machineId);
            ErrorMessage = _localizer["FailedToLoadPhotos"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task LoadThumbnailAsync(MachinePhotoDisplayItem item)
    {
        try
        {
            Stream stream = await _photoCache.GetThumbnailAsync(item.PhotoId);
            item.ThumbnailImageSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading photo thumbnail {PhotoId}", item.PhotoId);
        }
    }

    // --- Upload photo ---
    async Task UploadPhotoAsync()
    {
        if(SelectedLicense == null)
        {
            ErrorMessage = _localizer["SelectLicenseRequired"];
            HasError     = true;

            return;
        }

        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".webp");
            picker.FileTypeFilter.Add(".tiff");
            picker.FileTypeFilter.Add(".tif");
            picker.FileTypeFilter.Add(".bmp");

#if !HAS_UNO
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
#endif

            Windows.Storage.StorageFile? file = await picker.PickSingleFileAsync();

            if(file == null) return;

            IsUploading = true;
            HasError    = false;

            using Stream stream = await file.OpenStreamForReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            string? source = string.IsNullOrWhiteSpace(SourceUrl) ? null : SourceUrl;

            MachinePhotoDto? result = await _photosService.UploadPhotoAsync(_machineId,
                                                                            SelectedLicense.Id ?? 0,
                                                                            source,
                                                                            fileBytes,
                                                                            file.Name);

            if(result == null)
            {
                ErrorMessage = _localizer["FailedToUploadPhoto"];
                HasError     = true;

                return;
            }

            // Clear form
            SourceUrl = string.Empty;

            // Reload photos to show the new one
            await LoadPhotosAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo");
            ErrorMessage = _localizer["FailedToUploadPhoto"];
            HasError     = true;
        }
        finally
        {
            IsUploading = false;
        }
    }

    // --- Delete photo ---
    async Task DeletePhotoAsync(MachinePhotoDisplayItem? item)
    {
        if(item == null) return;

        try
        {
            HasError     = false;
            ErrorMessage = string.Empty;

            bool success = await _photosService.DeletePhotoAsync(item.PhotoId);

            if(!success)
            {
                ErrorMessage = _localizer["FailedToDeletePhoto"];
                HasError     = true;

                return;
            }

            // Reload
            await LoadPhotosAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo {PhotoId}", item.PhotoId);
            ErrorMessage = _localizer["FailedToDeletePhoto"];
            HasError     = true;
        }
    }

    // --- View photo detail ---
    void ViewPhoto(MachinePhotoDisplayItem? item)
    {
        if(item == null) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.PhotoId, item.PhotoId }
        };

        _regionManager.RequestNavigate(RegionNames.Content, "PhotoDetailPage", parameters);
    }

    // --- Navigation ---
    void GoBack() => _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminMachinesPage));
}
