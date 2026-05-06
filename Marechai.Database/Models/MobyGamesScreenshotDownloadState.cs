using System;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class MobyGamesScreenshotDownloadState : BaseModel<long>
{
    [Required]
    [StringLength(64)]
    public string MobyGameId { get; set; }

    public ulong SoftwareId { get; set; }

    [Required]
    [StringLength(512)]
    public string ScreenshotPageUrl { get; set; }

    [StringLength(256)]
    public string Caption { get; set; }

    [StringLength(256)]
    public string Platform { get; set; }

    [Required]
    public MobyGamesCoverDownloadStatus Status { get; set; }

    [StringLength(1024)]
    public string ErrorMessage { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public Guid? SoftwareScreenshotId { get; set; }

    [StringLength(1024)]
    public string OriginalUrl { get; set; }
}
