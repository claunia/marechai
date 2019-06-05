using System;

namespace Cicm.Database.Models
{
    public class PeopleByCompany : BaseModel<long>
    {
        public int       PersonId  { get; set; }
        public int       CompanyId { get; set; }
        public string    Position  { get; set; }
        public DateTime? Start     { get; set; }
        public DateTime? End       { get; set; }
        public bool      Ongoing   { get; set; }

        public virtual Person  Person  { get; set; }
        public virtual Company Company { get; set; }
    }
}