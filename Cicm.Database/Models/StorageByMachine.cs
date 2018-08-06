namespace Cicm.Database.Models
{
    public class StorageByMachine
    {
        public int              MachineId { get; set; }
        public StorageType      Type      { get; set; }
        public StorageInterface Interface { get; set; }
        public long?            Capacity  { get; set; }
        public long             Id        { get; set; }

        public virtual Machine Machine { get; set; }
    }
}