/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Processor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     High level representation of a processor .
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

namespace Cicm.Database.Schemas
{
    /// <summary>Processor</summary>
    public class Processor
    {
        /// <summary>Size in bits of address bus with host (not interprocessor)</summary>
        public int AddressBus;
        /// <summary>Company</summary>
        public Company Company;
        /// <summary>How many processor cores per processor package</summary>
        public int Cores;
        /// <summary>Size in bits of data bus with host (not interprocessor)</summary>
        public int DataBus;
        /// <summary>Size of die in square milimeters</summary>
        public float DieSize;
        /// <summary>Number of available Floating Point Registers</summary>
        public int Fpr;
        /// <summary>Size in bits of FPRs</summary>
        public int FprSize;
        /// <summary>Number of available General Purpose Registers</summary>
        public int Gpr;
        /// <summary>Size in bits of GPRs</summary>
        public int GprSize;
        /// <summary>ID</summary>
        public int Id;
        /// <summary>Instruction set</summary>
        public InstructionSet InstructionSet;
        /// <summary>Extensions to the instruction set that are implemented in this processor</summary>
        public InstructionSetExtension[] InstructionSetExtensions;
        /// <summary>Datetime of introduction</summary>
        public DateTime Introduced;
        /// <summary>Size in kibibytes of L1 data cache. If -1, <see cref="L1Instruction" /> is size of L1 unified cache</summary>
        public float L1Data;
        /// <summary>Size in kibibytes of L1 instruction cache. If <see cref="L1Data" /> is -1, this is size of L1 unified cache</summary>
        public float L1Instruction;
        /// <summary>
        ///     Size in kibibytes of L2 cache. It includes cache that's in same physical package but not in same chip die
        ///     (e.g. Pentium II)
        /// </summary>
        public float L2;
        /// <summary>Size in kibibytes of L3 cache</summary>
        public float L3;
        /// <summary>Model/SKU code</summary>
        public string ModelCode;
        /// <summary>Name</summary>
        public string Name;
        /// <summary>Package</summary>
        public string Package;
        /// <summary>Name of litography process</summary>
        public string Process;
        /// <summary>Nanometers of litography process</summary>
        public float ProcessNm;
        /// <summary>Number of available SIMD registers</summary>
        public int Simd;
        /// <summary>Size in bits of SIMD registers</summary>
        public int SimdSize;
        /// <summary>Nominal speed, in MHz</summary>
        public double Speed;
        /// <summary>How many simultaneos threads can run on each processor core</summary>
        public int ThreadsPerCore;
        /// <summary>How many transistors in package</summary>
        public ulong Transistors;
    }
}