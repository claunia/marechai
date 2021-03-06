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

namespace Marechai.ViewModels
{
    public class ResolutionViewModel : BaseViewModel<int>
    {
        public int   Width     { get; set; }
        public int   Height    { get; set; }
        public long? Colors    { get; set; }
        public long? Palette   { get; set; }
        public bool  Chars     { get; set; }
        public bool  Grayscale { get; set; }

        public long? PaletteView => Palette ?? Colors;

        public override string ToString()
        {
            if(Chars)
            {
                if(Colors == null)
                    return $"{Width}x{Height} characters";

                if(Palette != null &&
                   Colors  != Palette)
                    return Grayscale ? $"{Width}x{Height} characters at {Colors} grays from a palette of {Palette}"
                               : $"{Width}x{Height} characters at {Colors} colors from a palette of {Palette}";

                return Colors == 2 && Grayscale ? $"{Width}x{Height} black and white characters"
                           : $"{Width}x{Height} characters at {Colors} colors";
            }

            if(Colors == null)
                return $"{Width}x{Height} pixels";

            if(Palette != null &&
               Colors  != Palette)
                return Grayscale ? $"{Width}x{Height} pixels at {Colors} grays from a palette of {Palette}"
                           : $"{Width}x{Height} pixels at {Colors} colors from a palette of {Palette}";

            return Colors == 2 && Grayscale ? $"{Width}x{Height} black and white pixels"
                       : $"{Width}x{Height} pixels at {Colors} colors";
        }
    }
}