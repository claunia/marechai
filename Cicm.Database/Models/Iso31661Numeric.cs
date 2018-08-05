using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Iso31661Numeric
    {
        public Iso31661Numeric()
        {
            Companies = new HashSet<Companies>();
        }

        public short  Id   { get; set; }
        public string Name { get; set; }

        public ICollection<Companies> Companies { get; set; }
    }
}