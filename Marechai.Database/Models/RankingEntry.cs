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

namespace Marechai.Database.Models;

/// <summary>
///     One software placement inside a <see cref="RankingDefinition" /> (e.g. "this game
///     is #3 in the Action ranking with score 8.42"). Capped at 250 entries per
///     definition. Composite PK on (<see cref="RankingDefinitionId" />, <see cref="SoftwareId" />)
///     prevents the same software from appearing twice in the same ranking. The whole
///     table is wiped and re-inserted by <c>MarechaiRankingsWorker</c> every 24 h.
/// </summary>
public class RankingEntry
{
    public         uint              RankingDefinitionId { get; set; }
    public virtual RankingDefinition RankingDefinition   { get; set; }

    public         ulong    SoftwareId { get; set; }
    public virtual Software Software   { get; set; }

    /// <summary>1-based position within the ranking (1 = best).</summary>
    public int Rank { get; set; }

    /// <summary>The Marechai score on a 0–10 scale, after Bayesian shrinkage.</summary>
    public double Score { get; set; }
}
