using System;
using System.Collections.Generic;
using Marechai.Database;

namespace Marechai.ViewModels
{
    public class MachineViewModel : BaseViewModel<int>
    {
        public string                    Name              { get; set; }
        public string                    Model             { get; set; }
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
        public string                    Company           { get; set; }
        public MachineType               Type              { get; set; }
        public string                    Family            { get; set; }
        public string IntroducedView =>
            Introduced?.Year == 1000 ? "Prototype" : Introduced?.ToShortDateString() ?? "Unknown";
    }
}