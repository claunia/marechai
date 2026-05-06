using System;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class MobyGamesImportState : BaseModel<long>
{
    [Required]
    [StringLength(64)]
    public string MobyGameId { get; set; }

    [Required]
    public MobyGamesImportStatus Status { get; set; }

    [StringLength(1024)]
    public string ErrorMessage { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public int BatchNumber { get; set; }

    public ulong? SoftwareId { get; set; }

    public int? MobyNumericId { get; set; }
}
