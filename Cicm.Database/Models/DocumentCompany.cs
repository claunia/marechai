using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Cicm.Database.Models
{
    public class DocumentCompany : BaseModel<int>
    {
        [Required]
        public string Name { get;    set; }
        public int? CompanyId { get; set; }

        [DisplayName("Linked company")]
        public virtual Company Company { get;                            set; }
        public virtual ICollection<CompaniesByDocument> Documents { get; set; }
        public virtual ICollection<CompaniesByBook>     Books     { get; set; }
        public virtual ICollection<CompaniesByMagazine> Magazines { get; set; }
    }
}