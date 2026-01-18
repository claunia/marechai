using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class SoftwareProductCode : BaseModel<ulong>
{
    public         ulong           ReleaseId { get; set; }
    public virtual SoftwareRelease Release   { get; set; }
    [Required]
    public ProductCodeIssuer Issuer { get; set; } // Microsoft, Sony, Nintendo, etc.
    [Required]
    public string Code { get; set; }
}