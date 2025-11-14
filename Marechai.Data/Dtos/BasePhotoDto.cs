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

using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class BasePhotoDto : BaseDto<Guid>
{
    [JsonPropertyName("aperture")]
    public double? Aperture { get; set; }
    [JsonPropertyName("author")]
    public string? Author { get; set; }
    [JsonPropertyName("camera_manufacturer")]
    public string? CameraManufacturer { get; set; }
    [JsonPropertyName("camera_model")]
    public string? CameraModel { get; set; }
    [JsonPropertyName("colorspace")]
    public ColorSpace? ColorSpace { get; set; }
    [JsonPropertyName("comments")]
    public string? Comments { get; set; }
    [JsonPropertyName("contrast")]
    public Contrast? Contrast { get; set; }
    [JsonPropertyName("creation_date")]
    public DateTime? CreationDate { get; set; }
    [JsonPropertyName("digital_zoom")]
    public double? DigitalZoomRatio { get; set; }
    [JsonPropertyName("exif_version")]
    public string? ExifVersion { get; set; }
    [JsonPropertyName("exposure")]
    public double? ExposureTime { get; set; }
    [JsonPropertyName("exposure_method")]
    public ExposureMode? ExposureMethod { get; set; }
    [JsonPropertyName("exposure_program")]
    public ExposureProgram? ExposureProgram { get; set; }
    [JsonPropertyName("flash")]
    public Flash? Flash { get; set; }
    [JsonPropertyName("focal")]
    public double? Focal { get; set; }
    [JsonPropertyName("focal_length")]
    public double? FocalLength { get; set; }
    [JsonPropertyName("focal_equivalent")]
    public double? FocalLengthEquivalent { get; set; }
    [JsonPropertyName("horizontal_resolution")]
    public double? HorizontalResolution { get; set; }
    [JsonPropertyName("iso")]
    public ushort? IsoRating { get; set; }
    [JsonPropertyName("lens")]
    public string? Lens { get; set; }
    [JsonPropertyName("light_source")]
    public LightSource? LightSource { get; set; }
    [JsonPropertyName("metering_mode")]
    public MeteringMode? MeteringMode { get; set; }
    [JsonPropertyName("resolution_unit")]
    public ResolutionUnit? ResolutionUnit { get; set; }
    [JsonPropertyName("orientation")]
    public Orientation? Orientation { get; set; }
    [JsonPropertyName("saturation")]
    public Saturation? Saturation { get; set; }
    [JsonPropertyName("scene_capture_type")]
    public SceneCaptureType? SceneCaptureType { get; set; }
    [JsonPropertyName("sensing_method")]
    public SensingMethod? SensingMethod { get; set; }
    [JsonPropertyName("sharpness")]
    public Sharpness? Sharpness { get; set; }
    [JsonPropertyName("software")]
    public string? SoftwareUsed { get; set; }
    [JsonPropertyName("subject_distance_range")]
    public SubjectDistanceRange? SubjectDistanceRange { get; set; }
    [JsonPropertyName("upload_date")]
    public DateTime UploadDate { get; set; }
    [JsonPropertyName("vertical_resolution")]
    public double? VerticalResolution { get; set; }
    [JsonPropertyName("white_balance")]
    public WhiteBalance? WhiteBalance { get; set; }
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
    [JsonPropertyName("license_name")]
    public string? LicenseName { get; set; }
    [JsonPropertyName("license_id")]
    public int LicenseId { get; set; }
    [JsonPropertyName("original_extension")]
    public string OriginalExtension { get; set; }
}