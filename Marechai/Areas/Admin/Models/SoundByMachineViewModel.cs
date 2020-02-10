/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Filename       : SoundByMachine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Junction of sound synth and machines.
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

namespace Marechai.Areas.Admin.Models
{
    public class SoundByMachineViewModel : BaseViewModel<long>
    {
        public string Machine { get; set; }
        [DisplayName("Sound synthetizer")]
        public string SoundSynth { get; set; }
    }
}