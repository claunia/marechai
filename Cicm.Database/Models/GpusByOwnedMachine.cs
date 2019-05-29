using System.ComponentModel;

namespace Cicm.Database.Models
{
    public class GpusByOwnedMachine : BaseModel<long>
    {
        public int  GpuId          { get; set; }
        public long OwnedMachineId { get; set; }

        [DisplayName("GPU")]
        public virtual Gpu Gpu { get;                   set; }
        public virtual OwnedMachine OwnedMachine { get; set; }
    }
}