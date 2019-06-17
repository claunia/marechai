using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cicm.Database.Models
{
    public class Magazine : DocumentBase
    {
        [StringLength(8, MinimumLength = 8)]
        public string Issn { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}")]
        [DataType(DataType.Date)]
        public DateTime? FirstPublication { get; set; }

        public virtual Iso31661Numeric                  Country   { get; set; }
        public virtual ICollection<MagazineIssue>       Issues    { get; set; }
        public virtual ICollection<CompaniesByMagazine> Companies { get; set; }
    }
}