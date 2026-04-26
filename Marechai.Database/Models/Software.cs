using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class Software : BaseModel<ulong>
{
    [Required]
    public string Name { get;                                                set; }
    public         ulong?                           FamilyId          { get; set; }
    public virtual SoftwareFamily                   Family            { get; set; }
    public         bool                             IsOperatingSystem { get; set; }
    public         bool                             IsGame            { get; set; }
    public virtual ICollection<SoftwareVersion>     Versions          { get; set; }
    public virtual ICollection<SoftwareCompanyRole> CompanyRoles      { get; set; }
    public virtual ICollection<SoftwareScreenshot>  Screenshots       { get; set; }
    public virtual ICollection<SoftwareRelease>             DirectReleases     { get; set; }
    public virtual ICollection<SoftwareBySoftwareRelease>   CompilationReleases { get; set; }
    public virtual ICollection<SoftwareDescription>         Descriptions        { get; set; }
}