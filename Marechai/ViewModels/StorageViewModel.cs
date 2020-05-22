using Marechai.Database;

namespace Marechai.ViewModels
{
    public class StorageViewModel
    {
        public StorageType      Type      { get; set; }
        public StorageInterface Interface { get; set; }
        public long?            Capacity  { get; set; }
    }
}