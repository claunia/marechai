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
///     One placement of a software inside a Marechai ranking. Returned by
///     <c>GET /software/{id}/rankings</c> to drive the chip row under the Marechai-score
///     banner on the software detail page. See <see cref="RankingIndexEntryDto" /> for the
///     meaning of <see cref="Dimension" /> / <see cref="DimensionId" /> / <see cref="DimensionName" />.
/// </summary>
public class SoftwareRankingPlacementDto
{
    [JsonPropertyName("ranking_id")]
    public uint RankingId { get; set; }

    [JsonPropertyName("dimension")]
    public byte Dimension { get; set; }

    [JsonPropertyName("dimension_id")]
    public long? DimensionId { get; set; }

    [Required]
    [JsonPropertyName("dimension_name")]
    public string DimensionName { get; set; }

    [JsonPropertyName("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("entry_count")]
    public int EntryCount { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }
}
