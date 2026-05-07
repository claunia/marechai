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
///     Per-user view of a message: read flag and per-user soft-delete. One row is created for every participant
///     (including the sender) when a message is inserted, so soft-delete is uniform between Inbox and Sent folders.
/// </summary>
public class MessageState
{
    public long   MessageId { get; set; }
    public string UserId    { get; set; }

    public bool      IsRead    { get; set; }
    public DateTime? ReadAt    { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual Message         Message { get; set; }
    public virtual ApplicationUser User    { get; set; }
}
