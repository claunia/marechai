using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models;

public class SoftwareCompanyRole
{
    public         ulong    SoftwareId { get; set; }
    public virtual Software Software   { get; set; }
    public         int      CompanyId  { get; set; }
    public virtual Company  Company    { get; set; }
    [Column(TypeName = "char(3)")]
    [Required]
    public string RoleId { get; set; }

    public virtual SoftwareRole Role { get; set; }
}