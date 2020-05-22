using System;
using System.Collections.Generic;

namespace Marechai.ViewModels
{
    public class ProcessorViewModel
    {
        public string       Name                     { get; set; }
        public string       CompanyName              { get; set; }
        public string       ModelCode                { get; set; }
        public DateTime?    Introduced               { get; set; }
        public double?      Speed                    { get; set; }
        public string       Package                  { get; set; }
        public int?         Gprs                     { get; set; }
        public int?         GprSize                  { get; set; }
        public int?         Fprs                     { get; set; }
        public int?         FprSize                  { get; set; }
        public int?         Cores                    { get; set; }
        public int?         ThreadsPerCore           { get; set; }
        public string       Process                  { get; set; }
        public float?       ProcessNm                { get; set; }
        public float?       DieSize                  { get; set; }
        public long?        Transistors              { get; set; }
        public int?         DataBus                  { get; set; }
        public int?         AddrBus                  { get; set; }
        public int?         SimdRegisters            { get; set; }
        public int?         SimdSize                 { get; set; }
        public float?       L1Instruction            { get; set; }
        public float?       L1Data                   { get; set; }
        public float?       L2                       { get; set; }
        public float?       L3                       { get; set; }
        public string       InstructionSet           { get; set; }
        public List<string> InstructionSetExtensions { get; set; }
        public int?         CompanyId                { get; set; }
    }
}