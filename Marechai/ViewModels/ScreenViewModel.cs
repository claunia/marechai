using System.ComponentModel.DataAnnotations.Schema;

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

        [NotMapped]
        public long? Colors => EffectiveColors ?? NativeResolution.Colors;

        [NotMapped]
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