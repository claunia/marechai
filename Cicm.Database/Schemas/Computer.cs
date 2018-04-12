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
    public class Computer
    {
        public int id;
        public int company;
        public int year;
        public string model;
        public int cpu1;
        public float mhz1;
        public int cpu2;
        public float mhz2;
        public int bits;
        public int ram;
        public int rom;
        public int gpu;
        public int vram;
        public int colors;
        public string res;
        public int spu;
        public int mpu;
        public int sound_channels;
        public int music_channels;
        public int hdd1;
        public int hdd2;
        public int hdd3;
        public int disk1;
        public string cap1;
        public int disk2;
        public string cap2;
        public string comment;
    }
}