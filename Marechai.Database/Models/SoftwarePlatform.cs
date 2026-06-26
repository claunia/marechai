using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class SoftwarePlatform : BaseModel<ulong>
{
    [Required]
    public string Name { get;                                           set; } // Xbox, PlayStation, PC, etc.
    public Guid?  LogoId        { get; set; }
    public string LogoExtension { get; set; }
    public virtual ICollection<SoftwareRelease>           SoftwareReleases { get; set; }
    public virtual ICollection<SoftwareScreenshot>        Screenshots      { get; set; }
    public virtual ICollection<SoftwarePlatformsByMachine> Machines         { get; set; }
}