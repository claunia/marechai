using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class SoftwareGenre : BaseModel<int>
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    [Required]
    public SoftwareGenreType Type { get; set; }

    public virtual ICollection<GenreBySoftware> Softwares { get; set; }
}
