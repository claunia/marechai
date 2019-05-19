﻿/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : SoundSynth.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Describes chips that generate sound.
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class SoundSynth : BaseModel<int>
    {
        public SoundSynth()
        {
            SoundByMachine = new HashSet<SoundByMachine>();
        }

        public string Name      { get; set; }
        public int?   CompanyId { get; set; }
        [DisplayName("Model code")]
        public string ModelCode { get;     set; }
        public DateTime? Introduced { get; set; }
        [DisplayName("PCM voices")]
        public int? Voices { get; set; }
        [DisplayName("Sample rate (Hz)")]
        public double? Frequency { get; set; }
        [DisplayName("Sample resolution")]
        public int? Depth { get; set; }
        [DisplayName("Square wave channels")]
        public int? SquareWave { get; set; }
        [DisplayName("White noise channels")]
        public int? WhiteNoise { get; set; }
        public int? Type { get;       set; }

        public virtual Company                     Company        { get; set; }
        public virtual ICollection<SoundByMachine> SoundByMachine { get; set; }

        [NotMapped]
        public string IntroducedView => Introduced?.ToShortDateString() ?? "Unknown";
    }
}