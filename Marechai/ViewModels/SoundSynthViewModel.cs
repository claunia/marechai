using System;

namespace Marechai.ViewModels
{
    public class SoundSynthViewModel
    {
        public int       Id          { get; set; }
        public string    Name        { get; set; }
        public string    CompanyName { get; set; }
        public int?      CompanyId   { get; set; }
        public string    ModelCode   { get; set; }
        public DateTime? Introduced  { get; set; }
        public int?      Voices      { get; set; }
        public double?   Frequency   { get; set; }
        public int?      Depth       { get; set; }
        public int?      SquareWave  { get; set; }
        public int?      WhiteNoise  { get; set; }
        public int?      Type        { get; set; }
    }
}