/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : OwnedComputer.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Describes a computer owned by a user.
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

namespace Cicm.Database.Models
{
    public class OwnedComputer : BaseModel<int>
    {
        public int        DbId    { get; set; }
        public string     Date    { get; set; }
        public StatusType Status  { get; set; }
        public int        Trade   { get; set; }
        public int        Boxed   { get; set; }
        public int        Manuals { get; set; }
        public int        Cpu1    { get; set; }
        public decimal    Mhz1    { get; set; }
        public int        Cpu2    { get; set; }
        public decimal    Mhz2    { get; set; }
        public int        Ram     { get; set; }
        public int        Vram    { get; set; }
        public string     Rigid   { get; set; }
        public int        Disk1   { get; set; }
        public int        Cap1    { get; set; }
        public int        Disk2   { get; set; }
        public int        Cap2    { get; set; }
    }
}