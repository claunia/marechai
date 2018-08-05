namespace Cicm.Database.Models
{
    public class SoundByMachine
    {
        public int  SoundSynth { get; set; }
        public int  Machine    { get; set; }
        public long Id         { get; set; }

        public Machines    MachineNavigation    { get; set; }
        public SoundSynths SoundSynthNavigation { get; set; }
    }
}