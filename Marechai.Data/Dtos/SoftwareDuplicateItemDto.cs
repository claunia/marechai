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

/// <summary>
///     One Software row inside a duplicate group surfaced by GET /software/admin/duplicates.
///     All reference-typed fields are marked <see cref="RequiredAttribute"/> so the OpenAPI emitter
///     produces direct <c>$ref</c> shapes and Kiota does NOT generate composed-type wrappers
///     (see the kiota composed-type-wrapper trap note in the project memory).
/// </summary>
public class SoftwareDuplicateItemDto : BaseDto<ulong>
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }

    [JsonPropertyName("kind")]
    public SoftwareKind Kind { get; set; }

    /// <summary>Year of the earliest <c>SoftwareRelease.ReleaseDate</c>, or null when there are no releases.</summary>
    [JsonPropertyName("earliest_release_year")]
    public int? EarliestReleaseYear { get; set; }

    /// <summary>Total number of <c>SoftwareRelease</c> rows directly attached to this Software.</summary>
    [JsonPropertyName("releases_count")]
    public int ReleasesCount { get; set; }

    /// <summary>
    ///     Distinct platform names across all direct releases, comma-separated. Empty string when
    ///     no releases are attached. Server may truncate to top N + "+M more" to keep the cell compact.
    /// </summary>
    [JsonPropertyName("platforms")]
    [Required]
    public required string Platforms { get; set; }
}
