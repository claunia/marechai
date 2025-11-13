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

public class PersonByMagazineDto : BaseDto<long>
{
    [JsonPropertyName("person_id")]
    [Required]
    public int PersonId { get; set; }
    [JsonPropertyName("magazine_id")]
    [Required]
    public long MagazineId { get; set; }
    [JsonPropertyName("role_id")]
    [Required]
    public required string RoleId { get; set; }
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("alias")]
    public string? Alias { get; set; }
    [JsonPropertyName("surname")]
    public string? Surname { get; set; }
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
    [JsonIgnore]
    public string FullName => DisplayName ?? Alias ?? $"{Name} {Surname}";
}