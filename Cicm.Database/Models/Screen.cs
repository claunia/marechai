using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class Screen : BaseModel<int>
    {
        [Range(1, 131072)]
        [DisplayName("Width (mm)")]
        public double? Width { get; set; }
        [Range(1, 131072)]
        [DisplayName("Height (mm)")]
        public double? Height { get; set; }
        [Required]
        [DisplayName("Diagonal (inches)")]
        public double Diagonal { get; set; }
        [DisplayName("Native resolution")]
        public virtual Resolution NativeResolution { get; set; }
        [Range(2, 281474976710656)]
        [DisplayName("Effective colors")]
        public long? EffectiveColors { get; set; }
        [Required]
        public string Type { get; set; }

        [NotMapped]
        public long? Colors => EffectiveColors ?? NativeResolution.Colors;

        [NotMapped]
        public string Size
        {
            get
            {
                if(Width != null && Height != null) return $"{Width}x{Height} mm";

                return "Unknown";
            }
        }

        public virtual ICollection<ResolutionsByScreen> Resolutions       { get; set; }
        public virtual ICollection<ScreensByMachine>    ScreensByMachines { get; set; }
        [Required]
        public int NativeResolutionId { get; set; }
    }
}