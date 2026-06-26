using System;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class IgdbPlatform : BaseModel<int>
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [Required]
    public IgdbMatchStatus MatchStatus { get; set; }

    [StringLength(64)]
    public string MatchType { get; set; }

    public DateTime? MatchedOn { get; set; }

    public ulong? SoftwarePlatformId { get; set; }

    [StringLength(64)]
    public string LogoImageId { get; set; }

    public bool EnrichmentApplied { get; set; }

    public DateTime? EnrichedOn { get; set; }
}
