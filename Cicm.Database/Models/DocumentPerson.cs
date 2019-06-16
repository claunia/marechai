using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class DocumentPerson : BaseModel<int>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        public int? PersonId { get;  set; }

        public virtual Person Person { get; set; }

        [NotMapped]
        public string FullName => $"{Name} {Surname}";
    }
}