/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : MemoryByMachine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Junction between memory and machine.
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

using System.ComponentModel;
using Cicm.Database;

namespace cicm_web.Areas.Admin.Models
{
    public class MemoryByMachineViewModel : BaseViewModel<long>
    {
        public MemoryType  Type  { get; set; }
        public MemoryUsage Usage { get; set; }
        public long?       Size  { get; set; }
        [DisplayName("Speed (Hz)")]
        public double? Speed { get; set; }

        public string Machine { get; set; }
    }
}