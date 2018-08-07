/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Resolution.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Image resolution.
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

using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Resolution
    {
        public Resolution()
        {
            ResolutionsByGpu = new HashSet<ResolutionsByGpu>();
        }

        public int   Id      { get; set; }
        public int   Width   { get; set; }
        public int   Height  { get; set; }
        public long? Colors  { get; set; }
        public long? Palette { get; set; }
        public sbyte Chars   { get; set; }

        public virtual ICollection<ResolutionsByGpu> ResolutionsByGpu { get; set; }
    }
}