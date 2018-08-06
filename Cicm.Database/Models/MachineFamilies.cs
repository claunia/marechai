using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class MachineFamilies
    {
        public MachineFamilies()
        {
            Machines = new HashSet<Machines>();
        }

        public int    Id        { get; set; }
        public int    CompanyId { get; set; }
        public string Name      { get; set; }

        public Companies             Company  { get; set; }
        public ICollection<Machines> Machines { get; set; }
    }
}