using System;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class MobyGamesPromoArtDownloadState : BaseModel<long>
{
    [Required]
    [StringLength(64)]
    public string MobyGameId { get; set; }

    public ulong SoftwareId { get; set; }

    [Required]
    [StringLength(512)]
    public string PromoPageUrl { get; set; }

    [StringLength(256)]
    public string Caption { get; set; }

    [StringLength(256)]
    public string GroupName { get; set; }

    [Required]
    public MobyGamesCoverDownloadStatus Status { get; set; }

    [StringLength(1024)]
    public string ErrorMessage { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public Guid? SoftwarePromoArtId { get; set; }

    [StringLength(1024)]
    public string OriginalUrl { get; set; }
}
