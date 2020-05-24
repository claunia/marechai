using Marechai.Database;

namespace Marechai.ViewModels
{
    public class MemoryViewModel
    {
        public MemoryType  Type  { get; set; }
        public MemoryUsage Usage { get; set; }
        public long?       Size  { get; set; }
        public double?     Speed { get; set; }
    }
}