﻿/******************************************************************************
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

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class Resolution : BaseModel<int>
    {
        public Resolution() => ResolutionsByGpu = new HashSet<ResolutionsByGpu>();

        [Required, Range(1, 131072)]
        public int Width { get; set; }
        [Required, Range(1, 131072)]
        public int Height { get; set; }
        [Range(2, 281474976710656)]
        public long? Colors { get; set; }
        [Range(2, 281474976710656)]
        public long? Palette { get; set; }
        [DisplayName("Character based"), DefaultValue(false)]
        public bool Chars { get; set; }
        [DisplayName("Grayscale"), DefaultValue(false)]
        public bool Grayscale { get; set; }

        public virtual ICollection<ResolutionsByGpu>    ResolutionsByGpu    { get; set; }
        public virtual ICollection<ResolutionsByScreen> ResolutionsByScreen { get; set; }
        public virtual ICollection<Screen>              Screens             { get; set; }

        public long? PaletteView => Palette ?? Colors;
    }
}