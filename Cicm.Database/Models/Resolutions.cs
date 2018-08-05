using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Resolutions
    {
        public Resolutions()
        {
            ResolutionsByGpu = new HashSet<ResolutionsByGpu>();
        }

        public int   Id      { get; set; }
        public int   Width   { get; set; }
        public int   Height  { get; set; }
        public long? Colors  { get; set; }
        public long? Palette { get; set; }
        public sbyte Chars   { get; set; }

        public ICollection<ResolutionsByGpu> ResolutionsByGpu { get; set; }
    }
}