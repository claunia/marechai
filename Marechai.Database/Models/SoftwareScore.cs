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

namespace Marechai.Database.Models;

/// <summary>
///     One precomputed Marechai score row per eligible software (≥3 critic reviews OR
///     ≥1 user rating). UNLIKE <see cref="RankingEntry" /> this table is NOT capped at
///     250 — every eligible software gets a row so the per-software score banner on the
///     detail page works even for titles ranked outside the top 250 of every dimension.
///     <see cref="GlobalRank" /> is the 1-based position across ALL eligible software.
///     The table is wiped and re-inserted by <c>MarechaiRankingsWorker</c> every 24 h.
/// </summary>
public class SoftwareScore
{
    public         ulong    SoftwareId { get; set; }
    public virtual Software Software   { get; set; }

    /// <summary>0–10 Marechai score (Bayesian-shrunken average of critic + user sides).</summary>
    public double Score { get; set; }

    /// <summary>1-based position within the full eligible set (1 = best).</summary>
    public int GlobalRank { get; set; }

    /// <summary>Raw critic mean on the 0–100 scale (e.g. 78.4). Null if no critic reviews.</summary>
    public double? CriticAverage { get; set; }

    /// <summary>Raw user-rating mean on the 0–5 star scale. Null if no user ratings.</summary>
    public double? UserStarAverage { get; set; }

    public int CriticReviewCount { get; set; }
    public int UserRatingCount   { get; set; }

    /// <summary>UTC timestamp of the calculator run that produced this row.</summary>
    public DateTime ComputedAt { get; set; }
}
