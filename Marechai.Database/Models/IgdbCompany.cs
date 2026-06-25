using System;
using System.ComponentModel.DataAnnotations;
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
}
