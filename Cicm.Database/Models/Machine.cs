using System;
using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Machine
    {
        public Machine()
        {
            Gpus       = new HashSet<GpusByMachine>();
            Memory     = new HashSet<MemoryByMachine>();
            Processors = new HashSet<ProcessorsByMachine>();
            Sound      = new HashSet<SoundByMachine>();
            Storage    = new HashSet<StorageByMachine>();
        }

        public int         Id         { get; set; }
        public int         CompanyId  { get; set; }
        public string      Name       { get; set; }
        public MachineType Type       { get; set; }
        public DateTime?   Introduced { get; set; }
        public int?        FamilyId   { get; set; }
        public string      Model      { get; set; }

        public virtual Company                          Company    { get; set; }
        public virtual MachineFamily                    Family     { get; set; }
        public virtual ICollection<GpusByMachine>       Gpus       { get; set; }
        public virtual ICollection<MemoryByMachine>     Memory     { get; set; }
        public virtual ICollection<ProcessorsByMachine> Processors { get; set; }
        public virtual ICollection<SoundByMachine>      Sound      { get; set; }
        public virtual ICollection<StorageByMachine>    Storage    { get; set; }
    }
}