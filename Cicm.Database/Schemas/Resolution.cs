﻿/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Processor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     High level representation of a processor .
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
    /// <summary>Processor</summary>
    public class Resolution
    {
        /// <summary>If <c>true</c> width and height indicate characters, else they indicate pixels</summary>
        public bool Chars;
        /// <summary>Colors</summary>
        public long Colors;
        /// <summary>Height</summary>
        public int Height;
        /// <summary>Resolution ID</summary>
        public int Id;
        /// <summary>Palette, 0 if same as <see cref="Colors" /></summary>
        public long Palette;
        /// <summary>Width</summary>
        public int Width;
    }
}