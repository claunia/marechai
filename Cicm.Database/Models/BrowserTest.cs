/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : BrowserTest.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains browser tests.
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
    public class BrowserTest
    {
        public int    Id        { get; set; }
        public string UserAgent { get; set; }
        public string Browser   { get; set; }
        public string Version   { get; set; }
        public string Os        { get; set; }
        public string Platform  { get; set; }
        public bool   Gif87     { get; set; }
        public bool   Gif89     { get; set; }
        public bool   Jpeg      { get; set; }
        public bool   Png       { get; set; }
        public bool   Pngt      { get; set; }
        public bool   Agif      { get; set; }
        public bool   Table     { get; set; }
        public bool   Colors    { get; set; }
        public bool   Js        { get; set; }
        public bool   Frames    { get; set; }
        public bool   Flash     { get; set; }
    }
}