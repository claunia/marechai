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

public class PersonDto : BaseDto<int>
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }
    [JsonPropertyName("surname")]
    public string? Surname { get; set; }
    [JsonPropertyName("country")]
    public string? CountryOfBirth { get; set; }
    [JsonPropertyName("birthdate")]
    public DateTime BirthDate { get; set; }
    [JsonPropertyName("birthdate_precision")]
    public DatePrecision BirthDatePrecision { get; set; }
    [JsonPropertyName("death_date")]
    public DateTime? DeathDate { get; set; }
    [JsonPropertyName("death_date_precision")]
    public DatePrecision DeathDatePrecision { get; set; }
    [JsonPropertyName("webpage")]
    public string? Webpage { get; set; }
    [JsonPropertyName("twitter")]
    public string? Twitter { get; set; }
    [JsonPropertyName("facebook")]
    public string? Facebook { get; set; }
    [JsonPropertyName("photo")]
    public Guid? Photo { get; set; }
    [JsonPropertyName("alias")]
    public string? Alias { get; set; }
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
    [JsonPropertyName("country_id")]
    public short? CountryOfBirthId { get; set; }
    [JsonIgnore]
    public string FullName => DisplayName ?? Alias ?? $"{Name} {Surname}";
}