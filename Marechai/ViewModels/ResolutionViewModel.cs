using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Marechai.ViewModels
{
    public class ResolutionViewModel : BaseViewModel<int>
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public long? Colors { get; set; }
        public long? Palette { get; set; }
        public bool Chars { get; set; }
        public bool Grayscale { get; set; }

        public long? PaletteView => Palette ?? Colors;

        public override string ToString()
        {
            if(Chars)
            {
                if(Colors == null)
                    return$"{Width}x{Height} characters";

                if(Palette != null &&
                   Colors  != Palette)
                    return Grayscale ? $"{Width}x{Height} characters at {Colors} grays from a palette of {Palette}"
                               : $"{Width}x{Height} characters at {Colors} colors from a palette of {Palette}";

                return Colors == 2 && Grayscale ? $"{Width}x{Height} black and white characters"
                           : $"{Width}x{Height} characters at {Colors} colors";
            }

            if(Colors == null)
                return$"{Width}x{Height} pixels";

            if(Palette != null &&
               Colors  != Palette)
                return Grayscale ? $"{Width}x{Height} pixels at {Colors} grays from a palette of {Palette}"
                           : $"{Width}x{Height} pixels at {Colors} colors from a palette of {Palette}";

            return Colors == 2 && Grayscale ? $"{Width}x{Height} black and white pixels"
                       : $"{Width}x{Height} pixels at {Colors} colors";
        }
    }
}