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

public class SoftwareVersionDto : BaseDto<ulong>
{
    [JsonPropertyName("family")]
    public string? Family { get; set; }
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }
    [JsonPropertyName("codename")]
    public string? Codename { get; set; }
    [JsonPropertyName("version")]
    public string? Version { get; set; }
    [JsonPropertyName("introduced")]
    public DateTime? Introduced { get; set; }
    [JsonPropertyName("previous")]
    public string? Previous { get; set; }
    [JsonPropertyName("license")]
    public string? License { get; set; }
    [JsonPropertyName("family_id")]
    [Required]
    public ulong FamilyId { get; set; }
    [JsonPropertyName("license_id")]
    public int? LicenseId { get; set; }
    [JsonPropertyName("previous_id")]
    public ulong? PreviousId { get; set; }
}