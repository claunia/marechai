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

using Marechai.Database.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Database;

/// <summary>
///     Centralized registration of every Marechai-specific EF interceptor.
///     Must be called by every consumer that builds <see cref="MarechaiContext"/> options
///     externally (Marechai.Server, Marechai.MobyGames, any future bulk-import tool) so that
///     the SaveChanges-driven SearchEntries upkeep happens consistently regardless of whether
///     a row is mutated by the API, the Blazor admin, or a CLI batch import.
/// </summary>
public static class MarechaiDbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder AddMarechaiInterceptors(this DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.AddInterceptors(new MariaDb12CollationInterceptor(),
                                       new SearchIndexInterceptor());
}
