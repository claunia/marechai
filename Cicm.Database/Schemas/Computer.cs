﻿/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Computer.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     High level representation of a computer.
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
    /// <summary>Computer</summary>
    public class Computer
    {
        /// <summary>Bits of GPRs of primary CPU</summary>
        public int Bits;
        /// <summary>Capacity of first removable disk format</summary>
        public string Cap1;
        /// <summary>Capacity of second removable disk format</summary>
        public string Cap2;
        /// <summary>Maximum colors on screen</summary>
        public int Colors;
        /// <summary>Free-form comments</summary>
        public string Comment;
        /// <summary>Manufacturer's company ID</summary>
        public int Company;
        /// <summary>Primary CPU</summary>
        public int Cpu1;
        /// <summary>Secondary CPU</summary>
        public int Cpu2;
        /// <summary>ID of first removable disk format</summary>
        public int Disk1;
        /// <summary>ID of second removable disk format</summary>
        public int Disk2;
        /// <summary>ID of GPU</summary>
        public int Gpu;
        /// <summary>ID of first hard disk format</summary>
        public int Hdd1;
        /// <summary>ID of second hard disk format</summary>
        public int Hdd2;
        /// <summary>ID of third hard disk format</summary>
        public int Hdd3;
        /// <summary>ID</summary>
        public int Id;
        /// <summary>Frequency in MHz of primary CPU</summary>
        public float Mhz1;
        /// <summary>Frequency in MHz of secondary CPU</summary>
        public float Mhz2;
        /// <summary>Model name</summary>
        public string Model;
        /// <summary>ID of MPU</summary>
        public int Mpu;
        /// <summary>Audio channels supported by the MPU</summary>
        public int MusicChannels;
        /// <summary>Size in kibibytes of program RAM</summary>
        public int Ram;
        /// <summary>Resolution in WxH pixels</summary>
        public string Resolution;
        /// <summary>Size in kibibytes of firmware</summary>
        public int Rom;
        /// <summary>Audio channels supported by the DSP</summary>
        public int SoundChannels;
        /// <summary>ID of DSP</summary>
        public int Spu;
        /// <summary>Size in kibibytes for video RAM</summary>
        public int Vram;
        /// <summary>Introduction date, 0 if unknown</summary>
        public int Year;
    }
}