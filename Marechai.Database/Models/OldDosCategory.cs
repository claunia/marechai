using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

public class OldDosCategory : BaseModel<int>
{
    public int? ParentId { get; set; }

    [Required]
    [StringLength(512)]
    public string RussianName { get; set; }

    [StringLength(512)]
    public string EnglishName { get; set; }

    [StringLength(2048)]
    public string Path { get; set; }

    public DateTime? LastCrawledOn { get; set; }

    public virtual OldDosCategory               Parent   { get; set; }
    public virtual ICollection<OldDosCategory>  Children { get; set; }
    public virtual ICollection<OldDosSoftware>  Softwares { get; set; }
}
