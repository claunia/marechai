using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class SoftwareSubvariant : BaseModel<ulong>
{
    public         ulong           VariantId { get; set; }
    public virtual SoftwareVariant Variant   { get; set; }
    [Required]
    public string Name { get;                                               set; } // "32-bit", "64-bit", etc.
    public virtual ICollection<SoftwareSubvariantLanguage> Languages { get; set; }
}