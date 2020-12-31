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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System;
using Marechai.Database;

namespace Marechai.ViewModels
{
    public class BasePhotoViewModel : BaseViewModel<Guid>
    {
        public double?               Aperture              { get; set; }
        public string                Author                { get; set; }
        public string                CameraManufacturer    { get; set; }
        public string                CameraModel           { get; set; }
        public ColorSpace?           ColorSpace            { get; set; }
        public string                Comments              { get; set; }
        public Contrast?             Contrast              { get; set; }
        public DateTime?             CreationDate          { get; set; }
        public double?               DigitalZoomRatio      { get; set; }
        public string                ExifVersion           { get; set; }
        public double?               ExposureTime          { get; set; }
        public ExposureMode?         ExposureMethod        { get; set; }
        public ExposureProgram?      ExposureProgram       { get; set; }
        public Flash?                Flash                 { get; set; }
        public double?               Focal                 { get; set; }
        public double?               FocalLength           { get; set; }
        public double?               FocalLengthEquivalent { get; set; }
        public double?               HorizontalResolution  { get; set; }
        public ushort?               IsoRating             { get; set; }
        public string                Lens                  { get; set; }
        public LightSource?          LightSource           { get; set; }
        public MeteringMode?         MeteringMode          { get; set; }
        public ResolutionUnit?       ResolutionUnit        { get; set; }
        public Orientation?          Orientation           { get; set; }
        public Saturation?           Saturation            { get; set; }
        public SceneCaptureType?     SceneCaptureType      { get; set; }
        public SensingMethod?        SensingMethod         { get; set; }
        public Sharpness?            Sharpness             { get; set; }
        public string                SoftwareUsed          { get; set; }
        public SubjectDistanceRange? SubjectDistanceRange  { get; set; }
        public DateTime              UploadDate            { get; set; }
        public double?               VerticalResolution    { get; set; }
        public WhiteBalance?         WhiteBalance          { get; set; }
        public string                UserId                { get; set; }
        public string                LicenseName           { get; set; }
        public int                   LicenseId             { get; set; }
        public string                OriginalExtension     { get; set; }
    }
}