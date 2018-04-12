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
        /// <summary>Supports PNG with alpha channel</summary>
        public bool AlphaPng;
        /// <summary>Supports animated GIF</summary>
        public bool AnimatedGif;
        /// <summary>Browser architecture</summary>
        public string Architecture;
        /// <summary>Supports colors</summary>
        public bool Color;
        /// <summary>Supports Adobe Flash</summary>
        public bool Flash;
        /// <summary>Supports IFRAMEs</summary>
        public bool Frames;
        /// <summary>Supports GIF87</summary>
        public bool Gif87;
        /// <summary>Supports GIF with transparency (GIF89)</summary>
        public bool Gif89;
        /// <summary>ID</summary>
        public ushort Id;
        /// <summary>Supports JPEG</summary>
        public bool Jpeg;
        /// <summary>Supports JavaScript</summary>
        public bool Js;
        /// <summary>Browser name</summary>
        public string Name;
        /// <summary>Browser operating system</summary>
        public string OperatingSystem;
        /// <summary>Supports PNG</summary>
        public bool Png;
        /// <summary>Supports HTML tables</summary>
        public bool Tables;
        /// <summary>User agent string</summary>
        public string UserAgent;
        /// <summary>Browser version</summary>
        public string Version;
    }
}