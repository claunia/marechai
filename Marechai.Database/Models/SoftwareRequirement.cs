using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class SoftwareRequirement
{
    public         ulong           SoftwareVersionId         { get; set; }
    public virtual SoftwareVersion SoftwareVersion           { get; set; }
    public         ulong           RequiredSoftwareVersionId { get; set; }
    public virtual SoftwareVersion RequiredSoftwareVersion   { get; set; }
    [Required]
    public SoftwareRequirementType RequirementType { get; set; }
}