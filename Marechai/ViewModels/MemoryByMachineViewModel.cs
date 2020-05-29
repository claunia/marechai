using Marechai.Database;

namespace Marechai.ViewModels
{
    public class MemoryByMachineViewModel : BaseViewModel<long>
    {
        public int         MachineId { get; set; }
        public MemoryType  Type      { get; set; }
        public MemoryUsage Usage     { get; set; }
        public long?       Size      { get; set; }
        public double?     Speed     { get; set; }
    }
}