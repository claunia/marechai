namespace Marechai.ViewModels
{
    public class ProcessorByMachineViewModel : BaseViewModel<long>
    {
        public int    ProcessorId { get; set; }
        public int    MachineId   { get; set; }
        public string CompanyName { get; set; }
        public string Name        { get; set; }
        public float? Speed       { get; set; }
    }
}