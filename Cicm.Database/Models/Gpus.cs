using System;
using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Gpus
    {
        public Gpus()
        {
            GpusByMachine    = new HashSet<GpusByMachine>();
            ResolutionsByGpu = new HashSet<ResolutionsByGpu>();
        }

        public int       Id          { get; set; }
        public string    Name        { get; set; }
        public int?      CompanyId   { get; set; }
        public string    ModelCode   { get; set; }
        public DateTime? Introduced  { get; set; }
        public string    Package     { get; set; }
        public string    Process     { get; set; }
        public float?    ProcessNm   { get; set; }
        public float?    DieSize     { get; set; }
        public long?     Transistors { get; set; }

        public Companies                     Company          { get; set; }
        public ICollection<GpusByMachine>    GpusByMachine    { get; set; }
        public ICollection<ResolutionsByGpu> ResolutionsByGpu { get; set; }
    }
}