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
///     Well-known seeded user identifiers. Inserted by EF migrations and used at runtime by services and controllers.
/// </summary>
public static class WellKnownUsers
{
    /// <summary>
    ///     The built-in <c>system</c> account used for bot-authored messages (e.g. report notifications). Locked out
    ///     indefinitely with no password set so it can never sign in.
    /// </summary>
    public const string SystemUserId = "00000000-0000-0000-0000-00000000sys";

    public const string SystemUserName    = "system";
    public const string SystemDisplayName = "System";
}
