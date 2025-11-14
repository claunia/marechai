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
using System.ComponentModel.DataAnnotations.Schema;
using Marechai.Data;

namespace Marechai.Database.Models;

public class Audit : BaseModel<long>
{
    public AuditType Type { get; set; }
    [Required]
    public string UserId { get; set; }
    public string Table { get;  set; }
    [Column(TypeName = "json")]
    public Dictionary<string, object> Keys { get; set; }
    [Column(TypeName = "json")]
    public Dictionary<string, object> OldValues { get; set; }
    [Column(TypeName = "json")]
    public Dictionary<string, object> NewValues { get; set; }
    [Column(TypeName = "json")]
    public List<string> AffectedColumns { get; set; }

    [Required]
    public virtual ApplicationUser User { get; set; }
}