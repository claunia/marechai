namespace Marechai.ViewModels
{
    public class GpuByMachineViewModel : BaseViewModel<long>
    {
        public int    GpuId       { get; set; }
        public int    MachineId   { get; set; }
        public string CompanyName { get; set; }
        public string Name        { get; set; }
    }
}