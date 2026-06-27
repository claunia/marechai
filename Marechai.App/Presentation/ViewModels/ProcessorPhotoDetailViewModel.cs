#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using Humanizer;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml.Media.Imaging;
using ColorSpace = Marechai.Data.ColorSpace;
using Contrast = Marechai.Data.Contrast;
using ExposureMode = Marechai.Data.ExposureMode;
using ExposureProgram = Marechai.Data.ExposureProgram;
using Flash = Marechai.Data.Flash;
using LightSource = Marechai.Data.LightSource;
using MeteringMode = Marechai.Data.MeteringMode;
using Orientation = Marechai.Data.Orientation;
using ResolutionUnit = Marechai.Data.ResolutionUnit;
using Saturation = Marechai.Data.Saturation;
using SceneCaptureType = Marechai.Data.SceneCaptureType;
using SensingMethod = Marechai.Data.SensingMethod;
using Sharpness = Marechai.Data.Sharpness;
using SubjectDistanceRange = Marechai.Data.SubjectDistanceRange;
using WhiteBalance = Marechai.Data.WhiteBalance;

namespace Marechai.App.Presentation.ViewModels;

public partial class ProcessorPhotoDetailViewModel : ObservableObject, IRegionAware
{
    private readonly ProcessorsService                    _processorsService;
    private readonly ImageSourceFactory                    _imageSourceFactory;
    private readonly IStringLocalizer                      _localizer;
    private readonly ILogger<ProcessorPhotoDetailViewModel> _logger;
    private readonly IRegionManager                        _regionManager;
    private readonly ProcessorPhotoCache                   _photoCache;

    private int _processorId;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _errorOccurred;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isPortrait = true;

    // EXIF Camera Settings
    [ObservableProperty]
    private string _photoAperture = string.Empty;

    [ObservableProperty]
    private string _photoAuthor = string.Empty;

    [ObservableProperty]
    private string _photoCameraManufacturer = string.Empty;

    [ObservableProperty]
    private string _photoCameraModel = string.Empty;

    // Photo Properties
    [ObservableProperty]
    private string _photoColorSpace = string.Empty;

    [ObservableProperty]
    private string _photoComments = string.Empty;

    [ObservableProperty]
    private string _photoContrast = string.Empty;

    [ObservableProperty]
    private string _photoCreationDate = string.Empty;

    [ObservableProperty]
    private string _photoDigitalZoomRatio = string.Empty;

    [ObservableProperty]
    private string _photoExifVersion = string.Empty;

    [ObservableProperty]
    private string _photoExposureMode = string.Empty;

    [ObservableProperty]
    private string _photoExposureProgram = string.Empty;

    [ObservableProperty]
    private string _photoExposureTime = string.Empty;

    [ObservableProperty]
    private string _photoFlash = string.Empty;

    [ObservableProperty]
    private string _photoFocalLength = string.Empty;

    [ObservableProperty]
    private string _photoFocalLengthEquivalent = string.Empty;

    // Resolution and Other
    [ObservableProperty]
    private string _photoHorizontalResolution = string.Empty;

    [ObservableProperty]
    private BitmapImage? _photoImageSource;

    [ObservableProperty]
    private string _photoIsoRating = string.Empty;

    [ObservableProperty]
    private string _photoLensModel = string.Empty;

    [ObservableProperty]
    private string _photoLicenseName = string.Empty;

    [ObservableProperty]
    private string _photoLightSource = string.Empty;

    [ObservableProperty]
    private string _photoProcessorCompanyName = string.Empty;

    [ObservableProperty]
    private string _photoProcessorName = string.Empty;

    [ObservableProperty]
    private string _photoMeteringMode = string.Empty;

    [ObservableProperty]
    private string _photoOrientation = string.Empty;

    [ObservableProperty]
    private string _photoOriginalExtension = string.Empty;

    [ObservableProperty]
    private string _photoResolutionUnit = string.Empty;

    [ObservableProperty]
    private string _photoSaturation = string.Empty;

    [ObservableProperty]
    private string _photoSceneCaptureType = string.Empty;

    [ObservableProperty]
    private string _photoSensingMethod = string.Empty;

    [ObservableProperty]
    private string _photoSharpness = string.Empty;

    [ObservableProperty]
    private string _photoSoftwareUsed = string.Empty;

    [ObservableProperty]
    private string _photoSource = string.Empty;

    [ObservableProperty]
    private string _photoSubjectDistanceRange = string.Empty;

    [ObservableProperty]
    private string _photoUploadDate = string.Empty;

    [ObservableProperty]
    private string _photoVerticalResolution = string.Empty;

    [ObservableProperty]
    private string _photoWhiteBalance = string.Empty;

    public ProcessorPhotoDetailViewModel(ILogger<ProcessorPhotoDetailViewModel> logger, IRegionManager regionManager,
                                          ProcessorsService                     processorsService, ProcessorPhotoCache photoCache,
                                          IStringLocalizer                      localizer,         ImageSourceFactory  imageSourceFactory)
    {
        _logger             = logger;
        _regionManager      = regionManager;
        _processorsService  = processorsService;
        _photoCache         = photoCache;
        _localizer          = localizer;
        _imageSourceFactory = imageSourceFactory;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<Guid>(NavParamKeys.PhotoId, out Guid photoId))
            _ = LoadPhotoCommand.ExecuteAsync(photoId);
    }

    [RelayCommand]
    public Task GoBack()
    {
        _regionManager.Regions[RegionNames.Content].NavigationService.Journal.GoBack();

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToProcessor()
    {
        if(_processorId <= 0) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.ProcessorId, _processorId },
            { NavParamKeys.NavigationSource, nameof(ProcessorPhotoDetailViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(ProcessorDetailPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task LoadPhoto(Guid photoId)
    {
        try
        {
            IsLoading        = true;
            ErrorOccurred    = false;
            ErrorMessage     = string.Empty;
            PhotoImageSource = null;

            _logger.LogInformation("Loading Processor photo details for {PhotoId}", photoId);

            // Fetch photo details from API
            ProcessorPhotoDto? photo = await _processorsService.GetProcessorPhotoDetailsAsync(photoId);

            if(photo is null)
            {
                ErrorOccurred = true;
                ErrorMessage  = _localizer["Photo not found"];
                IsLoading     = false;

                return;
            }

            // Populate photo information
            _processorId = photo.ProcessorId ?? 0;

            PhotoAuthor               = photo.Author             ?? string.Empty;
            PhotoCameraManufacturer   = photo.CameraManufacturer ?? string.Empty;
            PhotoCameraModel          = photo.CameraModel        ?? string.Empty;
            PhotoComments             = photo.Comments           ?? string.Empty;
            PhotoLensModel            = photo.Lens               ?? string.Empty;
            PhotoLicenseName          = photo.LicenseName        ?? string.Empty;
            PhotoProcessorCompanyName = photo.ProcessorCompanyName ?? string.Empty;
            PhotoProcessorName        = photo.ProcessorName        ?? string.Empty;
            PhotoOriginalExtension    = photo.OriginalExtension  ?? string.Empty;

            if(photo.CreationDate.HasValue)
                PhotoCreationDate = photo.CreationDate.Value.ToString("MMMM d, yyyy 'at' HH:mm");

            // EXIF Camera Settings
            PhotoAperture     = photo.Aperture != null ? $"f/{photo.Aperture}" : string.Empty;
            PhotoExposureTime = photo.Exposure != null ? $"{photo.Exposure}s" : string.Empty;

            PhotoExposureMode = photo.ExposureMethod.HasValue
                                    ? ((ExposureMode)photo.ExposureMethod.Value).Humanize()
                                    : string.Empty;

            PhotoExposureProgram = photo.ExposureProgram.HasValue
                                       ? ((ExposureProgram)photo.ExposureProgram.Value).Humanize()
                                       : string.Empty;

            PhotoFocalLength           = photo.FocalLength     != null ? $"{photo.FocalLength}mm" : string.Empty;
            PhotoFocalLengthEquivalent = photo.FocalEquivalent != null ? $"{photo.FocalEquivalent}mm" : string.Empty;
            PhotoIsoRating             = photo.Iso             != null ? photo.Iso.ToString() : string.Empty;

            PhotoFlash = photo.Flash.HasValue ? ((Flash)photo.Flash.Value).Humanize() : string.Empty;

            PhotoLightSource = photo.LightSource.HasValue
                                   ? ((LightSource)photo.LightSource.Value).Humanize()
                                   : string.Empty;

            PhotoMeteringMode = photo.MeteringMode.HasValue
                                    ? ((MeteringMode)photo.MeteringMode.Value).Humanize()
                                    : string.Empty;

            PhotoWhiteBalance = photo.WhiteBalance.HasValue
                                    ? ((WhiteBalance)photo.WhiteBalance.Value).Humanize()
                                    : string.Empty;

            // Photo Properties
            PhotoColorSpace = photo.Colorspace.HasValue
                                  ? ((ColorSpace)photo.Colorspace.Value).Humanize()
                                  : string.Empty;

            PhotoContrast = photo.Contrast.HasValue ? ((Contrast)photo.Contrast.Value).Humanize() : string.Empty;

            PhotoSaturation = photo.Saturation.HasValue
                                  ? ((Saturation)photo.Saturation.Value).Humanize()
                                  : string.Empty;

            PhotoSharpness = photo.Sharpness.HasValue ? ((Sharpness)photo.Sharpness.Value).Humanize() : string.Empty;

            PhotoOrientation = photo.Orientation.HasValue
                                   ? ((Orientation)photo.Orientation.Value).Humanize()
                                   : string.Empty;

            PhotoSceneCaptureType = photo.SceneCaptureType.HasValue
                                        ? ((SceneCaptureType)photo.SceneCaptureType.Value).Humanize()
                                        : string.Empty;

            PhotoSensingMethod = photo.SensingMethod.HasValue
                                     ? ((SensingMethod)photo.SensingMethod.Value).Humanize()
                                     : string.Empty;

            PhotoSubjectDistanceRange = photo.SubjectDistanceRange.HasValue
                                            ? ((SubjectDistanceRange)photo.SubjectDistanceRange.Value).Humanize()
                                            : string.Empty;

            // Resolution and Other
            PhotoHorizontalResolution = photo.HorizontalResolution != null
                                            ? string.Format(_localizer["_0_DPI"], photo.HorizontalResolution)
                                            : string.Empty;

            PhotoVerticalResolution = photo.VerticalResolution != null
                                          ? string.Format(_localizer["_0_DPI"], photo.VerticalResolution)
                                          : string.Empty;

            PhotoResolutionUnit = photo.ResolutionUnit.HasValue
                                      ? ((ResolutionUnit)photo.ResolutionUnit.Value).Humanize()
                                      : string.Empty;

            PhotoDigitalZoomRatio = photo.DigitalZoom != null ? $"{photo.DigitalZoom}x" : string.Empty;
            PhotoExifVersion      = photo.ExifVersion ?? string.Empty;
            PhotoSoftwareUsed     = photo.Software    ?? string.Empty;

            PhotoUploadDate = photo.UploadDate.HasValue
                                  ? photo.UploadDate.Value.ToString(_localizer["MMMM_d_yyyy_at_HH_mm"])
                                  : string.Empty;

            PhotoSource = photo.Source ?? string.Empty;

            // Load the full photo image
            await LoadPhotoImageAsync(photoId);

            IsLoading = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading Processor photo details for {PhotoId}", photoId);
            ErrorOccurred = true;
            ErrorMessage  = ex.Message;
            IsLoading     = false;
        }
    }

    /// <summary>
    ///     Updates the portrait/landscape orientation flag
    /// </summary>
    public void UpdateOrientation(bool isPortrait)
    {
        IsPortrait = isPortrait;
    }

    private async Task LoadPhotoImageAsync(Guid photoId)
    {
        try
        {
            Stream stream = await _photoCache.GetPhotoAsync(photoId);

            PhotoImageSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading Processor photo image {PhotoId}", photoId);
            ErrorOccurred = true;
            ErrorMessage  = _localizer["Failed to load photo image"];
        }
    }
}
