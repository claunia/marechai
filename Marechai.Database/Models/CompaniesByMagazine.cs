using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class CompaniesByMagazine : BaseModel<long>
    {
        public int  CompanyId  { get; set; }
        public long MagazineId { get; set; }
        [Column(TypeName = "char(3)")]
        [Required]
        public string RoleId { get; set; }

        public virtual DocumentCompany Company  { get; set; }
        public virtual Magazine        Magazine { get; set; }
        public virtual DocumentRole    Role     { get; set; }
    }
}