using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class MachineFamilies
    {
        public MachineFamilies()
        {
            Machines = new HashSet<Machines>();
        }

        public int    Id      { get; set; }
        public int    Company { get; set; }
        public string Name    { get; set; }

        public Companies             CompanyNavigation { get; set; }
        public ICollection<Machines> Machines          { get; set; }
    }
}