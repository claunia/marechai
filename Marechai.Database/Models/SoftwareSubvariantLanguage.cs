using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class SoftwareSubvariantLanguage
{
    public         ulong              SubvariantId { get; set; }
    public virtual SoftwareSubvariant Subvariant   { get; set; }
    [StringLength(3)]
    public string LanguageCode { get;     set; }
    public virtual Iso639 Language { get; set; }
}