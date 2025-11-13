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

using System.Text.Json.Serialization;
using Marechai.Database;

namespace Marechai.Data.Dtos;

public class BaseScanDto : BaseDto<Guid>
{
    [JsonPropertyName("author")]
    public string? Author { get; set; }
    [JsonPropertyName("colorspace")]
    public ColorSpace? ColorSpace { get; set; }
    [JsonPropertyName("comments")]
    public string? Comments { get; set; }
    [JsonPropertyName("creation_date")]
    public DateTime? CreationDate { get; set; }
    [JsonPropertyName("exif_version")]
    public string? ExifVersion { get; set; }
    [JsonPropertyName("horizontal_resolution")]
    public double? HorizontalResolution { get; set; }
    [JsonPropertyName("resolution_unit")]
    public ResolutionUnit? ResolutionUnit { get; set; }
    [JsonPropertyName("scanner_manufacturer")]
    public string? ScannerManufacturer { get; set; }
    [JsonPropertyName("scanner_model")]
    public string? ScannerModel { get; set; }
    [JsonPropertyName("software")]
    public string? SoftwareUsed { get; set; }
    [JsonPropertyName("upload_date")]
    public DateTime UploadDate { get; set; }
    [JsonPropertyName("vertical_resolution")]
    public double? VerticalResolution { get; set; }
    [JsonPropertyName("original_extension")]
    public string OriginalExtension { get; set; }
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
}