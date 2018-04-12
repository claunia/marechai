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
    public class OwnComputer
    {
        /// <summary>Box present in collection</summary>
        public bool boxed;
        /// <summary>Capacity of first removable disk format</summary>
        public int cap1;
        /// <summary>Capacity of second removable disk format</summary>
        public int cap2;
        /// <summary>Primary CPU</summary>
        public int cpu1;
        /// <summary>Secondary CPU</summary>
        public int cpu2;
        /// <summary>Adquired date</summary>
        public string date;
        /// <summary>Computer's ID</summary>
        public int db_id;
        /// <summary>ID of first removable disk format</summary>
        public int disk1;
        /// <summary>ID of second removable disk format</summary>
        public int disk2;
        /// <summary>ID</summary>
        public int id;
        /// <summary>Original manuals present in collection</summary>
        public bool manuals;
        /// <summary>Frequency in MHz of primary CPU</summary>
        public float mhz1;
        /// <summary>Frequency in MHz of secondary CPU</summary>
        public float mhz2;
        /// <summary>Size in kibibytes of program RAM</summary>
        public int ram;
        /// <summary>Rigid disk</summary>
        public string rigid;
        /// <summary>Status</summary>
        public int status;
        /// <summary>Available for trade</summary>
        public bool trade;
        /// <summary>Size in kibibytes for video RAM</summary>
        public int vram;
    }
}