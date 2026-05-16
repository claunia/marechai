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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

/// <summary>
///     Request body for <c>PATCH /software/admin/addons/base-software-bulk</c>.
///     Sets <see cref="BaseSoftwareId"/> on every software row listed in
///     <see cref="SoftwareIds"/>, flipping <c>Kind</c> to <see cref="SoftwareKind.Dlc"/>
///     when needed.
/// </summary>
public class BulkSetBaseSoftwareRequestDto
{
    [JsonPropertyName("software_ids")]
    [Required]
    public required List<ulong> SoftwareIds { get; set; }

    [JsonPropertyName("base_software_id")]
    public ulong BaseSoftwareId { get; set; }
}
