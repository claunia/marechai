using System.ComponentModel;

namespace Cicm.Database.Models
{
    public class ProcessorsByOwnedMachine : BaseModel<long>
    {
        public int  ProcessorId    { get; set; }
        public long OwnedMachineId { get; set; }
        [DisplayName("Speed (MHz)")]
        public float Speed { get; set; }

        public virtual OwnedMachine OwnedMachine { get; set; }
        public virtual Processor    Processor    { get; set; }
    }
}