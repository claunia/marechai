﻿/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class Processor : BaseModel<int>
    {
        public Processor()
        {
            InstructionSetExtensions = new HashSet<InstructionSetExtensionsByProcessor>();
            ProcessorsByMachine      = new HashSet<ProcessorsByMachine>();
        }

        [Required, StringLength(50)]
        public string Name { get;    set; }
        public int? CompanyId { get; set; }
        [DisplayName("Model code"), StringLength(45)]
        public string ModelCode { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}"), DataType(DataType.Date)]
        public DateTime? Introduced { get; set; }
        [DisplayName("Instruction set")]
        public int? InstructionSetId { get; set; }
        [DisplayName("Nominal speed (MHz)")]
        public double? Speed { get; set; }
        [StringLength(45)]
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
        [StringLength(45)]
        public string Process { get; set; }
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
        public virtual ICollection<ProcessorsBySoftwareVariant>         Software                 { get; set; }
    }
}