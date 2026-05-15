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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

/// <summary>
///     A group of Software rows whose names normalise to the same key, surfaced by
///     GET /software/admin/duplicates. The admin UI renders the <see cref="NormalizedName"/>
///     as the group header and lists each <see cref="SoftwareDuplicateItemDto"/> beneath it
///     with a Merge action.
/// </summary>
public class SoftwareDuplicateGroupDto
{
    /// <summary>
    ///     The shared normalisation key (lower-case, parens/brackets stripped, whitespace
    ///     collapsed). All items in <see cref="Items"/> share this key.
    /// </summary>
    [JsonPropertyName("normalized_name")]
    [Required]
    public required string NormalizedName { get; set; }

    /// <summary>
    ///     The original Software rows that all collapse to <see cref="NormalizedName"/>. The
    ///     server only emits groups containing at least two items.
    /// </summary>
    [JsonPropertyName("items")]
    [Required]
    public required List<SoftwareDuplicateItemDto> Items { get; set; }
}
