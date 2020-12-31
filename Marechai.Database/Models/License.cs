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

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class License : BaseModel<int>
    {
        [Required]
        public string Name { get; set; }
        [DisplayName("SPDX identifier")]
        public string SPDX { get; set; }
        [DisplayName("FSF approved"), Required, DefaultValue(false)]
        public bool FsfApproved { get; set; }
        [DisplayName("OSI approved"), Required, DefaultValue(false)]
        public bool OsiApproved { get; set; }
        [DisplayName("License text link"), StringLength(512), Url]
        public string Link { get; set; }
        [DisplayName("License text"), Column(TypeName = "longtext"), StringLength(131072),
         DataType(DataType.MultilineText)]
        public string Text { get;                                               set; }
        public virtual ICollection<MachinePhoto>      Photos             { get; set; }
        public virtual ICollection<OwnedMachinePhoto> OwnedMachinePhotos { get; set; }
    }
}