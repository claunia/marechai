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

using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Helpers;

/// <summary>
///     Resolves the owning <c>Software</c> (or <c>SoftwareCompilation</c>) of a
///     <c>SoftwareRelease</c>, so covers attached to a release also carry their owner.
///     Pages listing covers per software/compilation filter on those owner columns only.
/// </summary>
public static class SoftwareCoverOwner
{
    /// <summary>
    ///     Returns the owner of the given release: the software it belongs to (directly or
    ///     through its version), or, failing that, the compilation it belongs to. Both are
    ///     <c>null</c> when the release does not exist or has no owner.
    /// </summary>
    public static async Task<(ulong? SoftwareId, ulong? CompilationId)> ResolveFromReleaseAsync(
        MarechaiContext context, ulong releaseId)
    {
        var owner = await context.SoftwareReleases
                                 .Where(r => r.Id == releaseId)
                                 .Select(r => new
                                  {
                                      SoftwareId = r.SoftwareId ??
                                                   (r.SoftwareVersionId != null
                                                        ? (ulong?)r.SoftwareVersion.SoftwareId
                                                        : null),
                                      r.SoftwareCompilationId
                                  })
                                 .FirstOrDefaultAsync();

        if(owner is null) return (null, null);

        return owner.SoftwareId is not null ? (owner.SoftwareId, null) : (null, owner.SoftwareCompilationId);
    }
}
