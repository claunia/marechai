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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Cicm.Database.Models
{
    public class BrowserTest : BaseModel<int>
    {
        [DisplayName("User agent")]
        [Required]
        [StringLength(128)]
        public string UserAgent { get; set; }
        [Required]
        [StringLength(64)]
        public string Browser { get; set; }
        [Required]
        [StringLength(16)]
        public string Version { get; set; }
        [DisplayName("Operating system")]
        [Required]
        [StringLength(32)]
        public string Os { get; set; }
        [Required]
        [StringLength(8)]
        public string Platform { get; set; }
        [DisplayName("GIF87")]
        public bool Gif87 { get; set; }
        [DisplayName("GIF89")]
        public bool Gif89 { get; set; }
        [DisplayName("JPEG")]
        public bool Jpeg { get; set; }
        [DisplayName("PNG")]
        public bool Png { get; set; }
        [DisplayName("Transparent PNG")]
        public bool Pngt { get; set; }
        [DisplayName("Animated GIF")]
        public bool Agif { get;   set; }
        public bool Table  { get; set; }
        public bool Colors { get; set; }
        [DisplayName("JavaScript")]
        public bool Js { get;     set; }
        public bool Frames { get; set; }
        public bool Flash  { get; set; }
    }
}