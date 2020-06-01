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

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class Screen : BaseModel<int>
    {
        [Range(1, 131072), DisplayName("Width (mm)")]
        public double? Width { get; set; }
        [Range(1, 131072), DisplayName("Height (mm)")]
        public double? Height { get; set; }
        [Required, DisplayName("Diagonal (inches)")]
        public double Diagonal { get; set; }
        [DisplayName("Native resolution")]
        public virtual Resolution NativeResolution { get; set; }
        [Range(2, 281474976710656), DisplayName("Effective colors")]
        public long? EffectiveColors { get; set; }
        [Required]
        public string Type { get; set; }

        public virtual ICollection<ResolutionsByScreen> Resolutions       { get; set; }
        public virtual ICollection<ScreensByMachine>    ScreensByMachines { get; set; }
        [Required]
        public int NativeResolutionId { get; set; }
    }
}