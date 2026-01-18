using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class SoftwareCompanyRole
{
    public         ulong    SoftwareId { get; set; }
    public virtual Software Software   { get; set; }
    public         int      CompanyId  { get; set; }
    public virtual Company  Company    { get; set; }
    [Required]
    public string Role { get; set; } // developer, distributor, publisher
}