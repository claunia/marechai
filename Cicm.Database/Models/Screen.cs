using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class Screen : BaseModel<int>
    {
        [Range(1, 131072)]
        public double? Width { get; set; }
        [Range(1, 131072)]
        public double? Height { get; set; }
        [Required]
        public double Diagonal { get; set; }
        [Required]
        public virtual Resolution NativeResolution { get; set; }
        [Range(2, 281474976710656)]
        public long? EffectiveColors { get; set; }
        [Required]
        public string Type { get; set; }

        [NotMapped]
        public long? Colors => EffectiveColors ?? NativeResolution.Colors;

        public virtual ICollection<ResolutionsByScreen> Resolutions       { get; set; }
        public virtual ICollection<ScreensByMachine>    ScreensByMachines { get; set; }
    }
}