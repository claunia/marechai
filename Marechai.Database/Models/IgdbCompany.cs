using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Marechai.Data;

namespace Marechai.Database.Models;

public class IgdbCompany : BaseModel<long>
{
    [Required]
    public long IgdbId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [Required]
    public IgdbMatchStatus MatchStatus { get; set; }

    [StringLength(64)]
    public string MatchType { get; set; }

    public double? MatchScore { get; set; }

    public DateTime? MatchedOn { get; set; }

    [StringLength(4096)]
    public string CandidatesJson { get; set; }

    [StringLength(1024)]
    public string ErrorMessage { get; set; }

    public int BatchNumber { get; set; }

    public int? CompanyId { get; set; }

    [Column(TypeName = "text")]
    public string WebsitesJson { get; set; }

    public short? Country { get; set; }

    [Column(TypeName = "text")]
    public string DescriptionRaw { get; set; }

    public long? StartDate { get; set; }

    public int? StartDateFormat { get; set; }

    public long? ChangeDate { get; set; }

    public int? ChangeDateFormat { get; set; }

    public int? Status { get; set; }

    public long? ParentIgdbId { get; set; }

    [StringLength(64)]
    public string LogoImageId { get; set; }

    [Column(TypeName = "text")]
    public string CompanyTypeHistoryJson { get; set; }

    public bool EnrichmentApplied { get; set; }

    public DateTime? EnrichedOn { get; set; }
}
