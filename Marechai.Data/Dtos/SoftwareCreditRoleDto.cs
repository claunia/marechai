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

using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

/// <summary>
///     Distinct credit-role pair returned by <c>GET /software/credits/roles?lang={iso639_3}</c>.
///     Used by the credits-suggestion dialog autocomplete: <see cref="Role" /> drives the
///     display list in the contributor's UI language; <see cref="CanonicalRole" /> is what the
///     dialog forwards on the wire when the contributor picks an existing role, so the database
///     keys on canonical English regardless of UI locale.
/// </summary>
public class SoftwareCreditRoleDto
{
    /// <summary>Localized role text. Falls back to <see cref="CanonicalRole" /> when no translation row exists yet.</summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>Canonical English role text. Always populated.</summary>
    [JsonPropertyName("canonical_role")]
    public string? CanonicalRole { get; set; }
}
