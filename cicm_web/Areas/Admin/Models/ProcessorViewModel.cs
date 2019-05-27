/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Processor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Describes general purpose processors or application specific coprocessors
//     that are not strictly for graphic or sound generation. 
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cicm_web.Areas.Admin.Models
{
    public class ProcessorViewModel : BaseViewModel<int>
    {
        public string Name    { get; set; }
        public string Company { get; set; }
        [DisplayName("Model code")]
        public string ModelCode { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}")]
        [DataType(DataType.Date)]
        public DateTime? Introduced { get; set; }
        [DisplayName("Instruction set")]
        public string InstructionSet { get; set; }

        [NotMapped]
        public string IntroducedView => Introduced?.ToShortDateString() ?? "Unknown";
    }
}