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

using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class AdminNotification : BaseModel<long>
{
    [Required]
    [MaxLength(256)]
    public string Title { get; set; }

    [Required]
    [MaxLength(1024)]
    public string Message { get; set; }

    [MaxLength(512)]
    public string LinkUrl { get; set; }

    [MaxLength(128)]
    public string LinkText { get; set; }

    public bool IsRead { get; set; }

    public string TargetUserId { get; set; }

    [Required]
    [MaxLength(64)]
    public string NotificationType { get; set; }

    public virtual ApplicationUser TargetUser { get; set; }
}
