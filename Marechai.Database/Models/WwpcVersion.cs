using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class WwpcVersion : BaseModel<long>
{
    [Required]
    public long WwpcSoftwareId { get; set; }
    public virtual WwpcSoftware WwpcSoftware { get; set; }

    /// <summary>Major release label as shown on WinWorldPC (e.g. "1.x", "3.0", "Windows 95").</summary>
    [Required]
    [StringLength(64)]
    public string MajorRelease { get; set; }

    /// <summary>Canonical URL of the major release page (e.g. <c>/product/adobe-illustrator/1x</c>).</summary>
    [StringLength(1024)]
    public string MajorReleaseUrl { get; set; }

    /// <summary>
    ///     Minor release label as shown in the Downloads table (e.g. "1.0 for Windows", "3.2 for DOS").
    ///     Combined with <see cref="MajorRelease" /> at promotion time to form the final
    ///     <c>SoftwareVersion.VersionString</c>.
    /// </summary>
    [Required]
    [StringLength(128)]
    public string VersionString { get; set; }

    [StringLength(64)]
    public string Language { get; set; }

    [StringLength(64)]
    public string Architecture { get; set; }

    /// <summary>Media kind chip text (e.g. "3½ Floppy", "5¼ Floppy", "CD-ROM").</summary>
    [StringLength(128)]
    public string MediaKind { get; set; }

    /// <summary>Size text as printed in the Downloads table (e.g. "3MB").</summary>
    [StringLength(32)]
    public string SizeText { get; set; }

    [StringLength(2048)]
    public string DownloadUrl { get; set; }

    public bool IsEnabledByDefault { get; set; } = true;

    public ulong? PromotedSoftwareVersionId { get; set; }
}
