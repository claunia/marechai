using System;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class MobyGamesVideoImportState : BaseModel<long>
{
    [Required]
    [StringLength(64)]
    public string MobyGameId { get; set; }

    public ulong SoftwareId { get; set; }

    [Required]
    [StringLength(512)]
    public string VideoUrl { get; set; }

    [StringLength(512)]
    public string Title { get; set; }

    [StringLength(32)]
    public string Provider { get; set; }

    [StringLength(64)]
    public string ExtractedVideoId { get; set; }

    [Required]
    public MobyGamesCoverDownloadStatus Status { get; set; }

    [StringLength(1024)]
    public string ErrorMessage { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public long? SoftwareVideoId { get; set; }
}
