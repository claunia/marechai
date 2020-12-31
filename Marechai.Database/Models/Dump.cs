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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class Dump : BaseModel<ulong>
    {
        [Required]
        public string Dumper { get;          set; }
        public string    UserId       { get; set; }
        public string    DumpingGroup { get; set; }
        public DateTime? DumpDate     { get; set; }
        [Required]
        public virtual Media Media { get; set; }
        [Required]
        public virtual MediaDump MediaDump { get; set; }

        public virtual ApplicationUser           User         { get; set; }
        public virtual ICollection<DumpHardware> DumpHardware { get; set; }
        public         ulong                     MediaId      { get; set; }
        public         ulong                     MediaDumpId  { get; set; }
    }
}