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

using System;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

/// <summary>
///     Freshness + concurrency snapshot of the persisted Marechai rankings. Returned as
///     part of <see cref="RankingIndexResponseDto" /> so the frontend can render a
///     "rankings are being computed" banner on a fresh install (<see cref="LastComputedAt" />
///     null AND <see cref="IsComputing" /> true) and a "last computed: …" timestamp on
///     normal pages.
/// </summary>
public class RankingsStatusDto
{
    [JsonPropertyName("last_computed_at")]
    public DateTime? LastComputedAt { get; set; }

    [JsonPropertyName("is_computing")]
    public bool IsComputing { get; set; }

    [JsonPropertyName("total_rankings")]
    public int TotalRankings { get; set; }
}
