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
using Marechai.Data;

namespace Marechai.Database.Models;

public class SoftwareCover : BaseModel<Guid>
{
    // Nullable: a cover is anchored by exactly one of SoftwareId, SoftwareReleaseId, or
    // SoftwareCompilationId, depending on whether it belongs to a piece of Software, a
    // specific SoftwareRelease, or a SoftwareCompilation (which has no single owning release).
    public         ulong?   SoftwareId { get;              set; }
    public virtual Software Software { get; set; }

    public ulong? SoftwareReleaseId { get;              set; }
    public virtual SoftwareRelease Release { get; set; }

    public         ulong?               SoftwareCompilationId { get; set; }
    public virtual SoftwareCompilation  SoftwareCompilation   { get; set; }

    [StringLength(64)]
    public string GroupId { get; set; }

    [Required]
    public SoftwareCoverType Type { get; set; }

    public string Caption { get; set; }

    [Required]
    public string OriginalExtension { get; set; }
}
