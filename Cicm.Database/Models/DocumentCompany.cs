using System.ComponentModel.DataAnnotations;

namespace Cicm.Database.Models
{
    public class DocumentCompany : BaseModel<int>
    {
        [Required]
        public string Name { get;    set; }
        public int? CompanyId { get; set; }

        public virtual Company Company { get; set; }
    }
}