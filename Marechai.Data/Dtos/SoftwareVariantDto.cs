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

public class SoftwareVariantDto : BaseDto<ulong>
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("introduced")]
    public DateTime? Introduced { get; set; }

    [JsonPropertyName("parent_id")]
    public ulong? ParentId { get; set; }

    [JsonPropertyName("parent")]
    public string? Parent { get; set; }

    [JsonPropertyName("version_id")]
    [Required]
    public ulong SoftwareVersionId { get; set; }

    [JsonPropertyName("software_version")]
    public string? SoftwareVersion { get; set; }

    [JsonPropertyName("minimum_memory")]
    public ulong? MinimumMemory { get; set; }

    [JsonPropertyName("recommended_memory")]
    public ulong? RecommendedMemory { get; set; }

    [JsonPropertyName("required_storage")]
    public ulong? RequiredStorage { get; set; }

    [JsonPropertyName("part_number")]
    public string? PartNumber { get; set; }

    [JsonPropertyName("serial_number")]
    public string? SerialNumber { get; set; }

    [JsonPropertyName("product_code")]
    public string? ProductCode { get; set; }

    [JsonPropertyName("catalogue_number")]
    public string? CatalogueNumber { get; set; }

    [JsonPropertyName("distribution_mode")]
    public DistributionMode DistributionMode { get; set; }

    [JsonPropertyName("family")]
    public string? Family { get; set; }
}