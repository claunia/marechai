/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : Machine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Describes a machine.
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
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class Machine : BaseModel<int>
    {
        public Machine()
        {
            Gpus       = new HashSet<GpusByMachine>();
            Memory     = new HashSet<MemoryByMachine>();
            Processors = new HashSet<ProcessorsByMachine>();
            Sound      = new HashSet<SoundByMachine>();
            Storage    = new HashSet<StorageByMachine>();
        }

        [Required]
        public int CompanyId { get; set; }
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        [Required]
        public MachineType Type { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}")]
        [DataType(DataType.Date)]
        public DateTime? Introduced { get; set; }
        public int? FamilyId { get;        set; }
        [StringLength(50)]
        public string Model { get; set; }

        public virtual Company                          Company    { get; set; }
        public virtual MachineFamily                    Family     { get; set; }
        public virtual ICollection<GpusByMachine>       Gpus       { get; set; }
        public virtual ICollection<MemoryByMachine>     Memory     { get; set; }
        public virtual ICollection<ProcessorsByMachine> Processors { get; set; }
        public virtual ICollection<SoundByMachine>      Sound      { get; set; }
        public virtual ICollection<StorageByMachine>    Storage    { get; set; }
        public virtual ICollection<MachinePhoto>        Photos     { get; set; }
        public virtual ICollection<ScreensByMachine>    Screens    { get; set; }
        public virtual ICollection<DocumentsByMachine>  Documents  { get; set; }
        public virtual ICollection<BooksByMachine>      Books      { get; set; }
        public virtual ICollection<MagazinesByMachine>  Magazines  { get; set; }

        [NotMapped]
        [DisplayName("Introduced")]
        public string IntroducedView =>
            Introduced == DateTime.MinValue ? "Prototype" : Introduced?.ToShortDateString() ?? "Unknown";
    }
}