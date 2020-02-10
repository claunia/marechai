using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class MagazineIssue : BaseModel<long>
    {
        [Required]
        public long MagazineId { get; set; }
        [Required]
        public string Caption { get;       set; }
        public string NativeCaption { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}"), DataType(DataType.Date)]
        public DateTime? Published { get; set; }
        [StringLength(18)]
        public string ProductCode { get; set; }
        public short Pages { get;        set; }

        public virtual Magazine                              Magazine        { get; set; }
        public virtual ICollection<PeopleByMagazine>         People          { get; set; }
        public virtual ICollection<MagazinesByMachine>       Machines        { get; set; }
        public virtual ICollection<MagazinesByMachineFamily> MachineFamilies { get; set; }
    }
}