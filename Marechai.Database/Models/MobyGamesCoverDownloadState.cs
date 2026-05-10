using System;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class MobyGamesCoverDownloadState : BaseModel<long>
{
    [Required]
    [StringLength(64)]
    public string MobyGameId { get; set; }

    public ulong SoftwareId { get; set; }

    [Required]
    [StringLength(512)]
    public string CoverPageUrl { get; set; }

    [Required]
    [StringLength(1024)]
    public string CoverType { get; set; }

    [Required]
    [StringLength(256)]
    public string Platform { get; set; }

    [StringLength(512)]
    public string Countries { get; set; }

    [StringLength(64)]
    public string GroupId { get; set; }

    [Required]
    public MobyGamesCoverDownloadStatus Status { get; set; }

    [StringLength(1024)]
    public string ErrorMessage { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public Guid? SoftwareCoverId { get; set; }

    public ulong? SoftwareReleaseId { get; set; }

    [StringLength(1024)]
    public string OriginalUrl { get; set; }
}
