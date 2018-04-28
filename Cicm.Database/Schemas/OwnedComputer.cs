/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : OwnComputer.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     High level representation of an owned computer.
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

namespace Cicm.Database.Schemas
{
    /// <summary>Owned computer</summary>
    public class OwnedComputer
    {
        /// <summary>Acquired date</summary>
        public string Acquired;
        /// <summary>Box present in collection</summary>
        public bool Boxed;
        /// <summary>Computer's ID</summary>
        public int ComputerId;
        /// <summary>Primary CPU</summary>
        public int Cpu1;
        /// <summary>Secondary CPU</summary>
        public int Cpu2;
        /// <summary>ID</summary>
        public int Id;
        /// <summary>Original manuals present in collection</summary>
        public bool Manuals;
        /// <summary>Frequency in MHz of primary CPU</summary>
        public float Mhz1;
        /// <summary>Frequency in MHz of secondary CPU</summary>
        public float Mhz2;
        /// <summary>Size in kibibytes of program RAM</summary>
        public int Ram;
        /// <summary>Rigid disk</summary>
        public string Rigid;
        /// <summary>Status</summary>
        public StatusType Status;
        /// <summary>Available for trade</summary>
        public bool Trade;
        /// <summary>Size in kibibytes for video RAM</summary>
        public int Vram;
    }
}