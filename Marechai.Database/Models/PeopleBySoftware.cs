using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class PeopleBySoftware : BaseModel<long>
{
    public         ulong    SoftwareId { get; set; }
    public virtual Software Software   { get; set; }
    public         int      PersonId   { get; set; }
    public virtual Person   Person     { get; set; }

    [Required]
    [StringLength(128)]
    public string Role { get; set; }
}
