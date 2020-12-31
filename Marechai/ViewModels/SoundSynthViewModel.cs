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

using System;

namespace Marechai.ViewModels
{
    public class SoundSynthViewModel : BaseViewModel<int>
    {
        public string    Name        { get; set; }
        public string    CompanyName { get; set; }
        public int?      CompanyId   { get; set; }
        public string    ModelCode   { get; set; }
        public DateTime? Introduced  { get; set; }
        public int?      Voices      { get; set; }
        public double?   Frequency   { get; set; }
        public int?      Depth       { get; set; }
        public int?      SquareWave  { get; set; }
        public int?      WhiteNoise  { get; set; }
        public int?      Type        { get; set; }

        public string IntroducedView => Introduced?.ToShortDateString() ?? "Unknown";
    }
}