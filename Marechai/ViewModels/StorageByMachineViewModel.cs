using Marechai.Database;

namespace Marechai.ViewModels
{
    public class StorageByMachineViewModel : BaseViewModel<long>
    {
        public int              MachineId { get; set; }
        public StorageType      Type      { get; set; }
        public StorageInterface Interface { get; set; }
        public long?            Capacity  { get; set; }
    }
}