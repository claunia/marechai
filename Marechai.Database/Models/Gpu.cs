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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class Gpu : BaseModel<int>
    {
        public Gpu()
        {
            GpusByMachine    = new HashSet<GpusByMachine>();
            ResolutionsByGpu = new HashSet<ResolutionsByGpu>();
        }

        [Required, StringLength(128)]
        public string Name { get;    set; }
        public int? CompanyId { get; set; }
        [DisplayName("Model code"), StringLength(45)]
        public string ModelCode { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}"), DataType(DataType.Date)]
        public DateTime? Introduced { get; set; }
        [StringLength(45)]
        public string Package { get; set; }
        [StringLength(45)]
        public string Process { get; set; }
        [DisplayName("Process (nm)")]
        public float? ProcessNm { get; set; }
        [DisplayName("Die size (mm²)")]
        public float? DieSize { get; set; }
        [Range(1, long.MaxValue)]
        public long? Transistors { get; set; }

        public virtual Company                       Company          { get; set; }
        public virtual ICollection<GpusByMachine>    GpusByMachine    { get; set; }
        public virtual ICollection<ResolutionsByGpu> ResolutionsByGpu { get; set; }

        [NotMapped]
        public string IntroducedView => Introduced?.ToShortDateString() ?? "Unknown";
    }
}