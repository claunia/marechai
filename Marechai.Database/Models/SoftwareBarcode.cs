using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class SoftwareBarcode : BaseModel<ulong>
{
    public         ulong           ReleaseId { get; set; }
    public virtual SoftwareRelease Release   { get; set; }
    [Required]
    public string Code { get; set; }
    [Required]
    public BarcodeType Type { get; set; } // EAN13, UPC, etc.
}