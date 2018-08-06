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

        public Company                          Company    { get; set; }
        public MachineFamily                    Family     { get; set; }
        public ICollection<GpusByMachine>       Gpus       { get; set; }
        public ICollection<MemoryByMachine>     Memory     { get; set; }
        public ICollection<ProcessorsByMachine> Processors { get; set; }
        public ICollection<SoundByMachine>      Sound      { get; set; }
        public ICollection<StorageByMachine>    Storage    { get; set; }
    }
}