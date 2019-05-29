using System;
using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class OwnedMachine : BaseModel<long>
    {
        public DateTime   AcquisitionDate     { get; set; }
        public DateTime?  LostDate            { get; set; }
        public StatusType Status              { get; set; }
        public DateTime?  LastStatusDate      { get; set; }
        public bool       Trade               { get; set; }
        public bool       Boxed               { get; set; }
        public bool       Manuals             { get; set; }
        public string     SerialNumber        { get; set; }
        public bool       SerialNumberVisible { get; set; }
        public int        MachineId           { get; set; }

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