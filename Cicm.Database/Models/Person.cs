using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class Person : BaseModel<int>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get;                         set; }
        public virtual Iso31661Numeric CountryOfBirth { get; set; }
        public         DateTime        BirthDate      { get; set; }
        public         DateTime?       DeathDate      { get; set; }
        public         string          Webpage        { get; set; }
        public         string          Twitter        { get; set; }
        public         string          Facebook       { get; set; }
        public         Guid            Photo          { get; set; }

        [NotMapped]
        public string FullName => $"{Name} {Surname}";

        public short? CountryOfBirthId { get; set; }
    }
}