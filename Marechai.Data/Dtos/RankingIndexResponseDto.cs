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
///     Top-level response of <c>GET /software/rankings/index</c>: bundles the freshness
///     <see cref="Status" /> with the actual <see cref="Rankings" /> list so the frontend
///     can render the "being computed" banner alongside whatever rankings already exist
///     (worker is still mid-run after a fresh install).
/// </summary>
public class RankingIndexResponseDto
{
    /// <summary>
    ///     Non-nullable + <see cref="RequiredAttribute" /> so the OpenAPI schema emits a
    ///     direct <c>$ref</c> instead of <c>oneOf:[null,$ref]</c>; otherwise kiota generates
    ///     a composed-type wrapper that silently deserializes the nested DTO to null.
    /// </summary>
    [Required]
    [JsonPropertyName("status")]
    public RankingsStatusDto Status { get; set; }

    [Required]
    [JsonPropertyName("rankings")]
    public List<RankingIndexEntryDto> Rankings { get; set; }
}
