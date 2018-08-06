using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Resolution
    {
        public Resolution()
        {
            ResolutionsByGpu = new HashSet<ResolutionsByGpu>();
        }

        public int   Id      { get; set; }
        public int   Width   { get; set; }
        public int   Height  { get; set; }
        public long? Colors  { get; set; }
        public long? Palette { get; set; }
        public sbyte Chars   { get; set; }

        public virtual ICollection<ResolutionsByGpu> ResolutionsByGpu { get; set; }
    }
}