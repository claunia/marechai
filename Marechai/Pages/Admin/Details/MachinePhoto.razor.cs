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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Orientation = Marechai.Database.Orientation;

namespace Marechai.Pages.Admin.Details
{
    public partial class MachinePhoto
    {
        const int                     _maxUploadSize = 25 * 1048576;
        AuthenticationState           _authState;
        bool                          _editing;
        List<Database.Models.License> _licenses;
        bool                          _loaded;
        MachinePhotoViewModel         _model;
        bool                          _unknownAperture;
        bool                          _unknownAuthor;
        bool                          _unknownCameraManufacturer;
        bool                          _unknownCameraModel;
        bool                          _unknownColorSpace;
        bool                          _unknownComments;
        bool                          _unknownContrast;
        bool                          _unknownCreationDate;
        bool                          _unknownDigitalZoomRatio;
        bool                          _unknownExifVersion;
        bool                          _unknownExposure;
        bool                          _unknownExposureMethod;
        bool                          _unknownExposureProgram;
        bool                          _unknownFlash;
        bool                          _unknownFocal;
        bool                          _unknownFocalLength;
        bool                          _unknownFocalLengthEquivalent;
        bool                          _unknownHorizontalResolution;
        bool                          _unknownIsoRating;
        bool                          _unknownLens;
        bool                          _unknownLightSource;
        bool                          _unknownMeteringMode;
        bool                          _unknownOrientation;
        bool                          _unknownResolutionUnit;
        bool                          _unknownSaturation;
        bool                          _unknownSceneCaptureType;
        bool                          _unknownSensingMethod;
        bool                          _unknownSharpness;
        bool                          _unknownSoftwareUsed;
        bool                          _unknownSource;
        bool                          _unknownSubjectDistanceRange;
        bool                          _unknownVerticalResolution;
        bool                          _unknownWhiteBalance;
        ApplicationUser               _user;
        [Parameter]
        public Guid Id { get; set; }
        [Parameter]
        public int MachineId { get; set; }

        ushort ColorSpace
        {
            get
            {
                if(_model.ColorSpace is null)
                    return 0;

                return(ushort)_model.ColorSpace;
            }
            set => _model.ColorSpace = (ColorSpace)value;
        }

        ushort Contrast
        {
            get
            {
                if(_model.Contrast is null)
                    return 0;

                return(ushort)_model.Contrast;
            }
            set => _model.Contrast = (Contrast)value;
        }

        ushort ExposureMode
        {
            get
            {
                if(_model.ExposureMethod is null)
                    return 0;

                return(ushort)_model.ExposureMethod;
            }
            set => _model.ExposureMethod = (ExposureMode)value;
        }

        ushort ExposureProgram
        {
            get
            {
                if(_model.ExposureProgram is null)
                    return 0;

                return(ushort)_model.ExposureProgram;
            }
            set => _model.ExposureProgram = (ExposureProgram)value;
        }

        ushort Flash
        {
            get
            {
                if(_model.Flash is null)
                    return 0;

                return(ushort)_model.Flash;
            }
            set => _model.Flash = (Flash)value;
        }

        ushort LightSource
        {
            get
            {
                if(_model.LightSource is null)
                    return 0;

                return(ushort)_model.LightSource;
            }
            set => _model.LightSource = (LightSource)value;
        }

        ushort MeteringMode
        {
            get
            {
                if(_model.MeteringMode is null)
                    return 0;

                return(ushort)_model.MeteringMode;
            }
            set => _model.MeteringMode = (MeteringMode)value;
        }

        ushort ResolutionUnit
        {
            get
            {
                if(_model.ResolutionUnit is null)
                    return 0;

                return(ushort)_model.ResolutionUnit;
            }
            set => _model.ResolutionUnit = (ResolutionUnit)value;
        }

        ushort Orientation
        {
            get
            {
                if(_model.Orientation is null)
                    return 0;

                return(ushort)_model.Orientation;
            }
            set => _model.Orientation = (Orientation)value;
        }

        ushort Saturation
        {
            get
            {
                if(_model.Saturation is null)
                    return 0;

                return(ushort)_model.Saturation;
            }
            set => _model.Saturation = (Saturation)value;
        }

        ushort SceneCaptureType
        {
            get
            {
                if(_model.SceneCaptureType is null)
                    return 0;

                return(ushort)_model.SceneCaptureType;
            }
            set => _model.SceneCaptureType = (SceneCaptureType)value;
        }

        ushort SensingMethod
        {
            get
            {
                if(_model.SensingMethod is null)
                    return 0;

                return(ushort)_model.SensingMethod;
            }
            set => _model.SensingMethod = (SensingMethod)value;
        }

        ushort Sharpness
        {
            get
            {
                if(_model.Sharpness is null)
                    return 0;

                return(ushort)_model.Sharpness;
            }
            set => _model.Sharpness = (Sharpness)value;
        }

        ushort SubjectDistanceRange
        {
            get
            {
                if(_model.SubjectDistanceRange is null)
                    return 0;

                return(ushort)_model.SubjectDistanceRange;
            }
            set => _model.SubjectDistanceRange = (SubjectDistanceRange)value;
        }

        ushort WhiteBalance
        {
            get
            {
                if(_model.WhiteBalance is null)
                    return 0;

                return(ushort)_model.WhiteBalance;
            }
            set => _model.WhiteBalance = (WhiteBalance)value;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            if(Id == Guid.Empty)
                return;

            _model     = await Service.GetAsync(Id);
            _licenses  = await LicensesService.GetAsync();
            _authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            _editing = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                         StartsWith("admin/machines/photo/edit/", StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            _user = await UserManager.FindByIdAsync(_model.UserId);

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownAperture              = _model.Aperture is null;
            _unknownAuthor                = _model.Author is null;
            _unknownSource                = _model.Source is null;
            _unknownCameraManufacturer    = _model.CameraManufacturer is null;
            _unknownCameraModel           = _model.CameraModel is null;
            _unknownColorSpace            = _model.ColorSpace is null;
            _unknownContrast              = _model.Contrast is null;
            _unknownCreationDate          = _model.CreationDate is null;
            _unknownDigitalZoomRatio      = _model.DigitalZoomRatio is null;
            _unknownExifVersion           = _model.ExifVersion is null;
            _unknownExposure              = _model.ExposureTime is null;
            _unknownExposureMethod        = _model.ExposureMethod is null;
            _unknownExposureProgram       = _model.ExposureProgram is null;
            _unknownFlash                 = _model.Flash is null;
            _unknownFocal                 = _model.Focal is null;
            _unknownFocalLength           = _model.FocalLength is null;
            _unknownFocalLengthEquivalent = _model.FocalLengthEquivalent is null;
            _unknownIsoRating             = _model.IsoRating is null;
            _unknownLens                  = _model.Lens is null;
            _unknownLightSource           = _model.LightSource is null;
            _unknownMeteringMode          = _model.MeteringMode is null;
            _unknownResolutionUnit        = _model.ResolutionUnit is null;
            _unknownOrientation           = _model.Orientation is null;
            _unknownSaturation            = _model.Saturation is null;
            _unknownSceneCaptureType      = _model.SceneCaptureType is null;
            _unknownSensingMethod         = _model.SensingMethod is null;
            _unknownSharpness             = _model.Sharpness is null;
            _unknownSoftwareUsed          = _model.SoftwareUsed is null;
            _unknownSubjectDistanceRange  = _model.SoftwareUsed is null;
            _unknownHorizontalResolution  = _model.HorizontalResolution is null;
            _unknownVerticalResolution    = _model.VerticalResolution is null;
            _unknownWhiteBalance          = _model.WhiteBalance is null;
            _unknownComments              = _model.Comments is null;
        }

        void OnEditClicked()
        {
            _editing = true;
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnCancelClicked()
        {
            _editing = false;

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            await Service.UpdateAsync(_model, (await UserManager.GetUserAsync(_authState.User)).Id);
            _editing = false;
            _model   = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        void ValidateDoubleBiggerThanZero(ValidatorEventArgs e)
        {
            if(e.Value is double item &&
               item > 0)
                e.Status = ValidationStatus.Success;
            else
                e.Status = ValidationStatus.Error;
        }

        void ValidateDoubleBiggerOrEqualThanZero(ValidatorEventArgs e)
        {
            if(e.Value is double item &&
               item >= 0)
                e.Status = ValidationStatus.Success;
            else
                e.Status = ValidationStatus.Error;
        }

        void ValidateUnsignedShortBiggerThanZero(ValidatorEventArgs e)
        {
            if(e.Value is ushort item &&
               item > 0)
                e.Status = ValidationStatus.Success;
            else
                e.Status = ValidationStatus.Error;
        }

        void ValidateAuthor(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Author must be 255 characters or less."], 255);

        void ValidateSource(ValidatorEventArgs e) =>
            Validators.ValidateUrl(e, L["Source URL must be smaller than 255 characters."], 255);

        void ValidateCameraManufacturer(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Camera manufacturer must be 255 characters or less."], 255);

        void ValidateCameraModel(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Camera model must be 255 characters or less."], 255);

        void ValidateDate(ValidatorEventArgs e)
        {
            if(!(e.Value is DateTime item) ||
               item.Year <= 1816           ||
               item      >= DateTime.UtcNow)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        void ValidateExifVersion(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Exif version must be 255 characters or less."], 255);

        void ValidateLens(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Lens name must be 255 characters or less."], 255);

        void ValidateSoftwareUsed(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Software used must be 255 characters or less."], 255);

        void ValidateComments(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["User comments must be 255 characters or less."], 255);
    }
}