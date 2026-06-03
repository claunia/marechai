using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class WwpcCategory : BaseModel<int>
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; }

    [Required]
    public WwpcProductType ProductType { get; set; }

    public DateTime? LastCrawledOn { get; set; }

    public virtual ICollection<WwpcSoftware> Softwares { get; set; }
}
