using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class CompaniesByDocument : BaseModel<long>
    {
        public int  CompanyId  { get; set; }
        public long DocumentId { get; set; }
        [Column(TypeName = "char(3)"), Required]
        public string RoleId { get; set; }

        public virtual DocumentCompany Company  { get; set; }
        public virtual Document        Document { get; set; }
        public virtual DocumentRole    Role     { get; set; }
    }
}