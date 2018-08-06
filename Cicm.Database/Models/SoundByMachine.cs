namespace Cicm.Database.Models
{
    public class SoundByMachine
    {
        public int  SoundSynthId { get; set; }
        public int  MachineId    { get; set; }
        public long Id           { get; set; }

        public Machine    Machine    { get; set; }
        public SoundSynth SoundSynth { get; set; }
    }
}