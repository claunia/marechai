/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class SoundSynth : BaseModel<int>
    {
        public SoundSynth()
        {
            SoundByMachine = new HashSet<SoundByMachine>();
        }

        [Required]
        [StringLength(50)]
        public string Name { get;    set; }
        public int? CompanyId { get; set; }
        [DisplayName("Model code")]
        [StringLength(45)]
        public string ModelCode { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}")]
        [DataType(DataType.Date)]
        public DateTime? Introduced { get; set; }
        [DisplayName("PCM voices")]
        [Range(1, int.MaxValue)]
        public int? Voices { get; set; }
        [DisplayName("Sample rate (Hz)")]
        public double? Frequency { get; set; }
        [DisplayName("Sample resolution")]
        [Range(1, int.MaxValue)]
        public int? Depth { get; set; }
        [DisplayName("Square wave channels")]
        [Range(1, int.MaxValue)]
        public int? SquareWave { get; set; }
        [DisplayName("White noise channels")]
        [Range(1, int.MaxValue)]
        public int? WhiteNoise { get; set; }
        public int? Type { get;       set; }

        public virtual Company                     Company        { get; set; }
        public virtual ICollection<SoundByMachine> SoundByMachine { get; set; }

        [NotMapped]
        public string IntroducedView => Introduced?.ToShortDateString() ?? "Unknown";
    }
}