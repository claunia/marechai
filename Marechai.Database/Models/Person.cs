using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace Marechai.Database.Models
{
    public class Person : BaseModel<int>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [DisplayName("Country of birth")]
        public virtual Iso31661Numeric CountryOfBirth { get; set; }
        [DisplayName("Birth date")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }
        [DisplayName("Date of death")]
        [DataType(DataType.Date)]
        public DateTime? DeathDate { get; set; }
        [Url]
        public string Webpage { get; set; }
        [Remote("VerifyTwitter", "People", "Admin")]
        public string Twitter { get;          set; }
        public string Facebook         { get; set; }
        public Guid   Photo            { get; set; }
        public int?   DocumentPersonId { get; set; }
        public string Alias            { get; set; }
        [DisplayName("Name to be displayed")]
        public string DisplayName { get; set; }

        [NotMapped]
        [DisplayName("Name")]
        public string FullName => DisplayName ?? Alias ?? $"{Name} {Surname}";

        public         short?                       CountryOfBirthId { get; set; }
        public virtual ICollection<PeopleByCompany> Companies        { get; set; }
        public virtual DocumentPerson               DocumentPerson   { get; set; }
    }
}