using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class SoftwareAttribute : BaseModel<long>
{
    public         ulong           SoftwareReleaseId { get; set; }
    public virtual SoftwareRelease SoftwareRelease   { get; set; }

    [Required]
    [StringLength(64)]
    public string Category { get; set; }

    [Required]
    [StringLength(128)]
    public string Key { get; set; }

    [Required]
    [StringLength(512)]
    public string Value { get; set; }
}
