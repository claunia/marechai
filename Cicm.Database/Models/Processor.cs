using System;
using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Processor
    {
        public Processor()
        {
            InstructionSetExtensions = new HashSet<InstructionSetExtensionsByProcessor>();
            ProcessorsByMachine      = new HashSet<ProcessorsByMachine>();
        }

        public int       Id               { get; set; }
        public string    Name             { get; set; }
        public int?      CompanyId        { get; set; }
        public string    ModelCode        { get; set; }
        public DateTime? Introduced       { get; set; }
        public int?      InstructionSetId { get; set; }
        public double?   Speed            { get; set; }
        public string    Package          { get; set; }
        public int?      Gprs             { get; set; }
        public int?      GprSize          { get; set; }
        public int?      Fprs             { get; set; }
        public int?      FprSize          { get; set; }
        public int?      Cores            { get; set; }
        public int?      ThreadsPerCore   { get; set; }
        public string    Process          { get; set; }
        public float?    ProcessNm        { get; set; }
        public float?    DieSize          { get; set; }
        public long?     Transistors      { get; set; }
        public int?      DataBus          { get; set; }
        public int?      AddrBus          { get; set; }
        public int?      SimdRegisters    { get; set; }
        public int?      SimdSize         { get; set; }
        public float?    L1Instruction    { get; set; }
        public float?    L1Data           { get; set; }
        public float?    L2               { get; set; }
        public float?    L3               { get; set; }

        public virtual Company                                          Company                  { get; set; }
        public virtual InstructionSet                                   InstructionSet           { get; set; }
        public virtual ICollection<InstructionSetExtensionsByProcessor> InstructionSetExtensions { get; set; }
        public virtual ICollection<ProcessorsByMachine>                 ProcessorsByMachine      { get; set; }
    }
}