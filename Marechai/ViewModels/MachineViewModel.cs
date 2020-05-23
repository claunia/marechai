using System;
using System.Collections.Generic;

namespace Marechai.ViewModels
{
    public class MachineViewModel
    {
        public int                       Id                { get; set; }
        public string                    Name              { get; set; }
        public string                    Model             { get; set; }
        public string                    CompanyName       { get; set; }
        public int                       CompanyId         { get; set; }
        public Guid?                     CompanyLogo       { get; set; }
        public DateTime?                 Introduced        { get; set; }
        public int?                      FamilyId          { get; set; }
        public string                    FamilyName        { get; set; }
        public List<GpuViewModel>        Gpus              { get; set; }
        public List<MemoryViewModel>     Memory            { get; set; }
        public List<ProcessorViewModel>  Processors        { get; set; }
        public List<SoundSynthViewModel> SoundSynthesizers { get; set; }
        public List<StorageViewModel>    Storage           { get; set; }
    }
}