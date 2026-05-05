using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class SoftwareRelease : BaseModel<ulong>
{
    public string Title { get; set; }

    public bool IsCompilation { get; set; }

    public         ulong?   SoftwareId { get; set; }
    public virtual Software Software   { get; set; }

    public         ulong?          SoftwareVersionId { get; set; }
    public virtual SoftwareVersion SoftwareVersion   { get; set; }

    public         ulong?           PlatformId { get; set; }
    public virtual SoftwarePlatform Platform   { get; set; }

    public virtual ICollection<UnM49BySoftwareRelease>     Regions   { get; set; }
    public virtual ICollection<LanguageBySoftwareRelease>  Languages { get; set; }

    [Required]
    public int PublisherId { get;           set; }
    public virtual Company Publisher { get; set; }

    public DateTime? ReleaseDate { get; set; }
    [DefaultValue(DatePrecision.Full)]
    public DatePrecision ReleaseDatePrecision { get; set; }

    public virtual ICollection<SoftwareBarcode>                     Barcodes             { get; set; }
    public virtual ICollection<SoftwareProductCode>                 ProductCodes         { get; set; }
    public virtual ICollection<MinimumGpuBySoftwareRelease>         MinimumGpus          { get; set; }
    public virtual ICollection<RecommendedGpuBySoftwareRelease>     RecommendedGpus      { get; set; }
    public virtual ICollection<SoundSynthBySoftwareRelease>         SupportedSoundSynths { get; set; }
    public virtual ICollection<SoftwareVersionBySoftwareRelease>    IncludedVersions     { get; set; }
    public virtual ICollection<SoftwareBySoftwareRelease>           IncludedSoftware     { get; set; }
    public virtual ICollection<CollectedSoftwareRelease>            CollectedBy          { get; set; }
    public virtual ICollection<SoftwareAttribute>                   Attributes           { get; set; }
    public virtual ICollection<SoftwareCover>                       Covers               { get; set; }
}