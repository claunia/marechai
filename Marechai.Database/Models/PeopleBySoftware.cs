using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models;

public class PeopleBySoftware : BaseModel<long>
{
    public         ulong    SoftwareId { get; set; }
    public virtual Software Software   { get; set; }
    public         int      PersonId   { get; set; }
    public virtual Person   Person     { get; set; }

    [Required]
    [StringLength(256)]
    public string Role { get; set; }

    [Column(TypeName = "char(3)")]
    public string RoleId { get; set; }

    public virtual DocumentRole DocumentRole { get; set; }
}
