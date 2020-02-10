namespace Marechai.Database.Models
{
    public class BooksByMachineFamily : BaseModel<long>
    {
        public long BookId          { get; set; }
        public int  MachineFamilyId { get; set; }

        public virtual Book          Book          { get; set; }
        public virtual MachineFamily MachineFamily { get; set; }
    }
}