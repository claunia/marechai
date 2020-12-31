/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class BrowserTest : BaseModel<int>
    {
        [DisplayName("User agent"), Required, StringLength(128)]
        public string UserAgent { get; set; }
        [Required, StringLength(64)]
        public string Browser { get; set; }
        [Required, StringLength(16)]
        public string Version { get; set; }
        [DisplayName("Operating system"), Required, StringLength(32)]
        public string Os { get; set; }
        [Required, StringLength(8)]
        public string Platform { get; set; }
        [DisplayName("GIF87"), DefaultValue(false)]
        public bool Gif87 { get; set; }
        [DisplayName("GIF89"), DefaultValue(false)]
        public bool Gif89 { get; set; }
        [DisplayName("JPEG"), DefaultValue(false)]
        public bool Jpeg { get; set; }
        [DisplayName("PNG"), DefaultValue(false)]
        public bool Png { get; set; }
        [DisplayName("Transparent PNG"), DefaultValue(false)]
        public bool Pngt { get; set; }
        [DisplayName("Animated GIF"), DefaultValue(false)]
        public bool Agif { get; set; }
        [DefaultValue(false)]
        public bool Table { get; set; }
        [DefaultValue(false)]
        public bool Colors { get; set; }
        [DisplayName("JavaScript"), DefaultValue(false)]
        public bool Js { get; set; }
        [DefaultValue(false)]
        public bool Frames { get; set; }
        [DefaultValue(false)]
        public bool Flash { get; set; }
    }
}