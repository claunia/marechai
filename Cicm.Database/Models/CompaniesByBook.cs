using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class CompaniesByBook : BaseModel<long>
    {
        public int  CompanyId { get; set; }
        public long BookId    { get; set; }
        [Column(TypeName = "char(3)")]
        [Required]
        public string RoleId { get; set; }

        public virtual DocumentCompany Company { get; set; }
        public virtual Book            Book    { get; set; }
        public virtual DocumentRole   Role    { get; set; }
    }
}