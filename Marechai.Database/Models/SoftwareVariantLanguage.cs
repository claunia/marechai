using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class SoftwareVariantLanguage
{
    public         ulong           VariantId { get; set; }
    public virtual SoftwareVariant Variant   { get; set; }
    [StringLength(3)]
    public string LanguageCode { get;     set; }
    public virtual Iso639 Language { get; set; }
}