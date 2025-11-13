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

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class ScreenDto : BaseDto<int>
{
    [JsonPropertyName("width")]
    public double? Width { get; set; }
    [JsonPropertyName("height")]
    public double? Height { get; set; }
    [JsonPropertyName("diagonal")]
    [Required]
    public double Diagonal { get; set; }
    [JsonPropertyName("native_resolution_id")]
    [Required]
    public int NativeResolutionId { get; set; }
    [JsonPropertyName("native_resolution")]
    public ResolutionDto? NativeResolution { get; set; }
    [JsonPropertyName("effective_colors")]
    public long? EffectiveColors { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonIgnore]
    public long? Colors => EffectiveColors ?? NativeResolution.Colors;

    public string Size
    {
        get
        {
            if(Width != null && Height != null) return $"{Width}x{Height} mm";

            return "Unknown";
        }
    }
}