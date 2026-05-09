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
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

/// <summary>
///     Single-use registration invitation. The <see cref="Code" /> string (length 9, format
///     <c>XXXX-YYYY</c> using a 32-char unambiguous alphabet) is the primary key. A code is consumed by setting
///     <see cref="UsedById" /> + <see cref="UsedOn" /> in the same transaction that creates the user, with
///     <see cref="RowVersion" /> acting as the optimistic concurrency token (rotated on consume) so that two
///     simultaneous registration attempts using the same code can never both succeed.
/// </summary>
public class InvitationCode
{
    [Key]
    [Required]
    [StringLength(9, MinimumLength = 9)]
    public string Code { get; set; }

    [Required]
    public string CreatedById { get; set; }

    [Required]
    public DateTime CreatedOn { get; set; }

    public string UsedById { get; set; }

    public DateTime? UsedOn { get; set; }

    /// <summary>
    ///     Optimistic concurrency token. MariaDB has no native rowversion type so we store a <see cref="Guid" />
    ///     and configure it as <c>IsConcurrencyToken</c>; the consumer must rotate this value in the same UPDATE
    ///     that sets <see cref="UsedById" />.
    /// </summary>
    [ConcurrencyCheck]
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public virtual ApplicationUser CreatedBy { get; set; }
    public virtual ApplicationUser UsedBy    { get; set; }
}
