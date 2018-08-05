namespace Cicm.Database.Models
{
    public class StorageByMachine
    {
        public int   Machine   { get; set; }
        public int   Type      { get; set; }
        public int   Interface { get; set; }
        public long? Capacity  { get; set; }
        public long  Id        { get; set; }

        public Machines MachineNavigation { get; set; }
    }
}