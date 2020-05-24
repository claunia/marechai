using System;

namespace Marechai.ViewModels
{
    public class GpuViewModel : BaseViewModel<int>
    {
        public string    Name        { get; set; }
        public int?      CompanyId   { get; set; }
        public string    Company     { get; set; }
        public string    ModelCode   { get; set; }
        public DateTime? Introduced  { get; set; }
        public string    Package     { get; set; }
        public string    Process     { get; set; }
        public float?    ProcessNm   { get; set; }
        public float?    DieSize     { get; set; }
        public long?     Transistors { get; set; }

        public string IntroducedView => Introduced?.ToShortDateString() ?? "Unknown";
    }
}