/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : BrowserTest.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     High level representation of a browser test.
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
    /// <summary>Browser test</summary>
    public class BrowserTest
    {
        /// <summary>Supports animated GIF</summary>
        public bool agif;
        /// <summary>Browser name</summary>
        public string browser;
        /// <summary>Supports colors</summary>
        public bool colors;
        /// <summary>Supports Adobe Flash</summary>
        public bool flash;
        /// <summary>Supports IFRAMEs</summary>
        public bool frames;
        /// <summary>Supports GIF87</summary>
        public bool gif87;
        /// <summary>Supports GIF with transparency (GIF89)</summary>
        public bool gif89;
        /// <summary>ID</summary>
        public ushort id;
        /// <summary>User agent string</summary>
        public string idstring;
        /// <summary>Supports JPEG</summary>
        public bool jpeg;
        /// <summary>Supports JavaScript</summary>
        public bool js;
        /// <summary>Browser operating system</summary>
        public string os;
        /// <summary>Browser architecture</summary>
        public string platform;
        /// <summary>Supports PNG</summary>
        public bool png;
        /// <summary>Supports PNG with alpha channel</summary>
        public bool pngt;
        /// <summary>Supports HTML tables</summary>
        public bool table;
        /// <summary>Browser version</summary>
        public string version;
    }
}