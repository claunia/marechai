using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class DocumentPerson : BaseModel<int>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get;  set; }
        public int?   PersonId { get; set; }
        public string Alias    { get; set; }
        [DisplayName("Name to be displayed")]
        public string DisplayName { get; set; }

        [NotMapped, DisplayName("Name")]
        public string FullName => DisplayName ?? Alias ?? $"{Name} {Surname}";

        [DisplayName("Linked person")]
        public virtual Person Person { get;                           set; }
        public virtual ICollection<PeopleByDocument> Documents { get; set; }
        public virtual ICollection<PeopleByBook>     Books     { get; set; }
        public virtual ICollection<PeopleByMagazine> Magazines { get; set; }
    }
}