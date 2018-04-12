/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Console.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     High level representation of a videogame console.
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
    public class Console
    {
        public int id;
        public int company;
        public string name;
        public int year;
        public int cpu1;
        public float mhz1;
        public int cpu2;
        public float mhz2;
        public int bits;
        public int ram;
        public int rom;
        public int gpu;
        public int vram;
        public string res;
        public int colors;
        public int palette;
        public int spu;
        public int schannels;
        public int mpu;
        public int mchannels;
        public int format;
        public int cap;
        public string comments;
    }
}