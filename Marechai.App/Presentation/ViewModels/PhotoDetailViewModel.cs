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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage.Streams;
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

public partial class PhotoDetailViewModel : ObservableObject, IRegionAware
{
    private readonly ComputersService              _computersService;
    private readonly ImageSourceFactory            _imageSourceFactory;
    private readonly IStringLocalizer              _localizer;
    private readonly ILogger<PhotoDetailViewModel> _logger;
    private readonly IRegionManager                _regionManager;
    private readonly MachinePhotoCache             _photoCache;
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
    private string _photoMachineCompany = string.Empty;

    [ObservableProperty]
    private string _photoMachineName = string.Empty;

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

    public PhotoDetailViewModel(ILogger<PhotoDetailViewModel> logger,             IRegionManager     regionManager,
                                ComputersService              computersService,   MachinePhotoCache  photoCache,
                                IStringLocalizer              localizer,          ImageSourceFactory imageSourceFactory)
    {
        _logger             = logger;
        _regionManager      = regionManager;
        _computersService   = computersService;
        _photoCache         = photoCache;
        _localizer          = localizer;
        _imageSourceFactory = imageSourceFactory;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

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
    public async Task LoadPhoto(Guid photoId)
    {
        try
        {
            IsLoading        = true;
            ErrorOccurred    = false;
            ErrorMessage     = string.Empty;
            PhotoImageSource = null;

            _logger.LogInformation("Loading photo details for {PhotoId}", photoId);

            // Fetch photo details from API
            MachinePhotoDto? photo = await _computersService.GetMachinePhotoDetailsAsync(photoId);

            if(photo is null)
            {
                ErrorOccurred = true;
                ErrorMessage  = _localizer["Photo not found"];
                IsLoading     = false;

                return;
            }

            // Populate photo information
            PhotoAuthor             = photo.Author             ?? string.Empty;
            PhotoCameraManufacturer = photo.CameraManufacturer ?? string.Empty;
            PhotoCameraModel        = photo.CameraModel        ?? string.Empty;
            PhotoComments           = photo.Comments           ?? string.Empty;
            PhotoLensModel          = photo.Lens               ?? string.Empty;
            PhotoLicenseName        = photo.LicenseName        ?? string.Empty;
            PhotoMachineCompany     = photo.MachineCompanyName ?? string.Empty;
            PhotoMachineName        = photo.MachineName        ?? string.Empty;
            PhotoOriginalExtension  = photo.OriginalExtension  ?? string.Empty;

            if(photo.CreationDate.HasValue)
                PhotoCreationDate = photo.CreationDate.Value.ToString("MMMM d, yyyy 'at' HH:mm");

            // EXIF Camera Settings
            PhotoAperture     = photo.Aperture != null ? $"f/{photo.Aperture}" : string.Empty;
            PhotoExposureTime = photo.Exposure != null ? $"{photo.Exposure}s" : string.Empty;

            // Extract ExposureMode - simple nullable integer now
            PhotoExposureMode = photo.ExposureMethod.HasValue
                                    ? ((ExposureMode)photo.ExposureMethod.Value).Humanize()
                                    : string.Empty;

            // Extract ExposureProgram - simple nullable integer now
            PhotoExposureProgram = photo.ExposureProgram.HasValue
                                       ? ((ExposureProgram)photo.ExposureProgram.Value).Humanize()
                                       : string.Empty;

            PhotoFocalLength           = photo.FocalLength     != null ? $"{photo.FocalLength}mm" : string.Empty;
            PhotoFocalLengthEquivalent = photo.FocalEquivalent != null ? $"{photo.FocalEquivalent}mm" : string.Empty;
            PhotoIsoRating             = photo.Iso             != null ? photo.Iso.ToString() : string.Empty;

            // Extract Flash - simple nullable integer now
            PhotoFlash = photo.Flash.HasValue ? ((Flash)photo.Flash.Value).Humanize() : string.Empty;

            // Extract LightSource - simple nullable integer now
            PhotoLightSource = photo.LightSource.HasValue
                                   ? ((LightSource)photo.LightSource.Value).Humanize()
                                   : string.Empty;

            // Extract MeteringMode - simple nullable integer now
            PhotoMeteringMode = photo.MeteringMode.HasValue
                                    ? ((MeteringMode)photo.MeteringMode.Value).Humanize()
                                    : string.Empty;

            // Extract WhiteBalance - simple nullable integer now
            PhotoWhiteBalance = photo.WhiteBalance.HasValue
                                    ? ((WhiteBalance)photo.WhiteBalance.Value).Humanize()
                                    : string.Empty;

            // Photo Properties
            // Extract ColorSpace - simple nullable integer now
            PhotoColorSpace = photo.Colorspace.HasValue
                                  ? ((ColorSpace)photo.Colorspace.Value).Humanize()
                                  : string.Empty;

            // Extract Contrast - simple nullable integer now
            PhotoContrast = photo.Contrast.HasValue ? ((Contrast)photo.Contrast.Value).Humanize() : string.Empty;

            // Extract Saturation - simple nullable integer now
            PhotoSaturation = photo.Saturation.HasValue
                                  ? ((Saturation)photo.Saturation.Value).Humanize()
                                  : string.Empty;

            // Extract Sharpness - simple nullable integer now
            PhotoSharpness = photo.Sharpness.HasValue ? ((Sharpness)photo.Sharpness.Value).Humanize() : string.Empty;

            // Extract Orientation - simple nullable integer now
            PhotoOrientation = photo.Orientation.HasValue
                                   ? ((Orientation)photo.Orientation.Value).Humanize()
                                   : string.Empty;

            // Extract SceneCaptureType - simple nullable integer now
            PhotoSceneCaptureType = photo.SceneCaptureType.HasValue
                                        ? ((SceneCaptureType)photo.SceneCaptureType.Value).Humanize()
                                        : string.Empty;

            // Extract SensingMethod - simple nullable integer now
            PhotoSensingMethod = photo.SensingMethod.HasValue
                                     ? ((SensingMethod)photo.SensingMethod.Value).Humanize()
                                     : string.Empty;

            // Extract SubjectDistanceRange - simple nullable integer now
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

            // Extract ResolutionUnit - simple nullable integer now
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
            _logger.LogError(ex, "Error loading photo details for {PhotoId}", photoId);
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

            if(PhotoImageSource is null)
            {
                _logger.LogWarning("Machine photo image source was null for {PhotoId}", photoId);
                ErrorOccurred = true;
                ErrorMessage  = _localizer["Failed to load photo image"];
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading photo image {PhotoId}", photoId);
            ErrorOccurred = true;
            ErrorMessage  = _localizer["Failed to load photo image"];
        }
    }

    /// <summary>
    ///     Extracts an integer value from AdditionalData dictionary
    /// </summary>
    private int ExtractInt(IDictionary<string, object> additionalData)
    {
        if(additionalData == null || additionalData.Count == 0) return 0;

        object? value = additionalData.Values.FirstOrDefault();

        if(value is int intValue) return intValue;
        if(value is double dblValue) return (int)dblValue;
        if(int.TryParse(value?.ToString() ?? "", out int parsed)) return parsed;

        return 0;
    }

    /// <summary>
    ///     Humanizes an enum value extracted from AdditionalData
    /// </summary>
    private string ExtractAndHumanizeEnum(IDictionary<string, object>? additionalData, Type enumType)
    {
        if(additionalData == null || additionalData.Count == 0) return string.Empty;

        int intValue = ExtractInt(additionalData);

        if(intValue == 0 && enumType != typeof(ExposureMode)) return string.Empty;

        object enumValue = Enum.ToObject(enumType, intValue);

        MethodInfo humanizeMethod = typeof(EnumHumanizeExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                  .First(m => m.Name == "Humanize" &&
                                                                              m.IsGenericMethodDefinition)
                                                                  .MakeGenericMethod(enumType);

        return (string)humanizeMethod.Invoke(null, [enumValue])!;
    }
}
