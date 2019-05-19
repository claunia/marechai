/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Processor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Describes general purpose processors or application specific coprocessors
//     that are not strictly for graphic or sound generation. 
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
    public class Processor : BaseModel<int>
    {
        public Processor()
        {
            InstructionSetExtensions = new HashSet<InstructionSetExtensionsByProcessor>();
            ProcessorsByMachine      = new HashSet<ProcessorsByMachine>();
        }

        public string Name      { get; set; }
        public int?   CompanyId { get; set; }
        [DisplayName("Model code")]
        public string ModelCode { get;     set; }
        public DateTime? Introduced { get; set; }
        [DisplayName("Instruction set")]
        public int? InstructionSetId { get; set; }
        [DisplayName("Nominal speed (MHz)")]
        public double? Speed { get;  set; }
        public string Package { get; set; }
        [DisplayName("General Purpose Registers")]
        public int? Gprs { get; set; }
        [DisplayName("General Purporse Register size")]
        public int? GprSize { get; set; }
        [DisplayName("Floating Point Registers")]
        public int? Fprs { get; set; }
        [DisplayName("Floating Point Register Size")]
        public int? FprSize { get; set; }
        public int? Cores { get;   set; }
        [DisplayName("Threads per core")]
        public int? ThreadsPerCore { get; set; }
        public string Process { get;      set; }
        [DisplayName("Process (nm)")]
        public float? ProcessNm { get; set; }
        [DisplayName("Die size (mm²)")]
        public float? DieSize { get;    set; }
        public long? Transistors { get; set; }
        [DisplayName("Data bus size")]
        public int? DataBus { get; set; }
        [DisplayName("Address bus size")]
        public int? AddrBus { get; set; }
        [DisplayName("SIMD registers")]
        public int? SimdRegisters { get; set; }
        [DisplayName("SIMD register size")]
        public int? SimdSize { get; set; }
        [DisplayName("L1 instruction cache (KiB)")]
        public float? L1Instruction { get; set; }
        [DisplayName("L1 data cache (KiB)")]
        public float? L1Data { get; set; }
        [DisplayName("L2 cache (KiB)")]
        public float? L2 { get; set; }
        [DisplayName("L3 cache (KiB)")]
        public float? L3 { get; set; }

        public virtual Company Company { get; set; }
        [DisplayName("Instruction set")]
        public virtual InstructionSet InstructionSet { get;                                             set; }
        public virtual ICollection<InstructionSetExtensionsByProcessor> InstructionSetExtensions { get; set; }
        public virtual ICollection<ProcessorsByMachine>                 ProcessorsByMachine      { get; set; }

        [NotMapped]
        public string IntroducedView => Introduced?.ToShortDateString() ?? "Unknown";
    }
}