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

public class MessageReport : BaseModel<long>
{
    public string ReporterId { get; set; }

    /// <summary>FK to the reported message; nullable (SetNull) so the report survives if the message is deleted.</summary>
    public long? MessageId { get; set; }

    public ReviewReportReason Reason { get; set; }

    [MaxLength(2048)]
    public string Explanation { get; set; }

    public bool IsResolved { get; set; }

    public string   ResolvedByUserId { get; set; }
    public DateTime? ResolvedOn       { get; set; }

    public virtual ApplicationUser Reporter   { get; set; }
    public virtual Message         Message    { get; set; }
    public virtual ApplicationUser ResolvedBy { get; set; }
}
