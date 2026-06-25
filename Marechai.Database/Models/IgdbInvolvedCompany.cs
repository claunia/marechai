using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class IgdbInvolvedCompany : BaseModel<long>
{
    [Required]
    public long IgdbId { get; set; }

    [Required]
    public long GameIgdbId { get; set; }

    [Required]
    public long CompanyIgdbId { get; set; }

    public bool Developer { get; set; }

    public bool Publisher { get; set; }
}
