using System.ComponentModel;

namespace Marechai.Database.Models
{
    public class SoundByOwnedMachine : BaseModel<long>
    {
        public int  SoundSynthId   { get; set; }
        public long OwnedMachineId { get; set; }

        public virtual OwnedMachine OwnedMachine { get; set; }
        [DisplayName("Sound synthetizer")]
        public virtual SoundSynth SoundSynth { get; set; }
    }
}