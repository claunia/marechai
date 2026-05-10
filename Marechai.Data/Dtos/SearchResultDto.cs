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

/// <summary>One result row returned by the site-wide search.</summary>
public class SearchResultDto
{
    /// <summary>Numeric value of <see cref="SearchEntityType"/>.</summary>
    [JsonPropertyName("entity_type")]
    public int EntityType { get; set; }

    [JsonPropertyName("entity_id")]
    public long EntityId { get; set; }

    [JsonPropertyName("display_name")]
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("alt_name")]
    public string? AltName { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("country_id")]
    public short? CountryId { get; set; }

    [JsonPropertyName("country_name")]
    public string? CountryName { get; set; }

    [JsonPropertyName("has_image")]
    public bool HasImage { get; set; }

    /// <summary>Computed relevance score (higher is better). Useful for diagnostics.</summary>
    [JsonPropertyName("score")]
    public double Score { get; set; }
}
