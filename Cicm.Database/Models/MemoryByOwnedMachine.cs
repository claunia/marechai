using System.ComponentModel;

namespace Cicm.Database.Models
{
    public class MemoryByOwnedMachine : BaseModel<long>
    {
        public long        OwnedMachineId { get; set; }
        public MemoryType  Type           { get; set; }
        public MemoryUsage Usage          { get; set; }
        public long        Size           { get; set; }
        [DisplayName("Speed (Hz)")]
        public double Speed { get; set; }

        public virtual OwnedMachine OwnedMachine { get; set; }
    }
}