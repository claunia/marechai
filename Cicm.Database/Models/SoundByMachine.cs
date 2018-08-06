namespace Cicm.Database.Models
{
    public class SoundByMachine
    {
        public int  SoundSynthId { get; set; }
        public int  MachineId    { get; set; }
        public long Id           { get; set; }

        public virtual Machine    Machine    { get; set; }
        public virtual SoundSynth SoundSynth { get; set; }
    }
}