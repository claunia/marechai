using System;
using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Machines
    {
        public Machines()
        {
            GpusByMachine       = new HashSet<GpusByMachine>();
            MemoryByMachine     = new HashSet<MemoryByMachine>();
            ProcessorsByMachine = new HashSet<ProcessorsByMachine>();
            SoundByMachine      = new HashSet<SoundByMachine>();
            StorageByMachine    = new HashSet<StorageByMachine>();
        }

        public int       Id         { get; set; }
        public int       Company    { get; set; }
        public string    Name       { get; set; }
        public int       Type       { get; set; }
        public DateTime? Introduced { get; set; }
        public int?      Family     { get; set; }
        public string    Model      { get; set; }

        public Companies                        CompanyNavigation   { get; set; }
        public MachineFamilies                  FamilyNavigation    { get; set; }
        public ICollection<GpusByMachine>       GpusByMachine       { get; set; }
        public ICollection<MemoryByMachine>     MemoryByMachine     { get; set; }
        public ICollection<ProcessorsByMachine> ProcessorsByMachine { get; set; }
        public ICollection<SoundByMachine>      SoundByMachine      { get; set; }
        public ICollection<StorageByMachine>    StorageByMachine    { get; set; }
    }
}