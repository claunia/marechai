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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.Data;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Helpers;

/// <summary>
///     Resolves the front-cover Guid for a batch of <c>Software</c> rows. Mirrors the
///     non-compilation paths of <c>SoftwareController.PopulateFrontCoverIdsAsync</c>:
///     a cover can be reached either directly via <c>SoftwareRelease.SoftwareId</c> or
///     indirectly via <c>SoftwareRelease.SoftwareVersion.SoftwareId</c>. Two
///     index-friendly IN/GROUP BY queries replace the per-row correlated subquery EF
///     used to emit. Compilation rows (where the dto Id is a <c>SoftwareRelease.Id</c>)
///     are intentionally NOT handled here — callers that need them must use
///     <c>SoftwareController.PopulateFrontCoverIdsAsync</c> directly.
/// </summary>
public static class SoftwareCoverLookup
{
    /// <summary>
    ///     Returns a map of <c>Software.Id</c> ⇒ front-cover <see cref="Guid" /> for the
    ///     given software IDs. Software rows without a front cover are absent from the
    ///     result. Returns an empty dictionary when <paramref name="softwareIds" /> is empty.
    /// </summary>
    public static async Task<Dictionary<ulong, Guid>> LookupFrontCoversAsync(
        MarechaiContext context, IReadOnlyCollection<ulong> softwareIds, CancellationToken ct = default)
    {
        var result = new Dictionary<ulong, Guid>();

        if(softwareIds.Count == 0) return result;

        // Path 1: cover.Release.SoftwareId — release attached to software directly.
        var direct = await context.SoftwareCovers
                                  .Where(sc => sc.Type == SoftwareCoverType.Front &&
                                               sc.Release.SoftwareId.HasValue    &&
                                               softwareIds.Contains(sc.Release.SoftwareId.Value))
                                  .Select(sc => new
                                   {
                                       SoftwareId = sc.Release.SoftwareId.Value,
                                       CoverId    = sc.Id
                                   })
                                  .ToListAsync(ct);

        foreach(IGrouping<ulong, Guid> g in direct.GroupBy(x => x.SoftwareId, x => x.CoverId))
            result[g.Key] = g.Min();

        // Path 2: cover.Release.SoftwareVersion.SoftwareId — release attached to a
        // version of the software. SoftwareVersion.SoftwareId is non-nullable ulong.
        var indirect = await context.SoftwareCovers
                                    .Where(sc => sc.Type == SoftwareCoverType.Front &&
                                                 sc.Release.SoftwareVersionId.HasValue &&
                                                 softwareIds.Contains(sc.Release.SoftwareVersion.SoftwareId))
                                    .Select(sc => new
                                     {
                                         SoftwareId = sc.Release.SoftwareVersion.SoftwareId,
                                         CoverId    = sc.Id
                                     })
                                    .ToListAsync(ct);

        foreach(IGrouping<ulong, Guid> g in indirect.GroupBy(x => x.SoftwareId, x => x.CoverId))
        {
            Guid candidate = g.Min();

            if(result.TryGetValue(g.Key, out Guid existing))
            {
                // Both paths matched: pick the lower Guid to mirror the original
                // FirstOrDefault(OrderBy(Id)) semantics across the union of covers.
                if(candidate.CompareTo(existing) < 0)
                    result[g.Key] = candidate;
            }
            else
                result[g.Key] = candidate;
        }

        return result;
    }
}
