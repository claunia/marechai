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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public abstract class BaseScan : BaseModel<Guid>
    {
        public string Author { get; set; }
        [DisplayName("Color space")]
        public ColorSpace? ColorSpace { get; set; }
        [DisplayName("User comments")]
        public string Comments { get; set; }
        [DisplayName("Date and time of digitizing")]
        public DateTime? CreationDate { get; set; }
        [DisplayName("Exif version")]
        public string ExifVersion { get; set; }
        [DisplayName("Horizontal resolution")]
        public double? HorizontalResolution { get; set; }
        [DisplayName("Resolution unit")]
        public ResolutionUnit? ResolutionUnit { get; set; }
        [DisplayName("Scanner manufacturer")]
        public string ScannerManufacturer { get; set; }
        [DisplayName("Scanner model")]
        public string ScannerModel { get; set; }
        [DisplayName("Software used")]
        public string SoftwareUsed { get; set; }
        [Timestamp]
        public DateTime UploadDate { get; set; }
        [DisplayName("Vertical resolution")]
        public double? VerticalResolution { get; set; }
        public string OriginalExtension { get;   set; }

        public virtual ApplicationUser User { get; set; }

        public string UserId { get; set; }
    }
}