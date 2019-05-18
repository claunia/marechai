/******************************************************************************
// Canary Islands Computer Museum Website
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
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class Machine
    {
        public Machine()
        {
            Gpus       = new HashSet<GpusByMachine>();
            Memory     = new HashSet<MemoryByMachine>();
            Processors = new HashSet<ProcessorsByMachine>();
            Sound      = new HashSet<SoundByMachine>();
            Storage    = new HashSet<StorageByMachine>();
        }

        public int         Id         { get; set; }
        public int         CompanyId  { get; set; }
        public string      Name       { get; set; }
        public MachineType Type       { get; set; }
        public DateTime?   Introduced { get; set; }
        public int?        FamilyId   { get; set; }
        public string      Model      { get; set; }

        public virtual Company                          Company    { get; set; }
        public virtual MachineFamily                    Family     { get; set; }
        public virtual ICollection<GpusByMachine>       Gpus       { get; set; }
        public virtual ICollection<MemoryByMachine>     Memory     { get; set; }
        public virtual ICollection<ProcessorsByMachine> Processors { get; set; }
        public virtual ICollection<SoundByMachine>      Sound      { get; set; }
        public virtual ICollection<StorageByMachine>    Storage    { get; set; }

        [NotMapped]
        [DisplayName("Introduced")]
        public string IntroducedView =>
            Introduced == DateTime.MinValue ? "Prototype" : Introduced?.ToShortDateString() ?? "Unknown";
    }
}