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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class DumpHardware : BaseModel<ulong>
    {
        [StringLength(48)]
        public string Manufacturer { get; set; }
        [StringLength(48), Required]
        public string Model { get; set; }
        [StringLength(48)]
        public string Revision { get; set; }
        [StringLength(32)]
        public string Firmware { get; set; }
        [StringLength(64)]
        public string Serial { get; set; }
        [StringLength(64), Required]
        public string SoftwareName { get; set; }
        [StringLength(32)]
        public string SoftwareVersion { get; set; }
        [StringLength(64)]
        public string SoftwareOperatingSystem { get; set; }
        [Required, Column(TypeName = "json")]
        public Extent[] Extents { get; set; }
        [Required]
        public virtual Dump Dump { get; set; }
    }
}