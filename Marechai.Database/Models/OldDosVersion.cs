using System;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class OldDosVersion : BaseModel<long>
{
    [Required]
    public long OldDosSoftwareId { get; set; }
    public virtual OldDosSoftware OldDosSoftware { get; set; }

    [Required]
    [StringLength(128)]
    public string VersionString { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public DatePrecision ReleaseDatePrecision { get; set; }

    [StringLength(256)]
    public string OsHint { get; set; }

    [StringLength(2048)]
    public string DownloadUrl { get; set; }

    [StringLength(512)]
    public string FileName { get; set; }

    [StringLength(2048)]
    public string Notes { get; set; }

    public bool IsEnabledByDefault { get; set; } = true;

    public ulong? PromotedSoftwareVersionId { get; set; }
}
