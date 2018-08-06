using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class MachineFamily
    {
        public MachineFamily()
        {
            Machines = new HashSet<Machine>();
        }

        public int    Id        { get; set; }
        public int    CompanyId { get; set; }
        public string Name      { get; set; }

        public virtual Company              Company  { get; set; }
        public virtual ICollection<Machine> Machines { get; set; }
    }
}