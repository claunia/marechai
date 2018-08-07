/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Gpu.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Describes chips (or chipsets) whose primary function is to generate
//     graphics (raster, vectorial, 3D, etc).
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
using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Gpu
    {
        public Gpu()
        {
            GpusByMachine    = new HashSet<GpusByMachine>();
            ResolutionsByGpu = new HashSet<ResolutionsByGpu>();
        }

        public int       Id          { get; set; }
        public string    Name        { get; set; }
        public int?      CompanyId   { get; set; }
        public string    ModelCode   { get; set; }
        public DateTime? Introduced  { get; set; }
        public string    Package     { get; set; }
        public string    Process     { get; set; }
        public float?    ProcessNm   { get; set; }
        public float?    DieSize     { get; set; }
        public long?     Transistors { get; set; }

        public virtual Company                       Company          { get; set; }
        public virtual ICollection<GpusByMachine>    GpusByMachine    { get; set; }
        public virtual ICollection<ResolutionsByGpu> ResolutionsByGpu { get; set; }
    }
}