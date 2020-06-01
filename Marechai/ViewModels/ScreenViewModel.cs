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

namespace Marechai.ViewModels
{
    public class ScreenViewModel : BaseViewModel<int>
    {
        public double?             Width              { get; set; }
        public double?             Height             { get; set; }
        public double              Diagonal           { get; set; }
        public int                 NativeResolutionId { get; set; }
        public ResolutionViewModel NativeResolution   { get; set; }
        public long?               EffectiveColors    { get; set; }
        public string              Type               { get; set; }

        public long? Colors => EffectiveColors ?? NativeResolution.Colors;

        public string Size
        {
            get
            {
                if(Width  != null &&
                   Height != null)
                    return$"{Width}x{Height} mm";

                return"Unknown";
            }
        }
    }
}