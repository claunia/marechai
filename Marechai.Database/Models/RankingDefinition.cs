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
using System.Collections.Generic;

namespace Marechai.Database.Models;

/// <summary>
///     One Marechai ranking definition (e.g. "Overall Top 250", "Top 250 Action games",
///     "Top 250 PlayStation 2 titles"). Each row identifies a single ranking by its
///     <see cref="Dimension" /> and optional <see cref="DimensionId" />:
///     <list type="bullet">
///         <item><see cref="RankingDimension.All" /> — <see cref="DimensionId" /> is null.</item>
///         <item><see cref="RankingDimension.Genre" /> — <see cref="DimensionId" /> = <c>SoftwareGenre.Id</c>.</item>
///         <item><see cref="RankingDimension.Platform" /> — <see cref="DimensionId" /> = <c>SoftwarePlatform.Id</c>.</item>
///     </list>
///     A definition is ONLY emitted when at least one software qualifies for the ranking,
///     so the table never contains empty rankings. The whole table is recomputed by
///     <c>MarechaiRankingsWorker</c> every 24 h via a single atomic delete+insert sweep.
/// </summary>
public class RankingDefinition : BaseModel<uint>
{
    /// <summary>Which dimension this ranking groups by. See <see cref="RankingDimension" />.</summary>
    public RankingDimension Dimension { get; set; }

    /// <summary>
    ///     The id of the grouping entity within <see cref="Dimension" />: <c>SoftwareGenre.Id</c>
    ///     for <see cref="RankingDimension.Genre" />, <c>SoftwarePlatform.Id</c> for
    ///     <see cref="RankingDimension.Platform" />, or <see langword="null" /> for
    ///     <see cref="RankingDimension.All" />. Stored as <c>long?</c> because the underlying ids
    ///     are heterogeneous (int / ulong) but always fit safely in a signed long.
    /// </summary>
    public long? DimensionId { get; set; }

    /// <summary>Number of <see cref="RankingEntry" /> rows linked to this definition (≤ 250).</summary>
    public int EntryCount { get; set; }

    /// <summary>UTC timestamp of the calculator run that produced this row.</summary>
    public DateTime ComputedAt { get; set; }

    public virtual ICollection<RankingEntry> Entries { get; set; }
}

/// <summary>
///     The grouping axis for a <see cref="RankingDefinition" />. Persisted as a <c>tinyint</c>
///     so values must stay stable; do NOT reorder, only append.
/// </summary>
public enum RankingDimension : byte
{
    All      = 0,
    Genre    = 1,
    Platform = 2
}
