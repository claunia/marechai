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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class MagazineIssue : BaseModel<long>
    {
        [Required]
        public long MagazineId { get; set; }
        [Required]
        public string Caption { get;       set; }
        public string NativeCaption { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}"), DataType(DataType.Date)]
        public DateTime? Published { get; set; }
        [StringLength(18)]
        public string ProductCode { get; set; }
        public short? Pages       { get; set; }
        public uint?  IssueNumber { get; set; }

        public virtual Magazine                              Magazine        { get; set; }
        public virtual ICollection<PeopleByMagazine>         People          { get; set; }
        public virtual ICollection<MagazinesByMachine>       Machines        { get; set; }
        public virtual ICollection<MagazinesByMachineFamily> MachineFamilies { get; set; }
        public virtual ICollection<Media>                    Coverdiscs      { get; set; }
    }
}