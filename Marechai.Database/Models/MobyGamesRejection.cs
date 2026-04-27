using System;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class MobyGamesRejection : BaseModel<long>
{
    [Required]
    [StringLength(64)]
    public string MobyGameId { get; set; }

    [Required]
    [StringLength(512)]
    public string GameName { get; set; }

    [Required]
    [StringLength(1024)]
    public string Reason { get; set; }

    public DateTime RejectedOn { get; set; }

    public DateTime? ReviewedOn { get; set; }

    [Required]
    public MobyGamesRejectionReview ReviewAction { get; set; }
}
