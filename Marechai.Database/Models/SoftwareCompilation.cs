using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class SoftwareCompilation : BaseModel<ulong>
{
    [Required]
    public string Name { get; set; }

    public         ulong?   SoftwareId { get; set; }
    public virtual Software Software   { get; set; }

    /// <summary>
    ///     The game this compilation is add-on content for (e.g. a Season Pass bundling DLCs for a single game).
    ///     Distinct from <see cref="SoftwareId" />, which links a compilation bundled with a piece of software.
    /// </summary>
    public         ulong?   BaseSoftwareId { get; set; }
    public virtual Software BaseSoftware   { get; set; }

    public         int?     MachineId { get; set; }
    public virtual Machine  Machine   { get; set; }

    public         ulong?                   PredecessorId    { get; set; }
    public virtual SoftwareCompilation      Predecessor      { get; set; }
    public         SoftwareRelationshipType RelationshipType { get; set; } = SoftwareRelationshipType.Sequel;
    public virtual ICollection<SoftwareCompilation> Successors { get; set; }

    public virtual ICollection<SoftwareRelease> Releases { get; set; }
    public virtual ICollection<SoftwareCover>   Covers   { get; set; }

    public virtual ICollection<SoftwareBySoftwareCompilation>        IncludedSoftware     { get; set; }
    public virtual ICollection<SoftwareVersionBySoftwareCompilation> IncludedVersions     { get; set; }
    public virtual ICollection<SoftwareCompilationBySoftwareCompilation> IncludedCompilations   { get; set; }
    public virtual ICollection<SoftwareCompilationBySoftwareCompilation> ContainingCompilations { get; set; }
}
