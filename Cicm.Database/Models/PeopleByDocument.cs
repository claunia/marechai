using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class PeopleByDocument : BaseModel<long>
    {
        public int  PersonId   { get; set; }
        public long DocumentId { get; set; }
        [Column(TypeName = "char(3)")]
        [Required]
        public string RoleId { get; set; }

        public virtual DocumentPerson Person   { get; set; }
        public virtual Document       Document { get; set; }
        public virtual DocumentRole   Role     { get; set; }
    }
}