using System;
using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class SoundSynth
    {
        public SoundSynth()
        {
            SoundByMachine = new HashSet<SoundByMachine>();
        }

        public int       Id         { get; set; }
        public string    Name       { get; set; }
        public int?      CompanyId  { get; set; }
        public string    ModelCode  { get; set; }
        public DateTime? Introduced { get; set; }
        public int?      Voices     { get; set; }
        public double?   Frequency  { get; set; }
        public int?      Depth      { get; set; }
        public int?      SquareWave { get; set; }
        public int?      WhiteNoise { get; set; }
        public int?      Type       { get; set; }

        public virtual Company                     Company        { get; set; }
        public virtual ICollection<SoundByMachine> SoundByMachine { get; set; }
    }
}