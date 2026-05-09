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

using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Marechai.Database;

/// <summary>
///     MariaDB 12+ changed the default collation for utf8mb4 from utf8mb4_general_ci to utf8mb4_uca1400_ai_ci.
///     This interceptor restores the old behavior per-session so that new tables/columns created by EF migrations
///     use utf8mb4_general_ci, matching existing schema objects and avoiding FK collation mismatches.
/// </summary>
public sealed class MariaDb12CollationInterceptor : DbConnectionInterceptor
{
    const string _sql = "SET SESSION character_set_collations = 'utf8mb4=utf8mb4_general_ci';";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using DbCommand cmd = connection.CreateCommand();
        cmd.CommandText = _sql;
        cmd.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData,
                                                     CancellationToken cancellationToken = default)
    {
        await using DbCommand cmd = connection.CreateCommand();
        cmd.CommandText = _sql;

        // Intentionally NOT passing cancellationToken: this is a single
        // sub-millisecond SET SESSION executed once per pooled connection.
        // Cancelling it mid-flight would leave the freshly-opened connection
        // without its collation override and surface as a noisy first-chance
        // OperationCanceledException in the debugger every time a client
        // aborts a request (e.g. MudVirtualize superseding a scroll batch).
        await cmd.ExecuteNonQueryAsync();
    }
}
