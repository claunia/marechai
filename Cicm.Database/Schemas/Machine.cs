/******************************************************************************
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
    public class Machine
    {
        /// <summary>Capacity of first removable disk format</summary>
        public string Cap1;
        /// <summary>Capacity of second removable disk format</summary>
        public string Cap2;
        /// <summary>Maximum colors on screen</summary>
        public int Colors;
        /// <summary>Manufacturer's company ID</summary>
        public int Company;
        /// <summary>ID of first removable disk format</summary>
        public int Disk1;
        /// <summary>ID of second removable disk format</summary>
        public int Disk2;
        /// <summary>ID of first hard disk format</summary>
        public int Hdd1;
        /// <summary>ID of second hard disk format</summary>
        public int Hdd2;
        /// <summary>ID of third hard disk format</summary>
        public int Hdd3;
        /// <summary>ID</summary>
        public int Id;
        /// <summary>Model name</summary>
        public string Model;
        /// <summary>Audio channels supported by the MPU</summary>
        public int MusicChannels;
        /// <summary>ID of MPU</summary>
        public int MusicSynth;
        /// <summary>Size in kibibytes of program RAM</summary>
        public int Ram;
        /// <summary>Resolution in WxH pixels</summary>
        public string Resolution;
        /// <summary>Size in kibibytes of firmware</summary>
        public int Rom;
        /// <summary>Audio channels supported by the DSP</summary>
        public int SoundChannels;
        /// <summary>ID of DSP</summary>
        public int SoundSynth;
        /// <summary>Machine type</summary>
        public MachineType Type;
        /// <summary>Size in kibibytes for video RAM</summary>
        public int Vram;
        /// <summary>Introduction date, 0 if unknown, 1000 if prototype</summary>
        public int Year;
    }
}