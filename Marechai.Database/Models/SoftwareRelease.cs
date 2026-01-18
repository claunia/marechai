using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class SoftwareRelease : BaseModel<ulong>
{
    public         ulong           SoftwareVersionId { get; set; }
    public virtual SoftwareVersion SoftwareVersion   { get; set; }

    public         ulong?          VariantId { get; set; }
    public virtual SoftwareVariant Variant   { get; set; }

    public         ulong?             SubvariantId { get; set; }
    public virtual SoftwareSubvariant Subvariant   { get; set; }

    public         ulong?           PlatformId { get; set; }
    public virtual SoftwarePlatform Platform   { get; set; }

    [Required]
    public short RegionId { get;                 set; } // ISO 3166-1 numeric
    public virtual Iso31661Numeric Region { get; set; }

    [Required]
    public int PublisherId { get;           set; }
    public virtual Company Publisher { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public virtual ICollection<SoftwareBarcode>                 Barcodes             { get; set; }
    public virtual ICollection<SoftwareProductCode>             ProductCodes         { get; set; }
    public virtual ICollection<MinimumGpuBySoftwareRelease>     MinimumGpus          { get; set; }
    public virtual ICollection<RecommendedGpuBySoftwareRelease> RecommendedGpus      { get; set; }
    public virtual ICollection<SoundSynthBySoftwareRelease>     SupportedSoundSynths { get; set; }
}