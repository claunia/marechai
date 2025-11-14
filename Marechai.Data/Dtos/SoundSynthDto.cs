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

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoundSynthDto : BaseDto<int>
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }
    [JsonPropertyName("company")]
    public string? CompanyName { get; set; }
    [JsonPropertyName("company_id")]
    public int? CompanyId { get; set; }
    [JsonPropertyName("model_code")]
    public string? ModelCode { get; set; }
    [JsonPropertyName("introduced")]
    public DateTime? Introduced { get; set; }
    [JsonPropertyName("voices")]
    public int? Voices { get; set; }
    [JsonPropertyName("frequency")]
    public double? Frequency { get; set; }
    [JsonPropertyName("depth")]
    public int? Depth { get; set; }
    [JsonPropertyName("square_wave")]
    public int? SquareWave { get; set; }
    [JsonPropertyName("white_noise")]
    public int? WhiteNoise { get; set; }
    [JsonPropertyName("type")]
    public int? Type { get; set; }
    [JsonIgnore]
    public string IntroducedView => Introduced?.ToShortDateString() ?? "Unknown";
}