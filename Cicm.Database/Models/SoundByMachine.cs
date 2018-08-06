namespace Cicm.Database.Models
{
    public class SoundByMachine
    {
        public int  SoundSynthId { get; set; }
        public int  MachineId    { get; set; }
        public long Id           { get; set; }

        public Machines    Machine    { get; set; }
        public SoundSynths SoundSynth { get; set; }
    }
}