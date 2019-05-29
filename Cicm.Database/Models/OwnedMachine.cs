using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Cicm.Database.Models
{
    public class OwnedMachine : BaseModel<long>
    {
        [DisplayName("Acquisition date")]
        public DateTime AcquisitionDate { get; set; }
        [DisplayName("Date when sold, traded, or otherwise lost")]
        public DateTime? LostDate { get; set; }
        public StatusType Status { get;  set; }
        [DisplayName("Last status check date")]
        public DateTime? LastStatusDate { get; set; }
        [DisplayName("Available for trade or sale")]
        public bool Trade { get; set; }
        [DisplayName("Has original boxes")]
        public bool Boxed { get; set; }
        [DisplayName("Has original manuals")]
        public bool Manuals { get; set; }
        [DisplayName("Serial number")]
        public string SerialNumber { get; set; }
        [DisplayName("Serial number visible to other users")]
        public bool SerialNumberVisible { get; set; }
        public int    MachineId { get;         set; }
        public string UserId    { get;         set; }

        public virtual ICollection<GpusByOwnedMachine>       Gpus       { get; set; }
        public virtual ICollection<MemoryByOwnedMachine>     Memory     { get; set; }
        public virtual ICollection<ProcessorsByOwnedMachine> Processors { get; set; }
        public virtual ICollection<SoundByOwnedMachine>      Sound      { get; set; }
        public virtual ICollection<StorageByOwnedMachine>    Storage    { get; set; }
        public virtual ICollection<OwnedMachinePhoto>        Photos     { get; set; }

        public virtual ApplicationUser User    { get; set; }
        public virtual Machine         Machine { get; set; }
    }
}